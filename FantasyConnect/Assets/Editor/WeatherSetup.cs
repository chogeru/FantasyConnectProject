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
        var settings = target.GetComponent<UnluckSoftware.StylizedWeatherSettings>();
        for (int i = 0; i < settings.elements.Length; i++) {
            var ps = settings.elements[i].particleSystem;
            if (ps != null && ps.name.StartsWith("Fog")) settings.elements[i].emission = new ParticleSystem.MinMaxCurve(0f);
        }
        // The asset is tuned for a high top-down view (50m boxes, fast fall). This game uses a close third-person
        // camera, so concentrate precipitation around the view and make snow drift instead of streak.
        for (int i = 0; i < settings.elements.Length; i++) {
            var ps = settings.elements[i].particleSystem;
            if (ps == null) continue;
            bool snow = ps.name.StartsWith("Snow");
            bool precipitation = snow || ps.name.StartsWith("Rain -C") || ps.name.StartsWith("Hail");
            if (!precipitation) continue;
            var shape = ps.shape;
            if (shape.scale.x > 30f) shape.scale = new Vector3(shape.scale.x * 0.5f, shape.scale.y * 0.5f, shape.scale.z);
            if (snow) {
                settings.elements[i].color = new ParticleSystem.MinMaxGradient(new Color(0.85f, 0.9f, 1f, 0.85f), new Color(1f, 1f, 1f, 1f));
                settings.elements[i].speed = Scale(settings.elements[i].speed, 0.35f);
                settings.elements[i].lifetime = Scale(settings.elements[i].lifetime, 2.8f);
                var main = ps.main;
                main.maxParticles = Mathf.Max(main.maxParticles, 3000);
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
