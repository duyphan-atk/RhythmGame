using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class GameplayLaneLayout : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private ChartNoteSpawner chartNoteSpawner;
    [SerializeField] private NoteManager noteManager;

    [Header("Parts")]
    [SerializeField] private RectTransform stageShade;
    [SerializeField] private RectTransform stageLeft;
    [SerializeField] private RectTransform stageRight;
    [SerializeField] private RectTransform hitHint;
    [SerializeField] private RectTransform[] laneLights = new RectTransform[0];
    [SerializeField] private RectTransform[] laneBottoms = new RectTransform[0];

    [Header("Layout")]
    [SerializeField] private int laneCount = 4;
    [SerializeField, Range(0.2f, 0.8f)] private float laneAreaWidthRatio = 0.36f;
    [SerializeField] private float minLaneSpacing = 120f;
    [SerializeField] private float maxLaneSpacing = 210f;
    [SerializeField, Range(0.05f, 0.48f)] private float hitlineFromBottomRatio = 0.12f;
    [SerializeField] private float hitlineOffsetY = 0f;
    [SerializeField] private float hitlineJudgeDistanceRatio = 0.68f;
    [SerializeField] private float touchRadiusRatio = 0.7f;
    [SerializeField] private float verticalBleed = 96f;

    [Header("Debug")]
    [SerializeField] private float appliedLaneSpacing;
    [SerializeField] private float appliedHitlineY;

    public float AppliedLaneSpacing => appliedLaneSpacing;
    public float AppliedHitlineY => appliedHitlineY;

    private RectTransform rectTransform;
    private Vector2 lastCanvasSize;

    public void Configure(
        Canvas targetCanvas,
        ChartNoteSpawner spawner,
        NoteManager manager,
        RectTransform shade,
        RectTransform left,
        RectTransform right,
        RectTransform hint,
        RectTransform[] lights,
        RectTransform[] bottoms)
    {
        canvas = targetCanvas;
        chartNoteSpawner = spawner;
        noteManager = manager;
        stageShade = shade;
        stageLeft = left;
        stageRight = right;
        hitHint = hint;
        laneLights = lights;
        laneBottoms = bottoms;

        ApplyLayout();
    }

    private void Awake()
    {
        ResolveReferences();
        ApplyLayout();
    }

    private void OnEnable()
    {
        ResolveReferences();
        ApplyLayout();
    }

    private void Update()
    {
        if (canvas == null)
            ResolveReferences();

        Vector2 canvasSize = GetCanvasSize();
        if ((canvasSize - lastCanvasSize).sqrMagnitude > 0.5f)
            ApplyLayout();
    }

    private void OnValidate()
    {
        laneCount = Mathf.Max(1, laneCount);
        minLaneSpacing = Mathf.Max(1f, minLaneSpacing);
        maxLaneSpacing = Mathf.Max(minLaneSpacing, maxLaneSpacing);
        verticalBleed = Mathf.Max(0f, verticalBleed);

        ResolveReferences();
        ApplyLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyLayout();
    }

    public void ApplyLayout()
    {
        if (!isActiveAndEnabled)
            return;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        Vector2 canvasSize = GetCanvasSize();
        if (canvasSize.x <= 0f || canvasSize.y <= 0f)
            return;

        lastCanvasSize = canvasSize;

        Stretch(rectTransform);

        int activeLaneCount = Mathf.Max(1, laneCount);
        float laneAreaWidth = canvasSize.x * laneAreaWidthRatio;
        appliedLaneSpacing = Mathf.Clamp(
            laneAreaWidth / activeLaneCount,
            minLaneSpacing,
            maxLaneSpacing);

        appliedHitlineY = -canvasSize.y * 0.5f
            + canvasSize.y * hitlineFromBottomRatio
            + hitlineOffsetY;

        float firstLaneX = -((activeLaneCount - 1) * appliedLaneSpacing) * 0.5f;
        float laneBandWidth = activeLaneCount * appliedLaneSpacing;
        float stageHeight = canvasSize.y + verticalBleed * 2f;
        float sideWidth = Mathf.Clamp(appliedLaneSpacing * 0.86f, 110f, 180f);
        float stageWidth = laneBandWidth + sideWidth * 2f;
        float sideX = laneBandWidth * 0.5f + sideWidth * 0.5f;

        SetRect(stageShade, new Vector2(stageWidth, stageHeight), Vector2.zero);
        SetRect(stageLeft, new Vector2(sideWidth, stageHeight), new Vector2(-sideX, 0f));
        SetRect(stageRight, new Vector2(sideWidth, stageHeight), new Vector2(sideX, 0f));
        SetRect(hitHint, new Vector2(laneBandWidth, Mathf.Max(22f, appliedLaneSpacing * 0.16f)),
            new Vector2(0f, appliedHitlineY + Mathf.Max(12f, appliedLaneSpacing * 0.12f)));

        for (int i = 0; i < activeLaneCount; i++)
        {
            float laneX = firstLaneX + i * appliedLaneSpacing;

            if (laneLights != null && i < laneLights.Length)
            {
                SetRect(laneLights[i],
                    new Vector2(Mathf.Max(48f, appliedLaneSpacing * 0.4f), stageHeight),
                    new Vector2(laneX, 0f));
            }

            if (laneBottoms != null && i < laneBottoms.Length)
            {
                SetRect(laneBottoms[i],
                    new Vector2(appliedLaneSpacing, Mathf.Max(24f, appliedLaneSpacing * 0.18f)),
                    new Vector2(laneX, appliedHitlineY));
            }
        }

        float hitlineJudgeDistance = Mathf.Max(80f, appliedLaneSpacing * hitlineJudgeDistanceRatio);
        float touchRadius = Mathf.Max(80f, appliedLaneSpacing * touchRadiusRatio);

        chartNoteSpawner?.ApplyGameplayLayout(appliedLaneSpacing, appliedHitlineY, touchRadius);
        noteManager?.ApplyHitlineLayout(appliedHitlineY, hitlineJudgeDistance);
    }

    private void ResolveReferences()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (chartNoteSpawner == null)
            chartNoteSpawner = FindFirstObjectByType<ChartNoteSpawner>();

        if (noteManager == null)
            noteManager = FindFirstObjectByType<NoteManager>();
    }

    private Vector2 GetCanvasSize()
    {
        if (canvas == null)
            return Vector2.zero;

        RectTransform canvasRect = canvas.transform as RectTransform;
        return canvasRect != null ? canvasRect.rect.size : Vector2.zero;
    }

    private static void SetRect(RectTransform rect, Vector2 size, Vector2 position)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void Stretch(RectTransform rect)
    {
        if (rect == null)
            return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }
}
