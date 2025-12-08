using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeBarManager : MonoBehaviour
{
    public Transform fakeBar;
    public Transform rightHand;
    public Transform leftHand;
    // public Vector3 leftHandOffset = new Vector3(0.0f, 0.2f, 0.0f);
    // public Vector3 rightHandOffset = new Vector3(0.0f, 0.2f, 0.0f);
    public Vector3 offset = new Vector3(0.0f, 0.07f, -0.03f);

    private Vector3 initialBarPosition;
    private Vector3 initialLeftHandPosition;
    private Vector3 initialRightHandPosition;

    void Start()
    {
         if (fakeBar != null)
        {
            initialBarPosition = fakeBar.position;
            // initialBarRotation = fakeBar.rotation;
        }
        if (leftHand != null)
        {
            initialLeftHandPosition = leftHand.position;
            // initialLeftHandRotation = leftHand.rotation;
            // initialLeftHandOffset = initialLeftHandPosition - initialBarPosition;
        }
        if (rightHand != null)
        {
            initialRightHandPosition = rightHand.position;
            // initialRightHandRotation = rightHand.rotation;
            // initialRightHandOffset = initialRightHandPosition - initialBarPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 middlePoint = (leftHand.position + rightHand.position) / 2 + offset;
        if (fakeBar != null && leftHand != null && rightHand != null) {
            fakeBar.position = middlePoint;
            fakeBar.rotation = Quaternion.LookRotation(rightHand.position - leftHand.position);
        }
    }
}
