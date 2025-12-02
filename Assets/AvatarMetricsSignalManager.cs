using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AvatarMetricsSignalManager : MonoBehaviour
{
    public AvatarMetrics avatarMetrics;
    public BenchPressMetrics benchPressMetrics;
    public SharedInfoManager sharedInfoManager;
    public HandCollider leftHandCollider;
    public HandCollider rightHandCollider;
    [SerializeField] public float foreArmAngleMin;
    [SerializeField] public float foreArmAngleMax;
    [SerializeField] public float upperArmAngleMin;
    [SerializeField] public float upperArmAngleMax;
    [SerializeField] public float elbowHeightMin;
    [SerializeField] public float elbowHeightMax;
    [SerializeField] public float torsoAngleMin;
    [SerializeField] public float torsoAngleMax;
    [SerializeField] public float requiredCollisionTime = 2.0f;
    public TMP_Text isLyingText;
    public TMP_Text isHandPositionCorrectText;
    
    private float collisionTimer = 0f;
    private bool isLeftHandColliding = false;
    private bool isRightHandColliding = false;
    private bool wasHandPositionCorrect = false;
    
    private void SignalIsLying()
    {
        bool condition = avatarMetrics.TorsoAngle > torsoAngleMin && avatarMetrics.TorsoAngle < torsoAngleMax;
        if (condition)
        {
            sharedInfoManager.SetIsLying(condition);
            if (isLyingText != null && isLyingText.isActiveAndEnabled)
            {
                isLyingText.text = "Is Lying: True";
            }
        }
        else
        {
            if (isLyingText != null && isLyingText.isActiveAndEnabled)
            {
                isLyingText.text = "Is Lying: False";
            }
        }
    }

    private float Average(float a, float b)
    {
        return (a + b) / 2.0f;
    }

    private bool CheckIsCorrect()
    {
        return Average(benchPressMetrics.ForearmAngleLeft, benchPressMetrics.ForearmAngleRight) > foreArmAngleMin &&
               Average(benchPressMetrics.ForearmAngleLeft, benchPressMetrics.ForearmAngleRight) < foreArmAngleMax &&
               Average(benchPressMetrics.UpperArmBodyAngleLeft, benchPressMetrics.UpperArmBodyAngleRight) > upperArmAngleMin &&
               Average(benchPressMetrics.UpperArmBodyAngleLeft, benchPressMetrics.UpperArmBodyAngleRight) < upperArmAngleMax &&
               Average(benchPressMetrics.ElbowHeightLeft, benchPressMetrics.ElbowHeightRight) > elbowHeightMin &&
               Average(benchPressMetrics.ElbowHeightLeft, benchPressMetrics.ElbowHeightRight) < elbowHeightMax;
    }

    private void SignalIsCorrect()
    {
        isLeftHandColliding = leftHandCollider != null && leftHandCollider.isColliding;
        isRightHandColliding = rightHandCollider != null && rightHandCollider.isColliding;

        bool isBothHandsColliding = isLeftHandColliding && isRightHandColliding;
        bool isPositionCorrect = CheckIsCorrect();

        if (isBothHandsColliding && isPositionCorrect)
        {
            collisionTimer += Time.deltaTime;

            if (collisionTimer >= requiredCollisionTime)
            {
                if (!wasHandPositionCorrect)
                {
                    sharedInfoManager.SetIsHandPositionCorrect(true);
                    wasHandPositionCorrect = true;
                }

                if (isHandPositionCorrectText != null && isHandPositionCorrectText.isActiveAndEnabled)
                {
                    isHandPositionCorrectText.text = $"Is Hand Correct: True (Timer: {collisionTimer:F2}s)";
                }
            }
            else
            {
                if (isHandPositionCorrectText != null && isHandPositionCorrectText.isActiveAndEnabled)
                {
                    isHandPositionCorrectText.text = $"Is Hand Correct: Counting... ({collisionTimer:F2}s / {requiredCollisionTime:F2}s)";
                }
            }
        }
        else
        {
            if (collisionTimer > 0f || wasHandPositionCorrect)
            {
                collisionTimer = 0f;
                wasHandPositionCorrect = false;
                sharedInfoManager.SetIsHandPositionCorrect(false);
            }

            if (isHandPositionCorrectText != null && isHandPositionCorrectText.isActiveAndEnabled)
            {
                if (!isBothHandsColliding)
                {
                    string handStatus = $"L:{(isLeftHandColliding ? "✓" : "✗")} R:{(isRightHandColliding ? "✓" : "✗")}";
                    isHandPositionCorrectText.text = $"Is Hand Correct: False (Need Both Hands on Bar) [{handStatus}]";
                }
                else
                {
                    string handStatus = $"L:{(isLeftHandColliding ? "✓" : "✗")} R:{(isRightHandColliding ? "✓" : "✗")}";
                    isHandPositionCorrectText.text = $"Is Hand Correct: False (Position Incorrect) [{handStatus}]";
                }
            }
        }
    }

    void Start()
    {
        if (leftHandCollider != null)
        {
            leftHandCollider.OnCollisionStateChanged += OnLeftHandCollisionChanged;
        }
        if (rightHandCollider != null)
        {
            rightHandCollider.OnCollisionStateChanged += OnRightHandCollisionChanged;
        }
    }

    private void OnDestroy()
    {
        if (leftHandCollider != null)
        {
            leftHandCollider.OnCollisionStateChanged -= OnLeftHandCollisionChanged;
        }
        if (rightHandCollider != null)
        {
            rightHandCollider.OnCollisionStateChanged -= OnRightHandCollisionChanged;
        }
    }

    private void OnLeftHandCollisionChanged(bool isColliding)
    {
        isLeftHandColliding = isColliding;
        ResetTimerIfNeeded();
    }

    private void OnRightHandCollisionChanged(bool isColliding)
    {
        isRightHandColliding = isColliding;
        ResetTimerIfNeeded();
    }

    private void ResetTimerIfNeeded()
    {
        if (!isLeftHandColliding || !isRightHandColliding)
        {
            collisionTimer = 0f;
            wasHandPositionCorrect = false;
            sharedInfoManager.SetIsHandPositionCorrect(false);
        }
    }

    void Update()
    {
        SignalIsCorrect();
        SignalIsLying();
    }
}
