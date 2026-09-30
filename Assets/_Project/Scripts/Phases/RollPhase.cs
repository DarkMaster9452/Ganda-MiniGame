using UnityEngine;

/// <summary>Fáza 3 – balenie: A) W/S rytmus (napätie), B) A/D QTE tvarovanie, C) Space zlepenie.</summary>
public class RollPhase : PhaseBase
{
    enum Step { Tension, Shape, Seal, Done }

    [SerializeField] Vector2 tensionZone = new Vector2(0.4f, 0.8f);
    [SerializeField] Vector2 sealZone = new Vector2(0.42f, 0.58f);
    [SerializeField] int needCorrect = 10;
    [SerializeField] float stepALimit = 7f;
    [SerializeField] float qteWindow = 0.8f;
    [SerializeField] float sealSpeed = 1.3f;

    Step step;
    Transform paper;
    SpriteRenderer paperSr;
    MeterUI meter;
    Transform finalRoll;

    // A
    float tension, stepTime;
    int correct, tears, lastKey;
    // B
    int qteDone, qteHits, qteTarget;
    float qteTimer, qteGap, bigClearAt;
    bool qteActive;
    // C
    float sealPos, sealDir;
    // výsledky krokov
    float scoreA, scoreB, scoreC, doneTimer;

    void Awake()
    {
        duration = 15f;
        Title = "3. Balenie";
        Hint = "Krok A: striedavo  W  a  S  v rytme";
    }

    public override void Enter(InputReader input, GameContext ctx)
    {
        base.Enter(input, ctx);
        step = Step.Tension;
        tension = 0f; stepTime = 0f; correct = 0; tears = 0; lastKey = 0;
        qteDone = 0; qteHits = 0; qteActive = false; qteGap = 0.4f; bigClearAt = 0f;
        sealPos = 0f; sealDir = 1f;
        scoreA = scoreB = scoreC = 0f; doneTimer = 0f;
        finalRoll = null;

        paperSr = Draw.Rect(transform, "Paper", Vector2.zero, new Vector2(10f, 3.2f), new Color(0.97f, 0.95f, 0.88f), 3);
        paper = paperSr.transform;
        Draw.Rect(transform, "Shadow", new Vector2(0.1f, -0.15f), new Vector2(10.2f, 3.3f), new Color(0, 0, 0, 0.25f), 2);
        // prsty
        Draw.Circle(transform, "FingerL", new Vector2(-1.6f, -1.9f), 0.9f, new Color(0.9f, 0.7f, 0.55f), 6);
        Draw.Circle(transform, "FingerR", new Vector2(1.6f, -1.9f), 0.9f, new Color(0.9f, 0.7f, 0.55f), 6);

        meter = ctx.Hud.AddMeter("Napätie", 1f, tensionZone);
        ctx.Hud.SetSub("Krok A: striedavo W a S v rytme (drž zelenú zónu)");

        input.OnVertical += OnVertical;
        input.OnHorizontal += OnHorizontal;
        input.OnAction += OnSpace;
    }

    // ---- Krok A ----
    void OnVertical(int key)
    {
        if (step != Step.Tension) return;

        tension += key == lastKey ? 0.3f : 0.14f;   // dvakrát to isté = trhnutie
        lastKey = key;

        if (tension > 1f)
        {
            tears++;
            tension = 0.5f;
            ctx.Shake(0.15f);
            Sfx.Play("buzz", 0.5f);
        }
        else if (tension >= tensionZone.x && tension <= tensionZone.y)
        {
            correct++;
            Sfx.Play("tick", 0.6f);
            if (correct >= needCorrect) StartShape();
        }
    }

    void StartShape()
    {
        scoreA = ScoreA();
        step = Step.Shape;
        qteGap = 0.4f;
        ctx.Hud.ClearMeters();
        ctx.Hud.SetSub("Krok B: stlač šípku, ktorá sa ukáže (A alebo D)");
        Sfx.Play("ding", 0.5f);
    }

    float ScoreA() => Mathf.Clamp01(correct / (float)needCorrect) * Mathf.Max(0f, 1f - 0.15f * tears);

    // ---- Krok B ----
    void OnHorizontal(int dir)
    {
        if (step != Step.Shape || !qteActive) return;
        ResolveQte(dir == qteTarget);
    }

    void ResolveQte(bool hit)
    {
        qteActive = false;
        qteDone++;
        if (hit) qteHits++;
        ctx.Hud.SetBig(hit ? "OK" : "X", hit ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.4f, 0.35f), 110);
        Sfx.Play(hit ? "ding" : "buzz", 0.5f);
        bigClearAt = Time.time + 0.35f;
        qteGap = 0.4f;
        if (qteDone >= 3) StartSeal();
    }

    void StartSeal()
    {
        scoreB = qteHits / 3f;
        step = Step.Seal;
        sealPos = 0f; sealDir = 1f;
        meter = ctx.Hud.AddMeter("Zlepenie", 1f, sealZone);
        ctx.Hud.SetSub("Krok C: Space, keď je bežec v zelenom poli");
    }

    // ---- Krok C ----
    void OnSpace()
    {
        if (step != Step.Seal) return;
        float d = Mathf.Abs(sealPos - 0.5f);
        scoreC = d <= 0.08f ? 1f : Mathf.Clamp01(1f - (d - 0.08f) / 0.3f);
        Sfx.Play(scoreC > 0.7f ? "ding" : "click", 0.6f);
        step = Step.Done;
        doneTimer = 0.6f;
        ShowFinalRoll();
    }

    void ShowFinalRoll()
    {
        float q = (scoreA + scoreB + scoreC) / 3f;
        paper.gameObject.SetActive(false);
        finalRoll = RollVisual.Build(transform, new Vector2(0, 0.2f), 1.3f, q, 5, false);
    }

    public override void Tick(float dt)
    {
        if (bigClearAt > 0f && Time.time >= bigClearAt)
        {
            ctx.Hud.SetBig("");
            bigClearAt = 0f;
        }

        switch (step)
        {
            case Step.Tension:
                stepTime += dt;
                tension = Mathf.Max(0f, tension - 0.35f * dt);
                meter.Value = tension;
                float prog = correct / (float)needCorrect;
                // papier sa postupne zvinie (užšie a dlhšie) a chveje sa mimo zóny
                paper.localScale = new Vector3(Mathf.Lerp(10f, 6.5f, prog), Mathf.Lerp(3.2f, 0.9f, prog), 1f);
                bool inZone = tension >= tensionZone.x && tension <= tensionZone.y;
                paper.localRotation = Quaternion.Euler(0, 0, inZone ? 0f : Mathf.Sin(Time.time * 40f) * 2f);
                paperSr.color = tension > 0.9f ? new Color(1f, 0.7f, 0.65f) : new Color(0.97f, 0.95f, 0.88f);
                if (stepTime >= stepALimit) StartShape();
                break;

            case Step.Shape:
                if (!qteActive)
                {
                    qteGap -= dt;
                    if (qteGap <= 0f && qteDone < 3)
                    {
                        qteTarget = Random.value < 0.5f ? -1 : 1;
                        qteActive = true;
                        qteTimer = qteWindow;
                        ctx.Hud.SetBig(qteTarget < 0 ? "A" : "D", new Color(1f, 0.9f, 0.4f), 200);
                    }
                }
                else
                {
                    qteTimer -= dt;
                    if (qteTimer <= 0f) ResolveQte(false);
                }
                float shapeProg = qteDone / 3f;
                paper.localScale = new Vector3(6.5f, Mathf.Lerp(0.9f, 0.7f, shapeProg), 1f);
                break;

            case Step.Seal:
                sealPos += sealDir * sealSpeed * dt;
                if (sealPos >= 1f) { sealPos = 1f; sealDir = -1f; }
                if (sealPos <= 0f) { sealPos = 0f; sealDir = 1f; }
                meter.Value = sealPos;
                break;

            case Step.Done:
                doneTimer -= dt;
                if (finalRoll != null) finalRoll.localScale = Vector3.one * (1.3f + 0.08f * Mathf.Sin(doneTimer * 20f));
                if (doneTimer <= 0f) ForceFinish();
                break;
        }
    }

    public override void ForceFinish()
    {
        // čas vypršal počas kroku -> ohodnoť, čo je hotové
        if (step == Step.Tension) scoreA = ScoreA();
        else if (step == Step.Shape) scoreB = qteHits / 3f;
        base.ForceFinish();
        Score = (scoreA + scoreB + scoreC) / 3f;
        ctx.Hud.SetBig("");
    }
}
