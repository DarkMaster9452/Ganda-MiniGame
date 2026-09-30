using UnityEngine;

/// <summary>Fáza 1 – drvenie: striedavo A/D točí drvič, cieľ je jemnosť v zelenej zóne (Space = hotovo).</summary>
public class GrindPhase : PhaseBase
{
    [SerializeField] float perSwap = 0.03f;
    [SerializeField] Vector2 greenZone = new Vector2(0.6f, 0.85f);

    const int HerbCount = 30;
    static readonly Color[] StageColors =
    {
        new Color(0.30f, 0.62f, 0.22f),  // hrubé
        new Color(0.42f, 0.70f, 0.28f),  // stredné
        new Color(0.55f, 0.72f, 0.33f),  // jemné
        new Color(0.55f, 0.52f, 0.42f),  // prach
    };

    MeterUI meter;
    Transform top;
    SpriteRenderer[] herb;
    float level, angle, targetAngle;
    int lastDir, stage;

    void Awake()
    {
        duration = 15f;
        Title = "1. Drvenie";
        Hint = "Striedavo  A  a  D     |     Space = hotovo";
    }

    public override void Enter(InputReader input, GameContext ctx)
    {
        base.Enter(input, ctx);
        level = 0f; lastDir = 0; angle = targetAngle = 0f; stage = 0;

        Draw.Circle(transform, "Base", Vector2.zero, 6.4f, new Color(0.16f, 0.16f, 0.19f), 1);
        Draw.Circle(transform, "Inner", Vector2.zero, 5.6f, new Color(0.26f, 0.26f, 0.30f), 2);

        herb = new SpriteRenderer[HerbCount];
        var rng = new System.Random(7);
        for (int i = 0; i < HerbCount; i++)
        {
            float a = (float)rng.NextDouble() * Mathf.PI * 2f;
            float r = Mathf.Sqrt((float)rng.NextDouble()) * 2.0f;
            herb[i] = Draw.Circle(transform, "Herb", new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r, 0.7f, StageColors[0], 3);
        }

        // otočná vrchná časť (okraj + ryhy + tenký kríž)
        top = new GameObject("Top").transform;
        top.SetParent(transform, false);
        Draw.Ring(top, "Rim", Vector2.zero, 5.2f, new Color(0.6f, 0.6f, 0.66f), 5);
        for (int i = 0; i < 12; i++)
        {
            float a = i * 30f;
            var dir = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad));
            Draw.Rect(top, "Notch", dir * 2.75f, new Vector2(0.35f, 0.16f), new Color(0.75f, 0.75f, 0.8f), 6, a);
        }
        Draw.Rect(top, "BarH", Vector2.zero, new Vector2(4.6f, 0.14f), new Color(0.6f, 0.6f, 0.66f, 0.55f), 4);
        Draw.Rect(top, "BarV", Vector2.zero, new Vector2(0.14f, 4.6f), new Color(0.6f, 0.6f, 0.66f, 0.55f), 4);
        Draw.Circle(top, "Knob", Vector2.zero, 0.6f, new Color(0.8f, 0.8f, 0.85f), 7);

        meter = ctx.Hud.AddMeter("Jemnosť", 1.2f, greenZone);

        input.OnHorizontal += Swap;   // -1 = A, +1 = D
        input.OnAction += Finish;
    }

    void Swap(int dir)
    {
        if (IsDone || dir == 0 || dir == lastDir) return;
        lastDir = dir;
        level = Mathf.Min(1.2f, level + perSwap);
        targetAngle -= 30f * dir;
        ctx.Shake(0.03f);
        Sfx.Play("scrape", 0.5f);
    }

    public override void Tick(float dt)
    {
        angle = Mathf.MoveTowards(angle, targetAngle, 500f * dt);
        top.localRotation = Quaternion.Euler(0, 0, angle);
        meter.Value = level;

        int st = level < 0.25f ? 0 : level < 0.6f ? 1 : level < 0.85f ? 2 : 3;
        if (st != stage)
        {
            stage = st;
            if (st == 3) ctx.Hud.Toast("Prach! Priveľa", 0.8f);
            else Sfx.Play("click", 0.5f);
        }

        float d = Mathf.Lerp(0.7f, 0.12f, Mathf.Clamp01(level / 1.0f));
        foreach (var h in herb)
        {
            h.color = StageColors[stage];
            h.transform.localScale = new Vector3(d, d, 1f);
        }
    }

    void Finish()
    {
        if (level > 0.2f) ForceFinish();
    }

    public override void ForceFinish()
    {
        base.ForceFinish();
        Score = level < greenZone.x ? level / greenZone.x
              : level > greenZone.y ? Mathf.Clamp01(1f - (level - greenZone.y) * 2.5f)
              : 1f;
    }
}
