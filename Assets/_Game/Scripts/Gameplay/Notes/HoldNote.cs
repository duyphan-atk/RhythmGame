using UnityEngine;
using UnityEngine.UI;

public class HoldNote : NoteBase
{
    [Header("Hold Visual")]
    [SerializeField] private Image holdFillImage;
    [SerializeField] private RectTransform holdFillRect;

    private readonly HoldNoteStateMachine stateMachine = new HoldNoteStateMachine();

    private bool visualCreated;
    private bool judgmentEffectShown;

    protected override void Awake()
    {
        base.Awake();
        noteType = NoteType.Hold;
    }

    public override void Initialize(NoteRuntimeData data)
    {
        base.Initialize(data);

        stateMachine.Reset();
        judgmentEffectShown = false;

        ApplyDurationVisual(data.scrollSpeed);
        CreateHoldVisualIfNeeded();
        SetHoldProgress(0f);
        SetHoldFillColor(Color.yellow);

        SetColor(Color.white);
    }

    public override void OnPointerBegin(NotePointer pointer)
    {
        float currentTime = owner != null ? owner.CurrentTime : 0f;

        stateMachine.StartHold(
            pointer.fingerId,
            hitTime,
            duration,
            currentTime
        );

        SetColor(Color.yellow);
        SetHoldFillColor(Color.yellow);
        SetHoldProgress(stateMachine.Progress01);
    }

    public override void Tick(float currentTime)
    {
        base.Tick(currentTime);

        if (IsFinished)
            return;

        stateMachine.Tick(currentTime);

        if (stateMachine.IsHolding())
        {
            SetColor(Color.yellow);
            SetHoldFillColor(Color.yellow);
            SetHoldProgress(stateMachine.Progress01);
            return;
        }

        if (stateMachine.IsCompleted())
        {
            CompleteHold();
        }
    }

    public override void OnPointerEnd(NotePointer pointer)
    {
        if (IsFinished)
            return;

        float currentTime = owner != null ? owner.CurrentTime : 0f;

        stateMachine.Release(pointer.fingerId, currentTime);

        if (stateMachine.IsReleasedEarly())
        {
            SetHoldFillColor(Color.red);
            SetColor(Color.red);

            Fail(NoteResult.ReleasedEarly);
            return;
        }

        if (stateMachine.IsCompleted())
        {
            CompleteHold();
        }
    }

    public override float GetAutoMissTime(float missAfterHitTime)
    {
        // Nếu người chơi không chạm đầu Hold lúc nó tới hitline,
        // nó miss sớm giống Tap Note.
        return hitTime + missAfterHitTime;
    }

    public override Vector3 GetHitEffectWorldPosition()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        float holdHeight = rectTransform.rect.height;

        // Hold Note đang dùng pivot ở đáy:
        // đáy = đầu Hold
        // đỉnh = đuôi Hold
        // nên effect lấy vị trí đuôi Hold.
        return rectTransform.TransformPoint(new Vector3(0f, holdHeight, 0f));
    }

    private void CompleteHold()
    {
        if (IsFinished)
            return;

        SetHoldProgress(1f);
        SetHoldFillColor(Color.green);
        SetColor(Color.green);

        ShowJudgmentEffectOnce();

        Complete(NoteResult.Completed);
    }

    private void ShowJudgmentEffectOnce()
    {
        if (judgmentEffectShown)
            return;

        judgmentEffectShown = true;

        owner?.NotifyJudgmentEffect(this);
    }

    private void CreateHoldVisualIfNeeded()
    {
        if (visualCreated)
            return;

        visualCreated = true;

        GameObject fillObject = new GameObject(
            "HOLD_PROGRESS_FILL",
            typeof(RectTransform),
            typeof(Image)
        );

        fillObject.transform.SetParent(transform, false);

        holdFillRect = fillObject.GetComponent<RectTransform>();

        holdFillRect.anchorMin = new Vector2(0.5f, 0f);
        holdFillRect.anchorMax = new Vector2(0.5f, 0f);
        holdFillRect.pivot = new Vector2(0.5f, 0f);

        float fillWidth = rectTransform != null && rectTransform.sizeDelta.x > 0f
            ? rectTransform.sizeDelta.x * 0.75f
            : 70f;

        holdFillRect.sizeDelta = new Vector2(fillWidth, 0f);
        holdFillRect.anchoredPosition = Vector2.zero;

        holdFillImage = fillObject.GetComponent<Image>();
        if (noteImage != null)
            holdFillImage.sprite = noteImage.sprite;

        holdFillImage.type = Image.Type.Simple;
        holdFillImage.preserveAspect = false;
        holdFillImage.color = Color.yellow;
        holdFillImage.raycastTarget = false;
    }

    private void ApplyDurationVisual(float scrollSpeed)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        float width = rectTransform.sizeDelta.x;
        float baseHeight = rectTransform.sizeDelta.y;
        float durationHeight = Mathf.Max(0f, duration) * Mathf.Max(0f, scrollSpeed);

        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.sizeDelta = new Vector2(width, baseHeight + durationHeight);
    }

    private void SetHoldProgress(float progress01)
    {
        if (holdFillRect == null)
            return;

        progress01 = Mathf.Clamp01(progress01);

        float maxHeight = rectTransform.rect.height;

        Vector2 size = holdFillRect.sizeDelta;
        size.y = maxHeight * progress01;
        holdFillRect.sizeDelta = size;
    }

    private void SetHoldFillColor(Color color)
    {
        if (holdFillImage != null)
            holdFillImage.color = color;
    }
}
