using System.Collections;
using System.Collections.Generic;
using DiabloLike.AI;
using DiabloLike.Audio;
using DiabloLike.Combat;
using DiabloLike.UI;
using DiabloLike.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiabloLike.Core
{
    public sealed class GameDirector : MonoBehaviour
    {
        private const int MaxUpgradeSlots = 10;
        private readonly List<Health> enemies = new();
        private readonly List<SummoningVase> vases = new();
        private readonly List<string> recentEvents = new();
        private readonly List<ChestRewardOption> currentRewardOptions = new();
        private readonly List<ChestRewardOption> acquiredUpgrades = new();
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
        private bool explosionUnlocked;
        private bool hardenedBulletUnlocked;
        private int armorBonus;
        private bool rareArmorUnlocked;
        private float projectileSizeBonus;
        private float cooldownMultiplier = 1f;
        private int carriedHealth = -1;
        private int cachedPlayerMaxHealth;
        private int cachedPlayerMaxMana;
        private int wave;
        private float nextEncounterCheck;
        private Scene arenaScene;

        private ArenaLayout activeArena;

        public AIFeatureRouter AI { get; } = new();
        public string QuestText { get; private set; } = "Znic summoning vases.";
        public string ChatText { get; private set; } = "Lovec: Runovy kruh se probouzi.";
        public bool IsPlayerDead => playerDead;
        public bool IsInventoryOpen { get; private set; }
        public int PlayerMana => playerMana != null ? playerMana.Current : 0;
        public int PlayerMaxMana => playerMana != null ? playerMana.Max : cachedPlayerMaxMana;
        public int PlayerArmor => playerHealth != null ? playerHealth.Armor : armorBonus;
        public WeaponDefinition EquippedWeapon { get; private set; } = WeaponCatalog.Sword;
        public bool IsRewardChoiceOpen => rewardChoiceOpen;
        public IReadOnlyList<ChestRewardOption> CurrentRewardOptions => currentRewardOptions;
        public IReadOnlyList<ChestRewardOption> AcquiredUpgrades => acquiredUpgrades;

        private void Start()
        {
            StartCoroutine(BeginRun());
        }

        private IEnumerator BeginRun()
        {
            enemies.Clear();
            vases.Clear();
            recentEvents.Clear();
            playerDead = false;
            IsInventoryOpen = false;
            rewardChestSpawned = false;
            rewardChoiceOpen = false;
            portalSpawned = false;
            wave = 0;
            nextEncounterCheck = 0f;
            QuestText = $"Level {dungeonLevel}: znic summoning vases.";
            ChatText = $"Lovec: Level {dungeonLevel}. Kruh se probouzi.";

            yield return LoadArenaForCurrentLevel();
            if (activeArena == null)
            {
                ArenaFactory.BuildArena();
            }
            if (!IsBossLevel() && !IsMiniBossLevel())
            {
                CreateSummoningVases();
            }
            player = ActorFactory.CreatePlayer().transform;
            player.position = activeArena != null ? activeArena.PlayerSpawn.position : Vector3.zero;
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
            if (explosionUnlocked)
            {
                playerController.EnableExplodingWandProjectiles();
            }
            if (hardenedBulletUnlocked)
            {
                playerController.EnableHardenedBullet();
            }
            RefreshUpgradePriority();
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
            if (IsBossLevel() || IsMiniBossLevel())
            {
                SpawnBoss(IsMiniBossLevel());
            }
            else
            {
                SpawnWave();
            }
        }

        private void Update()
        {
            if (IsPlayerDead || restarting)
            {
                return;
            }

            enemies.RemoveAll(enemy => enemy == null || enemy.IsDead);
            vases.RemoveAll(vase => vase == null || !vase.IsAlive);

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

            StartCoroutine(RestartRunRoutine(true));
        }

        public void EquipWeapon(WeaponId weaponId)
        {
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
            QuestText = AliveVaseCount() > 0
                ? $"Znic summoning vases: {AliveVaseCount()} zbyva. Nepratele proudi z jejich run."
                : "Vazy jsou rozbite. Docisti zbytek posedlych.";

            var encounter = AI.Ask(new AIRequest(AIFeature.Encounter, "encounter_director", "", CreateSnapshot()));
            if (encounter.Intent == "spawn_wave" && AliveVaseCount() > 0)
            {
                SpawnWave();
            }

            if (AliveVaseCount() <= 0 && enemies.Count <= 0)
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

            var aliveVases = GetAliveVases();
            if (aliveVases.Count <= 0)
            {
                return;
            }

            wave++;
            var levelBonusWave = Mathf.FloorToInt((dungeonLevel - 1) * 0.5f);
            var count = Mathf.Clamp(aliveVases.Count + wave + levelBonusWave, 2, 8);
            for (var i = 0; i < count; i++)
            {
                var vase = aliveVases[i % aliveVases.Count];
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * Random.Range(1.5f, 2.8f);
                var position = vase.transform.position + offset;
                var spawnRanged = wave > 1 && Random.value < Mathf.Min(0.32f, 0.12f + dungeonLevel * 0.03f);
                var enemy = ActorFactory.CreateEnemy(position, player, this, spawnRanged);
                var health = enemy.GetComponent<Health>();
                health.Died += OnEnemyDied;
                enemies.Add(health);
            }

            AddEvent($"Vlna {wave} byla vyvolana vázami.");
        }

        private bool IsBossLevel()
        {
            return dungeonLevel > 0 && dungeonLevel % 9 == 0;
        }

        private bool IsMiniBossLevel()
        {
            return !IsBossLevel() && dungeonLevel > 8 && dungeonLevel % 2 == 0;
        }

        private void SpawnBoss(bool miniBoss)
        {
            var boss = ActorFactory.CreateBoss(new Vector3(0f, 0f, 6f), player, dungeonLevel, miniBoss);
            var health = boss.GetComponent<Health>();
            health.Died += OnEnemyDied;
            enemies.Add(health);
            var minionCount = miniBoss ? 1 : Mathf.Clamp(2 + dungeonLevel / 9, 2, 5);
            for (var i = 0; i < minionCount; i++)
            {
                var angle = i * Mathf.PI * 2f / minionCount;
                var minionPosition = boss.transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 3.2f;
                var minion = ActorFactory.CreateEnemy(minionPosition, player, this, i % 3 == 0);
                var minionHealth = minion.GetComponent<Health>();
                minionHealth.IncreaseMax(dungeonLevel * 3);
                minion.GetComponent<EnemyController>()?.SetSpeedBonus(dungeonLevel * 0.08f);
                minionHealth.Died += OnEnemyDied;
                enemies.Add(minionHealth);
            }
            QuestText = miniBoss ? "MINI BOSS: zlom Rotmawovu ozvenu." : "BOSS FIGHT: zlom Rotmaw a prezij jeho laser a dash.";
            ChatText = miniBoss ? "Rotmawova ozvena: nejsem tak velky, ale porad boli." : "Rotmaw: Utec, nebo shoříš.";
            AddEvent(miniBoss ? "MiniBoss Rotmaw vstoupil do runoveho kruhu." : "Boss Rotmaw vstoupil do runoveho kruhu.");
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

        public void OnVaseDestroyed(SummoningVase vase)
        {
            vases.Remove(vase);
            EffectFactory.SpawnHit(vase.transform.position + Vector3.up);
            AddEvent("Summoning vase se rozpadla.");
            if (AliveVaseCount() > 0)
            {
                QuestText = $"Vase znicena. {AliveVaseCount()} jeste drzi portal.";
                return;
            }

            QuestText = "Vsechny vazy jsou znicene. Chestka se objevila.";
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
            var selectedUpgrade = currentRewardOptions[optionIndex];
            currentRewardOptions.Clear();
            var existingIndex = acquiredUpgrades.FindIndex(upgrade => upgrade.Id == selectedUpgrade.Id);
            if (existingIndex >= 0)
            {
                var existing = acquiredUpgrades[existingIndex];
                acquiredUpgrades[existingIndex] = new ChestRewardOption(existing.Id, existing.Title, existing.Body, existing.IsRare, existing.StackCount + 1);
                ChatText = $"Upgrade {existing.Title} se sloucil a zesilil o 20 %.";
            }
            else if (acquiredUpgrades.Count >= MaxUpgradeSlots)
            {
                acquiredUpgrades[MaxUpgradeSlots - 1] = selectedUpgrade;
                ChatText = "Inventar je plny. Upgrade na 10. priorite byl nahrazen.";
            }
            else
            {
                acquiredUpgrades.Add(selectedUpgrade);
            }
            RefreshUpgradePriority();
            DiabloAudio.Play(GameSfx.ChestLoot, 0.05f);
            switch (reward)
            {
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
                case ChestRewardId.RareExplosion:
                    explosionUnlocked = true;
                    playerController?.EnableExplodingWandProjectiles();
                    ChatText = "Rare upgrade: wand projektily po zasahu vybuchuji.";
                    break;
                case ChestRewardId.SuperRareHardenedBullet:
                    hardenedBulletUnlocked = true;
                    playerController?.EnableHardenedBullet();
                    ChatText = "SUPER RARE: zlate hardened bullets davaji o 20 % vetsi damage.";
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

        public void MoveUpgrade(int index, int direction)
        {
            var target = index + direction;
            if (index < 0 || index >= acquiredUpgrades.Count || target < 0 || target >= acquiredUpgrades.Count)
            {
                return;
            }

            (acquiredUpgrades[index], acquiredUpgrades[target]) = (acquiredUpgrades[target], acquiredUpgrades[index]);
            RefreshUpgradePriority();
        }

        public void RemoveUpgrade(int index)
        {
            if (index >= 0 && index < acquiredUpgrades.Count)
            {
                acquiredUpgrades.RemoveAt(index);
                RefreshUpgradePriority();
            }
        }

        private void RefreshUpgradePriority()
        {
            if (playerController == null)
            {
                return;
            }

            var explosionIndex = -1;
            var splitIndex = -1;
            for (var i = 0; i < acquiredUpgrades.Count; i++)
            {
                if (acquiredUpgrades[i].Id == ChestRewardId.RareExplosion)
                {
                    explosionIndex = i;
                }
                else if (acquiredUpgrades[i].Id == ChestRewardId.SplitShot)
                {
                    splitIndex = i;
                }
            }

            playerController.SetSplitAfterExplosion(explosionIndex >= 0 && splitIndex >= 0 && explosionIndex < splitIndex);
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
            StartCoroutine(RestartRunRoutine(false));
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

        private IEnumerator RestartRunRoutine(bool resetProgress)
        {
            restarting = true;
            Time.timeScale = 1f;
            if (resetProgress)
            {
                ResetRunProgress();
            }
            CleanupRunObjects();
            yield return null;
            yield return BeginRun();
            restarting = false;
        }

        private void ResetRunProgress()
        {
            dungeonLevel = 1;
            lifeBonus = 0;
            manaBonus = 0;
            movementSpeedBonus = 0f;
            wandDamageBonus = 0;
            wandRangeBonus = 0f;
            swordDamageBonus = 0;
            swordRangeBonus = 0f;
            splitShotUnlocked = false;
            explosionUnlocked = false;
            hardenedBulletUnlocked = false;
            armorBonus = 0;
            rareArmorUnlocked = false;
            projectileSizeBonus = 0f;
            cooldownMultiplier = 1f;
            carriedHealth = -1;
            acquiredUpgrades.Clear();
            cachedPlayerMaxHealth = 0;
            cachedPlayerMaxMana = 0;
            EquippedWeapon = WeaponCatalog.Sword;
        }

        private void CreateSummoningVases()
        {
            var heightBonus = (dungeonLevel - 1) * 0.1f;
            if (activeArena != null && activeArena.VaseSpawns.Length > 0)
            {
                foreach (var spawn in activeArena.VaseSpawns)
                {
                    if (spawn != null)
                    {
                        RegisterVase(ActorFactory.CreateSummoningVase(
                            spawn.position, 2.4f + heightBonus, this));
                    }
                }

                return;
            }

            RegisterVase(ActorFactory.CreateSummoningVase(new Vector3(-8f, 0f, -7f), 2.8f + heightBonus, this));
            RegisterVase(ActorFactory.CreateSummoningVase(new Vector3(8f, 0f, -6f), 2.2f + heightBonus, this));
            RegisterVase(ActorFactory.CreateSummoningVase(new Vector3(-7f, 0f, 8f), 1.9f + heightBonus, this));
            RegisterVase(ActorFactory.CreateSummoningVase(new Vector3(7f, 0f, 7f), 3.1f + heightBonus, this));
        }

        private string GetArenaSceneName()
        {
            if (IsBossLevel())
            {
                return "BossArena";
            }

            return dungeonLevel >= 4 ? "Arena02" : "Arena01";
        }

        private IEnumerator LoadArenaForCurrentLevel()
        {
            var sceneName = GetArenaSceneName();
            if (!arenaScene.IsValid() || !arenaScene.isLoaded)
            {
                var currentScene = SceneManager.GetActiveScene();
                var currentLayout = FindArenaLayout(currentScene);
                if (currentScene.IsValid() && currentScene.isLoaded && currentLayout != null)
                {
                    arenaScene = currentScene;
                    activeArena = currentLayout;
                    yield break;
                }
            }

            if (arenaScene.IsValid() && arenaScene.isLoaded && arenaScene.name == sceneName)
            {
                activeArena = FindArenaLayout(arenaScene);
                yield break;
            }

            var previousArenaScene = arenaScene;
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null)
            {
                yield break;
            }

            yield return operation;
            arenaScene = SceneManager.GetSceneByName(sceneName);
            if (arenaScene.IsValid() && arenaScene.isLoaded)
            {
                activeArena = FindArenaLayout(arenaScene);
                SceneManager.SetActiveScene(arenaScene);
            }

            if (previousArenaScene.IsValid()
                && previousArenaScene.isLoaded
                && previousArenaScene != arenaScene)
            {
                yield return SceneManager.UnloadSceneAsync(previousArenaScene);
            }
        }

        private static ArenaLayout FindArenaLayout(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return null;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                var layout = root.GetComponentInChildren<ArenaLayout>(true);
                if (layout != null)
                {
                    return layout;
                }
            }

            return null;
        }

        private void SpawnRewardChest()
        {
            if (rewardChestSpawned)
            {
                return;
            }

            rewardChestSpawned = true;
            var position = activeArena != null
                ? activeArena.ChestSpawn.position
                : new Vector3(0f, 0f, 2.2f);
            ActorFactory.CreateUpgradeChest(position, this);
        }

        private void SpawnPortal()
        {
            if (portalSpawned)
            {
                return;
            }

            portalSpawned = true;
            DiabloAudio.Play(GameSfx.PortalOpen, 0.04f);
            var position = activeArena != null
                ? activeArena.PortalSpawn.position
                : new Vector3(0f, 0f, -3.2f);
            ActorFactory.CreateLevelPortal(position, this);
        }

        private void RollChestRewards()
        {
            currentRewardOptions.Clear();
            if (IsBossLevel())
            {
                currentRewardOptions.Add(new ChestRewardOption(ChestRewardId.SplitShot, "Split Hex", "RARE\nwand bullets split immediately", true));
                currentRewardOptions.Add(new ChestRewardOption(ChestRewardId.RareExplosion, "Blast Core", "RARE\nwand projectiles explode on hit", true));
                currentRewardOptions.Add(new ChestRewardOption(ChestRewardId.SuperRareHardenedBullet, "Hardened Bullet", "SUPER RARE\ngold bullets\n+20% wand damage", true));
                return;
            }
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

            var rareRound = dungeonLevel % 2 == 0;

            if (!splitShotUnlocked && (rareRound || Random.value < 0.28f))
            {
                pool.Add(new ChestRewardOption(ChestRewardId.SplitShot, "Split Hex", "RARE\nwand bullets split on hit", true));
            }

            if (!rareArmorUnlocked && (rareRound || Random.value < 0.22f))
            {
                pool.Add(new ChestRewardOption(ChestRewardId.RareArmor, "Graveplate", "RARE\n+3 armor", true));
            }

            if (rareRound || Random.value < 0.2f)
            {
                pool.Add(new ChestRewardOption(ChestRewardId.RareSwordBleed, "Butcher Edge", "RARE\n+10 sword damage\n+0.15 reach", true));
            }

            if (rareRound || Random.value < 0.24f)
            {
                pool.Add(new ChestRewardOption(ChestRewardId.RareWandBigBullet, "Fat Spark", "RARE\nbigger wand bullets\n+4 damage", true));
            }

            if (rareRound || Random.value < 0.18f)
            {
                pool.Add(new ChestRewardOption(ChestRewardId.RareSwiftCast, "Nervous Hands", "RARE\n10% faster attacks", true));
            }

            if (rareRound)
            {
                pool.Add(new ChestRewardOption(ChestRewardId.RareExplosion, "Blast Core", "RARE\nwand projectiles explode on hit", true));
            }

            if (Random.value < 0.06f)
            {
                pool.Add(new ChestRewardOption(ChestRewardId.SuperRareHardenedBullet, "Hardened Bullet", "SUPER RARE\ngold bullets\n+20% wand damage", true));
            }

            while (currentRewardOptions.Count < 3 && pool.Count > 0)
            {
                var index = Random.Range(0, pool.Count);
                currentRewardOptions.Add(pool[index]);
                pool.RemoveAt(index);
            }

            if (rareRound && !currentRewardOptions.Exists(option => option.IsRare))
            {
                var rareOptions = pool.FindAll(option => option.IsRare);
                if (rareOptions.Count > 0)
                {
                    currentRewardOptions[0] = rareOptions[Random.Range(0, rareOptions.Count)];
                }
            }
        }

        private void RegisterVase(GameObject vase)
        {
            vases.Add(vase.GetComponent<SummoningVase>());
        }

        private int AliveVaseCount()
        {
            var alive = 0;
            foreach (var vase in vases)
            {
                if (vase != null && vase.IsAlive)
                {
                    alive++;
                }
            }

            return alive;
        }

        private List<SummoningVase> GetAliveVases()
        {
            var aliveVases = new List<SummoningVase>();
            foreach (var vase in vases)
            {
                if (vase != null && vase.IsAlive)
                {
                    aliveVases.Add(vase);
                }
            }

            return aliveVases;
        }

        private static bool IsRuntimeObject(string objectName)
        {
            return objectName.StartsWith("Player - Rune Hunter")
                || objectName.StartsWith("Possessed")
                || objectName.StartsWith("Vlkodlak")
                || objectName.StartsWith("Boss -")
                || objectName.StartsWith("MiniBoss -")
                || objectName.StartsWith("Enemy Projectile")
                || objectName.StartsWith("Boss Projectile")
                || objectName.StartsWith("Ashen Arena Floor")
                || objectName.StartsWith("Arena Decoration")
                || objectName.StartsWith("Blood Moon Key Light")
                || objectName.StartsWith("Rune Stone")
                || objectName.StartsWith("Broken Obelisk")
                || objectName.StartsWith("Summoning Vase")
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
