#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DiabloLike.World;

namespace DiabloLike.EditorTools
{
    public static class ArenaLayoutGenerator
    {
        private const string Folder = "Assets/Prefabs/Arenas";
        private const string MaterialFolder = "Assets/Prefabs/Arenas/GeneratedMaterials";

        [MenuItem("Tests/Arenas/Create Static Arena Prefabs")]
        public static void CreateStaticArenaPrefabs()
        {
            EnsureFolder();
            CreateArena("Arena01", 1);
            CreateArena("Arena02", 2);
            CreateArena("BossArena", 3);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created static arena prefabs in Assets/Prefabs/Arenas.");
        }

        private static void CreateArena(string name, int variant)
        {
            var existing = new HashSet<GameObject>(
                Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None));

            ArenaFactory.BuildArena();

            var root = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(root, $"Create {name}");
            var groups = new Dictionary<string, Transform>();
            foreach (var groupName in new[]
                     { "Geometry", "Rocks", "Pillars", "Bridges", "Lava", "Runes", "Other Decorations" })
            {
                var group = new GameObject(groupName);
                group.transform.SetParent(root.transform, false);
                group.isStatic = true;
                groups[groupName] = group.transform;
            }

            var spawnGroup = new GameObject("Spawn Points");
            spawnGroup.transform.SetParent(root.transform, false);
            groups["Spawn Points"] = spawnGroup.transform;

            var newRoots = new List<GameObject>();
            foreach (var candidate in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (existing.Contains(candidate) || candidate == root || candidate.transform.parent != null)
                {
                    continue;
                }

                newRoots.Add(candidate);
            }

            foreach (var child in newRoots)
            {
                var groupName = GroupFor(child.name);
                child.transform.SetParent(groups[groupName], true);
                SetStaticRecursively(child);
            }

            CreateBaseFloor(groups["Geometry"]);

            var layout = root.AddComponent<ArenaLayout>();
            var player = CreatePoint(spawnGroup.transform, "PlayerSpawn", Vector3.zero);
            var chest = CreatePoint(spawnGroup.transform, "ChestSpawn", new Vector3(0f, 0f, 2.2f));
            var portal = CreatePoint(spawnGroup.transform, "PortalSpawn", new Vector3(0f, 0f, -3.2f));
            var vasePositions = variant == 2
                ? new[] { new Vector3(-9f, 0f, -4f), new Vector3(9f, 0f, -4f), new Vector3(-8f, 0f, 7f), new Vector3(8f, 0f, 7f) }
                : new[] { new Vector3(-8f, 0f, -7f), new Vector3(8f, 0f, -6f), new Vector3(-7f, 0f, 8f), new Vector3(7f, 0f, 7f) };
            var vases = new Transform[vasePositions.Length];
            for (var i = 0; i < vasePositions.Length; i++)
            {
                vases[i] = CreatePoint(spawnGroup.transform, $"VaseSpawn_{i + 1}", vasePositions[i]);
            }

            layout.Configure(player, vases, chest, portal);

            MakeMaterialsPersistent(root, name);

            if (variant == 2)
            {
                root.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            }
            else if (variant == 3)
            {
                root.transform.localScale = new Vector3(1.12f, 1f, 1.12f);
            }

            var path = $"{Folder}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static Transform CreatePoint(Transform parent, string name, Vector3 position)
        {
            var point = new GameObject(name);
            point.transform.SetParent(parent, false);
            point.transform.localPosition = position;
            return point.transform;
        }

        private static void CreateBaseFloor(Transform parent)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Arena Base Floor";
            floor.transform.SetParent(parent, false);
            floor.transform.localPosition = new Vector3(0f, -0.16f, 0f);
            floor.transform.localScale = Vector3.one * 2.9f;
            floor.isStatic = true;
        }

        private static string GroupFor(string objectName)
        {
            if (objectName.Contains("Floor")) return "Geometry";
            if (objectName.Contains("Rock")) return "Rocks";
            if (objectName.Contains("Pillar")) return "Pillars";
            if (objectName.Contains("Bridge")) return "Bridges";
            if (objectName.Contains("Lava")) return "Lava";
            if (objectName.Contains("Rune")) return "Runes";
            return "Other Decorations";
        }

        private static void SetStaticRecursively(GameObject root)
        {
            root.isStatic = true;
            foreach (Transform child in root.transform)
            {
                SetStaticRecursively(child.gameObject);
            }
        }

        private static void MakeMaterialsPersistent(GameObject root, string arenaName)
        {
            EnsureMaterialFolder();
            var lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (lit == null)
            {
                return;
            }

            var materialIndex = 0;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var source = materials[i];
                    var material = new Material(lit)
                    {
                        name = $"{arenaName}_{renderer.name}_{materialIndex++}"
                    };

                    if (source != null && source.shader != null)
                    {
                        if (source.HasProperty("_Color"))
                        {
                            material.color = source.color;
                        }
                        if (source.HasProperty("_MainTex"))
                        {
                            material.mainTexture = source.mainTexture;
                        }
                    }
                    else
                    {
                        material.color = ColorFor(renderer.name);
                    }

                    var assetPath = AssetDatabase.GenerateUniqueAssetPath(
                        $"{MaterialFolder}/{material.name}.mat");
                    AssetDatabase.CreateAsset(material, assetPath);
                    materials[i] = material;
                }

                renderer.sharedMaterials = materials;
            }
        }

        private static Color ColorFor(string objectName)
        {
            if (objectName.Contains("Lava")) return new Color(0.9f, 0.08f, 0.015f);
            if (objectName.Contains("Crystal") || objectName.Contains("Rune")) return new Color(0.1f, 0.35f, 0.85f);
            if (objectName.Contains("Bridge")) return new Color(0.3f, 0.14f, 0.05f);
            if (objectName.Contains("Pillar") || objectName.Contains("Rock")) return new Color(0.25f, 0.23f, 0.23f);
            return new Color(0.35f, 0.42f, 0.28f);
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Arenas");
            }

            EnsureMaterialFolder();
        }

        private static void EnsureMaterialFolder()
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                AssetDatabase.CreateFolder(Folder, "GeneratedMaterials");
            }
        }
    }
}
#endif
