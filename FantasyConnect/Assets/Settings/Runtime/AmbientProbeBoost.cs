using UnityEngine;
using UnityEngine.Rendering;

// Lifts the baked ambient probe for dynamic objects (characters, enemies) in dark lightmapped scenes,
// without re-baking lightmaps. Lightmapped geometry is unaffected.
public class AmbientProbeBoost : MonoBehaviour {
    [Min(1f)] public float multiplier = 2.5f;
    [Tooltip("Flat colour added to the probe (linear), useful when the baked probe is nearly black.")]
    public Color addColor = new Color(0.05f, 0.045f, 0.04f);

    SphericalHarmonicsL2 original;
    bool applied;

    void OnEnable () {
        original = RenderSettings.ambientProbe;
        var probe = original * multiplier;
        probe.AddAmbientLight(addColor);
        RenderSettings.ambientProbe = probe;
        applied = true;
    }

    void OnDisable () {
        if (!applied) return;
        RenderSettings.ambientProbe = original;
        applied = false;
    }
}
