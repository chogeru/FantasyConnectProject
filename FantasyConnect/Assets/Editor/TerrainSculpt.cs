using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Adds relief to flat scene terrains without breaking gameplay:
//  - hills/mountains rise outside the playable boundary (the invisible "マップ壁" walls)
//  - gentle rolling ground inside, optionally a raised rim for boss arenas
//  - large structures keep their ground flat; small grounded props are re-seated on the new surface
//  - steep slopes get a rock layer, and tree belts are planted on the outer hills
public static class TerrainSculpt {
    public class Settings {
        public float innerAmplitude = 3f;
        public float innerWavelength = 90f;
        public float outerHeight = 35f;
        public float outerRamp = 70f;
        public float bowlHeight;
        public float bowlWidth = 30f;
        public float protectMargin = 3f;
        public float protectFalloff = 16f;
        public float maxStructureShift = 2f;
        public float rockSlope = 32f;
        public string rockLayerPath = "Assets/開発用アセット/マップアセット/AZURE Nature/Terrain Layers/Summer/AN_Sum_Rock.terrainlayer";
        public float talusAngle = 38f;
        public int erosionIterations = 14;
        // ecology planting
        public float ringTreesPerThousandSqm = 2.5f;
        public float innerTreesPerThousandSqm = 0.12f;
        public float understoryChance = 0.55f;
        public float rocksPerThousandSqm = 0.35f;
        public bool innerRocks = true;
        public bool naturalizeExisting = true;
        public string[] treeKeywords = { "Tree", "Broadleaf", "Pine", "Fir", "Birch", "Oak", "Spruce" };
        public string[] bushKeywords = { "Bush" };
        public string[] rockKeywords = { "Rock", "Stone" };
        public string[] extraTrees, extraBushes, extraRocks;
        public int seed = 1;
        // optional per-location multiplier for the inner relief (world x, z) -> scale, e.g. calmer near the start of a flight
        public System.Func<float, float, float> innerAmplitudeScale;
    }

    const string WallName = "マップ壁";
    static readonly string[] SkipRoots = { "Terrain", "Weather Controller", "Ambient Particles", "Height Fog Global", "URP Reflection Probe", "マップの壁" };
    static readonly string[] SnapKeywords = { "Tree", "Bush", "Grass", "Flower", "Plant", "Fern", "Rock", "Stone", "Mushroom", "Log", "Stump", "Branch", "Pine", "Birch", "Broadleaf", "Dead", "Wolf", "Bear", "Deer", "Enemy", "Player", "Spawn" };

    static readonly string[] NatureKeywords = { "Tree", "Bush", "Grass", "Flower", "Plant", "Fern", "Rock", "Stone", "Mushroom", "Log", "Stump", "Branch", "Pine", "Birch", "Broadleaf", "Dead", "Fir", "Spruce", "Oak", "Leaf", "Leaves" };

    class Unit {
        public Transform transform;
        public Bounds bounds;
        public bool point;
    }

    class Protect { public Rect rect; public float margin; public float falloff; public float pad; public Transform move; public bool pinned; }

    static List<Protect> lastProtects;
    static List<string> lastProtectNames;

    // Lists the protection zones from the last Apply that influence a point (for diagnosing flat spots).
    public static string ProtectsAt (float x, float z) {
        if (lastProtects == null) return "run Apply first";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < lastProtects.Count; i++) {
            var p = lastProtects[i];
            float dx = Mathf.Max(p.rect.xMin - x, 0, x - p.rect.xMax), dz = Mathf.Max(p.rect.yMin - z, 0, z - p.rect.yMax);
            if (Mathf.Sqrt(dx * dx + dz * dz) < p.margin + p.falloff) sb.Append(lastProtectNames[i] + " rect=" + p.rect + "; ");
        }
        return sb.Length == 0 ? "none" : sb.ToString();
    }

    // Adds TerrainGroundFollow to animated ground creatures (no physics body) so root motion / timeline walks hug the new relief.
    // Timeline-driven characters are handled by AdjustAnimations; every other animated creature keeps its placed height
    // above the ground (0 for walkers, hovering height for flyers) while it moves.
    public static string AddGroundFollowers () {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var terrains = Object.FindObjectsOfType<Terrain>();
        var timelineBound = new HashSet<GameObject>();
        foreach (var root in scene.GetRootGameObjects())
            foreach (var director in root.GetComponentsInChildren<UnityEngine.Playables.PlayableDirector>(true)) {
                var timeline = director.playableAsset as UnityEngine.Timeline.TimelineAsset;
                if (timeline == null) continue;
                foreach (var track in timeline.GetOutputTracks()) {
                    if (!(track is UnityEngine.Timeline.AnimationTrack)) continue;
                    var b = director.GetGenericBinding(track);
                    if (b is Component) timelineBound.Add(((Component)b).gameObject);
                    else if (b is GameObject) timelineBound.Add((GameObject)b);
                }
            }
        int added = 0, removed = 0;

        // timeline walkers: the timeline rewrites their pose every frame, so they need a hard snap (no smoothing).
        // Flyers are recognised by flying well above the ground somewhere along their path.
        foreach (var root in scene.GetRootGameObjects())
            foreach (var director in root.GetComponentsInChildren<UnityEngine.Playables.PlayableDirector>(true)) {
                var timeline = director.playableAsset as UnityEngine.Timeline.TimelineAsset;
                if (timeline == null) continue;
                var walkers = new Dictionary<Animator, float>();
                foreach (var track in timeline.GetOutputTracks()) {
                    var a = director.GetGenericBinding(track) as Animator;
                    if (a != null && track is UnityEngine.Timeline.AnimationTrack && a.GetComponentInChildren<SkinnedMeshRenderer>(true) != null
                        && a.GetComponentInChildren<Camera>(true) == null && !a.name.Contains("Player")) walkers[a] = float.MinValue;
                }
                if (walkers.Count == 0) continue;
                var keys = new List<Animator>(walkers.Keys);
                for (double t = 0; t < timeline.duration; t += 2) {
                    director.time = t; director.Evaluate();
                    foreach (var a in keys) {
                        var p = a.transform.position;
                        foreach (var te in terrains) {
                            var o = te.transform.position; var sz = te.terrainData.size;
                            if (p.x < o.x || p.x > o.x + sz.x || p.z < o.z || p.z > o.z + sz.z) continue;
                            walkers[a] = Mathf.Max(walkers[a], p.y - (te.SampleHeight(p) + o.y));
                            break;
                        }
                    }
                }
                director.time = 0; director.Evaluate();
                foreach (var kv in walkers) {
                    var existing = kv.Key.GetComponent<TerrainGroundFollow>();
                    if (kv.Value > 6f || kv.Value == float.MinValue) {
                        if (existing != null) { Undo.DestroyObjectImmediate(existing); removed++; }
                        continue;
                    }
                    var follow = existing != null ? existing : Undo.AddComponent<TerrainGroundFollow>(kv.Key.gameObject);
                    Undo.RecordObject(follow, "Ground Follow");
                    follow.heightOffset = 0f;
                    follow.smoothing = 0f;
                    if (existing == null) added++;
                }
            }

        foreach (var root in scene.GetRootGameObjects())
            foreach (var animator in root.GetComponentsInChildren<Animator>(true)) {
                if (timelineBound.Contains(animator.gameObject)) continue;
                var go = animator.gameObject;
                var existing = go.GetComponent<TerrainGroundFollow>();
                bool eligible = animator.runtimeAnimatorController != null && go.GetComponent<Rigidbody>() == null && go.GetComponent<CharacterController>() == null
                    && go.GetComponentInParent<Canvas>() == null && go.GetComponentInChildren<Camera>(true) == null && !go.name.Contains("Player")
                    // some creatures (e.g. split rock golems) are built from rigid mesh pieces rather than a skinned mesh
                    && !timelineBound.Contains(go) && (go.GetComponentInChildren<SkinnedMeshRenderer>(true) != null || go.GetComponentInChildren<MeshRenderer>(true) != null);
                if (!eligible) {
                    if (existing != null) { Undo.DestroyObjectImmediate(existing); removed++; }
                    continue;
                }
                float ground = float.NaN;
                var p = go.transform.position;
                foreach (var t in terrains) {
                    var o = t.transform.position; var sz = t.terrainData.size;
                    if (p.x >= o.x && p.x <= o.x + sz.x && p.z >= o.z && p.z <= o.z + sz.z) { ground = t.SampleHeight(p) + o.y; break; }
                }
                if (float.IsNaN(ground)) continue;
                var follow = existing != null ? existing : Undo.AddComponent<TerrainGroundFollow>(go);
                Undo.RecordObject(follow, "Ground Follow Offset");
                // small gaps come from seating a wide body on a slope: those creatures stand on the ground; larger gaps are hovering flyers
                float gap = p.y - ground;
                follow.heightOffset = Mathf.Abs(gap) < 6f ? 0f : gap;
                if (existing == null) added++;
            }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        return $"ground followers +{added}, removed {removed}";
    }

    // Restores a scene and its terrain data from git (HEAD) so the pipeline can be re-run from the original state.
    public static string ResetFromGit (string sceneName) { return ResetFromGit(sceneName, "HEAD"); }

    public static string ResetFromGit (string sceneName, string revision) {
        string scenePath = "Assets/シーン/" + sceneName + ".unity";
        var paths = new List<string> { scenePath };
        foreach (var dep in AssetDatabase.GetDependencies(scenePath, false))
            if (AssetDatabase.GetMainAssetTypeAtPath(dep) == typeof(TerrainData)) paths.Add(dep);
        // animations adjusted by a previous run must be restored too, or they would be shifted twice
        foreach (var dep in AssetDatabase.GetDependencies(scenePath, true))
            if ((dep.EndsWith(".playable") || dep.EndsWith(".anim")) && !paths.Contains(dep)) paths.Add(dep);
        UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
        foreach (var p in paths) { var a = AssetDatabase.LoadMainAssetAtPath(p); if (a != null && !(a is SceneAsset)) Resources.UnloadAsset(a); }
        EditorUtility.UnloadUnusedAssetsImmediate();
        var psi = new System.Diagnostics.ProcessStartInfo("git") {
            WorkingDirectory = System.IO.Directory.GetCurrentDirectory(), UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardError = true, RedirectStandardOutput = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8, StandardErrorEncoding = System.Text.Encoding.UTF8
        };
        psi.Arguments = "-c core.quotepath=false checkout " + revision + " -- " + string.Join(" ", paths.ConvertAll(p => "\"" + p + "\"").ToArray());
        string err;
        using (var proc = System.Diagnostics.Process.Start(psi)) { err = proc.StandardError.ReadToEnd(); proc.WaitForExit(); }
        foreach (var p in paths) AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
        return "reset " + string.Join(", ", paths.ToArray()) + (string.IsNullOrEmpty(err) ? "" : " ERR " + err);
    }

    // ---------- public entry ----------
    // Full pipeline for one scene: sculpt, regenerate ground cover for the new slopes, ecological planting, save.
    public static string Run (string sceneName, Settings s, System.Func<Terrain, VegetationScatter.Biome> biome) {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/シーン/" + sceneName + ".unity");
        var sb = new System.Text.StringBuilder(Apply(s)).Append('\n');
        foreach (var t in Object.FindObjectsOfType<Terrain>()) sb.Append(VegetationScatter.Apply(t, biome(t))).Append('\n');
        sb.Append(PlantEcology(s));
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return sb.ToString();
    }

    public static string Apply (Settings s) {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var terrains = Object.FindObjectsOfType<Terrain>();
        if (terrains.Length == 0) return scene.name + ": no terrain";

        System.Func<float, float, float> groundAt = (x, z) => {
            foreach (var t in terrains) {
                var o = t.transform.position; var sz = t.terrainData.size;
                if (x >= o.x && x <= o.x + sz.x && z >= o.z && z <= o.z + sz.z) return t.SampleHeight(new Vector3(x, 0, z)) + o.y;
            }
            return float.NaN;
        };

        // 1. collect scene units
        var units = new List<Unit>();
        foreach (var root in scene.GetRootGameObjects()) {
            if (System.Array.IndexOf(SkipRoots, root.name) >= 0) continue;
            Collect(root.transform, units);
        }

        var snaps = new List<Unit>();
        var protects = new List<Protect>();
        var waterLevels = new List<float>();
        var structureBounds = new List<Bounds>();
        foreach (var u in units) {
            float gy = groundAt(u.bounds.center.x, u.bounds.center.z);
            if (float.IsNaN(gy)) continue;
            float footprint = Mathf.Max(u.bounds.size.x, u.bounds.size.z);
            if (IsWater(u.transform)) {
                // a sea/lake plane under the whole map sets a floor that land must not sink below
                if (footprint > 250f) waterLevels.Add(u.bounds.max.y);
                else protects.Add(new Protect { rect = new Rect(u.bounds.min.x, u.bounds.min.z, u.bounds.size.x, u.bounds.size.z), margin = s.protectMargin, falloff = s.protectFalloff, pinned = true });
                continue;
            }
            if (footprint > 250f) continue;
            bool grounded = Mathf.Abs(u.bounds.min.y - gy) < 2.5f || (u.bounds.min.y < gy && u.bounds.max.y > gy);
            bool hovering = !grounded && u.bounds.min.y > gy && u.bounds.min.y - gy < 8f;
            bool snapName = MatchesAny(u.transform.name, SnapKeywords);
            // animated creatures are living things that follow the ground, not structures to build a flat pad for
            bool creature = u.transform.GetComponentInChildren<Animator>(true) != null && u.transform.GetComponentInParent<Canvas>() == null;
            var rect = new Rect(u.bounds.min.x, u.bounds.min.z, u.bounds.size.x, u.bounds.size.z);

            if (grounded && (snapName || creature || footprint <= 12f)) {
                snaps.Add(u);
                // plants, rocks and creatures simply follow the ground; man-made props get a small calm zone so they are not tilted on bumps
                if (!creature && !MatchesAny(u.transform.name, NatureKeywords)) protects.Add(new Protect { rect = rect, margin = 1f, falloff = 6f });
            } else if (hovering && footprint > 30f) {
                // large planes stay put, and so does the ground beneath them
                protects.Add(new Protect { rect = rect, margin = s.protectMargin, falloff = s.protectFalloff, pinned = true });
            } else if (grounded || hovering) {
                protects.Add(new Protect { rect = rect, margin = s.protectMargin, falloff = s.protectFalloff, move = u.transform });
                if (!u.point) structureBounds.Add(u.bounds);
            }
        }

        // 2. playable region: flood fill from the player/enemies, stopped by walls and cliffs
        string boundarySource;
        var boundary = BuildPlayable(scene, terrains, out boundarySource);
        if (boundary == null && structureBounds.Count > 0) {
            boundarySource = "structures";
            var b = structureBounds[0];
            foreach (var sb in structureBounds) b.Encapsulate(sb);
            b.Expand(new Vector3(30f, 0, 30f));
            boundary = Playable.FromRect(b, terrains);
        }
        lastPlayable = boundary;

        lastProtects = protects;
        lastProtectNames = new List<string>();
        foreach (var p in protects) lastProtectNames.Add(p.move != null ? p.move.name : (p.pinned ? "pinned" : "calm"));

        // snapshot the original surface so animations can later be moved by exactly how much the ground changed
        var originalHeights = new List<float[,]>();
        foreach (var t in terrains) { int r = t.terrainData.heightmapResolution; originalHeights.Add(t.terrainData.GetHeights(0, 0, r, r)); }
        System.Func<float, float, float> originalGroundAt = (x, z) => {
            for (int i = 0; i < terrains.Length; i++) {
                var t = terrains[i]; var o = t.transform.position; var sz = t.terrainData.size;
                if (x < o.x || x > o.x + sz.x || z < o.z || z > o.z + sz.z) continue;
                var hm = originalHeights[i]; int r = hm.GetLength(0);
                float gx = (x - o.x) / sz.x * (r - 1), gz = (z - o.z) / sz.z * (r - 1);
                int ix = Mathf.Min((int)gx, r - 2), iz = Mathf.Min((int)gz, r - 2);
                float fx = gx - ix, fz = gz - iz;
                float hh = Mathf.Lerp(Mathf.Lerp(hm[iz, ix], hm[iz, ix + 1], fx), Mathf.Lerp(hm[iz + 1, ix], hm[iz + 1, ix + 1], fx), fz);
                return hh * sz.y + o.y;
            }
            return float.NaN;
        };

        // record old ground under snapped units before changing heights
        var oldGround = new List<float[]>();
        foreach (var u in snaps) oldGround.Add(SampleFootprint(u.bounds, groundAt));

        // terrains whose floor sits at height 0 cannot be dug into, so their relief only rises
        float minBase = float.MaxValue;
        foreach (var t in terrains) {
            int r = t.terrainData.heightmapResolution;
            var hh = t.terrainData.GetHeights(0, 0, r, r);
            foreach (var v in hh) minBase = Mathf.Min(minBase, v * t.terrainData.size.y);
        }
        bool canDig = minBase > s.innerAmplitude + 0.5f;

        // each protected footprint moves as a flat block; inside the playable area keep it small so scripted spawns stay valid
        foreach (var p in protects) {
            var c = p.rect.center;
            float pad = Displacement(c.x, c.y, s, boundary, canDig);
            bool inside = boundary == null || boundary.Sd(c.x, c.y) < 0f;
            p.pad = p.pinned ? 0f : inside ? Mathf.Clamp(pad, -s.maxStructureShift, s.maxStructureShift) : pad;
        }

        // 3. sculpt each terrain
        var report = new System.Text.StringBuilder();
        var rockLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(s.rockLayerPath);
        float waterLevel = float.NegativeInfinity;
        foreach (var w in waterLevels) waterLevel = Mathf.Max(waterLevel, w);
        if (waterLevels.Count > 0) report.Append($"water floor {waterLevel:F2} | ");
        foreach (var t in terrains) report.Append(SculptTerrain(t, s, boundary, protects, rockLayer, canDig, waterLevel)).Append(" | ");

        report.Append(StitchSeams(terrains));
        report.Append(" | ").Append(AdjustAnimations(scene, originalGroundAt, groundAt));

        int shifted = 0;
        foreach (var p in protects) {
            if (p.move == null || Mathf.Abs(p.pad) < 0.01f) continue;
            Undo.RecordObject(p.move, "Terrain Sculpt Shift");
            p.move.position += new Vector3(0, p.pad, 0);
            PrefabUtility.RecordPrefabInstancePropertyModifications(p.move);
            shifted++;
        }

        // 4. re-seat snapped props on the new surface (use the smallest rise so nothing ends up floating)
        int moved = 0, naturalized = 0;
        var jitterRng = new System.Random(s.seed * 104729);
        Physics.SyncTransforms();
        for (int i = 0; i < snaps.Count; i++) {
            var tr0 = snaps[i].transform;
            if (s.naturalizeExisting && MatchesAny(tr0.name, s.treeKeywords) && !tr0.name.Contains("Bush")) {
                // hand-placed trees sit on a visible grid with identical size/rotation: loosen that up
                var b = snaps[i].bounds;
                for (int tries = 0; tries < 4; tries++) {
                    float ang = (float)(jitterRng.NextDouble() * Mathf.PI * 2), dist = Mathf.Lerp(1.5f, 6f, (float)jitterRng.NextDouble());
                    var offset = new Vector3(Mathf.Cos(ang) * dist, 0, Mathf.Sin(ang) * dist);
                    var nb = new Bounds(b.center + offset, b.size);
                    var probe = new Vector3(nb.center.x, groundAt(nb.center.x, nb.center.z) + 2f, nb.center.z);
                    if (float.IsNaN(probe.y)) continue;
                    bool hit = false;
                    foreach (var c in Physics.OverlapBox(probe, new Vector3(1.2f, 1.5f, 1.2f), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
                        if (!(c is TerrainCollider) && !c.transform.IsChildOf(tr0)) { hit = true; break; }
                    if (hit) continue;
                    Undo.RecordObject(tr0, "Naturalize Tree");
                    tr0.position += offset;
                    // its height still matches the old spot's original ground, so compare against the new spot's current ground
                    snaps[i].bounds = nb;
                    break;
                }
                Undo.RecordObject(tr0, "Naturalize Tree");
                tr0.rotation = Quaternion.Euler(0, (float)(jitterRng.NextDouble() * 360.0), 0) * tr0.rotation;
                tr0.localScale *= Mathf.Lerp(0.8f, 1.25f, (float)jitterRng.NextDouble());
                PrefabUtility.RecordPrefabInstancePropertyModifications(tr0);
                naturalized++;
            }
            var now = SampleFootprint(snaps[i].bounds, groundAt);
            float delta = float.MaxValue;
            for (int k = 0; k < now.Length; k++) {
                if (float.IsNaN(now[k]) || float.IsNaN(oldGround[i][k])) continue;
                delta = Mathf.Min(delta, now[k] - oldGround[i][k]);
            }
            if (delta == float.MaxValue || Mathf.Abs(delta) < 0.01f) continue;
            var tr = snaps[i].transform;
            Undo.RecordObject(tr, "Terrain Sculpt Snap");
            tr.position += new Vector3(0, delta, 0);
            PrefabUtility.RecordPrefabInstancePropertyModifications(tr);
            moved++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        return $"{scene.name} [{boundarySource}] zones {protects.Count}, shifted {shifted}, snapped {moved}/{snaps.Count}, naturalized trees {naturalized} :: {report}";
    }

    // Ecological planting, run after VegetationScatter:
    //  - fertility = hollows/valleys (collect water) + large-scale grove noise; ridges and steep ground stay sparse
    //  - trees grow in groves, denser and larger on the outer hills, only occasional groves inside the playable area
    //  - understory bushes gather around tree bases, rocks sit on slopes and rocky ground
    public static string PlantEcology (Settings s) {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Physics.SyncTransforms();
        string src;
        // prefer the region computed by Apply (includes the structure fallback); re-evaluate when run on its own
        var boundary = lastPlayable ?? BuildPlayable(scene, Object.FindObjectsOfType<Terrain>(), out src);
        var sbOut = new System.Text.StringBuilder();
        foreach (var t in Object.FindObjectsOfType<Terrain>()) {
            var td = t.terrainData;
            Undo.RegisterCompleteObjectUndo(td, "Ecology Planting");
            var prototypes = new List<TreePrototype>(td.treePrototypes);
            var trees = PrototypeIndices(prototypes, s.extraTrees, s.treeKeywords, s.bushKeywords);
            var bushes = PrototypeIndices(prototypes, s.extraBushes, s.bushKeywords, null);
            var rocks = PrototypeIndices(prototypes, s.extraRocks, s.rockKeywords, null);
            td.treePrototypes = prototypes.ToArray();

            var o = t.transform.position; var size = td.size;
            var rng = new System.Random(s.seed * 7919 + t.name.GetHashCode());
            var list = new List<TreeInstance>(td.treeInstances);
            int aRes = td.alphamapResolution;
            var alpha = td.GetAlphamaps(0, 0, aRes, aRes);
            var layers = td.terrainLayers;
            var bare = new bool[layers.Length];
            int rockIndex = RockLayerIndex(td);
            for (int i = 0; i < layers.Length; i++) {
                string n = layers[i] != null ? layers[i].name : "";
                bare[i] = MatchesAny(n, new[] { "Ground", "Path", "Mud", "Soil", "Dirt", "Sand", "Tile", "Rock", "Ice" });
            }
            System.Func<float, float, float> bareAt = (u, v) => {
                int ax = Mathf.Clamp((int)(u * (aRes - 1)), 0, aRes - 1), ay = Mathf.Clamp((int)(v * (aRes - 1)), 0, aRes - 1);
                float b = 0f; for (int i = 0; i < bare.Length; i++) if (bare[i]) b += alpha[ay, ax, i];
                return b;
            };
            System.Func<float, float, float> rockAt = (u, v) => rockIndex < 0 ? 0f :
                alpha[Mathf.Clamp((int)(v * (aRes - 1)), 0, aRes - 1), Mathf.Clamp((int)(u * (aRes - 1)), 0, aRes - 1), rockIndex];
            System.Func<float, float, float> fertility = (u, v) => {
                float wx = o.x + u * size.x, wz = o.z + v * size.z;
                float hc = td.GetInterpolatedHeight(u, v);
                float ring = 0f; const float r = 14f;
                for (int k = 0; k < 6; k++) {
                    float a = k * Mathf.PI / 3f;
                    ring += td.GetInterpolatedHeight(Mathf.Clamp01(u + Mathf.Cos(a) * r / size.x), Mathf.Clamp01(v + Mathf.Sin(a) * r / size.z));
                }
                float hollow = Mathf.Clamp((ring / 6f - hc) / 3f, -1f, 1f);          // + in valleys, - on ridges
                float grove = Spread(Fbm(wx / 110f + s.seed * 4.4f, wz / 110f + s.seed * 1.9f, 3));  // patchy woodland
                return Mathf.Clamp01(0.62f * grove + 0.38f * (hollow * 0.5f + 0.5f));
            };
            System.Func<Vector3, float, bool> blocked = (world, radius) => {
                var hits = Physics.OverlapBox(world + Vector3.up * 2f, new Vector3(radius, 1.8f, radius), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
                foreach (var hh in hits) if (!(hh is TerrainCollider)) return true;
                return false;
            };

            // spatial hash of instances for spacing tests
            const float hashCell = 4f;
            var hash = new Dictionary<long, List<Vector2>>();
            System.Action<Vector2> addHash = p => {
                long kk = ((long)Mathf.FloorToInt(p.x / hashCell) << 32) ^ (uint)Mathf.FloorToInt(p.y / hashCell);
                List<Vector2> l; if (!hash.TryGetValue(kk, out l)) hash[kk] = l = new List<Vector2>(); l.Add(p);
            };
            System.Func<Vector2, float, bool> crowded = (p, spacing) => {
                int cx = Mathf.FloorToInt(p.x / hashCell), cz = Mathf.FloorToInt(p.y / hashCell), rr = Mathf.CeilToInt(spacing / hashCell);
                for (int dz = -rr; dz <= rr; dz++)
                    for (int dx = -rr; dx <= rr; dx++) {
                        List<Vector2> l; if (!hash.TryGetValue(((long)(cx + dx) << 32) ^ (uint)(cz + dz), out l)) continue;
                        foreach (var q in l) if ((q - p).sqrMagnitude < spacing * spacing) return true;
                    }
                return false;
            };
            // naturalize hand-painted trees: they were stamped on a regular grid with uniform size
            int thinned = 0;
            if (s.naturalizeExisting) {
                var treeSet = new HashSet<int>(trees);
                var natural = new List<TreeInstance>();
                foreach (var inst in list) {
                    if (!treeSet.Contains(inst.prototypeIndex)) { natural.Add(inst); continue; }
                    var ti = inst;
                    float f0 = fertility(ti.position.x, ti.position.z);
                    // open up clearings on poor ground
                    if (f0 < 0.42f && rng.NextDouble() < 0.55) { thinned++; continue; }
                    for (int tries = 0; tries < 4; tries++) {
                        float ang = (float)(rng.NextDouble() * Mathf.PI * 2), dist = Mathf.Lerp(1f, 4f, (float)rng.NextDouble());
                        float nu = ti.position.x + Mathf.Cos(ang) * dist / size.x, nv = ti.position.z + Mathf.Sin(ang) * dist / size.z;
                        if (nu < 0 || nu > 1 || nv < 0 || nv > 1 || bareAt(nu, nv) > 0.3f || td.GetSteepness(nu, nv) > 30f) continue;
                        if (blocked(new Vector3(o.x + nu * size.x, td.GetInterpolatedHeight(nu, nv) + o.y, o.z + nv * size.z), 1.5f)) continue;
                        ti.position = new Vector3(nu, td.GetInterpolatedHeight(nu, nv) / size.y, nv);
                        break;
                    }
                    float sc = Mathf.Lerp(0.75f, 1.3f, Mathf.Clamp01(f0 * 0.6f + (float)rng.NextDouble() * 0.5f));
                    ti.widthScale = ti.widthScale * sc; ti.heightScale = ti.heightScale * sc * Mathf.Lerp(0.93f, 1.07f, (float)rng.NextDouble());
                    ti.rotation = (float)(rng.NextDouble() * Mathf.PI * 2);
                    natural.Add(ti);
                }
                list = natural;
            }
            foreach (var inst in list) addHash(new Vector2(o.x + inst.position.x * size.x, o.z + inst.position.z * size.z));

            System.Action<int, float, float, float> place = (proto, u, v, scale) => {
                list.Add(new TreeInstance {
                    position = new Vector3(u, td.GetInterpolatedHeight(u, v) / size.y, v),
                    prototypeIndex = proto, widthScale = scale, heightScale = scale * Mathf.Lerp(0.92f, 1.08f, (float)rng.NextDouble()),
                    rotation = (float)(rng.NextDouble() * Mathf.PI * 2), color = Color.white, lightmapColor = Color.white
                });
                addHash(new Vector2(o.x + u * size.x, o.z + v * size.z));
            };

            int addedTrees = 0, addedBushes = 0, addedRocks = 0;
            float area = size.x * size.z / 1000f;
            float maxDensity = Mathf.Max(s.ringTreesPerThousandSqm, s.innerTreesPerThousandSqm, 0.0001f);
            if (trees.Count > 0) {
                int budget = Mathf.Min(4000, Mathf.RoundToInt(area * maxDensity));
                var parents = new List<Vector2>();
                for (int a = 0; a < budget * 10 && addedTrees < budget; a++) {
                    float u, v;
                    bool seedling = parents.Count > 0 && rng.NextDouble() < 0.6;
                    if (seedling) {
                        // seeds fall near existing trees: forests grow as clumps with clearings between them
                        var parent = parents[rng.Next(parents.Count)];
                        float ang = (float)(rng.NextDouble() * Mathf.PI * 2), dist = Mathf.Lerp(4f, 12f, (float)rng.NextDouble());
                        u = parent.x + Mathf.Cos(ang) * dist / size.x; v = parent.y + Mathf.Sin(ang) * dist / size.z;
                        if (u < 0f || u > 1f || v < 0f || v > 1f) continue;
                    } else { u = (float)rng.NextDouble(); v = (float)rng.NextDouble(); }
                    float wx = o.x + u * size.x, wz = o.z + v * size.z;
                    float sd = boundary != null ? boundary.Sd(wx, wz) : -999f;
                    bool outside = sd > 6f;
                    float density = outside ? s.ringTreesPerThousandSqm : (sd > -8f ? 0f : s.innerTreesPerThousandSqm);
                    if (density <= 0f) continue;
                    float steep = td.GetSteepness(u, v);
                    if (steep > 30f || rockAt(u, v) > 0.35f || bareAt(u, v) > 0.35f) continue;
                    float f = fertility(u, v);
                    // ridges and steep ground reject most candidates; inside the playable area only the most fertile spots grow groves
                    float accept = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(outside ? 0.5f : 0.66f, outside ? 0.72f : 0.85f, f)) * (1f - steep / 34f);
                    if (seedling) accept = Mathf.Min(1f, accept * 1.8f);
                    if ((float)rng.NextDouble() > accept * density / maxDensity) continue;
                    float spacing = Mathf.Lerp(8f, 3.8f, f) * Mathf.Lerp(0.8f, 1.25f, (float)rng.NextDouble());
                    if (crowded(new Vector2(wx, wz), spacing)) continue;
                    if (!outside && blocked(new Vector3(wx, td.GetInterpolatedHeight(u, v) + o.y, wz), 2.5f)) continue;
                    float scale = Mathf.Lerp(0.75f, 1.35f, f) * Mathf.Lerp(0.85f, 1.15f, (float)rng.NextDouble());
                    place(trees[rng.Next(trees.Count)], u, v, scale);
                    parents.Add(new Vector2(u, v));
                    addedTrees++;

                    // understory around the trunk
                    if (bushes.Count > 0 && rng.NextDouble() < s.understoryChance * (0.5f + f)) {
                        int nb = 1 + rng.Next(2);
                        for (int b = 0; b < nb; b++) {
                            float ang = (float)(rng.NextDouble() * Mathf.PI * 2), dist = Mathf.Lerp(2.2f, 5f, (float)rng.NextDouble());
                            float bu = u + Mathf.Cos(ang) * dist / size.x, bv = v + Mathf.Sin(ang) * dist / size.z;
                            if (bu < 0 || bu > 1 || bv < 0 || bv > 1 || td.GetSteepness(bu, bv) > 30f || bareAt(bu, bv) > 0.4f) continue;
                            var bw = new Vector2(o.x + bu * size.x, o.z + bv * size.z);
                            if (crowded(bw, 1.8f)) continue;
                            if (!outside && blocked(new Vector3(bw.x, td.GetInterpolatedHeight(bu, bv) + o.y, bw.y), 1f)) continue;
                            place(bushes[rng.Next(bushes.Count)], bu, bv, Mathf.Lerp(0.7f, 1.2f, (float)rng.NextDouble()));
                            addedBushes++;
                        }
                    }
                }
            }

            if (rocks.Count > 0 && s.rocksPerThousandSqm > 0f) {
                int budget = Mathf.RoundToInt(area * s.rocksPerThousandSqm);
                for (int a = 0; a < budget * 12 && addedRocks < budget; a++) {
                    float u = (float)rng.NextDouble(), v = (float)rng.NextDouble();
                    float steep = td.GetSteepness(u, v);
                    // rocks surface where soil is thin: slopes and the edges of rocky ground
                    float rocky = Mathf.Max(Mathf.InverseLerp(14f, 34f, steep), rockAt(u, v));
                    if (steep > 42f || rng.NextDouble() > rocky) continue;
                    float wx = o.x + u * size.x, wz = o.z + v * size.z;
                    bool outside = boundary == null || boundary.Sd(wx, wz) > 4f;
                    if (!outside && !s.innerRocks) continue;
                    if (!outside && blocked(new Vector3(wx, td.GetInterpolatedHeight(u, v) + o.y, wz), 1.5f)) continue;
                    if (crowded(new Vector2(wx, wz), 5f)) continue;
                    place(rocks[rng.Next(rocks.Count)], u, v, Mathf.Lerp(0.6f, 1.8f, Mathf.Pow((float)rng.NextDouble(), 2f)));
                    addedRocks++;
                }
            }

            td.SetTreeInstances(list.ToArray(), true);
            t.treeDistance = Mathf.Max(t.treeDistance, 400f);
            t.treeBillboardDistance = Mathf.Max(t.treeBillboardDistance, 60f);
            EditorUtility.SetDirty(td);
            sbOut.Append($"{t.name}: thinned {thinned}, trees +{addedTrees}, understory +{addedBushes}, rocks +{addedRocks}; ");
        }
        return scene.name + ": " + sbOut;
    }

    static List<int> PrototypeIndices (List<TreePrototype> prototypes, string[] extraPrefabs, string[] include, string[] exclude) {
        if (extraPrefabs != null)
            foreach (var path in extraPrefabs) {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null || prototypes.Exists(p => p.prefab == prefab)) continue;
                prototypes.Add(new TreePrototype { prefab = prefab, bendFactor = 0f });
                foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
                    foreach (var m in r.sharedMaterials) if (m != null && !m.enableInstancing) { m.enableInstancing = true; EditorUtility.SetDirty(m); }
            }
        var result = new List<int>();
        for (int i = 0; i < prototypes.Count; i++) {
            var pf = prototypes[i].prefab;
            if (pf == null || !MatchesAny(pf.name, include)) continue;
            // HIGHLANDS bushes were retired (teal palette clashes with the AZURE terrains) and get stripped by VegetationScatter
            if (AssetDatabase.GetAssetPath(pf).Contains("HIGHLANDS") && pf.name.StartsWith("Bush_")) continue;
            if (exclude != null && MatchesAny(pf.name, exclude)) continue;
            bool rockName = pf.name.Contains("Rock") || pf.name.Contains("Stone");
            if (!rockName && (pf.name.Contains("Grass") || pf.name.Contains("Flower"))) continue;
            result.Add(i);
        }
        return result;
    }

    // ---------- sculpting ----------
    static float Displacement (float wx0, float wz0, Settings s, Playable boundary, bool canDig) {
        // domain warp breaks up the blobby look of plain Perlin noise
        float wx = wx0 + 30f * (Fbm(wx0 / 220f + 5.1f, wz0 / 220f + s.seed, 2) - 0.5f) * 2f;
        float wz = wz0 + 30f * (Fbm(wx0 / 220f + 41.7f, wz0 / 220f + s.seed * 2.3f, 2) - 0.5f) * 2f;
        // broad rolls + medium swells (no small lumps: they read as molehills)
        float n = 0.7f * Fbm(wx / s.innerWavelength + s.seed * 17.1f, wz / s.innerWavelength + s.seed * 5.3f, 3)
                + 0.3f * Fbm(wx / (s.innerWavelength * 0.45f) + 3.3f, wz / (s.innerWavelength * 0.45f) + 8.8f, 2);
        // fBm values cluster tightly around 0.5 (std ~0.1), so stretch them to use the full amplitude with a soft limit
        float k = Mathf.Clamp((n - 0.5f) / 0.16f, -1.6f, 1.6f);
        k = k * (1f - 0.22f * Mathf.Abs(k));
        float amplitude = s.innerAmplitude * (s.innerAmplitudeScale != null ? s.innerAmplitudeScale(wx0, wz0) : 1f);
        float disp = canDig ? amplitude * k : amplitude * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.6f, 1f, k));
        if (boundary == null) return disp;

        float sd = boundary.Sd(wx0, wz0);
        if (s.bowlHeight > 0f) disp += s.bowlHeight * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-s.bowlWidth, 0f, sd));
        if (s.outerHeight > 0f) {
            // where the rise starts, how steep it is and how high it gets all wander along the boundary,
            // so the hills do not read as a uniform embankment parallel to the walls
            float start = 35f * Spread(Fbm(wx / 180f + 11.1f, wz / 180f + s.seed * 3.7f, 2));
            float d = sd - start;
            if (d > 0f) {
                float rampLen = s.outerRamp * Mathf.Lerp(0.6f, 1.6f, Spread(Fbm(wx / 240f + 21.9f, wz / 240f + 4.2f, 2)));
                float t = Mathf.Clamp01(d / rampLen);
                float ramp = t * t * (3f - 2f * t);
                float heightVar = Mathf.Lerp(0.2f, 1.1f, Mathf.SmoothStep(0f, 1f, Spread(Fbm(wx / 200f + s.seed * 2.7f, wz / 200f + s.seed * 9.1f, 3))));
                float ridge = 1f - Mathf.Abs(2f * Spread(Fbm(wx / 110f + 31.3f, wz / 110f + 12.7f, 3)) - 1f);
                disp += s.outerHeight * ramp * heightVar * (0.8f + 0.3f * ridge * ridge);
            }
        }
        return disp;
    }

    static bool IsWater (Transform t) {
        string[] keys = { "water", "水", "river", "ocean", "lake", "foam", "waterfall" };
        if (MatchesAny(t.name, keys)) return true;
        foreach (var r in t.GetComponentsInChildren<Renderer>(true)) {
            if (r is ParticleSystemRenderer) continue;
            var m = r.sharedMaterial;
            if (m != null && (MatchesAny(m.name, keys) || MatchesAny(m.shader.name, keys))) return true;
        }
        return false;
    }

    static string SculptTerrain (Terrain t, Settings s, Playable boundary, List<Protect> protects, TerrainLayer rockLayer, bool canDig, float waterLevel) {
        var td = t.terrainData;
        Undo.RegisterCompleteObjectUndo(td, "Terrain Sculpt");
        int res = td.heightmapResolution;
        var o = t.transform.position; var size = td.size;
        float[,] h = td.GetHeights(0, 0, res, res);
        float cell = size.x / (res - 1);

        // protection mask
        var protect = new float[res, res];
        var padGrid = new float[res, res];
        foreach (var p in protects) {
            float reach = p.margin + p.falloff;
            int x0 = Mathf.Clamp(Mathf.FloorToInt((p.rect.xMin - reach - o.x) / cell), 0, res - 1);
            int x1 = Mathf.Clamp(Mathf.CeilToInt((p.rect.xMax + reach - o.x) / cell), 0, res - 1);
            int z0 = Mathf.Clamp(Mathf.FloorToInt((p.rect.yMin - reach - o.z) / cell), 0, res - 1);
            int z1 = Mathf.Clamp(Mathf.CeilToInt((p.rect.yMax + reach - o.z) / cell), 0, res - 1);
            if (x0 >= x1 || z0 >= z1) continue;
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++) {
                    float wx = o.x + x * cell, wz = o.z + z * cell;
                    float dx = Mathf.Max(p.rect.xMin - wx, 0, wx - p.rect.xMax);
                    float dz = Mathf.Max(p.rect.yMin - wz, 0, wz - p.rect.yMax);
                    float d = Mathf.Sqrt(dx * dx + dz * dz);
                    float w = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(p.margin, reach, d));
                    if (w > protect[z, x]) { protect[z, x] = w; padGrid[z, x] = p.pad; }
                }
        }

        float maxRise = 0f, maxDig = 0f;
        var baseH = new float[res, res];
        var dispGrid = new float[res, res];
        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
                dispGrid[z, x] = Displacement(o.x + x * cell, o.z + z * cell, s, boundary, canDig);
        // soften creases from the distance field corners
        Blur(dispGrid, Mathf.Max(1, Mathf.RoundToInt(8f / cell)), 2);
        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
                // protected ground keeps a flat pad that moves as one block with its structure
                dispGrid[z, x] = Mathf.Lerp(dispGrid[z, x], padGrid[z, x], protect[z, x]);
        // blend pad edges into the surroundings
        Blur(dispGrid, Mathf.Max(1, Mathf.RoundToInt(3f / cell)), 1, protect);
        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++) {
                baseH[z, x] = h[z, x] * size.y;
                h[z, x] = Mathf.Clamp(baseH[z, x] + dispGrid[z, x], 0f, size.y - 1f);
            }

        // thermal erosion: material slides off slopes steeper than the talus angle, giving natural scree/fans
        // (protected ground is pinned so structures keep their footing)
        float talus = Mathf.Tan(s.talusAngle * Mathf.Deg2Rad) * cell;
        for (int it = 0; it < s.erosionIterations; it++)
            for (int z = 1; z < res - 1; z++)
                for (int x = 1; x < res - 1; x++) {
                    float free = 1f - protect[z, x];
                    if (free <= 0.01f) continue;
                    float hc = h[z, x];
                    int bx = 0, bz = 0; float maxDiff = 0f;
                    for (int k = 0; k < 4; k++) {
                        int nx = x + (k == 0 ? 1 : k == 1 ? -1 : 0), nz = z + (k == 2 ? 1 : k == 3 ? -1 : 0);
                        float diff = hc - h[nz, nx];
                        if (diff > maxDiff) { maxDiff = diff; bx = nx; bz = nz; }
                    }
                    if (maxDiff <= talus) continue;
                    float move = (maxDiff - talus) * 0.35f * Mathf.Min(free, 1f - protect[bz, bx]);
                    h[z, x] -= move; h[bz, bx] += move;
                }

        float floorLocal = waterLevel - o.y;
        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++) {
                // land that was above the water plane must stay above it
                if (baseH[z, x] > floorLocal) h[z, x] = Mathf.Max(h[z, x], Mathf.Min(baseH[z, x], floorLocal + 0.3f));
                float applied = h[z, x] - baseH[z, x];
                maxRise = Mathf.Max(maxRise, applied); maxDig = Mathf.Min(maxDig, applied);
                h[z, x] = Mathf.Clamp(h[z, x], 0f, size.y - 1f) / size.y;
            }
        td.SetHeights(0, 0, h);

        // keep tree instances on the new surface
        var trees = td.treeInstances;
        for (int i = 0; i < trees.Length; i++) trees[i].position.y = td.GetInterpolatedHeight(trees[i].position.x, trees[i].position.z) / size.y;
        td.SetTreeInstances(trees, true);

        string rock = PaintSlopes(td, s, rockLayer);
        EditorUtility.SetDirty(td);
        return $"{t.name}: +{maxRise:F1}m/{maxDig:F1}m {rock}";
    }

    static string PaintSlopes (TerrainData td, Settings s, TerrainLayer rockLayer) {
        int rockIndex = RockLayerIndex(td);
        if (rockIndex < 0) {
            if (rockLayer == null) return "no rock layer";
            var layers = new List<TerrainLayer>(td.terrainLayers) { rockLayer };
            // adding a layer resizes the splat maps; existing weights are preserved by Unity
            td.terrainLayers = layers.ToArray();
            rockIndex = layers.Count - 1;
        }
        int aRes = td.alphamapResolution;
        var alpha = td.GetAlphamaps(0, 0, aRes, aRes);
        int layerCount = alpha.GetLength(2);
        int painted = 0;
        for (int y = 0; y < aRes; y++)
            for (int x = 0; x < aRes; x++) {
                float steep = td.GetSteepness((x + 0.5f) / aRes, (y + 0.5f) / aRes);
                float w = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(s.rockSlope - 6f, s.rockSlope + 8f, steep));
                if (w <= alpha[y, x, rockIndex]) continue;
                float others = 1f - alpha[y, x, rockIndex];
                float scale = others > 0.0001f ? (1f - w) / others : 0f;
                for (int l = 0; l < layerCount; l++) if (l != rockIndex) alpha[y, x, l] *= scale;
                alpha[y, x, rockIndex] = w;
                painted++;
            }
        td.SetAlphamaps(0, 0, alpha);
        return $"rock texels {painted}";
    }

    static int RockLayerIndex (TerrainData td) {
        var layers = td.terrainLayers;
        for (int i = 0; i < layers.Length; i++) if (layers[i] != null && (layers[i].name.Contains("Rock") || layers[i].name.Contains("Cliff"))) return i;
        return -1;
    }

    // ---------- playable region ----------
    // Signed distance field on a 2m grid over the terrains: negative inside the walkable region, positive outside.
    public class Playable {
        public float originX, originZ, cell;
        public int w, h;
        public float[] sd;
        public float insideRatio;

        public float Sd (float x, float z) {
            float gx = (x - originX) / cell, gz = (z - originZ) / cell;
            if (gx < 0 || gz < 0 || gx > w - 1 || gz > h - 1) return 1000f;
            int ix = Mathf.Min((int)gx, w - 2), iz = Mathf.Min((int)gz, h - 2);
            float fx = gx - ix, fz = gz - iz;
            float a = sd[iz * w + ix], b = sd[iz * w + ix + 1], c = sd[(iz + 1) * w + ix], d = sd[(iz + 1) * w + ix + 1];
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fz);
        }

        public static Playable Grid (Terrain[] terrains) {
            var b = new Bounds(terrains[0].transform.position, Vector3.zero);
            foreach (var t in terrains) { b.Encapsulate(t.transform.position); b.Encapsulate(t.transform.position + t.terrainData.size); }
            var p = new Playable { originX = b.min.x, originZ = b.min.z, cell = 2f };
            p.w = Mathf.CeilToInt(b.size.x / p.cell) + 1;
            p.h = Mathf.CeilToInt(b.size.z / p.cell) + 1;
            p.sd = new float[p.w * p.h];
            return p;
        }

        public static Playable FromRect (Bounds rect, Terrain[] terrains) {
            var p = Grid(terrains);
            var inside = new bool[p.w * p.h];
            for (int z = 0; z < p.h; z++)
                for (int x = 0; x < p.w; x++) {
                    float wx = p.originX + x * p.cell, wz = p.originZ + z * p.cell;
                    inside[z * p.w + x] = wx >= rect.min.x && wx <= rect.max.x && wz >= rect.min.z && wz <= rect.max.z;
                }
            p.Build(inside);
            return p;
        }

        public void Build (bool[] inside) {
            var dIn = Chamfer(inside, false);   // distance from outside cells to the region
            var dOut = Chamfer(inside, true);   // distance from inside cells to the outside
            int count = 0;
            for (int i = 0; i < sd.Length; i++) { sd[i] = inside[i] ? -dOut[i] : dIn[i]; if (inside[i]) count++; }
            insideRatio = (float)count / sd.Length;
        }

        float[] Chamfer (bool[] inside, bool invert) {
            const float INF = 1e6f;
            var d = new float[w * h];
            for (int i = 0; i < d.Length; i++) d[i] = (inside[i] != invert) ? 0f : INF;
            float s = cell, dg = cell * 1.4142f;
            for (int z = 0; z < h; z++)
                for (int x = 0; x < w; x++) {
                    int i = z * w + x; float v = d[i];
                    if (x > 0) v = Mathf.Min(v, d[i - 1] + s);
                    if (z > 0) { v = Mathf.Min(v, d[i - w] + s); if (x > 0) v = Mathf.Min(v, d[i - w - 1] + dg); if (x < w - 1) v = Mathf.Min(v, d[i - w + 1] + dg); }
                    d[i] = v;
                }
            for (int z = h - 1; z >= 0; z--)
                for (int x = w - 1; x >= 0; x--) {
                    int i = z * w + x; float v = d[i];
                    if (x < w - 1) v = Mathf.Min(v, d[i + 1] + s);
                    if (z < h - 1) { v = Mathf.Min(v, d[i + w] + s); if (x < w - 1) v = Mathf.Min(v, d[i + w + 1] + dg); if (x > 0) v = Mathf.Min(v, d[i + w - 1] + dg); }
                    d[i] = v;
                }
            return d;
        }
    }

    // corridor-style maps (a walled path through decoration) legitimately cover only a small share of the terrain
    public static float MinRegionRatio = 0.05f;
    // Playable region for the active scene; falls back to the extent of solid colliders (towns without walls).
    public static Playable ComputePlayable (out string source) {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var terrains = Object.FindObjectsOfType<Terrain>();
        var p = BuildPlayable(scene, terrains, out source);
        if (p != null) return p;
        bool any = false; var b = new Bounds();
        foreach (var root in scene.GetRootGameObjects())
            foreach (var c in root.GetComponentsInChildren<Collider>(true)) {
                if (c is TerrainCollider || c.isTrigger || c.name.StartsWith(WallName)) continue;
                var s = c.bounds.size;
                if (Mathf.Max(s.x, s.z) > 250f || Mathf.Max(s.x, s.z) < 2f) continue;
                if (!any) { b = c.bounds; any = true; } else b.Encapsulate(c.bounds);
            }
        if (!any) return null;
        b.Expand(new Vector3(60f, 0, 60f));
        source = "structures";
        return Playable.FromRect(b, terrains);
    }

    static Playable lastPlayable, lastDebug;
    static bool[] lastBlocked;

    // Writes the playable region as an image (green = inside, red = outside, darker = farther from the edge) for inspection.
    public static string DebugPlayable (string pngPath) {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string src;
        var p = BuildPlayable(scene, Object.FindObjectsOfType<Terrain>(), out src);
        lastPlayable = p;
        if (p == null) p = lastDebug;
        if (p == null) return "no region: " + src;
        var tex = new Texture2D(p.w, p.h, TextureFormat.RGB24, false);
        for (int z = 0; z < p.h; z++)
            for (int x = 0; x < p.w; x++) {
                float d = p.sd[z * p.w + x];
                float k = 1f - Mathf.Clamp01(Mathf.Abs(d) / 80f) * 0.7f;
                bool barrier = lastBlocked != null && lastBlocked.Length == p.sd.Length && lastBlocked[z * p.w + x];
                tex.SetPixel(x, z, barrier ? Color.cyan : d < 0 ? new Color(0.1f, k, 0.1f) : new Color(k, 0.1f, 0.1f));
            }
        System.IO.File.WriteAllBytes(pngPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        return src;
    }

    static Playable BuildPlayable (UnityEngine.SceneManagement.Scene scene, Terrain[] terrains, out string source) {
        source = "none";
        Physics.SyncTransforms();
        var p = Playable.Grid(terrains);
        int n = p.w * p.h;

        System.Func<float, float, float> groundAt = (x, z) => {
            foreach (var t in terrains) {
                var o = t.transform.position; var sz = t.terrainData.size;
                if (x >= o.x && x <= o.x + sz.x && z >= o.z && z <= o.z + sz.z) return t.SampleHeight(new Vector3(x, 0, z)) + o.y;
            }
            return float.NaN;
        };

        // barriers: invisible walls and big solid colliders (cliffs, large rocks, buildings)
        var barrierColliders = new HashSet<Collider>();
        var seeds = new List<Vector3>();
        foreach (var root in scene.GetRootGameObjects()) {
            foreach (var c in root.GetComponentsInChildren<Collider>(true)) {
                if (c is TerrainCollider || c.isTrigger || !c.enabled || !c.gameObject.activeInHierarchy) continue;
                var bb = c.bounds;
                bool wall = c.name.StartsWith(WallName);
                if (wall || (Mathf.Max(bb.size.x, bb.size.z) >= 15f && bb.size.y >= 4f && Mathf.Max(bb.size.x, bb.size.z) < 400f)) barrierColliders.Add(c);
            }
            if (root.name.Contains("Player")) seeds.Add(root.transform.position);
            if (root.name == "敵" || root.name == "動物")
                foreach (Transform ch in root.transform) seeds.Add(ch.position);
        }
        if (seeds.Count == 0) return null;

        var blocked = new bool[n];
        var over = new bool[n];
        var hits = new Collider[16];
        for (int z = 0; z < p.h; z++)
            for (int x = 0; x < p.w; x++) {
                int i = z * p.w + x;
                float wx = p.originX + x * p.cell, wz = p.originZ + z * p.cell;
                float gy = groundAt(wx, wz);
                if (float.IsNaN(gy)) continue;
                over[i] = true;
                int k = Physics.OverlapBoxNonAlloc(new Vector3(wx, gy + 2f, wz), new Vector3(p.cell * 0.75f, 1.6f, p.cell * 0.75f), hits, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
                for (int j = 0; j < k; j++) if (barrierColliders.Contains(hits[j])) { blocked[i] = true; break; }
            }

        lastBlocked = blocked;

        // flood fill the walkable region from the seeds
        var inside = new bool[n];
        var queue = new Queue<int>();
        foreach (var sp in seeds) {
            int sx = Mathf.RoundToInt((sp.x - p.originX) / p.cell), sz = Mathf.RoundToInt((sp.z - p.originZ) / p.cell);
            if (sx < 0 || sz < 0 || sx >= p.w || sz >= p.h) continue;
            int si = sz * p.w + sx;
            if (!over[si] || blocked[si] || inside[si]) continue;
            inside[si] = true; queue.Enqueue(si);
        }
        while (queue.Count > 0) {
            int i = queue.Dequeue(); int x = i % p.w, z = i / p.w;
            for (int k = 0; k < 4; k++) {
                int nx = x + (k == 0 ? 1 : k == 1 ? -1 : 0), nz = z + (k == 2 ? 1 : k == 3 ? -1 : 0);
                if (nx < 0 || nz < 0 || nx >= p.w || nz >= p.h) continue;
                int ni = nz * p.w + nx;
                if (inside[ni] || blocked[ni] || !over[ni]) continue;
                inside[ni] = true; queue.Enqueue(ni);
            }
        }

        // anything not reachable from the map border without crossing the region (houses, fenced yards, wall cells) counts as inside
        var outside = new bool[n];
        for (int x = 0; x < p.w; x++) { Seed(outside, inside, x, 0, p, queue); Seed(outside, inside, x, p.h - 1, p, queue); }
        for (int z = 0; z < p.h; z++) { Seed(outside, inside, 0, z, p, queue); Seed(outside, inside, p.w - 1, z, p, queue); }
        while (queue.Count > 0) {
            int i = queue.Dequeue(); int x = i % p.w, z = i / p.w;
            for (int k = 0; k < 4; k++) {
                int nx = x + (k == 0 ? 1 : k == 1 ? -1 : 0), nz = z + (k == 2 ? 1 : k == 3 ? -1 : 0);
                if (nx < 0 || nz < 0 || nx >= p.w || nz >= p.h) continue;
                int ni = nz * p.w + nx;
                if (outside[ni] || inside[ni]) continue;
                outside[ni] = true; queue.Enqueue(ni);
            }
        }
        int overCount = 0, inCount = 0;
        for (int i = 0; i < n; i++) { if (over[i]) overCount++; inside[i] = !outside[i]; if (inside[i] && over[i]) inCount++; }
        if (overCount == 0 || (float)inCount / overCount > 0.9f) {
            source = $"leaked {100f * inCount / Mathf.Max(1, overCount):F0}%";
            p.Build(inside); lastDebug = p;
            return null;
        }
        if ((float)inCount / overCount < MinRegionRatio) {
            // seeds trapped by colliders (dense forests, enclosed spawn rooms): not a usable boundary
            source = $"trapped {100f * inCount / overCount:F1}%";
            p.Build(inside); lastDebug = p;
            return null;
        }
        p.Build(inside);
        source = $"flood {100f * inCount / overCount:F0}%";
        return p;
    }

    static void Seed (bool[] outside, bool[] inside, int x, int z, Playable p, Queue<int> q) {
        int i = z * p.w + x;
        if (outside[i] || inside[i]) return;
        outside[i] = true; q.Enqueue(i);
    }

    // ---------- animations ----------
    // Timeline/Animation clips that move objects in world space (cutscene cameras, recorded creature paths) were authored
    // against the old ground. Shift their height curves by how much the ground changed under them; cameras also keep
    // clearance over the surrounding terrain so they never pass through a new hill.
    static string AdjustAnimations (UnityEngine.SceneManagement.Scene scene, System.Func<float, float, float> oldGround, System.Func<float, float, float> newGround) {
        var done = new HashSet<AnimationClip>();
        int adjusted = 0;
        lastAnimReport = "";
        foreach (var root in scene.GetRootGameObjects()) {
            foreach (var director in root.GetComponentsInChildren<UnityEngine.Playables.PlayableDirector>(true)) {
                var timeline = director.playableAsset as UnityEngine.Timeline.TimelineAsset;
                if (timeline == null) continue;
                foreach (var track in timeline.GetOutputTracks()) {
                    var animTrack = track as UnityEngine.Timeline.AnimationTrack;
                    if (animTrack == null) continue;
                    var binding = director.GetGenericBinding(track);
                    Transform target = binding is Component ? ((Component)binding).transform : binding is GameObject ? ((GameObject)binding).transform : null;
                    if (target == null) continue;
                    // clip space -> world: track offset, then per-clip offset (recorded clips store where the object started)
                    bool trackOffsets = animTrack.trackOffset == UnityEngine.Timeline.TrackOffset.ApplyTransformOffsets;
                    var trackPos = trackOffsets ? animTrack.position : Vector3.zero;
                    var trackRot = trackOffsets ? animTrack.rotation : Quaternion.identity;
                    var clips = new List<KeyValuePair<AnimationClip, Matrix4x4>>();
                    if (animTrack.infiniteClip != null) {
                        var p = ReadProperty(animTrack, "infiniteClipOffsetPosition", Vector3.zero);
                        var r = ReadProperty(animTrack, "infiniteClipOffsetRotation", Quaternion.identity);
                        clips.Add(new KeyValuePair<AnimationClip, Matrix4x4>(animTrack.infiniteClip, Matrix4x4.TRS(trackPos, trackRot, Vector3.one) * Matrix4x4.TRS(p, r, Vector3.one)));
                    }
                    foreach (var tc in animTrack.GetClips()) {
                        var asset = tc.asset as UnityEngine.Timeline.AnimationPlayableAsset;
                        if (tc.animationClip == null) continue;
                        var m = Matrix4x4.TRS(trackPos, trackRot, Vector3.one);
                        if (asset != null) m = m * Matrix4x4.TRS(asset.position, asset.rotation, Vector3.one);
                        clips.Add(new KeyValuePair<AnimationClip, Matrix4x4>(tc.animationClip, m));
                    }
                    foreach (var kv in clips)
                        if (done.Add(kv.Key) && AdjustClip(kv.Key, target, kv.Value, oldGround, newGround)) { adjusted++; lastAnimReport += target.name + ":" + lastClipDelta.ToString("F1") + "m "; }
                }
            }
            foreach (var anim in root.GetComponentsInChildren<Animation>(true))
                foreach (var clip in AnimationUtility.GetAnimationClips(anim.gameObject))
                    if (clip != null && done.Add(clip) && AdjustClip(clip, anim.transform, Matrix4x4.identity, oldGround, newGround)) adjusted++;
        }
        return $"animations adjusted {adjusted} ({lastAnimReport})";
    }

    static string lastAnimReport = "";
    static float lastClipDelta;

    static T ReadProperty<T> (object obj, string name, T fallback) {
        var prop = obj.GetType().GetProperty(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (prop != null && prop.PropertyType == typeof(T)) return (T)prop.GetValue(obj);
        var field = obj.GetType().GetField("m_" + char.ToUpper(name[0]) + name.Substring(1), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field != null && field.FieldType == typeof(T)) return (T)field.GetValue(obj);
        return fallback;
    }

    static bool AdjustClip (AnimationClip clip, Transform root, Matrix4x4 clipToLocal, System.Func<float, float, float> oldGround, System.Func<float, float, float> newGround) {
        bool changed = false;
        var bindings = AnimationUtility.GetCurveBindings(clip);
        var paths = new HashSet<string>();
        // only the bound object's own position: bone curves (paths below the root) are the creature's pose and must not move
        foreach (var b in bindings) if (b.type == typeof(Transform) && b.propertyName == "m_LocalPosition.y" && string.IsNullOrEmpty(b.path)) paths.Add(b.path);
        foreach (var path in paths) {
            var target = string.IsNullOrEmpty(path) ? root : root.Find(path);
            if (target == null) continue;
            System.Func<string, AnimationCurve> curve = prop => AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), prop));
            var cx = curve("m_LocalPosition.x"); var cy = curve("m_LocalPosition.y"); var cz = curve("m_LocalPosition.z");
            if (cy == null || cy.length == 0) continue;
            var parent = target.parent;
            bool isCamera = target.GetComponentInChildren<Camera>(true) != null;
            // props animated relative to their structure (doors, gates, platforms) move with that structure, not the ground
            bool isCharacter = target.GetComponentInChildren<SkinnedMeshRenderer>(true) != null;
            if (!isCamera && !isCharacter) continue;

            float length = Mathf.Max(clip.length, cy.keys[cy.length - 1].time);
            float step = Mathf.Clamp(length / 1500f, 0.1f, 0.5f);
            var keys = new List<Keyframe>();
            var keysX = new List<Keyframe>();
            var keysZ = new List<Keyframe>();
            // a world-up shift expressed in clip space; with pitched offsets it has y and z parts
            var parentUp = parent != null ? parent.InverseTransformVector(Vector3.up) : Vector3.up;
            var clipUp = clipToLocal.inverse.MultiplyVector(parentUp);
            var times = new List<float>(); var locals = new List<Vector3>(); var deltas = new List<float>();
            float maxDelta = 0f;
            for (float t = 0f; ; t += step) {
                if (t > length) t = length;
                var local = new Vector3(cx != null ? cx.Evaluate(t) : target.localPosition.x, cy.Evaluate(t), cz != null ? cz.Evaluate(t) : target.localPosition.z);
                var placed = clipToLocal.MultiplyPoint3x4(local);
                var world = parent != null ? parent.TransformPoint(placed) : placed;
                float delta = GroundDelta(world.x, world.z, oldGround, newGround);
                if (isCamera) {
                    // keep the camera's original clearance over the highest nearby new ground
                    float clearanceNeed = 0f;
                    for (int k = 0; k < 8; k++) {
                        float a = k * Mathf.PI / 4f;
                        float sx = world.x + Mathf.Cos(a) * 6f, sz = world.z + Mathf.Sin(a) * 6f;
                        delta = Mathf.Max(delta, GroundDelta(sx, sz, oldGround, newGround));
                        float ng = newGround(sx, sz), og = oldGround(sx, sz);
                        if (float.IsNaN(ng) || float.IsNaN(og)) continue;
                        // never demand more clearance than the shot originally had (low-angle shots stay low)
                        float originalClearance = world.y - og;
                        if (originalClearance <= 0f) continue;
                        clearanceNeed = Mathf.Max(clearanceNeed, ng + Mathf.Min(2.5f, originalClearance) - (world.y + delta));
                    }
                    delta += Mathf.Max(0f, clearanceNeed);
                }
                times.Add(t); locals.Add(local); deltas.Add(delta);
                if (t >= length) break;
            }
            if (isCamera && deltas.Count > 2) {
                // a camera tracing every bump feels shaky: take the running maximum (never lower than required)
                // over ~2.5s and average it, which glides over the terrain without dipping below it
                int w = Mathf.Max(1, Mathf.RoundToInt(1.25f / step));
                var peak = new float[deltas.Count];
                for (int i = 0; i < deltas.Count; i++) {
                    float m = float.MinValue;
                    for (int j = Mathf.Max(0, i - w); j <= Mathf.Min(deltas.Count - 1, i + w); j++) m = Mathf.Max(m, deltas[j]);
                    peak[i] = m;
                }
                for (int i = 0; i < deltas.Count; i++) {
                    float sum = 0f; int cnt = 0;
                    for (int j = Mathf.Max(0, i - w); j <= Mathf.Min(deltas.Count - 1, i + w); j++) { sum += peak[j]; cnt++; }
                    deltas[i] = Mathf.Max(deltas[i], sum / cnt);
                }
            }
            for (int i = 0; i < times.Count; i++) {
                maxDelta = Mathf.Max(maxDelta, Mathf.Abs(deltas[i]));
                var shift = clipUp * deltas[i];
                keys.Add(new Keyframe(times[i], locals[i].y + shift.y));
                keysX.Add(new Keyframe(times[i], locals[i].x + shift.x));
                keysZ.Add(new Keyframe(times[i], locals[i].z + shift.z));
            }
            lastClipDelta = maxDelta;
            if (maxDelta < 0.02f) continue;
            System.Func<List<Keyframe>, AnimationCurve> build = list => {
                var c = new AnimationCurve(list.ToArray());
                for (int i = 0; i < c.length; i++) {
                    AnimationUtility.SetKeyLeftTangentMode(c, i, AnimationUtility.TangentMode.ClampedAuto);
                    AnimationUtility.SetKeyRightTangentMode(c, i, AnimationUtility.TangentMode.ClampedAuto);
                }
                return c;
            };
            Undo.RegisterCompleteObjectUndo(clip, "Adjust Animation To Terrain");
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.y"), build(keys));
            if (Mathf.Abs(clipUp.x) > 0.001f && cx != null) AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x"), build(keysX));
            if (Mathf.Abs(clipUp.z) > 0.001f && cz != null) AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.z"), build(keysZ));
            EditorUtility.SetDirty(clip);
            changed = true;
        }
        return changed;
    }

    static float GroundDelta (float x, float z, System.Func<float, float, float> oldGround, System.Func<float, float, float> newGround) {
        float o = oldGround(x, z), n = newGround(x, z);
        return float.IsNaN(o) || float.IsNaN(n) ? 0f : n - o;
    }

    // Per-terrain blur/erosion can leave neighbouring edges at slightly different heights; average the shared rows.
    public static string StitchSeams (Terrain[] terrains) {
        int seams = 0;
        for (int i = 0; i < terrains.Length; i++)
            for (int j = 0; j < terrains.Length; j++) {
                if (i == j) continue;
                var a = terrains[i]; var b = terrains[j];
                var ta = a.terrainData; var tb = b.terrainData;
                if (ta.heightmapResolution != tb.heightmapResolution || Mathf.Abs(ta.size.y - tb.size.y) > 0.01f) continue;
                int res = ta.heightmapResolution;
                Vector3 pa = a.transform.position, pb = b.transform.position;
                // the shared edge only needs matching length and resolution
                bool eastNeighbour = Mathf.Abs(pa.x + ta.size.x - pb.x) < 0.01f && Mathf.Abs(pa.z - pb.z) < 0.01f && Mathf.Abs(pa.y - pb.y) < 0.01f && Mathf.Abs(ta.size.z - tb.size.z) < 0.01f;
                bool northNeighbour = Mathf.Abs(pa.z + ta.size.z - pb.z) < 0.01f && Mathf.Abs(pa.x - pb.x) < 0.01f && Mathf.Abs(pa.y - pb.y) < 0.01f && Mathf.Abs(ta.size.x - tb.size.x) < 0.01f;
                if (!eastNeighbour && !northNeighbour) continue;
                var ha = ta.GetHeights(0, 0, res, res); var hb = tb.GetHeights(0, 0, res, res);
                for (int k = 0; k < res; k++) {
                    if (eastNeighbour) { float avg = (ha[k, res - 1] + hb[k, 0]) * 0.5f; ha[k, res - 1] = avg; hb[k, 0] = avg; }
                    else { float avg = (ha[res - 1, k] + hb[0, k]) * 0.5f; ha[res - 1, k] = avg; hb[0, k] = avg; }
                }
                ta.SetHeights(0, 0, ha); tb.SetHeights(0, 0, hb);
                EditorUtility.SetDirty(ta); EditorUtility.SetDirty(tb);
                seams++;
            }
        return seams > 0 ? $" stitched {seams} seams" : "";
    }

    // ---------- helpers ----------
    // separable box blur; with a pin mask, fully protected samples keep their value
    static void Blur (float[,] a, int radius, int passes, float[,] pin = null) {
        int h = a.GetLength(0), w = a.GetLength(1);
        var tmp = new float[h, w];
        for (int p = 0; p < passes; p++) {
            for (int z = 0; z < h; z++) {
                float sum = 0f; int count = 0;
                for (int x = -radius; x <= radius; x++) { int cx = Mathf.Clamp(x, 0, w - 1); sum += a[z, cx]; count++; }
                for (int x = 0; x < w; x++) {
                    tmp[z, x] = sum / count;
                    int xo = Mathf.Clamp(x - radius, 0, w - 1), xi = Mathf.Clamp(x + radius + 1, 0, w - 1);
                    sum += a[z, xi] - a[z, xo];
                }
            }
            for (int x = 0; x < w; x++) {
                float sum = 0f; int count = 0;
                for (int z = -radius; z <= radius; z++) { int cz = Mathf.Clamp(z, 0, h - 1); sum += tmp[cz, x]; count++; }
                for (int z = 0; z < h; z++) {
                    float v = sum / count;
                    a[z, x] = pin == null ? v : Mathf.Lerp(v, a[z, x], Mathf.Clamp01(pin[z, x] * 1.2f));
                    int zo = Mathf.Clamp(z - radius, 0, h - 1), zi = Mathf.Clamp(z + radius + 1, 0, h - 1);
                    sum += tmp[zi, x] - tmp[zo, x];
                }
            }
        }
    }

    static void Collect (Transform t, List<Unit> units) {
        var go = t.gameObject;
        if (go.name.StartsWith(WallName)) return;
        bool prefabRoot = PrefabUtility.IsOutermostPrefabInstanceRoot(go);
        bool hasGeometry = false;
        foreach (var r in go.GetComponents<Renderer>()) if (!(r is ParticleSystemRenderer)) hasGeometry = true;
        if (go.GetComponent<Collider>() != null) hasGeometry = true;

        if (prefabRoot || hasGeometry) {
            Bounds b; bool any;
            GatherBounds(t, out b, out any);
            if (!any) b = new Bounds(t.position, Vector3.zero);
            units.Add(new Unit { transform = t, bounds = b, point = !any });
            return;
        }
        if (t.childCount == 0) {
            // empty markers (spawn points etc.)
            units.Add(new Unit { transform = t, bounds = new Bounds(t.position, Vector3.zero), point = true });
            return;
        }
        foreach (Transform c in t) Collect(c, units);
    }

    static void GatherBounds (Transform t, out Bounds b, out bool any) {
        b = new Bounds(); any = false;
        foreach (var r in t.GetComponentsInChildren<Renderer>(true)) {
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        foreach (var c in t.GetComponentsInChildren<Collider>(true)) {
            if (c.isTrigger && Mathf.Max(c.bounds.size.x, c.bounds.size.z) > 30f) continue;
            if (!any) { b = c.bounds; any = true; } else b.Encapsulate(c.bounds);
        }
    }

    static float[] SampleFootprint (Bounds b, System.Func<float, float, float> groundAt) {
        var e = b.extents * 0.7f;
        return new[] {
            groundAt(b.center.x, b.center.z),
            groundAt(b.center.x - e.x, b.center.z - e.z), groundAt(b.center.x + e.x, b.center.z - e.z),
            groundAt(b.center.x - e.x, b.center.z + e.z), groundAt(b.center.x + e.x, b.center.z + e.z)
        };
    }

    static List<Vector2> WallPolygon (UnityEngine.SceneManagement.Scene scene) {
        var pts = new List<Vector2>();
        foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name.StartsWith(WallName)) pts.Add(new Vector2(t.position.x, t.position.z));
        if (pts.Count < 3) return null;
        var c = Vector2.zero;
        foreach (var p in pts) c += p;
        c /= pts.Count;
        pts.Sort((a, b2) => Mathf.Atan2(a.y - c.y, a.x - c.x).CompareTo(Mathf.Atan2(b2.y - c.y, b2.x - c.x)));
        return pts;
    }

    // negative inside the polygon, positive outside
    static float SignedDistance (Vector2 p, List<Vector2> poly) {
        float best = float.MaxValue;
        bool inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++) {
            Vector2 a = poly[j], b = poly[i];
            Vector2 ab = b - a;
            float tt = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.0001f));
            float d = (a + ab * tt - p).sqrMagnitude;
            if (d < best) best = d;
            if (((a.y > p.y) != (b.y > p.y)) && (p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)) inside = !inside;
        }
        best = Mathf.Sqrt(best);
        return inside ? -best : best;
    }

    // stretch an fBm value (clustered around 0.5) over the full 0..1 range
    static float Spread (float n) {
        return Mathf.Clamp01((n - 0.5f) / 0.32f + 0.5f);
    }

    static float Fbm (float x, float y, int octaves) {
        float sum = 0f, amp = 1f, norm = 0f, f = 1f;
        for (int i = 0; i < octaves; i++) {
            sum += Mathf.PerlinNoise(x * f + i * 19.19f, y * f + i * 7.77f) * amp;
            norm += amp; amp *= 0.5f; f *= 2.03f;
        }
        return sum / norm;
    }

    static bool MatchesAny (string name, string[] keys) {
        foreach (var k in keys) if (name.IndexOf(k, System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
        return false;
    }
}
