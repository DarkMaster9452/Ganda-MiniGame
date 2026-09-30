using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedurálne SFX a lo-fi hudobná slučka generované za behu (žiadne audio súbory).
/// Neskôr sa dajú nahradiť reálnymi klipmi (freesound CC0) v <see cref="Play"/>.
/// </summary>
public static class Sfx
{
    const int Rate = 22050;

    static AudioSource sfxSrc, musicSrc;
    static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    static void Init()
    {
        if (sfxSrc != null) return;
        clips.Clear();
        var go = new GameObject("Audio");
        Object.DontDestroyOnLoad(go);
        sfxSrc = go.AddComponent<AudioSource>();
        musicSrc = go.AddComponent<AudioSource>();
        musicSrc.loop = true;
        musicSrc.volume = 0.22f;
    }

    public static void Play(string id, float volume = 0.6f)
    {
        Init();
        if (!clips.TryGetValue(id, out var clip))
        {
            clip = Build(id);
            if (clip == null) return;
            clips[id] = clip;
        }
        sfxSrc.PlayOneShot(clip, volume);
    }

    public static void StartMusic()
    {
        Init();
        if (musicSrc.isPlaying) return;
        musicSrc.clip = BuildMusic();
        musicSrc.Play();
    }

    static float Noise() => Random.value * 2f - 1f;

    static AudioClip Make(string name, float dur, System.Func<float, float> f)
    {
        int n = Mathf.CeilToInt(dur * Rate);
        var data = new float[n];
        for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)Rate), -1f, 1f);
        var c = AudioClip.Create(name, n, 1, Rate, false);
        c.SetData(data, 0);
        return c;
    }

    static AudioClip Build(string id)
    {
        float lp = 0f;
        switch (id)
        {
            case "tick":
                return Make(id, 0.05f, t => Noise() * Mathf.Exp(-t * 110f) * 0.5f);
            case "scrape":
                return Make(id, 0.09f, t => Noise() * (0.5f + 0.5f * Mathf.Sin(t * 500f)) * Mathf.Exp(-t * 35f) * 0.45f);
            case "pour":
                return Make(id, 0.09f, t => { lp += 0.25f * (Noise() - lp); return lp * Mathf.Exp(-t * 20f) * 1.2f; });
            case "click":
                return Make(id, 0.04f, t => Mathf.Sin(t * 2f * Mathf.PI * 620f) * Mathf.Exp(-t * 90f) * 0.5f);
            case "ding":
                return Make(id, 0.4f, t => (Mathf.Sin(t * 2f * Mathf.PI * 880f) + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 1320f)) * Mathf.Exp(-t * 8f) * 0.4f);
            case "buzz":
                return Make(id, 0.22f, t => (Mathf.Sin(t * 2f * Mathf.PI * 110f) > 0 ? 0.3f : -0.3f) * Mathf.Exp(-t * 9f));
            case "cough":
                return Make(id, 0.4f, t => Noise() * (0.5f + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 22f)) * Mathf.Exp(-t * 5f) * 0.7f);
            case "exhale":
                return Make(id, 0.6f, t => { lp += 0.2f * (Noise() - lp); return lp * Mathf.Sin(Mathf.PI * t / 0.6f) * 1.4f; });
            case "win":
                return Make(id, 0.7f, t =>
                {
                    float f = t < 0.2f ? 523f : t < 0.4f ? 659f : 784f;
                    return Mathf.Sin(t * 2f * Mathf.PI * f) * Mathf.Exp(-(t % 0.2f) * 6f) * 0.4f;
                });
            case "lighter":
                return Make(id, 0.25f, t => Noise() * Mathf.Exp(-t * 25f) * 0.35f + Mathf.Sin(t * 2f * Mathf.PI * 2400f) * Mathf.Exp(-t * 60f) * 0.2f);
        }
        return null;
    }

    // Am7 - Fmaj7 - Cmaj7 - G6, každý akord 2 s => 8 s slučka
    static readonly float[][] Chords =
    {
        new[] { 110f, 130.8f, 164.8f, 196f },
        new[] { 87.3f, 110f, 130.8f, 164.8f },
        new[] { 130.8f, 164.8f, 196f, 246.9f },
        new[] { 98f, 123.5f, 146.8f, 164.8f },
    };

    static AudioClip BuildMusic()
    {
        return Make("lofi", 8f, t =>
        {
            int ci = Mathf.Min(3, (int)(t / 2f));
            float local = t - ci * 2f;
            float env = Mathf.Min(1f, local * 3f) * Mathf.Min(1f, (2f - local) * 3f);
            float s = 0f;
            foreach (float f in Chords[ci]) s += Mathf.Sin(t * 2f * Mathf.PI * f) * 0.12f;
            float hat = Noise() * Mathf.Exp(-(t % 0.5f) * 70f) * 0.12f;
            return s * env + hat;
        });
    }
}
