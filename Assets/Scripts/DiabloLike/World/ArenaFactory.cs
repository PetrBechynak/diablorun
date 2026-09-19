using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
            const int gridSize = 32;
            const float worldSize = 28f;
            const float baseHeight = -0.08f;
            const float waveHeight = 0.12f;
            var floor = new GameObject("Rocky Grass Arena Floor");
            var meshFilter = floor.AddComponent<MeshFilter>();
            var meshRenderer = floor.AddComponent<MeshRenderer>();
            var meshCollider = floor.AddComponent<MeshCollider>();

            var vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[gridSize * gridSize * 6];
            for (var z = 0; z <= gridSize; z++)
            {
                for (var x = 0; x <= gridSize; x++)
                {
                    var index = z * (gridSize + 1) + x;
                    var px = (x / (float)gridSize - 0.5f) * worldSize;
                    var pz = (z / (float)gridSize - 0.5f) * worldSize;
                    var height = Mathf.PerlinNoise((px + 40f) * 0.12f, (pz + 40f) * 0.12f) * waveHeight;
                    vertices[index] = new Vector3(px, baseHeight + height, pz);
                    uv[index] = new Vector2(x / (float)gridSize * 8f, z / (float)gridSize * 8f);
                }
            }

            var triangleIndex = 0;
            for (var z = 0; z < gridSize; z++)
            {
                for (var x = 0; x < gridSize; x++)
                {
                    var a = z * (gridSize + 1) + x;
                    var b = a + 1;
                    var c = a + gridSize + 1;
                    var d = c + 1;
                    triangles[triangleIndex++] = a;
                    triangles[triangleIndex++] = c;
                    triangles[triangleIndex++] = b;
                    triangles[triangleIndex++] = b;
                    triangles[triangleIndex++] = c;
                    triangles[triangleIndex++] = d;
                }
            }

            var mesh = new Mesh { name = "Rocky Grass Terrain Mesh" };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = Color.white;
#if UNITY_EDITOR
            var diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Environment/RockyTerrain/textures/rocky_terrain_02_diff_1k.jpg");
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Environment/RockyTerrain/textures/rocky_terrain_02_nor_gl_1k.exr");
            if (diffuse != null) material.mainTexture = diffuse;
            if (normal != null && material.HasProperty("_BumpMap"))
            {
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture("_BumpMap", normal);
                material.SetFloat("_BumpScale", 0.35f);
            }
#endif
            meshRenderer.sharedMaterial = material;
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
            var material = CreateMaterial(new Color(0.85f, 0.08f, 0.04f));
            for (var i = 0; i < 20; i++)
            {
                var angle = i * Mathf.PI * 2f / 20f;
                var rune = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rune.name = "Rune Pillar";
                rune.transform.position = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 5f;
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
                var rock = CreateRockAsset(i);
                rock.name = "Arena Decoration - Ruined Rock";
                NormalizeImportedMaterials(rock);
                rock.transform.position = position + Vector3.up * Random.Range(0.2f, 0.55f);
                rock.transform.rotation = Random.rotation;
                rock.transform.localScale = Vector3.one * Random.Range(0.35f, 0.8f) * 0.3f;
                if (rock.GetComponentInChildren<Renderer>() == null)
                {
                    rock.GetComponent<Renderer>().material = stoneMaterial;
                }
                foreach (var collider in rock.GetComponentsInChildren<Collider>()) Object.Destroy(collider);
                AddSolidRockCollider(rock);
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

            CreateLavaRiver(new Vector3(0f, 0.015f, -3.1f), new Vector3(24f, 0.035f, 1.35f), lavaMaterial, lavaGlowMaterial, -4.8f, 4.8f);
            CreateLavaRiver(new Vector3(0f, 0.02f, 3.1f), new Vector3(24f, 0.035f, 1.35f), lavaMaterial, lavaGlowMaterial, 0f);
            CreateBridge(new Vector3(-4.8f, 0.1f, -3.1f), bridgeMaterial, bridgeTrimMaterial);
            CreateBridge(new Vector3(4.8f, 0.1f, -3.1f), bridgeMaterial, bridgeTrimMaterial);
            CreateBridge(new Vector3(0f, 0.1f, 3.1f), bridgeMaterial, bridgeTrimMaterial);
        }

        private static void CreateLavaRiver(Vector3 position, Vector3 scale, Material lavaMaterial, Material glowMaterial, params float[] bridgePositions)
        {
            var lava = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lava.name = "Arena Decoration - Lava River";
            lava.transform.position = position;
            lava.transform.localScale = scale;
            lava.GetComponent<Renderer>().material = lavaMaterial;
            Object.Destroy(lava.GetComponent<Collider>());
            var lavaCollider = lava.AddComponent<BoxCollider>();
            lavaCollider.isTrigger = true;
            lava.AddComponent<LavaHazard>().Configure(bridgePositions);

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
#if UNITY_EDITOR
            var pillarAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Low Poly Trim Sheet Asset Collection/TrimSheet_Prefabs/Pillar.prefab");
            if (pillarAsset != null)
            {
                var imported = Object.Instantiate(pillarAsset);
                imported.name = "Arena Decoration - Stone Pillar";
                imported.transform.position = position;
                imported.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
                imported.transform.localScale = Vector3.one * 0.85f;
                NormalizeImportedMaterials(imported);
                foreach (var collider in imported.GetComponentsInChildren<Collider>()) Object.Destroy(collider);
                return;
            }
#endif
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

        private static GameObject CreateRockAsset(int index)
        {
#if UNITY_EDITOR
            var names = new[] { "Rock1A", "Rock1B", "Rock2", "Rock3", "Rock4A", "Rock5A", "Rock6A" };
            var path = $"Assets/Rocks and Boulders 2/Rocks/Prefabs/{names[index % names.Length]}.prefab";
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset != null) return Object.Instantiate(asset);
#endif
            return GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }

        private static void AddSolidRockCollider(GameObject rock)
        {
            var renderers = rock.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                var fallback = rock.AddComponent<SphereCollider>();
                fallback.radius = 0.35f;
                return;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var collider = rock.AddComponent<SphereCollider>();
            collider.center = rock.transform.InverseTransformPoint(bounds.center);
            var largestSize = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            var largestScale = Mathf.Max(0.001f, Mathf.Max(rock.transform.lossyScale.x,
                Mathf.Max(rock.transform.lossyScale.y, rock.transform.lossyScale.z)));
            collider.radius = largestSize / largestScale * 0.32f;
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
                    var normalized = new Material(shader) { name = $"{source?.name ?? "Environment"} - URP" };
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

        private static Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            return material;
        }
    }
}
