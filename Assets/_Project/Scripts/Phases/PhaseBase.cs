using UnityEngine;

public abstract class PhaseBase : MonoBehaviour
{
    [SerializeField] protected float duration = 15f;
    public float Duration => duration;
    public string Title { get; protected set; } = "";
    public string Hint { get; protected set; } = "";
    public bool IsDone { get; protected set; }
    public float Score { get; protected set; } // 0..1

    protected InputReader input;
    protected GameContext ctx;

    bool armed;

    /// <summary>
    /// Je Space držaný? Ignoruje držanie, ktoré začalo ešte v predošlej fáze
    /// (treba ho najprv raz uvoľniť), aby fáza nezačala "sama".
    /// </summary>
    protected bool Held
    {
        get
        {
            if (!input.ActionHeld) armed = true;
            return armed && input.ActionHeld;
        }
    }

    public virtual void Enter(InputReader input, GameContext ctx)
    {
        this.input = input;
        this.ctx = ctx;
        IsDone = false;
        Score = 0f;
        armed = false;
        Draw.Clear(transform);
        gameObject.SetActive(true);
    }

    public abstract void Tick(float dt);

    public virtual void Exit()
    {
        ctx.Hud.ClearMeters();
        gameObject.SetActive(false);
    }

    /// <summary>Ukončí fázu s aktuálnym stavom (čas vypršal alebo hráč dokončil). Potomok tu vypočíta Score.</summary>
    public virtual void ForceFinish() { IsDone = true; }
}
