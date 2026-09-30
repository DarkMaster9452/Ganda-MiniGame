/// <summary>Jednoduchý odpočet jednej fázy.</summary>
public class GameTimer
{
    public float Duration { get; private set; }
    public float Remaining { get; private set; }
    public bool Expired => Remaining <= 0f;

    public void Start(float duration)
    {
        Duration = duration;
        Remaining = duration;
    }

    public void Tick(float dt) => Remaining -= dt;
}
