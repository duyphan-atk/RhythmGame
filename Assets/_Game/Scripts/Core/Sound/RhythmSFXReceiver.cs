using UnityEngine;
using UnityEngine.InputSystem;

public class RhythmSFXReceiver :
	MonoBehaviour,
	INoteResultReceiver
{
	public void OnNoteFinished(
		NoteBase note,
		NoteResult result
	)
	{
		if (note == null || result != NoteResult.Completed || note.LastJudgment == HitJudgment.Miss)
			return;

		GameplaySfxPlayer.PlayTapSound();
	}
}
