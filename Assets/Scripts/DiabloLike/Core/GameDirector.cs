using System.Collections;
using System.Collections.Generic;
using DiabloLike.AI;
using DiabloLike.Audio;
using DiabloLike.Combat;
using DiabloLike.UI;
using DiabloLike.World;
using UnityEngine;

namespace DiabloLike.Core
{
    public sealed class GameDirector : MonoBehaviour
    {
        private readonly List<Health> enemies = new();
        private readonly List<SummoningPillar> pillars = new();
        private readonly List<string> recentEvents = new();
        private readonly List<ChestRewardOption> currentRewardOptions = new();
        private Transform player;
        private Health playerHealth;
        private Mana playerMana;
        private PlayerController playerController;
        private bool playerDead;
        private bool restarting;
        private bool rewardChestSpawned;
        private bool rewardChoiceOpen;
        private bool portalSpawned;
        private int dungeonLevel = 1;
        private int lifeBonus;
        private int manaBonus;
        private float movementSpeedBonus;
        private int wandDamageBonus;
        private float wandRangeBonus;
        private int swordDamageBonus;
        private float swordRangeBonus;
        private bool splitShotUnlocked;
        private int armorBonus;
        private bool rareArmorUnlocked;
        private float projectileSizeBonus;
        private float cooldownMultiplier = 1f;
        private int carriedHealth = -1;
        private int cachedPlayerMaxHealth;
        private int cachedPlayerMaxMana;
        private int wave;
        private float nextEncounterCheck;

        public AIFeatureRouter AI { get; } = new();
        public string QuestText { get; private set; } = "Znic summoning pillary.";
        public string ChatText { get; private set; } = "Lovec: Runovy kruh se probouzi.";
        public bool IsPlayerDead => playerDead;
        public bool IsInventoryOpen { get; private set; }
        public int PlayerMana => playerMana != null ? playerMana.Current : 0;
        public int PlayerMaxMana => playerMana != null ? playerMana.Max : cachedPlayerMaxMana;
        public int PlayerArmor => playerHealth != null ? playerHealth.Armor : armorBonus;
        public WeaponDefinition EquippedWeapon { get; private set; } = WeaponCatalog.Sword;
        public bool HasWandUpgrade { get; private set; }
        public bool IsRewardChoiceOpen => rewardChoiceOpen;
        public IReadOnlyList<ChestRewardOption> CurrentRewardOptions => currentRewardOptions;

        private void Start()
        {
            BeginRun();
        }

        private void BeginRun()
        {
            enemies.Clear();
            pillars.Clear();
            recentEvents.Clear();
            playerDead = false;
            IsInventoryOpen = false;
            rewardChestSpawned = false;
            rewardChoiceOpen = false;
            portalSpawned = false;
            wave = 0;
            nextEncounterCheck = 0f;
            QuestText = $"Level {dungeonLevel}: znic summoning pillary.";
            ChatText = $"Lovec: Level {dungeonLevel}. Kruh se probouzi.";

            ArenaFactory.BuildArena();
            CreateSummoningPillars();
            player = ActorFactory.CreatePlayer().transform;
            playerHealth = player.GetComponent<Health>();
            playerMana = player.GetComponent<Mana>();
            playerHealth.IncreaseMax(lifeBonus);
            playerHealth.IncreaseArmor(armorBonus);
            if (carriedHealth > 0)
            {
                playerHealth.SetCurrent(Mathf.Min(carriedHealth, playerHealth.Max));
                carriedHealth = -1;
            }

            playerMana.IncreaseMax(manaBonus);
            playerController = player.GetComponent<PlayerController>();
            playerController.AddMovementSpeed(movementSpeedBonus);
            playerController.AddWandDamage(wandDamageBonus);
            playerController.AddWandRange(wandRangeBonus);
            playerController.AddSwordDamage(swordDamageBonus);
            playerController.AddSwordRange(swordRangeBonus);
            playerController.AddProjectileSize(projectileSizeBonus);
            playerController.MultiplyCooldown(cooldownMultiplier);
            if (splitShotUnlocked)
            {
                playerController.EnableSplitWandProjectiles();
            }
            cachedPlayerMaxHealth = playerHealth.Max;
            cachedPlayerMaxMana = playerMana.Max;
            playerHealth.Died += OnPlayerDied;
            EquipWeapon(EquippedWeapon.Id);
            ActorFactory.CreateCamera(player);
            DiabloAudio.Ensure();
            DiabloAudio.Play(GameSfx.LevelStart, 0.03f);
            if (!TryGetComponent(out DiabloHud hud))
            {
                hud = gameObject.AddComponent<DiabloHud>();
            }

            hud.Configure(this);
            SpawnWave();
        }

        private void Update()
        {
            if (IsPlayerDead || restarting)
            {
                return;
            }

            enemies.RemoveAll(enemy => enemy == null || enemy.IsDead);
            pillars.RemoveAll(pillar => pillar == null || !pillar.IsAlive);

            if (Time.time >= nextEncounterCheck)
            {
                nextEncounterCheck = Time.time + 1f;
                TickQuestAndEncounter();
            }
        }

        public WorldSnapshot CreateSnapshot()
        {
            var snapshot = new WorldSnapshot
            {
                PlayerName = "Lovec",
                PlayerLevel = dungeonLevel,
                PlayerHealth = playerHealth != null ? playerHealth.Current : 0,
                PlayerMaxHealth = playerHealth != null ? playerHealth.Max : cachedPlayerMaxHealth,
                EnemiesAlive = enemies.Count,
                CurrentQuestId = "cleanse_rune_circle",
                PlayerPosition = player != null ? player.position : Vector3.zero
            };
            snapshot.RecentEvents.AddRange(recentEvents);
            return snapshot;
        }

        public void AskChat(string playerText)
        {
            if (IsPlayerDead)
            {
                return;
            }

            ChatText = AI.Ask(new AIRequest(AIFeature.Chat, "camp_sage", playerText, CreateSnapshot())).Text;
        }

        public void ToggleInventory()
        {
            IsInventoryOpen = !IsInventoryOpen;
        }

        public void RestartRun()
        {
            if (restarting)
            {
                return;
            }

            StartCoroutine(RestartRunRoutine());
        }

        public void EquipWeapon(WeaponId weaponId)
        {
            if (weaponId == WeaponId.MagicWand && HasWandUpgrade)
            {
                weaponId = WeaponId.EmberWand;
            }

            EquippedWeapon = WeaponCatalog.Get(weaponId);
            playerController?.EquipWeapon(EquippedWeapon);
        }

        private void TickQuestAndEncounter()
        {
            if (IsPlayerDead)
            {
                return;
            }

            var quest = AI.Ask(new AIRequest(AIFeature.Quest, "quest_director", "", CreateSnapshot()));
            QuestText = AlivePillarCount() > 0
                ? $"Znic summoning pillary: {AlivePillarCount()} zbyva. Nepratele proudi z jejich run."
                : "Pillary jsou rozbite. Docisti zbytek posedlych.";

            var encounter = AI.Ask(new AIRequest(AIFeature.Encounter, "encounter_director", "", CreateSnapshot()));
            if (encounter.Intent == "spawn_wave" && AlivePillarCount() > 0)
            {
                SpawnWave();
            }

            if (AlivePillarCount() <= 0 && enemies.Count <= 0)
            {
                QuestText = rewardChestSpawned ? "Otevri chestku a vyber upgrade." : "Kruh je ocisteny. Objevila se chestka.";
                ChatText = "Lovec: Zdroje summonu jsou mrtve.";
                SpawnRewardChest();
            }
        }

        private void SpawnWave()
        {
            if (IsPlayerDead)
            {
                return;
            }

            var alivePillars = GetAlivePillars();
            if (alivePillars.Count <= 0)
            {
                return;
            }

            wave++;
            var levelBonusWave = Mathf.FloorToInt((dungeonLevel - 1) * 0.5f);
            var count = Mathf.Clamp(alivePillars.Count + wave + levelBonusWave, 2, 8);
            for (var i = 0; i < count; i++)
            {
                var pillar = alivePillars[i % alivePillars.Count];
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * Random.Range(1.5f, 2.8f);
                var position = pillar.transform.position + offset;
                var spawnRanged = wave > 1 && Random.value < Mathf.Min(0.32f, 0.12f + dungeonLevel * 0.03f);
                var enemy = ActorFactory.CreateEnemy(position, player, this, spawnRanged);
                var health = enemy.GetComponent<Health>();
                health.Died += OnEnemyDied;
                enemies.Add(health);
            }

            AddEvent($"Vlna {wave} byla vyvolana pillary.");
        }

        private void OnEnemyDied(Health enemy)
        {
            EffectFactory.SpawnHit(enemy.transform.position);
            EffectFactory.SpawnLootEssence(enemy.transform.position);
            AddEvent("Nepritel padl a zanechal krvavou esenci.");
        }

        private void OnPlayerDied(Health health)
        {
            playerDead = true;
            ChatText = "Ticho. Lovec padl v runovem kruhu.";
            QuestText = "You died.";
            AddEvent("Hrac zemrel.");
        }

        public void OnPillarDestroyed(SummoningPillar pillar)
        {
            pillars.Remove(pillar);
            EffectFactory.SpawnHit(pillar.transform.position + Vector3.up);
            AddEvent("Summoning pillar se rozpadl.");
            if (AlivePillarCount() > 0)
            {
                QuestText = $"Pillar znicen. {AlivePillarCount()} jeste drzi portal.";
                return;
            }

            QuestText = "Vsechny pillary jsou znicene. Chestka se objevila.";
            ChatText = "Lovec: Vyber upgrade, pak projdi portalem.";
            SpawnRewardChest();
        }

        public void OnUpgradeChestOpened(UpgradeChest chest)
        {
            rewardChoiceOpen = true;
            RollChestRewards();
            ChatText = "Chestka nabizi tri cesty moci.";
            QuestText = "Vyber jeden upgrade z chestky.";
        }

        public void ChooseChestReward(int optionIndex)
        {
            if (!rewardChoiceOpen || optionIndex < 0 || optionIndex >= currentRewardOptions.Count)
            {
                return;
            }

            rewardChoiceOpen = false;
            var reward = currentRewardOptions[optionIndex].Id;
            currentRewardOptions.Clear();
            DiabloAudio.Play(GameSfx.ChestLoot, 0.05f);
            switch (reward)
            {
                case ChestRewardId.WandUpgrade:
                    HasWandUpgrade = true;
                    EquippedWeapon = WeaponCatalog.EmberWand;
                    playerController?.EquipWeapon(EquippedWeapon);
                    ChatText = "Chestka probudila Ember Wand.";
                    break;
                case ChestRewardId.LifeUpgrade:
                    lifeBonus += 15;
                    playerHealth?.IncreaseMax(15);
                    cachedPlayerMaxHealth = playerHealth != null ? playerHealth.Max : cachedPlayerMaxHealth + 15;
                    ChatText = "Krevni runa trochu zpevnila maso.";
                    break;
                case ChestRewardId.ManaUpgrade:
                    manaBonus += 12;
                    playerMana?.IncreaseMax(12);
                    cachedPlayerMaxMana = playerMana != null ? playerMana.Max : cachedPlayerMaxMana + 12;
                    ChatText = "Modra runa pridala trochu many.";
                    break;
                case ChestRewardId.MovementSpeed:
                    movementSpeedBonus += 0.5f;
                    playerController?.AddMovementSpeed(0.5f);
                    ChatText = "Boty prestaly nenavidet pohyb.";
                    break;
                case ChestRewardId.WandDamage:
                    wandDamageBonus += 6;
                    playerController?.AddWandDamage(6);
                    ChatText = "Wand projektil ma ostrejsi zuby.";
                    break;
                case ChestRewardId.WandRange:
                    wandRangeBonus += 0.45f;
                    playerController?.AddWandRange(0.45f);
                    ChatText = "Wand dosah se o chlup natahl.";
                    break;
                case ChestRewardId.SwordDamage:
                    swordDamageBonus += 5;
                    playerController?.AddSwordDamage(5);
                    ChatText = "Mec zacal rezat o trochu min trapne.";
                    break;
                case ChestRewardId.SwordRange:
                    swordRangeBonus += 0.25f;
                    playerController?.AddSwordRange(0.25f);
                    ChatText = "Mec ma trochu delsi argument.";
                    break;
                case ChestRewardId.SplitShot:
                    splitShotUnlocked = true;
                    playerController?.EnableSplitWandProjectiles();
                    ChatText = "Rare upgrade: wand projektily se po zasahu deli.";
                    break;
                case ChestRewardId.Armor:
                    armorBonus += 1;
                    playerHealth?.IncreaseArmor(1);
                    ChatText = "Nasadil sis kus plechu. Snad ne z alobalu.";
                    break;
                case ChestRewardId.RareArmor:
                    rareArmorUnlocked = true;
                    armorBonus += 3;
                    playerHealth?.IncreaseArmor(3);
                    ChatText = "Rare armor: konecne nevypadas uplne krehce.";
                    break;
                case ChestRewardId.RareSwordBleed:
                    swordDamageBonus += 10;
                    swordRangeBonus += 0.15f;
                    playerController?.AddSwordDamage(10);
                    playerController?.AddSwordRange(0.15f);
                    ChatText = "Rare sword upgrade: mec je hnusnejsi a delsi.";
                    break;
                case ChestRewardId.RareWandBigBullet:
                    projectileSizeBonus += 0.2f;
                    wandDamageBonus += 4;
                    playerController?.AddProjectileSize(0.2f);
                    playerController?.AddWandDamage(4);
                    ChatText = "Rare wand upgrade: vetsi koule, vetsi problem.";
                    break;
                case ChestRewardId.RareSwiftCast:
                    cooldownMultiplier *= 0.9f;
                    playerController?.MultiplyCooldown(0.9f);
                    ChatText = "Rare upgrade: ruce prestaly byt tak pomale.";
                    break;
            }

            QuestText = "Upgrade vybran. Portal do dalsiho levelu je otevreny.";
            SpawnPortal();
        }

        public void EnterNextLevel()
        {
            if (!portalSpawned || restarting)
            {
                return;
            }

            dungeonLevel++;
            carriedHealth = playerHealth != null ? Mathf.Max(1, playerHealth.Current) : -1;
            DiabloAudio.Play(GameSfx.Teleport, 0.04f);
            StartCoroutine(RestartRunRoutine());
        }

        private void AddEvent(string text)
        {
            recentEvents.Add(text);
            if (recentEvents.Count > 8)
            {
                recentEvents.RemoveAt(0);
            }
        }

        private void CleanupRunObjects()
        {
            foreach (var root in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (root == gameObject || root.transform.parent != null)
                {
                    continue;
                }

                if (IsRuntimeObject(root.name))
                {
                    Destroy(root);
                }
            }
        }

        private IEnumerator RestartRunRoutine()
        {
            restarting = true;
            Time.timeScale = 1f;
            CleanupRunObjects();
            yield return null;
            BeginRun();
            restarting = false;
        }

        private void CreateSummoningPillars()
        {
            var heightBonus = (dungeonLevel - 1) * 0.1f;
            RegisterPillar(ActorFactory.CreateSummoningPillar(new Vector3(-8f, 0f, -7f), 2.8f + heightBonus, this));
            RegisterPillar(ActorFactory.CreateSummoningPillar(new Vector3(8f, 0f, -6f), 2.2f + heightBonus, this));
            RegisterPillar(ActorFactory.CreateSummoningPillar(new Vector3(-7f, 0f, 8f), 1.9f + heightBonus, this));
            RegisterPillar(ActorFactory.CreateSummoningPillar(new Vector3(7f, 0f, 7f), 3.1f + heightBonus, this));
        }

        private void SpawnRewardChest()
        {
            if (rewardChestSpawned)
            {
                return;
            }

            rewardChestSpawned = true;
            ActorFactory.CreateUpgradeChest(new Vector3(0f, 0f, 2.2f), this);
        }

        private void SpawnPortal()
        {
            if (portalSpawned)
            {
                return;
            }

            portalSpawned = true;
            DiabloAudio.Play(GameSfx.PortalOpen, 0.04f);
            ActorFactory.CreateLevelPortal(new Vector3(0f, 0f, -3.2f), this);
        }

        private void RollChestRewards()
        {
            currentRewardOptions.Clear();
            var pool = new List<ChestRewardOption>
            {
                new(ChestRewardId.LifeUpgrade, "Blood Nick", "+15 max life\nsmall survival bump"),
                new(ChestRewardId.ManaUpgrade, "Blue Crackle", "+12 max mana\none more mistake"),
                new(ChestRewardId.MovementSpeed, "Quick Boots", "+0.5 movement speed\nless dramatic walking"),
                new(ChestRewardId.WandDamage, "Sharper Spark", "+6 wand projectile damage"),
                new(ChestRewardId.WandRange, "Longer Spark", "+0.45 wand projectile range"),
                new(ChestRewardId.SwordDamage, "Mean Edge", "+5 sword damage"),
                new(ChestRewardId.SwordRange, "Long Grip", "+0.25 sword reach"),
                new(ChestRewardId.Armor, "Dented Plate", "+1 armor\nless pain per hit")
            };

            if (!HasWandUpgrade)
            {
                pool.Add(new ChestRewardOption(ChestRewardId.WandUpgrade, "Ember Wand", "upgrade wand\nprojectile hits harder"));
            }

            if (HasWandUpgrade && !splitShotUnlocked && Random.value < 0.28f)
            {
                pool.Add(new ChestRewardOption(ChestRewardId.SplitShot, "Split Hex", "RARE\nwand bullets split on hit", true));
            }

            if (!rareArmorUnlocked && Random.value < 0.22f)
            {
                pool.Add(new ChestRewardOption(ChestRewardId.RareArmor, "Graveplate", "RARE\n+3 armor", true));
            }

            if (Random.value < 0.2f)
            {
                pool.Add(new ChestRewardOption(ChestRewardId.RareSwordBleed, "Butcher Edge", "RARE\n+10 sword damage\n+0.15 reach", true));
            }

            if (HasWandUpgrade && Random.value < 0.24f)
            {
                pool.Add(new ChestRewardOption(ChestRewardId.RareWandBigBullet, "Fat Spark", "RARE\nbigger wand bullets\n+4 damage", true));
            }

            if (Random.value < 0.18f)
            {
                pool.Add(new ChestRewardOption(ChestRewardId.RareSwiftCast, "Nervous Hands", "RARE\n10% faster attacks", true));
            }

            while (currentRewardOptions.Count < 3 && pool.Count > 0)
            {
                var index = Random.Range(0, pool.Count);
                currentRewardOptions.Add(pool[index]);
                pool.RemoveAt(index);
            }
        }

        private void RegisterPillar(GameObject pillar)
        {
            pillars.Add(pillar.GetComponent<SummoningPillar>());
        }

        private int AlivePillarCount()
        {
            var alive = 0;
            foreach (var pillar in pillars)
            {
                if (pillar != null && pillar.IsAlive)
                {
                    alive++;
                }
            }

            return alive;
        }

        private List<SummoningPillar> GetAlivePillars()
        {
            var alivePillars = new List<SummoningPillar>();
            foreach (var pillar in pillars)
            {
                if (pillar != null && pillar.IsAlive)
                {
                    alivePillars.Add(pillar);
                }
            }

            return alivePillars;
        }

        private static bool IsRuntimeObject(string objectName)
        {
            return objectName.StartsWith("Player - Rune Hunter")
                || objectName.StartsWith("Possessed")
                || objectName.StartsWith("Enemy Projectile")
                || objectName.StartsWith("Ashen Arena Floor")
                || objectName.StartsWith("Blood Moon Key Light")
                || objectName.StartsWith("Rune Stone")
                || objectName.StartsWith("Broken Obelisk")
                || objectName.StartsWith("Summoning Pillar")
                || objectName.StartsWith("Blood Essence Loot")
                || objectName.StartsWith("Combat Spark")
                || objectName.StartsWith("Curved Sword Slash VFX")
                || objectName.StartsWith("Arcane Slash VFX")
                || objectName.StartsWith("Wand Projectile")
                || objectName.StartsWith("Upgrade Chest")
                || objectName.StartsWith("Level Portal");
        }
    }
}
