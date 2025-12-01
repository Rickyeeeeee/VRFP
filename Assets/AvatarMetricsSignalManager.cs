using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class AvatarMetricsSignalManager : MonoBehaviour
{
    [Header("Reference Objects")]
    public AvatarMetrics avatarMetrics;
    public BenchPressMetrics benchPressMetrics;
    public SharedInfoManager sharedInfoManager;
    [Header("Correct Ranges")]
    [SerializeField] public float foreArmAngleMin;
    [SerializeField] public float foreArmAngleMax;
    [SerializeField] public float upperArmAngleMin;
    [SerializeField] public float upperArmAngleMax;
    [SerializeField] public float elbowHeightMin;
    [SerializeField] public float elbowHeightMax;
    [SerializeField] public float torsoAngleMin;
    [SerializeField] public float torsoAngleMax;
    [Header("Debug UI elements")]
    public TMP_Text isLyingText;
    public TMP_Text isHandPositionCorrectText;
    
    private void SignalIsLying()
    {
        bool condition = avatarMetrics.TorsoAngle > torsoAngleMin && avatarMetrics.TorsoAngle < torsoAngleMax;
        if (condition)
        {
            sharedInfoManager.SetIsLying(condition);

            if (isLyingText != null && isLyingText.isActiveAndEnabled)
            {
                isLyingText.text = $"Is Lying: True";
            }
        }
        else
        {
            if (isLyingText != null && isLyingText.isActiveAndEnabled)
            {
                isLyingText.text = $"Is Lying: False";
            }
        }
    }

    private float Average(float a, float b)
    {
        return (a + b) / 2.0f;
    }

    private void SignalIsCorrect()
    {
        bool condition =    Average(benchPressMetrics.ForearmAngleLeft, benchPressMetrics.ForearmAngleRight) > foreArmAngleMin &&
                            Average(benchPressMetrics.ForearmAngleLeft, benchPressMetrics.ForearmAngleRight) < foreArmAngleMax &&
                            Average(benchPressMetrics.UpperArmBodyAngleLeft, benchPressMetrics.UpperArmBodyAngleRight) > upperArmAngleMin &&
                            Average(benchPressMetrics.UpperArmBodyAngleLeft, benchPressMetrics.UpperArmBodyAngleRight) < upperArmAngleMax &&
                            Average(benchPressMetrics.ElbowHeightLeft, benchPressMetrics.ElbowHeightRight) > elbowHeightMin &&
                            Average(benchPressMetrics.ElbowHeightLeft, benchPressMetrics.ElbowHeightRight) < elbowHeightMax;
        // bool condition =    benchPressMetrics.ForearmAngleLeft > foreArmAngleMin &&
        //                     benchPressMetrics.ForearmAngleLeft < foreArmAngleMax &&
        //                     benchPressMetrics.ForearmAngleRight > foreArmAngleMin &&
        //                     benchPressMetrics.ForearmAngleRight < foreArmAngleMax &&
        //                     benchPressMetrics.UpperArmBodyAngleLeft > upperArmAngleMin &&
        //                     benchPressMetrics.UpperArmBodyAngleLeft < upperArmAngleMax &&
        //                     benchPressMetrics.UpperArmBodyAngleRight > upperArmAngleMin &&
        //                     benchPressMetrics.UpperArmBodyAngleRight < upperArmAngleMax &&
        //                     benchPressMetrics.ElbowHeightLeft > elbowHeightMin &&
        //                     benchPressMetrics.ElbowHeightLeft < elbowHeightMax &&
        //                     benchPressMetrics.ElbowHeightRight > elbowHeightMin &&
        //                     benchPressMetrics.ElbowHeightRight < elbowHeightMax;
        if (condition)
        {
            sharedInfoManager.SetIsHandPositionCorrect(condition);
            if (isHandPositionCorrectText != null && isHandPositionCorrectText.isActiveAndEnabled)
            {
                isHandPositionCorrectText.text = $"Is Hand Correct: True";
            }
        }
        else
        {
            if (isHandPositionCorrectText != null && isHandPositionCorrectText.isActiveAndEnabled)
            {
                isHandPositionCorrectText.text = $"Is Hand Correct: False";
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SignalIsCorrect();
        SignalIsLying();
    }
}
