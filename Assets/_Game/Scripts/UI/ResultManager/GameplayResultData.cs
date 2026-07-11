using UnityEngine;

public readonly struct GameplayResultData
{
    public readonly int perfect;
    public readonly int great;
    public readonly int good;
    public readonly int miss;
    public readonly int maxCombo;

    public GameplayResultData(int perfect, int great, int good, int miss, int maxCombo)
    {
        this.perfect = Mathf.Max(0, perfect);
        this.great = Mathf.Max(0, great);
        this.good = Mathf.Max(0, good);
        this.miss = Mathf.Max(0, miss);
        this.maxCombo = Mathf.Max(0, maxCombo);
    }
}
