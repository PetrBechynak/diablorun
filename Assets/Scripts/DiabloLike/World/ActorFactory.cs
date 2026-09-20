using DiabloLike.Audio;
using DiabloLike.Combat;
using DiabloLike.Core;
using DiabloLike.UI;
using UnityEngine;

namespace DiabloLike.World
{
    public static class ActorFactory
    {
        public static GameObject CreatePlayer()
        {
            var player = new GameObject("Player - Rune Hunter");
            player.name = "Player - Rune Hunter";
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            var controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = Vector3.up;
            player.AddComponent<Health>().Configure(140);
            player.AddComponent<Mana>().Configure(90, 7f);
            player.AddComponent<CombatFaction>().Configure(Faction.Player);
            player.AddComponent<PlayerController>();
            var playerModel = ModelFactory.BuildPlayerModel(player.transform);
            var animator = player.AddComponent<ProceduralActorAnimator>();
            animator.Configure(playerModel.modelRoot, playerModel.weapon);
            return player;
        }

        public static GameObject CreateEnemy(Vector3 position, Transform player, GameDirector director, bool ranged = false)
        {
            var enemy = new GameObject("Vlkodlak");
            enemy.name = ranged ? "Vlkodlak Hexer" : "Vlkodlak";
            enemy.transform.position = position;

            var controller = enemy.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.42f;
            controller.center = Vector3.up * 0.9f;
            enemy.AddComponent<Health>().Configure(ranged ? 52 : 70);
            enemy.AddComponent<CombatFaction>().Configure(Faction.Enemy);
            enemy.AddComponent<EnemyHealthBar>();
            enemy.AddComponent<EnemySpeechBubble>();
            var enemyController = enemy.AddComponent<EnemyController>();
            enemyController.Configure(player, director);
            if (ranged)
            {
                enemyController.ConfigureRanged();
            }
            var modelRoot = ModelFactory.BuildEnemyModel(enemy.transform, ranged);
            enemy.AddComponent<ProceduralActorAnimator>().Configure(modelRoot, null);
            EffectFactory.SpawnEnemyArrival(position);
            DiabloAudio.Play(GameSfx.EnemySpawn, 0.08f);
            return enemy;
        }

        public static GameObject CreateBoss(Vector3 position, Transform player, int level, bool miniBoss = false)
        {
            var boss = new GameObject(miniBoss ? "MiniBoss - Rotmaw" : "Boss - Rotmaw");
            boss.transform.position = position;
            var controller = boss.AddComponent<CharacterController>();
            controller.height = 2.8f;
            controller.radius = 0.85f;
            controller.center = Vector3.up * 1.4f;
            var healthMultiplier = miniBoss ? 0.25f : 15f;
            boss.AddComponent<Health>().Configure(Mathf.RoundToInt((900 + level * 45) * 2 * healthMultiplier));
            boss.AddComponent<CombatFaction>().Configure(Faction.Enemy);
            boss.AddComponent<EnemyHealthBar>();
            var modelRoot = ModelFactory.BuildEnemyModel(boss.transform, false);
            modelRoot.localScale = Vector3.one * (miniBoss ? 1.35f : 1.8f);
            boss.AddComponent<BossController>().Configure(player, level, miniBoss);
            EffectFactory.SpawnEnemyArrival(position);
            DiabloAudio.Play(GameSfx.EnemySpawn, 0.12f);
            return boss;
        }

        public static GameObject CreateSummoningVase(Vector3 position, float height, GameDirector director)
        {
            var vase = CreateAssetVase(position, height);
            vase.name = "Summoning Vase";
            var collider = vase.AddComponent<CapsuleCollider>();
            collider.direction = 1;
            collider.center = new Vector3(0f, height * 0.25f, 0f);
            collider.radius = 0.25f;
            collider.height = Mathf.Max(0.5f, height * 0.5f);
            vase.AddComponent<Health>().Configure(Mathf.RoundToInt(120 + height * 25f));
            vase.AddComponent<CombatFaction>().Configure(Faction.Summoner);
            vase.AddComponent<EnemyHealthBar>();
            vase.AddComponent<SummoningVase>().Configure(director);
            return vase;
        }

        private static GameObject CreateAssetVase(Vector3 position, float height)
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Low Poly Trim Sheet Asset Collection/TrimSheet_Prefabs/Vase.prefab");
            if (prefab != null)
            {
                var imported = Object.Instantiate(prefab);
                imported.name = "Destructible Vase";
                imported.transform.position = position;
                imported.transform.localScale = Vector3.one * Mathf.Clamp(height / 2.5f, 0.8f, 1.35f);
                NormalizeImportedMaterials(imported);
                return imported;
            }
#endif
            return ModelFactory.BuildObelisk(position, height);
        }

        private static void NormalizeImportedMaterials(GameObject root)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.materials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var source = materials[i];
                    var normalized = new Material(shader) { name = $"{source?.name ?? "Vase"} - URP" };
                    if (source != null)
                    {
                        if (source.HasProperty("_MainTex")) normalized.mainTexture = source.mainTexture;
                        if (source.HasProperty("_Color")) normalized.color = source.color;
                    }
                    materials[i] = normalized;
                }
                renderer.materials = materials;
            }
        }

        public static GameObject CreateUpgradeChest(Vector3 position, GameDirector director)
        {
            var chest = new GameObject("Upgrade Chest");
            chest.transform.position = position;
            AddChestPart(chest.transform, "Base", new Vector3(0f, 0.32f, 0f), new Vector3(1.2f, 0.52f, 0.8f), new Color(0.28f, 0.13f, 0.045f));
            AddChestPart(chest.transform, "Lid", new Vector3(0f, 0.68f, -0.02f), new Vector3(1.25f, 0.24f, 0.84f), new Color(0.42f, 0.2f, 0.065f));
            AddChestPart(chest.transform, "Rune Lock", new Vector3(0f, 0.52f, -0.43f), new Vector3(0.22f, 0.22f, 0.05f), new Color(0.05f, 0.42f, 1f));
            var collider = chest.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.size = new Vector3(1.35f, 1.05f, 1f);
            collider.isTrigger = true;
            chest.AddComponent<UpgradeChest>().Configure(director);
            return chest;
        }

        public static GameObject CreateLevelPortal(Vector3 position, GameDirector director)
        {
            var portal = new GameObject("Level Portal");
            portal.transform.position = position;
            var light = portal.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 6f;
            light.intensity = 3f;
            light.color = new Color(0.55f, 0.05f, 1f);

            for (var i = 0; i < 12; i++)
            {
                var angle = i * Mathf.PI * 2f / 12f;
                var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stone.name = "Portal Rune";
                stone.transform.SetParent(portal.transform, false);
                stone.transform.localPosition = new Vector3(Mathf.Cos(angle) * 1.4f, 0.08f, Mathf.Sin(angle) * 1.4f);
                stone.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
                stone.transform.localScale = new Vector3(0.22f, 0.12f, 0.5f);
                stone.GetComponent<Renderer>().material = CreateMaterial(new Color(0.45f, 0.05f, 0.85f));
                Object.Destroy(stone.GetComponent<Collider>());
            }

            var trigger = portal.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.35f;
            portal.AddComponent<LevelPortal>().Configure(director);
            return portal;
        }

        public static void CreateCamera(Transform target)
        {
            var cameraObject = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                camera = cameraObject.AddComponent<Camera>();
            }

            if (cameraObject.GetComponent<AudioListener>() == null)
            {
                cameraObject.AddComponent<AudioListener>();
            }

            camera.fieldOfView = 42f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 120f;
            cameraObject.transform.position = target.position + new Vector3(0f, 13f, -10f);
            var follow = cameraObject.GetComponent<CameraFollow>();
            if (follow == null)
            {
                follow = cameraObject.AddComponent<CameraFollow>();
            }

            follow.Configure(target);
        }

        private static void AddChestPart(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().material = CreateMaterial(color);
            Object.Destroy(part.GetComponent<Collider>());
        }

        private static Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            return material;
        }

    }
}
