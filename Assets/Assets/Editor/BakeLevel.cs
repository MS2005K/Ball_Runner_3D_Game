using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu: 3D Ball Runner > Bake Level Into Scene
/// Creates every object of the game (ground, path, hurdles, coins, trees, ball, camera, sun,
/// GameManager) as NORMAL scene objects, and saves the generated materials / meshes as asset files.
/// After this you can edit everything in the Scene view without pressing Play.
/// </summary>
public static class BakeLevel
{
    const string Dir     = "Assets/BallRunnerAssets";
    const string MatDir  = Dir + "/Materials";
    const string MeshDir = Dir + "/Meshes";

    [MenuItem("3D Ball Runner/Bake Level Into Scene")]
    static void Bake()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("3D Ball Runner", "Pehle Play band karein, phir dobara try karein.", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("3D Ball Runner",
                "Yeh current scene mein poora game level (hurdles, coins, trees, ball...) bana dega.\n\n" +
                "Pehle se bani 'Level' / 'Ball' / 'GameManager' objects aur BallRunnerAssets folder replace ho jayenge " +
                "(aap ki editing reset ho jayegi).\n\nContinue?", "Bake", "Cancel"))
            return;

        var scene = EditorSceneManager.GetActiveScene();

        // ---- 1. remove an older bake / old runtime leftovers
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "Level" || go.name == "Ball" || go.GetComponent<GameManager>() != null)
                Object.DestroyImmediate(go);
        AssetDatabase.DeleteAsset(Dir);
        AssetDatabase.CreateFolder("Assets", "BallRunnerAssets");
        AssetDatabase.CreateFolder(Dir, "Materials");
        AssetDatabase.CreateFolder(Dir, "Meshes");
        Mats.ClearCache();

        // ---- 2. the level (ground, path, hurdles, coins, scenery, finish)
        var level = new GameObject("Level");
        List<Hurdle> hurdles; List<Coin> coins;
        LevelBuilder.Build(level.transform, out hurdles, out coins);

        // ---- 3. ball
        var ball = BuildBall();

        // ---- 4. camera (re-uses the scene's Main Camera if it has one)
        var cam = Camera.main;
        if (cam == null)
        {
            var cg = new GameObject("Main Camera");
            cg.tag = "MainCamera";
            cam = cg.AddComponent<Camera>();
        }
        if (cam.GetComponent<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.25f, 0.76f, 1f);
        cam.fieldOfView = 60f;
        cam.farClipPlane = 600f;
        var follow = cam.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = ball.transform;
        follow.Snap();                       // camera starts behind the ball, also in Edit mode

        // ---- 5. sun (re-uses the first Directional Light)
        Light sun = null;
        foreach (var l in Find<Light>()) if (l.type == LightType.Directional) { sun = l; break; }
        if (sun == null) { var lg = new GameObject("Sun"); sun = lg.AddComponent<Light>(); sun.type = LightType.Directional; }
        sun.color = new Color(1f, 0.97f, 0.88f);
        sun.intensity = 1.15f;
        sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // ---- 6. sky / fog (stored inside the scene)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.62f, 0.7f, 0.82f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.55f, 0.84f, 1f);
        RenderSettings.fogStartDistance = 110f;
        RenderSettings.fogEndDistance = 330f;

        // ---- 7. game manager
        var gmGo = new GameObject("GameManager");
        var gm = gmGo.AddComponent<GameManager>();
        gm.ball = ball;
        gm.cam = follow;

        // ---- 8. save generated materials + meshes as real asset files
        SaveShared(level);
        SaveShared(ball.gameObject);
        AssetDatabase.SaveAssets();

        EditorSceneManager.MarkSceneDirty(scene);
        if (!string.IsNullOrEmpty(scene.path)) EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = level;
        var sv = SceneView.lastActiveSceneView;
        if (sv != null) sv.LookAt(new Vector3(0f, 3f, 12f), Quaternion.Euler(22f, 0f, 0f), 40f);

        EditorUtility.DisplayDialog("3D Ball Runner",
            "Level scene mein ban gaya: " + hurdles.Count + " hurdles, " + coins.Count + " coins.\n\n" +
            "Ab Scene view mein edit karein aur Ctrl+S karein. Game Play karne par pehle jaisa chalega.", "OK");
    }

    // ------------------------------------------------------------------ ball
    static BallController BuildBall()
    {
        var root = new GameObject("Ball");
        root.transform.position = new Vector3(0f, BallController.Radius, 0f);

        var vis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        vis.name = "BallVisual";
        Object.DestroyImmediate(vis.GetComponent<Collider>());
        vis.transform.SetParent(root.transform, false);
        vis.transform.localScale = Vector3.one * (BallController.Radius * 2f);
        vis.GetComponent<Renderer>().sharedMaterial = Mats.Make(new Color(0.9f, 0.05f, 0.07f), 0.92f, 0.1f);

        // light spots so the rolling is clearly visible
        Vector3[] dirs = { new Vector3(0, 1, 0.2f), new Vector3(1, 0.1f, 0.3f), new Vector3(-0.7f, -0.5f, 0.6f),
                           new Vector3(0.2f, -1, -0.3f), new Vector3(-1, 0.2f, -0.2f), new Vector3(0.3f, 0.2f, -1) };
        foreach (var d in dirs)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = "Spot";
            Object.DestroyImmediate(s.GetComponent<Collider>());
            s.transform.SetParent(vis.transform, false);
            s.transform.localPosition = d.normalized * 0.5f;
            s.transform.localScale = Vector3.one * 0.2f;
            var r = s.GetComponent<Renderer>();
            r.sharedMaterial = Mats.Make(new Color(1f, 0.55f, 0.55f), 0.9f);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        var bc = root.AddComponent<BallController>();
        bc.visual = vis.transform;
        return bc;
    }

    // ------------------------------------------------------------------ save generated assets
    static void SaveShared(GameObject root)
    {
        int i = 0;
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            var m = r.sharedMaterial;
            if (m == null || EditorUtility.IsPersistent(m)) continue;
            m.name = "Mat_" + ColorUtility.ToHtmlStringRGB(m.color) + "_" + (i++);
            AssetDatabase.CreateAsset(m, MatDir + "/" + m.name + ".mat");
        }
        foreach (var f in root.GetComponentsInChildren<MeshFilter>(true))
        {
            var me = f.sharedMesh;
            if (me == null || EditorUtility.IsPersistent(me)) continue;
            AssetDatabase.CreateAsset(me, AssetDatabase.GenerateUniqueAssetPath(MeshDir + "/" + me.name + ".asset"));
        }
    }

    static T[] Find<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
#else
        return Object.FindObjectsOfType<T>();
#endif
    }
}
