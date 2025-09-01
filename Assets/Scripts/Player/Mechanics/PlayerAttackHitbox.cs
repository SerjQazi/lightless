using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerAttackHitbox : MonoBehaviour
{
    private Collider2D hitboxCollider;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        if (hitboxCollider == null)
            Debug.LogError("No Collider2D found on ClawHitbox!");
        else
            hitboxCollider.isTrigger = true; // ensure it's a trigger
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only deal damage if the hitbox is active
        if (!gameObject.activeInHierarchy) return;

        WalkerEnemy enemy = other.GetComponent<WalkerEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(3, DamageType.Default);
            Debug.Log($"🧨 Claw hit {enemy.name} for 3 damage!");
        }
        else
        {
            Debug.Log($"⚠️ Claw hitbox triggered on non-enemy object: {other.name}");
        }
    }

    private void OnDrawGizmos()
    {
        if (hitboxCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(hitboxCollider.bounds.center, hitboxCollider.bounds.size);
        }
    }

}
