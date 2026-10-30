using System.Collections.Generic;
using UnityEngine;

public static class MeshUtil
{
    /// <summary>Ring in the XY plane (used for the golden coins).</summary>
    public static Mesh Torus(float R, float r, int seg = 22, int sides = 10)
    {
        var v = new List<Vector3>(); var n = new List<Vector3>(); var t = new List<int>();
        for (int i = 0; i <= seg; i++)
        {
            float u = i / (float)seg * Mathf.PI * 2f;
            Vector3 c = new Vector3(Mathf.Cos(u) * R, Mathf.Sin(u) * R, 0);
            for (int j = 0; j <= sides; j++)
            {
                float w = j / (float)sides * Mathf.PI * 2f;
                Vector3 d = new Vector3(Mathf.Cos(u) * Mathf.Cos(w), Mathf.Sin(u) * Mathf.Cos(w), Mathf.Sin(w));
                v.Add(c + d * r); n.Add(d);
            }
        }
        for (int i = 0; i < seg; i++)
            for (int j = 0; j < sides; j++)
            {
                int a = i * (sides + 1) + j, b = a + sides + 1;
                t.Add(a); t.Add(b); t.Add(a + 1);
                t.Add(a + 1); t.Add(b); t.Add(b + 1);
            }
        var m = new Mesh { name = "Torus" };
        m.SetVertices(v); m.SetNormals(n); m.SetTriangles(t, 0);
        return m;
    }

    /// <summary>Flat-shaded 4-sided pyramid (red spikes).</summary>
    public static Mesh Pyramid(float baseSize, float height)
    {
        float b = baseSize * 0.5f;
        Vector3 apex = new Vector3(0, height, 0);
        Vector3 p0 = new Vector3(-b, 0, -b), p1 = new Vector3(-b, 0, b),
                p2 = new Vector3(b, 0, b), p3 = new Vector3(b, 0, -b);
        var v = new List<Vector3>(); var t = new List<int>();
        Vector3 inside = new Vector3(0, height * 0.25f, 0);
        System.Action<Vector3, Vector3, Vector3> tri = (a, c, d) =>
        {
            Vector3 centroid = (a + c + d) / 3f;
            Vector3 nrm = Vector3.Cross(c - a, d - a);
            if (Vector3.Dot(nrm, centroid - inside) < 0) { var tmp = c; c = d; d = tmp; }
            int s = v.Count; v.Add(a); v.Add(c); v.Add(d);
            t.Add(s); t.Add(s + 1); t.Add(s + 2);
        };
        tri(p0, p1, apex); tri(p1, p2, apex); tri(p2, p3, apex); tri(p3, p0, apex);
        tri(p0, p1, p2); tri(p0, p2, p3);
        var m = new Mesh { name = "Pyramid" };
        m.SetVertices(v); m.SetTriangles(t, 0); m.RecalculateNormals(); m.RecalculateBounds();
        return m;
    }
}
