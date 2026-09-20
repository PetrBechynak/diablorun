using UnityEngine;
using UnityEngine.VFX;

namespace DiabloLike.World
{
    public static class EffectFactory
    {
        public static void SpawnSlash(Vector3 position, Quaternion rotation)
        {
#if UNITY_EDITOR
            var chargeSlash = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/slash5-HungNguyen/prefab/slash/white-black bolder.prefab");
            if (chargeSlash != null)
            {
                var instance = Object.Instantiate(chargeSlash, position + Vector3.up * 0.18f, rotation);
                instance.name = "Curved Sword Slash VFX";
                instance.transform.localScale = Vector3.one * 1.25f;
                foreach (var visualEffect in instance.GetComponentsInChildren<VisualEffect>(true))
                {
                    visualEffect.enabled = true;
                    visualEffect.Reinit();
                    visualEffect.Play();
                }
                instance.AddComponent<VfxBoundsOverride>().Configure(Vector3.one * 8f);
                instance.AddComponent<SlashArcFallback>();
                instance.AddComponent<SlashLifetime>();
                Object.Destroy(instance, 0.5f);
                return;
            }
#endif

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
            var collider = essence.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 1.5f;
        }

        public static void SpawnExplosion(Vector3 position)
        {
            SpawnBurst(position + Vector3.up * 0.35f, new Color(1f, 0.35f, 0.03f), 24, 1.1f);
            SpawnRing(position + Vector3.up * 0.05f, new Color(1f, 0.2f, 0.02f), 1.35f);
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

    internal sealed class VfxBoundsOverride : MonoBehaviour
    {
        private Vector3 size;

        public VfxBoundsOverride Configure(Vector3 boundsSize)
        {
            size = boundsSize;
            return this;
        }

        private void LateUpdate()
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.bounds = new Bounds(transform.position, size);
            }
        }
    }

    /// <summary>
    /// Keeps the imported slash readable when the legacy VFX Graph asset is culled
    /// by Unity 6 before it produces a particle. It is deliberately unlit,
    /// transparent, shadowless and animated; it is not a world-space prop.
    /// </summary>
    internal sealed class SlashArcFallback : MonoBehaviour
    {
        private float age;
        private Transform arc;
        private Renderer arcRenderer;

        private void Awake()
        {
            arc = new GameObject("Slash arc visual").transform;
            arc.SetParent(transform, false);

            var meshFilter = arc.gameObject.AddComponent<MeshFilter>();
            var meshRenderer = arc.gameObject.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = BuildMesh();
            meshRenderer.sharedMaterial = BuildMaterial();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            arcRenderer = meshRenderer;
        }

        private void Update()
        {
            age += Time.deltaTime;
            var t = Mathf.Clamp01(age / 0.42f);
            arc.localScale = Vector3.one * Mathf.Lerp(0.45f, 1.15f, Mathf.SmoothStep(0f, 1f, t));
            // Mirror left/right across the player's front-to-back axis.
            // This is a local Z flip, not a 180-degree turn around world Y.
            arc.localRotation = Quaternion.Euler(180f, -45, 180f);
            if (arcRenderer != null)
                arcRenderer.material.color = Color.Lerp(new Color(1f, 1f, 1f, 0.98f), new Color(0.48f, 0.82f, 1f, 0.82f), t);
        }

        private static Mesh BuildMesh()
        {
            const int segments = 28;
            var vertices = new Vector3[(segments + 1) * 2];
            var triangles = new int[segments * 6];
            for (var i = 0; i <= segments; i++)
            {
                var t = i / (float)segments;
                var angle = Mathf.Lerp(-68f, 68f, t) * Mathf.Deg2Rad;
                var radius = Mathf.Lerp(0.55f, 1.75f, t);
                var width = Mathf.Lerp(0.17f, 0.045f, t) * Mathf.Sin(t * Mathf.PI);
                var direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                var side = new Vector3(Mathf.Cos(angle), 0f, -Mathf.Sin(angle));
                var center = direction * radius;
                vertices[i * 2] = center - side * width;
                vertices[i * 2 + 1] = center + side * width;
                if (i == segments) continue;
                var v = i * 2;
                var q = i * 6;
                triangles[q] = v;
                triangles[q + 1] = v + 2;
                triangles[q + 2] = v + 1;
                triangles[q + 3] = v + 1;
                triangles[q + 4] = v + 2;
                triangles[q + 5] = v + 3;
            }

            var mesh = new Mesh { name = "Runtime Slash Arc" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material BuildMaterial()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = new Color(0.62f, 0.86f, 1f, 0.92f);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = 3000;
            return material;
        }
    }

    internal sealed class SlashLifetime : MonoBehaviour
    {
        private void Awake()
        {
            Invoke(nameof(Remove), 0.5f);
        }

        private void Remove()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
