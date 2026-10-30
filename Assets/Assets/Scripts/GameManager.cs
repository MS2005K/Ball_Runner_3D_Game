using UnityEngine;

public enum GameState { Menu, Playing, Won, Over }

/// <summary>
/// Game flow: start screen, playing, win, game over (ONE life, no continue), restart, HUD.
/// The level (ground, hurdles, coins, trees...) now lives in the Scene - this script only
/// finds the Coins and Hurdles that are in the scene and runs the rules.
/// </summary>
public class GameManager : MonoBehaviour
{
    public GameState State = GameState.Menu;
    public BallController ball;      // assigned in the Inspector (set by the bake tool)
    public CameraFollow cam;         // assigned in the Inspector (set by the bake tool)

    [HideInInspector] public int Coins;
    [HideInInspector] public int Lives = 1;
    public int TotalCoins { get; private set; }   // = number of Coin objects in the scene

    AudioSource sfx;
    Hurdle[] hurdles;
    Coin[] coins;
    Texture2D heart;
    GUIStyle btnStyle;

    static T[] All<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return FindObjectsByType<T>(FindObjectsSortMode.None);
#else
        return FindObjectsOfType<T>();
#endif
    }

    // ------------------------------------------------------------ setup
    void Start()
    {
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;
        heart = MakeHeart();

        if (ball == null) ball = All<BallController>()[0];
        if (cam == null) cam = All<CameraFollow>()[0];
        hurdles = All<Hurdle>();
        coins = All<Coin>();
        TotalCoins = coins.Length;

        cam.target = ball.transform;
        cam.Snap();
    }

    // ------------------------------------------------------------ flow
    public void StartGame()
    {
        Coins = 0; Lives = 1;
        State = GameState.Playing;
    }

    public void Restart()
    {
        // full reset: coins, life, ball, game state
        Coins = 0; Lives = 1;
        foreach (var c in coins) if (c != null) c.gameObject.SetActive(true);
        ball.ResetBall();
        cam.Snap();
        State = GameState.Playing;
    }

    void Update()
    {
        if (State != GameState.Playing) return;

        ball.Tick(Time.deltaTime);

        Vector3 bp = ball.transform.position;
        Vector2 flat = new Vector2(bp.x, bp.z);

        // hurdle collision -> instant game over (only life)
        for (int i = 0; i < hurdles.Length; i++)
            if (hurdles[i] != null && hurdles[i].gameObject.activeInHierarchy &&
                hurdles[i].Hits(flat, BallController.Radius * 0.85f)) { OnHit(); return; }

        // coin collection
        for (int i = 0; i < coins.Length; i++)
        {
            var c = coins[i];
            if (c == null || !c.gameObject.activeSelf) continue;
            if ((c.transform.position - bp).sqrMagnitude < 1.05f * 1.05f)
            {
                Collect(c);
                if (Coins >= TotalCoins) { Win(); return; }
            }
        }
    }

    void Collect(Coin c)
    {
        Coins++;
        sfx.PlayOneShot(SoundFx.Coin());
        CoinBurst.Spawn(c.transform.position, new Color(1f, 0.85f, 0.2f), 12);
        c.gameObject.SetActive(false);   // hidden, comes back on Restart
    }

    void OnHit()
    {
        Lives = 0;
        State = GameState.Over;
        sfx.PlayOneShot(SoundFx.GameOver());
        CoinBurst.Spawn(ball.transform.position, new Color(1f, 0.15f, 0.15f), 20);
        ball.visual.gameObject.SetActive(false);
    }

    void Win()
    {
        State = GameState.Won;
        sfx.PlayOneShot(SoundFx.Win());
    }

    // ------------------------------------------------------------ UI
    void OnGUI()
    {
        float s = Screen.height / 720f;
        GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
        float W = Screen.width / s, H = 720f;

        if (btnStyle == null)
        {
            btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 30, fontStyle = FontStyle.Bold };
        }

        if (State == GameState.Playing || State == GameState.Won || State == GameState.Over)
            DrawHud(W);

        switch (State)
        {
            case GameState.Menu: DrawMenu(W, H); break;
            case GameState.Won: DrawEnd(W, H, "YOU WIN!", new Color(1f, 0.85f, 0.2f),
                                        "Coins Collected: " + Coins + " / " + TotalCoins, null, "PLAY AGAIN"); break;
            case GameState.Over: DrawEnd(W, H, "Game Over", new Color(1f, 0.3f, 0.3f),
                                        "Coins Collected: " + Coins + " / " + TotalCoins,
                                        "Life Lost!\nYou have lost your only life.", "RESTART"); break;
        }
    }

    void DrawHud(float W)
    {
        Box(new Rect(14, 14, 250, 54), new Color(0, 0, 0, 0.35f));
        Label(new Rect(14, 14, 250, 54), "Coins: " + Coins + " / " + TotalCoins, 30, new Color(1f, 0.88f, 0.25f), TextAnchor.MiddleCenter);

        Box(new Rect(W - 214, 14, 200, 54), new Color(0, 0, 0, 0.35f));
        GUI.DrawTexture(new Rect(W - 206, 22, 38, 38), heart);
        Label(new Rect(W - 164, 14, 150, 54), "Lives: " + Lives, 30, Color.white, TextAnchor.MiddleLeft);
    }

    void DrawMenu(float W, float H)
    {
        float pw = 680, ph = 560;
        var r = new Rect((W - pw) / 2f, (H - ph) / 2f, pw, ph);
        Box(r, new Color(0.05f, 0.1f, 0.2f, 0.78f));
        Label(new Rect(r.x, r.y + 14, pw, 80), "3D BALL RUNNER", 62, new Color(1f, 0.3f, 0.3f), TextAnchor.MiddleCenter);
        Label(new Rect(r.x, r.y + 100, pw, 100), "Collect 50 Coins\nAvoid the Hurdles\nYou Have 1 Life", 28, Color.white, TextAnchor.MiddleCenter);
        Label(new Rect(r.x, r.y + 210, pw, 30), "CONTROLS", 22, new Color(1f, 0.88f, 0.25f), TextAnchor.MiddleCenter);
        Label(new Rect(r.x, r.y + 238, pw, 110), "W / \u2191 = Forward      S / \u2193 = Backward\nA / \u2190 = Left      D / \u2192 = Right", 22, Color.white, TextAnchor.UpperCenter);
        Label(new Rect(r.x + 40, r.y + 312, pw - 80, 130),
            "The ball does NOT move automatically - you control everything.\n3 tracks, static hurdles, no enemies. Hitting any hurdle = Game Over.\nCollect all 50 coins to win.",
            18, new Color(0.85f, 0.92f, 1f), TextAnchor.UpperCenter);
        if (GUI.Button(new Rect(r.x + (pw - 320) / 2f, r.y + ph - 100, 320, 70), "START GAME", btnStyle)) StartGame();
    }

    void DrawEnd(float W, float H, string title, Color tc, string line, string extra, string button)
    {
        GUI.color = new Color(1, 1, 1, 1);
        Box(new Rect(0, 0, W, H), new Color(0, 0, 0, 0.45f));
        float pw = 600, ph = extra != null ? 420 : 360;
        var r = new Rect((W - pw) / 2f, (H - ph) / 2f, pw, ph);
        Box(r, new Color(0.05f, 0.1f, 0.2f, 0.88f));
        Label(new Rect(r.x, r.y + 20, pw, 90), title, 70, tc, TextAnchor.MiddleCenter);
        Label(new Rect(r.x, r.y + 120, pw, 50), line, 32, Color.white, TextAnchor.MiddleCenter);
        if (extra != null) Label(new Rect(r.x, r.y + 175, pw, 90), extra, 26, new Color(1f, 0.7f, 0.7f), TextAnchor.UpperCenter);
        if (GUI.Button(new Rect(r.x + (pw - 320) / 2f, r.y + ph - 100, 320, 70), button, btnStyle)) Restart();
    }

    static void Box(Rect r, Color c)
    {
        var old = GUI.color; GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }

    static void Label(Rect r, string text, int size, Color col, TextAnchor anchor)
    {
        var st = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = FontStyle.Bold, alignment = anchor, wordWrap = true };
        st.normal.textColor = new Color(0, 0, 0, 0.7f);
        GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, st);
        st.normal.textColor = col;
        GUI.Label(r, text, st);
    }

    static Texture2D MakeHeart()
    {
        const int n = 64;
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        var px = new Color[n * n];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float fx = (x / (n - 1f) - 0.5f) * 2.8f;
                float fy = (y / (n - 1f) - 0.5f) * 2.8f + 0.15f;
                float a = fx * fx + fy * fy - 1f;
                bool inside = a * a * a - fx * fx * fy * fy * fy <= 0f;
                px[y * n + x] = inside ? new Color(0.95f, 0.1f, 0.2f, 1f) : new Color(0, 0, 0, 0);
            }
        t.SetPixels(px);
        t.filterMode = FilterMode.Bilinear;
        t.Apply();
        return t;
    }
}
