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
            CreateDecorations();
            CreateLavaRiversAndBridges();
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

        private static void CreateDecorations()
        {
            var stoneMaterial = CreateMaterial(new Color(0.22f, 0.2f, 0.2f));
            var metalMaterial = CreateMaterial(new Color(0.12f, 0.13f, 0.16f));
            var crystalMaterial = CreateMaterial(new Color(0.12f, 0.42f, 0.8f));
            var emberMaterial = CreateMaterial(new Color(0.95f, 0.16f, 0.03f));

            for (var i = 0; i < 16; i++)
            {
                var angle = i * Mathf.PI * 2f / 16f;
                var radius = i % 2 == 0 ? 10.8f : 9.8f;
                var position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "Arena Decoration - Ruined Rock";
                rock.transform.position = position + Vector3.up * Random.Range(0.2f, 0.55f);
                rock.transform.rotation = Random.rotation;
                rock.transform.localScale = Vector3.one * Random.Range(0.35f, 0.8f);
                rock.GetComponent<Renderer>().material = stoneMaterial;
                Object.Destroy(rock.GetComponent<Collider>());
            }

            for (var i = 0; i < 8; i++)
            {
                var angle = i * Mathf.PI * 2f / 8f + 0.2f;
                var position = new Vector3(Mathf.Cos(angle) * 11.8f, 0f, Mathf.Sin(angle) * 11.8f);
                CreatePillar(position, angle, metalMaterial, crystalMaterial, emberMaterial);
            }

            for (var i = 0; i < 6; i++)
            {
                var angle = i * Mathf.PI * 2f / 6f + 0.35f;
                var position = new Vector3(Mathf.Cos(angle) * 7.6f, 0f, Mathf.Sin(angle) * 7.6f);
                var crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crystal.name = "Arena Decoration - Rune Crystal";
                crystal.transform.position = position + Vector3.up * 0.45f;
                crystal.transform.rotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 35f);
                crystal.transform.localScale = new Vector3(0.18f, 0.7f, 0.18f);
                crystal.GetComponent<Renderer>().material = crystalMaterial;
                Object.Destroy(crystal.GetComponent<Collider>());
            }
        }

        private static void CreateLavaRiversAndBridges()
        {
            var lavaMaterial = CreateMaterial(new Color(0.85f, 0.07f, 0.015f));
            var lavaGlowMaterial = CreateMaterial(new Color(1f, 0.32f, 0.02f));
            var bridgeMaterial = CreateMaterial(new Color(0.25f, 0.12f, 0.055f));
            var bridgeTrimMaterial = CreateMaterial(new Color(0.42f, 0.24f, 0.09f));

            CreateLavaRiver(new Vector3(0f, 0.015f, -3.1f), new Vector3(24f, 0.035f, 1.35f), lavaMaterial, lavaGlowMaterial);
            CreateLavaRiver(new Vector3(0f, 0.02f, 3.1f), new Vector3(24f, 0.035f, 1.35f), lavaMaterial, lavaGlowMaterial);
            CreateBridge(new Vector3(-4.8f, 0.1f, -3.1f), bridgeMaterial, bridgeTrimMaterial);
            CreateBridge(new Vector3(4.8f, 0.1f, -3.1f), bridgeMaterial, bridgeTrimMaterial);
            CreateBridge(new Vector3(0f, 0.1f, 3.1f), bridgeMaterial, bridgeTrimMaterial);
        }

        private static void CreateLavaRiver(Vector3 position, Vector3 scale, Material lavaMaterial, Material glowMaterial)
        {
            var lava = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lava.name = "Arena Decoration - Lava River";
            lava.transform.position = position;
            lava.transform.localScale = scale;
            lava.GetComponent<Renderer>().material = lavaMaterial;
            Object.Destroy(lava.GetComponent<Collider>());

            for (var i = -5; i <= 5; i++)
            {
                var ember = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ember.name = "Arena Decoration - Lava Glow";
                ember.transform.position = position + new Vector3(i * 2.1f, 0.04f, Mathf.Sin(i * 2.3f) * 0.32f);
                ember.transform.localScale = new Vector3(0.75f, 0.025f, 0.08f);
                ember.GetComponent<Renderer>().material = glowMaterial;
                Object.Destroy(ember.GetComponent<Collider>());
            }
        }

        private static void CreateBridge(Vector3 position, Material bridgeMaterial, Material trimMaterial)
        {
            var bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bridge.name = "Arena Decoration - Bridge";
            bridge.transform.position = position;
            bridge.transform.localScale = new Vector3(1.3f, 0.16f, 2.1f);
            bridge.GetComponent<Renderer>().material = bridgeMaterial;

            for (var i = -2; i <= 2; i++)
            {
                var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plank.name = "Arena Decoration - Bridge Plank";
                plank.transform.position = position + new Vector3(0f, 0.1f, i * 0.38f);
                plank.transform.localScale = new Vector3(1.5f, 0.06f, 0.25f);
                plank.GetComponent<Renderer>().material = trimMaterial;
            }
        }

        private static void CreatePillar(Vector3 position, float angle, Material metalMaterial, Material crystalMaterial, Material emberMaterial)
        {
            var baseStone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseStone.name = "Arena Decoration - Pillar";
            baseStone.transform.position = position + Vector3.up * 0.75f;
            baseStone.transform.localScale = new Vector3(0.7f, 0.75f, 0.7f);
            baseStone.GetComponent<Renderer>().material = metalMaterial;
            Object.Destroy(baseStone.GetComponent<Collider>());

            var crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crystal.name = "Arena Decoration - Pillar Flame";
            crystal.transform.position = position + Vector3.up * 1.75f;
            crystal.transform.localScale = Vector3.one * 0.42f;
            crystal.GetComponent<Renderer>().material = angle % 2f > 1f ? crystalMaterial : emberMaterial;
            Object.Destroy(crystal.GetComponent<Collider>());
        }

        private static Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            return material;
        }
    }
}
