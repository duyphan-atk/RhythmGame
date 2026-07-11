using UnityEngine;

public class ChartPlaybackClock : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float audioOffset = 0f;

    [Header("State")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float startLeadInSeconds = 3f;



    private float _startTime;
    private float _pausedSongTime;
    private bool _isPlaying;
    private bool _hasPausedSongTime;
    private bool _waitingForAudioStart;
    private bool _audioStarted;

    public float SongTime
    {
        get
        {
            float rawTime;

            if (audioSource != null)
            {
                rawTime = _audioStarted
                    ? audioSource.time
                    : GetVirtualRawTime();
            }
            else if (!_isPlaying)
            {
                rawTime = _pausedSongTime;
            }
            else
            {
                rawTime = Time.time - _startTime;
            }

            return rawTime + audioOffset;
        }
    }

    public bool IsPlaying => _isPlaying;

    public void SetOffset(float offset)
    {
        audioOffset = offset;
    }

    public void SetVolume(float volume01)
    {
        if (audioSource == null)
            return;

        audioSource.volume = Mathf.Clamp01(volume01);
    }

    /// <summary>
    /// Đổi AudioClip đang phát. Gọi trước Play().
    /// ChartNoteSpawner gọi method này khi SelectedSongManager có bài đang chọn.
    /// </summary>
    public void SetClip(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.clip = clip;
    }

    private void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }

    public void Play()
    {
        float rawStartTime = _hasPausedSongTime
            ? _pausedSongTime - audioOffset
            : -Mathf.Max(0f, startLeadInSeconds);

        _startTime = Time.time - rawStartTime;
        _isPlaying = true;
        _hasPausedSongTime = false;

        if (audioSource == null)
            return;

        audioSource.Stop();
        _audioStarted = false;
        _waitingForAudioStart = rawStartTime < 0f;

        if (!_waitingForAudioStart)
            StartAudioAt(rawStartTime);
    }

    public void Pause()
    {
        _pausedSongTime = SongTime;
        _hasPausedSongTime = true;
        _isPlaying = false;

        if (audioSource != null && _audioStarted)
        {
            audioSource.Pause();
        }
        else if (audioSource != null)
        {
            audioSource.Stop();
        }

        _waitingForAudioStart = false;
    }

    public void Stop()
    {
        _pausedSongTime = 0f;
        _hasPausedSongTime = false;
        _isPlaying = false;
        _waitingForAudioStart = false;
        _audioStarted = false;

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void Update()
    {
        if (!_isPlaying || !_waitingForAudioStart)
            return;

        if (GetVirtualRawTime() >= 0f)
            StartAudioAt(0f);
    }

    private float GetVirtualRawTime()
    {
        if (!_isPlaying)
            return _pausedSongTime - audioOffset;

        return Time.time - _startTime;
    }

    private void StartAudioAt(float rawStartTime)
    {
        if (audioSource == null)
            return;

        float clipLength = audioSource.clip != null ? audioSource.clip.length : 0f;
        audioSource.time = clipLength > 0f
            ? Mathf.Clamp(rawStartTime, 0f, Mathf.Max(0f, clipLength - 0.001f))
            : 0f;

        audioSource.Play();
        _audioStarted = true;
        _waitingForAudioStart = false;
    }
}
