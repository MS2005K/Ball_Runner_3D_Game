using System;
using UnityEngine;

/// <summary>Procedurally generated sound effects (no audio files needed).</summary>
public static class SoundFx
{
    static AudioClip coin, over, win;

    static AudioClip Make(string name, float dur, Func<float, float> f)
    {
        const int rate = 44100;
        int n = (int)(rate * dur);
        var d = new float[n];
        for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(f(i / (float)rate), -1f, 1f);
        var c = AudioClip.Create(name, n, 1, rate, false);
        c.SetData(d, 0);
        return c;
    }

    public static AudioClip Coin()
    {
        if (coin == null)
            coin = Make("coin", 0.35f, t =>
            {
                float freq = t < 0.08f ? 988f : 1319f;
                float env = Mathf.Min(1f, t * 400f) * Mathf.Exp(-t * 9f);
                return Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
            });
        return coin;
    }

    public static AudioClip GameOver()
    {
        if (over == null)
            over = Make("over", 0.8f, t =>
            {
                float ph = 2f * Mathf.PI * (420f * t - 260f * t * t);
                float env = Mathf.Max(0f, 1f - t / 0.8f);
                return (Mathf.Sin(ph) + 0.4f * Mathf.Sin(ph * 2f)) * env * 0.35f;
            });
        return over;
    }

    public static AudioClip Win()
    {
        if (win == null)
        {
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
            win = Make("win", 0.9f, t =>
            {
                int idx = Mathf.Min(3, (int)(t / 0.17f));
                float lt = t - idx * 0.17f;
                float env = Mathf.Min(1f, lt * 300f) * Mathf.Exp(-lt * 5f);
                return Mathf.Sin(2f * Mathf.PI * notes[idx] * t) * env * 0.45f;
            });
        }
        return win;
    }
}
