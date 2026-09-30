using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Pomocné IMGUI kreslenie (volať iba z OnGUI).</summary>
public static class UIDraw
{
    static GUIStyle style;

    public static void Fill(Rect r, Color c)
    {
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    public static void Text(Rect r, string text, int size, Color c, TextAnchor anchor, float u)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = false, clipping = TextClipping.Overflow };
        }
        style.fontSize = Mathf.Max(8, Mathf.RoundToInt(size * u));
        style.alignment = anchor;

        var shadow = r;
        shadow.x += 2 * u;
        shadow.y += 2 * u;
        style.normal.textColor = new Color(0, 0, 0, 0.7f * c.a);
        GUI.Label(shadow, text, style);
        style.normal.textColor = c;
        GUI.Label(r, text, style);
    }
}

/// <summary>Časovač, názov fázy, nápoveda, veľký stredový text, toasty a metre.</summary>
public class HUD : MonoBehaviour
{
    string phaseTitle = "";
    string sub = "";
    string hint = "";
    float hintUntil;
    string big = "";
    Color bigColor = Color.white;
    int bigSize = 140;
    string toast = "";
    float toastUntil;
    float timer;
    bool showTimer;
    readonly List<MeterUI> meters = new List<MeterUI>();

    public void ResetAll()
    {
        phaseTitle = sub = hint = big = "";
        showTimer = false;
        meters.Clear();
    }

    public void SetPhase(string title, string hintText)
    {
        phaseTitle = title;
        hint = hintText;
        hintUntil = Time.time + 1.5f;
        sub = "";
    }

    public void SetTimer(float seconds)
    {
        timer = seconds;
        showTimer = true;
    }

    public void HideTimer() => showTimer = false;

    /// <summary>Malý popis aktuálneho kroku pod názvom fázy.</summary>
    public void SetSub(string s) => sub = s;

    /// <summary>Veľký text v strede (odpočet, QTE, kašeľ). Prázdny reťazec = skryť.</summary>
    public void SetBig(string text, Color? color = null, int size = 140)
    {
        big = text;
        bigColor = color ?? Color.white;
        bigSize = size;
    }

    public void Toast(string text, float seconds = 1.1f)
    {
        toast = text;
        toastUntil = Time.time + seconds;
    }

    public MeterUI AddMeter(string label, float max, Vector2? zone = null)
    {
        var m = new MeterUI(label, max);
        if (zone.HasValue) m.WithZone(zone.Value);
        meters.Add(m);
        return m;
    }

    public void ClearMeters() => meters.Clear();

    void OnGUI()
    {
        float u = Screen.height / 720f;
        float w = Screen.width;
        float h = Screen.height;

        if (!string.IsNullOrEmpty(phaseTitle))
            UIDraw.Text(new Rect(20 * u, 14 * u, 600 * u, 40 * u), phaseTitle, 30, new Color(1f, 0.92f, 0.6f), TextAnchor.MiddleLeft, u);
        if (!string.IsNullOrEmpty(sub))
            UIDraw.Text(new Rect(20 * u, 54 * u, 800 * u, 30 * u), sub, 22, Color.white, TextAnchor.MiddleLeft, u);

        if (showTimer)
        {
            int secs = Mathf.CeilToInt(Mathf.Max(0f, timer));
            Color tc = secs <= 3 ? new Color(1f, 0.35f, 0.3f) : Color.white;
            UIDraw.Text(new Rect(w - 140 * u, 10 * u, 120 * u, 56 * u), secs + " s", 44, tc, TextAnchor.MiddleRight, u);
        }

        if (Time.time < hintUntil && !string.IsNullOrEmpty(hint))
        {
            float a = Mathf.Clamp01((hintUntil - Time.time) / 0.4f);
            UIDraw.Text(new Rect(0, h * 0.18f, w, 50 * u), hint, 34, new Color(1, 1, 1, a), TextAnchor.MiddleCenter, u);
        }

        if (!string.IsNullOrEmpty(big))
            UIDraw.Text(new Rect(0, h * 0.5f - 100 * u, w, 200 * u), big, bigSize, bigColor, TextAnchor.MiddleCenter, u);

        if (Time.time < toastUntil && !string.IsNullOrEmpty(toast))
        {
            float a = Mathf.Clamp01((toastUntil - Time.time) / 0.3f);
            UIDraw.Text(new Rect(0, h * 0.3f, w, 60 * u), toast, 44, new Color(0.6f, 1f, 0.7f, a), TextAnchor.MiddleCenter, u);
        }

        for (int i = 0; i < meters.Count; i++)
        {
            float mw = 460 * u, mh = 26 * u;
            var r = new Rect((w - mw) * 0.5f, h - (60 + i * 66) * u, mw, mh);
            meters[i].Draw(r, u);
        }
    }
}
