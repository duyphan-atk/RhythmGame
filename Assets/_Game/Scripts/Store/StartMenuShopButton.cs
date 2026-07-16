using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(600)]
public class StartMenuShopButton : MonoBehaviour
{
    private const string ShopButtonName = "RG Shop Button";

    private void Awake()
    {
        Transform panel = transform.Find("Panel");
        if (panel == null)
            return;

        Button shopButton = FindOrCreateShopButton(panel);
        if (shopButton == null)
            return;

        shopButton.onClick = new Button.ButtonClickedEvent();
        shopButton.onClick.AddListener(OpenShop);

        StartMenuSettingsButton settingsClick = shopButton.GetComponent<StartMenuSettingsButton>();
        if (settingsClick != null)
            settingsClick.enabled = false;

        BuildShopLabel(shopButton.transform);
        ArrangeMenuButtons(panel, shopButton);
    }

    private static Button FindOrCreateShopButton(Transform panel)
    {
        Transform existing = panel.Find(ShopButtonName);
        if (existing != null)
            return existing.GetComponent<Button>();

        Button template = FindSettingsButton(panel);
        if (template == null)
            return null;

        Button clone = Instantiate(template, panel);
        clone.name = ShopButtonName;
        clone.gameObject.SetActive(true);
        return clone;
    }

    private static Button FindSettingsButton(Transform panel)
    {
        foreach (Button button in panel.GetComponentsInChildren<Button>(false))
        {
            Image image = button.GetComponent<Image>();
            if (image != null && image.sprite != null && image.sprite.name.ToLowerInvariant().Contains("setting"))
                return button;
        }

        List<Button> buttons = new(panel.GetComponentsInChildren<Button>(false));
        buttons.Sort((left, right) => right.GetComponent<RectTransform>().anchoredPosition.y.CompareTo(left.GetComponent<RectTransform>().anchoredPosition.y));
        return buttons.Count > 1 ? buttons[1] : null;
    }

    private static void BuildShopLabel(Transform button)
    {
        if (button.Find("Shop Label Cover") != null)
            return;

        GameObject coverObject = new("Shop Label Cover", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform coverRect = coverObject.GetComponent<RectTransform>();
        coverRect.SetParent(button, false);
        coverRect.anchorMin = new Vector2(0.30f, 0.24f);
        coverRect.anchorMax = new Vector2(0.72f, 0.76f);
        coverRect.offsetMin = Vector2.zero;
        coverRect.offsetMax = Vector2.zero;
        coverObject.GetComponent<Image>().color = new Color(0.04f, 0.12f, 0.32f, 0.86f);

        GameObject labelObject = new("Shop Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(coverRect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = "SHOP";
        label.fontSize = 28f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
    }

    private static void ArrangeMenuButtons(Transform panel, Button shopButton)
    {
        List<Button> buttons = new(panel.GetComponentsInChildren<Button>(false));
        buttons.Remove(shopButton);
        buttons.Sort((left, right) => right.GetComponent<RectTransform>().anchoredPosition.y.CompareTo(left.GetComponent<RectTransform>().anchoredPosition.y));
        buttons.Insert(Mathf.Min(2, buttons.Count), shopButton);

        for (int i = 0; i < buttons.Count; i++)
        {
            RectTransform rect = buttons[i].GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(291f, 144f - i * 96f);
            rect.sizeDelta = new Vector2(300f, 96f);
            rect.localScale = Vector3.one;
        }
    }

    private static void OpenShop()
    {
        SceneLoadUtility.LoadSceneByName("StoreMenu");
    }
}
