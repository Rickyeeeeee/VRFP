using UnityEngine;
using Unity.XR.CoreUtils;

public class CameraManager : MonoBehaviour
{
    
    public static CameraManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
    [Header("References")]
    public XROrigin xrOrigin;     // XR rig
    public Camera vrCamera;       // VR HMD camera

    [Header("Teleport Target")]
    public Transform Target;      // Assign any entity here

    private void Reset()
    {
        xrOrigin = FindObjectOfType<XROrigin>();
        vrCamera = Camera.main;
    }

    /// <summary>
    /// Teleports VR camera to a world position & rotation.
    /// </summary>
    public void TeleportCamera(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (!xrOrigin || !vrCamera)
        {
            Debug.LogError("CameraManager: Missing XR Origin or VR Camera reference.");
            return;
        }

        // Current HMD pos
        Vector3 camPos = vrCamera.transform.position;

        // Compute the head offset inside the rig
        Vector3 offset = camPos - xrOrigin.transform.position;

        // Move XR Origin so HMD ends at target
        xrOrigin.transform.position = targetPosition;
        xrOrigin.transform.rotation = targetRotation;
    }

    /// <summary>
    /// Teleport VR camera to a Transform target.
    /// Uses target transform's position & rotation.
    /// </summary>
    public void TeleportToTarget()
    {
        if (!Target)
        {
            Debug.LogError("CameraManager: No target assigned!");
            return;
        }

        TeleportCamera(Target.position, Target.rotation);
    }
}
