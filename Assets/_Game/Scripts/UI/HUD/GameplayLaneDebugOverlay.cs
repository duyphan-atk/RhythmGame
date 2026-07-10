using System.Text;
using TMPro;
using UnityEngine;

public class GameplayLaneDebugOverlay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameplayLaneLayout laneLayout;
    [SerializeField] private TextMeshProUGUI outputText;

    [Header("Display")]
    [SerializeField] private bool showOverlay;

    private readonly StringBuilder builder = new StringBuilder(96);

    private void Awake()
    {
        ResolveReferences();
    }

    private void Update()
    {
        ResolveReferences();
        UpdateText();
    }

    private void ResolveReferences()
    {
        if (laneLayout == null)
            laneLayout = FindFirstObjectByType<GameplayLaneLayout>();

        if (outputText == null)
            outputText = GetComponent<TextMeshProUGUI>();
    }

    private void UpdateText()
    {
        if (outputText == null)
            return;

        outputText.enabled = showOverlay;
        if (!showOverlay || laneLayout == null)
            return;

        builder.Clear();
        builder.Append("Lane ").Append(laneLayout.AppliedLaneSpacing.ToString("0.0")).Append('\n');
        builder.Append("HitY ").Append(laneLayout.AppliedHitlineY.ToString("0.0")).Append('\n');
        builder.Append("Touch ").Append(laneLayout.AppliedTouchRadius.ToString("0.0")).Append('\n');
        builder.Append("Judge ").Append(laneLayout.AppliedHitlineJudgeDistance.ToString("0.0"));

        outputText.text = builder.ToString();
    }
}
