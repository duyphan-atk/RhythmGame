public struct NoteRuntimeData
{
    public int noteId;
    public int laneIndex;
    public NoteType noteType;
    public NoteVisualConfig visualConfig;

    public float hitTime;
    public float duration;

    public float anchoredX;
    public float hitlineY;
    public float scrollSpeed;

    public float touchRadius;

    public FlickDirection flickDirection;
    public int[] slidePath;
    public float laneSpacing;
}
