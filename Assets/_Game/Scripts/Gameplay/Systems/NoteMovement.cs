using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class NoteMovement : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private float hitTime;
    [SerializeField] private float hitlineY;
    [SerializeField] private float scrollSpeed = 600f;

    private RectTransform rectTransform;
    private bool initialized;
    private bool lockY;
    private float lockedY;

    public float HitTime => hitTime;
    public float HitlineY => hitlineY;
    public float ScrollSpeed => scrollSpeed;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(NoteRuntimeData data)
    {
        hitTime = data.hitTime;
        hitlineY = data.hitlineY;
        scrollSpeed = data.scrollSpeed;

        Vector2 pos = rectTransform.anchoredPosition;
        pos.x = data.anchoredX;
        rectTransform.anchoredPosition = pos;

        lockY = false;
        initialized = true;
    }

    public void Tick(float currentTime)
    {
        if (!initialized)
            return;

        float y = lockY ? lockedY : hitlineY + (hitTime - currentTime) * scrollSpeed;

        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = y;
        rectTransform.anchoredPosition = pos;
    }

    public void ApplyScrollSpeed(float newScrollSpeed)
    {
        scrollSpeed = Mathf.Max(0f, newScrollSpeed);
    }

    public void LockY(float y)
    {
        lockY = true;
        lockedY = y;
    }

    public void UnlockY()
    {
        lockY = false;
    }
}
