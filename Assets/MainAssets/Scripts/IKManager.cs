using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    public Transform bar;
    public Transform rightHandTarget;
    public Transform leftHandTarget;

    private float initialBarY;
    private float initialLeftHandY;
    private float initialRightHandY;

    // Start is called before the first frame update
    void Start()
    {
        if (bar != null)
        {
            initialBarY = bar.position.y;
        }
        if (leftHandTarget != null)
        {
            initialLeftHandY = leftHandTarget.position.y;
        }
        if (rightHandTarget != null)
        {
            initialRightHandY = rightHandTarget.position.y;
        }
        Debug.LogWarning("[IKManager] initalBarY: " + initialBarY + " initialLeftHandY: " + initialLeftHandY + " initialRightHandY: " + initialRightHandY);
    }

    // Update is called once per frame
    void Update()
    {
        if (bar != null && leftHandTarget != null && rightHandTarget != null)
        {
            float yDelta = bar.position.y - initialBarY;

            Vector3 newLeftPosition = leftHandTarget.position;
            newLeftPosition.y = initialLeftHandY + yDelta;
            leftHandTarget.position = newLeftPosition;

            Vector3 newRightPosition = rightHandTarget.position;
            newRightPosition.y = initialRightHandY + yDelta;
            rightHandTarget.position = newRightPosition;

        }
    }
}
