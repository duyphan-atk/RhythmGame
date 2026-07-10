#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Dypsloom.RhythmTimeline.Core;
using Dypsloom.RhythmTimeline.Core.Notes;
using Dypsloom.RhythmTimeline.Core.Playables;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

public static class ChartTimelineConverter
{
    private const double DefaultClipDuration = 0.1d;

    public static RhythmTimelineAsset CreateTimelineFromChart(
        ChartData chart,
        string assetPath,
        AudioClip audioClip = null)
    {
        if (chart == null)
        {
            Debug.LogError("ChartTimelineConverter: Chart is null.");
            return null;
        }

        if (chart.notes == null)
        {
            Debug.LogError("ChartTimelineConverter: Chart notes is null.");
            return null;
        }

        int laneCount = Mathf.Max(1, chart.laneCount);
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        EnsureDirectory(assetPath);

        RhythmTimelineAsset timeline = ScriptableObject.CreateInstance<RhythmTimelineAsset>();
        ApplyTimelineSettings(timeline, chart, audioClip);

        AssetDatabase.CreateAsset(timeline, assetPath);

        CreateAudioTrack(timeline, audioClip);
        List<RhythmTrack> tracks = CreateRhythmTracks(timeline, laneCount);
        NoteDefinition[] noteDefinitions = LoadOrCreateNoteDefinitions();

        chart.notes.Sort((a, b) => a.time.CompareTo(b.time));

        for (int i = 0; i < chart.notes.Count; i++)
        {
            NoteData note = chart.notes[i];
            int lane = Mathf.Clamp(note.lane, 0, laneCount - 1);

            TimelineClip clip = tracks[lane].CreateClip<RhythmClip>();
            ApplyNoteToClip(clip, note, noteDefinitions[(int)note.type]);
        }

        EditorUtility.SetDirty(timeline);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved | RefreshReason.WindowNeedsRedraw);

        Debug.Log($"ChartTimelineConverter: Created timeline '{assetPath}' from chart '{chart.songName}' with {chart.notes.Count} notes.");
        return timeline;
    }

    public static TimelineClip AddNoteToTimeline(RhythmTimelineAsset timeline, NoteData note)
    {
        if (timeline == null)
        {
            Debug.LogError("ChartTimelineConverter: Timeline is null.");
            return null;
        }

        if (note == null)
        {
            Debug.LogError("ChartTimelineConverter: Note is null.");
            return null;
        }

        int lane = Mathf.Max(0, note.lane);
        RhythmTrack track = FindOrCreateRhythmTrack(timeline, lane);
        NoteDefinition noteDefinition = LoadOrCreateNoteDefinition(note.type);

        TimelineClip clip = track.CreateClip<RhythmClip>();
        ApplyNoteToClip(clip, note, noteDefinition);

        EditorUtility.SetDirty(track);
        EditorUtility.SetDirty(timeline);
        AssetDatabase.SaveAssets();
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved | RefreshReason.WindowNeedsRedraw);

        Debug.Log($"ChartTimelineConverter: Added {note.type} note at {note.time:0.###}s on lane {lane}.");
        return clip;
    }

    public static ChartData ExportTimelineToChart(RhythmTimelineAsset timeline)
    {
        if (timeline == null)
        {
            Debug.LogError("ChartTimelineConverter: Timeline is null.");
            return null;
        }

        ChartData chart = new ChartData
        {
            songName = string.IsNullOrWhiteSpace(timeline.FullName) ? timeline.name : timeline.FullName,
            bpm = timeline.Bpm > 0f ? timeline.Bpm : 120f,
            offset = 0f,
            laneCount = 0
        };

        foreach (TrackAsset trackAsset in timeline.GetOutputTracks())
        {
            if (trackAsset is not RhythmTrack rhythmTrack)
                continue;

            int lane = Mathf.Max(0, rhythmTrack.ID);
            chart.laneCount = Mathf.Max(chart.laneCount, lane + 1);

            foreach (TimelineClip clip in rhythmTrack.GetClips())
            {
                if (clip.asset is not RhythmClip rhythmClip)
                    continue;

                NoteType noteType = GetNoteTypeFromClip(rhythmClip);

                NoteData note = new NoteData
                {
                    time = (float)clip.start,
                    lane = lane,
                    type = noteType,
                    duration = GetNoteDurationFromClip(noteType, clip),
                    flickDirection = FlickDirection.Any
                };

                ChartTimelineMetadata.Decode(rhythmClip.ClipParameters.StringParameter, note);
                note.duration = GetNoteDurationFromClip(note.type, clip);
                chart.notes.Add(note);
            }
        }

        chart.notes.Sort((a, b) => a.time.CompareTo(b.time));

        Debug.Log($"ChartTimelineConverter: Exported timeline '{timeline.name}' to chart with {chart.notes.Count} notes.");
        return chart;
    }

    private static void ApplyTimelineSettings(
        RhythmTimelineAsset timeline,
        ChartData chart,
        AudioClip audioClip)
    {
        SerializedObject serializedTimeline = new SerializedObject(timeline);

        serializedTimeline.FindProperty("m_Bpm").floatValue = chart.bpm > 0f ? chart.bpm : 120f;
        serializedTimeline.FindProperty("m_FullName").stringValue = chart.songName;
        serializedTimeline.FindProperty("m_AudioClip").objectReferenceValue = audioClip;

        serializedTimeline.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateAudioTrack(RhythmTimelineAsset timeline, AudioClip audioClip)
    {
        AudioTrack audioTrack = timeline.CreateTrack<AudioTrack>(null, "Music");

        if (audioClip == null)
            return;

        TimelineClip audioClipTimeline = audioTrack.CreateClip(audioClip);
        audioClipTimeline.start = 0d;
        audioClipTimeline.duration = audioClip.length;
    }

    private static List<RhythmTrack> CreateRhythmTracks(RhythmTimelineAsset timeline, int laneCount)
    {
        List<RhythmTrack> tracks = new List<RhythmTrack>();

        for (int i = 0; i < laneCount; i++)
        {
            RhythmTrack rhythmTrack = timeline.CreateTrack<RhythmTrack>(null, $"Lane {i}");
            rhythmTrack.SetID(i);
            tracks.Add(rhythmTrack);
        }

        return tracks;
    }

    private static NoteDefinition[] LoadOrCreateNoteDefinitions()
    {
        return new[]
        {
            LoadOrCreateNoteDefinition(NoteType.Tap),
            LoadOrCreateNoteDefinition(NoteType.Hold),
            LoadOrCreateNoteDefinition(NoteType.Flick),
            LoadOrCreateNoteDefinition(NoteType.Slide)
        };
    }

    private static NoteDefinition LoadOrCreateNoteDefinition(NoteType noteType)
    {
        string folder = "Assets/_Game/Data/NoteDefinitions";
        string path = $"{folder}/RG_{noteType}NoteDefinition.asset";

        NoteDefinition existing = AssetDatabase.LoadAssetAtPath<NoteDefinition>(path);
        if (existing != null)
        {
            ConfigureNoteDefinition(existing, noteType);
            return existing;
        }

        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Game/Data", "NoteDefinitions");

        NoteDefinition noteDefinition = ScriptableObject.CreateInstance<NoteDefinition>();
        ConfigureNoteDefinition(noteDefinition, noteType);

        AssetDatabase.CreateAsset(noteDefinition, path);
        return noteDefinition;
    }

    private static void ConfigureNoteDefinition(NoteDefinition noteDefinition, NoteType noteType)
    {
        if (noteDefinition == null)
            return;

        noteDefinition.name = $"RG_{noteType}";

        SerializedObject serializedDefinition = new SerializedObject(noteDefinition);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/_ThirdParty/Dypsloom/RhythmTimeline/Demos/OSU/Notes/OSUTapNote.prefab");

        if (prefab != null)
            serializedDefinition.FindProperty("m_NotePrefab").objectReferenceValue = prefab;

        serializedDefinition.FindProperty("m_ClipDuration").enumValueIndex =
            noteType == NoteType.Hold || noteType == NoteType.Slide
                ? (int)NoteDefinition.ClipDurationType.Free
                : (int)NoteDefinition.ClipDurationType.FixedDuration;

        serializedDefinition.FindProperty("m_DurationScaler").floatValue = 0.1f;
        serializedDefinition.FindProperty("m_RhythmClipEditorSettings.m_Color").colorValue =
            GetTimelineClipColor(noteType);

        serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(noteDefinition);
    }

    private static Color GetTimelineClipColor(NoteType noteType)
    {
        return noteType switch
        {
            NoteType.Hold => new Color(1f, 0.82f, 0.28f, 1f),
            NoteType.Flick => new Color(1f, 0.45f, 0.95f, 1f),
            NoteType.Slide => new Color(0.42f, 1f, 0.5f, 1f),
            _ => new Color(0.33f, 0.86f, 1f, 1f)
        };
    }

    private static double GetClipDuration(NoteData note)
    {
        if ((note.type == NoteType.Hold || note.type == NoteType.Slide) && note.duration > 0f)
            return note.duration;

        return DefaultClipDuration;
    }

    private static NoteType GetNoteTypeFromClip(RhythmClip rhythmClip)
    {
        int value = rhythmClip.ClipParameters.IntParameter;
        return System.Enum.IsDefined(typeof(NoteType), value)
            ? (NoteType)value
            : NoteType.Tap;
    }

    private static float GetNoteDurationFromClip(
        NoteType noteType,
        TimelineClip clip)
    {
        if (noteType != NoteType.Hold && noteType != NoteType.Slide)
            return 0f;

        return Mathf.Max(0f, (float)clip.duration);
    }

    private static string GetClipDisplayName(NoteData note)
    {
        return $"RG_{note.type} L{note.lane}";
    }

    private static void ApplyNoteToClip(
        TimelineClip clip,
        NoteData note,
        NoteDefinition noteDefinition)
    {
        clip.displayName = GetClipDisplayName(note);
        clip.start = Mathf.Max(0f, note.time);
        clip.duration = GetClipDuration(note);

        RhythmClip rhythmClip = clip.asset as RhythmClip;
        if (rhythmClip == null)
            return;

        rhythmClip.SetNoteDefinition(noteDefinition);
        rhythmClip.ClipParameters.IntParameter = (int)note.type;
        rhythmClip.ClipParameters.FloatParameter = note.duration;
        rhythmClip.ClipParameters.StringParameter = ChartTimelineMetadata.Encode(note);

        EditorUtility.SetDirty(rhythmClip);
    }

    private static RhythmTrack FindOrCreateRhythmTrack(RhythmTimelineAsset timeline, int lane)
    {
        foreach (TrackAsset trackAsset in timeline.GetOutputTracks())
        {
            if (trackAsset is RhythmTrack rhythmTrack && rhythmTrack.ID == lane)
                return rhythmTrack;
        }

        RhythmTrack newTrack = timeline.CreateTrack<RhythmTrack>(null, $"Lane {lane}");
        newTrack.SetID(lane);
        return newTrack;
    }

    private static void EnsureDirectory(string assetPath)
    {
        string directory = Path.GetDirectoryName(assetPath);
        if (string.IsNullOrEmpty(directory) || AssetDatabase.IsValidFolder(directory))
            return;

        string[] parts = directory.Replace("\\", "/").Split('/');
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
