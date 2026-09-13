using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Dresses a scene with the SoStylized / ToonScapes packs:
//  1. biome-coloured vegetation clusters (terrain tree instances, GPU instanced)
//  2. landmarks just outside the playable boundary (visible targets on the horizon)
//  3. props along path edges (lamps, signs, benches)
//  4. ponds in hollows with lilies and reeds
//  5. distant background mountains
//  6. hero set pieces along a timeline camera flight (title screen)
//  7. ambient life: fish in ponds, drifting leaves/petals near set pieces
// Biome is read per location from the terrain splat layers, so mixed terrains (title) get matching dressing.
// Re-running replaces the previous decoration.
public static class MapDecorator {
    const string SS = "Assets/SoStylized/Environment/";
    const string TS = "Assets/ToonScapes/Spring Isles/";
    const string RootName = "Map Decor";

    public enum Biome { Summer, Forest, Autumn, Snow, Wasteland, Blossom }

    class Palette {
        public string[] trees, bushes, flowers, rocks, logs, landmarks, pathProps, pondPlants, ambient;
    }

    static Palette Get (Biome b) {
        switch (b) {
            case Biome.Forest: return new Palette {
                trees = Paths(SS + "Trees/Fir/Prefabs/P_", "FirTree1", "FirTree2", "FirTree3") .Concat(Paths(SS + "Trees/Pine/Prefabs/P_", "Pine01", "Pine02")),
                bushes = Paths(SS + "Foliage/Prefabs/P_", "Fern1", "Fern2", "Fern3", "RedFern01", "Bush2"),
                flowers = Paths(SS + "Foliage/Prefabs/P_", "FlowerCrocus01", "Foxtails1"),
                rocks = Paths(SS + "Rocks/Classic/Prefabs/P_", "RockClumpClassic7", "RockClumpClassic9", "RockClassic11", "BoulderClassic1"),
                logs = Paths(SS + "Trees/Fir/Prefabs/P_", "FirFallen1", "FirStump1"),
                landmarks = Paths(SS + "Trees/Fir/Prefabs/P_", "FirTree4").Concat(Paths(SS + "Rocks/Spire/Prefabs/P_", "RockSpire_RockClumpC01")).Concat(Paths(TS + "Prefabs/Presets/Structures/TSI_", "Preset_Stone_Shrine_02A")),
                pathProps = Paths(TS + "Prefabs/Props/Ornamental Props/TSI_", "Stone_Lantern_01A_Rope").Concat(Paths(TS + "Prefabs/Props/Wood Props/TSI_", "Sign_Post_02A")),
                pondPlants = Paths(SS + "Foliage/Prefabs/P_", "Marshtail01", "Marshtail02", "LilyPadCluster1", "LilyPadCluster2"),
                ambient = Paths(TS + "Particles/TSI_", "Sun_Shaft_01A") };
            case Biome.Autumn: return new Palette {
                trees = Paths(SS + "Trees/Oak/Prefabs/P_", "OakTree1_Red", "OakTree2_Red", "OakTree4_Red").Concat(Paths(TS + "Prefabs/Vegetation/Trees/TSI_", "Amberleaf_Tree_02A", "Amberleaf_Tree_03A")),
                bushes = Paths(SS + "Foliage/Prefabs/P_", "Bush1_Dry", "Bush2_Dry", "Fern1_Yellow", "Fern2_Yellow").Concat(Paths(TS + "Prefabs/Vegetation/Plants & Flowers/TSI_", "Amberleaf_Bush_01A")),
                flowers = Paths(SS + "Foliage/Prefabs/P_", "FoxtailsLight1", "TulipsOrange"),
                rocks = Paths(SS + "Rocks/Classic/Prefabs/P_", "RockClumpClassic8", "RockClassic12", "BoulderClassic2"),
                logs = Paths(SS + "Trees/Oak/Prefabs/P_", "OakFallen2", "OakStump1"),
                landmarks = Paths(TS + "Prefabs/Presets/Structures/TSI_", "Preset_Bell_Tower_01A", "Preset_Temple_01A").Concat(Paths(TS + "Prefabs/Building Props/Torii Gate/TSI_", "Torii_Gate_01A")).Concat(Paths(SS + "Trees/Oak/Prefabs/P_", "OakTree3_Red")),
                pathProps = Paths(TS + "Prefabs/Props/Wood Props/TSI_", "Lamp_Post_01A", "Sign_Post_01A", "Wood_Bench_02A"),
                pondPlants = Paths(SS + "Foliage/Prefabs/P_", "Marshtail03", "LilyPadCluster1"),
                ambient = Paths(TS + "Particles/TSI_", "Blowing_Leaves_01A") };
            case Biome.Snow: return new Palette {
                trees = Paths(SS + "Trees/Pine/Prefabs/P_", "Pine01_Snowy", "Pine02_Snowy", "Pine03_Snowy", "PineSnowCover01", "PineSnowCover02"),
                bushes = Paths(SS + "Foliage/Prefabs/P_", "Bush1_Snowy", "Bush2_Snowy", "BushSnowDead01").Concat(Paths(SS + "Misc/Prefabs/P_", "SnowDrift01", "SnowDrift02", "SnowDrift04")),
                flowers = Paths(SS + "Foliage/Prefabs/P_", "FlowersIce01"),
                rocks = Paths(SS + "Rocks/Spire/Prefabs/P_", "RockSpire_RockClump03", "RockSpire_Rock05", "RockSpire_Rock08"),
                logs = Paths(SS + "Trees/Pine/Prefabs/P_", "PineFallen2_SnowCover", "PineStump1_SnowCover", "PineDead02_Snowy"),
                landmarks = Paths(SS + "Rocks/Spire/Prefabs/P_", "RockSpire_RockClumpB10", "RockSpire_RockClumpC03").Concat(Paths(SS + "Trees/Pine/Prefabs/P_", "PineDead03_Snowy")),
                pathProps = Paths(TS + "Prefabs/Props/Ornamental Props/TSI_", "Stone_Lantern_01A_Rope").Concat(Paths(TS + "Prefabs/Props/Wood Props/TSI_", "Sign_Post_02A")),
                pondPlants = new string[0], ambient = new string[0] };
            case Biome.Wasteland: return new Palette {
                trees = Paths(SS + "Trees/Desert/Prefabs/P_", "TreeWindswept01", "TreeWindswept02", "TreeWindsweptB03").Concat(Paths(SS + "Trees/Pine/Prefabs/P_", "PineDead01")),
                bushes = Paths(SS + "Foliage/Prefabs/P_", "DesertBush01", "DesertBush02", "DesertBushB01", "Tumbleweed01", "DesertTwigRoots01"),
                flowers = Paths(SS + "Foliage/Prefabs/P_", "FlowerDesertBulb01", "CactusPricklyPear01"),
                rocks = Paths(SS + "Rocks/Desert/Prefabs/P_", "RockDesert_Rock12", "RockDesert_Rock16", "RockDesert_Clump08", "RockDesert_Clump10"),
                logs = Paths(SS + "Rocks/Desert/Prefabs/P_", "RockDesert_Shelf01"),
                landmarks = Paths(SS + "Rocks/Desert/Prefabs/P_", "RockDesert_Hoodoo02", "RockDesert_Hoodoo05", "RockDesert_Hoodoo09", "RockDesert_Layered01", "RockDesert_Platform02"),
                pathProps = Paths(TS + "Prefabs/Props/Wood Props/TSI_", "Wood_Cart_01A", "Sign_Post_01A", "Wood_Fence_03A"),
                pondPlants = new string[0], ambient = new string[0] };
            case Biome.Blossom: return new Palette {
                trees = Paths(TS + "Prefabs/Vegetation/Trees/TSI_", "Blossom_Tree_01A", "Blossom_Tree_02A", "Blossom_Tree_03A"),
                bushes = Paths(TS + "Prefabs/Vegetation/Plants & Flowers/TSI_", "Blossom_Bush_01A", "Flower_Bush_01A", "Flower_Bush_02A").Concat(Paths(SS + "Foliage/Prefabs/P_", "FlowerBush02")),
                flowers = Paths(SS + "Foliage/Prefabs/P_", "TulipsPink", "Allium"),
                rocks = Paths(SS + "Rocks/Hexic/Prefabs/P_", "RockHexic_Piece02", "RockHexic_Piece06"),
                logs = new string[0],
                landmarks = Paths(SS + "Rocks/Hexic/Prefabs/P_", "RockHexic_Spire01", "RockHexic_Rocks03").Concat(Paths(TS + "Prefabs/Presets/Structures/TSI_", "Preset_Stone_Shrine_01A")),
                pathProps = Paths(TS + "Prefabs/Props/Ornamental Props/TSI_", "Stone_Lantern_01A_Rope"),
                pondPlants = Paths(TS + "Prefabs/Vegetation/Water Vegetation/TSI_", "Water_Lily_02A", "Water_Lily_Flower_01A"),
                ambient = Paths(TS + "Particles/TSI_", "Blowing_Petals_01A") };
            default: return new Palette {
                trees = Paths(SS + "Trees/Oak/Prefabs/P_", "OakTree1", "OakTree2", "OakTree4").Concat(Paths(SS + "Trees/Birch/Prefabs/P_", "BirchTree1", "BirchTreeB1")),
                bushes = Paths(SS + "Foliage/Prefabs/P_", "Bush1", "Bush2", "Bush1_Light", "FlowerBush01", "BushLeafy01"),
                flowers = Paths(SS + "Foliage/Prefabs/P_", "TulipsYellow", "TulipsRed", "FlowerCrocus02", "Foxtails1"),
                rocks = Paths(SS + "Rocks/Classic/Prefabs/P_", "RockClumpClassic7", "RockClumpClassic10", "RockClassic9", "BoulderClassic3"),
                logs = Paths(SS + "Trees/Oak/Prefabs/P_", "OakFallen1", "OakStump1", "OakRoots2"),
                landmarks = Paths(SS + "Trees/Oak/Prefabs/P_", "OakTreeLarge1").Concat(Paths(TS + "Prefabs/Presets/Structures/TSI_", "Preset_Gazebo_01A", "Preset_Stone_Shrine_01A")).Concat(Paths(SS + "Rocks/Hexic/Prefabs/P_", "RockHexic_Spire01")),
                pathProps = Paths(TS + "Prefabs/Props/Wood Props/TSI_", "Lamp_Post_01A", "Sign_Post_01A", "Wood_Bench_01A", "Wood_Cart_01A"),
                pondPlants = Paths(SS + "Foliage/Prefabs/P_", "Marshtail01", "Marshtail02", "LilyPadCluster1", "LilyPadCluster2").Concat(Paths(TS + "Prefabs/Vegetation/Water Vegetation/TSI_", "Water_Lily_Flower_01A")),
                ambient = Paths(TS + "Particles/TSI_", "Blowing_Petals_01A") };
        }
    }

    public class Options {
        public Biome defaultBiome = Biome.Summer;
        public bool bossArena;           // keep the playable area clear
        public bool town;                // no vegetation inside the playable area
        public float clusterDensity = 1f;
        public int landmarks = 5;
        public int pathProps = 10;
        public int ponds = 2;
        public bool backgroundMountains = true;
        public bool titleSetPieces;
        public bool innerGroves = true;  // small tree groups in open fields of normal maps
        public float regionPadding = 0f; // corridor maps: treat this much space beside the corridor as "inside"
        public int seed = 1;
    }

    // ---------------------------------------------------------------------------------------------------------
    public static string Decorate (Options o) {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var terrains = Object.FindObjectsOfType<Terrain>();
        if (terrains.Length == 0) return scene.name + ": no terrain";
        foreach (var r in scene.GetRootGameObjects()) if (r.name == RootName) Object.DestroyImmediate(r);
        RemovePreviousTreeInstances(terrains);
        Physics.SyncTransforms();

        var root = new GameObject(RootName);
        UnityEditor.SceneManagement.EditorSceneManager.MoveGameObjectToScene(root, scene);
        var rng = new System.Random(o.seed * 92821 + scene.name.GetHashCode());
        var ctx = new Context { scene = scene, terrains = terrains, rng = rng, root = root.transform, options = o };

        string src;
        ctx.playable = TerrainSculpt.ComputePlayable(out src);
        ctx.cameraPath = TitleCameraPath(scene);
        var report = new System.Text.StringBuilder($"{scene.name} [{src}] ");

        report.Append(Vegetation(ctx)).Append(" | ");
        report.Append(Landmarks(ctx)).Append(" | ");
        report.Append(PathProps(ctx)).Append(" | ");
        report.Append(Ponds(ctx)).Append(" | ");
        if (o.backgroundMountains) report.Append(Mountains(ctx)).Append(" | ");
        if (o.titleSetPieces) report.Append(TitleSetPieces(ctx)).Append(" | ");

        foreach (var t in root.GetComponentsInChildren<Transform>(true)) {
            if (t.GetComponent<ParticleSystem>() != null || t.GetComponentInParent<Animator>() != null) continue;
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic);
        }
        foreach (var t in terrains) { t.treeDistance = Mathf.Max(t.treeDistance, 400f); EditorUtility.SetDirty(t.terrainData); }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        report.Append($"objects {root.GetComponentsInChildren<Renderer>(true).Length} renderers");
        return report.ToString();
    }

    class Context {
        public UnityEngine.SceneManagement.Scene scene;
        public Terrain[] terrains;
        public System.Random rng;
        public Transform root;
        public Options options;
        public TerrainSculpt.Playable playable;
        public List<Vector3> cameraPath;
        public readonly List<Vector3> occupied = new List<Vector3>(); // big decor footprints (xz, radius in y)

        public float Rand (float a, float b) { return Mathf.Lerp(a, b, (float)rng.NextDouble()); }
        public T Pick<T> (IList<T> list) { return list[rng.Next(list.Count)]; }

        // negative = playable / keep clear, positive = free to decorate
        public float Sd (float x, float z) {
            float sd = playable != null ? playable.Sd(x, z) + options.regionPadding : 1000f;
            if (cameraPath != null && cameraPath.Count > 0) {
                float best = float.MaxValue;
                foreach (var p in cameraPath) { float dx = p.x - x, dz = p.z - z; best = Mathf.Min(best, dx * dx + dz * dz); }
                sd = Mathf.Min(sd, Mathf.Sqrt(best) - 12f);
            }
            return sd;
        }

        public bool Occupied (float x, float z, float radius) {
            foreach (var p in occupied) { float dx = p.x - x, dz = p.z - z, r = p.y + radius; if (dx * dx + dz * dz < r * r) return true; }
            return false;
        }
    }

    // ---------------------------------------------------------------------------------------------------------
    // 1. vegetation clusters
    static string Vegetation (Context c) {
        int trees = 0, small = 0;
        foreach (var t in c.terrains) {
            var td = t.terrainData; var o = t.transform.position; var size = td.size;
            var protos = new List<TreePrototype>(td.treePrototypes);
            var list = new List<TreeInstance>(td.treeInstances);
            float area = size.x * size.z / 10000f; // hectares
            int clusters = Mathf.RoundToInt(area * 1.6f * c.options.clusterDensity);
            for (int k = 0; k < clusters; k++) {
                float cu = (float)c.rng.NextDouble(), cv = (float)c.rng.NextDouble();
                float cx = o.x + cu * size.x, cz = o.z + cv * size.z;
                var biome = BiomeAt(t, cu, cv, c.options.defaultBiome);
                if (biome == null) continue;
                var pal = Get(biome.Value);
                float sd = c.Sd(cx, cz);
                bool inside = sd < 0f;
                if (inside && (c.options.bossArena || c.options.town) && sd < -6f) continue;

                // cluster type: groves only outside or deep in open fields, otherwise understory patches
                int kind = c.rng.Next(10);
                bool groveAllowed = c.options.innerGroves && !c.options.bossArena && !c.options.town && sd < -20f;
                if (inside && kind < 4 && !groveAllowed) kind = 4 + c.rng.Next(6);
                string[] pool; int count; float radius; Vector2 scale; float spacing;
                if (kind < 4) { pool = pal.trees; count = 3 + c.rng.Next(5); radius = 18f; scale = new Vector2(0.8f, 1.3f); spacing = 6f; }
                else if (kind < 7) { pool = pal.bushes; count = 4 + c.rng.Next(6); radius = 7f; scale = new Vector2(0.7f, 1.3f); spacing = 2.2f; }
                else if (kind < 9) { pool = pal.flowers; count = 6 + c.rng.Next(8); radius = 5f; scale = new Vector2(0.8f, 1.4f); spacing = 1.2f; }
                else { pool = pal.rocks.Concat(pal.logs); count = 2 + c.rng.Next(3); radius = 8f; scale = new Vector2(0.6f, 1.4f); spacing = 3f; }
                if (pool.Length == 0) continue;

                for (int i = 0; i < count; i++) {
                    float a = (float)(c.rng.NextDouble() * Mathf.PI * 2), d = radius * Mathf.Sqrt((float)c.rng.NextDouble());
                    float x = cx + Mathf.Cos(a) * d, z = cz + Mathf.Sin(a) * d;
                    float u = (x - o.x) / size.x, v = (z - o.z) / size.z;
                    if (u < 0 || u > 1 || v < 0 || v > 1) continue;
                    if (td.GetSteepness(u, v) > (kind < 4 ? 26f : 34f) || BareAt(t, u, v) > 0.35f) continue;
                    float psd = c.Sd(x, z);
                    if (psd < 0f && (c.options.bossArena || c.options.town)) continue;
                    if (psd < 0f && kind < 4 && (!groveAllowed || psd > -12f)) continue; // groves only in open fields, away from edges
                    if (psd < 0f && Blocked(t, u, v, kind < 4 ? 3f : 1.5f)) continue;
                    if (psd < 0f && kind < 4 && BareAt(t, u, v) > 0.05f) continue; // keep trees off paths
                    if (psd > -2f && psd < 3f) continue; // keep wall lines clean
                    var prefab = LoadTreeCompatible(c.Pick(pool));
                    if (prefab == null) continue;
                    if (Crowded(list, td, u, v, spacing)) continue;
                    int idx = protos.FindIndex(p => p.prefab == prefab);
                    if (idx < 0) { protos.Add(new TreePrototype { prefab = prefab, bendFactor = 0f }); idx = protos.Count - 1; EnableInstancing(prefab); }
                    float sc = c.Rand(scale.x, scale.y);
                    list.Add(new TreeInstance { position = new Vector3(u, td.GetInterpolatedHeight(u, v) / size.y, v), prototypeIndex = idx,
                        widthScale = sc, heightScale = sc * c.Rand(0.9f, 1.1f), rotation = c.Rand(0f, Mathf.PI * 2f), color = Color.white, lightmapColor = Color.white });
                    if (kind < 4) trees++; else small++;
                }
            }
            td.treePrototypes = protos.ToArray();
            td.SetTreeInstances(list.ToArray(), true);
        }
        return $"vegetation trees +{trees}, understory +{small}";
    }

    // Dresses the edge of the play space (invisible wall lines, boss arena rims) with rock clusters, logs and bushes,
    // so the boundary reads as a natural edge instead of open ground. Adds to an existing decoration without redoing it.
    public static string DecorateRim (Options o, float spacing = 22f) {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var terrains = Object.FindObjectsOfType<Terrain>();
        GameObject rootGo = null;
        foreach (var r in scene.GetRootGameObjects()) if (r.name == RootName) rootGo = r;
        if (rootGo == null) { rootGo = new GameObject(RootName); UnityEditor.SceneManagement.EditorSceneManager.MoveGameObjectToScene(rootGo, scene); }
        var old = rootGo.transform.Find("Rim");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        Physics.SyncTransforms();
        var c = new Context { scene = scene, terrains = terrains, rng = new System.Random(o.seed * 7727 + 11), root = rootGo.transform, options = o };
        string src;
        c.playable = TerrainSculpt.ComputePlayable(out src);
        c.cameraPath = TitleCameraPath(scene);
        if (c.playable == null) return scene.name + ": no playable region";

        var spots = new List<Vector2>(); int clusters = 0, pieces = 0;
        for (int attempt = 0; attempt < 20000 && clusters < 60; attempt++) {
            var t = c.Pick(terrains); var td = t.terrainData; var to = t.transform.position;
            float u = c.Rand(0.02f, 0.98f), v = c.Rand(0.02f, 0.98f);
            float x = to.x + u * td.size.x, z = to.z + v * td.size.z;
            float sd = c.Sd(x, z);
            if (sd < 3f || sd > 12f || td.GetSteepness(u, v) > 30f) continue;
            bool near = false; foreach (var s in spots) if ((s - new Vector2(x, z)).sqrMagnitude < spacing * spacing) { near = true; break; }
            if (near) continue;
            var biome = BiomeAt(t, u, v, o.defaultBiome);
            if (biome == null) continue;
            var pal = Get(biome.Value);
            var big = pal.rocks.Concat(pal.logs);
            if (big.Length == 0) continue;
            spots.Add(new Vector2(x, z));
            int n = 2 + c.rng.Next(3);
            for (int i = 0; i < n; i++) {
                float a = c.Rand(0f, Mathf.PI * 2f), d = c.Rand(0f, 5f);
                float px = x + Mathf.Cos(a) * d, pz = z + Mathf.Sin(a) * d;
                if (c.Sd(px, pz) < 2f) continue; // never inside the play space
                string path = i == 0 ? c.Pick(big) : (c.rng.NextDouble() < 0.6 && pal.bushes.Length > 0 ? c.Pick(pal.bushes) : c.Pick(big));
                var go = Place(c, path, t, px, pz, c.Rand(0f, 360f), i == 0 ? c.Rand(1.2f, 2.2f) : c.Rand(0.8f, 1.3f), "Rim");
                if (go != null) pieces++;
            }
            clusters++;
        }
        foreach (var tr in Group(c, "Rim").GetComponentsInChildren<Transform>(true))
            GameObjectUtility.SetStaticEditorFlags(tr.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        return $"{scene.name} rim clusters {clusters}, pieces {pieces}";
    }

    // 2. landmarks just outside the play space, on open visible ground
    static string Landmarks (Context c) {
        int placed = 0;
        for (int attempt = 0; attempt < c.options.landmarks * 60 && placed < c.options.landmarks; attempt++) {
            var t = c.Pick(c.terrains); var td = t.terrainData; var o = t.transform.position;
            float u = c.Rand(0.05f, 0.95f), v = c.Rand(0.05f, 0.95f);
            float x = o.x + u * td.size.x, z = o.z + v * td.size.z;
            float sd = c.Sd(x, z);
            if (sd < 18f || sd > 90f) continue;
            if (td.GetSteepness(u, v) > 18f || c.Occupied(x, z, 70f)) continue;
            var biome = BiomeAt(t, u, v, c.options.defaultBiome);
            if (biome == null) continue;
            var pal = Get(biome.Value);
            if (pal.landmarks.Length == 0) continue;
            var go = Place(c, c.Pick(pal.landmarks), t, x, z, FaceToward(c, x, z), 1f, "Landmarks");
            if (go == null) continue;
            ClearAround(t, go, 3f);
            c.occupied.Add(new Vector3(x, Radius(go), z));
            // a little dressing at the foot so it does not look dropped in
            for (int i = 0; i < 4 && pal.bushes.Length > 0; i++) {
                float a = c.Rand(0f, Mathf.PI * 2f), d = Radius(go) + c.Rand(1f, 4f);
                Place(c, c.Pick(pal.bushes), t, x + Mathf.Cos(a) * d, z + Mathf.Sin(a) * d, c.Rand(0f, 360f), c.Rand(0.8f, 1.2f), "Landmarks");
            }
            placed++;
        }
        return $"landmarks {placed}";
    }

    // 3. props along path edges inside the play space
    static string PathProps (Context c) {
        if (c.options.pathProps <= 0) return "path props 0";
        int placed = 0;
        var spots = new List<Vector2>();
        for (int attempt = 0; attempt < 6000 && placed < c.options.pathProps; attempt++) {
            var t = c.Pick(c.terrains); var td = t.terrainData; var o = t.transform.position;
            float u = c.Rand(0.02f, 0.98f), v = c.Rand(0.02f, 0.98f);
            if (BareAt(t, u, v) < 0.7f || td.GetSteepness(u, v) > 12f) continue; // on the path
            float x = o.x + u * td.size.x, z = o.z + v * td.size.z;
            float sd = c.Sd(x, z);
            if (c.options.titleSetPieces) { if (sd > 30f) continue; }  // title: near the camera flight
            else if (sd > -6f) continue;
            // step sideways onto the verge
            float a0 = c.Rand(0f, Mathf.PI * 2f); bool found = false; float px = 0, pz = 0, faceX = 0, faceZ = 0;
            for (int s = 0; s < 8 && !found; s++) {
                float a = a0 + s * Mathf.PI / 4f;
                for (float d = 2f; d <= 7f; d += 1f) {
                    float qx = x + Mathf.Cos(a) * d, qz = z + Mathf.Sin(a) * d;
                    float qu = (qx - o.x) / td.size.x, qv = (qz - o.z) / td.size.z;
                    if (qu < 0 || qu > 1 || qv < 0 || qv > 1) break;
                    if (BareAt(t, qu, qv) > 0.3f) continue;
                    if (td.GetSteepness(qu, qv) > 15f || Blocked(t, qu, qv, 1.2f)) break;
                    px = qx + Mathf.Cos(a) * 1f; pz = qz + Mathf.Sin(a) * 1f; faceX = x; faceZ = z; found = true; break;
                }
            }
            if (!found) continue;
            if (c.options.titleSetPieces && c.Sd(px, pz) < -8f) continue; // not in front of the lens
            bool near = false; foreach (var p in spots) if ((p - new Vector2(px, pz)).sqrMagnitude < 40f * 40f) { near = true; break; }
            if (near) continue;
            var biome = BiomeAt(t, (px - o.x) / td.size.x, (pz - o.z) / td.size.z, c.options.defaultBiome);
            if (biome == null) continue;
            var pal = Get(biome.Value);
            if (pal.pathProps.Length == 0) continue;
            float yaw = Mathf.Atan2(faceX - px, faceZ - pz) * Mathf.Rad2Deg;
            var go = Place(c, c.Pick(pal.pathProps), t, px, pz, yaw, 1f, "Path Props");
            if (go == null) continue;
            ClearAround(t, go, 0.5f);
            spots.Add(new Vector2(px, pz));
            placed++;
        }
        return $"path props {placed}";
    }

    // 4. ponds in hollows outside the play space
    static string Ponds (Context c) {
        if (c.options.ponds <= 0) return "ponds 0";
        var water = AssetDatabase.LoadAssetAtPath<GameObject>(SS + "Water/Prefabs/P_Water.prefab");
        var fish = AssetDatabase.LoadAssetAtPath<GameObject>(TS + "Prefabs/Animals/TSI_Fish_01A.prefab");
        if (water == null) return "ponds: no water prefab";
        int placed = 0;
        for (int attempt = 0; attempt < 3000 && placed < c.options.ponds; attempt++) {
            var t = c.Pick(c.terrains); var td = t.terrainData; var o = t.transform.position;
            float u = c.Rand(0.05f, 0.95f), v = c.Rand(0.05f, 0.95f);
            float x = o.x + u * td.size.x, z = o.z + v * td.size.z;
            float sd = c.Sd(x, z);
            if (sd < 10f || sd > 120f || c.Occupied(x, z, 25f)) continue;
            var biome = BiomeAt(t, u, v, c.options.defaultBiome);
            if (biome == null || biome == Biome.Snow || biome == Biome.Wasteland) continue;
            // needs fairly level ground; the basin is dug so the water sits below the surrounding rim
            float rim = float.MaxValue, top = float.MinValue; const float r = 11f; bool edge = false;
            for (int k = 0; k < 12; k++) {
                float a = k * Mathf.PI / 6f, rx = x + Mathf.Cos(a) * r, rz = z + Mathf.Sin(a) * r;
                if (rx < o.x || rx > o.x + td.size.x || rz < o.z || rz > o.z + td.size.z) { edge = true; break; }
                float h = t.SampleHeight(new Vector3(rx, 0, rz)) + o.y;
                rim = Mathf.Min(rim, h); top = Mathf.Max(top, h);
            }
            if (edge || top - rim > 1.2f || td.GetSteepness(u, v) > 8f) continue;
            float level = rim - 0.45f;
            DigBasin(t, x, z, r * 1.5f, level - 1.6f);
            var pond = new GameObject("Pond");
            pond.transform.SetParent(Group(c, "Ponds"), false);
            pond.transform.position = new Vector3(x, level, z);
            var w = (GameObject)PrefabUtility.InstantiatePrefab(water, c.scene);
            w.transform.SetParent(pond.transform, false);
            var wb = WorldBounds(w);
            // the water plane is square: keep its corners inside the dug basin
            float fit = wb.size.x > 0.01f ? (r * 1.75f) / Mathf.Max(wb.size.x, wb.size.z) : 1f;
            w.transform.rotation = Quaternion.Euler(0, c.Rand(0f, 90f), 0);
            w.transform.localScale *= fit;
            w.transform.position = new Vector3(x, level, z);
            ClearDetails(t, new Bounds(new Vector3(x, level, z), new Vector3(r * 2f, 10f, r * 2f)));
            var pal = Get(biome.Value);
            for (int i = 0; i < 10 && pal.pondPlants.Length > 0; i++) {
                float a = c.Rand(0f, Mathf.PI * 2f), d = c.Rand(r * 0.45f, r * 1.05f);
                var p = Place(c, c.Pick(pal.pondPlants), t, x + Mathf.Cos(a) * d, z + Mathf.Sin(a) * d, c.Rand(0f, 360f), c.Rand(0.8f, 1.3f), null, pond.transform);
                if (p != null && p.name.Contains("Lily")) p.transform.position = new Vector3(p.transform.position.x, level + 0.02f, p.transform.position.z);
            }
            for (int i = 0; i < 5 && pal.rocks.Length > 0; i++) {
                float a = c.Rand(0f, Mathf.PI * 2f);
                Place(c, c.Pick(pal.rocks), t, x + Mathf.Cos(a) * r * 1.1f, z + Mathf.Sin(a) * r * 1.1f, c.Rand(0f, 360f), c.Rand(0.5f, 0.9f), null, pond.transform);
            }
            // 7. ambient life
            if (fish != null)
                for (int i = 0; i < 3; i++) {
                    var f = (GameObject)PrefabUtility.InstantiatePrefab(fish, c.scene);
                    f.transform.SetParent(pond.transform, false);
                    f.transform.position = new Vector3(x + c.Rand(-4f, 4f), level - 0.25f, z + c.Rand(-4f, 4f));
                    f.transform.rotation = Quaternion.Euler(0, c.Rand(0f, 360f), 0);
                }
            c.occupied.Add(new Vector3(x, r + 4f, z));
            placed++;
        }
        return $"ponds {placed}";
    }

    // 5. distant mountains, inside the camera far plane
    static string Mountains (Context c) {
        var prefabs = Paths(SS + "Rocks/Mountains/Prefabs/P_", "Mountain01", "Mountain02", "Mountain03", "Mountain04");
        var b = new Bounds(c.terrains[0].transform.position, Vector3.zero);
        foreach (var t in c.terrains) { b.Encapsulate(t.transform.position); b.Encapsulate(t.transform.position + t.terrainData.size); }
        float radius = Mathf.Max(b.extents.x, b.extents.z) + 260f;
        // terrain transforms often sit far below the actual ground (heights are stored in the heightmap)
        float baseY = float.MaxValue;
        foreach (var t in c.terrains)
            for (int i = 0; i <= 8; i++)
                foreach (var q in new[] { new Vector2(i / 8f, 0f), new Vector2(i / 8f, 1f), new Vector2(0f, i / 8f), new Vector2(1f, i / 8f) })
                    baseY = Mathf.Min(baseY, t.terrainData.GetInterpolatedHeight(q.x, q.y) + t.transform.position.y);
        int count = 7; int placed = 0;
        float offset = c.Rand(0f, 360f);
        for (int i = 0; i < count; i++) {
            float a = (offset + i * 360f / count + c.Rand(-15f, 15f)) * Mathf.Deg2Rad;
            float d = radius * c.Rand(1f, 1.25f);
            var pos = new Vector3(b.center.x + Mathf.Cos(a) * d, baseY, b.center.z + Mathf.Sin(a) * d);
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>(c.Pick(prefabs));
            if (pf == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(pf, c.scene);
            go.transform.SetParent(Group(c, "Background"), false);
            go.transform.rotation = Quaternion.Euler(0, c.Rand(0f, 360f), 0);
            var wb = WorldBounds(go);
            float targetHeight = c.Rand(160f, 280f);
            float sc = wb.size.y > 1f ? targetHeight / wb.size.y : 0.3f;
            go.transform.localScale = Vector3.one * sc;
            wb = WorldBounds(go);
            float pivotAboveBottom = go.transform.position.y - wb.min.y;
            go.transform.position = new Vector3(pos.x, baseY + pivotAboveBottom - targetHeight * 0.25f, pos.z);
            foreach (var col in go.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(col);
            TintMountain(go, c.options.defaultBiome);
            foreach (var r in go.GetComponentsInChildren<Renderer>(true)) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            placed++;
        }
        return $"mountains {placed}";
    }

    // The mountain shader paints grass on gentle slopes; green peaks clash with snow/wasteland/autumn maps.
    public static void TintMountain (GameObject go, Biome biome) {
        if (biome == Biome.Summer || biome == Biome.Forest) return;
        const string folder = "Assets/Settings/MapDecor";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Settings", "MapDecor");
        string path = $"{folder}/MV_Mountain_{biome}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) {
            var src = AssetDatabase.LoadAssetAtPath<Material>(SS + "Rocks/Materials/Mountain/MV_Mountain.mat");
            mat = new Material(src);
            switch (biome) {
                case Biome.Snow: mat.SetFloat("_Grass_Slope_Max", 0f); mat.SetFloat("_Snow_Top_Amount", 0.85f); break;
                case Biome.Wasteland: mat.SetFloat("_Grass_Slope_Max", 0f); mat.SetFloat("_Snow_Top_Amount", 0f); break;
                default: mat.SetFloat("_Grass_Slope_Max", 0.08f); mat.SetFloat("_Snow_Top_Amount", 0.15f); break;
            }
            AssetDatabase.CreateAsset(mat, path);
        }
        foreach (var r in go.GetComponentsInChildren<Renderer>(true)) {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) if (mats[i] != null && mats[i].shader.name == "Shader Graphs/S_Mountain") mats[i] = mat;
            r.sharedMaterials = mats;
        }
    }

    // 6. title: one set piece per biome stretch of the camera flight
    static string TitleSetPieces (Context c) {
        if (c.cameraPath == null || c.cameraPath.Count < 10) return "set pieces: no camera path";
        var done = new HashSet<Biome>();
        int placed = 0;
        for (int i = c.cameraPath.Count / 12; i < c.cameraPath.Count - 3; i += 3) {
            var p = c.cameraPath[i];
            var next = c.cameraPath[Mathf.Min(i + 3, c.cameraPath.Count - 1)];
            var forward = next - p; forward.y = 0; if (forward.sqrMagnitude < 0.01f) continue; forward.Normalize();
            var right = new Vector3(forward.z, 0, -forward.x);
            Terrain t; float u, v;
            if (!Locate(c, p.x, p.z, out t, out u, out v)) continue;
            var biome = BiomeAt(t, u, v, c.options.defaultBiome);
            if (biome == null || done.Contains(biome.Value)) continue;
            // keep going until the stretch is established, then place ahead and to the side so it enters the frame
            if (i + 12 >= c.cameraPath.Count) break;
            Terrain t2; float u2, v2;
            if (!Locate(c, c.cameraPath[i + 12].x, c.cameraPath[i + 12].z, out t2, out u2, out v2) || BiomeAt(t2, u2, v2, c.options.defaultBiome) != biome) continue;

            var ahead = c.cameraPath[i + 12];
            float side = (placed % 2 == 0 ? 1f : -1f) * 32f;
            var center = ahead + right * side;
            if (!Locate(c, center.x, center.z, out t2, out u2, out v2)) continue;
            var pal = Get(biome.Value);
            float yaw = Mathf.Atan2(-right.x * Mathf.Sign(side), -right.z * Mathf.Sign(side)) * Mathf.Rad2Deg; // face the path

            string hero = null, second = null, tree = pal.trees.Length > 0 ? pal.trees[0] : null;
            switch (biome.Value) {
                case Biome.Summer: hero = TS + "Prefabs/Presets/Structures/TSI_Preset_Gazebo_02A.prefab"; second = TS + "Prefabs/Presets/Structures/TSI_Preset_Pond_01A.prefab"; tree = TS + "Prefabs/Vegetation/Trees/TSI_Blossom_Tree_01A.prefab"; break;
                case Biome.Autumn: hero = TS + "Prefabs/Presets/Structures/TSI_Preset_Bell_Tower_01A.prefab"; second = TS + "Prefabs/Building Props/Torii Gate/TSI_Torii_Gate_01A.prefab"; tree = TS + "Prefabs/Vegetation/Trees/TSI_Amberleaf_Tree_01A.prefab"; break;
                case Biome.Snow: hero = SS + "Rocks/Spire/Prefabs/P_RockSpire_RockClumpB10.prefab"; second = SS + "Rocks/Spire/Prefabs/P_RockSpire_RockClumpC01.prefab"; tree = SS + "Trees/Pine/Prefabs/P_PineSnowCover01.prefab"; break;
                case Biome.Blossom: hero = TS + "Prefabs/Presets/Structures/TSI_Preset_Temple_01A.prefab"; second = TS + "Prefabs/Building Props/Torii Gate/TSI_Torii_Gate_01B.prefab"; tree = TS + "Prefabs/Vegetation/Trees/TSI_Blossom_Tree_02A.prefab"; break;
                default: hero = pal.landmarks.Length > 0 ? pal.landmarks[0] : null; break;
            }
            var group = Group(c, "Title Set Pieces");
            var heroGo = hero != null ? Place(c, hero, t2, center.x, center.z, yaw, 1f, null, group) : null;
            if (heroGo == null) continue;
            ClearAround(t2, heroGo, 3f);
            if (second != null) {
                var sp = center - forward * 14f + right * Mathf.Sign(side) * -10f;
                var s = Place(c, second, t2, sp.x, sp.z, yaw, 1f, null, group);
                if (s != null) ClearAround(t2, s, 1f);
            }
            if (tree != null)
                for (int k = 0; k < 3; k++) {
                    var tp = center + forward * c.Rand(-18f, 18f) + right * Mathf.Sign(side) * c.Rand(8f, 20f);
                    Place(c, tree, t2, tp.x, tp.z, c.Rand(0f, 360f), c.Rand(0.9f, 1.2f), null, group);
                }
            for (int k = 0; k < 10 && pal.bushes.Length > 0; k++) {
                var bp = center + forward * c.Rand(-16f, 16f) + right * Mathf.Sign(side) * c.Rand(-14f, 8f);
                if (c.Sd(bp.x, bp.z) < -6f) continue;
                Place(c, c.Pick(pal.bushes), t2, bp.x, bp.z, c.Rand(0f, 360f), c.Rand(0.8f, 1.2f), null, group);
            }
            if (pal.ambient.Length > 0) {
                var amb = Place(c, pal.ambient[0], t2, center.x, center.z, 0f, 1f, null, group);
                if (amb != null) amb.transform.position += Vector3.up * 3f;
            }
            done.Add(biome.Value);
            placed++;
        }
        return $"title set pieces {placed} ({string.Join(",", done)})";
    }

    // ---------------------------------------------------------------------------------------------------------
    static GameObject Place (Context c, string path, Terrain t, float x, float z, float yaw, float scale, string groupName, Transform parent = null) {
        var pf = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (pf == null) return null;
        Terrain tt; float u, v;
        if (!Locate(c, x, z, out tt, out u, out v)) return null;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(pf, c.scene);
        go.transform.SetParent(parent != null ? parent : Group(c, groupName ?? "Misc"), false);
        go.transform.rotation = Quaternion.Euler(0, yaw, 0);
        go.transform.localScale = pf.transform.localScale * scale;
        // seat on the lowest ground under the footprint so nothing floats on a slope
        go.transform.position = new Vector3(x, 0, z);
        var b = WorldBounds(go);
        float low = float.MaxValue;
        var e = new Vector2(Mathf.Min(b.extents.x, 6f) * 0.6f, Mathf.Min(b.extents.z, 6f) * 0.6f);
        foreach (var q in new[] { Vector2.zero, new Vector2(e.x, e.y), new Vector2(-e.x, e.y), new Vector2(e.x, -e.y), new Vector2(-e.x, -e.y) }) {
            Terrain qt; float qu, qv;
            if (Locate(c, x + q.x, z + q.y, out qt, out qu, out qv)) low = Mathf.Min(low, qt.SampleHeight(new Vector3(x + q.x, 0, z + q.y)) + qt.transform.position.y);
        }
        float pivotToBottom = go.transform.position.y - b.min.y;
        go.transform.position = new Vector3(x, low + Mathf.Clamp(pivotToBottom, 0f, 0.3f) - 0.08f, z);
        return go;
    }

    static Transform Group (Context c, string name) {
        var t = c.root.Find(name);
        if (t != null) return t;
        var g = new GameObject(name);
        g.transform.SetParent(c.root, false);
        return g.transform;
    }

    static bool Locate (Context c, float x, float z, out Terrain terrain, out float u, out float v) {
        foreach (var t in c.terrains) {
            var o = t.transform.position; var s = t.terrainData.size;
            if (x < o.x || x > o.x + s.x || z < o.z || z > o.z + s.z) continue;
            terrain = t; u = (x - o.x) / s.x; v = (z - o.z) / s.z; return true;
        }
        terrain = null; u = v = 0; return false;
    }

    static float FaceToward (Context c, float x, float z) {
        if (c.playable == null) return c.Rand(0f, 360f);
        // look back toward the play space: step along the falling distance gradient
        float gx = c.Sd(x + 4f, z) - c.Sd(x - 4f, z), gz = c.Sd(x, z + 4f) - c.Sd(x, z - 4f);
        return Mathf.Atan2(-gx, -gz) * Mathf.Rad2Deg + c.Rand(-25f, 25f);
    }

    static Biome? BiomeAt (Terrain t, float u, float v, Biome fallback) {
        var td = t.terrainData;
        int ax = Mathf.Clamp((int)(u * (td.alphamapWidth - 1)), 0, td.alphamapWidth - 1), ay = Mathf.Clamp((int)(v * (td.alphamapHeight - 1)), 0, td.alphamapHeight - 1);
        var a = td.GetAlphamaps(ax, ay, 1, 1);
        float summer = 0, autumn = 0, snow = 0, blossom = 0, bare = 0;
        var layers = td.terrainLayers;
        for (int i = 0; i < layers.Length; i++) {
            if (layers[i] == null) continue;
            string n = layers[i].name, tex = layers[i].diffuseTexture != null ? layers[i].diffuseTexture.name : "";
            float w = a[0, 0, i];
            if (tex.Contains("Grass_10")) blossom += w;
            else if (n.Contains("Aut_")) autumn += w;
            else if (n.Contains("Snow") || n.Contains("Ice")) snow += w;
            else if (n.Contains("Grass")) summer += w;
            else bare += w;
        }
        float best = Mathf.Max(summer, autumn, snow, blossom);
        if (best < 0.35f) return fallback == Biome.Wasteland || fallback == Biome.Snow ? fallback : (Biome?)fallback;
        if (best == blossom) return Biome.Blossom;
        if (best == autumn) return Biome.Autumn;
        if (best == snow) return Biome.Snow;
        return fallback == Biome.Forest || fallback == Biome.Wasteland ? fallback : Biome.Summer;
    }

    static float BareAt (Terrain t, float u, float v) {
        var td = t.terrainData;
        int ax = Mathf.Clamp((int)(u * (td.alphamapWidth - 1)), 0, td.alphamapWidth - 1), ay = Mathf.Clamp((int)(v * (td.alphamapHeight - 1)), 0, td.alphamapHeight - 1);
        var a = td.GetAlphamaps(ax, ay, 1, 1);
        float bare = 0;
        var layers = td.terrainLayers;
        for (int i = 0; i < layers.Length; i++) {
            if (layers[i] == null) continue;
            string n = layers[i].name;
            if (n.Contains("Ground") || n.Contains("Path") || n.Contains("Mud") || n.Contains("Tile") || n.Contains("Rock") || n.Contains("Sand")) bare += a[0, 0, i];
        }
        return bare;
    }

    static bool Blocked (Terrain t, float u, float v, float radius) {
        var td = t.terrainData; var o = t.transform.position;
        var p = new Vector3(o.x + u * td.size.x, td.GetInterpolatedHeight(u, v) + o.y + 1.5f, o.z + v * td.size.z);
        foreach (var h in Physics.OverlapBox(p, new Vector3(radius, 1.3f, radius), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore)) {
            if (h is TerrainCollider || h.GetComponent<UnityEngine.Rendering.Volume>() != null) continue;
            if (h.bounds.size.x > 150f && h.bounds.size.z > 150f) continue;
            return true;
        }
        return false;
    }

    static bool Crowded (List<TreeInstance> list, TerrainData td, float u, float v, float spacing) {
        float su = spacing / td.size.x, sv = spacing / td.size.z;
        // linear scan limited to recent additions keeps this cheap; nearby old trees are handled by the terrain's own density
        for (int i = list.Count - 1; i >= 0 && i >= list.Count - 400; i--) {
            float du = list[i].position.x - u, dv = list[i].position.z - v;
            if ((du / su) * (du / su) + (dv / sv) * (dv / sv) < 1f) return true;
        }
        return false;
    }

    static readonly Dictionary<string, GameObject> treeCache = new Dictionary<string, GameObject>();
    static GameObject LoadTreeCompatible (string path) {
        GameObject pf;
        if (treeCache.TryGetValue(path, out pf)) return pf;
        pf = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        // terrain trees need a LODGroup or a MeshRenderer on the prefab root
        if (pf != null && pf.GetComponent<LODGroup>() == null && pf.GetComponent<MeshRenderer>() == null) pf = null;
        treeCache[path] = pf;
        return pf;
    }

    static void EnableInstancing (GameObject prefab) {
        foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
            foreach (var m in r.sharedMaterials)
                if (m != null && !m.enableInstancing) { m.enableInstancing = true; EditorUtility.SetDirty(m); }
    }

    static void RemovePreviousTreeInstances (Terrain[] terrains) {
        foreach (var t in terrains) {
            var td = t.terrainData;
            var protos = td.treePrototypes;
            var ours = new bool[protos.Length];
            for (int i = 0; i < protos.Length; i++) {
                var p = protos[i].prefab != null ? AssetDatabase.GetAssetPath(protos[i].prefab) : "";
                ours[i] = p.StartsWith("Assets/SoStylized") || p.StartsWith("Assets/ToonScapes");
            }
            var keep = new List<TreeInstance>();
            foreach (var inst in td.treeInstances) if (!ours[inst.prototypeIndex]) keep.Add(inst);
            td.SetTreeInstances(keep.ToArray(), true);
        }
    }

    // bowl-shaped dip: floor at floorY in the middle, blending back to the original surface at the radius
    static void DigBasin (Terrain t, float x, float z, float radius, float floorY) {
        var td = t.terrainData; var o = t.transform.position; int res = td.heightmapResolution;
        float cell = td.size.x / (res - 1);
        int cx = Mathf.RoundToInt((x - o.x) / cell), cz = Mathf.RoundToInt((z - o.z) / cell), rr = Mathf.CeilToInt(radius / cell) + 1;
        int x0 = Mathf.Clamp(cx - rr, 0, res - 1), x1 = Mathf.Clamp(cx + rr, 0, res - 1), z0 = Mathf.Clamp(cz - rr, 0, res - 1), z1 = Mathf.Clamp(cz + rr, 0, res - 1);
        var h = td.GetHeights(x0, z0, x1 - x0 + 1, z1 - z0 + 1);
        for (int j = 0; j <= z1 - z0; j++)
            for (int i = 0; i <= x1 - x0; i++) {
                float wx = o.x + (x0 + i) * cell, wz = o.z + (z0 + j) * cell;
                float d = Mathf.Sqrt((wx - x) * (wx - x) + (wz - z) * (wz - z)) / radius;
                if (d >= 1f) continue;
                float cur = h[j, i] * td.size.y + o.y;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(1f, 0.55f, d));
                h[j, i] = (Mathf.Min(cur, Mathf.Lerp(cur, floorY, k)) - o.y) / td.size.y;
            }
        td.SetHeights(x0, z0, h);
        // trees that stood in the basin would float
        var keep = new List<TreeInstance>();
        foreach (var inst in td.treeInstances) {
            float wx = o.x + inst.position.x * td.size.x, wz = o.z + inst.position.z * td.size.z;
            if ((wx - x) * (wx - x) + (wz - z) * (wz - z) < radius * radius) continue;
            keep.Add(inst);
        }
        td.SetTreeInstances(keep.ToArray(), true);
    }

    // remove terrain grass/trees poking through a placed object
    static void ClearAround (Terrain t, GameObject go, float margin) {
        var b = WorldBounds(go);
        b.Expand(new Vector3(margin * 2f, 0, margin * 2f));
        foreach (var terrain in Object.FindObjectsOfType<Terrain>()) {
            ClearDetails(terrain, b);
            var td = terrain.terrainData; var o = terrain.transform.position;
            var keep = new List<TreeInstance>(); bool changed = false;
            foreach (var inst in td.treeInstances) {
                var w = new Vector3(o.x + inst.position.x * td.size.x, b.center.y, o.z + inst.position.z * td.size.z);
                if (b.Contains(w)) { changed = true; continue; }
                keep.Add(inst);
            }
            if (changed) td.SetTreeInstances(keep.ToArray(), true);
        }
    }

    static void ClearDetails (Terrain t, Bounds b) {
        var td = t.terrainData; var o = t.transform.position; int res = td.detailResolution;
        int x0 = Mathf.Clamp(Mathf.FloorToInt((b.min.x - o.x) / td.size.x * res), 0, res), x1 = Mathf.Clamp(Mathf.CeilToInt((b.max.x - o.x) / td.size.x * res), 0, res);
        int z0 = Mathf.Clamp(Mathf.FloorToInt((b.min.z - o.z) / td.size.z * res), 0, res), z1 = Mathf.Clamp(Mathf.CeilToInt((b.max.z - o.z) / td.size.z * res), 0, res);
        if (x1 <= x0 || z1 <= z0) return;
        for (int l = 0; l < td.detailPrototypes.Length; l++) td.SetDetailLayer(x0, z0, l, new int[z1 - z0, x1 - x0]);
    }

    static Bounds WorldBounds (GameObject go) {
        bool any = false; var b = new Bounds(go.transform.position, Vector3.zero);
        foreach (var r in go.GetComponentsInChildren<Renderer>(true)) {
            if (r is ParticleSystemRenderer) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        return b;
    }

    static float Radius (GameObject go) {
        var b = WorldBounds(go);
        return Mathf.Max(b.extents.x, b.extents.z);
    }

    static List<Vector3> TitleCameraPath (UnityEngine.SceneManagement.Scene scene) {
        if (scene.name != "Title") return null;
        UnityEngine.Playables.PlayableDirector director = null; Transform cam = null;
        foreach (var r in scene.GetRootGameObjects()) {
            if (director == null) director = r.GetComponentInChildren<UnityEngine.Playables.PlayableDirector>(true);
            if (cam == null) { var c = r.GetComponentInChildren<Camera>(true); if (c != null) cam = c.transform; }
        }
        if (director == null || cam == null) return null;
        var list = new List<Vector3>();
        for (double t = 0; t <= director.duration; t += 2.0) { director.time = t; director.Evaluate(); list.Add(cam.position); }
        director.time = 0; director.Evaluate();
        return list;
    }

    static string[] Paths (string prefix, params string[] names) {
        var r = new string[names.Length];
        for (int i = 0; i < names.Length; i++) r[i] = prefix + names[i] + ".prefab";
        return r;
    }

    static string[] Concat (this string[] a, string[] b) {
        var r = new string[a.Length + b.Length];
        a.CopyTo(r, 0); b.CopyTo(r, a.Length);
        return r;
    }

    // ---------------------------------------------------------------------------------------------------------
    // Variety pass: many small, different pieces (saplings, stumps, branches, ferns, flower patches, leaf litter,
    // small rocks, cacti...) sprinkled as terrain tree instances. Uses its own pools so it can be re-run safely.
    static string[] VarietyPool (Biome b) {
        string F = SS + "Foliage/Prefabs/P_", O = SS + "Trees/Oak/Prefabs/P_", Pi = SS + "Trees/Pine/Prefabs/P_", Fi = SS + "Trees/Fir/Prefabs/P_",
               Bi = SS + "Trees/Birch/Prefabs/P_", RC = SS + "Rocks/Classic/Prefabs/P_", RD = SS + "Rocks/Desert/Prefabs/P_", De = SS + "Trees/Desert/Prefabs/P_",
               TP = TS + "Prefabs/Vegetation/Plants & Flowers/TSI_", TT = TS + "Prefabs/Vegetation/Trees/TSI_", TR = TS + "Prefabs/Rocks/TSI_", Mi = SS + "Misc/Prefabs/P_";
        switch (b) {
            case Biome.Forest: return Paths(Fi, "FirBranch1", "FirBranch2", "FirSapling1", "FirSapling2", "FirStump1").Concat(Paths(F, "Fern1", "RedFern02", "Fern3", "ElephantEars01"))
                .Concat(Paths(O, "OakRoots2", "OakBranch1")).Concat(Paths(TP, "Leaf_Patch_01A", "Plant_05A", "Plant_08A", "Grass_Patch_03A")).Concat(Paths(TR, "Rock_Medium_01B", "Rock_Small_01A"));
            case Biome.Autumn: return Paths(O, "OakBranch2", "OakRoots1", "OakStump1").Concat(Paths(F, "RedFern01", "RedFern02", "Fern3_Yellow", "FoxtailsLight1", "Bush3_Dry"))
                .Concat(Paths(TP, "Leaf_Patch_01A", "Leaf_Patch_02A", "Leaf_Patch_03A", "Amberleaf_Bush_01A", "Plant_21B")).Concat(Paths(TT, "Amberleaf_Shrub_01A")).Concat(Paths(TR, "Rock_Small_02A", "Rock_Medium_03A"));
            case Biome.Snow: return Paths(Pi, "PineStump1_SnowCover", "PineFallen1_SnowCover", "PineDead01_Snowy").Concat(Paths(O, "OakFallen2_SnowCover", "OakStump1_SnowCover", "OakTree2_Snow"))
                .Concat(Paths(F, "BushSnowDead02", "Bush3_Snowy")).Concat(Paths(Mi, "SnowDrift03", "SnowDrift05")).Concat(Paths(TR, "Rock_Small_03C", "Rock_Medium_02C"));
            case Biome.Wasteland: return Paths(F, "DesertTwigRoots02", "DesertTwigRoots03", "CactusBulb01", "CactusBulb02", "CactusPricklyPear02", "CactusPricklyPear03", "CactusPricklyPear04", "Cactus01", "Cactus02", "DesertBushB02", "DesertBush03", "DesertBush04", "Tumbleweed01")
                .Concat(Paths(RD, "RockDesert_Rock05", "RockDesert_Rock08", "RockDesert_Rock14", "RockDesert_Clump05", "RockDesert_Clump06")).Concat(Paths(De, "TreeWindsweptSapling01", "TreeWindsweptSaplingB02"));
            case Biome.Blossom: return Paths(TP, "Petals_01A", "Petals_02A", "Petals_03A", "Flower_Patch_03A", "Rose_02B", "Plant_13A", "Flower_Bush_02B").Concat(Paths(TT, "Blossom_Shrub_01A")).Concat(Paths(F, "Allium", "TulipsPink"));
            default: return Paths(O, "OakBranch1", "OakRoots1", "OakSapling1", "OakSapling2").Concat(Paths(Bi, "BirchSapling1", "BirchStump")).Concat(Paths(F, "Fern2", "Fern3", "FlowerCrocus01", "Sunflower1", "TulipsPink", "TulipsOrange", "Allium", "Foxtails1", "Bush3_Light"))
                .Concat(Paths(TP, "Plant_04A", "Plant_08A", "Plant_21A", "Flower_Patch_01A", "Flower_Patch_02A", "Grass_Patch_01A", "Grass_Patch_04A", "Rose_01A", "Bush_01A", "Bush_02B", "Wheat_Patch_01A"))
                .Concat(Paths(TT, "Springleaf_Shrub_01A", "Broadleaf_Shrub_01A")).Concat(Paths(TR, "Rock_Small_01A", "Rock_Medium_02A")).Concat(Paths(RC, "RockClassic3", "RockClumpClassic2"));
        }
    }

    public static string DecorateVariety (Options o, float perHectare = 14f) {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var terrains = Object.FindObjectsOfType<Terrain>();
        Physics.SyncTransforms();
        var allPools = new HashSet<string>();
        foreach (Biome b in System.Enum.GetValues(typeof(Biome))) foreach (var p in VarietyPool(b)) allPools.Add(p);
        var c = new Context { scene = scene, terrains = terrains, rng = new System.Random(o.seed * 4271 + 5), options = o };
        string src;
        c.playable = TerrainSculpt.ComputePlayable(out src);
        c.cameraPath = TitleCameraPath(scene);

        int added = 0; var kinds = new HashSet<string>();
        foreach (var t in terrains) {
            var td = t.terrainData; var to = t.transform.position; var size = td.size;
            var protos = new List<TreePrototype>(td.treePrototypes);
            // drop the previous variety instances
            var list = new List<TreeInstance>();
            foreach (var inst in td.treeInstances) {
                var pf = protos[inst.prototypeIndex].prefab;
                if (pf != null && allPools.Contains(AssetDatabase.GetAssetPath(pf))) continue;
                list.Add(inst);
            }
            int target = Mathf.RoundToInt(size.x * size.z / 10000f * perHectare);
            for (int a = 0; a < target * 6 && target > 0; a++) {
                float u = (float)c.rng.NextDouble(), v = (float)c.rng.NextDouble();
                float x = to.x + u * size.x, z = to.z + v * size.z;
                float sd = c.Sd(x, z);
                var biome = BiomeAt(t, u, v, o.defaultBiome);
                if (biome == null) continue;
                if (td.GetSteepness(u, v) > 32f) continue;
                float bare = BareAt(t, u, v);
                if (bare > 0.35f && biome != Biome.Wasteland) continue;
                if (sd < 0f && (o.bossArena || o.town) && sd < -4f) continue;
                if (sd > -2f && sd < 2f) continue;
                if (c.cameraPath != null && sd < -5f) continue; // keep the title flight's lens clear
                var pool = VarietyPool(biome.Value);
                var path = c.Pick(pool);
                var prefab = LoadTreeCompatible(path);
                if (prefab == null) continue;
                // anything that blocks movement stays out of the play space
                bool solid = prefab.GetComponentInChildren<CapsuleCollider>(true) != null;
                if (sd < 0f && solid) continue;
                if (sd < 0f && Blocked(t, u, v, 0.8f)) continue;
                if (Crowded(list, td, u, v, 1.6f)) continue;
                int idx = protos.FindIndex(p => p.prefab == prefab);
                if (idx < 0) { protos.Add(new TreePrototype { prefab = prefab, bendFactor = 0f }); idx = protos.Count - 1; EnableInstancing(prefab); }
                float sc = c.Rand(0.75f, 1.3f);
                list.Add(new TreeInstance { position = new Vector3(u, td.GetInterpolatedHeight(u, v) / size.y, v), prototypeIndex = idx,
                    widthScale = sc, heightScale = sc * c.Rand(0.9f, 1.1f), rotation = c.Rand(0f, Mathf.PI * 2f), color = Color.white, lightmapColor = Color.white });
                kinds.Add(prefab.name);
                added++;
                if (added >= target * terrains.Length) break;
            }
            td.treePrototypes = protos.ToArray();
            td.SetTreeInstances(list.ToArray(), true);
            EditorUtility.SetDirty(td);
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        return $"{scene.name}: variety +{added} ({kinds.Count} kinds)";
    }

    public static string ValidateVariety () {
        var missing = new List<string>(); var incompatible = new List<string>();
        foreach (Biome b in System.Enum.GetValues(typeof(Biome)))
            foreach (var p in VarietyPool(b)) {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (go == null) missing.Add(p); else if (LoadTreeCompatible(p) == null) incompatible.Add(go.name);
            }
        return "missing: " + string.Join(", ", missing.ToArray()) + "\nnot terrain-compatible: " + string.Join(", ", incompatible.ToArray());
    }

    // Gameplay safety pass: removes decoration that could trap or block the player/enemies.
    //  - anything within clearRadius of the player, enemies, animals or trigger zones (spawn points, scene exits)
    //  - decoration with colliders inside the play space that sits in a narrow gap (< gapWidth to another solid)
    public static string AuditGameplay (float clearRadius = 4f, float gapWidth = 2.5f) {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var terrains = Object.FindObjectsOfType<Terrain>();
        Physics.SyncTransforms();
        string src;
        var playable = TerrainSculpt.ComputePlayable(out src);

        var keep = new List<Vector3>();
        foreach (var root in scene.GetRootGameObjects()) {
            if (root.name == RootName) continue;
            if (root.CompareTag("Player")) keep.Add(root.transform.position);
            if (root.name == "敵" || root.name == "動物") foreach (Transform ch in root.transform) keep.Add(ch.position);
            foreach (var col in root.GetComponentsInChildren<Collider>(true))
                if (col.isTrigger && col.GetComponent<UnityEngine.Rendering.Volume>() == null && Mathf.Max(col.bounds.size.x, col.bounds.size.z) < 150f)
                    keep.Add(col.bounds.center);
        }

        int removedObjects = 0, removedTrees = 0, gapRemoved = 0;
        GameObject decor = null;
        foreach (var r in scene.GetRootGameObjects()) if (r.name == RootName) decor = r;
        if (decor != null) {
            var solids = new List<Transform>();
            // groups are plain containers; hand-placed prefabs sitting directly under the root are left alone
            foreach (Transform group in decor.transform) {
                if (PrefabUtility.IsPartOfPrefabInstance(group.gameObject)) continue;
                foreach (Transform item in group) solids.Add(item);
            }
            foreach (var item in solids) {
                if (item == null) continue;
                var b = WorldBounds(item.gameObject);
                bool nearKey = false;
                foreach (var k in keep) {
                    float dx = Mathf.Max(b.min.x - k.x, 0, k.x - b.max.x), dz = Mathf.Max(b.min.z - k.z, 0, k.z - b.max.z);
                    if (dx * dx + dz * dz < clearRadius * clearRadius) { nearKey = true; break; }
                }
                bool inside = playable != null && playable.Sd(b.center.x, b.center.z) < 0f;
                bool solid = item.GetComponentInChildren<Collider>(true) != null;
                bool gap = false;
                if (!nearKey && inside && solid) {
                    // another solid close by, leaving a gap a character could get wedged in
                    var probe = b; probe.Expand(new Vector3(gapWidth * 2f, 0, gapWidth * 2f));
                    foreach (var h in Physics.OverlapBox(probe.center, new Vector3(probe.extents.x, Mathf.Max(1f, b.extents.y), probe.extents.z), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore)) {
                        if (h is TerrainCollider || h.transform.IsChildOf(item) || h.GetComponent<UnityEngine.Rendering.Volume>() != null) continue;
                        if (h.bounds.size.x > 150f && h.bounds.size.z > 150f) continue;
                        if (h.bounds.Intersects(b)) continue; // touching counts as one clump, not a gap
                        gap = true; break;
                    }
                }
                if (nearKey || gap) { Object.DestroyImmediate(item.gameObject); if (nearKey) removedObjects++; else gapRemoved++; }
            }
        }

        foreach (var t in terrains) {
            var td = t.terrainData; var o = t.transform.position;
            var protos = td.treePrototypes;
            var ours = new bool[protos.Length];
            for (int i = 0; i < protos.Length; i++) {
                var p = protos[i].prefab != null ? AssetDatabase.GetAssetPath(protos[i].prefab) : "";
                ours[i] = p.StartsWith("Assets/SoStylized") || p.StartsWith("Assets/ToonScapes");
            }
            var list = new List<TreeInstance>(); bool changed = false;
            foreach (var inst in td.treeInstances) {
                if (ours[inst.prototypeIndex]) {
                    float wx = o.x + inst.position.x * td.size.x, wz = o.z + inst.position.z * td.size.z;
                    bool near = false;
                    foreach (var k in keep) { float dx = k.x - wx, dz = k.z - wz; if (dx * dx + dz * dz < clearRadius * clearRadius) { near = true; break; } }
                    if (near) { removedTrees++; changed = true; continue; }
                }
                list.Add(inst);
            }
            if (changed) { td.SetTreeInstances(list.ToArray(), true); EditorUtility.SetDirty(td); }
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        return $"{scene.name}: key points {keep.Count}, removed near spawns/exits {removedObjects} objects + {removedTrees} trees, removed gap blockers {gapRemoved}";
    }

    // verifies every palette entry resolves to a prefab
    public static string ValidatePalettes () {
        var missing = new List<string>();
        foreach (Biome b in System.Enum.GetValues(typeof(Biome))) {
            var p = Get(b);
            foreach (var arr in new[] { p.trees, p.bushes, p.flowers, p.rocks, p.logs, p.landmarks, p.pathProps, p.pondPlants, p.ambient })
                foreach (var path in arr) if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) missing.Add(b + ":" + path);
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(SS + "Water/Prefabs/P_Water.prefab") == null) missing.Add("water");
        return missing.Count == 0 ? "all palette prefabs found" : string.Join("\n", missing.ToArray());
    }
}
