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
public void TeleportCamera(Transform target)
{
    if (!xrOrigin || !vrCamera)
    {
        Debug.LogError("CameraManager: Missing XR Origin or VR Camera reference.");
        return;
    }

    // --- Step 1: camera's local pose inside XR Origin ---
    Vector3 localCamPos = xrOrigin.transform.InverseTransformPoint(vrCamera.transform.position);
    Quaternion localCamRot =
        Quaternion.Inverse(xrOrigin.transform.rotation) * vrCamera.transform.rotation;

    // --- Step 2: set XR Origin rotation so that camera rotation becomes target rotation ---
    // XROriginRot * localCamRot = targetRot
    Quaternion desiredOriginRot = target.rotation * Quaternion.Inverse(localCamRot);
    xrOrigin.transform.rotation = desiredOriginRot;

    // --- Step 3: set XR Origin position so that camera position becomes target position ---
    // XROriginPos + (XROriginRot * localCamPos) = targetPos
    Vector3 worldOffset = xrOrigin.transform.rotation * localCamPos;
    xrOrigin.transform.position = target.position - worldOffset;

    Debug.Log($"Camera final pos: {vrCamera.transform.position}");
    Debug.Log($"Camera final rot: {vrCamera.transform.rotation}");
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

        TeleportCamera(Target);
    }
}
