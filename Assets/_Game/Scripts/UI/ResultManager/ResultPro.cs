using UnityEngine;
using TMPro;
using System.Collections;

public class ResultPro : MonoBehaviour
{
    [Header("UI Text References")]
    public TextMeshProUGUI perfectText;
    public TextMeshProUGUI goodText;
    public TextMeshProUGUI badText;
    public TextMeshProUGUI missText;
    public TextMeshProUGUI maxComboText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI rankText;

    [Header("Animation Settings")]
    public float statsRollTime = 0.5f;
    public float scoreRollTime = 1.2f;
    public float rankStampTime = 0.2f;

    [Header("Preview Data")]
    [Tooltip("Chỉ dùng khi mở riêng scene score để xem UI. Gameplay sẽ gọi Show() với số liệu thật.")]
    [SerializeField] private bool playOnStartForPreview;
    public int targetPerfect = 100;
    public int targetGood = 0;
    public int targetBad = 0;
    public int targetMiss = 0;
    public int targetMaxCombo = 100;

    private int calculatedTotalScore;
    private float calculatedAccuracy;
    private bool isRainbowRank = false;

    void Start()
    {
        if (playOnStartForPreview)
            Show(new GameplayResultData(targetPerfect, targetGood, targetBad, targetMiss, targetMaxCombo));
    }

    void Update()
    {
        // Hiệu ứng đổi màu cầu vồng liên tục cho Rank SSS
        if (isRainbowRank && rankText != null)
        {
            float hue = Mathf.Repeat(Time.time * 0.5f, 1f); // Tốc độ đổi màu: 0.5
            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

            // Giữ nguyên độ trong suốt (Alpha) để không bị lỗi lúc đang Fade in
            rainbowColor.a = rankText.alpha;
            rankText.color = rainbowColor;
        }
    }

    public void Show(GameplayResultData data)
    {
        StopAllCoroutines();
        targetPerfect = data.perfect;
        targetGood = data.great;
        targetBad = data.good;
        targetMiss = data.miss;
        targetMaxCombo = data.maxCombo;
        ResetUI();
        StartCoroutine(PlayResultSequence());
    }

    void ResetUI()
    {
        perfectText.text = "0";
        goodText.text = "0";
        badText.text = "0";
        missText.text = "0";
        maxComboText.text = "0";
        totalScoreText.text = "0000000";

        rankText.text = "";
        rankText.transform.localScale = Vector3.one * 8f; // Phóng to để dập xuống
        rankText.alpha = 0f;
        isRainbowRank = false;
    }

    IEnumerator PlayResultSequence()
    {
        // Perfect = 100%, Great = 75%, Good = 50%, Miss = 0%.
        int totalNotes = targetPerfect + targetGood + targetBad + targetMiss;
        if (totalNotes > 0)
        {
            calculatedAccuracy = (targetPerfect + targetGood * 0.75f + targetBad * 0.5f) / totalNotes;
            calculatedTotalScore = Mathf.RoundToInt(calculatedAccuracy * 1000000f);
        }

        // 1. Chạy chỉ số phụ
        StartCoroutine(RollNumber(perfectText, targetPerfect, statsRollTime, ""));
        StartCoroutine(RollNumber(goodText, targetGood, statsRollTime, ""));
        StartCoroutine(RollNumber(badText, targetBad, statsRollTime, ""));
        StartCoroutine(RollNumber(missText, targetMiss, statsRollTime, ""));
        StartCoroutine(RollNumber(maxComboText, targetMaxCombo, statsRollTime, ""));

        yield return new WaitForSeconds(statsRollTime + 0.2f);

        // 2. Chạy tổng điểm
        yield return StartCoroutine(RollNumber(totalScoreText, calculatedTotalScore, scoreRollTime, "D7"));

        yield return new WaitForSeconds(0.3f);

        // 3. Đóng mộc Rank
        string rankName;
        Color rankColor;
        GetRankData(out rankName, out rankColor);

        yield return StartCoroutine(StampRankEffect(rankName, rankColor));
    }

    // Logic xử lý Phân loại Rank và Màu sắc
    void GetRankData(out string rankName, out Color rankColor)
    {
        isRainbowRank = false;

        if (calculatedAccuracy >= 0.9999f)
        {
            isRainbowRank = true;
            rankColor = Color.white; 
            rankName = "SSS";
        }
        else if (calculatedAccuracy >= 0.98f)
        {
            rankColor = new Color(1f, 0.25f, 0f); 
            rankName = "SS";
        }
        else if (calculatedAccuracy >= 0.95f)
        {
            rankColor = new Color(1f, 0.55f, 0f); 
            rankName = "S";
        }
        else if (calculatedAccuracy >= 0.90f)
        {
            rankColor = Color.yellow; 
            rankName = "A";
        }
        else if (calculatedAccuracy >= 0.80f)
        {
            rankColor = new Color(0.6f, 0.2f, 0.8f); 
            rankName = "B";
        }
        else if (calculatedAccuracy >= 0.70f)
        {
            rankColor = Color.green; 
            rankName = "C";
        }
        else
        {
            rankColor = Color.white; 
            rankName = "D";
        }
    }

    IEnumerator RollNumber(TextMeshProUGUI textUI, int endValue, float duration, string format)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);

            int current = (int)Mathf.Lerp(0, endValue, easeOut);
            textUI.text = string.IsNullOrEmpty(format) ? current.ToString() : current.ToString(format);
            yield return null;
        }
        textUI.text = string.IsNullOrEmpty(format) ? endValue.ToString() : endValue.ToString(format);
    }

    IEnumerator StampRankEffect(string rank, Color color)
    {
        rankText.text = rank;

        // Chỉ set màu tĩnh nếu không phải là Rank SSS
        if (!isRainbowRank)
        {
            rankText.color = color;
        }

        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 8f;
        Vector3 targetScale = Vector3.one;

        while (elapsed < rankStampTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rankStampTime;
            float easeIn = t * t * t;

            rankText.transform.localScale = Vector3.Lerp(startScale, targetScale, easeIn);
            rankText.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        rankText.transform.localScale = targetScale;
        rankText.alpha = 1f;
    }
}
