using UnityEngine;

/// <summary>Výsledková obrazovka: percentá, hviezdy, high score, konfety pri 3★.</summary>
public class ResultsScreen : MonoBehaviour
{
    static readonly string[] PhaseNames = { "Drvenie", "Usporiadanie", "Balenie", "Dym" };

    bool visible;
    float final, high, bonus;
    bool newHigh;
    int stars;
    float[] phaseScores = new float[4];
    float shownAt;
    Texture2D starTex;

    public void Show(float finalScore, int starCount, float highScore, bool isNewHigh, float bonusPct, float[] scores)
    {
        final = finalScore;
        stars = starCount;
        high = highScore;
        newHigh = isNewHigh;
        bonus = bonusPct;
        phaseScores = (float[])scores.Clone();
        shownAt = Time.time;
        visible = true;
    }

    public void Hide() => visible = false;

    void OnGUI()
    {
        if (!visible) return;
        if (starTex == null) starTex = MakeStar();

        float u = Screen.height / 720f;
        float w = Screen.width, h = Screen.height;
        UIDraw.Fill(new Rect(0, 0, w, h), new Color(0, 0, 0, 0.72f));

        if (stars == 3) Confetti(w, h, u);

        UIDraw.Text(new Rect(0, 30 * u, w, 60 * u), "VÝSLEDOK", 48, new Color(1f, 0.92f, 0.6f), TextAnchor.MiddleCenter, u);

        // odpočítavanie percent
        float t = Mathf.Clamp01((Time.time - shownAt) / 1.0f);
        int pct = Mathf.RoundToInt(final * 100f * t);
        UIDraw.Text(new Rect(0, 100 * u, w, 130 * u), pct + " %", 110, Color.white, TextAnchor.MiddleCenter, u);

        // hviezdy
        float size = 80 * u, gap = 20 * u;
        float sx = (w - (3 * size + 2 * gap)) * 0.5f;
        for (int i = 0; i < 3; i++)
        {
            bool on = i < stars;
            float pop = on ? Mathf.Clamp01((Time.time - shownAt - 0.4f - i * 0.25f) / 0.25f) : 1f;
            float s = size * Mathf.Lerp(1.5f, 1f, pop);
            GUI.color = on ? new Color(1f, 0.85f, 0.2f, pop) : new Color(0.35f, 0.35f, 0.35f, 1f);
            GUI.DrawTexture(new Rect(sx + i * (size + gap) - (s - size) * 0.5f, 240 * u - (s - size) * 0.5f, s, s), starTex);
        }
        GUI.color = Color.white;

        float y = 350 * u;
        for (int i = 0; i < 4; i++)
        {
            string line = PhaseNames[i] + ":  " + Mathf.RoundToInt(phaseScores[i] * 100f) + " %";
            UIDraw.Text(new Rect(0, y, w, 32 * u), line, 26, Color.white, TextAnchor.MiddleCenter, u);
            y += 34 * u;
        }
        UIDraw.Text(new Rect(0, y, w, 32 * u), "Bonus za čas:  +" + Mathf.RoundToInt(bonus * 100f) + " %", 26, new Color(0.6f, 1f, 0.7f), TextAnchor.MiddleCenter, u);
        y += 46 * u;

        string hs = newHigh ? "NOVÝ REKORD!  " + Mathf.RoundToInt(high * 100f) + " %" : "Najlepšie skóre:  " + Mathf.RoundToInt(high * 100f) + " %";
        UIDraw.Text(new Rect(0, y, w, 36 * u), hs, 30, newHigh ? new Color(1f, 0.85f, 0.2f) : Color.white, TextAnchor.MiddleCenter, u);

        float blink = 0.6f + 0.4f * Mathf.Sin(Time.time * 4f);
        UIDraw.Text(new Rect(0, h - 70 * u, w, 40 * u), "Space = znova", 32, new Color(1, 1, 1, blink), TextAnchor.MiddleCenter, u);
    }

    void Confetti(float w, float h, float u)
    {
        var old = Random.state;
        Random.InitState(1234);
        for (int i = 0; i < 70; i++)
        {
            float sx = Random.value, sy = Random.value, speed = 0.15f + Random.value * 0.25f, freq = 1f + Random.value * 3f;
            var col = Color.HSVToRGB(Random.value, 0.7f, 1f);
            float y = ((sy + Time.time * speed) % 1.1f - 0.05f) * h;
            float x = sx * w + Mathf.Sin(Time.time * freq + i) * 25f * u;
            UIDraw.Fill(new Rect(x, y, 10 * u, 6 * u), col);
        }
        Random.state = old;
    }

    static Texture2D MakeStar()
    {
        const int s = 96;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var verts = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float ang = Mathf.PI / 2f + i * Mathf.PI / 5f;
            float r = (i % 2 == 0 ? 0.48f : 0.2f) * s;
            verts[i] = new Vector2(s * 0.5f + Mathf.Cos(ang) * r, s * 0.5f + Mathf.Sin(ang) * r);
        }
        var px = new Color[s * s];
        for (int y = 0; y < s; y++)
        for (int x = 0; x < s; x++)
        {
            bool inside = false;
            for (int i = 0, j = 9; i < 10; j = i++)
            {
                if ((verts[i].y > y) != (verts[j].y > y) &&
                    x < (verts[j].x - verts[i].x) * (y - verts[i].y) / (verts[j].y - verts[i].y) + verts[i].x)
                    inside = !inside;
            }
            px[y * s + x] = inside ? Color.white : new Color(1, 1, 1, 0);
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }
}
