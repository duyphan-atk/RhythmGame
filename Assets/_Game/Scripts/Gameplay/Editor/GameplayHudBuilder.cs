#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class GameplayHudBuilder
{
    private const string HudRootName = "RG Gameplay HUD";
    private const string ComboPrefabPath = "Assets/_Game/Prefabs/Core/UI/ComboDisplay_Root.prefab";
    private const string ComboConfigPath = "Assets/_Game/Data/Combo/ComboConfig.asset";
    private const string PerfectEffectFolder = "Assets/_Game/Sprites/GamePlay/PerfectEF";
    private const string HitEffectFolder = "Assets/_Game/Sprites/GamePlay/HitEf";

    [MenuItem("Tools/RhythmGame/Gameplay/Rebuild Gameplay HUD In Open Scene")]
    public static void RebuildGameplayHudInOpenScene()
    {
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        NoteManager noteManager = UnityEngine.Object.FindFirstObjectByType<NoteManager>();
        GameplayLaneLayout laneLayout = UnityEngine.Object.FindFirstObjectByType<GameplayLaneLayout>();

        if (canvas == null)
        {
            Debug.LogError("GameplayHudBuilder: No Canvas found in the open scene.");
            return;
        }

        if (noteManager == null)
        {
            Debug.LogError("GameplayHudBuilder: No NoteManager found in the open scene.");
            return;
        }

        if (laneLayout == null)
            Debug.LogWarning("GameplayHudBuilder: No GameplayLaneLayout found. Lane flash will fall back to center lane.");

        Transform existing = canvas.transform.Find(HudRootName);
        if (existing != null)
            UnityEngine.Object.DestroyImmediate(existing.gameObject);

        RectTransform root = CreateRect(canvas.transform, HudRootName);
        Stretch(root);
        root.SetAsLastSibling();

        ComboManager comboManager = CreateComboManager(root, noteManager);
        CreateComboDisplay(root, comboManager);
        CreateJudgmentReceiver(root, canvas, noteManager, laneLayout);
        CreateDebugOverlay(root, laneLayout);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("GameplayHudBuilder: Rebuilt RG Gameplay HUD in the open scene.");
    }

    private static ComboManager CreateComboManager(RectTransform root, NoteManager noteManager)
    {
        GameObject managerObject = new GameObject("Combo Manager", typeof(RectTransform), typeof(ComboManager));
        managerObject.layer = root.gameObject.layer;
        managerObject.transform.SetParent(root, false);

        RectTransform rect = managerObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.sizeDelta = Vector2.zero;

        ComboManager comboManager = managerObject.GetComponent<ComboManager>();
        SerializedObject serializedManager = new SerializedObject(comboManager);
        serializedManager.FindProperty("_noteManager").objectReferenceValue = noteManager;
        serializedManager.FindProperty("_config").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<ComboConfig>(ComboConfigPath);
        serializedManager.ApplyModifiedPropertiesWithoutUndo();

        return comboManager;
    }

    private static void CreateComboDisplay(RectTransform root, ComboManager comboManager)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ComboPrefabPath);
        GameObject displayObject;

        if (prefab != null)
        {
            displayObject = PrefabUtility.InstantiatePrefab(prefab, root) as GameObject;
        }
        else
        {
            displayObject = CreateFallbackComboDisplay(root);
        }

        if (displayObject == null)
            return;

        displayObject.name = "Combo Display";

        RectTransform rect = displayObject.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 130f);
            rect.sizeDelta = new Vector2(260f, 120f);
        }

        ComboDisplay display = displayObject.GetComponent<ComboDisplay>();
        if (display == null)
            return;

        SerializedObject serializedDisplay = new SerializedObject(display);
        serializedDisplay.FindProperty("_comboManager").objectReferenceValue = comboManager;
        serializedDisplay.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateFallbackComboDisplay(RectTransform root)
    {
        GameObject displayObject = new GameObject(
            "Combo Display",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(ComboDisplay));
        displayObject.layer = root.gameObject.layer;
        displayObject.transform.SetParent(root, false);

        TextMeshProUGUI label = CreateText(displayObject.transform, "Combo Label", "COMBO", 34f);
        label.rectTransform.anchoredPosition = new Vector2(0f, 28f);

        TextMeshProUGUI number = CreateText(displayObject.transform, "Combo Number", "0", 72f);
        number.rectTransform.anchoredPosition = new Vector2(0f, -28f);

        ComboDisplay display = displayObject.GetComponent<ComboDisplay>();
        SerializedObject serializedDisplay = new SerializedObject(display);
        serializedDisplay.FindProperty("_labelText").objectReferenceValue = label;
        serializedDisplay.FindProperty("_numberText").objectReferenceValue = number;
        serializedDisplay.FindProperty("_canvasGroup").objectReferenceValue =
            displayObject.GetComponent<CanvasGroup>();
        serializedDisplay.ApplyModifiedPropertiesWithoutUndo();

        return displayObject;
    }

    private static void CreateJudgmentReceiver(
        RectTransform root,
        Canvas canvas,
        NoteManager noteManager,
        GameplayLaneLayout laneLayout)
    {
        GameObject receiverObject = new GameObject(
            "Judgment Effect Receiver",
            typeof(RectTransform),
            typeof(HitEffectSpriteReceiver));
        receiverObject.layer = root.gameObject.layer;
        receiverObject.transform.SetParent(root, false);

        RectTransform rect = receiverObject.GetComponent<RectTransform>();
        Stretch(rect);

        HitEffectSpriteReceiver receiver = receiverObject.GetComponent<HitEffectSpriteReceiver>();
        SerializedObject serializedReceiver = new SerializedObject(receiver);
        serializedReceiver.FindProperty("targetCanvas").objectReferenceValue = canvas;
        serializedReceiver.FindProperty("noteManager").objectReferenceValue = noteManager;
        serializedReceiver.FindProperty("laneLayout").objectReferenceValue = laneLayout;
        serializedReceiver.FindProperty("perfectSprite").objectReferenceValue = LoadSprite($"{PerfectEffectFolder}/mania-hit300.png");
        serializedReceiver.FindProperty("greatSprite").objectReferenceValue = LoadSprite($"{PerfectEffectFolder}/mania-hit200.png");
        serializedReceiver.FindProperty("goodSprite").objectReferenceValue = LoadSprite($"{PerfectEffectFolder}/mania-hit100.png");
        serializedReceiver.FindProperty("missSprite").objectReferenceValue = LoadSprite($"{PerfectEffectFolder}/mania-hit0.png");
        serializedReceiver.FindProperty("centerOffset").vector2Value = new Vector2(0f, 76f);
        serializedReceiver.FindProperty("effectSize").vector2Value = new Vector2(180f, 64f);
        serializedReceiver.FindProperty("laneFlashSprites").arraySize = 0;

        Sprite[] flashSprites = LoadNumberedSprites(HitEffectFolder, "lightingN-*.png");
        SerializedProperty flashProperty = serializedReceiver.FindProperty("laneFlashSprites");
        flashProperty.arraySize = flashSprites.Length;
        for (int i = 0; i < flashSprites.Length; i++)
        {
            flashProperty.GetArrayElementAtIndex(i).objectReferenceValue = flashSprites[i];
        }

        serializedReceiver.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateDebugOverlay(RectTransform root, GameplayLaneLayout laneLayout)
    {
        TextMeshProUGUI debugText = CreateText(root, "Lane Debug Overlay", string.Empty, 22f);
        debugText.alignment = TextAlignmentOptions.TopLeft;
        debugText.color = new Color(0.72f, 0.95f, 1f, 0.9f);
        debugText.raycastTarget = false;

        RectTransform rect = debugText.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(18f, -18f);
        rect.sizeDelta = new Vector2(280f, 130f);

        GameplayLaneDebugOverlay overlay = debugText.gameObject.AddComponent<GameplayLaneDebugOverlay>();
        SerializedObject serializedOverlay = new SerializedObject(overlay);
        serializedOverlay.FindProperty("laneLayout").objectReferenceValue = laneLayout;
        serializedOverlay.FindProperty("outputText").objectReferenceValue = debugText;
        serializedOverlay.FindProperty("showOverlay").boolValue = false;
        serializedOverlay.ApplyModifiedPropertiesWithoutUndo();
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.layer = parent.gameObject.layer;
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(260f, 70f);

        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.raycastTarget = false;

        return textComponent;
    }

    private static Sprite LoadSprite(string path)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        Sprite largestSprite = null;
        float largestArea = 0f;

        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is not Sprite nestedSprite)
                continue;

            float area = nestedSprite.rect.width * nestedSprite.rect.height;
            if (largestSprite == null || area > largestArea)
            {
                largestSprite = nestedSprite;
                largestArea = area;
            }
        }

        if (largestSprite != null)
            return largestSprite;

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
            return sprite;

        Debug.LogWarning($"GameplayHudBuilder: Missing sprite '{path}'.");
        return null;
    }

    private static Sprite[] LoadNumberedSprites(string folder, string searchPattern)
    {
        if (!Directory.Exists(folder))
            return Array.Empty<Sprite>();

        List<string> paths = new List<string>(Directory.GetFiles(folder, searchPattern, SearchOption.TopDirectoryOnly));
        paths.RemoveAll(path => path.Contains("@2x") || path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase));
        paths.Sort(CompareNumberedPaths);

        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < paths.Count; i++)
        {
            string assetPath = paths[i].Replace("\\", "/");
            Sprite sprite = LoadSprite(assetPath);
            if (sprite != null)
                sprites.Add(sprite);
        }

        return sprites.ToArray();
    }

    private static int CompareNumberedPaths(string a, string b)
    {
        return ExtractTrailingNumber(a).CompareTo(ExtractTrailingNumber(b));
    }

    private static int ExtractTrailingNumber(string path)
    {
        Match match = Regex.Match(Path.GetFileNameWithoutExtension(path), @"(\d+)$");
        return match.Success ? int.Parse(match.Value) : 0;
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
}
#endif
