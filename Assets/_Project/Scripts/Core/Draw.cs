using UnityEngine;

/// <summary>Procedurálne sprity (štvorec, kruh, medzikružie), aby hra nepotrebovala žiadne importované assety.</summary>
public static class Draw
{
    static Sprite square, circle, ring;

    public static Sprite Square
    {
        get
        {
            if (square == null)
            {
                var t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                t.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                t.Apply();
                square = Sprite.Create(t, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f); // 1 unit
            }
            return square;
        }
    }

    public static Sprite CircleSprite
    {
        get
        {
            if (circle == null) circle = MakeRound(false);
            return circle;
        }
    }

    public static Sprite RingSprite
    {
        get
        {
            if (ring == null) ring = MakeRound(true);
            return ring;
        }
    }

    static Sprite MakeRound(bool ringOnly)
    {
        const int s = 128;
        var t = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var px = new Color[s * s];
        for (int y = 0; y < s; y++)
        for (int x = 0; x < s; x++)
        {
            float dx = (x + 0.5f) / s * 2f - 1f;
            float dy = (y + 0.5f) / s * 2f - 1f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            float a = ringOnly
                ? Mathf.Clamp01(1f - Mathf.Abs(d - 0.86f) / 0.13f)
                : Mathf.Clamp01((1f - d) * s * 0.5f);
            px[y * s + x] = new Color(1, 1, 1, a);
        }
        t.SetPixels(px);
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s); // priemer 1 unit
    }

    public static SpriteRenderer Make(Transform parent, string name, Sprite sprite, Vector2 pos, Vector2 size, Color color, int order, float rot = 0f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0, 0, rot);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    public static SpriteRenderer Rect(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int order, float rot = 0f)
        => Make(parent, name, Square, pos, size, color, order, rot);

    public static SpriteRenderer Circle(Transform parent, string name, Vector2 pos, float diameter, Color color, int order)
        => Make(parent, name, CircleSprite, pos, new Vector2(diameter, diameter), color, order);

    public static SpriteRenderer Ring(Transform parent, string name, Vector2 pos, float diameter, Color color, int order)
        => Make(parent, name, RingSprite, pos, new Vector2(diameter, diameter), color, order);

    public static void Clear(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--) Object.Destroy(t.GetChild(i).gameObject);
    }
}
