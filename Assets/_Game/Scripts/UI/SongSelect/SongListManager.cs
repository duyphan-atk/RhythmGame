using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using Keyboard = UnityEngine.InputSystem.Keyboard;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SongListManager : MonoBehaviour
{
    public static SongListManager Instance;

    public List<SongData> _songList;
    public SongItemUI _itemPrefab;
    public Transform _contentArea;

    public Image _centerPreviewImage;
    public TextMeshProUGUI _rightBpmText;

    [Header("Navigation")]
    [SerializeField] private string gameplaySceneName = "KhoaCuBu";
    [SerializeField] private string mainMenuSceneName = "StartMenu";

    [Header("Generated Layout")]
    [SerializeField] private bool buildGeneratedLayout = true;
    [SerializeField] private bool hideLegacyLayout = true;
    [SerializeField] private bool includeProjectSongData = true;
    [SerializeField] private string projectSongDataFolder = "Assets/_Game/Data/Songs";
    [SerializeField] private bool playPreviewOnSelect = true;
    [SerializeField] private float previewStartSeconds = 0f;
    [SerializeField] private float cardHeight = 92f;
    [SerializeField] private float cardSpacing = 10f;

    private const string GeneratedRootName = "RG Generated Song Select";

    private readonly Dictionary<SongData, Button> _songButtons = new();
    private readonly Dictionary<SongData, Image> _songButtonImages = new();
    private readonly Dictionary<SongData, TextMeshProUGUI> _songButtonTitles = new();
    private SongData _selectedSong;
    private AudioSource _previewAudioSource;
    private RectTransform _generatedRoot;
    private Image _previewArt;
    private AspectRatioFitter _previewArtFitter;
    private TextMeshProUGUI _noArtText;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _artistText;
    private TextMeshProUGUI _bpmText;
    private TextMeshProUGUI _difficultyText;
    private TextMeshProUGUI _hintText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (buildGeneratedLayout)
        {
            BuildGeneratedLayout();
            SelectSong(GetFirstSong(), false);
        }
        else
        {
            PopulateList();
        }
    }

    private void Update()
    {
        if (WasConfirmPressedThisFrame())
            PlaySelectedSong();
    }

    public void PopulateList()
    {
        if (_itemPrefab == null || _contentArea == null)
        {
            Debug.LogWarning("SongListManager: Missing item prefab or content area.");
            return;
        }

        for (int i = _contentArea.childCount - 1; i >= 0; i--)
        {
            Destroy(_contentArea.GetChild(i).gameObject);
        }

        foreach (SongData song in _songList)
        {
            if (song == null)
                continue;

            SongItemUI item = Instantiate(_itemPrefab, _contentArea);
            item.Setup(song);
        }
    }

    public void SelectOrPlaySong(SongData song)
    {
        if (song == null)
            return;

        if (_selectedSong == song)
        {
            PlaySelectedSong();
            return;
        }

        SelectSong(song, true);
    }

    public void PlaySelectedSong()
    {
        SongData selectedSong = SelectedSongManager.Instance != null
            ? SelectedSongManager.Instance.SelectedSong
            : _selectedSong;

        if (selectedSong == null)
        {
            Debug.LogWarning("SongListManager: Select a song before pressing Play.");
            return;
        }

        if (_previewAudioSource != null)
            _previewAudioSource.Stop();

        LoadScene(gameplaySceneName);
    }

    public void BackToMainMenu()
    {
        if (_previewAudioSource != null)
            _previewAudioSource.Stop();

        LoadScene(mainMenuSceneName);
    }

    private void BuildGeneratedLayout()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("SongListManager: Cannot build generated layout because no Canvas was found.");
            PopulateList();
            return;
        }

        RemoveOldGeneratedRoot(canvas.transform);
        MergeProjectSongData();
        EnsureCanvasScaler(canvas);
        EnsureEventSystem();
        EnsurePreviewAudioSource();

        _generatedRoot = CreateRect(GeneratedRootName, canvas.transform);
        Stretch(_generatedRoot);
        _generatedRoot.SetAsLastSibling();
        HideLegacyLayout(canvas);

        Image bg = _generatedRoot.gameObject.AddComponent<Image>();
        bg.color = new Color(0.045f, 0.035f, 0.075f, 0.98f);

        RectTransform topBar = CreatePanel("Top Bar", _generatedRoot, new Color(0.96f, 0.93f, 0.98f, 0.96f));
        Anchor(topBar, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(0f, 64f));

        RectTransform backButton = CreatePanel("Back Button", topBar, new Color(0.42f, 0.12f, 0.36f, 0.95f));
        Anchor(backButton, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(112f, 42f));
        Button back = backButton.gameObject.AddComponent<Button>();
        back.targetGraphic = backButton.GetComponent<Image>();
        back.onClick.AddListener(BackToMainMenu);
        CreateText("Back", backButton, 18, FontStyles.Bold, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(100f, 34f));

        CreateText("Select a Song", topBar, 30, FontStyles.Normal, TextAlignmentOptions.Left, new Color(0.34f, 0.14f, 0.29f, 1f), new Vector2(146f, 0f), new Vector2(420f, 54f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
        CreateText("RhythmGame", topBar, 22, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.38f, 0.34f, 0.42f, 1f), Vector2.zero, new Vector2(360f, 48f));
        CreateText("Settings", topBar, 18, FontStyles.Bold, TextAlignmentOptions.Center, Color.white, new Vector2(-168f, 0f), new Vector2(150f, 40f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));
        CreateText("9999", topBar, 22, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.47f, 0.08f, 0.42f, 1f), new Vector2(-38f, 0f), new Vector2(100f, 40f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));

        RectTransform detailPanel = CreatePanel("Selected Song Detail", _generatedRoot, new Color(0.05f, 0.04f, 0.08f, 0.58f));
        Anchor(detailPanel, new Vector2(0.03f, 0.07f), new Vector2(0.58f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        _titleText = CreateText(string.Empty, detailPanel, 43, FontStyles.Bold, TextAlignmentOptions.Left, Color.white, new Vector2(24f, -38f), new Vector2(520f, 66f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        _artistText = CreateText("Tap once to select. Tap selected song again to play.", detailPanel, 18, FontStyles.Normal, TextAlignmentOptions.Left, new Color(0.9f, 0.82f, 0.96f, 1f), new Vector2(26f, -92f), new Vector2(540f, 36f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        _bpmText = CreateText("BPM: --", detailPanel, 22, FontStyles.Bold, TextAlignmentOptions.Left, new Color(0.93f, 0.86f, 1f, 1f), new Vector2(28f, -132f), new Vector2(260f, 36f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        _difficultyText = CreateText("DIFFICULTY --", detailPanel, 19, FontStyles.Bold, TextAlignmentOptions.Left, new Color(1f, 0.82f, 0.35f, 1f), new Vector2(28f, -168f), new Vector2(320f, 34f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        RectTransform artFrame = CreatePanel("Song Art Frame", detailPanel, new Color(0.23f, 0.12f, 0.32f, 0.86f));
        Anchor(artFrame, new Vector2(0.38f, 0.08f), new Vector2(0.96f, 0.76f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Mask artMask = artFrame.gameObject.AddComponent<Mask>();
        artMask.showMaskGraphic = true;
        _previewArt = CreatePanel("Song Art", artFrame, new Color(0.52f, 0.35f, 0.68f, 0.94f)).GetComponent<Image>();
        Stretch(_previewArt.rectTransform);
        _previewArt.rectTransform.localEulerAngles = new Vector3(0f, 0f, -2.25f);
        _previewArtFitter = _previewArt.gameObject.AddComponent<AspectRatioFitter>();
        _previewArtFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        _noArtText = CreateText("NO ART", artFrame, 38, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.4f), Vector2.zero, new Vector2(300f, 70f));

        RectTransform scorePanel = CreatePanel("Score Placeholder", detailPanel, new Color(0.1f, 0.05f, 0.12f, 0.62f));
        Anchor(scorePanel, new Vector2(0.02f, 0.08f), new Vector2(0.34f, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        CreateText("LOCAL BEST", scorePanel, 18, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.95f, 0.84f, 1f, 1f), new Vector2(0f, 54f), new Vector2(240f, 34f));
        CreateText("--'---'---", scorePanel, 28, FontStyles.Normal, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(260f, 46f));
        CreateText("Rank -", scorePanel, 18, FontStyles.Normal, TextAlignmentOptions.Center, new Color(1f, 0.78f, 0.96f, 1f), new Vector2(0f, -50f), new Vector2(240f, 34f));

        RectTransform listPanel = CreatePanel("Song Card List", _generatedRoot, new Color(0f, 0f, 0f, 0f));
        Anchor(listPanel, new Vector2(0.62f, 0.07f), new Vector2(0.985f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        BuildScrollList(listPanel);

        _hintText = CreateText("Enter = play selected", _generatedRoot, 18, FontStyles.Normal, TextAlignmentOptions.Right, new Color(1f, 1f, 1f, 0.72f), new Vector2(-26f, 18f), new Vector2(420f, 36f), new Vector2(1f, 0f), new Vector2(1f, 0f));
    }

    private void BuildScrollList(RectTransform parent)
    {
        ScrollRect scrollRect = parent.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.scrollSensitivity = 38f;

        RectTransform viewport = CreateRect("Viewport", parent);
        Stretch(viewport);
        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        RectTransform content = CreateRect("Content", viewport);
        Anchor(content, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 0f));

        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 18, 8, 10);
        layout.spacing = cardSpacing;
        layout.childAlignment = TextAnchor.UpperRight;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = content;

        foreach (SongData song in _songList)
        {
            if (song == null)
                continue;

            CreateSongCard(song, content);
        }
    }

    private void CreateSongCard(SongData song, RectTransform parent)
    {
        RectTransform card = CreatePanel("Song Card - " + song.SongTitle, parent, new Color(0.18f, 0.04f, 0.18f, 0.82f));
        LayoutElement layoutElement = card.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = cardHeight;
        layoutElement.minHeight = cardHeight;

        Image cardImage = card.GetComponent<Image>();
        Button button = card.gameObject.AddComponent<Button>();
        button.targetGraphic = cardImage;
        button.onClick.AddListener(() => SelectOrPlaySong(song));

        RectTransform levelBlock = CreatePanel("Difficulty Block", card, new Color(0.78f, 0.08f, 0.48f, 0.96f));
        Anchor(levelBlock, new Vector2(0f, 0f), new Vector2(0.2f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        CreateText(GetDifficultyLabel(song), levelBlock, 25, FontStyles.Bold, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(110f, 46f));

        RectTransform titleBlock = CreateRect("Title Block", card);
        Anchor(titleBlock, new Vector2(0.23f, 0f), new Vector2(0.78f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        CreateText(song.SongTitle, titleBlock, 21, FontStyles.Bold, TextAlignmentOptions.Left, Color.white, new Vector2(0f, 14f), new Vector2(320f, 34f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
        CreateText(GetBpmLabel(song), titleBlock, 14, FontStyles.Normal, TextAlignmentOptions.Left, new Color(0.9f, 0.78f, 0.94f, 1f), new Vector2(0f, -18f), new Vector2(260f, 26f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));

        RectTransform rankBlock = CreatePanel("Rank Block", card, new Color(1f, 1f, 1f, 0.82f));
        Anchor(rankBlock, new Vector2(0.8f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        CreateText("A", rankBlock, 32, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.52f, 0.14f, 0.5f, 1f), Vector2.zero, new Vector2(110f, 54f));

        _songButtons[song] = button;
        _songButtonImages[song] = cardImage;
        _songButtonTitles[song] = titleBlock.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void SelectSong(SongData song, bool playPreview)
    {
        _selectedSong = song;

        if (SelectedSongManager.Instance != null)
            SelectedSongManager.Instance.SetSelectedSong(song);

        ShowSongDetails(song, true);
        UpdateCardSelection();

        if (playPreview && playPreviewOnSelect)
            PlayPreview(song);
    }

    private void ShowSongDetails(SongData song, bool selected)
    {
        if (song == null)
            return;

        if (_titleText != null)
            _titleText.text = song.SongTitle;

        if (_artistText != null)
            _artistText.text = selected
                ? "Tap this song again to start."
                : "Tap once to select. Tap selected song again to play.";

        if (_bpmText != null)
            _bpmText.text = GetBpmLabel(song);

        if (_difficultyText != null)
            _difficultyText.text = "DIFFICULTY " + GetDifficultyLabel(song);

        if (_previewArt != null)
        {
            _previewArt.sprite = song.PreviewImage;
            _previewArt.color = song.PreviewImage != null
                ? Color.white
                : new Color(0.52f, 0.35f, 0.68f, 0.94f);
            _previewArt.preserveAspect = false;

            if (_previewArtFitter != null && song.PreviewImage != null)
            {
                Rect sourceRect = song.PreviewImage.rect;
                _previewArtFitter.aspectRatio = sourceRect.height > 0f
                    ? sourceRect.width / sourceRect.height
                    : 1f;
            }
        }

        if (_noArtText != null)
        {
            _noArtText.gameObject.SetActive(song.PreviewImage == null);
        }
    }

    private void UpdateCardSelection()
    {
        foreach (KeyValuePair<SongData, Image> pair in _songButtonImages)
        {
            bool selected = pair.Key == _selectedSong;
            pair.Value.color = selected
                ? new Color(0.82f, 0.12f, 0.54f, 0.96f)
                : new Color(0.18f, 0.04f, 0.18f, 0.82f);
        }
    }

    private void PlayPreview(SongData song)
    {
        if (_previewAudioSource == null || song == null || song.audioClip == null)
            return;

        _previewAudioSource.Stop();
        _previewAudioSource.clip = song.audioClip;
        _previewAudioSource.volume = RuntimeGameplaySettings.MusicVolume01;
        _previewAudioSource.loop = true;
        _previewAudioSource.time = Mathf.Clamp(previewStartSeconds, 0f, Mathf.Max(0f, song.audioClip.length - 0.05f));
        _previewAudioSource.Play();
    }

    private void MergeProjectSongData()
    {
        if (!includeProjectSongData)
            return;

#if UNITY_EDITOR
        if (string.IsNullOrWhiteSpace(projectSongDataFolder))
            return;

        HashSet<SongData> existingSongs = new(_songList);
        string[] guids = AssetDatabase.FindAssets("t:SongData", new[] { projectSongDataFolder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            SongData song = AssetDatabase.LoadAssetAtPath<SongData>(path);
            if (song == null || existingSongs.Contains(song))
                continue;

            _songList.Add(song);
            existingSongs.Add(song);
        }
#endif
    }

    private void HideLegacyLayout(Canvas canvas)
    {
        if (!hideLegacyLayout || canvas == null)
            return;

        if (transform == canvas.transform)
        {
            return;
        }

        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            Transform child = canvas.transform.GetChild(i);
            if (child == _generatedRoot)
                continue;

            if (transform == child || transform.IsChildOf(child))
            {
                CanvasGroup group = child.GetComponent<CanvasGroup>();
                if (group == null)
                    group = child.gameObject.AddComponent<CanvasGroup>();

                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void EnsurePreviewAudioSource()
    {
        _previewAudioSource = GetComponent<AudioSource>();
        if (_previewAudioSource == null)
            _previewAudioSource = gameObject.AddComponent<AudioSource>();

        _previewAudioSource.playOnAwake = false;
        _previewAudioSource.spatialBlend = 0f;
    }

    private SongData GetFirstSong()
    {
        foreach (SongData song in _songList)
        {
            if (song != null)
                return song;
        }

        return null;
    }

    private static string GetBpmLabel(SongData song)
    {
        return song != null && song._bpm > 0f ? $"BPM: {song._bpm:0.#}" : "BPM: --";
    }

    private static string GetDifficultyLabel(SongData song)
    {
        if (song == null)
            return "--";

        if (!string.IsNullOrWhiteSpace(song._difficulty))
            return song._difficulty;

        return song.difficultyLevel.ToString();
    }

    private static void LoadScene(string sceneName)
    {
        SceneLoadUtility.LoadSceneByName(sceneName);
    }

    private static bool WasConfirmPressedThisFrame()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            return true;
#endif

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return false;

        return keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;
#else
        return false;
#endif
    }

    private static void EnsureCanvasScaler(Canvas canvas)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static void RemoveOldGeneratedRoot(Transform canvasTransform)
    {
        Transform oldRoot = canvasTransform.Find(GeneratedRootName);
        if (oldRoot != null)
            Destroy(oldRoot.gameObject);
    }

    private static RectTransform CreateRect(string objectName, Transform parent)
    {
        GameObject obj = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static RectTransform CreatePanel(string objectName, Transform parent, Color color)
    {
        RectTransform rect = CreateRect(objectName, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return rect;
    }

    private static TextMeshProUGUI CreateText(
        string text,
        Transform parent,
        float fontSize,
        FontStyles style,
        TextAlignmentOptions alignment,
        Color color,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Vector2? anchorMin = null,
        Vector2? anchorMax = null)
    {
        RectTransform rect = CreateRect("Text - " + text, parent);
        Vector2 min = anchorMin ?? new Vector2(0.5f, 0.5f);
        Vector2 max = anchorMax ?? min;
        Anchor(rect, min, max, min, anchoredPosition, sizeDelta);

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = alignment;
        label.color = color;
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
        return label;
    }

    private static void Stretch(RectTransform rect, float inset = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(-inset * 2f, -inset * 2f);
    }

    private static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }
}
