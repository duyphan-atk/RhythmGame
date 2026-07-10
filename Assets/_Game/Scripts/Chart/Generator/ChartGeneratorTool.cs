using UnityEngine;

public class ChartGeneratorTool : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField] private ChartGenerationSettings generationSettings;
    [SerializeField] private AudioSource musicSource;

    [Header("Save")]
    [SerializeField] private string saveFileName = "test_chart";

    [Header("Preview Optional")]
    [SerializeField] private ChartVisualizer visualizer;

    public string SaveFileName => saveFileName;
    public AudioClip CurrentAudioClip => musicSource != null ? musicSource.clip : null;

    public void ApplySongData(SongData song)
    {
        if (song == null) return;

        // --- Audio ---
        if (musicSource != null && song.audioClip != null)
            musicSource.clip = song.audioClip;

        // --- Chart file name ---
        if (!string.IsNullOrEmpty(song.ComputedChartFileName))
            saveFileName = song.ComputedChartFileName;

        // --- Kết nối BPM từ SongData → ChartGenerationSettings ---
        // SongData._bpm là BPM chuẩn của bài nhạc (nhập tay hoặc từ Auto Detect).
        // Nó sẽ pre-fill vào ChartGenerationSettings để không phải nhập lại.
        // Vẫn có thể Override tay trong ChartGenerationSettings sau đó.
        if (generationSettings != null && song._bpm > 0f)
        {
            generationSettings.bpm = song._bpm;
        }

        // --- Kết nối Difficulty từ SongData → ChartGenerationSettings ---
        // SongData._difficulty ("Easy"/"Normal"/"Hard") map vào enum preset.
        if (generationSettings != null && !string.IsNullOrEmpty(song._difficulty))
        {
            generationSettings.difficulty = ParseDifficulty(song._difficulty);
        }

        Debug.Log($"ChartGeneratorTool: Đã chọn '{song._songTitle}' " +
                  $"| BPM={song._bpm} | Difficulty={song._difficulty} " +
                  $"| Chart='{song.ComputedChartFileName}'");
    }

    /// <summary>
    /// Chuyển chuỗi độ khó ("Easy"/"Normal"/"Hard") thành enum ChartDifficultyPreset.
    /// </summary>
    private ChartDifficultyPreset ParseDifficulty(string diff)
    {
        switch (diff.Trim().ToLower())
        {
            case "easy":   return ChartDifficultyPreset.Easy;
            case "hard":   return ChartDifficultyPreset.Hard;
            default:       return ChartDifficultyPreset.Normal;
        }
    }

    public void GenerateAndSave()
    {
        if (generationSettings == null)
        {
            Debug.LogError("Generation settings is missing.");
            return;
        }

        if (musicSource == null || musicSource.clip == null)
        {
            Debug.LogError("Music source or AudioClip is missing.");
            return;
        }

        generationSettings.ApplyPreset();

        string songName = musicSource.clip.name;
        float songLength = musicSource.clip.length;

        ChartData chart = SimpleChartGenerator.Generate(
            songName,
            generationSettings.bpm,
            songLength,
            generationSettings.offset,
            generationSettings.laneCount,
            generationSettings.difficulty
        );

        ChartSaveLoad.Save(chart, saveFileName);

        Debug.Log($"Generated and saved chart: {chart.songName} | Notes: {chart.notes.Count}");

        if (visualizer != null)
        {
            visualizer.Settings = generationSettings;
            visualizer.Draw(chart);
        }
    }

    public void LoadAndPreview()
    {
        if (visualizer == null)
        {
            Debug.LogError("Visualizer is missing.");
            return;
        }

        if (!BeatmapParser.TryLoadChart(saveFileName, out ChartData loadedChart))
        {
            Debug.LogError($"Failed to load chart preview: {saveFileName}");
            return;
        }

        visualizer.Settings = generationSettings;
        visualizer.Draw(loadedChart);

        Debug.Log($"Loaded preview chart: {loadedChart.songName} | Notes: {loadedChart.notes.Count}");
    }

    public void AutoDetectBpm()
    {
        if (musicSource == null || musicSource.clip == null)
        {
            Debug.LogError("ChartGeneratorTool: Music source or AudioClip is missing.");
            return;
        }

        if (generationSettings == null)
        {
            Debug.LogError("ChartGeneratorTool: Generation Settings is missing.");
            return;
        }

        float detectedBpm = BpmDetector.Detect(musicSource.clip);
        generationSettings.bpm = detectedBpm;

        Debug.Log($"ChartGeneratorTool: Auto-detected BPM = {detectedBpm:F1} " +
                  $"và đã điền vào Generation Settings.");
    }

    public void SaveEditedChart()
    {
        if (visualizer == null)
        {
            Debug.LogError("Visualizer is missing.");
            return;
        }

        ChartData chart = visualizer.GetCurrentChart();

        if (chart == null)
        {
            Debug.LogWarning("No chart loaded to save.");
            return;
        }

        ChartSaveLoad.Save(chart, saveFileName);

        Debug.Log($"Saved edited chart: {chart.songName} | Notes: {chart.notes.Count}");
    }

    public ChartData GetCurrentPreviewChart()
    {
        return visualizer != null ? visualizer.GetCurrentChart() : null;
    }

    public void PreviewChart(ChartData chart)
    {
        if (visualizer == null)
        {
            Debug.LogError("Visualizer is missing.");
            return;
        }

        if (chart == null)
        {
            Debug.LogWarning("No chart data to preview.");
            return;
        }

        visualizer.Settings = generationSettings;
        visualizer.Draw(chart);

        Debug.Log($"Previewed chart: {chart.songName} | Notes: {chart.notes.Count}");
    }

    public void PreviewNoteVisualTestChart()
    {
        if (visualizer == null)
        {
            Debug.LogError("Visualizer is missing.");
            return;
        }

        ChartData chart = CreateNoteVisualTestChart();

        visualizer.Settings = generationSettings;
        visualizer.Draw(chart);

        Debug.Log("Previewed note visual test chart with Tap, Hold, Flick, and Slide notes.");
    }

    public void SaveNoteVisualTestChart()
    {
        ChartData chart = CreateNoteVisualTestChart();
        ChartSaveLoad.Save(chart, "chart_note_visual_test");

        Debug.Log("Saved note visual test chart: chart_note_visual_test");
    }

    public void PrepareNoteVisualRuntimeTest()
    {
        SaveNoteVisualTestChart();

        ChartNoteSpawner spawner = FindFirstObjectByType<ChartNoteSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("ChartGeneratorTool: ChartNoteSpawner not found.");
            return;
        }

        spawner.SetChartFileName("chart_note_visual_test");
        Debug.Log("Prepared runtime note visual test. Press Play to spawn typed notes.");
    }

    private ChartData CreateNoteVisualTestChart()
    {
        float bpm = generationSettings != null && generationSettings.bpm > 0f
            ? generationSettings.bpm
            : 120f;

        int laneCount = generationSettings != null && generationSettings.laneCount > 0
            ? generationSettings.laneCount
            : 4;

        return new ChartData
        {
            songName = "Note Visual Test",
            bpm = bpm,
            offset = 0f,
            laneCount = laneCount,
            notes =
            {
                new NoteData { time = 2f, lane = 0, type = NoteType.Tap },
                new NoteData { time = 4f, lane = 1, type = NoteType.Hold, duration = 1.5f },
                new NoteData
                {
                    time = 6f,
                    lane = 2,
                    type = NoteType.Flick,
                    flickDirection = FlickDirection.Up
                },
                new NoteData
                {
                    time = 8f,
                    lane = Mathf.Min(3, laneCount - 1),
                    type = NoteType.Slide,
                    duration = 1.25f,
                    slidePath = new[] { Mathf.Min(3, laneCount - 1), 2, 1 }
                }
            }
        };
    }
}
