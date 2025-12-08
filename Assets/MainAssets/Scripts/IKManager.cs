using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    public Transform bar;
    public Transform rightHandTarget;
    public Transform leftHandTarget;

    private Vector3 initialBarPosition;
    private Vector3 initialLeftHandPosition;
    private Vector3 initialRightHandPosition;
    private Quaternion initialBarRotation;
    private Quaternion initialLeftHandRotation;
    private Quaternion initialRightHandRotation;

    private Vector3 initialLeftHandOffset;
    private Vector3 initialRightHandOffset;

    private Vector3 minLeftHandTransform = new Vector3(-0.6f, 0.8f, 0.1f);
    private Vector3 maxLeftHandTransform = new Vector3(-0.3f, 1.108f, 0.6f);
    
    private Vector3 minRightHandTransform = new Vector3(0.28f, 0.82f, 0.15f);
    private Vector3 maxRightHandTransform = new Vector3(0.55f, 1.108f, 0.65f);

    // Start is called before the first frame update
    void Start()
    {
        if (bar != null)
        {
            initialBarPosition = bar.position;
            initialBarRotation = bar.rotation;
        }
        if (leftHandTarget != null)
        {
            initialLeftHandPosition = leftHandTarget.position;
            initialLeftHandRotation = leftHandTarget.rotation;
            initialLeftHandOffset = initialLeftHandPosition - initialBarPosition;
        }
        if (rightHandTarget != null)
        {
            initialRightHandPosition = rightHandTarget.position;
            initialRightHandRotation = rightHandTarget.rotation;
            initialRightHandOffset = initialRightHandPosition - initialBarPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bar != null && leftHandTarget != null && rightHandTarget != null)
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
            leftHandTarget.position = newLeftPosition;
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
            rightHandTarget.position = newRightPosition;
            rightHandTarget.rotation = rotationDelta * initialRightHandRotation;
        }
    }
}
