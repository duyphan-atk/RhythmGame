using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongItemUI : MonoBehaviour
{
    public TextMeshProUGUI _titleText;
    public Image _previewImage;

    public SongData _data;

    public void Setup(SongData data)
    {
        _data = data;

        if (_titleText != null)
            _titleText.text = data != null ? data.SongTitle : string.Empty;

        if (_previewImage != null)
            _previewImage.sprite = data != null ? data.PreviewImage : null;
    }

    public void OnSelect()
    {
        if (_data == null)
            return;

        if (SelectedSongManager.Instance != null)
            SelectedSongManager.Instance.SetSelectedSong(_data);
        else
            Debug.LogWarning("SongItemUI: SelectedSongManager is missing in the scene.");

        SongListManager listManager = SongListManager.Instance;
        if (listManager != null)
        {
            if (listManager._centerPreviewImage != null)
                listManager._centerPreviewImage.sprite = _data.PreviewImage;

            if (listManager._rightBpmText != null)
                listManager._rightBpmText.text = _data._bpm > 0 ? $"{_data._bpm:0.#} BPM" : "-- BPM";
        }

        Debug.Log($"Selected: {_data.SongTitle}");
    }
}
