using UnityEngine;

public class BarbellSync : MonoBehaviour
{
    public Transform targetBar;
    public Transform StartingPoint;
    private Vector3 positionOffset;

    void Start()
    {
        positionOffset = transform.position - StartingPoint.position;
    }

    void LateUpdate()
    {
        if (targetBar != null)
        {
            // transform.position = targetBar.position + positionOffset;
            Vector3 currentEuler = transform.eulerAngles;
            Vector3 targetEuler = targetBar.eulerAngles;
            transform.rotation = Quaternion.Euler(targetEuler.x, targetEuler.y, targetEuler.z);
        }
    }
}