using UnityEditor;
using UnityEngine;

// ToonScapes ships shaders only for URP 17 (Unity 6); on URP 12 they have no usable SubShader and render pink.
// This moves the materials onto URP 12's Lit shader, carrying over the textures that define the look.
// The original shader name is kept in an override tag so the materials can be switched back after an upgrade.
public static class ToonScapesURP12 {
    const string OriginalShaderTag = "ToonScapesOriginalShader";

    public static string Convert (string folder = "Assets/ToonScapes") {
        var lit = Shader.Find("Universal Render Pipeline/Lit");
        int converted = 0, skipped = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { folder })) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null || !m.shader.name.StartsWith("ToonScapes/URP/")) continue;
            string kind = m.shader.name.Substring("ToonScapes/URP/".Length);
            if (kind == "Skybox" || kind == "Terrain") { skipped++; continue; }

            // read everything before switching shaders (properties of the old shader become inaccessible)
            Texture baseMap = null, normal = null, emission = null;
            Color tint = Color.white, emissionColor = Color.black, waterColor = Color.clear;
            float cutoff = -1f;
            switch (kind) {
                case "Stone":
                    baseMap = Tex(m, "_SurfaceTexture"); normal = Tex(m, "_SurfaceNormalMap") ?? Tex(m, "_MacroNormalMap");
                    break;
                case "Water": case "Waterfall": case "Water Ripples":
                    normal = Tex(m, "_NormalMap");
                    waterColor = m.HasProperty("_WaterColor") ? m.GetColor("_WaterColor") : new Color(0f, 0.7f, 0.75f);
                    break;
                default: // Surface, SurfaceTop, Background, Vegetation
                    baseMap = Tex(m, "_MainTexture"); normal = Tex(m, "_NormalMap"); emission = Tex(m, "_EmissionMap");
                    if (m.HasProperty("_BaseTint")) { var t = m.GetColor("_BaseTint"); if (t.maxColorComponent > 0.05f) tint = t; }
                    if (emission != null && m.HasProperty("_EmissionColor")) emissionColor = m.GetColor("_EmissionColor");
                    if (m.HasProperty("_AlphaClipThreshold")) cutoff = m.GetFloat("_AlphaClipThreshold");
                    break;
            }

            m.SetOverrideTag(OriginalShaderTag, m.shader.name);
            m.shader = lit;
            // keywords/colours left over from the ToonScapes shader would switch on Lit features (e.g. red emission)
            m.shaderKeywords = new string[0];
            m.SetColor("_EmissionColor", Color.black);
            m.SetTexture("_EmissionMap", null);
            m.SetFloat("_WorkflowMode", 1f);
            m.SetFloat("_Metallic", 0f);
            m.SetFloat("_Smoothness", 0.15f);
            if (baseMap != null) m.SetTexture("_BaseMap", baseMap);
            m.SetColor("_BaseColor", tint);
            if (normal != null) { m.SetTexture("_BumpMap", normal); m.EnableKeyword("_NORMALMAP"); }

            if (emission != null && emissionColor.maxColorComponent > 0.01f) {
                m.SetTexture("_EmissionMap", emission);
                m.SetColor("_EmissionColor", emissionColor);
                m.EnableKeyword("_EMISSION");
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }

            if (cutoff >= 0f) {
                // foliage cards: cut out and render both faces
                m.SetFloat("_AlphaClip", 1f); m.SetFloat("_Cutoff", cutoff); m.EnableKeyword("_ALPHATEST_ON");
                m.SetFloat("_Cull", 0f); m.doubleSidedGI = true;
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            }

            if (waterColor.a > 0f) {
                m.SetColor("_BaseColor", new Color(waterColor.r, waterColor.g, waterColor.b, 0.72f));
                m.SetFloat("_Surface", 1f); m.SetFloat("_Blend", 0f); m.SetFloat("_Smoothness", 0.9f);
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetFloat("_ZWrite", 0f);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            m.enableInstancing = true;
            EditorUtility.SetDirty(m);
            converted++;
        }
        AssetDatabase.SaveAssets();
        return $"converted {converted}, left as is {skipped}";
    }

    static Texture Tex (Material m, string name) {
        return m.HasProperty(name) ? m.GetTexture(name) : null;
    }
}
