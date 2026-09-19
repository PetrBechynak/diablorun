using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DiabloLike.World
{
    public static class ModelFactory
    {
        public static (Transform modelRoot, Transform weapon) BuildPlayerModel(Transform root)
        {
#if UNITY_EDITOR
            var importedPlayer = LoadImportedPrefab("Assets/Synty/SidekickCharacters/Characters/Starter/Starter_01/Starter_01.prefab");
            if (importedPlayer != null)
            {
                importedPlayer.name = "Imported Hero - Sidekick";
                SetupImportedAnimator(importedPlayer);
                importedPlayer.transform.SetParent(root, false);
                importedPlayer.transform.localPosition = Vector3.zero;
                importedPlayer.transform.localRotation = Quaternion.identity;
                importedPlayer.transform.localScale = Vector3.one * 1.15f;
                var playerWeapon = AttachImportedWeapon(importedPlayer.transform,
                    "Assets/Synty/SidekickCharacters/_Demos/Meshes/Weapons/Pirate_Sword/SK_Sword.fbx",
                    "Equipped Sword", Vector3.zero, Quaternion.Euler(90f, 180f, 0f));
                return (importedPlayer.transform, playerWeapon);
            }
#endif
            var modelRoot = new GameObject("Model").transform;
            modelRoot.SetParent(root, false);

            AddPart(modelRoot, "Cloak", PrimitiveType.Capsule, new Vector3(0f, 0.95f, 0f), new Vector3(0.78f, 1.08f, 0.58f), new Color(0.035f, 0.08f, 0.18f));
            AddPart(modelRoot, "Cloak Trim", PrimitiveType.Cylinder, new Vector3(0f, 0.38f, 0f), new Vector3(0.52f, 0.08f, 0.52f), new Color(0.08f, 0.25f, 0.52f));
            AddPart(modelRoot, "Hood", PrimitiveType.Sphere, new Vector3(0f, 1.85f, -0.04f), new Vector3(0.58f, 0.58f, 0.52f), new Color(0.025f, 0.045f, 0.1f));
            AddPart(modelRoot, "Face", PrimitiveType.Sphere, new Vector3(0f, 1.78f, 0.27f), new Vector3(0.31f, 0.34f, 0.16f), new Color(0.48f, 0.3f, 0.2f));
            AddPart(modelRoot, "Rune Eye L", PrimitiveType.Sphere, new Vector3(-0.12f, 1.84f, 0.39f), new Vector3(0.06f, 0.06f, 0.035f), new Color(0.2f, 0.8f, 1f));
            AddPart(modelRoot, "Rune Eye R", PrimitiveType.Sphere, new Vector3(0.12f, 1.84f, 0.39f), new Vector3(0.06f, 0.06f, 0.035f), new Color(0.2f, 0.8f, 1f));
            AddPart(modelRoot, "Shoulder L", PrimitiveType.Sphere, new Vector3(-0.48f, 1.28f, 0f), new Vector3(0.34f, 0.25f, 0.34f), new Color(0.08f, 0.28f, 0.62f));
            AddPart(modelRoot, "Shoulder R", PrimitiveType.Sphere, new Vector3(0.48f, 1.28f, 0f), new Vector3(0.34f, 0.25f, 0.34f), new Color(0.08f, 0.28f, 0.62f));
            var staff = AddPart(modelRoot, "Arcane Staff", PrimitiveType.Cylinder, new Vector3(0.67f, 1.1f, 0.28f), new Vector3(0.07f, 1.25f, 0.07f), new Color(0.32f, 0.16f, 0.07f), Quaternion.Euler(10f, 0f, -8f));
            AddPart(modelRoot, "Staff Crystal", PrimitiveType.Sphere, new Vector3(0.55f, 2.32f, 0.18f), new Vector3(0.2f, 0.28f, 0.2f), new Color(0.15f, 0.65f, 1f));
            return (modelRoot, staff.transform);
        }

        public static Transform BuildEnemyModel(Transform root, bool ranged = false)
        {
#if UNITY_EDITOR
            var enemyPrefab = ranged
                ? "Assets/Synty/SidekickCharacters/Characters/Starter/Starter_03/Starter_03.prefab"
                : "Assets/Synty/SidekickCharacters/Characters/Starter/Starter_02/Starter_02.prefab";
            var importedEnemy = LoadImportedPrefab(enemyPrefab);
            if (importedEnemy != null)
            {
                importedEnemy.name = "Imported Enemy - Sidekick";
                SetupImportedAnimator(importedEnemy);
                importedEnemy.transform.SetParent(root, false);
                importedEnemy.transform.localPosition = Vector3.zero;
                importedEnemy.transform.localRotation = Quaternion.identity;
                importedEnemy.transform.localScale = Vector3.one * 1.1f;
                AttachImportedWeapon(importedEnemy.transform,
                    "Assets/Synty/SidekickCharacters/_Demos/Meshes/Weapons/Goblin_Axe/SK_Axe.fbx",
                    "Enemy Axe", new Vector3(0.04f, -0.02f, 0.06f), Quaternion.Euler(90f, 0f, 0f));
                return importedEnemy.transform;
            }
#endif
            var modelRoot = new GameObject("Model").transform;
            modelRoot.SetParent(root, false);

            AddPart(modelRoot, "Demon Body", PrimitiveType.Capsule, new Vector3(0f, 0.98f, 0f), new Vector3(0.8f, 1.08f, 0.58f), new Color(0.22f, 0.025f, 0.035f));
            AddPart(modelRoot, "Shoulder Mantle", PrimitiveType.Sphere, new Vector3(0f, 1.4f, -0.08f), new Vector3(0.92f, 0.35f, 0.62f), new Color(0.09f, 0.012f, 0.02f));
            AddPart(modelRoot, "Demon Head", PrimitiveType.Sphere, new Vector3(0f, 1.78f, 0.08f), new Vector3(0.45f, 0.42f, 0.4f), new Color(0.45f, 0.06f, 0.045f));
            AddPart(modelRoot, "Eye L", PrimitiveType.Sphere, new Vector3(-0.16f, 1.82f, 0.39f), new Vector3(0.07f, 0.06f, 0.035f), new Color(1f, 0.55f, 0.05f));
            AddPart(modelRoot, "Eye R", PrimitiveType.Sphere, new Vector3(0.16f, 1.82f, 0.39f), new Vector3(0.07f, 0.06f, 0.035f), new Color(1f, 0.55f, 0.05f));
            AddPart(modelRoot, "Horn L", PrimitiveType.Cylinder, new Vector3(-0.28f, 2.18f, 0.02f), new Vector3(0.16f, 0.55f, 0.16f), new Color(0.12f, 0.012f, 0.01f), Quaternion.Euler(0f, 0f, -18f));
            AddPart(modelRoot, "Horn R", PrimitiveType.Cylinder, new Vector3(0.28f, 2.18f, 0.02f), new Vector3(0.16f, 0.55f, 0.16f), new Color(0.12f, 0.012f, 0.01f), Quaternion.Euler(0f, 0f, 18f));
            AddPart(modelRoot, "Claw L", PrimitiveType.Capsule, new Vector3(-0.68f, 0.95f, 0.22f), new Vector3(0.2f, 0.62f, 0.2f), new Color(0.36f, 0.035f, 0.04f), Quaternion.Euler(20f, 0f, 22f));
            AddPart(modelRoot, "Claw R", PrimitiveType.Capsule, new Vector3(0.68f, 0.95f, 0.22f), new Vector3(0.2f, 0.62f, 0.2f), new Color(0.36f, 0.035f, 0.04f), Quaternion.Euler(20f, 0f, -22f));
            AddPart(modelRoot, "Chest Core", PrimitiveType.Sphere, new Vector3(0f, 1.12f, 0.38f), new Vector3(0.22f, 0.24f, 0.08f), new Color(1f, 0.12f, 0.015f));
            return modelRoot;
        }

        public static GameObject BuildObelisk(Vector3 position, float height)
        {
            var root = new GameObject("Broken Obelisk");
            root.transform.position = position;
            AddPart(root.transform, "Stone Core", PrimitiveType.Cube, new Vector3(0f, height * 0.5f, 0f), new Vector3(0.75f, height, 0.75f), new Color(0.18f, 0.17f, 0.16f));
            AddPart(root.transform, "Rune Glow", PrimitiveType.Cube, new Vector3(0f, height * 0.62f, -0.39f), new Vector3(0.38f, 0.08f, 0.03f), new Color(0.85f, 0.05f, 0.03f));
            return root;
        }

        private static GameObject AddPart(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color, Quaternion rotation = default)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = rotation == default ? Quaternion.identity : rotation;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().material = CreateMaterial(color);
            Object.Destroy(part.GetComponent<Collider>());
            return part;
        }

        private static Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            return material;
        }

#if UNITY_EDITOR
        private static GameObject LoadImportedPrefab(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return prefab == null ? null : Object.Instantiate(prefab);
        }

        private static void SetupImportedAnimator(GameObject actor)
        {
            var animator = actor.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                return;
            }

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/Synty/SidekickCharacters/Animations/New Animator Controller.controller");
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.enabled = true;
            }
        }

        private static Transform AttachImportedWeapon(Transform parent, string assetPath, string objectName, Vector3 localPosition, Quaternion localRotation)
        {
            var weaponAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (weaponAsset == null)
            {
                return null;
            }

            var hand = FindRightHand(parent);
            var weapon = Object.Instantiate(weaponAsset, hand != null ? hand : parent);
            weapon.name = objectName;
            weapon.transform.localPosition = localPosition;
            weapon.transform.localRotation = localRotation;
            weapon.transform.localScale = Vector3.one * 0.8f;
            NormalizeWeaponMaterials(weapon);
            foreach (var collider in weapon.GetComponentsInChildren<Collider>(true))
            {
                Object.Destroy(collider);
            }
            return weapon.transform;
        }

        private static Transform FindRightHand(Transform root)
        {
            // Prefer the deforming hand bone. IK targets such as ik_hand_r are
            // not part of the animated skeleton and can leave a weapon behind.
            foreach (var candidate in root.GetComponentsInChildren<Transform>(true))
            {
                var normalized = candidate.name.Replace(" ", "").Replace("_", "").ToLowerInvariant();
                if (normalized == "handr" || normalized == "righthand" || normalized == "mixamorig:righthand")
                {
                    return candidate;
                }
            }
            foreach (var candidate in root.GetComponentsInChildren<Transform>(true))
            {
                var normalized = candidate.name.Replace(" ", "").Replace("_", "").ToLowerInvariant();
                if (!normalized.Contains("ik") && (normalized.EndsWith("righthand") || normalized.EndsWith("handr")))
                {
                    return candidate;
                }
            }
            return null;
        }

        private static void NormalizeWeaponMaterials(GameObject weapon)
        {
            var urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                return;
            }

            foreach (var renderer in weapon.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.materials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var source = materials[i];
                    var texture = source != null && source.HasProperty("_MainTex") ? source.mainTexture : null;
                    var color = source != null && source.HasProperty("_Color") ? source.color : Color.white;
                    var normalized = new Material(urpShader)
                    {
                        name = $"{source?.name ?? "Weapon"} - URP"
                    };
                    normalized.color = color;
                    if (texture != null)
                    {
                        normalized.mainTexture = texture;
                    }
                    materials[i] = normalized;
                }
                renderer.materials = materials;
            }
        }
#endif
    }
}
