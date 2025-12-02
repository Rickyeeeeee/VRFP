using UnityEngine;

public class HandCollider : MonoBehaviour
{
    public bool isColliding = false;
    public string targetTag = "Bar";
    
    public System.Action<bool> OnCollisionStateChanged;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            if (!isColliding)
            {
                isColliding = true;
                OnCollisionStateChanged?.Invoke(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            if (isColliding)
            {
                isColliding = false;
                OnCollisionStateChanged?.Invoke(false);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(targetTag) && !isColliding)
        {
            isColliding = true;
            OnCollisionStateChanged?.Invoke(true);
        }
    }
}
