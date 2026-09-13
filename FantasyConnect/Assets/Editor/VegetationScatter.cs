using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Rebuilds terrain vegetation: grass/flowers as GPU-instanced details, bushes/trees/rocks as sparse tree instances.
public static class VegetationScatter {
    const string AZ = "Assets/開発用アセット/マップアセット/AZURE Nature/Prefabs/";
    const string HL = "Assets/開発用アセット/マップアセット/HIGHLANDS - Stylized Environment/Prefabs/Forest/";
    const string ST = "Assets/開発用アセット/マップアセット/Suntail Village/Prefabs/Nature/";
    const string WrapperFolder = "Assets/Settings/VegetationDetail";

    public class DetailDef {
        public string prefab;
        public int maxPerCell;
        public float minScale = 0.8f, maxScale = 1.25f;
        public float noiseScale = 0.02f;
        public float noiseMin;
        public int seed;
        public DetailDef (string prefab, int maxPerCell, float noiseMin, float noiseScale, int seed, float minScale = 0.8f, float maxScale = 1.25f) {
            this.prefab = prefab; this.maxPerCell = maxPerCell; this.noiseMin = noiseMin; this.noiseScale = noiseScale; this.seed = seed; this.minScale = minScale; this.maxScale = maxScale;
        }
    }

    public class ScatterDef {
        public string prefab;
        public float perThousandSqm;
        public float minScale, maxScale;
        public float spacing;
        public float clusterNoiseMin;
        public ScatterDef (string prefab, float perThousandSqm, float spacing, float minScale, float maxScale, float clusterNoiseMin = 0f) {
            this.prefab = prefab; this.perThousandSqm = perThousandSqm; this.spacing = spacing; this.minScale = minScale; this.maxScale = maxScale; this.clusterNoiseMin = clusterNoiseMin;
        }
    }

    static readonly string[] BareSoilLayers = { "Ground", "Path", "Mud", "Soil", "Dirt", "Sand", "Tile", "Rock", "NewLayer" };

    public class Biome {
        public string[] vegetatedLayers;
        public DetailDef[] details;
        public ScatterDef[] scatter;
        public float detailDistance = 80f;
        public float bossArenaRadius;
        // tree-painted plants to convert; hand-placed hero plants (e.g. forest m_grass) are left untouched
        public string[] convertTreeNames = { "AN_Grass", "Flower", "m_grass" };
    }

    public static Biome Summer (bool town, float arenaRadius = 0f) {
        return new Biome {
            vegetatedLayers = new[] { "Grass" },
            bossArenaRadius = arenaRadius,
            detailDistance = town ? 40f : 45f,
            details = new[] {
                // base grass has no noise cut-off so meadows have no bald circles; accents below stay patchy
                new DetailDef(AZ + "AN_Grass_1 2.prefab", town ? 2 : 3, 0f, 0.015f, 11),
                new DetailDef(ST + "Grass_1.prefab", town ? 1 : 2, 0.3f, 0.025f, 23),
                new DetailDef(HL + "Clovers.prefab", 1, 0.6f, 0.05f, 37),
                new DetailDef(ST + "Flowers_Yellow.prefab", 1, 0.74f, 0.04f, 41),
                new DetailDef(ST + "Flowers_Purple.prefab", 1, 0.77f, 0.045f, 53),
                new DetailDef(AZ + "AN_Flowers_Blue.prefab", 1, 0.85f, 0.05f, 67),
            },
            scatter = town ? new[] {
                new ScatterDef(ST + "Bush_2.prefab", 0.25f, 6f, 0.8f, 1.2f),
            } : new[] {
                new ScatterDef(ST + "Bush_2.prefab", 0.9f, 5f, 0.8f, 1.3f, 0.35f),
                new ScatterDef(AZ + "AN_Bush_3.prefab", 0.4f, 6f, 0.8f, 1.2f, 0.45f),
                new ScatterDef(HL + "Stone_1(Grass).prefab", 0.25f, 8f, 0.7f, 1.4f),
                new ScatterDef(HL + "Stone_3(Grass).prefab", 0.25f, 8f, 0.7f, 1.4f),
                new ScatterDef(AZ + "AN_Broadleaf_1_Green.prefab", 0.05f, 25f, 0.8f, 1.2f, 0.5f),
                new ScatterDef(AZ + "AN_Broadleaf_2_Green.prefab", 0.04f, 25f, 0.8f, 1.2f, 0.5f),
            }
        };
    }

    public static Biome Forest () {
        return new Biome {
            vegetatedLayers = new[] { "Grass" },
            detailDistance = 45f,
            convertTreeNames = new[] { "AN_Grass", "Flower" },
            details = new[] {
                new DetailDef(AZ + "AN_Grass_1 2.prefab", 2, 0.2f, 0.02f, 11),
                new DetailDef(ST + "Grass_2.prefab", 1, 0.45f, 0.03f, 29, 0.9f, 1.3f),
                new DetailDef(AZ + "AN_Flowers_Blue.prefab", 1, 0.8f, 0.05f, 67),
                new DetailDef(HL + "Mushrooms_1.prefab", 1, 0.85f, 0.06f, 43),
            },
            scatter = new[] {
                new ScatterDef(ST + "Bush_2.prefab", 0.8f, 5f, 0.9f, 1.4f, 0.3f),
                new ScatterDef(HL + "Rock_1(Grass).prefab", 0.12f, 12f, 0.8f, 1.5f),
            }
        };
    }

    public static Biome Autumn (float arenaRadius = 0f) {
        return new Biome {
            vegetatedLayers = new[] { "Grass", "Leaves" },
            bossArenaRadius = arenaRadius,
            detailDistance = 45f,
            details = new[] {
                new DetailDef(AZ + "AN_Grass_1.prefab", 3, 0.1f, 0.015f, 11),
                new DetailDef(WrapperFolder + "/AN_Grass_2_Autumn_Detail.prefab", 2, 0.35f, 0.03f, 19),
                new DetailDef(HL + "Mushrooms_1.prefab", 1, 0.82f, 0.06f, 43),
                new DetailDef(AZ + "AN_Flowers.prefab", 1, 0.8f, 0.05f, 61),
            },
            scatter = new[] {
                new ScatterDef(AZ + "AN_Dead_Bush.prefab", 0.5f, 6f, 0.8f, 1.3f, 0.3f),
                new ScatterDef(ST + "Bush_2.prefab", 0.25f, 6f, 0.8f, 1.2f, 0.45f),
                new ScatterDef(HL + "Stone_2(Grass).prefab", 0.2f, 8f, 0.7f, 1.4f),
                new ScatterDef(AZ + "AN_Broadleaf_5.prefab", 0.03f, 25f, 0.8f, 1.2f, 0.55f),
            }
        };
    }

    public static Biome Snow (float arenaRadius = 0f) {
        return new Biome {
            vegetatedLayers = new[] { "Grass" },
            bossArenaRadius = arenaRadius,
            detailDistance = 45f,
            details = new[] {
                new DetailDef(AZ + "AN_Grass_1 1.prefab", 2, 0.55f, 0.02f, 11, 0.6f, 1.0f),
            },
            scatter = new[] {
                new ScatterDef(AZ + "AN_Dead_Bush.prefab", 0.12f, 10f, 0.7f, 1.1f, 0.4f),
                new ScatterDef(AZ + "AN_DeadTree_1.prefab", 0.015f, 30f, 0.8f, 1.2f, 0.5f),
            }
        };
    }

    public static Biome Wasteland (float arenaRadius = 0f) {
        return new Biome {
            vegetatedLayers = new[] { "Grass" },
            bossArenaRadius = arenaRadius,
            detailDistance = 55f,
            details = new[] {
                // wasteland ground layers cover almost everything, so rely on noise for sparse tufts
                new DetailDef(AZ + "AN_Grass_1.prefab", 1, 0.52f, 0.025f, 11, 0.6f, 1.0f),
                new DetailDef(AZ + "AN_Grass_2.prefab", 1, 0.7f, 0.035f, 19, 0.6f, 0.9f),
            },
            scatter = new[] {
                new ScatterDef(AZ + "AN_Dead_Bush.prefab", 0.3f, 8f, 0.7f, 1.3f, 0.35f),
                new ScatterDef(HL + "Dead_Bush_1.prefab", 0.2f, 8f, 0.7f, 1.2f, 0.4f),
                new ScatterDef(AZ + "AN_DeadTree_2.prefab", 0.02f, 30f, 0.8f, 1.2f, 0.55f),
            }
        };
    }

    // Boss arenas: keep ground cover but no bushes/trees/rocks that could block combat
    public static Biome DetailsOnly (Biome biome) {
        biome.scatter = new ScatterDef[0];
        return biome;
    }

    public static string Apply (Terrain terrain, Biome biome) {
        var td = terrain.terrainData;
        Undo.RegisterCompleteObjectUndo(td, "Vegetation Scatter");
        Physics.SyncTransforms();
        var rng = new System.Random(terrain.name.GetHashCode() ^ td.name.GetHashCode());
        Vector3 origin = terrain.transform.position;
        Vector3 size = td.size;

        // 1. drop grass/flowers that were painted as trees; keep real trees
        var prototypes = new List<TreePrototype>(td.treePrototypes);
        var scatterPrefabs = new HashSet<GameObject>();
        foreach (var s in biome.scatter) { var p = AssetDatabase.LoadAssetAtPath<GameObject>(s.prefab); if (p != null) scatterPrefabs.Add(p); }
        var kept = new List<TreeInstance>();
        int removed = 0;
        foreach (var inst in td.treeInstances) {
            var proto = prototypes[inst.prototypeIndex].prefab;
            bool grassLike = proto != null && System.Array.Exists(biome.convertTreeNames, n => proto.name.Contains(n));
            // HIGHLANDS bushes were an earlier palette choice that clashed with the AZURE terrains
            bool retiredPalette = proto != null && AssetDatabase.GetAssetPath(proto).StartsWith(HL + "Bush_");
            // previously scattered instances are regenerated so re-runs stay idempotent
            if (grassLike || retiredPalette || (proto != null && scatterPrefabs.Contains(proto))) removed++; else kept.Add(inst);
        }

        // 2. vegetation weight from splat layers
        int aRes = td.alphamapResolution;
        float[,,] alpha = td.GetAlphamaps(0, 0, aRes, aRes);
        var layers = td.terrainLayers;
        var vegLayer = new bool[layers.Length];
        var bareLayer = new bool[layers.Length];
        for (int i = 0; i < layers.Length; i++) {
            string n = layers[i] != null ? layers[i].name : "";
            foreach (var k in biome.vegetatedLayers) if (n.Contains(k)) vegLayer[i] = true;
            foreach (var k in BareSoilLayers) if (n.Contains(k)) { bareLayer[i] = true; vegLayer[i] = false; }
        }
        System.Func<float, float, float> VegWeight = (u, v) => {
            int ax = Mathf.Clamp((int)(u * (aRes - 1)), 0, aRes - 1);
            int ay = Mathf.Clamp((int)(v * (aRes - 1)), 0, aRes - 1);
            float w = 0f, bare = 0f;
            for (int i = 0; i < vegLayer.Length; i++) {
                if (vegLayer[i]) w += alpha[ay, ax, i];
                else if (bareLayer[i]) bare += alpha[ay, ax, i];
            }
            // keep plants off soil, including soft grass/soil blend edges
            return bare > 0.25f ? 0f : w;
        };

        // 3. coarse blocked grid from scene colliders (buildings, props, water)
        const float cell = 2f;
        int gx = Mathf.CeilToInt(size.x / cell), gz = Mathf.CeilToInt(size.z / cell);
        var blocked = new bool[gz, gx];
        var hits = new Collider[8];
        for (int z = 0; z < gz; z++)
            for (int x = 0; x < gx; x++) {
                float wx = origin.x + (x + 0.5f) * cell, wz = origin.z + (z + 0.5f) * cell;
                float wy = terrain.SampleHeight(new Vector3(wx, 0, wz)) + origin.y;
                int n = Physics.OverlapBoxNonAlloc(new Vector3(wx, wy + 1.5f, wz), new Vector3(cell * 0.5f, 1.4f, cell * 0.5f), hits, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
                for (int h = 0; h < n; h++) {
                    if (hits[h] is TerrainCollider) continue;
                    // post-process volume boxes and other huge area colliders are not obstacles
                    if (hits[h].GetComponent<UnityEngine.Rendering.Volume>() != null) continue;
                    var hb = hits[h].bounds.size;
                    if (hb.x > 150f && hb.z > 150f) continue;
                    blocked[z, x] = true; break;
                }
            }
        Vector3 arenaCenter = origin + new Vector3(size.x * 0.5f, 0, size.z * 0.5f);
        System.Func<float, float, bool> IsBlocked = (u, v) => {
            int bx = Mathf.Clamp((int)(u * gx), 0, gx - 1), bz = Mathf.Clamp((int)(v * gz), 0, gz - 1);
            if (blocked[bz, bx]) return true;
            if (biome.bossArenaRadius > 0f) {
                float dx = origin.x + u * size.x - arenaCenter.x, dz = origin.z + v * size.z - arenaCenter.z;
                if (dx * dx + dz * dz < biome.bossArenaRadius * biome.bossArenaRadius) return true;
            }
            return false;
        };

        // 4. details
        int dRes = Mathf.Clamp(Mathf.NextPowerOfTwo(Mathf.RoundToInt(size.x)), 256, 1024);
        td.SetDetailResolution(dRes, 32);
        if (!AssetDatabase.IsValidFolder(WrapperFolder)) AssetDatabase.CreateFolder("Assets/Settings", "VegetationDetail");
        var detailProtos = new List<DetailPrototype>();
        var detailDefs = new List<DetailDef>();
        foreach (var d in biome.details) {
            var wrapper = GetDetailWrapper(d.prefab);
            if (wrapper == null) continue;
            detailProtos.Add(new DetailPrototype {
                usePrototypeMesh = true,
                prototype = wrapper,
                renderMode = DetailRenderMode.VertexLit,
                useInstancing = true,
                minWidth = d.minScale, maxWidth = d.maxScale,
                minHeight = d.minScale, maxHeight = d.maxScale,
                noiseSpread = 0.4f,
                healthyColor = Color.white, dryColor = Color.white
            });
            detailDefs.Add(d);
        }
        td.detailPrototypes = detailProtos.ToArray();
        long detailTotal = 0;
        for (int i = 0; i < detailDefs.Count; i++) {
            var d = detailDefs[i];
            var map = new int[dRes, dRes];
            for (int y = 0; y < dRes; y++)
                for (int x = 0; x < dRes; x++) {
                    float u = (x + 0.5f) / dRes, v = (y + 0.5f) / dRes;
                    float w = VegWeight(u, v);
                    if (w < 0.5f) continue;
                    if (td.GetSteepness(u, v) > 35f) continue;
                    if (IsBlocked(u, v)) continue;
                    float n = Mathf.PerlinNoise(x * d.noiseScale + d.seed * 13.7f, y * d.noiseScale + d.seed * 7.3f);
                    if (n < d.noiseMin) continue;
                    float t = Mathf.InverseLerp(d.noiseMin, 1f, n);
                    int count = Mathf.RoundToInt(d.maxPerCell * w * Mathf.Lerp(0.5f, 1f, t) + (float)rng.NextDouble() * 0.6f - 0.3f);
                    if (count <= 0) continue;
                    map[y, x] = count;
                    detailTotal += count;
                }
            td.SetDetailLayer(0, 0, i, map);
        }

        // 5. sparse bushes / trees / rocks
        var placed = new List<Vector2>();
        foreach (var inst in kept) placed.Add(new Vector2(inst.position.x * size.x, inst.position.z * size.z));
        int added = 0;
        float area = size.x * size.z;
        foreach (var s in biome.scatter) {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(s.prefab);
            if (prefab == null) continue;
            int protoIndex = prototypes.FindIndex(p => p.prefab == prefab);
            if (protoIndex < 0) { prototypes.Add(new TreePrototype { prefab = prefab, bendFactor = 0f }); protoIndex = prototypes.Count - 1; EnableInstancing(prefab); }
            int target = Mathf.RoundToInt(area / 1000f * s.perThousandSqm);
            int attempts = target * 6, got = 0;
            float noiseSeed = s.prefab.GetHashCode() % 1000;
            for (int a = 0; a < attempts && got < target; a++) {
                float u = (float)rng.NextDouble(), v = (float)rng.NextDouble();
                float w = VegWeight(u, v);
                if (w < 0.55f || td.GetSteepness(u, v) > 25f) continue;
                if (s.clusterNoiseMin > 0f && Mathf.PerlinNoise(u * 12f + noiseSeed, v * 12f + noiseSeed) < s.clusterNoiseMin) continue;
                if (IsBlocked(u, v)) continue;
                var p2 = new Vector2(u * size.x, v * size.z);
                bool tooClose = false;
                foreach (var q in placed) if ((q - p2).sqrMagnitude < s.spacing * s.spacing) { tooClose = true; break; }
                if (tooClose) continue;
                float sc = Mathf.Lerp(s.minScale, s.maxScale, (float)rng.NextDouble());
                kept.Add(new TreeInstance {
                    position = new Vector3(u, td.GetInterpolatedHeight(u, v) / size.y, v),
                    prototypeIndex = protoIndex,
                    widthScale = sc, heightScale = sc,
                    rotation = (float)(rng.NextDouble() * Mathf.PI * 2),
                    color = Color.white, lightmapColor = Color.white
                });
                placed.Add(p2);
                got++;
            }
            added += got;
        }
        td.treePrototypes = prototypes.ToArray();
        td.SetTreeInstances(kept.ToArray(), true);

        terrain.detailObjectDistance = biome.detailDistance;
        terrain.detailObjectDensity = 1f;
        terrain.treeDistance = Mathf.Max(terrain.treeDistance, 120f);
        terrain.treeBillboardDistance = Mathf.Max(terrain.treeBillboardDistance, 40f);
        terrain.drawInstanced = true;
        EditorUtility.SetDirty(td);
        EditorUtility.SetDirty(terrain);
        return $"{terrain.name}: removed {removed} grass-trees, details {detailTotal} ({detailProtos.Count} kinds, res {dRes}), scattered {added}, trees now {kept.Count}";
    }

    // Adds ground cover to wherever a specific terrain texture is painted, keeping the terrain's other detail layers.
    // Useful when a biome's ground layer is not named "Grass" (autumn leaf litter, recoloured meadow textures).
    public static string AddDetailsForTexture (Terrain terrain, string diffuseTextureName, params DetailDef[] defs) {
        var td = terrain.terrainData;
        Undo.RegisterCompleteObjectUndo(td, "Texture Details");
        var layers = td.terrainLayers;
        var match = new bool[layers.Length];
        bool any = false;
        for (int i = 0; i < layers.Length; i++)
            if (layers[i] != null && layers[i].diffuseTexture != null && layers[i].diffuseTexture.name == diffuseTextureName) { match[i] = true; any = true; }
        if (!any) return $"{terrain.name}: no layer uses {diffuseTextureName}";

        int aRes = td.alphamapResolution;
        var alpha = td.GetAlphamaps(0, 0, aRes, aRes);
        int dRes = td.detailResolution;
        if (dRes < 256) { dRes = Mathf.Clamp(Mathf.NextPowerOfTwo(Mathf.RoundToInt(td.size.x)), 256, 1024); td.SetDetailResolution(dRes, 32); }
        var rng = new System.Random(terrain.name.GetHashCode() ^ diffuseTextureName.GetHashCode());
        var protos = new List<DetailPrototype>(td.detailPrototypes);
        long total = 0;
        foreach (var d in defs) {
            var wrapper = GetDetailWrapper(d.prefab);
            if (wrapper == null) continue;
            int index = protos.FindIndex(p => p.prototype == wrapper);
            if (index < 0) {
                protos.Add(new DetailPrototype {
                    usePrototypeMesh = true, prototype = wrapper, renderMode = DetailRenderMode.VertexLit, useInstancing = true,
                    minWidth = d.minScale, maxWidth = d.maxScale, minHeight = d.minScale, maxHeight = d.maxScale,
                    noiseSpread = 0.4f, healthyColor = Color.white, dryColor = Color.white
                });
                td.detailPrototypes = protos.ToArray();
                index = protos.Count - 1;
            }
            var map = td.GetDetailLayer(0, 0, dRes, dRes, index);
            for (int y = 0; y < dRes; y++)
                for (int x = 0; x < dRes; x++) {
                    float u = (x + 0.5f) / dRes, v = (y + 0.5f) / dRes;
                    int ax = Mathf.Clamp((int)(u * (aRes - 1)), 0, aRes - 1), ay = Mathf.Clamp((int)(v * (aRes - 1)), 0, aRes - 1);
                    float w = 0f;
                    for (int l = 0; l < match.Length; l++) if (match[l]) w += alpha[ay, ax, l];
                    if (w < 0.5f || td.GetSteepness(u, v) > 35f) continue;
                    float n = Mathf.PerlinNoise(x * d.noiseScale + d.seed * 13.7f, y * d.noiseScale + d.seed * 7.3f);
                    if (n < d.noiseMin) continue;
                    // at least one blade per covered cell so the ground layer never shows bald spots
                    int count = Mathf.Max(1, Mathf.RoundToInt(d.maxPerCell * w * Mathf.Lerp(0.5f, 1f, Mathf.InverseLerp(d.noiseMin, 1f, n)) + (float)rng.NextDouble() * 0.6f - 0.3f));
                    if (count <= map[y, x]) continue;
                    total += count - map[y, x];
                    map[y, x] = count;
                }
            td.SetDetailLayer(0, 0, index, map);
        }
        terrain.detailObjectDistance = Mathf.Max(terrain.detailObjectDistance, 45f);
        EditorUtility.SetDirty(td);
        return $"{terrain.name}: +{total} details on {diffuseTextureName}";
    }

    static GameObject GetDetailWrapper (string prefabPath) {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return null;
        // hand-made recoloured wrappers (e.g. autumn grass) are used as-is
        if (prefabPath.StartsWith(WrapperFolder)) return prefab;
        string wrapperPath = $"{WrapperFolder}/{prefab.name}_Detail.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(wrapperPath);
        if (existing != null) {
            var er = existing.GetComponent<MeshRenderer>();
            foreach (var m in er.sharedMaterials) {
                if (m == null) continue;
                m.enableInstancing = true;
                m.SetShaderPassEnabled("ShadowCaster", false);
                EditorUtility.SetDirty(m);
            }
            return existing;
        }

        MeshFilter mf = null;
        var lod = prefab.GetComponent<LODGroup>();
        if (lod != null && lod.GetLODs().Length > 0) {
            foreach (var r in lod.GetLODs()[0].renderers) { if (r != null) { mf = r.GetComponent<MeshFilter>(); if (mf != null) break; } }
        }
        if (mf == null) mf = prefab.GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return null;
        var mr = mf.GetComponent<MeshRenderer>();

        var go = new GameObject(prefab.name + "_Detail");
        go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
        var newMr = go.AddComponent<MeshRenderer>();
        newMr.sharedMaterials = mr.sharedMaterials;
        newMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        foreach (var m in mr.sharedMaterials) {
            if (m == null) continue;
            m.enableInstancing = true;
            // ground cover shadows cost one draw per cascade and are barely visible
            m.SetShaderPassEnabled("ShadowCaster", false);
            EditorUtility.SetDirty(m);
        }
        var saved = PrefabUtility.SaveAsPrefabAsset(go, wrapperPath);
        Object.DestroyImmediate(go);
        return saved;
    }

    static void EnableInstancing (GameObject prefab) {
        foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
            foreach (var m in r.sharedMaterials)
                if (m != null && !m.enableInstancing) { m.enableInstancing = true; EditorUtility.SetDirty(m); }
    }
}
