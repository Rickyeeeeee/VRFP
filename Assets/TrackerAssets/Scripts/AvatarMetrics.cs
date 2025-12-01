using UnityEngine;

public class AvatarMetrics : MonoBehaviour
{
    [Header("Torso References")]
    public Transform Hips;
    public Transform Chest;   // or UpperChest

    [Header("Head Reference")]
    public Transform Head;

    [Header("Results (runtime)")]
    /// <summary>
    /// Angle between torso (Chest–Hips) and world up (Vector3.up).
    /// 0° = upright, 90° = torso horizontal.
    /// </summary>
    public float TorsoAngle;

    /// <summary>
    /// Height of the head relative to the hips (or world Y if hips not set).
    /// </summary>
    public float HeadHeight;

    void Update()
    {
        TorsoAngle = ComputeTorsoAngle();
        HeadHeight = ComputeHeadHeight();
    }

    // ---------------------------
    //      METRIC FUNCTIONS
    // ---------------------------

    /// <summary>
    /// Computes the torso angle as the angle between the torso direction
    /// (Chest - Hips) and world up. 0° = upright, 90° = lying flat.
    /// </summary>
    float ComputeTorsoAngle()
    {
        if (Hips == null || Chest == null) return 0f;

        Vector3 torsoDir = (Chest.position - Hips.position).normalized;
        return Vector3.Angle(torsoDir, Vector3.up);
    }

    /// <summary>
    /// Computes the head height.
    /// If Hips is assigned: height relative to hips (Head.y - Hips.y).
    /// Otherwise: absolute world height of the head (Head.y).
    /// </summary>
    float ComputeHeadHeight()
    {
        if (Head == null) return 0f;

        if (Hips == null)
        {
            // Fallback: use absolute world height
            return Head.position.y;
        }

        // Relative height to hips (more invariant to avatar root position)
        return Head.position.y - Hips.position.y;
    }
}
