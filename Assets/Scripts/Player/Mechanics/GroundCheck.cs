using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [Header("Ground Check Settings")]
    public float radius = 0.2f;
    public LayerMask groundLayer;

    public bool IsGrounded { get; private set; }

    void Update()
    {
        IsGrounded = Physics2D.OverlapCircle(transform.position, radius, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
