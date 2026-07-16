using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EndOfChartTitle
{
    Fail,
    Clear,
    FullCombo,
    AllPerfect
}

/// <summary>Short full-screen title revealed between the chart and the detailed result screen.</summary>
public class EndOfChartPresentation : MonoBehaviour
{
    private const float FadeDuration = 0.35f;
    private CanvasGroup group;
    private Image titleImage;
    private TextMeshProUGUI fallbackText;

    public static EndOfChartPresentation GetOrCreate(Canvas canvas)
    {
        EndOfChartPresentation existing = canvas.GetComponentInChildren<EndOfChartPresentation>(true);
        if (existing != null)
            return existing;

        GameObject root = new GameObject("RG End Of Chart Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(EndOfChartPresentation));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.76f);
        root.transform.SetAsLastSibling();

        EndOfChartPresentation presentation = root.GetComponent<EndOfChartPresentation>();
        presentation.BuildVisuals();
        root.SetActive(false);
        return presentation;
    }

    public IEnumerator Show(EndOfChartTitle title, float visibleSeconds)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        ApplyTitle(title);
        GameplaySfxPlayer.Play(GetSoundCue(title));

        yield return FadeTo(1f);
        yield return new WaitForSecondsRealtime(visibleSeconds);
        yield return FadeTo(0f);
        gameObject.SetActive(false);
    }

    private void BuildVisuals()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = true;
        group.interactable = true;

        CreateRibbonBand("Ribbon Outer Band", new Color(0.14f, 0.01f, 0.12f, 0.92f), 0.145f);
        CreateRibbonBand("Ribbon Core Band", new Color(0.52f, 0.01f, 0.36f, 0.9f), 0.08f);

        GameObject imageObject = new GameObject("Title Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(transform, false);
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = Vector2.zero;
        imageRect.sizeDelta = new Vector2(960f, 540f);
        titleImage = imageObject.GetComponent<Image>();
        titleImage.preserveAspect = true;
        titleImage.raycastTarget = false;

        GameObject textObject = new GameObject("Fallback Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(imageObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        fallbackText = textObject.GetComponent<TextMeshProUGUI>();
        fallbackText.font = TMP_Settings.defaultFontAsset;
        fallbackText.fontSize = 82f;
        fallbackText.fontStyle = FontStyles.Bold;
        fallbackText.alignment = TextAlignmentOptions.Center;
        fallbackText.color = Color.white;
        fallbackText.raycastTarget = false;
    }

    private void CreateRibbonBand(string objectName, Color color, float heightPercent)
    {
        GameObject bandObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bandObject.transform.SetParent(transform, false);

        RectTransform rect = bandObject.GetComponent<RectTransform>();
        float halfHeight = heightPercent * 0.5f;
        rect.anchorMin = new Vector2(0f, 0.5f - halfHeight);
        rect.anchorMax = new Vector2(1f, 0.5f + halfHeight);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = bandObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private void ApplyTitle(EndOfChartTitle title)
    {
        GameplaySfxCatalog catalog = GameplaySfxPlayer.Catalog;
        Sprite sprite = null;
        string label;
        switch (title)
        {
            case EndOfChartTitle.AllPerfect:
                sprite = catalog != null ? catalog.titleAllPerfect : null;
                label = "ALL PERFECT";
                break;
            case EndOfChartTitle.FullCombo:
                sprite = catalog != null ? catalog.titleFullCombo : null;
                label = "FULL COMBO";
                break;
            case EndOfChartTitle.Clear:
                sprite = catalog != null ? catalog.titleTrackComplete : null;
                label = "TRACK COMPLETE";
                break;
            default:
                sprite = catalog != null ? catalog.titleTrackLost : null;
                label = "TRACK LOST";
                break;
        }

        titleImage.sprite = sprite;
        titleImage.enabled = sprite != null;
        fallbackText.text = label;
        fallbackText.gameObject.SetActive(sprite == null);
    }

    private GameplaySfxCue GetSoundCue(EndOfChartTitle title)
    {
        if (title == EndOfChartTitle.AllPerfect)
            return GameplaySfxCue.TrackAllPerfect;
        return title == EndOfChartTitle.Fail ? GameplaySfxCue.TrackFail : GameplaySfxCue.TrackClear;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = group.alpha;
        float elapsed = 0f;
        while (elapsed < FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / FadeDuration);
            yield return null;
        }

        group.alpha = targetAlpha;
    }
}
