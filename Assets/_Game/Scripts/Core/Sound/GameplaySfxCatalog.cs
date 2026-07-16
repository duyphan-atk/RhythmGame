using UnityEngine;

[CreateAssetMenu(menuName = "RhythmGame/Gameplay SFX Catalog", fileName = "GameplaySfxCatalog")]
public class GameplaySfxCatalog : ScriptableObject
{
    [Header("Scene transitions")]
    public AudioClip fadeIn;
    public AudioClip fadeOut;

    [Header("Gameplay")]
    public AudioClip tap;
    public AudioClip arc;
    public AudioClip hpClear;

    [Header("Chart result")]
    public AudioClip trackClear;
    public AudioClip trackFail;
    public AudioClip trackAllPerfect;
    public Sprite titleAllPerfect;
    public Sprite titleFullCombo;
    public Sprite titleTrackComplete;
    public Sprite titleTrackLost;

    [Header("Store")]
    public AudioClip unlock;
}
