using UnityEngine;

namespace DiabloLike.World
{
    public static class ArenaFactory
    {
        public static void BuildArena()
        {
            CreateFloor();
            CreateLight();
            CreateRuneRing();
        }

        private static void CreateFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Ashen Arena Floor";
            floor.transform.position = new Vector3(0f, -0.55f, 0f);
            floor.transform.localScale = new Vector3(28f, 1f, 28f);

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.14f, 0.13f, 0.12f);
            floor.GetComponent<Renderer>().material = material;
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("Blood Moon Key Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.72f, 0.58f);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateRuneRing()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.85f, 0.08f, 0.04f);

            for (var i = 0; i < 20; i++)
            {
                var angle = i * Mathf.PI * 2f / 20f;
                var rune = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rune.name = "Rune Stone";
                rune.transform.position = new Vector3(Mathf.Cos(angle), 0.05f, Mathf.Sin(angle)) * 5f;
                rune.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
                rune.transform.localScale = new Vector3(0.22f, 0.1f, 0.75f);
                rune.GetComponent<Renderer>().material = material;
            }
        }
    }
}
