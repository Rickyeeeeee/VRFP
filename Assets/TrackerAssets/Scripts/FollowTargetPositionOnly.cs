using UnityEngine;

/// <summary>
/// Makes this object follow a target's POSITION (with an offset),
/// but DOES NOT follow its rotation.
/// 
/// Attach this to your XR rig root (XR Origin / XRRig).
/// </summary>
public class FollowTargetPositionOnly : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The object whose position you want to follow.")]
    public Transform target;

    [Header("Offset")]
    [Tooltip("World-space offset from the target's position.")]
    public Vector3 positionOffset = Vector3.zero;

    [Tooltip("If true, use the initial offset between rig and target from Start().")]
    public bool useInitialOffset = true;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("[FollowTargetPositionOnly] No target assigned.", this);
            return;
        }

        if (useInitialOffset)
        {
            // Compute the offset based on how you placed them in the scene
            positionOffset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Follow only POSITION (with offset)
        transform.position = target.position + positionOffset;

        // IMPORTANT:
        // We do NOT touch transform.rotation here.
        // So the rig keeps its own rotation (HMD + locomotion).
    }
}
