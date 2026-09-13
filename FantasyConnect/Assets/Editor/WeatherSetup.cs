using UnityEditor;
using UnityEngine;

// Places the Stylized Weather controller in the active scene with a biome-appropriate default weather.
public static class WeatherSetup {
    const string ControllerPrefab = "Assets/Unluck Software/Stylized Weather/Prefabs/Weather Controller.prefab";
    const string ObjectName = "Weather Controller";

    static ParticleSystem.MinMaxCurve Scale (ParticleSystem.MinMaxCurve c, float k) {
        switch (c.mode) {
            case ParticleSystemCurveMode.Constant: c.constant *= k; break;
            case ParticleSystemCurveMode.TwoConstants: c.constantMin *= k; c.constantMax *= k; break;
            default: c.curveMultiplier *= k; break;
        }
        return c;
    }

    // Keeps only what the scene's weathers need: the controller prefab carries ~40 particle systems for every preset,
    // and each would otherwise update every frame; the wrap helpers also copy all particles every 0.2s.
    public static string Optimize (GameObject controllerObject, params string[] usedWeathers) {
        var needed = new System.Collections.Generic.HashSet<ParticleSystem>();
        foreach (var ws in controllerObject.GetComponentsInChildren<UnluckSoftware.StylizedWeatherSettings>(true)) {
            if (System.Array.IndexOf(usedWeathers, ws.name) < 0) continue;
            foreach (var e in ws.elements)
                if (e.particleSystem != null && e.emission.constantMax > 0f)
                    foreach (var child in e.particleSystem.GetComponentsInChildren<ParticleSystem>(true)) needed.Add(child); // keep sub-emitters
        }
        int disabled = 0, kept = 0;
        var container = controllerObject.transform.Find("Particles");
        if (container != null)
            foreach (Transform t in container) {
                var ps = t.GetComponent<ParticleSystem>();
                bool keep = ps != null && needed.Contains(ps);
                if (t.gameObject.activeSelf != keep) { Undo.RecordObject(t.gameObject, "Weather Optimize"); t.gameObject.SetActive(keep); }
                if (keep) kept++; else disabled++;
            }
        // the emitter already follows the camera; wrapping particles back into the box is not needed
        foreach (var w in controllerObject.GetComponentsInChildren<UnluckSoftware.StylizedWeatherParticleWrapper>(true)) {
            Undo.RecordObject(w, "Weather Optimize");
            w.wrapParticles = false;
            w.enabled = false;
        }
        foreach (var c in controllerObject.GetComponentsInChildren<Component>(true)) if (c != null) PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        foreach (var t in controllerObject.GetComponentsInChildren<Transform>(true)) PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(controllerObject.scene);
        return $"particle systems kept {kept}, disabled {disabled}";
    }

    // Lays out WeatherZone boxes along a Timeline camera flight: the biome under each stretch of the path is read from
    // the terrain splat layers once, here in the editor, so nothing has to be sampled at runtime.
    public static string BuildZonesAlongTimeline (UnityEngine.Playables.PlayableDirector director, Transform cameraTransform,
                                                  string[] layerKeywords, string[] weathers, float sideWidth = 400f) {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects()) if (root.name == "Weather Zones") Object.DestroyImmediate(root);
        var terrains = Object.FindObjectsOfType<Terrain>();

        // 1. sample the flight and classify each point by the dominant biome within 40m
        var points = new System.Collections.Generic.List<Vector3>();
        var kinds = new System.Collections.Generic.List<int>();
        for (double t = 0; t <= director.duration; t += 2.0) {
            director.time = t; director.Evaluate();
            var p = cameraTransform.position;
            var weights = new float[weathers.Length];
            for (int s = 0; s <= 12; s++) {
                var q = s == 0 ? p : p + new Vector3(Mathf.Cos(s * Mathf.PI / 6f), 0, Mathf.Sin(s * Mathf.PI / 6f)) * 40f;
                foreach (var te in terrains) {
                    var o = te.transform.position; var td = te.terrainData;
                    if (q.x < o.x || q.x > o.x + td.size.x || q.z < o.z || q.z > o.z + td.size.z) continue;
                    var a = td.GetAlphamaps(Mathf.Clamp((int)((q.x - o.x) / td.size.x * (td.alphamapWidth - 1)), 0, td.alphamapWidth - 1),
                                            Mathf.Clamp((int)((q.z - o.z) / td.size.z * (td.alphamapHeight - 1)), 0, td.alphamapHeight - 1), 1, 1);
                    for (int l = 0; l < td.terrainLayers.Length; l++)
                        for (int k = 0; k < layerKeywords.Length; k++)
                            if (td.terrainLayers[l] != null && td.terrainLayers[l].name.Contains(layerKeywords[k])) weights[k] += a[0, 0, l];
                    break;
                }
            }
            int best = -1; float bw = 0f;
            for (int k = 0; k < weights.Length; k++) if (weights[k] > bw) { bw = weights[k]; best = k; }
            points.Add(p); kinds.Add(best);
        }
        director.time = 0; director.Evaluate();

        // 2. majority filter so a single odd sample does not split a stretch
        var smooth = new int[kinds.Count];
        for (int i = 0; i < kinds.Count; i++) {
            var votes = new System.Collections.Generic.Dictionary<int, int>();
            for (int j = Mathf.Max(0, i - 3); j <= Mathf.Min(kinds.Count - 1, i + 3); j++) { int v; votes.TryGetValue(kinds[j], out v); votes[kinds[j]] = v + 1; }
            int top = kinds[i], topCount = 0;
            foreach (var kv in votes) if (kv.Key >= 0 && kv.Value > topCount) { top = kv.Key; topCount = kv.Value; }
            smooth[i] = top;
        }

        // 3. one box per continuous stretch, oriented along the flight direction
        var parent = new GameObject("Weather Zones");
        UnityEditor.SceneManagement.EditorSceneManager.MoveGameObjectToScene(parent, scene);
        var report = new System.Text.StringBuilder();
        int start = 0;
        for (int i = 1; i <= smooth.Length; i++) {
            if (i < smooth.Length && smooth[i] == smooth[start]) continue;
            if (smooth[start] >= 0) {
                var a = points[start]; var b = points[i - 1];
                var dir = b - a; dir.y = 0f;
                if (dir.sqrMagnitude < 1f) dir = Vector3.forward;
                var zoneGo = new GameObject($"Zone {parent.transform.childCount} {weathers[smooth[start]]}");
                zoneGo.transform.SetParent(parent.transform, false);
                // overlap neighbours a little so there is never a gap between areas
                const float overlap = 20f;
                zoneGo.transform.position = new Vector3((a.x + b.x) * 0.5f, (a.y + b.y) * 0.5f, (a.z + b.z) * 0.5f);
                zoneGo.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                var zone = zoneGo.AddComponent<WeatherZone>();
                zone.weather = weathers[smooth[start]];
                zone.size = new Vector3(sideWidth, 2000f, dir.magnitude + overlap * 2f);
                report.Append($"{zone.weather}({dir.magnitude:F0}m) ");
            }
            start = i;
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        return report.ToString();
    }

    public static string Apply (string weatherName, bool keepWindZone = false) {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects()) if (root.name == ObjectName) Object.DestroyImmediate(root);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ControllerPrefab);
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        go.name = ObjectName;

        var controller = go.GetComponent<UnluckSoftware.StylizedWeatherController>();
        // scenes use BOXOPHOBIC Height Fog; the controller would otherwise drive RenderSettings fog
        controller.affectGlobalFog = false;
        controller.drawGizmos = false;
        controller.followObject = null;

        GameObject target = null;
        foreach (var ws in go.GetComponentsInChildren<UnluckSoftware.StylizedWeatherSettings>(true)) {
            if (ws.name == weatherName) target = ws.gameObject;
            ws.gameObject.SetActive(false);
        }
        if (target == null) { Object.DestroyImmediate(go); return $"{scene.name}: weather '{weatherName}' not found"; }
        controller.defaultWeather = target;

        // Height Fog already provides the haze; the 20-40m fog sprites spawn around the camera and read as a white blob.
        // Ground-hit splat sub-emitters show up as flat blue discs on the terrain.
        // every preset is prepared, so scenes that switch weather at runtime get the same tuning
        var resized = new System.Collections.Generic.HashSet<ParticleSystem>();
        foreach (var settings in go.GetComponentsInChildren<UnluckSoftware.StylizedWeatherSettings>(true)) {
            for (int i = 0; i < settings.elements.Length; i++) {
                var ps = settings.elements[i].particleSystem;
                if (ps == null) continue;
                if (ps.name.StartsWith("Fog")) { settings.elements[i].emission = new ParticleSystem.MinMaxCurve(0f); continue; }
                // The asset is tuned for a high top-down view (50m boxes, fast fall). This game uses a close third-person
                // camera, so concentrate precipitation around the view and make snow drift instead of streak.
                bool snow = ps.name.StartsWith("Snow");
                bool precipitation = snow || ps.name.StartsWith("Rain -C") || ps.name.StartsWith("Hail");
                if (!precipitation) continue;
                if (resized.Add(ps)) {
                    var shape = ps.shape;
                    if (shape.scale.x > 30f) shape.scale = new Vector3(shape.scale.x * 0.5f, shape.scale.y * 0.5f, shape.scale.z);
                    var main = ps.main;
                    if (snow) main.maxParticles = Mathf.Max(main.maxParticles, 3000);
                }
                if (snow) {
                    settings.elements[i].color = new ParticleSystem.MinMaxGradient(new Color(0.85f, 0.9f, 1f, 0.85f), new Color(1f, 1f, 1f, 1f));
                    settings.elements[i].speed = Scale(settings.elements[i].speed, 0.35f);
                    settings.elements[i].lifetime = Scale(settings.elements[i].lifetime, 2.8f);
                }
            }
        }

        foreach (var t in go.GetComponentsInChildren<Transform>(true))
            if (t.name == "Ground" && t.parent != null && (t.parent.name.StartsWith("Snow") || t.parent.name.StartsWith("Hail"))) {
                var sub = t.parent.GetComponent<ParticleSystem>().subEmitters;
                sub.enabled = false;
                t.gameObject.SetActive(false);
            }

        if (!keepWindZone && controller.windZone != null) {
            // terrain trees/grass already have their own wind; a strong weather wind zone makes them flail
            controller.windZone.gameObject.SetActive(false);
            controller.windZone = null;
        }

        // flakes/leaves passing right in front of the lens would cover the screen and bloom out
        foreach (var r in go.GetComponentsInChildren<ParticleSystemRenderer>(true)) {
            var n = r.gameObject.name;
            if (n.StartsWith("Snow") || n.StartsWith("Hail") || n.StartsWith("Leaves") || n.StartsWith("Rain -C") || n.StartsWith("Paper") || n.StartsWith("Crumbled"))
                r.maxParticleSize = Mathf.Min(r.maxParticleSize, 0.04f);
        }

        // the particle wrappers only follow controller.followObject, which is not known until runtime,
        // so move the whole rig with the active camera instead
        go.AddComponent<AmbientParticleFollow>().forwardOffset = 6f;

        foreach (var c in go.GetComponentsInChildren<Component>(true)) {
            if (c == null) continue;
            PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        }
        foreach (var t in go.GetComponentsInChildren<Transform>(true)) PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);
        EditorUtility.SetDirty(go);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        return $"{scene.name}: {weatherName}";
    }
}
