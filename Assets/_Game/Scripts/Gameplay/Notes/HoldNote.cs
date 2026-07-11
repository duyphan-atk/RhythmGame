using UnityEngine;
using UnityEngine.UI;

public class HoldNote : NoteBase
{
    [Header("Hold Visual")]
    [SerializeField] private Image holdFillImage;
    [SerializeField] private RectTransform holdFillRect;
    [SerializeField, Min(0.03f)] private float sustainEffectInterval = 0.12f;

    private readonly HoldNoteStateMachine stateMachine = new HoldNoteStateMachine();

    private bool visualCreated;
    private bool judgmentEffectShown;
    private float baseVisualWidth;
    private float baseVisualHeight;
    private float runtimeHitlineY;
    private float runtimeScrollSpeed;
    private float nextSustainEffectTime;

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
        runtimeHitlineY = data.hitlineY;
        runtimeScrollSpeed = data.scrollSpeed;
        nextSustainEffectTime = 0f;

        CacheBaseVisualSize();
        ApplyDurationVisual(data.scrollSpeed);
        CreateHoldVisualIfNeeded();
        SetHoldProgress(0f);
        SetHoldFillColor(Color.yellow);

        SetColor(Color.white);
    }

    public override void ApplyScrollSpeed(float newScrollSpeed)
    {
        runtimeScrollSpeed = newScrollSpeed;
        base.ApplyScrollSpeed(newScrollSpeed);

        if (stateMachine.IsHolding())
            ApplyRemainingDurationVisual(owner != null ? owner.CurrentTime : hitTime);
        else
            ApplyDurationVisual(newScrollSpeed);

        SetHoldProgress(stateMachine.IsHolding() ? 1f : stateMachine.Progress01);
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
        nextSustainEffectTime = currentTime;

        if (movement != null)
            movement.LockY(runtimeHitlineY);

        ApplyRemainingDurationVisual(currentTime);
        SetHoldProgress(1f);
        owner?.NotifySustainEffect(this);
    }

    public override void Tick(float currentTime)
    {
        base.Tick(currentTime);

        if (IsFinished)
            return;

        stateMachine.Tick(currentTime);

        if (stateMachine.IsHolding())
        {
            if (movement != null)
                movement.LockY(runtimeHitlineY);

            SetColor(Color.yellow);
            SetHoldFillColor(Color.yellow);
            ApplyRemainingDurationVisual(currentTime);
            SetHoldProgress(1f);
            NotifySustainEffectIfDue(currentTime);
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

            if (movement != null)
                movement.UnlockY();

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

        if (movement != null)
            movement.UnlockY();

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

        if (baseVisualWidth <= 0f || baseVisualHeight <= 0f)
            CacheBaseVisualSize();

        float durationHeight = Mathf.Max(0f, duration) * Mathf.Max(0f, scrollSpeed);

        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.sizeDelta = new Vector2(baseVisualWidth, baseVisualHeight + durationHeight);
    }

    private void ApplyRemainingDurationVisual(float currentTime)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        if (baseVisualWidth <= 0f || baseVisualHeight <= 0f)
            CacheBaseVisualSize();

        float tailTime = hitTime + Mathf.Max(0f, duration);
        float remainingSeconds = Mathf.Max(0f, tailTime - currentTime);
        float remainingHeight = remainingSeconds * Mathf.Max(0f, runtimeScrollSpeed);

        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, runtimeHitlineY);
        rectTransform.sizeDelta = new Vector2(baseVisualWidth, baseVisualHeight + remainingHeight);
    }

    private void CacheBaseVisualSize()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        baseVisualWidth = rectTransform.sizeDelta.x;
        baseVisualHeight = rectTransform.sizeDelta.y;
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

    private void NotifySustainEffectIfDue(float currentTime)
    {
        if (owner == null || currentTime < nextSustainEffectTime)
            return;

        nextSustainEffectTime = currentTime + sustainEffectInterval;
        owner.NotifySustainEffect(this);
    }
}
