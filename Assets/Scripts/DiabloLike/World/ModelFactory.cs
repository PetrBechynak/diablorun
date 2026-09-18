using UnityEngine;

namespace DiabloLike.World
{
    public static class ModelFactory
    {
        public static (Transform modelRoot, Transform weapon) BuildPlayerModel(Transform root)
        {
            var modelRoot = new GameObject("Model").transform;
            modelRoot.SetParent(root, false);

            AddPart(modelRoot, "Coat", PrimitiveType.Capsule, new Vector3(0f, 1f, 0f), new Vector3(0.75f, 1.15f, 0.55f), new Color(0.08f, 0.18f, 0.28f));
            AddPart(modelRoot, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.9f, 0f), new Vector3(0.42f, 0.42f, 0.42f), new Color(0.72f, 0.55f, 0.42f));
            AddPart(modelRoot, "Left Shoulder", PrimitiveType.Sphere, new Vector3(-0.48f, 1.35f, 0f), new Vector3(0.34f, 0.24f, 0.34f), new Color(0.1f, 0.35f, 0.65f));
            AddPart(modelRoot, "Right Shoulder", PrimitiveType.Sphere, new Vector3(0.48f, 1.35f, 0f), new Vector3(0.34f, 0.24f, 0.34f), new Color(0.1f, 0.35f, 0.65f));
            var blade = AddPart(modelRoot, "Blade", PrimitiveType.Cube, new Vector3(0.62f, 1.05f, 0.52f), new Vector3(0.1f, 0.1f, 1.45f), new Color(0.78f, 0.82f, 0.86f));
            AddPart(modelRoot, "Hilt", PrimitiveType.Cube, new Vector3(0.62f, 0.85f, -0.12f), new Vector3(0.42f, 0.1f, 0.12f), new Color(0.48f, 0.29f, 0.12f));
            return (modelRoot, blade.transform);
        }

        public static Transform BuildEnemyModel(Transform root)
        {
            var modelRoot = new GameObject("Model").transform;
            modelRoot.SetParent(root, false);

            AddPart(modelRoot, "Rib Cage", PrimitiveType.Capsule, new Vector3(0f, 1.02f, 0f), new Vector3(0.82f, 1.05f, 0.56f), new Color(0.34f, 0.035f, 0.03f));
            AddPart(modelRoot, "Hunched Back", PrimitiveType.Sphere, new Vector3(0f, 1.24f, -0.22f), new Vector3(0.85f, 0.52f, 0.62f), new Color(0.2f, 0.025f, 0.025f));
            AddPart(modelRoot, "Skull", PrimitiveType.Sphere, new Vector3(0f, 1.78f, 0.1f), new Vector3(0.42f, 0.34f, 0.36f), new Color(0.62f, 0.18f, 0.13f));
            AddPart(modelRoot, "Jaw", PrimitiveType.Cube, new Vector3(0f, 1.58f, 0.24f), new Vector3(0.34f, 0.12f, 0.2f), new Color(0.16f, 0.015f, 0.012f));
            AddPart(modelRoot, "Left Horn", PrimitiveType.Cube, new Vector3(-0.28f, 2.02f, 0.03f), new Vector3(0.1f, 0.42f, 0.1f), new Color(0.08f, 0.01f, 0.008f), Quaternion.Euler(0f, 0f, -24f));
            AddPart(modelRoot, "Right Horn", PrimitiveType.Cube, new Vector3(0.28f, 2.02f, 0.03f), new Vector3(0.1f, 0.42f, 0.1f), new Color(0.08f, 0.01f, 0.008f), Quaternion.Euler(0f, 0f, 24f));
            AddPart(modelRoot, "Left Arm", PrimitiveType.Cube, new Vector3(-0.68f, 1.08f, 0.08f), new Vector3(0.18f, 0.24f, 0.85f), new Color(0.48f, 0.045f, 0.035f), Quaternion.Euler(26f, -12f, 12f));
            AddPart(modelRoot, "Right Arm", PrimitiveType.Cube, new Vector3(0.68f, 1.08f, 0.08f), new Vector3(0.18f, 0.24f, 0.85f), new Color(0.48f, 0.045f, 0.035f), Quaternion.Euler(26f, 12f, -12f));
            AddPart(modelRoot, "Left Claw", PrimitiveType.Cube, new Vector3(-0.82f, 0.72f, 0.5f), new Vector3(0.14f, 0.12f, 0.48f), new Color(0.8f, 0.08f, 0.045f), Quaternion.Euler(42f, 0f, 8f));
            AddPart(modelRoot, "Right Claw", PrimitiveType.Cube, new Vector3(0.82f, 0.72f, 0.5f), new Vector3(0.14f, 0.12f, 0.48f), new Color(0.8f, 0.08f, 0.045f), Quaternion.Euler(42f, 0f, -8f));
            AddPart(modelRoot, "Left Leg", PrimitiveType.Cube, new Vector3(-0.24f, 0.34f, -0.05f), new Vector3(0.22f, 0.72f, 0.22f), new Color(0.25f, 0.025f, 0.022f), Quaternion.Euler(-8f, 0f, 5f));
            AddPart(modelRoot, "Right Leg", PrimitiveType.Cube, new Vector3(0.24f, 0.34f, -0.05f), new Vector3(0.22f, 0.72f, 0.22f), new Color(0.25f, 0.025f, 0.022f), Quaternion.Euler(-8f, 0f, -5f));
            AddPart(modelRoot, "Chest Ember", PrimitiveType.Sphere, new Vector3(0f, 1.1f, 0.34f), new Vector3(0.28f, 0.2f, 0.08f), new Color(1f, 0.07f, 0.02f));
            AddPart(modelRoot, "Back Spike", PrimitiveType.Cube, new Vector3(0f, 1.32f, -0.54f), new Vector3(0.2f, 0.2f, 0.9f), new Color(0.12f, 0.012f, 0.012f), Quaternion.Euler(35f, 0f, 0f));
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
    }
}
