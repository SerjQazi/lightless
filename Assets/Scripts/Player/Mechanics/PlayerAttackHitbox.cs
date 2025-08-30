using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🧤 Claw hitbox triggered with: {other.name}");

        WalkerEnemy enemy = other.GetComponent<WalkerEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(3, DamageType.Default);
            Debug.Log($"🧨 Claw hit {enemy.name} for 3 damage!");
        }
        else
        {
            Debug.Log($"⚠️ Triggered object is not a WalkerEnemy: {other.name}");
        }
    }
}
