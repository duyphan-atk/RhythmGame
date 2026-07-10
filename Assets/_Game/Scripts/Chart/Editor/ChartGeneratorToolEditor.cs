#if UNITY_EDITOR
using System;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Dypsloom.RhythmTimeline.Core;

[CustomEditor(typeof(ChartGeneratorTool))]
public class ChartGeneratorToolEditor : Editor
{
    private static RhythmTimelineAsset _selectedTimeline;
    private static NoteType _newNoteType = NoteType.Tap;
    private static int _newNoteLane;
    private static float _newNoteTime;
    private static float _newNoteDuration = 1f;
    private static FlickDirection _newFlickDirection = FlickDirection.Any;
    private static string _newSlidePath = string.Empty;

    // Cache danh sách SongData để không load lại mỗi frame
    private SongData[] _allSongs;
    private string[]   _songDisplayNames;
    private int        _selectedIndex = 0; // 0 = "-- Chọn bài --"

    private void OnEnable()
    {
        RefreshSongList();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ChartGeneratorTool tool = (ChartGeneratorTool)target;

        GUILayout.Space(12);
        EditorGUILayout.LabelField("── Song Selection ──────────────────", EditorStyles.boldLabel);

        // Nút refresh nếu vừa thêm bài mới
        if (GUILayout.Button("↻ Làm mới danh sách bài nhạc", GUILayout.Height(22)))
        {
            RefreshSongList();
        }

        if (_allSongs == null || _allSongs.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "Chưa có SongData nào.\nRight click Project → Create → Data → SongData",
                MessageType.Info);
        }
        else
        {
            // Dropdown chọn bài
            int newIndex = EditorGUILayout.Popup("Chọn bài nhạc", _selectedIndex, _songDisplayNames);

            if (newIndex != _selectedIndex && newIndex > 0)
            {
                SongData chosenSong = _allSongs[newIndex - 1]; // -1 vì index 0 là "-- Chọn bài --"

                // Hiện cảnh báo xác nhận trước khi đổi (tránh mất chart chưa lưu)
                bool confirmed = EditorUtility.DisplayDialog(
                    "Đổi bài nhạc?",
                    $"Chuyển sang \"{chosenSong._songTitle}\".\n\n" +
                    "Nếu chưa bấm 'Save Edited Chart', các chỉnh sửa trên chart hiện tại sẽ bị mất.",
                    "Đổi bài",
                    "Hủy");

                if (confirmed)
                {
                    _selectedIndex = newIndex;
                    Undo.RecordObject(tool, "Select Song");
                    tool.ApplySongData(chosenSong);
                    EditorUtility.SetDirty(tool);

                    // Đồng bộ ChartNoteSpawner.chartFileName để Play mode load đúng JSON.
                    ChartNoteSpawner spawner = FindFirstObjectByType<ChartNoteSpawner>();
                    if (spawner != null)
                    {
                        Undo.RecordObject(spawner, "Select Song - Sync Spawner");
                        spawner.SetChartFileName(chosenSong.ComputedChartFileName);
                        EditorUtility.SetDirty(spawner);
                        Debug.Log($"ChartGeneratorToolEditor: Đã đồng bộ ChartNoteSpawner → '{chosenSong.ComputedChartFileName}'");
                    }
                }
            }
            else
            {
                _selectedIndex = newIndex;
            }

            // Preview chart filename sẽ được tạo
            if (_selectedIndex > 0)
            {
                SongData currentSong = _allSongs[_selectedIndex - 1];
                string preview = string.IsNullOrEmpty(currentSong.ComputedChartFileName)
                    ? "(Chưa gán AudioClip vào SongData)"
                    : $"→ Sẽ lưu: {currentSong.ComputedChartFileName}.json";

                EditorGUILayout.HelpBox(preview, MessageType.None);
            }
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("── Actions ─────────────────────────", EditorStyles.boldLabel);

        if (GUILayout.Button("Auto Detect BPM"))      tool.AutoDetectBpm();

        GUILayout.Space(4);

        if (GUILayout.Button("Generate And Save Chart")) tool.GenerateAndSave();
        if (GUILayout.Button("Load Preview Chart"))      tool.LoadAndPreview();
        if (GUILayout.Button("Save Edited Chart"))       tool.SaveEditedChart();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("── Visual Test ─────────────────────", EditorStyles.boldLabel);

        if (GUILayout.Button("Preview Note Visual Test"))
            tool.PreviewNoteVisualTestChart();

        if (GUILayout.Button("Save Note Visual Test Chart"))
            tool.SaveNoteVisualTestChart();

        if (GUILayout.Button("Prepare Note Visual Runtime Test"))
        {
            tool.PrepareNoteVisualRuntimeTest();

            ChartNoteSpawner spawner = FindFirstObjectByType<ChartNoteSpawner>();
            if (spawner != null)
                EditorUtility.SetDirty(spawner);
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("── Timeline Bridge ─────────────────", EditorStyles.boldLabel);

        _selectedTimeline = (RhythmTimelineAsset)EditorGUILayout.ObjectField(
            "Timeline Asset",
            _selectedTimeline,
            typeof(RhythmTimelineAsset),
            false);

        if (_selectedTimeline == null && Selection.activeObject is RhythmTimelineAsset selectedAsset)
            _selectedTimeline = selectedAsset;

        if (GUILayout.Button("Create Timeline From Preview"))
            CreateTimelineFromPreview(tool);

        if (GUILayout.Button("Create Timeline From Saved Chart"))
            CreateTimelineFromSavedChart(tool);

        using (new EditorGUI.DisabledScope(_selectedTimeline == null))
        {
            if (GUILayout.Button("Setup Timeline Audio Preview"))
                ChartTimelineAudioPreviewSetup.Setup(_selectedTimeline, tool.CurrentAudioClip, showDialog: true);
        }

        DrawTimelineNoteTools();

        if (GUILayout.Button("Export Timeline To JSON"))
            ExportTimelineToJson(tool, prepareRuntime: false);

        if (GUILayout.Button("Export Timeline And Prepare Runtime"))
            ExportTimelineToJson(tool, prepareRuntime: true);
    }

    // ─── Private ─────────────────────────────────────────────────────────────

    private void RefreshSongList()
    {
        string[] guids = AssetDatabase.FindAssets("t:SongData");

        _allSongs = new SongData[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            _allSongs[i] = AssetDatabase.LoadAssetAtPath<SongData>(path);
        }

        // Index 0 là placeholder "-- Chọn bài --"
        _songDisplayNames = new string[_allSongs.Length + 1];
        _songDisplayNames[0] = "-- Chọn bài nhạc --";
        for (int i = 0; i < _allSongs.Length; i++)
        {
            string title  = _allSongs[i]._songTitle;
            string file   = _allSongs[i].ComputedChartFileName;
            string label  = string.IsNullOrEmpty(title) ? _allSongs[i].name : title;
            string suffix = string.IsNullOrEmpty(file)  ? " (chưa gán AudioClip)" : $"  [{file}]";
            _songDisplayNames[i + 1] = label + suffix;
        }

        _selectedIndex = 0;
    }

    private void CreateTimelineFromPreview(ChartGeneratorTool tool)
    {
        ChartData chart = tool.GetCurrentPreviewChart();
        if (chart == null)
        {
            EditorUtility.DisplayDialog(
                "No Preview Chart",
                "Load, generate, or preview a chart before creating a timeline.",
                "OK");
            return;
        }

        CreateTimelineAsset(tool, chart);
    }

    private void CreateTimelineFromSavedChart(ChartGeneratorTool tool)
    {
        if (!BeatmapParser.TryLoadChart(tool.SaveFileName, out ChartData chart))
        {
            EditorUtility.DisplayDialog(
                "Cannot Load Chart",
                $"Could not load saved chart '{tool.SaveFileName}'.",
                "OK");
            return;
        }

        CreateTimelineAsset(tool, chart);
    }

    private void CreateTimelineAsset(ChartGeneratorTool tool, ChartData chart)
    {
        string safeName = string.IsNullOrWhiteSpace(chart.songName)
            ? tool.SaveFileName
            : chart.songName.Replace(' ', '_');

        string path = EditorUtility.SaveFilePanelInProject(
            "Create Rhythm Timeline From Chart",
            $"{safeName}_Timeline",
            "asset",
            "Create a Dypsloom Rhythm Timeline asset from the current chart.",
            "Assets/_Game/Data/Timelines");

        if (string.IsNullOrWhiteSpace(path))
            return;

        RhythmTimelineAsset timeline = ChartTimelineConverter.CreateTimelineFromChart(
            chart,
            path,
            tool.CurrentAudioClip);

        if (timeline != null)
        {
            _selectedTimeline = timeline;
            Selection.activeObject = timeline;
            EditorGUIUtility.PingObject(timeline);
        }
    }

    private void ExportTimelineToJson(ChartGeneratorTool tool, bool prepareRuntime)
    {
        if (_selectedTimeline == null)
        {
            EditorUtility.DisplayDialog(
                "No Timeline Selected",
                "Assign a Timeline Asset or select one in the Project window first.",
                "OK");
            return;
        }

        ChartData chart = ChartTimelineConverter.ExportTimelineToChart(_selectedTimeline);
        if (chart == null)
            return;

        ChartSaveLoad.Save(chart, tool.SaveFileName);
        tool.PreviewChart(chart);

        if (prepareRuntime)
        {
            ChartNoteSpawner spawner = FindFirstObjectByType<ChartNoteSpawner>();
            if (spawner != null)
            {
                Undo.RecordObject(spawner, "Prepare Runtime From Timeline");
                spawner.SetChartFileName(tool.SaveFileName);
                EditorUtility.SetDirty(spawner);
            }
        }

        string message = prepareRuntime
            ? $"Exported '{_selectedTimeline.name}' to '{tool.SaveFileName}.json' and prepared runtime."
            : $"Exported '{_selectedTimeline.name}' to '{tool.SaveFileName}.json'.";

        Debug.Log(message);
        EditorUtility.DisplayDialog("Timeline Export Complete", message, "OK");
    }

    private void DrawTimelineNoteTools()
    {
        GUILayout.Space(6);
        EditorGUILayout.LabelField("Timeline Note Tools", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Use this section for RhythmGame notes. Avoid the Dypsloom Select Note dropdown unless you need package demos.",
            MessageType.Info);

        _newNoteType = (NoteType)EditorGUILayout.EnumPopup("RG Note Type", _newNoteType);
        _newNoteLane = Mathf.Max(0, EditorGUILayout.IntField("Lane", _newNoteLane));
        _newNoteTime = Mathf.Max(0f, EditorGUILayout.FloatField("Time Seconds", _newNoteTime));

        if (_newNoteType == NoteType.Hold || _newNoteType == NoteType.Slide)
            _newNoteDuration = Mathf.Max(0.1f, EditorGUILayout.FloatField("Duration Seconds", _newNoteDuration));

        if (_newNoteType == NoteType.Flick)
            _newFlickDirection = (FlickDirection)EditorGUILayout.EnumPopup("Flick Direction", _newFlickDirection);

        if (_newNoteType == NoteType.Slide)
            _newSlidePath = EditorGUILayout.TextField("Slide Path Lanes", _newSlidePath);

        using (new EditorGUI.DisabledScope(_selectedTimeline == null))
        {
            if (GUILayout.Button("Add RG Note At Time"))
                AddTimelineNoteAtTime();
        }
    }

    private void AddTimelineNoteAtTime()
    {
        if (_selectedTimeline == null)
        {
            EditorUtility.DisplayDialog(
                "No Timeline Selected",
                "Assign a Timeline Asset before adding a note.",
                "OK");
            return;
        }

        NoteData note = new NoteData
        {
            time = _newNoteTime,
            lane = _newNoteLane,
            type = _newNoteType,
            duration = _newNoteType == NoteType.Hold || _newNoteType == NoteType.Slide
                ? _newNoteDuration
                : 0f,
            flickDirection = _newNoteType == NoteType.Flick
                ? _newFlickDirection
                : FlickDirection.Any,
            slidePath = _newNoteType == NoteType.Slide
                ? ParseSlidePath(_newSlidePath, _newNoteLane)
                : Array.Empty<int>()
        };

        Undo.RegisterCompleteObjectUndo(_selectedTimeline, "Add RG Note To Timeline");
        ChartTimelineConverter.AddNoteToTimeline(_selectedTimeline, note);
        EditorGUIUtility.PingObject(_selectedTimeline);
    }

    private static int[] ParseSlidePath(string value, int fallbackLane)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new[] { fallbackLane };

        int[] lanes = value
            .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseLane)
            .ToArray();

        return lanes.Length > 0 ? lanes : new[] { fallbackLane };
    }

    private static int ParseLane(string value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int lane)
            ? Mathf.Max(0, lane)
            : 0;
    }
}
#endif
