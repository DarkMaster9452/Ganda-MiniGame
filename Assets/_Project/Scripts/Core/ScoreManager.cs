using UnityEngine;

public static class ScoreManager
{
    const string HighKey = "Rolka.HighScore";
    public const float BonusPerSecond = 0.01f;
    public const float MaxBonusSeconds = 10f;

    public static float Bonus(float savedSeconds) => Mathf.Min(savedSeconds, MaxBonusSeconds) * BonusPerSecond;

    /// <summary>Vážený súčet fáz + bonus za ušetrený čas, 0..1.</summary>
    public static float Final(float[] scores, float[] weights, float savedSeconds)
    {
        float total = 0f;
        for (int i = 0; i < scores.Length; i++) total += scores[i] * weights[i];
        return Mathf.Clamp01(total + Bonus(savedSeconds));
    }

    public static int Stars(float final) => final >= 0.9f ? 3 : final >= 0.75f ? 2 : final >= 0.5f ? 1 : 0;

    public static float HighScore => PlayerPrefs.GetFloat(HighKey, 0f);

    /// <summary>Uloží high score, ak je nové. Vráti true, ak bolo prekonané.</summary>
    public static bool TrySaveHigh(float final)
    {
        if (final <= HighScore) return false;
        PlayerPrefs.SetFloat(HighKey, final);
        PlayerPrefs.Save();
        return true;
    }
}
