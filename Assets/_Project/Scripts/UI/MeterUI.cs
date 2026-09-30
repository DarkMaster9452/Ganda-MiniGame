using UnityEngine;

/// <summary>Znovupoužiteľný meter (0..Max) s voliteľnou zelenou zónou. Kreslí ho HUD.</summary>
public class MeterUI
{
    public string Label;
    public float Value;
    public float Max = 1f;
    public bool HasZone;
    public Vector2 Zone;

    public MeterUI(string label, float max = 1f)
    {
        Label = label;
        Max = max;
    }

    public MeterUI WithZone(Vector2 zone)
    {
        HasZone = true;
        Zone = zone;
        return this;
    }

    public bool InZone => HasZone && Value >= Zone.x && Value <= Zone.y;

    public void Draw(Rect r, float u)
    {
        UIDraw.Fill(new Rect(r.x - 3 * u, r.y - 3 * u, r.width + 6 * u, r.height + 6 * u), new Color(0, 0, 0, 0.6f));
        UIDraw.Fill(r, new Color(0.15f, 0.15f, 0.17f, 0.95f));

        if (HasZone)
        {
            float zx = r.x + r.width * (Zone.x / Max);
            float zw = r.width * ((Zone.y - Zone.x) / Max);
            UIDraw.Fill(new Rect(zx, r.y, zw, r.height), new Color(0.25f, 0.75f, 0.35f, 0.55f));
        }

        float v = Mathf.Clamp01(Value / Max);
        Color c = !HasZone ? new Color(0.95f, 0.75f, 0.2f)
                : InZone ? new Color(0.35f, 0.95f, 0.45f)
                : Value > Zone.y ? new Color(0.95f, 0.3f, 0.25f)
                : new Color(0.95f, 0.75f, 0.2f);
        UIDraw.Fill(new Rect(r.x, r.y + r.height * 0.7f, r.width * v, r.height * 0.3f), c);
        UIDraw.Fill(new Rect(r.x + r.width * v - 2 * u, r.y - 4 * u, 4 * u, r.height + 8 * u), Color.white);

        UIDraw.Text(new Rect(r.x, r.y - 26 * u, r.width, 24 * u), Label, 20, Color.white, TextAnchor.MiddleCenter, u);
    }
}
