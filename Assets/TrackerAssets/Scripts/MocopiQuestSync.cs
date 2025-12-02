using UnityEngine;

/// <summary>
/// Synchronizes a Mocopi-driven avatar's head with the Meta Quest 3 HMD position/rotation.
/// This locks the virtual body to the Quest's tracking space.
/// </summary>
public class MocopiQuestSynchronizer : MonoBehaviour
{
    [Header("Quest 3 References (Master)")]
    [Tooltip("The Transform representing the Quest 3 HMD position/rotation.")]
    public Transform questHmdTransform;

    [Header("Mocopi Avatar References (Slave)")]
    [Tooltip("The root Transform of your Mocopi-driven avatar.")]
    public Transform mocopiAvatarRoot;
    [Tooltip("The specific Transform/Bone representing the Head in the Mocopi avatar.")]
    public Transform mocopiHeadBone;

    [Header("Calibration Settings")]
    [Tooltip("Toggle to perform a one-time calibration.")]
    public bool performCalibration = true;

    // Stores the fixed offset between the Mocopi Head and the Mocopi Root
    private Vector3 headToRootOffset;

    // Stores the rotational difference to correct for different 'forward' definitions
    private Quaternion rotationOffset = Quaternion.identity;

    void Start()
    {
        if (questHmdTransform == null || mocopiAvatarRoot == null || mocopiHeadBone == null)
        {
            Debug.LogError("Synchronizer setup incomplete. Please assign all required Transforms in the Inspector.");
            enabled = false;
            return;
        }

        // 1. Calculate the initial offset between the avatar's root and head.
        // This offset represents the initial posture when the script starts.
        headToRootOffset = mocopiAvatarRoot.position - mocopiHeadBone.position;

        if (performCalibration)
        {
            CalibrateAlignment();
        }
    }

    /// <summary>
    /// Calculates the rotational difference between the Quest and Mocopi systems.
    /// Assumes the user is looking straight forward when the script starts.
    /// </summary>
    private void CalibrateAlignment()
    {
        // Get the initial rotation difference between the Quest HMD and the Mocopi Head.
        // This corrects for any rotational axis mismatch (e.g., if Mocopi defines Z-forward 
        // and Quest defines -Z-forward).
        rotationOffset = questHmdTransform.rotation * Quaternion.Inverse(mocopiHeadBone.rotation);
        
        Debug.Log("Quest-Mocopi Alignment Calibrated.");
    }

    // LateUpdate runs after all standard Update() and fixed Update() calls,
    // ensuring we synchronize *after* the Mocopi system has moved the avatar for the frame.
    void LateUpdate()
    {
        // 1. Force the Mocopi Head to match the Quest HMD's rotation (with offset correction)
        // This locks the visual direction of the head.
        // mocopiHeadBone.rotation = questHmdTransform.rotation * Quaternion.Inverse(rotationOffset);


        // 2. Adjust the Mocopi Avatar's Root Position
        // New Root Position = Quest HMD Position - (Mocopi Head-to-Root Offset)
        // We use the HMD position and subtract the calculated offset, ensuring the avatar's body 
        // follows the HMD while maintaining the Mocopi's current body posture.
        mocopiAvatarRoot.position = questHmdTransform.position + headToRootOffset;
    }
}