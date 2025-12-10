using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    public static IKManager Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }
    public Transform bar;
    public Transform rightHandTarget;
    public Transform leftHandTarget;
    public Transform rightHandRotationTarget;
    public Transform leftHandRotationTarget;

    [SerializeField]
    private Vector3 initialBarPosition;
    [SerializeField]
    private Vector3 initialLeftHandPosition;
    [SerializeField]
    private Vector3 initialRightHandPosition;
    [SerializeField]
    private Quaternion initialBarRotation;
    [SerializeField]
    private Quaternion initialLeftHandRotation;
    [SerializeField]
    private Quaternion initialRightHandRotation;

    [SerializeField]
    private Vector3 initialLeftHandOffset;
    [SerializeField]
    private Vector3 initialRightHandOffset;

    private Vector3 minLeftHandTransform = new Vector3(-0.6f, 0.8f, 0.1f);
    private Vector3 maxLeftHandTransform = new Vector3(-0.3f, 1.108f, 0.6f);
    
    private Vector3 minRightHandTransform = new Vector3(0.28f, 0.82f, 0.15f);
    private Vector3 maxRightHandTransform = new Vector3(0.55f, 1.108f, 0.65f);

    // Start is called before the first frame update
    private bool initialized = false;
    public void Initialize()
    { 
        Debug.Log("Initialize");
        if (bar != null)
        {
            initialBarPosition = bar.position;
            initialBarRotation = bar.rotation;
        }
        if (leftHandTarget != null)
        {
            initialLeftHandPosition = leftHandTarget.position;
            initialLeftHandRotation = leftHandRotationTarget.rotation;
            initialLeftHandOffset = initialLeftHandPosition - initialBarPosition;
        }
        if (rightHandTarget != null)
        {
            initialRightHandPosition = rightHandTarget.position;
            initialRightHandRotation = rightHandRotationTarget.rotation;
            initialRightHandOffset = initialRightHandPosition - initialBarPosition;
        }
        initialized=true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (bar != null && leftHandTarget != null && rightHandTarget != null && initialized)
        {
            // float yDelta = bar.position.y - initialBarPosition.y;

            // // left hand
            // Vector3 newLeftPosition = leftHandTarget.position;
            // newLeftPosition.y = initialLeftHandPosition.y + yDelta;
            // if (newLeftPosition.y <= minLeftHandTransform.y)
            // {
            //     newLeftPosition.y = minLeftHandTransform.y;
            // } 
            // else if (newLeftPosition.y >= maxLeftHandTransform.y)
            // {
            //     newLeftPosition.y = newLeftPosition.y;
            // }
            // leftHandTarget.position = newLeftPosition;

            // // right hand
            // Vector3 newRightPosition = rightHandTarget.position;
            // newRightPosition.y = initialRightHandPosition.y + yDelta;
            // if (newRightPosition.y <= minRightHandTransform.y)
            // {
            //     newRightPosition.y = minRightHandTransform.y;
            // }
            // else if (newRightPosition.y >= maxRightHandTransform.y)
            // {
            //     newRightPosition.y = newRightPosition.y;
            // }
            // rightHandTarget.position = newRightPosition;

            Quaternion rotationDelta = bar.rotation * Quaternion.Inverse(initialBarRotation);

            // left hand
            Vector3 rotatedLeftOffset = rotationDelta * initialLeftHandOffset;
            Vector3 newLeftPosition = bar.position + rotatedLeftOffset;
            if (newLeftPosition.y <= minLeftHandTransform.y)
            {
                newLeftPosition.y = minLeftHandTransform.y;
            } 
            else if (newLeftPosition.y >= maxLeftHandTransform.y)
            {
                newLeftPosition.y = newLeftPosition.y;
            }
            // leftHandTarget.position = newLeftPosition;
            leftHandTarget.rotation = rotationDelta * initialLeftHandRotation;

            // right hand
            Vector3 rotatedRightOffset = rotationDelta * initialRightHandOffset;
            Vector3 newRightPosition = bar.position + rotatedRightOffset;
            if (newRightPosition.y <= minRightHandTransform.y)
            {
                newRightPosition.y = minRightHandTransform.y;
            }
            else if (newRightPosition.y >= maxRightHandTransform.y)
            {
                newRightPosition.y = newRightPosition.y;
            }
            // rightHandTarget.position = newRightPosition;
            rightHandTarget.rotation = rotationDelta * initialRightHandRotation;
        }
    }
}
