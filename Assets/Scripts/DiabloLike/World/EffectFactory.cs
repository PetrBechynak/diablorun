using UnityEngine;

namespace DiabloLike.World
{
    public static class EffectFactory
    {
        public static void SpawnSlash(Vector3 position, Quaternion rotation)
        {
            const int segments = 9;
            for (var i = 0; i < segments; i++)
            {
                var t = i / (float)(segments - 1);
                var angle = Mathf.Lerp(-62f, 62f, t);
                var radius = Mathf.Lerp(0.75f, 1.65f, t);
                var local = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
                var segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = "Curved Sword Slash VFX";
                segment.transform.position = position + rotation * local + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.18f;
                segment.transform.rotation = rotation * Quaternion.Euler(0f, angle + 90f, 0f);
                segment.transform.localScale = new Vector3(0.46f, 0.045f, Mathf.Lerp(0.18f, 0.34f, Mathf.Sin(t * Mathf.PI)));
                segment.GetComponent<Renderer>().material = CreateMaterial(Color.Lerp(new Color(0.35f, 0.68f, 1f, 0.65f), new Color(0.9f, 0.96f, 1f, 0.9f), Mathf.Sin(t * Mathf.PI)));
                Object.Destroy(segment.GetComponent<Collider>());
                segment.AddComponent<TransientScaleEffect>().Configure(0.16f, segment.transform.localScale, segment.transform.localScale * 0.35f);
            }
        }

        public static void SpawnHit(Vector3 position)
        {
            SpawnBurst(position + Vector3.up * 0.9f, new Color(1f, 0.12f, 0.04f), 10, 0.45f);
        }

        public static void SpawnEnemyArrival(Vector3 position)
        {
            SpawnBurst(position + Vector3.up * 0.12f, new Color(0.85f, 0.05f, 0.03f), 30, 1.15f);
            SpawnRing(position + Vector3.up * 0.08f, new Color(0.75f, 0.02f, 0.02f), 1.4f);
        }

        public static void SpawnEnemyDeath(Vector3 position)
        {
            SpawnBurst(position + Vector3.up * 0.8f, new Color(0.95f, 0.02f, 0.02f), 28, 1.05f);
            SpawnBurst(position + Vector3.up * 0.35f, new Color(0.16f, 0.01f, 0.01f), 16, 0.75f);
            SpawnRing(position + Vector3.up * 0.05f, new Color(0.95f, 0.08f, 0.02f), 1.15f);
        }

        public static void SpawnLootEssence(Vector3 position)
        {
            var essence = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            essence.name = "Blood Essence Loot";
            essence.transform.position = position + Vector3.up * 0.35f;
            essence.transform.localScale = Vector3.one * 0.32f;
            essence.GetComponent<Renderer>().material = CreateMaterial(new Color(1f, 0.08f, 0.12f));
            Object.Destroy(essence.GetComponent<Collider>());
            essence.AddComponent<FloatingLoot>();
        }

        private static void SpawnBurst(Vector3 origin, Color color, int count, float radius)
        {
            for (var i = 0; i < count; i++)
            {
                var spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = "Combat Spark";
                spark.transform.position = origin;
                spark.transform.localScale = Vector3.one * Random.Range(0.05f, 0.12f);
                spark.GetComponent<Renderer>().material = CreateMaterial(color);
                Object.Destroy(spark.GetComponent<Collider>());

                var direction2D = Random.insideUnitCircle.normalized * Random.Range(radius * 0.35f, radius);
                var target = origin + new Vector3(direction2D.x, Random.Range(0.25f, 0.9f), direction2D.y);
                spark.AddComponent<TransientMoveEffect>().Configure(Random.Range(0.22f, 0.45f), origin, target);
            }
        }

        private static void SpawnRing(Vector3 origin, Color color, float radius)
        {
            for (var i = 0; i < 18; i++)
            {
                var angle = i * Mathf.PI * 2f / 18f;
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = "Combat Spark";
                shard.transform.position = origin + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                shard.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
                shard.transform.localScale = new Vector3(0.18f, 0.035f, 0.48f);
                shard.GetComponent<Renderer>().material = CreateMaterial(color);
                Object.Destroy(shard.GetComponent<Collider>());
                shard.AddComponent<TransientScaleEffect>().Configure(0.55f, shard.transform.localScale, Vector3.zero);
            }
        }

        private static Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            return material;
        }
    }
}
