using System.Collections.Generic;
using UnityEngine;

/// <summary>Fáza 4 – dym: drž Space = nádych (meter pľúc), pusti v zóne = dymový krúžok, preplnenie = kašeľ.</summary>
public class SmokePhase : PhaseBase
{
    class Puff
    {
        public Transform t;
        public SpriteRenderer sr;
        public Vector2 vel;
        public float life, maxLife, growth, alpha;
    }

    [SerializeField] float fillRate = 0.45f;
    [SerializeField] float zoneMin = 0.7f;
    [SerializeField] float zoneMaxBest = 0.95f;
    [SerializeField] float zoneMaxWorst = 0.80f;   // pri roztrhnutej rolke
    [SerializeField] float stunTime = 1f;
    [SerializeField] int maxPuffs = 140;

    static readonly Vector2 Mouth = new Vector2(-2.7f, -0.9f);

    readonly List<Puff> puffs = new List<Puff>();
    MeterUI meter;
    Transform roll, ember, glow, arrow;
    SpriteRenderer glowSr;
    Vector2 zone;
    float lungs, aim, size, stun, wispTimer;
    bool prevHeld, needRelease;
    int rings, coughs;

    void Awake()
    {
        duration = 10f;
        Title = "4. Dym";
        Hint = "drž Space = nádych, pusti v zelenej zóne  |  A/D smer, W/S veľkosť";
    }

    public override void Enter(InputReader input, GameContext ctx)
    {
        base.Enter(input, ctx);
        puffs.Clear();
        lungs = 0f; aim = 0f; size = 1f; stun = 0f; wispTimer = 0f;
        prevHeld = false; needRelease = false; rings = 0; coughs = 0;

        float q = ctx.Scores[2];
        zone = new Vector2(zoneMin, Mathf.Lerp(zoneMaxWorst, zoneMaxBest, q));

        // hlava zboku
        Draw.Circle(transform, "Head", new Vector2(-5.3f, -0.2f), 5.6f, new Color(0.09f, 0.08f, 0.11f), 1);
        Draw.Circle(transform, "Nose", new Vector2(-2.55f, 0.35f), 0.9f, new Color(0.09f, 0.08f, 0.11f), 1);
        Draw.Rect(transform, "Lips", new Vector2(-2.75f, Mouth.y), new Vector2(0.6f, 0.22f), new Color(0.35f, 0.15f, 0.18f), 2);
        Draw.Circle(transform, "Eye", new Vector2(-3.4f, 0.75f), 0.18f, new Color(0.9f, 0.9f, 0.9f), 2);

        // rolka: ľavý koniec (filter) pri ústach, sklonená mierne hore
        const float scale = 0.72f;
        const float rot = 9f;
        float half = 3.2f * scale;
        var center = Mouth + new Vector2(Mathf.Cos(rot * Mathf.Deg2Rad), Mathf.Sin(rot * Mathf.Deg2Rad)) * (half - 0.05f);
        roll = RollVisual.Build(transform, center, scale, q, 4, true, rot);
        ember = roll.Find("Ember");
        glow = roll.Find("Glow");
        glowSr = glow.GetComponent<SpriteRenderer>();

        // šípka smeru (výdych)
        arrow = Draw.Rect(transform, "Arrow", Mouth, new Vector2(1.2f, 0.08f), new Color(1f, 1f, 1f, 0.35f), 3).transform;

        meter = ctx.Hud.AddMeter("Pľúca", 1f, zone);
        ctx.Hud.SetSub("Krúžky: 0");
        Sfx.Play("lighter", 0.5f);
    }

    public override void Tick(float dt)
    {
        // žeravý koniec
        float flick = 1f + 0.12f * Mathf.Sin(Time.time * 17f) + 0.08f * Mathf.Sin(Time.time * 29f);
        glow.localScale = Vector3.one * (1.8f * flick);
        glowSr.color = new Color(1f, 0.5f, 0.1f, 0.25f + 0.15f * lungs + 0.05f * flick);

        // stun po kašli
        if (stun > 0f)
        {
            stun -= dt;
            if (stun <= 0f) ctx.Hud.SetBig("");
        }
        bool canAct = stun <= 0f;

        // smer a veľkosť krúžku
        if (canAct)
        {
            aim = Mathf.Clamp(aim + input.Move.x * 1.5f * dt, -1f, 1f);
            size = Mathf.Clamp(size + input.Move.y * 1.2f * dt, 0.5f, 1.5f);
        }
        float ang = aim * 30f;
        arrow.localRotation = Quaternion.Euler(0, 0, ang);
        arrow.localScale = new Vector3(0.6f + size, 0.08f + 0.1f * size, 1f);
        var dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
        arrow.localPosition = Mouth + new Vector2(0.4f, 0.1f) + dir * (0.3f + size * 0.5f);

        // nádych / výdych
        bool held = Held && canAct && !needRelease;
        if (needRelease && !input.ActionHeld) needRelease = false;

        if (held)
        {
            lungs += fillRate * dt;
            if (lungs > 1f)
            {
                Cough();
                held = false;
            }
        }
        else if (prevHeld)
        {
            Exhale();
        }
        prevHeld = held;
        if (!held && lungs > 0f) lungs = Mathf.Max(0f, lungs - 2.5f * dt); // meter po výdychu klesne
        meter.Value = lungs;

        // tenký dym z rolky
        wispTimer -= dt;
        if (wispTimer <= 0f)
        {
            wispTimer = 0.14f;
            SpawnPuff(ember.position, new Vector2(Random.Range(-0.15f, 0.25f), Random.Range(0.6f, 1f)), Random.Range(0.25f, 0.4f), 0.55f, 0.22f, 1.6f, false);
        }

        UpdatePuffs(dt);
    }

    void Exhale()
    {
        Vector2 start = Mouth + new Vector2(0.3f, 0.25f);
        var d = new Vector2(Mathf.Cos(aim * 30f * Mathf.Deg2Rad), Mathf.Sin(aim * 30f * Mathf.Deg2Rad));

        if (lungs >= zone.x && lungs <= zone.y)
        {
            rings++;
            ctx.Hud.SetSub("Krúžky: " + rings);
            ctx.Hud.Toast("Krúžok!", 0.6f);
            Sfx.Play("exhale", 0.5f);
            Sfx.Play("ding", 0.4f);
            SpawnPuff(start, d * 3.2f, 0.7f * size, 0.9f, 0.55f * size, 3.2f, true);
        }
        else if (lungs > 0.15f)
        {
            Sfx.Play("exhale", 0.45f);
            for (int i = 0; i < 9; i++)
                SpawnPuff(start, d * Random.Range(1.5f, 3.2f) + Random.insideUnitCircle * 0.8f, Random.Range(0.3f, 0.6f), 0.5f, Random.Range(0.3f, 0.6f), 2.2f, false);
        }
        lungs = Mathf.Min(lungs, 0.99f);
    }

    void Cough()
    {
        coughs++;
        stun = stunTime;
        needRelease = true;
        lungs = 0f;
        ctx.Shake(0.35f);
        Sfx.Play("cough", 0.7f);
        ctx.Hud.SetBig("KAŠEĽ!", new Color(1f, 0.4f, 0.35f), 120);
        Vector2 start = Mouth + new Vector2(0.3f, 0.25f);
        for (int i = 0; i < 16; i++)
            SpawnPuff(start, new Vector2(Random.Range(1f, 4.5f), Random.Range(-1.5f, 2f)), Random.Range(0.4f, 0.9f), 0.6f, Random.Range(0.4f, 0.8f), 2.0f, false);
    }

    void SpawnPuff(Vector2 pos, Vector2 vel, float diameter, float alpha, float growth, float maxLife, bool ring)
    {
        if (puffs.Count >= maxPuffs) return;
        var col = ring ? new Color(0.92f, 0.94f, 1f, alpha) : new Color(0.75f, 0.77f, 0.82f, alpha);
        var sr = ring
            ? Draw.Ring(transform, "SmokeRing", pos, diameter, col, 10)
            : Draw.Circle(transform, "Smoke", pos, diameter, col, 9);
        sr.transform.position = new Vector3(pos.x, pos.y, 0f);
        puffs.Add(new Puff { t = sr.transform, sr = sr, vel = vel, life = 0f, maxLife = maxLife, growth = growth, alpha = alpha });
    }

    void UpdatePuffs(float dt)
    {
        for (int i = puffs.Count - 1; i >= 0; i--)
        {
            var p = puffs[i];
            p.life += dt;
            if (p.life >= p.maxLife)
            {
                Destroy(p.t.gameObject);
                puffs.RemoveAt(i);
                continue;
            }
            float k = p.life / p.maxLife;
            p.t.position += (Vector3)(p.vel * dt);
            p.vel *= 1f - 0.4f * dt;
            float s = p.t.localScale.x + p.growth * dt;
            p.t.localScale = new Vector3(s, s, 1f);
            var c = p.sr.color;
            c.a = p.alpha * (1f - k);
            p.sr.color = c;
        }
    }

    public override void ForceFinish()
    {
        base.ForceFinish();
        Score = Mathf.Clamp01(Mathf.Min(1f, rings * 0.2f) - coughs * 0.25f);
        ctx.Hud.SetBig("");
    }
}
