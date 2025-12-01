using UnityEngine;

public class HipRotationLimiter : MonoBehaviour
{
    [Tooltip("Root/hip transform of the avatar")]
    public Transform hip; // drag your hip/root bone here in the inspector

    // Set this to true if you want to work in local space instead of world space
    public bool useLocalRotation = true;

    void LateUpdate()
    {
        if (!hip) return;

        if (useLocalRotation)
        {
            // Constrain LOCAL rotation: keeps only Y
            Vector3 e = hip.localEulerAngles;
            hip.localRotation = Quaternion.Euler(0.0f, 0f, 0.0f);
            hip.localPosition = Vector3.zero;
        }
        else
        {
            // Constrain WORLD rotation: keeps only Y
            Vector3 e = hip.eulerAngles;
            hip.rotation = Quaternion.Euler(0.0f, 0f, 0.0f);
            hip.localPosition = Vector3.zero;
        }
    }
}
