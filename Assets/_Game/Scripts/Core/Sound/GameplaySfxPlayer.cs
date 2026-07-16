using System.Collections.Generic;
using UnityEngine;

public enum GameplaySfxCue
{
    FadeIn,
    FadeOut,
    Tap,
    Arc,
    HpClear,
    TrackClear,
    TrackFail,
    TrackAllPerfect,
    Unlock
}

/// <summary>Shared, persistent player for the small set of gameplay and store feedback sounds.</summary>
public class GameplaySfxPlayer : MonoBehaviour
{
    private const string CatalogPath = "GameplaySfxCatalog";
    private static GameplaySfxPlayer instance;
    private static GameplaySfxCatalog catalog;

    private readonly List<AudioSource> sources = new List<AudioSource>();

    public static GameplaySfxCatalog Catalog
    {
        get
        {
            if (catalog == null)
                catalog = Resources.Load<GameplaySfxCatalog>(CatalogPath);
            return catalog;
        }
    }

    public static float Play(GameplaySfxCue cue)
    {
        AudioClip clip = GetClip(cue);
        if (clip == null)
            return 0f;

        GameplaySfxPlayer player = EnsureInstance();
        AudioSource source = player.GetAvailableSource();
        source.clip = clip;
        source.volume = IsTapSound(cue) ? RuntimeGameplaySettings.TapSoundVolume01 : 1f;
        source.Play();
        return clip.length;
    }

    public static void PlayTapSound()
    {
        switch (RuntimeGameplaySettings.TapSoundEffect)
        {
            case RuntimeGameplaySettings.TapSoundEffectOption.Tap:
                Play(GameplaySfxCue.Tap);
                break;
            case RuntimeGameplaySettings.TapSoundEffectOption.Arc:
                Play(GameplaySfxCue.Arc);
                break;
        }
    }

    public static void PreviewTapSound()
    {
        RuntimeGameplaySettings.TapSoundEffectOption effect = RuntimeGameplaySettings.TapSoundEffect;
        if (effect == RuntimeGameplaySettings.TapSoundEffectOption.Mute)
            return;

        Play(effect == RuntimeGameplaySettings.TapSoundEffectOption.Arc ? GameplaySfxCue.Arc : GameplaySfxCue.Tap);
    }

    public static AudioClip GetClip(GameplaySfxCue cue)
    {
        GameplaySfxCatalog sourceCatalog = Catalog;
        if (sourceCatalog == null)
            return null;

        switch (cue)
        {
            case GameplaySfxCue.FadeIn: return sourceCatalog.fadeIn;
            case GameplaySfxCue.FadeOut: return sourceCatalog.fadeOut;
            case GameplaySfxCue.Tap: return sourceCatalog.tap;
            case GameplaySfxCue.Arc: return sourceCatalog.arc;
            case GameplaySfxCue.HpClear: return sourceCatalog.hpClear;
            case GameplaySfxCue.TrackClear: return sourceCatalog.trackClear;
            case GameplaySfxCue.TrackFail: return sourceCatalog.trackFail;
            case GameplaySfxCue.TrackAllPerfect: return sourceCatalog.trackAllPerfect;
            case GameplaySfxCue.Unlock: return sourceCatalog.unlock;
            default: return null;
        }
    }

    private static bool IsTapSound(GameplaySfxCue cue)
    {
        return cue == GameplaySfxCue.Tap || cue == GameplaySfxCue.Arc;
    }

    private static GameplaySfxPlayer EnsureInstance()
    {
        if (instance != null)
            return instance;

        GameObject root = new GameObject("RG Gameplay SFX");
        instance = root.AddComponent<GameplaySfxPlayer>();
        DontDestroyOnLoad(root);
        return instance;
    }

    private AudioSource GetAvailableSource()
    {
        for (int i = 0; i < sources.Count; i++)
        {
            if (!sources[i].isPlaying)
                return sources[i];
        }

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.ignoreListenerPause = true;
        sources.Add(source);
        return source;
    }
}
