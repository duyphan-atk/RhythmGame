using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Populates the StoreMenu Song tab from SongData assets by reusing the first SongItemSlot as a template.
/// </summary>
public class StoreSongListLoader : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private SongPreviewButton songItemTemplate;
    [SerializeField] private string editorSongDataFolder = "Assets/_Game/Data/Songs";
    [SerializeField] private bool includeFreeSongs = false;

    private void Start()
    {
        Populate();
    }

    [ContextMenu("Populate Song Store")]
    public void Populate()
    {
        ResolveReferences();

        if (contentRoot == null || songItemTemplate == null)
        {
            Debug.LogWarning("[Store] StoreSongListLoader is missing Content Root or Song Item Template.");
            return;
        }

        List<SongData> songs = LoadStoreSongs();
        if (songs.Count == 0)
        {
            Debug.LogWarning("[Store] No purchasable SongData found for StoreMenu.");
            return;
        }

        ClearGeneratedItems();
        songItemTemplate.gameObject.SetActive(false);

        foreach (SongData song in songs)
        {
            SongPreviewButton item = Instantiate(songItemTemplate, contentRoot);
            item.gameObject.name = "SongItemSlot - " + song.SongTitle;
            item.gameObject.SetActive(true);
            ConfigureItem(item, song);
        }
    }

    private void ResolveReferences()
    {
        if (contentRoot == null)
            contentRoot = transform;

        if (songItemTemplate == null)
            songItemTemplate = contentRoot.GetComponentInChildren<SongPreviewButton>(true);
    }

    private void ClearGeneratedItems()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = contentRoot.GetChild(i);
            SongPreviewButton item = child.GetComponent<SongPreviewButton>();
            if (item == null || item == songItemTemplate)
                continue;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    private List<SongData> LoadStoreSongs()
    {
        Dictionary<string, SongData> byId = new Dictionary<string, SongData>();

        foreach (SongData song in Resources.LoadAll<SongData>("Songs"))
            AddSong(byId, song);

#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(editorSongDataFolder))
        {
            foreach (string guid in AssetDatabase.FindAssets("t:SongData", new[] { editorSongDataFolder }))
                AddSong(byId, AssetDatabase.LoadAssetAtPath<SongData>(AssetDatabase.GUIDToAssetPath(guid)));
        }
#endif

        return new List<SongData>(byId.Values);
    }

    private void AddSong(Dictionary<string, SongData> byId, SongData song)
    {
        if (song == null)
            return;
        if (!includeFreeSongs && song.unlockType == SongUnlockType.Free)
            return;

        string itemId = SongUnlockService.GetSongItemId(song);
        if (string.IsNullOrWhiteSpace(itemId) || byId.ContainsKey(itemId))
            return;

        byId.Add(itemId, song);
    }

    private static void ConfigureItem(SongPreviewButton item, SongData song)
    {
        item.previewClip = song.audioClip;
        item.itemId = SongUnlockService.GetSongItemId(song);

        if (item.songTitleText != null)
            item.songTitleText.text = song.SongTitle;

        Image cover = item.transform.Find("CoverArt ")?.GetComponent<Image>();
        if (cover != null)
        {
            cover.sprite = song.PreviewImage;
            cover.color = song.PreviewImage != null ? Color.white : cover.color;
        }

        SetButtonPrice(item.coinBuyButton, SongUnlockService.GetPrice(song, CurrencyType.Money));
        SetButtonPrice(item.diamondBuyButton, SongUnlockService.GetPrice(song, CurrencyType.Diamond));
        item.RefreshOwnedState();
    }

    private static void SetButtonPrice(Button button, int price)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = price.ToString();
    }
}
