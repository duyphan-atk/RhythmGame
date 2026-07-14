using UnityEngine;

[System.Serializable]
public class JudgmentWindow
{
    [Header("Judgment Window In Seconds")]
    public float perfectWindow = 0.042f; // 42ms, close to Project Sekai tap PERFECT.
    public float greatWindow = 0.083f;   // 83ms.
    public float goodWindow = 0.108f;    // 108ms.
    public float missWindow = 0.125f;    // 125ms. Inside this but outside Good consumes as Miss.

    public HitJudgment Judge(float inputTime, float hitTime, out float deltaMs)
    {
        float delta = inputTime - hitTime;
        float absDelta = Mathf.Abs(delta);

        deltaMs = delta * 1000f;

        if (absDelta <= perfectWindow)
            return HitJudgment.Perfect;

        if (absDelta <= greatWindow)
            return HitJudgment.Great;

        if (absDelta <= goodWindow)
            return HitJudgment.Good;

        return HitJudgment.Miss;
    }

    public bool IsInsideHitWindow(float inputTime, float hitTime)
    {
        float delta = Mathf.Abs(inputTime - hitTime);
        return delta <= missWindow;
    }

    public bool IsTooEarlyButInsideMissWindow(float inputTime, float hitTime)
    {
        float delta = hitTime - inputTime;
        return delta > goodWindow && delta <= missWindow;
    }

    public void Normalize()
    {
        perfectWindow = Mathf.Max(0f, perfectWindow);
        greatWindow = Mathf.Max(perfectWindow, greatWindow);
        goodWindow = Mathf.Max(greatWindow, goodWindow);
        missWindow = Mathf.Max(goodWindow, missWindow);
    }
}
