using UnityEngine;
using UnityEngine.UI;

public class SlideNote : NoteBase
{
    [Header("Slide")]
    [SerializeField] private SlideCheckpointSystem checkpointSystem;
    [SerializeField] private float checkpointStepY = 86f;
    [SerializeField] private float checkpointSize = 34f;

    protected override void Awake()
    {
        base.Awake();
        noteType = NoteType.Slide;

        if (checkpointSystem == null)
            checkpointSystem = GetComponent<SlideCheckpointSystem>();
    }

    public override void Initialize(NoteRuntimeData data)
    {
        base.Initialize(data);

        if (checkpointSystem == null)
            checkpointSystem = GetComponent<SlideCheckpointSystem>();

        RebuildCheckpoints(data);
    }

    public override void OnPointerBegin(NotePointer pointer)
    {
        if (checkpointSystem == null)
        {
            Fail(NoteResult.Failed);
            return;
        }

        bool started = checkpointSystem.Begin(pointer.position);

        if (!started)
        {
            Fail(NoteResult.Failed);
            return;
        }

        owner?.NotifyJudgmentEffect(this);

        SetColor(Color.yellow);
    }

    public override void OnPointerMove(NotePointer pointer)
    {
        if (checkpointSystem == null)
            return;

        checkpointSystem.Move(pointer.position);

        if (checkpointSystem.IsCompleted())
        {
            Complete(NoteResult.Completed);
        }
    }

    public override void OnPointerEnd(NotePointer pointer)
    {
        if (IsFinished)
            return;

        if (checkpointSystem == null)
        {
            Fail(NoteResult.Failed);
            return;
        }

        if (!checkpointSystem.IsCompleted())
        {
            checkpointSystem.Cancel();
            Fail(NoteResult.Failed);
        }
    }

    public override float GetAutoMissTime(float missAfterHitTime)
    {
        return hitTime + duration + missAfterHitTime;
    }

    private void RebuildCheckpoints(NoteRuntimeData data)
    {
        if (checkpointSystem == null)
            return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("SLIDE_CHECKPOINT"))
                Destroy(child.gameObject);
        }

        int[] path = data.slidePath;
        if (path == null || path.Length == 0)
            path = new[] { data.laneIndex };

        var checkpoints = new System.Collections.Generic.List<RectTransform>();

        for (int i = 0; i < path.Length; i++)
        {
            GameObject checkpointObject = new GameObject(
                $"SLIDE_CHECKPOINT_{i}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            checkpointObject.transform.SetParent(transform, false);

            RectTransform checkpointRect = checkpointObject.GetComponent<RectTransform>();
            checkpointRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkpointRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkpointRect.pivot = new Vector2(0.5f, 0.5f);
            checkpointRect.sizeDelta = new Vector2(checkpointSize, checkpointSize);

            float laneOffset = (path[i] - data.laneIndex) * data.laneSpacing;
            checkpointRect.anchoredPosition = new Vector2(laneOffset, checkpointStepY * i);

            Image checkpointImage = checkpointObject.GetComponent<Image>();
            checkpointImage.raycastTarget = false;

            if (noteImage != null)
                checkpointImage.sprite = noteImage.sprite;

            checkpointImage.color = new Color(1f, 1f, 1f, 0.82f);
            checkpoints.Add(checkpointRect);
        }

        checkpointSystem.Initialize(checkpoints, touchRadius);
    }
}
