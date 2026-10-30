using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Creates (and caches) simple colored materials for Built-in RP or URP.</summary>
public static class Mats
{
    static Shader shader;
    static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

    public static void ClearCache() { cache.Clear(); shader = null; }

    public static Material Make(Color c, float smooth = 0.3f, float metal = 0f, float emission = 0f)
    {
        string key = c.ToString() + smooth + metal + emission;
        Material m;
        if (cache.TryGetValue(key, out m) && m != null) return m;

        if (shader == null)
        {
            shader = GraphicsSettings.currentRenderPipeline != null
                ? Shader.Find("Universal Render Pipeline/Lit")
                : Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");
        }
        m = new Material(shader);
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metal);
        if (emission > 0f)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c * emission);
        }
        cache[key] = m;
        return m;
    }
}
