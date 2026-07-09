using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NoteVisualConfig",
    menuName = "Rhythm Game/Chart/Note Visual Config")]
public class NoteVisualConfig : ScriptableObject
{
    [SerializeField] private NoteVisualStyle fallback = NoteVisualStyle.Default(NoteType.Tap);
    [SerializeField] private NoteVisualStyle[] styles =
    {
        NoteVisualStyle.Default(NoteType.Tap),
        NoteVisualStyle.Default(NoteType.Hold),
        NoteVisualStyle.Default(NoteType.Flick),
        NoteVisualStyle.Default(NoteType.Slide)
    };

    public NoteVisualStyle GetStyle(NoteType noteType)
    {
        if (styles != null)
        {
            for (int i = 0; i < styles.Length; i++)
            {
                if (styles[i].noteType == noteType)
                    return styles[i];
            }
        }

        return fallback;
    }
}

[Serializable]
public struct NoteVisualStyle
{
    public NoteType noteType;
    public Sprite sprite;
    public Color color;

    [Header("Runtime Canvas")]
    public Vector2 uiSize;
    public bool preserveAspect;

    [Header("World Preview")]
    public Vector3 previewScale;

    public static NoteVisualStyle Default(NoteType noteType)
    {
        return new NoteVisualStyle
        {
            noteType = noteType,
            color = Color.white,
            uiSize = new Vector2(100f, 100f),
            preserveAspect = true,
            previewScale = new Vector3(0.8f, 0.8f, 1f)
        };
    }
}
