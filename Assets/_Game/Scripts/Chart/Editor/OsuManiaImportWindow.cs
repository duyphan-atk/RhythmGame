#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class OsuManiaImportWindow : EditorWindow
{
    private const string MusicFolder = "Assets/_Game/Audio/Music/OsuImported";
    private const string SongDataFolder = "Assets/_Game/Data/Songs/OsuImported";

    private string osuFilePath = string.Empty;
    private string chartFileName = string.Empty;
    private bool copyAudioToProject = true;
    private bool createSongData = true;
    private bool previewInOpenTool = true;
    private bool prepareRuntimeSpawner = true;

    [MenuItem("Tools/RhythmGame/Chart/Import Osu Mania Beatmap")]
    public static void Open()
    {
        GetWindow<OsuManiaImportWindow>("osu!mania Import");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("osu!mania Import", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Imports .osu files with Mode: 3 into RhythmGame JSON charts. Only Tap and Hold notes are created.",
            MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.TextField("osu File", osuFilePath);
            if (GUILayout.Button("Browse", GUILayout.Width(86f)))
                BrowseOsuFile();
        }

        chartFileName = EditorGUILayout.TextField("Chart File Name", chartFileName);
        copyAudioToProject = EditorGUILayout.Toggle("Copy Audio To Project", copyAudioToProject);
        createSongData = EditorGUILayout.Toggle("Create SongData", createSongData);
        previewInOpenTool = EditorGUILayout.Toggle("Preview In Open Tool", previewInOpenTool);
        prepareRuntimeSpawner = EditorGUILayout.Toggle("Prepare Runtime Spawner", prepareRuntimeSpawner);

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(osuFilePath)))
        {
            if (GUILayout.Button("Import osu!mania Beatmap", GUILayout.Height(30f)))
                ImportBeatmap();
        }
    }

    private void BrowseOsuFile()
    {
        string selectedPath = EditorUtility.OpenFilePanel("Select osu!mania .osu file", string.Empty, "osu");
        if (string.IsNullOrWhiteSpace(selectedPath))
            return;

        osuFilePath = selectedPath;

        if (OsuManiaBeatmapParser.TryParse(osuFilePath, out OsuManiaBeatmapParser.ImportResult result, out _))
            chartFileName = BuildChartFileName(result);
        else
            chartFileName = "chart_" + SongData.SanitizeForFileName(Path.GetFileNameWithoutExtension(osuFilePath));
    }

    private void ImportBeatmap()
    {
        if (!OsuManiaBeatmapParser.TryParse(osuFilePath, out OsuManiaBeatmapParser.ImportResult result, out string error))
        {
            EditorUtility.DisplayDialog("Import Failed", error, "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(chartFileName))
            chartFileName = BuildChartFileName(result);

        ChartSaveLoad.Save(result.Chart, chartFileName);

        AudioClip importedClip = null;
        if (copyAudioToProject)
            importedClip = ImportAudio(result);

        SongData songData = null;
        if (createSongData)
            songData = CreateSongData(result, importedClip);

        SyncOpenScene(result.Chart, importedClip);

        string message =
            $"Imported '{Path.GetFileName(osuFilePath)}'\n" +
            $"Chart: {chartFileName}.json\n" +
            $"Lanes: {result.LaneCount}\n" +
            $"Notes: {result.Chart.notes.Count}\n" +
            $"BPM: {result.Chart.bpm:0.###}";

        if (songData != null)
            message += $"\nSongData: {AssetDatabase.GetAssetPath(songData)}";

        Debug.Log($"OsuManiaImportWindow: {message.Replace("\n", " | ")}");
        EditorUtility.DisplayDialog("osu!mania Import Complete", message, "OK");
    }

    private static string BuildChartFileName(OsuManiaBeatmapParser.ImportResult result)
    {
        string source = !string.IsNullOrWhiteSpace(result.AudioFileName)
            ? Path.GetFileNameWithoutExtension(result.AudioFileName)
            : result.Chart.songName;

        return "chart_" + SongData.SanitizeForFileName(source);
    }

    private static AudioClip ImportAudio(OsuManiaBeatmapParser.ImportResult result)
    {
        if (string.IsNullOrWhiteSpace(result.AudioFilePath) || !File.Exists(result.AudioFilePath))
        {
            Debug.LogWarning($"OsuManiaImportWindow: Audio file not found: {result.AudioFilePath}");
            return null;
        }

        EnsureFolder(MusicFolder);

        string extension = Path.GetExtension(result.AudioFilePath);
        string safeName = SongData.SanitizeForFileName(Path.GetFileNameWithoutExtension(result.AudioFilePath));
        string targetPath = AssetDatabase.GenerateUniqueAssetPath($"{MusicFolder}/{safeName}{extension}");

        File.Copy(result.AudioFilePath, targetPath, overwrite: false);
        AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceSynchronousImport);

        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(targetPath);
        if (clip == null)
            Debug.LogWarning($"OsuManiaImportWindow: Copied audio but could not load AudioClip: {targetPath}");

        return clip;
    }

    private static SongData CreateSongData(OsuManiaBeatmapParser.ImportResult result, AudioClip clip)
    {
        EnsureFolder(SongDataFolder);

        string safeName = SongData.SanitizeForFileName(result.Chart.songName);
        string path = AssetDatabase.GenerateUniqueAssetPath($"{SongDataFolder}/{safeName}.asset");

        SongData song = CreateInstance<SongData>();
        song._songTitle = result.Chart.songName;
        song._sceneName = "KhoaCuBu";
        song._bpm = result.Chart.bpm;
        song._difficulty = string.IsNullOrWhiteSpace(result.Version) ? "Normal" : result.Version;
        song.songGroupId = SongData.SanitizeForFileName($"{result.Artist}_{result.Title}");
        song.difficultyLevel = Difficulty.Easy;
        song.unlockType = SongUnlockType.Free;
        song.audioClip = clip;

        AssetDatabase.CreateAsset(song, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return song;
    }

    private void SyncOpenScene(ChartData chart, AudioClip clip)
    {
        ChartGeneratorTool tool = FindFirstObjectByType<ChartGeneratorTool>();
        if (tool != null)
        {
            SerializedObject serializedTool = new SerializedObject(tool);
            serializedTool.FindProperty("saveFileName").stringValue = chartFileName;

            if (clip != null)
            {
                SerializedProperty musicSourceProperty = serializedTool.FindProperty("musicSource");
                AudioSource source = musicSourceProperty.objectReferenceValue as AudioSource;
                if (source != null)
                {
                    Undo.RecordObject(source, "Import osu!mania Audio");
                    source.clip = clip;
                    EditorUtility.SetDirty(source);
                }
            }

            serializedTool.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tool);

            if (previewInOpenTool)
                tool.PreviewChart(chart);
        }

        if (prepareRuntimeSpawner)
        {
            ChartNoteSpawner spawner = FindFirstObjectByType<ChartNoteSpawner>();
            if (spawner != null)
            {
                Undo.RecordObject(spawner, "Import osu!mania Runtime Chart");
                spawner.SetChartFileName(chartFileName);
                EditorUtility.SetDirty(spawner);
            }
        }

        if (tool != null || prepareRuntimeSpawner)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void EnsureFolder(string folder)
    {
        string[] parts = folder.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }
}
#endif
