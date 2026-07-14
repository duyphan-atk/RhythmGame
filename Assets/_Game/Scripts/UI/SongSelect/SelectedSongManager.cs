using UnityEngine;

public class SelectedSongManager : MonoBehaviour
{
    public static SelectedSongManager Instance { get; private set; }
    public const string LastSelectedSongGroupKey = "RhythmGame.LastSelectedSongGroup";
    public const string LastSelectedDifficultyKey = "RhythmGame.LastSelectedDifficulty";

    public SongData _selectedSong;
    public SongData SelectedSong => _selectedSong;
    public Difficulty SelectedDifficulty { get; private set; } = Difficulty.Medium;

    public static SelectedSongManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        GameObject managerObject = new("SelectedSongManager");
        return managerObject.AddComponent<SelectedSongManager>();
    }

    public static bool TryRestoreLastSelection(out SongData song, out Difficulty difficulty)
    {
        difficulty = LoadSavedDifficulty();
        song = null;

        string savedGroupId = PlayerPrefs.GetString(LastSelectedSongGroupKey, string.Empty);
        SongData[] availableSongs = Resources.LoadAll<SongData>("Songs");
        if (availableSongs.Length == 0)
            return false;

        foreach (SongData candidate in availableSongs)
        {
            if (candidate == null)
                continue;

            string candidateGroupId = string.IsNullOrWhiteSpace(candidate.songGroupId)
                ? candidate.name
                : candidate.songGroupId;

            if (SongData.SanitizeForFileName(candidateGroupId) == savedGroupId)
            {
                song = candidate;
                return true;
            }
        }

        // Direct Play Mode has no menu selection. A committed song is a more useful
        // fallback than the legacy per-machine chart JSON configured in the scene.
        song = availableSongs[0];
        return song != null;
    }

    private void Awake()
    {
        // Logic Singleton chuẩn bài
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent != null)
                transform.SetParent(null);

            DontDestroyOnLoad(gameObject); // Giữ Object này không bị xóa khi đổi Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSelectedSong(SongData song)
    {
        _selectedSong = song;
        if (song != null)
            SelectedDifficulty = song.difficultyLevel;

        SaveSelection();
    }

    public void SetSelectedSong(SongData song, Difficulty difficulty)
    {
        _selectedSong = song;
        SelectedDifficulty = difficulty;
        SaveSelection();
    }

    private void SaveSelection()
    {
        if (_selectedSong == null)
            return;

        string groupId = !string.IsNullOrWhiteSpace(_selectedSong.songGroupId)
            ? _selectedSong.songGroupId
            : _selectedSong.name;

        PlayerPrefs.SetString(LastSelectedSongGroupKey, SongData.SanitizeForFileName(groupId));
        PlayerPrefs.SetInt(LastSelectedDifficultyKey, (int)SelectedDifficulty);
        PlayerPrefs.Save();
    }

    private static Difficulty LoadSavedDifficulty()
    {
        int savedDifficulty = PlayerPrefs.GetInt(LastSelectedDifficultyKey, (int)Difficulty.Medium);
        return System.Enum.IsDefined(typeof(Difficulty), savedDifficulty)
            ? (Difficulty)savedDifficulty
            : Difficulty.Medium;
    }
}
