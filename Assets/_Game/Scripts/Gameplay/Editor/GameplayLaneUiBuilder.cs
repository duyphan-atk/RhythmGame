#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class GameplayLaneUiBuilder
{
    private const string RootName = "RG Lane UI";
    private const string LaneFolder = "Assets/_Game/Sprites/GamePlay/Lane";
    private const int LaneCount = 4;
    private const float LaneSpacing = 160f;
    private const float StageHeight = 770f;
    private const float LightHeight = 640f;
    private const float HitlineY = -330f;

    [MenuItem("Tools/RhythmGame/Gameplay/Rebuild Lane UI In Open Scene")]
    public static void RebuildLaneUiInOpenScene()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("GameplayLaneUiBuilder: No Canvas found in the open scene.");
            return;
        }

        ConfigureCanvasScaler(canvas);

        Transform existing = canvas.transform.Find(RootName);
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        RectTransform root = CreateRect(canvas.transform, RootName);
        root.SetAsFirstSibling();
        Stretch(root);
        GameplayLaneLayout layout = root.gameObject.AddComponent<GameplayLaneLayout>();

        Image shade = CreateImage(root, "Stage Shade", null);
        RectTransform shadeRect = shade.rectTransform;
        Center(shadeRect, new Vector2(760f, StageHeight), new Vector2(0f, 0f));
        shade.color = new Color(0.02f, 0.015f, 0.06f, 0.62f);
        shade.raycastTarget = false;

        Sprite left = LoadSprite("mania-stage-left.png");
        Sprite right = LoadSprite("mania-stage-right.png");
        Sprite light = LoadSprite("mania-stage-light.png");
        Sprite bottom = LoadSprite("mania-stage-bottom.png");
        Sprite hint = LoadSprite("mania-stage-hint.png");

        CreateStageSide(root, "Stage Left", left, -404f);
        CreateStageSide(root, "Stage Right", right, 404f);
        RectTransform stageLeft = root.Find("Stage Left") as RectTransform;
        RectTransform stageRight = root.Find("Stage Right") as RectTransform;

        float firstLaneX = -((LaneCount - 1) * LaneSpacing) * 0.5f;
        RectTransform[] laneLights = new RectTransform[LaneCount];
        RectTransform[] laneBottoms = new RectTransform[LaneCount];

        for (int i = 0; i < LaneCount; i++)
        {
            float laneX = firstLaneX + i * LaneSpacing;

            Image laneLight = CreateImage(root, $"Lane {i} Light", light);
            Center(laneLight.rectTransform, new Vector2(64f, LightHeight), new Vector2(laneX, 10f));
            laneLight.color = i % 2 == 0
                ? new Color(0.95f, 0.82f, 1f, 0.22f)
                : new Color(0.5f, 0.9f, 1f, 0.18f);
            laneLight.raycastTarget = false;
            laneLights[i] = laneLight.rectTransform;

            Image laneBottom = CreateImage(root, $"Lane {i} Hit Bottom", bottom);
            Center(laneBottom.rectTransform, new Vector2(LaneSpacing, 29f), new Vector2(laneX, HitlineY));
            laneBottom.color = Color.white;
            laneBottom.raycastTarget = false;
            laneBottoms[i] = laneBottom.rectTransform;
        }

        Image hitHint = CreateImage(root, "Hit Hint", hint);
        Center(hitHint.rectTransform, new Vector2(LaneSpacing * LaneCount, 25f), new Vector2(0f, HitlineY + 18f));
        hitHint.color = new Color(1f, 0.8f, 1f, 0.88f);
        hitHint.raycastTarget = false;

        layout.Configure(
            canvas,
            Object.FindFirstObjectByType<ChartNoteSpawner>(),
            Object.FindFirstObjectByType<NoteManager>(),
            shadeRect,
            stageLeft,
            stageRight,
            hitHint.rectTransform,
            laneLights,
            laneBottoms);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("GameplayLaneUiBuilder: Rebuilt RG Lane UI in the open scene.");
    }

    private static void ConfigureCanvasScaler(Canvas canvas)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static void CreateStageSide(RectTransform root, string name, Sprite sprite, float x)
    {
        Image side = CreateImage(root, name, sprite);
        Center(side.rectTransform, new Vector2(167f, StageHeight), new Vector2(x, 0f));
        side.color = Color.white;
        side.raycastTarget = false;
    }

    private static Image CreateImage(RectTransform parent, string name, Sprite sprite)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.layer = parent.gameObject.layer;
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        return image;
    }

    private static RectTransform CreateRect(Transform parent, string name)
    {
        GameObject rectObject = new GameObject(name, typeof(RectTransform));
        rectObject.layer = parent.gameObject.layer;
        rectObject.transform.SetParent(parent, false);
        return rectObject.GetComponent<RectTransform>();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }

    private static void Center(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static Sprite LoadSprite(string fileName)
    {
        string path = $"{LaneFolder}/{fileName}";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite nestedSprite)
                {
                    sprite = nestedSprite;
                    break;
                }
            }
        }

        if (sprite == null)
            Debug.LogWarning($"GameplayLaneUiBuilder: Missing lane sprite '{path}'.");

        return sprite;
    }
}
#endif
