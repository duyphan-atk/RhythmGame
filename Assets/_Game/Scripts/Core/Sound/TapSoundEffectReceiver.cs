using UnityEngine;

/// <summary>Runtime listener for successful notes. It is created with the gameplay UI bootstrap.</summary>
public class TapSoundEffectReceiver : MonoBehaviour
{
    private NoteManager noteManager;

    private void OnEnable()
    {
        noteManager = FindFirstObjectByType<NoteManager>();
        if (noteManager != null)
            noteManager.OnNoteJudgedEvent += HandleNoteJudged;
    }

    private void OnDisable()
    {
        if (noteManager != null)
            noteManager.OnNoteJudgedEvent -= HandleNoteJudged;
    }

    private void HandleNoteJudged(NoteBase note, HitJudgment judgment, float deltaMs)
    {
        if (note == null || judgment == HitJudgment.None || judgment == HitJudgment.Miss)
            return;

        GameplaySfxPlayer.PlayTapSound();
    }
}
