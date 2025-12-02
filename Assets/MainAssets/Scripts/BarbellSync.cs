using UnityEngine;

public class BarbellSync : MonoBehaviour
{
    public Transform targetBar;
    
    private Vector3 positionOffset;

    void Start()
    {
        positionOffset = transform.position - targetBar.position;
    }

    void LateUpdate()
    {
        if (targetBar != null)
        {
            transform.position = targetBar.position + positionOffset;
            Vector3 currentEuler = transform.eulerAngles;
            Vector3 targetEuler = targetBar.eulerAngles;
            transform.rotation = Quaternion.Euler(currentEuler.x, targetEuler.y, targetEuler.z);
        }
    }
}