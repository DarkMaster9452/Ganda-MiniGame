using UnityEngine;

/// <summary>Fáza 2 – usporiadanie: WASD pohybuje kurzorom, držanie Space sype materiál na papierik (8 segmentov, cieľ ~0.7).</summary>
public class ArrangePhase : PhaseBase
{
    const int Segments = 8;
    const float PaperW = 12f, PaperH = 1.8f, SegW = PaperW / Segments;
    const float BarBaseY = 2.3f, BarMaxH = 1.4f;

    [SerializeField] float target = 0.7f;
    [SerializeField] float pourRate = 1.1f;      // fill/s
    [SerializeField] float cursorSpeed = 6f;
    [SerializeField] float baseSupply = 7.5f;    // pri priemernej kvalite drvenia
    [SerializeField] float maxFill = 1.3f;

    readonly Vector2 paperCenter = new Vector2(0f, -0.4f);
    readonly float[] fill = new float[Segments];
    readonly SpriteRenderer[] heaps = new SpriteRenderer[Segments];
    readonly SpriteRenderer[] bars = new SpriteRenderer[Segments];

    Transform cursor, stream;
    Vector2 pos;
    float supply, supplyMax, lost, sfxTimer;
    MeterUI meter;

    void Awake()
    {
        duration = 12f;
        Title = "2. Usporiadanie";
        Hint = "WASD = pohyb     |     drž Space = sypať rovnomerne";
    }

    public override void Enter(InputReader input, GameContext ctx)
    {
        base.Enter(input, ctx);
        System.Array.Clear(fill, 0, Segments);
        lost = 0f; sfxTimer = 0f;
        supplyMax = supply = baseSupply * Mathf.Lerp(0.65f, 1.15f, ctx.Scores[0]);
        pos = new Vector2(-PaperW * 0.5f + SegW * 0.5f, paperCenter.y + 1.2f);

        // papierik
        Draw.Rect(transform, "PaperShadow", paperCenter + new Vector2(0.08f, -0.1f), new Vector2(PaperW + 0.1f, PaperH + 0.1f), new Color(0, 0, 0, 0.3f), 1);
        Draw.Rect(transform, "Paper", paperCenter, new Vector2(PaperW, PaperH), new Color(0.96f, 0.94f, 0.87f), 2);
        for (int i = 0; i <= Segments; i++)
            Draw.Rect(transform, "Div", paperCenter + new Vector2(-PaperW * 0.5f + i * SegW, 0), new Vector2(0.03f, PaperH), new Color(0, 0, 0, i % Segments == 0 ? 0.25f : 0.1f), 3);

        for (int i = 0; i < Segments; i++)
        {
            float cx = SegCenterX(i);
            heaps[i] = Draw.Rect(transform, "Heap" + i, new Vector2(cx, paperCenter.y), new Vector2(SegW * 0.86f, 0.01f), new Color(0.35f, 0.62f, 0.22f), 4);
            bars[i] = Draw.Rect(transform, "Bar" + i, new Vector2(cx, BarBaseY), new Vector2(SegW * 0.6f, 0.01f), Color.green, 4);
        }
        // indikátor: podklad + cieľová čiara
        Draw.Rect(transform, "BarBg", new Vector2(0, BarBaseY + BarMaxH * 0.5f), new Vector2(PaperW + 0.3f, BarMaxH + 0.2f), new Color(0, 0, 0, 0.35f), 3);
        Draw.Rect(transform, "TargetLine", new Vector2(0, BarBaseY + BarMaxH * target), new Vector2(PaperW + 0.3f, 0.05f), new Color(1, 1, 1, 0.9f), 6);

        // sypací prúd a kurzor ("štipka")
        stream = Draw.Rect(transform, "Stream", pos, new Vector2(0.14f, 1f), new Color(0.4f, 0.7f, 0.25f), 7).transform;
        stream.gameObject.SetActive(false);
        cursor = new GameObject("Cursor").transform;
        cursor.SetParent(transform, false);
        Draw.Circle(cursor, "Hand", Vector2.zero, 0.7f, new Color(0.9f, 0.7f, 0.55f), 8);
        Draw.Circle(cursor, "Pinch", new Vector2(0, -0.12f), 0.32f, new Color(0.35f, 0.62f, 0.22f), 9);

        meter = ctx.Hud.AddMeter("Zásoba", 1f);
        ctx.Hud.SetSub("Cieľ: všetky stĺpce na bielej čiare");
    }

    float SegCenterX(int i) => -PaperW * 0.5f + SegW * (i + 0.5f);

    public override void Tick(float dt)
    {
        pos += input.Move * cursorSpeed * dt;
        pos.x = Mathf.Clamp(pos.x, -7.8f, 7.8f);
        pos.y = Mathf.Clamp(pos.y, -3.2f, 2.0f);
        cursor.localPosition = pos;

        bool pouring = Held && supply > 0f;
        stream.gameObject.SetActive(pouring);
        if (pouring)
        {
            float amt = Mathf.Min(pourRate * dt, supply);
            supply -= amt;

            bool onPaper = Mathf.Abs(pos.x) <= PaperW * 0.5f && Mathf.Abs(pos.y - paperCenter.y) <= PaperH * 0.5f + 0.5f;
            if (onPaper)
            {
                int seg = Mathf.Clamp(Mathf.FloorToInt((pos.x + PaperW * 0.5f) / SegW), 0, Segments - 1);
                float add = Mathf.Min(amt, Mathf.Max(0f, maxFill - fill[seg]));
                fill[seg] += add;
                lost += amt - add;
            }
            else lost += amt;

            float top = pos.y - 0.3f, bottom = paperCenter.y;
            stream.localPosition = new Vector2(pos.x, (top + bottom) * 0.5f);
            stream.localScale = new Vector3(0.14f, Mathf.Max(0.1f, Mathf.Abs(top - bottom)), 1f);
            stream.GetComponent<SpriteRenderer>().color = onPaper ? new Color(0.4f, 0.7f, 0.25f) : new Color(0.8f, 0.3f, 0.25f);

            sfxTimer -= dt;
            if (sfxTimer <= 0f) { Sfx.Play("pour", 0.35f); sfxTimer = 0.1f; }
        }

        for (int i = 0; i < Segments; i++)
        {
            float f = fill[i];
            float hh = Mathf.Max(0.01f, f * PaperH * 0.85f);
            heaps[i].transform.localScale = new Vector3(SegW * 0.86f, hh, 1f);
            heaps[i].transform.localPosition = new Vector2(SegCenterX(i), paperCenter.y - PaperH * 0.5f + hh * 0.5f + 0.05f);

            float bh = Mathf.Max(0.01f, f / 1f * BarMaxH);
            bars[i].transform.localScale = new Vector3(SegW * 0.6f, bh, 1f);
            bars[i].transform.localPosition = new Vector2(SegCenterX(i), BarBaseY + bh * 0.5f);
            float err = Mathf.Abs(f - target);
            bars[i].color = err < 0.12f ? new Color(0.35f, 0.9f, 0.4f) : err < 0.25f ? new Color(0.95f, 0.75f, 0.25f) : new Color(0.9f, 0.3f, 0.25f);
        }

        meter.Value = supply / supplyMax;

        if (supply <= 0.001f && !IsDone) ForceFinish();
    }

    public override void ForceFinish()
    {
        base.ForceFinish();
        float sq = 0f;
        for (int i = 0; i < Segments; i++) sq += (fill[i] - target) * (fill[i] - target);
        float rms = Mathf.Sqrt(sq / Segments);
        float lossFrac = lost / supplyMax;
        Score = Mathf.Clamp01(1f - rms * 2.5f - lossFrac * 0.6f);
    }
}
