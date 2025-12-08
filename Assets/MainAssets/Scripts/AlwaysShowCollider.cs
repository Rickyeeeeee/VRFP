using UnityEngine;

[ExecuteAlways]
public class AlwaysShowCollider : MonoBehaviour
{
    public Color gizmoColor = Color.green;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        Collider col = GetComponent<Collider>();
        if (!col) return;

        // BOX COLLIDER
        if (col is BoxCollider bc)
        {
            Gizmos.matrix = bc.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(bc.center, bc.size);
        }
        // SPHERE COLLIDER
        else if (col is SphereCollider sc)
        {
            Gizmos.DrawWireSphere(sc.transform.TransformPoint(sc.center), sc.radius);
        }
        // CAPSULE COLLIDER
        else if (col is CapsuleCollider cc)
        {
            DrawCapsule(cc);
        }
    }

    // Draw capsule outline manually
    private void DrawCapsule(CapsuleCollider cc)
    {
        Gizmos.matrix = cc.transform.localToWorldMatrix;

        float radius = cc.radius;
        float height = Mathf.Max(cc.height, radius * 2f);
        Vector3 center = cc.center;

        float cylinderHeight = height - 2f * radius;
        Vector3 up = Vector3.up * (cylinderHeight / 2f);

        Vector3 top = center + up;
        Vector3 bottom = center - up;

        Gizmos.DrawWireSphere(top, radius);
        Gizmos.DrawWireSphere(bottom, radius);

        Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
        Gizmos.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);
        Gizmos.DrawLine(top + Vector3.right * radius,   bottom + Vector3.right * radius);
        Gizmos.DrawLine(top - Vector3.right * radius,   bottom - Vector3.right * radius);
    }
}
