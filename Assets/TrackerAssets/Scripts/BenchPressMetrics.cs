using UnityEngine;

public class BenchPressMetrics : MonoBehaviour
{
    [Header("Torso References")]
    public Transform Hips;
    public Transform Chest;   // or UpperChest

    [Header("Left Arm")]
    public Transform LeftShoulder;
    public Transform LeftElbow;
    public Transform LeftWrist;

    [Header("Right Arm")]
    public Transform RightShoulder;
    public Transform RightElbow;
    public Transform RightWrist;

    [Header("Results (runtime)")]
    public float ForearmAngleLeft;
    public float UpperArmBodyAngleLeft;
    public float ElbowHeightLeft;

    public float ForearmAngleRight;
    public float UpperArmBodyAngleRight;
    public float ElbowHeightRight;

    void Update()
    {
        // LEFT ARM
        ForearmAngleLeft       = ComputeForearmAngle(LeftElbow, LeftWrist);
        UpperArmBodyAngleLeft  = ComputeUpperArmBodyAngle(LeftShoulder, LeftElbow);
        ElbowHeightLeft        = ComputeElbowHeight(LeftElbow);

        // RIGHT ARM
        ForearmAngleRight      = ComputeForearmAngle(RightElbow, RightWrist);
        UpperArmBodyAngleRight = ComputeUpperArmBodyAngle(RightShoulder, RightElbow);
        ElbowHeightRight       = ComputeElbowHeight(RightElbow);
    }

    // ---------------------------
    //      METRIC FUNCTIONS
    // ---------------------------

    // 1. Forearm–Vertical angle, adjusted so 90° = vertical
    float ComputeForearmAngle(Transform elbow, Transform wrist)
    {
        if (elbow == null || wrist == null) return 0f;

        Vector3 forearmDir = (wrist.position - elbow.position).normalized;
        float angleFromVertical = Vector3.Angle(forearmDir, Vector3.up);

        // User wants: 90° = ideal (vertical)
        return 90f - angleFromVertical;
    }

    // 2. Upper-arm angle relative to torso direction
    float ComputeUpperArmBodyAngle(Transform shoulder, Transform elbow)
    {
        if (shoulder == null || elbow == null || Hips == null || Chest == null) return 0f;

        Vector3 upperArm = (elbow.position - shoulder.position).normalized;
        Vector3 torsoDir = (Hips.position - Chest.position).normalized;

        return Vector3.Angle(upperArm, torsoDir); // ideal ~30–50°
    }

    // 3. Elbow height deviation from chest height
    float ComputeElbowHeight(Transform elbow)
    {
        if (elbow == null || Chest == null) return 0f;

        return elbow.position.y - Chest.position.y;
    }
}
