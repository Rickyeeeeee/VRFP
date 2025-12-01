using UnityEngine;

public class RelativeTransformFollower : MonoBehaviour
{
    [Header("Avatar bones")]
    public Transform referenceBoneA;   // e.g., Head
    public Transform referenceBoneB;   // e.g., Hand

    [Header("Object that will follow the relative transform")]
    public Transform targetObject;     // The transform you want to drive

    [Header("Optional: world or local relative transform")]
    public bool useLocalSpace = false;

    // The initial relative transform (computed once)
    private Vector3 initialLocalOffset;
    private Quaternion initialLocalRotation;

    void Start()
    {
        if (!referenceBoneA || !referenceBoneB || !targetObject)
        {
            Debug.LogError("[RelativeTransformFollower] Missing references.");
            enabled = false;
            return;
        }

        // Compute initial relative transform of BoneB relative to BoneA
        if (useLocalSpace)
        {
            // Relative transform in local spaces:
            initialLocalOffset = Quaternion.Inverse(referenceBoneA.localRotation) *
                                 (referenceBoneB.localPosition - referenceBoneA.localPosition);

            initialLocalRotation = Quaternion.Inverse(referenceBoneA.localRotation) *
                                   referenceBoneB.localRotation;
        }
        else
        {
            // Relative transform in world space:
            initialLocalOffset = referenceBoneA.InverseTransformPoint(referenceBoneB.position);
            initialLocalRotation = Quaternion.Inverse(referenceBoneA.rotation) *
                                   referenceBoneB.rotation;
        }
    }

    void LateUpdate()
    {
        if (!referenceBoneA || !referenceBoneB || !targetObject)
            return;

        // Apply live relative transform of the two bones to targetObject:
        if (useLocalSpace)
        {
            // Convert relative offsets into world (or parent) space
            Vector3 worldPos = referenceBoneA.position +
                               referenceBoneA.rotation * initialLocalOffset;

            Quaternion worldRot = referenceBoneA.rotation *
                                  initialLocalRotation;

            targetObject.SetPositionAndRotation(worldPos, worldRot);
        }
        else
        {
            // Get relative movement of A→B
            Vector3 currentRelativePos =
                referenceBoneA.TransformPoint(initialLocalOffset);

            Quaternion currentRelativeRot =
                referenceBoneA.rotation * initialLocalRotation;

            targetObject.SetPositionAndRotation(currentRelativePos, currentRelativeRot);
        }
    }
}
