using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Runtime prompt shown when the player tries to play a locked song.</summary>
public static class LockedSongShopPrompt
{
    private const string RootName = "RG Locked Song Prompt";

    public static void Show()
    {
        Canvas canvas = SongListManager.Instance != null
            ? SongListManager.Instance.GetComponentInParent<Canvas>()
            : RuntimeCanvasUtility.FindSceneCanvas();
        if (canvas == null)
            return;

        Transform existing = canvas.transform.Find(RootName);
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            existing.SetAsLastSibling();
            return;
        }

        GameObject root = CreateUiObject(RootName, canvas.transform, typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect);
        Image dimmer = root.GetComponent<Image>();
        dimmer.color = new Color(0f, 0f, 0f, 0.68f);

        GameObject panel = CreateUiObject("Dialog", root.transform, typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(480f, 230f);
        panelRect.anchoredPosition = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.10f, 0.07f, 0.18f, 0.98f);

        CreateText("SONG LOCKED", panel.transform, 30f, FontStyles.Bold, new Color(1f, 0.36f, 0.40f, 1f), new Vector2(0f, 62f), new Vector2(420f, 42f));
        CreateText("This song is locked. Would you like to visit the shop?", panel.transform, 18f, FontStyles.Normal, Color.white, new Vector2(0f, 5f), new Vector2(420f, 52f));

        Button noButton = CreateButton("No", panel.transform, "NO", new Vector2(-105f, -72f), new Color(0.22f, 0.20f, 0.30f, 1f));
        noButton.onClick.AddListener(() => root.SetActive(false));

        Button yesButton = CreateButton("Yes", panel.transform, "YES", new Vector2(105f, -72f), new Color(0.18f, 0.48f, 0.70f, 1f));
        yesButton.onClick.AddListener(() => SceneLoadUtility.LoadSceneByName("StoreMenu"));
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 position, Color color)
    {
        GameObject buttonObject = CreateUiObject(name, parent, typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(170f, 48f);
        rect.anchoredPosition = position;
        buttonObject.GetComponent<Image>().color = color;
        CreateText(label, buttonObject.transform, 18f, FontStyles.Bold, Color.white, Vector2.zero, Vector2.zero);
        return buttonObject.GetComponent<Button>();
    }

    private static TextMeshProUGUI CreateText(string text, Transform parent, float size, FontStyles style, Color color, Vector2 position, Vector2 sizeDelta)
    {
        GameObject textObject = CreateUiObject("Text", parent, typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = sizeDelta;
        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        return label;
    }

    private static GameObject CreateUiObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject gameObject = new GameObject(name, components);
        gameObject.GetComponent<RectTransform>().SetParent(parent, false);
        return gameObject;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
