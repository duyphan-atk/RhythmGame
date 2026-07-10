# Map Editor Timeline Handoff

Last updated: 2026-07-09

## Current Goal

Build a rhythm-game map editor workflow that starts from the existing JSON chart/runtime spawner, then bridges into Dypsloom Rhythm Timeline for timeline-based editing.

## What Has Been Implemented

- Added `NoteVisualConfig`:
  - Path: `Assets/_Game/Data/NoteVisuals/DefaultNoteVisualConfig.asset`
  - Controls sprite, tint, runtime UI size, and preview scale for `Tap`, `Hold`, `Flick`, and `Slide`.
- Updated gameplay note art:
  - Source WebP files are under `Assets/_Game/Sprites/GamePlay`.
  - Runtime-friendly PNG copies were generated beside them:
    - `Click.png` -> Tap
    - `Hold.png` -> Hold
    - `Drag.png` -> Slide
    - `Flick.png` -> Flick
  - PNG import settings are `Sprite (2D and UI)`, single sprite.
- Updated preview chart rendering:
  - `ChartVisualizer` applies `NoteVisualConfig`.
- Updated runtime note visuals:
  - `NoteBase` applies `NoteVisualConfig`.
  - `ChartNoteSpawner` passes `NoteType` and visual config into runtime notes.
  - `HoldNote` now stretches its visual height by `duration * scrollSpeed`, so a 3-second Hold is visibly longer than a 1-second Hold.
- Updated runtime spawning:
  - `ChartNoteSpawner.useGeneratedTypedNotes = true`
  - Runtime now creates typed notes:
    - `TapNote`
    - `HoldNote`
    - `FlickNote`
    - `SlideNote`
- Added test buttons to `ChartGeneratorToolEditor`:
  - `Preview Note Visual Test`
  - `Save Note Visual Test Chart`
  - `Prepare Note Visual Runtime Test`
- Added Timeline bridge:
  - `Create Timeline From Preview`
  - `Create Timeline From Saved Chart`
  - `Export Timeline To JSON`
  - `Export Timeline And Prepare Runtime`
- Added Timeline note tools:
  - `RG Note Type`
  - `Lane`
  - `Time Seconds`
  - `Duration Seconds` for Hold/Slide
  - `Flick Direction` for Flick
  - `Slide Path Lanes` for Slide
  - `Add RG Note At Time`
  - These tools create RhythmGame notes directly and avoid the cluttered Dypsloom demo note dropdown.
- Added direct Timeline context-menu actions:
  - Right-click a `Lane` / `RhythmTrack` in the Timeline window.
  - Use `RhythmGame/Add RG Tap at Cursor`.
  - Use `RhythmGame/Add RG Hold at Cursor`.
  - Use `RhythmGame/Add RG Flick at Cursor`.
  - Use `RhythmGame/Add RG Slide at Cursor`.
  - These actions read the Timeline context-menu cursor time and the selected/right-clicked track lane, so they do not depend on the visible playhead.
- Added Timeline keyboard shortcuts:
  - `4`: Add `RG_Tap`
  - `5`: Add `RG_Hold`
  - `6`: Add `RG_Slide`
  - `7`: Add `RG_Flick`
  - Shortcuts use the selected lane track and the current Timeline time.
- Added Timeline audio preview setup:
  - Menu: `Tools/RhythmGame/Timeline/Setup Audio Preview For Open Timeline`
  - Inspector fallback button: `Setup Timeline Audio Preview`
  - Creates/uses scene object `===Timeline Audio Preview===`.
  - Adds/configures `PlayableDirector` and `AudioSource`.
  - Assigns the open/selected `RhythmTimelineAsset` to the director.
  - Binds Timeline `AudioTrack` output to the preview `AudioSource`.
  - Opens/locks the Timeline window against that `PlayableDirector` so Timeline preview/play can work without entering Play Mode.
- Added converter:
  - `Assets/_Game/Scripts/Chart/Editor/ChartTimelineConverter.cs`
  - Creates `RhythmTimelineAsset` from `ChartData`.
  - Exports `RhythmTimelineAsset` back to `ChartData`.
- Added metadata helper:
  - `Assets/_Game/Scripts/Chart/Timeline/ChartTimelineMetadata.cs`
  - Encodes flick direction and slide path into `RhythmClip.ClipParameters.StringParameter`.
- Added NoteDefinition assets:
  - `Assets/_Game/Data/NoteDefinitions/TapNoteDefinition.asset`
  - `Assets/_Game/Data/NoteDefinitions/HoldNoteDefinition.asset`
  - `Assets/_Game/Data/NoteDefinitions/FlickNoteDefinition.asset`
  - `Assets/_Game/Data/NoteDefinitions/SlideNoteDefinition.asset`
  - They use Dypsloom OSU tap prefab as an editor fallback so Timeline clips render without `m_NotePrefab` errors.
- New Timeline notes now use/create clearer `RG_*` NoteDefinition assets:
  - `Assets/_Game/Data/NoteDefinitions/RG_TapNoteDefinition.asset`
  - `Assets/_Game/Data/NoteDefinitions/RG_HoldNoteDefinition.asset`
  - `Assets/_Game/Data/NoteDefinitions/RG_FlickNoteDefinition.asset`
  - `Assets/_Game/Data/NoteDefinitions/RG_SlideNoteDefinition.asset`
  - Old non-prefixed assets may still exist for older timelines.

## Important Scene

Use:

`Assets/_Game/Scenes/Sandbox/tndKhoa/KhoaCuBu.unity`

Main object:

`===Chart Generator Tool===`

## Manual Test Flow

### Visual Preview Test

1. Open `KhoaCuBu`.
2. Select `===Chart Generator Tool===`.
3. Click `Preview Note Visual Test`.
4. Expected:
   - Four notes appear in preview.
   - Tap/Hold/Flick/Slide use different visuals.
   - Notes are on different lanes.

### Runtime Test

1. Click `Prepare Note Visual Runtime Test`.
2. Press Play.
3. Expected:
   - Runtime spawns typed notes at 2s, 4s, 6s, and 8s.
   - Hierarchy names include `Tap`, `Hold`, `Flick`, and `Slide`.
   - Slide note has `SLIDE_CHECKPOINT_*` children.
   - Hold note visual height grows with duration.

### Timeline Export Test

1. Click `Preview Note Visual Test`.
2. Click `Create Timeline From Preview`.
3. Save under `Assets/_Game/Data/TimeLine` or `Assets/_Game/Data/Timelines`.
4. Open the created Timeline asset.
5. Expected:
   - Tracks: `Music`, `Lane 0`, `Lane 1`, `Lane 2`, `Lane 3`.
   - Clips: `Tap L0`, `Hold L1`, `Flick L2`, `Slide L3`.
   - No `m_NotePrefab` error.

### Timeline Import Back To JSON

1. Assign the Timeline asset to `Timeline Asset` in the `Timeline Bridge` section.
2. Edit clips in the Timeline window.
3. Click `Export Timeline And Prepare Runtime`.
4. Press Play.
5. Expected:
   - The edited Timeline is exported to the current `saveFileName` JSON.
   - `ChartNoteSpawner` is prepared to load that chart.
   - `Chart Visualizer` redraws the exported Timeline chart immediately.
   - Runtime spawns notes from the exported chart.

### Add Note At Exact Time

1. Select `===Chart Generator Tool===`.
2. Assign the Timeline asset in `Timeline Asset`.
3. In `Timeline Note Tools`, set:
   - `RG Note Type`: `Tap`, `Hold`, `Flick`, or `Slide`
   - `Lane`: target lane
   - `Time Seconds`: exact note time, for example `10`
   - `Duration Seconds`: only for Hold/Slide
   - `Flick Direction`: only for Flick
   - `Slide Path Lanes`: only for Slide, for example `1,2,3`
4. Click `Add RG Note At Time`.
5. Expected:
   - A clip named like `RG_Tap L2` is added at the requested time/lane.
   - Exporting the Timeline writes that note to JSON.
   - Runtime spawns the new note after `Export Timeline And Prepare Runtime`.

### Add Note Directly In Timeline

1. Open the Timeline asset in Unity Timeline.
2. Right-click the target lane track at the target horizontal time position, for example around `10s` on `Lane 2`.
4. Choose:
   - `RhythmGame/Add RG Tap at Cursor`
   - `RhythmGame/Add RG Hold at Cursor`
   - `RhythmGame/Add RG Flick at Cursor`
   - `RhythmGame/Add RG Slide at Cursor`
5. Expected:
   - A clip named like `RG_Tap L2` is created on that lane at the time where the context menu was opened.
   - Hold/Slide default to `1s` duration and can be resized in Timeline.
   - Exporting the Timeline writes the note to JSON.

### Add Note With Keyboard

1. Set up Timeline audio preview so the Timeline is opened through a `PlayableDirector`.
2. Select the lane track to edit.
3. Move/scrub the Timeline to the target time.
4. Press:
   - `4` for Tap
   - `5` for Hold
   - `6` for Slide
   - `7` for Flick
5. Expected:
   - A clip named like `RG_Tap L2` is created on the selected lane at the current Timeline time.
   - Hold/Slide default to `1s` duration and can be resized in Timeline.

### Timeline Audio Preview

1. Open or select the target `RhythmTimelineAsset`.
2. Run `Tools/RhythmGame/Timeline/Setup Audio Preview For Open Timeline`.
3. Expected:
   - Scene gets an object named `===Timeline Audio Preview===`.
   - It has `PlayableDirector` and `AudioSource`.
   - The Timeline window is reopened/locked through that director.
   - Timeline preview/play controls should become usable for audio preview without Play Mode.
4. If the Timeline still does not play audio:
   - Confirm the Timeline has a `Music`/`AudioTrack` with an AudioClip.
   - Confirm `===Timeline Audio Preview===` exists and its `PlayableDirector.playableAsset` points to the Timeline.
   - Confirm the AudioTrack binding on the director points to its `AudioSource`.

## Known Notes

- Timeline currently uses Dypsloom OSU tap note prefab only as editor fallback. Runtime gameplay still uses the project's own `ChartNoteSpawner`.
- `NoteDefinition` colors are set per note type to distinguish Timeline clips.
- If an old Timeline was created before `m_NotePrefab` was fixed, create a new Timeline asset instead of reusing the broken one.
- Prefer Timeline context-menu actions for normal editing. Use `Timeline Note Tools` only as a fallback for typing an exact numeric time.
- The Dypsloom `Select Note` dropdown still shows demo package assets and is easy to confuse with the project's note types.
- Current console warning unrelated to this feature may come from `_Debug/TestComboLogger.cs`.

## Next Recommended Steps

1. Verify direct Timeline shortcut add note with keys `4/5/6/7`.
2. Improve Timeline note clip visuals with custom center textures/icons per note type.
3. Add a richer custom inspector/editor window for editing existing note metadata after a clip is selected.
4. Update lane visuals/background from provided art.
5. Later: integrate direct Dypsloom Timeline runtime only after JSON-runtime workflow is stable.
