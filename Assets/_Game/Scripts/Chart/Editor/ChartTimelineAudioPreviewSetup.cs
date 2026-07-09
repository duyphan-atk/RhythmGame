#if UNITY_EDITOR
using System.Linq;
using Dypsloom.RhythmTimeline.Core;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public static class ChartTimelineAudioPreviewSetup
{
    private const string PreviewObjectName = "===Timeline Audio Preview===";

    [MenuItem("Tools/RhythmGame/Timeline/Setup Audio Preview For Open Timeline")]
    public static void SetupFromMenu()
    {
        RhythmTimelineAsset timeline = GetCurrentTimeline();
        if (timeline == null)
        {
            EditorUtility.DisplayDialog(
                "No Rhythm Timeline",
                "Open or select a RhythmTimelineAsset first.",
                "OK");
            return;
        }

        Setup(timeline, fallbackAudioClip: null, showDialog: false);
    }

    [MenuItem("Tools/RhythmGame/Timeline/Setup Audio Preview For Open Timeline", true)]
    private static bool ValidateSetupFromMenu()
    {
        return GetCurrentTimeline() != null;
    }

    public static PlayableDirector Setup(
        RhythmTimelineAsset timeline,
        AudioClip fallbackAudioClip,
        bool showDialog)
    {
        if (timeline == null)
            return null;

        GameObject previewObject = GameObject.Find(PreviewObjectName);
        if (previewObject == null)
        {
            previewObject = new GameObject(PreviewObjectName);
            Undo.RegisterCreatedObjectUndo(previewObject, "Create Timeline Audio Preview");
        }

        PlayableDirector director = previewObject.GetComponent<PlayableDirector>();
        if (director == null)
            director = Undo.AddComponent<PlayableDirector>(previewObject);

        AudioSource audioSource = previewObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = Undo.AddComponent<AudioSource>(previewObject);

        Undo.RecordObjects(new Object[] { previewObject, director, audioSource }, "Setup Timeline Audio Preview");

        director.playableAsset = timeline;
        director.playOnAwake = false;
        director.timeUpdateMode = DirectorUpdateMode.DSPClock;
        director.time = 0d;

        AudioClip audioClip = GetTimelineAudioClip(timeline) ?? fallbackAudioClip;
        if (audioClip != null)
            audioSource.clip = audioClip;

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        int boundAudioTracks = BindAudioTracks(timeline, director, audioSource);
        director.RebuildGraph();

        EditorUtility.SetDirty(previewObject);
        EditorUtility.SetDirty(director);
        EditorUtility.SetDirty(audioSource);

        Selection.activeGameObject = previewObject;

        TimelineEditorWindow timelineWindow = TimelineEditor.GetOrCreateWindow();
        timelineWindow.SetTimeline(director);
        timelineWindow.locked = true;
        TimelineEditor.Refresh(RefreshReason.ContentsModified | RefreshReason.WindowNeedsRedraw);

        string message = boundAudioTracks > 0
            ? $"Timeline audio preview is ready. Bound {boundAudioTracks} audio track(s) to '{PreviewObjectName}'."
            : "Timeline preview object is ready, but no AudioTrack was found to bind.";

        Debug.Log($"ChartTimelineAudioPreviewSetup: {message}");

        if (showDialog)
            EditorUtility.DisplayDialog("Timeline Audio Preview", message, "OK");

        return director;
    }

    private static RhythmTimelineAsset GetCurrentTimeline()
    {
        if (TimelineEditor.inspectedAsset is RhythmTimelineAsset inspectedTimeline)
            return inspectedTimeline;

        if (Selection.activeObject is RhythmTimelineAsset selectedTimeline)
            return selectedTimeline;

        return null;
    }

    private static int BindAudioTracks(
        RhythmTimelineAsset timeline,
        PlayableDirector director,
        AudioSource audioSource)
    {
        int count = 0;

        foreach (AudioTrack audioTrack in timeline.GetOutputTracks().OfType<AudioTrack>())
        {
            director.SetGenericBinding(audioTrack, audioSource);
            count++;
        }

        return count;
    }

    private static AudioClip GetTimelineAudioClip(RhythmTimelineAsset timeline)
    {
        SerializedObject serializedTimeline = new SerializedObject(timeline);
        SerializedProperty audioClipProperty = serializedTimeline.FindProperty("m_AudioClip");
        return audioClipProperty != null
            ? audioClipProperty.objectReferenceValue as AudioClip
            : null;
    }
}
#endif
