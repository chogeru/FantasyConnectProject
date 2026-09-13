using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Material Variants (m_Parent) are a Unity 2022+ feature. In 2021 a variant only keeps the properties it overrides,
// so everything it should inherit (textures, colours) is lost. This bakes the parent's values into the variant.
public static class MaterialVariantFlatten {
    static readonly Regex ParentRegex = new Regex(@"m_Parent:\s*\{fileID:\s*2100000,\s*guid:\s*([0-9a-f]{32})");
    static readonly Regex PropertyNameRegex = new Regex(@"^\s{4}- (\w+):", RegexOptions.Multiline);

    public static string FlattenFolder (string folder) {
        var variants = new Dictionary<string, string>(); // material path -> parent path
        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { folder })) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var m = ParentRegex.Match(File.ReadAllText(path));
            if (!m.Success) continue;
            var parentPath = AssetDatabase.GUIDToAssetPath(m.Groups[1].Value);
            if (!string.IsNullOrEmpty(parentPath)) variants[path] = parentPath;
        }

        var done = new HashSet<string>();
        int flattened = 0, copied = 0;
        foreach (var path in variants.Keys) copied += Flatten(path, variants, done, ref flattened);
        AssetDatabase.SaveAssets();
        return $"variants {variants.Count}, flattened {flattened}, inherited values copied {copied}";
    }

    static int Flatten (string path, Dictionary<string, string> variants, HashSet<string> done, ref int flattened) {
        if (done.Contains(path)) return 0;
        done.Add(path);
        string parentPath;
        if (!variants.TryGetValue(path, out parentPath)) return 0;
        int copied = Flatten(parentPath, variants, done, ref flattened); // parents first, so chains resolve

        var overridden = SavedPropertyNames(path);
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var parent = AssetDatabase.LoadAssetAtPath<Material>(parentPath);
        if (material == null || parent == null) return copied;
        if (material.shader != parent.shader && parent.shader != null && material.shader.name.Contains("InternalError")) material.shader = parent.shader;

        var shader = material.shader;
        for (int i = 0; i < shader.GetPropertyCount(); i++) {
            string name = shader.GetPropertyName(i);
            if (overridden.Contains(name) || !parent.HasProperty(name)) continue;
            switch (shader.GetPropertyType(i)) {
                case UnityEngine.Rendering.ShaderPropertyType.Texture:
                    material.SetTexture(name, parent.GetTexture(name));
                    material.SetTextureScale(name, parent.GetTextureScale(name));
                    material.SetTextureOffset(name, parent.GetTextureOffset(name));
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Color: material.SetColor(name, parent.GetColor(name)); break;
                case UnityEngine.Rendering.ShaderPropertyType.Vector: material.SetVector(name, parent.GetVector(name)); break;
                default: material.SetFloat(name, parent.GetFloat(name)); break;
            }
            copied++;
        }
        foreach (var keyword in parent.shaderKeywords) if (!material.IsKeywordEnabled(keyword)) material.EnableKeyword(keyword);
        if (!overridden.Contains("m_CustomRenderQueue")) material.renderQueue = parent.renderQueue;
        material.enableInstancing = material.enableInstancing || parent.enableInstancing;
        material.doubleSidedGI = parent.doubleSidedGI;
        EditorUtility.SetDirty(material);
        flattened++;
        return copied;
    }

    // property names written in the variant's own m_SavedProperties (its overrides)
    static HashSet<string> SavedPropertyNames (string path) {
        var set = new HashSet<string>();
        var text = File.ReadAllText(path);
        int start = text.IndexOf("m_SavedProperties:");
        if (start < 0) return set;
        int end = text.IndexOf("\n  m_", start + 20);
        var block = end > 0 ? text.Substring(start, end - start) : text.Substring(start);
        foreach (Match m in PropertyNameRegex.Matches(block)) set.Add(m.Groups[1].Value);
        return set;
    }
}
