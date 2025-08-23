using UnityEngine;

public class Pickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("🎯 Pickup triggered by Player!");

            // Get the Shoot component from the player
            Shoot playerShoot = collision.gameObject.GetComponent<Shoot>();
            Animator playerAnimator = collision.gameObject.GetComponent<Animator>();

            if (playerShoot != null)
            {
                playerShoot.enabled = true;       // Enable shooting
                playerShoot.hasSlingshot = true;  // Mark that player now has slingshot
                Debug.Log("✅ Slingshot shooting enabled on player.");
            }
            else
            {
                Debug.LogWarning("⚠️ No Shoot component found on Player!");
            }

            if (playerAnimator != null)
            {
                playerAnimator.SetBool("hasSlingShot", true);
                Debug.Log("🎬 Animator 'hasSlingShot' set to TRUE.");
            }
            else
            {
                Debug.LogWarning("⚠️ No Animator found on Player!");
            }

            // Destroy the pickup item after collection
            Debug.Log("💥 Slingshot pickup collected and destroyed.");
            Destroy(gameObject);
        }
    }
}
