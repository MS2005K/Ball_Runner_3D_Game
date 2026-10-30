using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the whole cartoon low-poly world: 3 lanes, static hurdles, 50 coins and decoration.
/// All hurdles are STATIC. Every row leaves at least one free lane.
/// </summary>
public static class LevelBuilder
{
    public static readonly float[] LaneX = { -3.2f, 0f, 3.2f };
    public const int CoinCount = 50;
    static Mesh pyramid;
    const float PathLength = 230f;

    static readonly Color Wood     = new Color(0.66f, 0.36f, 0.16f);
    static readonly Color DarkWood = new Color(0.45f, 0.24f, 0.10f);
    static readonly Color Red      = new Color(0.9f, 0.1f, 0.14f);
    static readonly Color Grass    = new Color(0.36f, 0.78f, 0.18f);

    // ---------------------------------------------------------------- helpers
    static GameObject P(PrimitiveType t, Transform parent, Vector3 pos, Vector3 scale, Color c,
                        float smooth = 0.2f, Vector3 euler = default(Vector3))
    {
        var g = GameObject.CreatePrimitive(t);
        Object.DestroyImmediate(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localEulerAngles = euler;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = Mats.Make(c, smooth);
        return g;
    }

    static Transform Group(Transform parent, string name, Vector3 pos)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        return g.transform;
    }

    // ---------------------------------------------------------------- main
    public static void Build(Transform root, out List<Hurdle> obstacles, out List<Coin> coins)
    {
        var rnd = new System.Random(2024);
        pyramid = null;
        obstacles = new List<Hurdle>();
        coins = new List<Coin>();

        BuildGround(Group(root, "Ground", Vector3.zero));
        BuildPath(Group(root, "Path", Vector3.zero));
        BuildHurdles(Group(root, "Hurdles", Vector3.zero), rnd, obstacles);
        BuildCoins(Group(root, "Coins", Vector3.zero), rnd, obstacles, coins);
        BuildScenery(root, rnd);
        BuildFinish(root);
    }

    // ---------------------------------------------------------------- ground & path
    static void BuildGround(Transform root)
    {
        P(PrimitiveType.Cube, root, new Vector3(0, -0.5f, 100f), new Vector3(500, 1, 620), Grass, 0.05f);
        // lighter / darker grass patches for colorful terrain
        var r = new System.Random(5);
        for (int i = 0; i < 40; i++)
        {
            float x = (float)(r.NextDouble() * 120 - 60);
            if (Mathf.Abs(x) < 8f) x += Mathf.Sign(x) * 10f;
            float z = (float)(r.NextDouble() * 260 - 20);
            Color c = r.Next(2) == 0 ? new Color(0.5f, 0.86f, 0.22f) : new Color(0.28f, 0.68f, 0.17f);
            P(PrimitiveType.Cylinder, root, new Vector3(x, 0.01f, z),
              new Vector3(6 + (float)r.NextDouble() * 10, 0.01f, 6 + (float)r.NextDouble() * 10), c, 0.05f);
        }
    }

    static void BuildPath(Transform root)
    {
        float cz = PathLength * 0.5f - 15f;
        P(PrimitiveType.Cube, root, new Vector3(0, 0.01f, cz), new Vector3(10.6f, 0.04f, PathLength),
          new Color(0.74f, 0.84f, 0.16f), 0.05f);
        for (int i = 0; i < 3; i++)
        {
            Color c = i == 1 ? new Color(0.95f, 0.97f, 0.28f) : new Color(0.91f, 0.95f, 0.24f);
            P(PrimitiveType.Cube, root, new Vector3(LaneX[i], 0.03f, cz), new Vector3(3.0f, 0.06f, PathLength), c, 0.05f);
        }
    }

    // ---------------------------------------------------------------- hurdles
    static void BuildHurdles(Transform root, System.Random rnd, List<Hurdle> obs)
    {
        int type = 0;
        for (int row = 0; row < 16; row++)
        {
            float z = 16f + row * 11.5f;
            int count = (row % 3 == 2) ? 1 : 2;       // max 2 of 3 lanes -> never fully blocked
            int first = rnd.Next(3);
            int second = (first + 1 + rnd.Next(2)) % 3;
            for (int k = 0; k < count; k++)
            {
                int lane = k == 0 ? first : second;
                float x = LaneX[lane] + ((float)rnd.NextDouble() - 0.5f) * 0.3f;
                float zz = z + ((float)rnd.NextDouble() - 0.5f) * 2f;
                MakeHurdle(root, type % 7, x, zz, obs);
                type++;
            }
        }
    }

    static void MakeHurdle(Transform root, int type, float x, float z, List<Hurdle> obs)
    {
        var g = Group(root, "Hurdle_" + type, new Vector3(x, 0, z));
        switch (type)
        {
            case 0: // wooden barrier
                P(PrimitiveType.Cube, g, new Vector3(-1.0f, 0.65f, 0), new Vector3(0.22f, 1.3f, 0.22f), DarkWood);
                P(PrimitiveType.Cube, g, new Vector3(1.0f, 0.65f, 0), new Vector3(0.22f, 1.3f, 0.22f), DarkWood);
                P(PrimitiveType.Cube, g, new Vector3(0, 0.45f, 0), new Vector3(2.3f, 0.28f, 0.14f), Wood);
                P(PrimitiveType.Cube, g, new Vector3(0, 0.95f, 0), new Vector3(2.3f, 0.28f, 0.14f), Wood);
                Hb(g, obs, 1.15f, 0.2f); break;
            case 1: // red spikes
                var mesh = pyramid != null ? pyramid : (pyramid = MeshUtil.Pyramid(1.0f, 1.3f));
                Vector3[] sp = { new Vector3(-0.65f, 0, 0), new Vector3(0.6f, 0, 0.25f), new Vector3(0f, 0, -0.55f) };
                foreach (var p in sp)
                {
                    var s = new GameObject("Spike");
                    s.transform.SetParent(g, false); s.transform.localPosition = p;
                    s.transform.localEulerAngles = new Vector3(0, Random.Range(0f, 45f), 0);
                    s.AddComponent<MeshFilter>().sharedMesh = mesh;
                    s.AddComponent<MeshRenderer>().sharedMaterial = Mats.Make(Red, 0.35f);
                }
                Hb(g, obs, 1.1f, 0.9f); break;
            case 2: // wooden block / crate
                P(PrimitiveType.Cube, g, new Vector3(0, 0.8f, 0), new Vector3(1.6f, 1.6f, 1.6f), Wood);
                P(PrimitiveType.Cube, g, new Vector3(0, 0.08f, 0), new Vector3(1.7f, 0.16f, 1.7f), DarkWood);
                P(PrimitiveType.Cube, g, new Vector3(0, 1.52f, 0), new Vector3(1.7f, 0.16f, 1.7f), DarkWood);
                P(PrimitiveType.Cube, g, new Vector3(0, 0.8f, 0), new Vector3(1.7f, 0.14f, 1.7f), DarkWood, 0.2f, new Vector3(0, 0, 0));
                Hb(g, obs, 0.85f, 0.85f); break;
            case 3: // rock
                P(PrimitiveType.Sphere, g, new Vector3(0, 0.55f, 0), new Vector3(2.0f, 1.4f, 1.8f), new Color(0.55f, 0.56f, 0.6f), 0.1f, new Vector3(0, 25, 0));
                P(PrimitiveType.Sphere, g, new Vector3(0.5f, 0.3f, -0.6f), new Vector3(0.9f, 0.7f, 0.9f), new Color(0.62f, 0.63f, 0.66f), 0.1f);
                Hb(g, obs, 1.0f, 0.9f); break;
            case 4: // small brick wall
                P(PrimitiveType.Cube, g, new Vector3(0, 0.45f, 0), new Vector3(2.3f, 0.9f, 0.6f), new Color(0.78f, 0.42f, 0.30f), 0.1f);
                P(PrimitiveType.Cube, g, new Vector3(0, 0.95f, 0), new Vector3(2.4f, 0.14f, 0.7f), new Color(0.6f, 0.6f, 0.62f), 0.1f);
                Hb(g, obs, 1.2f, 0.4f); break;
            case 5: // wooden fence
                for (int i = -1; i <= 1; i++)
                    P(PrimitiveType.Cube, g, new Vector3(i * 1.1f, 0.5f, 0), new Vector3(0.18f, 1.0f, 0.18f), DarkWood);
                P(PrimitiveType.Cube, g, new Vector3(0, 0.35f, 0), new Vector3(2.4f, 0.14f, 0.1f), Wood);
                P(PrimitiveType.Cube, g, new Vector3(0, 0.78f, 0), new Vector3(2.4f, 0.14f, 0.1f), Wood);
                Hb(g, obs, 1.2f, 0.2f); break;
            default: // wooden ramp (rises toward +Z) - also lethal
                P(PrimitiveType.Cube, g, new Vector3(-0.9f, 0.5f, -1.0f), new Vector3(0.2f, 1.0f, 0.2f), DarkWood);
                P(PrimitiveType.Cube, g, new Vector3(0.9f, 0.5f, -1.0f), new Vector3(0.2f, 1.0f, 0.2f), DarkWood);
                P(PrimitiveType.Cube, g, new Vector3(0, 0.75f, 0), new Vector3(2.2f, 0.16f, 3.0f), new Color(0.78f, 0.4f, 0.18f), 0.2f, new Vector3(-30, 0, 0));
                P(PrimitiveType.Cube, g, new Vector3(0, 1.5f, 1.3f), new Vector3(2.4f, 0.16f, 0.3f), DarkWood);
                Hb(g, obs, 1.15f, 1.4f); break;
        }
    }

    // ---------------------------------------------------------------- coins
    static void BuildCoins(Transform root, System.Random rnd, List<Hurdle> obs, List<Coin> coins)
    {
        var mesh = MeshUtil.Torus(0.42f, 0.12f);
        var mat = Mats.Make(new Color(1f, 0.78f, 0.1f), 0.8f, 0.6f, 0.35f);
        int lane = 1;
        for (int i = 0; i < CoinCount; i++)
        {
            if (i % 5 == 0) lane = Mathf.Clamp(lane + rnd.Next(-1, 2), 0, 2);
            float z = 6f + i * 3.75f;

            int chosen = -1;
            for (int a = 0; a < 3 && chosen < 0; a++)
            {
                int l = (lane + a) % 3;
                if (!BlockedAt(obs, LaneX[l], z)) chosen = l;
            }
            if (chosen < 0) { chosen = lane; z += 2f; }

            var g = new GameObject("Coin_" + i);
            g.transform.SetParent(root, false);
            g.transform.position = new Vector3(LaneX[chosen], 1.0f, z);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            g.AddComponent<MeshRenderer>().sharedMaterial = mat;
            coins.Add(g.AddComponent<Coin>());
        }
    }

    static void Hb(Transform g, List<Hurdle> obs, float hx, float hz)
    {
        var h = g.gameObject.AddComponent<Hurdle>();
        h.half = new Vector2(hx, hz);
        obs.Add(h);
    }

    static bool BlockedAt(List<Hurdle> obs, float x, float z)
    {
        var p = new Vector2(x, z);
        foreach (var o in obs) if (o.Hits(p, 1.0f)) return true;
        return false;
    }

    // ---------------------------------------------------------------- scenery
    static void BuildScenery(Transform root, System.Random rnd)
    {
        Transform hills = Group(root, "Hills", Vector3.zero), trees = Group(root, "Trees", Vector3.zero),
                  flowers = Group(root, "Flowers", Vector3.zero), clouds = Group(root, "Clouds", Vector3.zero),
                  fences = Group(root, "SideFences", Vector3.zero), ramps = Group(root, "DecoRamps", Vector3.zero),
                  rocks = Group(root, "Rocks", Vector3.zero);
        // hills
        Color[] greens = { new Color(0.3f, 0.72f, 0.18f), new Color(0.42f, 0.82f, 0.2f), new Color(0.24f, 0.62f, 0.2f) };
        for (int i = 0; i < 22; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float x = side * (28f + (float)rnd.NextDouble() * 40f);
            float z = -20f + (float)rnd.NextDouble() * 270f;
            float w = 24f + (float)rnd.NextDouble() * 26f, h = 8f + (float)rnd.NextDouble() * 14f;
            P(PrimitiveType.Sphere, hills, new Vector3(x, -h * 0.25f, z), new Vector3(w, h, w), greens[rnd.Next(3)], 0.05f);
        }
        // backdrop hill with dark cliff (like the reference)
        P(PrimitiveType.Sphere, hills, new Vector3(0, -10f, 280f), new Vector3(140, 48, 70), new Color(0.35f, 0.76f, 0.2f), 0.05f);
        P(PrimitiveType.Cube, hills, new Vector3(5f, 8f, 252f), new Vector3(46, 18, 6), new Color(0.1f, 0.5f, 0.4f), 0.05f, new Vector3(0, 8, 0));
        P(PrimitiveType.Sphere, hills, new Vector3(-60f, -8f, 230f), new Vector3(70, 30, 60), new Color(0.3f, 0.7f, 0.2f), 0.05f);
        P(PrimitiveType.Sphere, hills, new Vector3(70f, -8f, 235f), new Vector3(80, 34, 60), new Color(0.4f, 0.8f, 0.22f), 0.05f);

        // trees
        for (int i = 0; i < 46; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float x = side * (9f + (float)rnd.NextDouble() * 20f);
            float z = -8f + (float)rnd.NextDouble() * 225f;
            MakeTree(trees, x, z, 0.8f + (float)rnd.NextDouble() * 0.7f);
        }
        // flowers
        for (int i = 0; i < 110; i++)
        {
            float side = rnd.Next(2) == 0 ? -1f : 1f;
            float x = side * (6.8f + (float)rnd.NextDouble() * 18f);
            float z = -8f + (float)rnd.NextDouble() * 225f;
            MakeFlower(flowers, x, z, rnd.Next(3));
        }
        // clouds
        for (int i = 0; i < 16; i++)
        {
            float x = -90f + (float)rnd.NextDouble() * 180f;
            float z = (float)rnd.NextDouble() * 280f;
            MakeCloud(clouds, new Vector3(x, 28f + (float)rnd.NextDouble() * 18f, z), 0.8f + (float)rnd.NextDouble());
        }
        // decorative side fences (not hurdles - outside the playable area)
        for (int s = -1; s <= 1; s += 2)
            for (float z = -6f; z < 205f; z += 6f)
            {
                var g = Group(fences, "SideFence", new Vector3(s * 6.2f, 0, z));
                P(PrimitiveType.Cube, g, new Vector3(0, 0.45f, 0), new Vector3(0.2f, 0.9f, 0.2f), DarkWood);
                P(PrimitiveType.Cube, g, new Vector3(0, 0.35f, 3f), new Vector3(0.1f, 0.12f, 6f), Wood);
                P(PrimitiveType.Cube, g, new Vector3(0, 0.7f, 3f), new Vector3(0.1f, 0.12f, 6f), Wood);
            }
        // decorative ramps / A-frames beside the path (like the reference image)
        for (int i = 0; i < 6; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            var g = Group(ramps, "DecoRamp", new Vector3(side * (11f + i), 0, 20f + i * 32f));
            g.localEulerAngles = new Vector3(0, side * 70f, 0);
            P(PrimitiveType.Cube, g, new Vector3(-1.0f, 0.9f, -1.2f), new Vector3(0.25f, 1.8f, 0.25f), DarkWood);
            P(PrimitiveType.Cube, g, new Vector3(1.0f, 0.9f, -1.2f), new Vector3(0.25f, 1.8f, 0.25f), DarkWood);
            P(PrimitiveType.Cube, g, new Vector3(0, 1.0f, 0), new Vector3(2.6f, 0.2f, 3.6f), new Color(0.78f, 0.4f, 0.18f), 0.2f, new Vector3(-35, 0, 0));
        }
        // decorative rocks
        for (int i = 0; i < 20; i++)
        {
            float side = rnd.Next(2) == 0 ? -1f : 1f;
            float s = 0.8f + (float)rnd.NextDouble() * 1.4f;
            P(PrimitiveType.Sphere, rocks, new Vector3(side * (8f + (float)rnd.NextDouble() * 14f), s * 0.3f, (float)rnd.NextDouble() * 210f),
              new Vector3(s * 1.4f, s, s * 1.2f), new Color(0.58f, 0.6f, 0.64f), 0.1f, new Vector3(0, rnd.Next(180), 0));
        }
    }

    static void MakeTree(Transform root, float x, float z, float s)
    {
        var g = Group(root, "Tree", new Vector3(x, 0, z));
        g.localScale = Vector3.one * s;
        P(PrimitiveType.Cylinder, g, new Vector3(0, 1.0f, 0), new Vector3(0.45f, 1.0f, 0.45f), new Color(0.5f, 0.28f, 0.12f));
        P(PrimitiveType.Sphere, g, new Vector3(0, 3.3f, 0), new Vector3(2.8f, 2.8f, 2.8f), new Color(0.2f, 0.7f, 0.2f), 0.1f, new Vector3(20, 30, 10));
        P(PrimitiveType.Sphere, g, new Vector3(0.5f, 4.0f, 0.2f), new Vector3(1.6f, 1.6f, 1.6f), new Color(0.28f, 0.78f, 0.24f), 0.1f);
    }

    static void MakeFlower(Transform root, float x, float z, int kind)
    {
        var g = Group(root, "Flower", new Vector3(x, 0, z));
        Color petals = kind == 0 ? Color.white : (kind == 1 ? new Color(1f, 0.6f, 0.75f) : new Color(1f, 0.95f, 0.5f));
        P(PrimitiveType.Cylinder, g, new Vector3(0, 0.25f, 0), new Vector3(0.06f, 0.25f, 0.06f), new Color(0.15f, 0.6f, 0.15f));
        P(PrimitiveType.Sphere, g, new Vector3(0, 0.55f, 0), new Vector3(0.55f, 0.12f, 0.55f), petals, 0.2f);
        P(PrimitiveType.Sphere, g, new Vector3(0, 0.6f, 0), new Vector3(0.22f, 0.12f, 0.22f), new Color(1f, 0.85f, 0.1f));
        P(PrimitiveType.Sphere, g, new Vector3(0.3f, 0.12f, 0), new Vector3(0.5f, 0.12f, 0.2f), new Color(0.1f, 0.55f, 0.15f));
    }

    static void MakeCloud(Transform root, Vector3 pos, float s)
    {
        var g = Group(root, "Cloud", pos);
        g.localScale = Vector3.one * s * 4f;
        Color c = new Color(0.95f, 0.98f, 1f);
        P(PrimitiveType.Sphere, g, new Vector3(0, 0, 0), new Vector3(2.4f, 1.3f, 1.4f), c, 0f);
        P(PrimitiveType.Sphere, g, new Vector3(-1.2f, -0.1f, 0), new Vector3(1.6f, 1.0f, 1.2f), c, 0f);
        P(PrimitiveType.Sphere, g, new Vector3(1.2f, -0.1f, 0), new Vector3(1.7f, 1.0f, 1.2f), c, 0f);
        P(PrimitiveType.Sphere, g, new Vector3(0.2f, 0.5f, 0), new Vector3(1.4f, 1.1f, 1.1f), c, 0f);
        foreach (var r in g.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    static void BuildFinish(Transform root)
    {
        var g = Group(root, "Finish", new Vector3(0, 0, 206f));
        P(PrimitiveType.Cube, g, new Vector3(-5.4f, 2.5f, 0), new Vector3(0.5f, 5f, 0.5f), DarkWood);
        P(PrimitiveType.Cube, g, new Vector3(5.4f, 2.5f, 0), new Vector3(0.5f, 5f, 0.5f), DarkWood);
        for (int i = 0; i < 12; i++)
            P(PrimitiveType.Cube, g, new Vector3(-5.5f + i * 1.0f, 4.7f, 0), new Vector3(1.0f, 0.9f, 0.3f),
              i % 2 == 0 ? Color.white : new Color(0.15f, 0.15f, 0.15f), 0.1f);
    }
}
