#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dypsloom.RhythmTimeline.Core;
using Dypsloom.RhythmTimeline.Core.Playables;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Timeline;

internal abstract class ChartTimelineAddNoteAction : TimelineAction
{
    protected abstract NoteType Type { get; }

    public override ActionValidity Validate(ActionContext context)
    {
        if (context.timeline is not RhythmTimelineAsset)
            return ActionValidity.NotApplicable;

        return GetRhythmTracks(context.tracks).Any()
            ? ActionValidity.Valid
            : ActionValidity.NotApplicable;
    }

    public override bool Execute(ActionContext context)
    {
        if (context.timeline is not RhythmTimelineAsset timeline)
            return false;

        List<RhythmTrack> rhythmTracks = GetRhythmTracks(context.tracks).ToList();
        if (rhythmTracks.Count == 0)
            return false;

        double noteTime = context.invocationTime ?? GetTimelinePlayheadTime();
        Object[] undoTargets = rhythmTracks
            .Cast<Object>()
            .Append(timeline)
            .ToArray();

        Undo.RecordObjects(undoTargets, $"Add RG {Type} Note At Cursor");

        TimelineClip lastClip = null;
        foreach (RhythmTrack rhythmTrack in rhythmTracks)
        {
            NoteData note = CreateNote(rhythmTrack.ID, Mathf.Max(0f, (float)noteTime));
            lastClip = ChartTimelineConverter.AddNoteToTimeline(timeline, note);
        }

        if (lastClip != null)
            TimelineEditor.selectedClip = lastClip;

        EditorGUIUtility.PingObject(timeline);
        return true;
    }

    private NoteData CreateNote(int lane, float time)
    {
        NoteData note = new NoteData
        {
            time = time,
            lane = Mathf.Max(0, lane),
            type = Type,
            flickDirection = FlickDirection.Any,
            slidePath = System.Array.Empty<int>()
        };

        if (Type == NoteType.Hold)
            note.duration = 1f;
        else if (Type == NoteType.Slide)
        {
            note.duration = 1f;
            note.slidePath = new[] { note.lane };
        }

        return note;
    }

    private static IEnumerable<RhythmTrack> GetRhythmTracks(IEnumerable<TrackAsset> tracks)
    {
        return tracks == null
            ? Enumerable.Empty<RhythmTrack>()
            : tracks.OfType<RhythmTrack>();
    }

    private static double GetTimelinePlayheadTime()
    {
        PropertyInfo property = typeof(TimelineEditor).GetProperty(
            "inspectedSequenceTime",
            BindingFlags.NonPublic | BindingFlags.Static);

        if (property != null && property.GetValue(null) is double timelineTime)
            return timelineTime;

        return TimelineEditor.inspectedDirector != null
            ? TimelineEditor.inspectedDirector.time
            : 0d;
    }
}

[MenuEntry("RhythmGame/Add RG Tap at Cursor")]
internal sealed class ChartTimelineAddTapAction : ChartTimelineAddNoteAction
{
    protected override NoteType Type => NoteType.Tap;

    [TimelineShortcut("RhythmGame/Add RG Tap", KeyCode.Alpha4)]
    private static void HandleShortcut(ShortcutArguments args)
    {
        Invoker.InvokeWithSelected<ChartTimelineAddTapAction>();
    }
}

[MenuEntry("RhythmGame/Add RG Hold at Cursor")]
internal sealed class ChartTimelineAddHoldAction : ChartTimelineAddNoteAction
{
    protected override NoteType Type => NoteType.Hold;

    [TimelineShortcut("RhythmGame/Add RG Hold", KeyCode.Alpha5)]
    private static void HandleShortcut(ShortcutArguments args)
    {
        Invoker.InvokeWithSelected<ChartTimelineAddHoldAction>();
    }
}

[MenuEntry("RhythmGame/Add RG Flick at Cursor")]
internal sealed class ChartTimelineAddFlickAction : ChartTimelineAddNoteAction
{
    protected override NoteType Type => NoteType.Flick;

    [TimelineShortcut("RhythmGame/Add RG Flick", KeyCode.Alpha7)]
    private static void HandleShortcut(ShortcutArguments args)
    {
        Invoker.InvokeWithSelected<ChartTimelineAddFlickAction>();
    }
}

[MenuEntry("RhythmGame/Add RG Slide at Cursor")]
internal sealed class ChartTimelineAddSlideAction : ChartTimelineAddNoteAction
{
    protected override NoteType Type => NoteType.Slide;

    [TimelineShortcut("RhythmGame/Add RG Slide", KeyCode.Alpha6)]
    private static void HandleShortcut(ShortcutArguments args)
    {
        Invoker.InvokeWithSelected<ChartTimelineAddSlideAction>();
    }
}
#endif
