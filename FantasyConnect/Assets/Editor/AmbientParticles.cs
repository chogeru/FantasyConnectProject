using UnityEditor;
using UnityEngine;

// Builds camera-following ambient particles (pollen, fireflies, leaves, snow, dust, embers) per scene.
public static class AmbientParticles {
    const string MaterialFolder = "Assets/Settings/AmbientParticles";
    const string ObjectName = "Ambient Particles";

    const string Dot = "Assets/開発用アセット/アニメーション/Malbers Animations/Common/Particles/Textures/txt_dot.png";
    const string Ember = "Assets/開発用アセット/アニメーション/Malbers Animations/Common/Particles/Textures/txt_ember.png";
    const string Firefly = "Assets/開発用アセット/マップアセット/Toon Fantasy Nature/Particles/Textures/TFF_Firefly_01A_D.tif";
    const string Leaf = "Assets/開発用アセット/マップアセット/Polyart/PolyartStudio/SharedResources/Textures/ParticleEffects/T_Leaf.png";
    const string Dust = "Assets/開発用アセット/エフェクト/Hovl Studio/HSFiles/Textures/Dust2.png";

    public enum Preset { Pollen, Fireflies, Leaves, Snow, Dust, Embers, DustMotes }

    class Settings {
        public string texture; public bool additive;
        public int maxParticles; public float rate;
        public Vector2 lifetime, size, speed;
        public Color colorA, colorB;
        public Vector3 box; public Vector3 offset;
        public float gravity, noiseStrength, noiseFrequency;
        public Vector3 wind;
        public bool spin, flicker;
    }

    static Settings Get (Preset p) {
        switch (p) {
            case Preset.Pollen: return new Settings {
                texture = Dot, additive = false, maxParticles = 160, rate = 22,
                lifetime = new Vector2(8, 12), size = new Vector2(0.03f, 0.07f), speed = new Vector2(0.05f, 0.2f),
                colorA = new Color(1f, 1f, 0.9f, 0.75f), colorB = new Color(1f, 0.95f, 0.6f, 0.6f),
                box = new Vector3(40, 8, 40), offset = new Vector3(0, 2, 0), gravity = -0.003f,
                noiseStrength = 0.3f, noiseFrequency = 0.2f, wind = new Vector3(0.3f, 0, 0.1f) };
            case Preset.Fireflies: return new Settings {
                texture = Firefly, additive = true, maxParticles = 90, rate = 10,
                lifetime = new Vector2(5, 8), size = new Vector2(0.08f, 0.16f), speed = new Vector2(0f, 0.1f),
                colorA = new Color(0.9f, 1.6f, 0.4f, 1f), colorB = new Color(1.4f, 1.5f, 0.5f, 1f),
                box = new Vector3(35, 4, 35), offset = new Vector3(0, 0.5f, 0),
                noiseStrength = 0.6f, noiseFrequency = 0.35f, flicker = true };
            case Preset.Leaves: return new Settings {
                texture = Leaf, additive = false, maxParticles = 120, rate = 11,
                lifetime = new Vector2(8, 12), size = new Vector2(0.15f, 0.3f), speed = new Vector2(0f, 0.2f),
                colorA = new Color(1f, 0.55f, 0.15f, 1f), colorB = new Color(0.85f, 0.2f, 0.1f, 1f),
                box = new Vector3(40, 2, 40), offset = new Vector3(0, 12, 0), gravity = 0.05f,
                noiseStrength = 0.8f, noiseFrequency = 0.25f, wind = new Vector3(0.6f, 0, 0.2f), spin = true };
            case Preset.Snow: return new Settings {
                texture = Dot, additive = false, maxParticles = 1500, rate = 220,
                lifetime = new Vector2(6, 8), size = new Vector2(0.03f, 0.07f), speed = new Vector2(0f, 0.1f),
                colorA = new Color(1f, 1f, 1f, 0.9f), colorB = new Color(0.9f, 0.95f, 1f, 0.8f),
                box = new Vector3(50, 1, 50), offset = new Vector3(0, 15, 0), gravity = 0.1f,
                noiseStrength = 0.3f, noiseFrequency = 0.2f, wind = new Vector3(0.4f, 0, 0) };
            case Preset.Dust: return new Settings {
                texture = Dust, additive = false, maxParticles = 160, rate = 20,
                lifetime = new Vector2(6, 10), size = new Vector2(0.6f, 1.6f), speed = new Vector2(0f, 0.1f),
                colorA = new Color(0.85f, 0.68f, 0.48f, 0.12f), colorB = new Color(0.75f, 0.6f, 0.45f, 0.08f),
                box = new Vector3(40, 3, 40), offset = new Vector3(0, 1, 0),
                noiseStrength = 0.4f, noiseFrequency = 0.15f, wind = new Vector3(1.5f, 0, 0.3f), spin = true };
            case Preset.Embers: return new Settings {
                texture = Ember, additive = true, maxParticles = 120, rate = 14,
                lifetime = new Vector2(4, 7), size = new Vector2(0.03f, 0.08f), speed = new Vector2(0f, 0.1f),
                colorA = new Color(2f, 0.5f, 0.9f, 1f), colorB = new Color(2f, 0.7f, 0.3f, 1f),
                box = new Vector3(40, 2, 40), offset = new Vector3(0, -1, 0), gravity = -0.08f,
                noiseStrength = 0.5f, noiseFrequency = 0.4f, flicker = true };
            default: return new Settings {
                texture = Dot, additive = true, maxParticles = 60, rate = 8,
                lifetime = new Vector2(8, 12), size = new Vector2(0.015f, 0.04f), speed = new Vector2(0f, 0.03f),
                colorA = new Color(1f, 0.95f, 0.8f, 0.5f), colorB = new Color(1f, 0.9f, 0.7f, 0.35f),
                box = new Vector3(12, 4, 12), offset = new Vector3(0, 0, 0),
                noiseStrength = 0.1f, noiseFrequency = 0.2f };
        }
    }

    public static string Apply (Preset preset) {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects()) if (root.name == ObjectName) Object.DestroyImmediate(root);

        var s = Get(preset);
        var go = new GameObject(ObjectName);
        go.AddComponent<AmbientParticleFollow>().offset = s.offset;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.prewarm = true;
        main.duration = 10f;
        main.maxParticles = s.maxParticles;
        main.startLifetime = new ParticleSystem.MinMaxCurve(s.lifetime.x, s.lifetime.y);
        main.startSize = new ParticleSystem.MinMaxCurve(s.size.x, s.size.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(s.speed.x, s.speed.y);
        main.startColor = new ParticleSystem.MinMaxGradient(s.colorA, s.colorB);
        main.gravityModifier = s.gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        if (s.spin) { main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f); }

        var emission = ps.emission;
        emission.rateOverTime = s.rate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = s.box;

        var noise = ps.noise;
        noise.enabled = s.noiseStrength > 0f;
        noise.strength = s.noiseStrength;
        noise.frequency = s.noiseFrequency;
        noise.scrollSpeed = 0.2f;
        noise.quality = ParticleSystemNoiseQuality.Medium;

        if (s.wind != Vector3.zero) {
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(s.wind.x * 0.6f, s.wind.x);
            vel.y = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.z = new ParticleSystem.MinMaxCurve(s.wind.z * 0.6f, s.wind.z);
        }

        if (s.spin) {
            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-2f, 2f);
        }

        // fade in / out so particles never pop at the emitter box edges
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                     new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        if (s.flicker) {
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            var curve = new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(0.15f, 1f), new Keyframe(0.3f, 0.5f), new Keyframe(0.5f, 1f), new Keyframe(0.7f, 0.4f), new Keyframe(0.85f, 1f), new Keyframe(1f, 0.2f));
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = GetMaterial(preset, s);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        // flakes passing right in front of the lens would otherwise cover the screen and bloom out
        renderer.maxParticleSize = 0.02f;

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        return $"{scene.name}: {preset} (max {s.maxParticles})";
    }

    static Material GetMaterial (Preset preset, Settings s) {
        if (!AssetDatabase.IsValidFolder(MaterialFolder)) AssetDatabase.CreateFolder("Assets/Settings", "AmbientParticles");
        string path = $"{MaterialFolder}/Ambient_{preset}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) {
            mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(s.texture));
        mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", s.additive ? 2f : 0f);
        mat.SetFloat("_Cull", 0f);
        mat.SetFloat("_CameraFadingEnabled", 1f);
        mat.SetFloat("_CameraNearFadeDistance", 0.3f);
        mat.SetFloat("_CameraFarFadeDistance", 2f);
        mat.SetVector("_CameraFadeParams", new Vector4(0.3f, 1f / (2f - 0.3f), 0f, 0f));
        mat.EnableKeyword("_FADING_ON");
        mat.enableInstancing = true;
        var edAsm = System.Reflection.Assembly.Load("Unity.RenderPipelines.Universal.Editor");
        var setup = edAsm.GetType("UnityEditor.BaseShaderGUI").GetMethod("SetupMaterialBlendMode", new[] { typeof(Material) });
        setup?.Invoke(null, new object[] { mat });
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
