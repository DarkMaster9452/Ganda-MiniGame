using System.Collections;
using UnityEngine;

/// <summary>
/// Riadi celú hru: odpočet 3-2-1 -> 4 fázy -> výsledky -> Space = znova.
/// Všetko (kamera, pozadie, fázy, HUD) sa vytvorí v kóde, takže stačí spustiť ľubovoľnú scénu.
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] float[] weights = { 0.2f, 0.25f, 0.35f, 0.2f };

    InputReader input;
    HUD hud;
    ResultsScreen results;
    PhaseBase[] phases;
    GameContext ctx;
    GameTimer timer;
    Camera cam;
    Vector3 camHome;
    float shake;

    void Awake()
    {
        Application.targetFrameRate = 60;
        Cursor.visible = false;

        SetupCamera();
        BuildBackground();

        input = gameObject.AddComponent<InputReader>();
        hud = gameObject.AddComponent<HUD>();
        results = gameObject.AddComponent<ResultsScreen>();
        timer = new GameTimer();
        ctx = new GameContext { Hud = hud, Shake = Shake };

        phases = new PhaseBase[]
        {
            CreatePhase<GrindPhase>("Phase1_Grind"),
            CreatePhase<ArrangePhase>("Phase2_Arrange"),
            CreatePhase<RollPhase>("Phase3_Roll"),
            CreatePhase<SmokePhase>("Phase4_Smoke"),
        };
    }

    T CreatePhase<T>(string goName) where T : PhaseBase
    {
        var go = new GameObject(goName);
        go.transform.SetParent(transform, false);
        var p = go.AddComponent<T>();
        go.SetActive(false);
        return p;
    }

    void SetupCamera()
    {
        cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = go.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.2f, 0.13f, 0.1f);
        cam.transform.position = new Vector3(0, 0, -10f);
        cam.transform.rotation = Quaternion.identity;
        camHome = cam.transform.position;
    }

    void BuildBackground()
    {
        var bg = new GameObject("Table").transform;
        bg.SetParent(transform, false);
        Draw.Rect(bg, "Wood", Vector2.zero, new Vector2(40f, 24f), new Color(0.24f, 0.16f, 0.11f), -100);
        for (int i = -6; i <= 6; i++)
            Draw.Rect(bg, "Plank", new Vector2(i * 1.6f, 0), new Vector2(0.05f, 24f), new Color(0, 0, 0, 0.18f), -99);
    }

    public void Shake(float amount) => shake = Mathf.Max(shake, amount);

    void LateUpdate()
    {
        if (shake > 0.001f)
        {
            cam.transform.position = camHome + (Vector3)(Random.insideUnitCircle * shake);
            shake = Mathf.Lerp(shake, 0f, Time.deltaTime * 9f);
        }
        else cam.transform.position = camHome;
    }

    IEnumerator Start()
    {
        Sfx.StartMusic();
        while (true) yield return RunGame();
    }

    IEnumerator RunGame()
    {
        results.Hide();
        hud.ResetAll();
        ctx.Reset();

        // intro 3-2-1
        for (int n = 3; n >= 1; n--)
        {
            hud.SetBig(n.ToString(), Color.white, 200);
            Sfx.Play("tick", 0.7f);
            yield return new WaitForSeconds(1f);
        }
        hud.SetBig("");

        float savedSeconds = 0f;
        for (int i = 0; i < phases.Length; i++)
        {
            var p = phases[i];
            input.ClearListeners();
            hud.ResetAll();
            hud.SetPhase(p.Title, p.Hint);
            timer.Start(p.Duration);
            p.Enter(input, ctx);

            while (!p.IsDone && !timer.Expired)
            {
                timer.Tick(Time.deltaTime);
                hud.SetTimer(timer.Remaining);
                p.Tick(Time.deltaTime);
                yield return null;
            }
            if (!p.IsDone) p.ForceFinish();

            savedSeconds += Mathf.Max(0f, timer.Remaining);
            ctx.Scores[i] = p.Score;
            input.ClearListeners();
            p.Exit();

            hud.SetBig("");
            hud.Toast(p.Title.Substring(3) + ": " + Mathf.RoundToInt(p.Score * 100f) + " %", 0.9f);
            Sfx.Play("ding", 0.5f);
            hud.HideTimer();
        }

        float final = ScoreManager.Final(ctx.Scores, weights, savedSeconds);
        int stars = ScoreManager.Stars(final);
        bool newHigh = ScoreManager.TrySaveHigh(final);
        hud.ResetAll();
        results.Show(final, stars, ScoreManager.HighScore, newHigh, ScoreManager.Bonus(savedSeconds), ctx.Scores);
        Sfx.Play(stars > 0 ? "win" : "buzz", 0.7f);

        // Space = znova (krátka pauza, aby ho nechytil dozvuk z poslednej fázy)
        yield return new WaitForSeconds(0.6f);
        bool restart = false;
        input.OnAction += () => restart = true;
        yield return new WaitUntil(() => restart);
        input.ClearListeners();
    }
}
