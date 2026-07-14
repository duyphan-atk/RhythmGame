using UnityEngine;

public static class RuntimeGameplaySettings
{
    public const string NoteSpeedKey = "NoteSpeed";
    public const string NoteVolumeKey = "NoteVolume";
    public const string MusicVolumeKey = NoteVolumeKey;
    public const string AudioOffsetKey = "AudioOffset";
    public const string AudioPresetKey = "AudioPreset";
    public const string FrameRateKey = "VisualQuality";

    public const float DefaultNoteSpeedMultiplier = 1f;
    public const int DefaultNoteVolumePercent = 100;
    public const int DefaultAudioOffsetMs = 0;
    public const int DefaultFrameRateIndex = 0;
    public const int UnlimitedFrameRate = -1;
    public const float MinNoteSpeedSetting = 1f;
    public const float MaxNoteSpeedSetting = 6.5f;
    public const float MinScrollSpeed = 50f;
    public const float MaxScrollSpeed = 3500f;
    public static readonly int[] FrameRateOptions = { 60, 120, UnlimitedFrameRate };
    public static readonly string[] FrameRateLabels = { "60 FPS", "120 FPS", "Unlimited FPS" };

    public static float NoteSpeedMultiplier
    {
        get => Mathf.Clamp(PlayerPrefs.GetFloat(NoteSpeedKey, DefaultNoteSpeedMultiplier), MinNoteSpeedSetting, MaxNoteSpeedSetting);
        set => PlayerPrefs.SetFloat(NoteSpeedKey, Mathf.Clamp(value, MinNoteSpeedSetting, MaxNoteSpeedSetting));
    }

    public static float ScrollSpeed
    {
        get => Mathf.Lerp(
            MinScrollSpeed,
            MaxScrollSpeed,
            Mathf.InverseLerp(MinNoteSpeedSetting, MaxNoteSpeedSetting, NoteSpeedMultiplier));
        set => NoteSpeedMultiplier = Mathf.Lerp(
            MinNoteSpeedSetting,
            MaxNoteSpeedSetting,
            Mathf.InverseLerp(MinScrollSpeed, MaxScrollSpeed, value));
    }

    public static int NoteVolumePercent
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(NoteVolumeKey, DefaultNoteVolumePercent), 0, 100);
        set => PlayerPrefs.SetInt(NoteVolumeKey, Mathf.Clamp(value, 0, 100));
    }

    public static float NoteVolume01 => NoteVolumePercent / 100f;
    public static int MusicVolumePercent { get => NoteVolumePercent; set => NoteVolumePercent = value; }
    public static float MusicVolume01 => NoteVolume01;

    public static int AudioOffsetMs
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(AudioOffsetKey, DefaultAudioOffsetMs), -500, 1000);
        set => PlayerPrefs.SetInt(AudioOffsetKey, Mathf.Clamp(value, -500, 1000));
    }

    public static float AudioOffsetSeconds => AudioOffsetMs / 1000f;

    public static bool HeadphonesPreset
    {
        get => PlayerPrefs.GetInt(AudioPresetKey, 0) == 1;
        set => PlayerPrefs.SetInt(AudioPresetKey, value ? 1 : 0);
    }

    public static int FrameRateIndex
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(FrameRateKey, DefaultFrameRateIndex), 0, FrameRateOptions.Length - 1);
        set => PlayerPrefs.SetInt(FrameRateKey, Mathf.Clamp(value, 0, FrameRateOptions.Length - 1));
    }

    public static string FrameRateLabel => FrameRateLabels[FrameRateIndex];

    public static void ApplyFrameRate()
    {
        ApplyFrameRate(FrameRateIndex);
    }

    public static void ApplyFrameRate(int frameRateIndex)
    {
        frameRateIndex = Mathf.Clamp(frameRateIndex, 0, FrameRateOptions.Length - 1);
        int targetFrameRate = FrameRateOptions[frameRateIndex];

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}
