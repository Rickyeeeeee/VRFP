using TMPro;
using UnityEngine;

public class BenchPressUI : MonoBehaviour
{
    [Header("Metrics Source - Bench Press")]
    public BenchPressMetrics Metrics;   // Drag your bench press metrics here

    [Header("Metrics Source - Avatar")]
    public AvatarMetrics AvatarMetrics; // Drag your avatar metrics here

    [Header("Left Arm UI")]
    public TMP_Text ForearmLeftText;
    public TMP_Text UpperArmLeftText;
    public TMP_Text ElbowHeightLeftText;

    [Header("Right Arm UI")]
    public TMP_Text ForearmRightText;
    public TMP_Text UpperArmRightText;
    public TMP_Text ElbowHeightRightText;

    [Header("Avatar UI")]
    public TMP_Text TorsoAngleText;
    public TMP_Text HeadHeightText;

    void Update()
    {
        // -----------------------
        // Bench Press Arm Metrics
        // -----------------------
        if (Metrics != null)
        {
            if (ForearmLeftText != null)
                ForearmLeftText.text      = $"Forearm L: {Metrics.ForearmAngleLeft:F1}°";

            if (UpperArmLeftText != null)
                UpperArmLeftText.text     = $"UpperArm L: {Metrics.UpperArmBodyAngleLeft:F1}°";

            if (ElbowHeightLeftText != null)
                ElbowHeightLeftText.text  = $"ElbowHeight L: {Metrics.ElbowHeightLeft:F2} m";

            if (ForearmRightText != null)
                ForearmRightText.text     = $"Forearm R: {Metrics.ForearmAngleRight:F1}°";

            if (UpperArmRightText != null)
                UpperArmRightText.text    = $"UpperArm R: {Metrics.UpperArmBodyAngleRight:F1}°";

            if (ElbowHeightRightText != null)
                ElbowHeightRightText.text = $"ElbowHeight R: {Metrics.ElbowHeightRight:F2} m";
        }

        // -----------------------
        // Avatar Metrics
        // -----------------------
        if (AvatarMetrics != null)
        {
            if (TorsoAngleText != null)
                TorsoAngleText.text = $"Torso Angle: {AvatarMetrics.TorsoAngle:F1}°";

            if (HeadHeightText != null)
                HeadHeightText.text = $"Head Height: {AvatarMetrics.HeadHeight:F2} m";
        }
    }
}
