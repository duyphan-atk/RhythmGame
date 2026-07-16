using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Owns the full-screen fade and serializes scene requests to prevent repeated clicks.</summary>
public class SceneTransitionController : MonoBehaviour
{
    private const float FadeOutDuration = 0.35f;
    private const float FadeInDuration = 0.7f;
    private const float IncomingSceneBlackHold = 0.1f;
    private static SceneTransitionController instance;

    private CanvasGroup overlayGroup;
    private bool isTransitioning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance().StartCoroutine(EnsureInstance().FadeInitialSceneIn());
    }

    public static bool RequestScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        SceneTransitionController controller = EnsureInstance();
        if (controller.isTransitioning)
            return false;

        controller.StartCoroutine(controller.TransitionToScene(sceneName));
        return true;
    }

    private static SceneTransitionController EnsureInstance()
    {
        if (instance != null)
            return instance;

        GameObject root = new GameObject("RG Scene Transition");
        instance = root.AddComponent<SceneTransitionController>();
        DontDestroyOnLoad(root);
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        isTransitioning = true;
        SetOverlayInteraction(true);

        float soundLength = GameplaySfxPlayer.Play(GameplaySfxCue.FadeOut);
        yield return FadeTo(1f, FadeOutDuration);
        if (soundLength > FadeOutDuration)
            yield return new WaitForSecondsRealtime(soundLength - FadeOutDuration);

        AsyncOperation sceneLoad = SceneLoadUtility.LoadSceneAsync(sceneName);
        if (sceneLoad != null)
        {
            // The next scene is not allowed to draw until the existing frame is fully black.
            sceneLoad.allowSceneActivation = false;
            while (sceneLoad.progress < 0.9f)
                yield return null;

            SetOverlayAlpha(1f);
            sceneLoad.allowSceneActivation = true;
            yield break;
        }

        // This fallback is only used for editor-only scenes not present in Build Settings.
        if (!SceneLoadUtility.LoadSceneImmediately(sceneName))
        {
            isTransitioning = false;
            yield return FadeTo(0f, FadeInDuration);
            SetOverlayInteraction(false);
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isTransitioning)
            StartCoroutine(FadeSceneIn());
    }

    private IEnumerator FadeInitialSceneIn()
    {
        SetOverlayAlpha(1f);
        SetOverlayInteraction(true);
        yield return new WaitForEndOfFrame();
        yield return FadeSceneIn();
    }

    private IEnumerator FadeSceneIn()
    {
        // Keep the new scene covered until its UI has been initialized and rendered once.
        SetOverlayAlpha(1f);
        SetOverlayInteraction(true);
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(IncomingSceneBlackHold);
        GameplaySfxPlayer.Play(GameplaySfxCue.FadeIn);
        yield return FadeTo(0f, FadeInDuration);
        isTransitioning = false;
        SetOverlayInteraction(false);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = overlayGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetOverlayAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));
            yield return null;
        }

        SetOverlayAlpha(targetAlpha);
    }

    private void BuildOverlay()
    {
        GameObject canvasObject = new GameObject("Fade Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = short.MaxValue;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject shade = new GameObject("Black Fade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        shade.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = shade.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = shade.GetComponent<Image>();
        image.color = Color.black;
        overlayGroup = shade.GetComponent<CanvasGroup>();
        SetOverlayAlpha(0f);
        SetOverlayInteraction(false);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (overlayGroup != null)
            overlayGroup.alpha = Mathf.Clamp01(alpha);
    }

    private void SetOverlayInteraction(bool enabled)
    {
        if (overlayGroup == null)
            return;

        overlayGroup.blocksRaycasts = enabled;
        overlayGroup.interactable = enabled;
    }
}
