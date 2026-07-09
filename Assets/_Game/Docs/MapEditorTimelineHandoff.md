# Map Editor Timeline Handoff

Last updated: 2026-07-09

## Current Goal

Build a rhythm-game map editor workflow that starts from the existing JSON chart/runtime spawner, then bridges into Dypsloom Rhythm Timeline for timeline-based editing.

## What Has Been Implemented

- Added `NoteVisualConfig`:
  - Path: `Assets/_Game/Data/NoteVisuals/DefaultNoteVisualConfig.asset`
  - Controls sprite, tint, runtime UI size, and preview scale for `Tap`, `Hold`, `Flick`, and `Slide`.
- Updated preview chart rendering:
  - `ChartVisualizer` applies `NoteVisualConfig`.
- Updated runtime note visuals:
  - `NoteBase` applies `NoteVisualConfig`.
  - `ChartNoteSpawner` passes `NoteType` and visual config into runtime notes.
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
   - Runtime spawns notes from the exported chart.

## Known Notes

- Timeline currently uses Dypsloom OSU tap note prefab only as editor fallback. Runtime gameplay still uses the project's own `ChartNoteSpawner`.
- `NoteDefinition` colors are set per note type to distinguish Timeline clips.
- If an old Timeline was created before `m_NotePrefab` was fixed, create a new Timeline asset instead of reusing the broken one.
- Current console warning unrelated to this feature may come from `_Debug/TestComboLogger.cs`.

## Next Recommended Steps

1. Verify `Export Timeline And Prepare Runtime` with edited clip positions and durations.
2. Improve Timeline note clip visuals with custom center textures/icons per note type.
3. Add robust export support for manually changed note type, flick direction, and slide path from Timeline inspector fields.
4. Add a dedicated Timeline editor window or custom inspector for editing note type metadata cleanly.
5. Later: integrate direct Dypsloom Timeline runtime only after JSON-runtime workflow is stable.
