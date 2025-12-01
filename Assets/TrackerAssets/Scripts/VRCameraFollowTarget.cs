using UnityEngine;

public class VRMatchHeadRotation : MonoBehaviour
{
    public Transform avatarHead;  // mocopi-driven head bone

    void LateUpdate()
    {
        // hard set camera rig rotation = head rotation
        transform.rotation = Quaternion.identity;
    }
}
