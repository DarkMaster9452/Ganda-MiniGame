using System;

/// <summary>Spoločné dáta medzi fázami (skóre predošlých fáz, HUD, shake kamery).</summary>
public class GameContext
{
    public HUD Hud;
    public float[] Scores = new float[4];
    public Action<float> Shake = _ => { };

    public void Reset() => Array.Clear(Scores, 0, Scores.Length);
}
