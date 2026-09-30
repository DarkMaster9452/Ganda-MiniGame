using UnityEngine;

/// <summary>Sprite hotovej rolky vo 3 variantoch podľa kvality: rovná / krivá / roztrhnutá.</summary>
public static class RollVisual
{
    public enum Variant { Straight, Crooked, Torn }

    public static Variant VariantFor(float quality) =>
        quality >= 0.75f ? Variant.Straight : quality >= 0.45f ? Variant.Crooked : Variant.Torn;

    /// <summary>Vytvorí rolku so stredom v pos; široký (horiaci) koniec je vpravo. Ak lit, pridá "Ember" a "Glow".</summary>
    public static Transform Build(Transform parent, Vector2 pos, float scale, float quality, int order, bool lit, float rot = 0f)
    {
        var root = new GameObject("Roll").transform;
        root.SetParent(parent, false);
        root.localPosition = pos;
        root.localRotation = Quaternion.Euler(0, 0, rot);
        root.localScale = new Vector3(scale, scale, 1f);

        var variant = VariantFor(quality);
        var paper = variant == Variant.Torn ? new Color(0.88f, 0.68f, 0.62f) : new Color(0.97f, 0.95f, 0.88f);

        // filter
        Draw.Rect(root, "Filter", new Vector2(-2.75f, 0f), new Vector2(0.55f, 0.36f), new Color(0.75f, 0.6f, 0.4f), order);

        const int n = 6;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)(n - 1);
            float h = Mathf.Lerp(0.42f, 0.9f, t);
            float x = -2.2f + i * 0.92f;
            float y = 0f, r = 0f;
            if (variant == Variant.Crooked)
            {
                y = Mathf.Sin(i * 1.7f) * 0.12f;
                r = Mathf.Sin(i * 1.2f) * 9f;
            }
            Draw.Rect(root, "Seg" + i, new Vector2(x, y), new Vector2(0.94f, h), paper, order, r);
            // šev
            Draw.Rect(root, "Seam" + i, new Vector2(x, y + h * 0.18f), new Vector2(0.94f, 0.03f), new Color(0, 0, 0, 0.12f), order + 1, r);
        }

        if (variant == Variant.Torn)
        {
            Draw.Rect(root, "Tear", new Vector2(0.4f, 0.12f), new Vector2(0.22f, 0.6f), new Color(0.15f, 0.1f, 0.08f), order + 2, 12f);
            Draw.Rect(root, "Tear2", new Vector2(-1.2f, -0.1f), new Vector2(0.16f, 0.4f), new Color(0.15f, 0.1f, 0.08f), order + 2, -15f);
        }

        // hlava (tabak) na širokom konci
        Draw.Circle(root, "Head", new Vector2(2.9f, 0f), 0.85f, new Color(0.35f, 0.5f, 0.2f), order + 1);

        if (lit)
        {
            Draw.Circle(root, "Glow", new Vector2(3.0f, 0f), 1.8f, new Color(1f, 0.5f, 0.1f, 0.35f), order + 2);
            Draw.Circle(root, "Ember", new Vector2(3.0f, 0f), 0.75f, new Color(1f, 0.45f, 0.1f), order + 3);
        }
        return root;
    }
}
