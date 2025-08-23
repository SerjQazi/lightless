//using UnityEngine;

//public class Pickup : MonoBehaviour
//{
//    private Transform playerTransform;
//    private Shoot playerShoot; // Reference to the Shoot component on the player

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        if (collision.gameObject.CompareTag("Player"))
//        {
//            playerTransform = collision.transform; // Store reference to the player's Transform
//            playerShoot = collision.gameObject.GetComponent<Shoot>();
//            if (playerShoot != null)
//            {
//                playerShoot.enabled = true; // Enable the specified script on the player
//            }
//            Destroy(gameObject); // Remove the power-up after collection
//        }
//    }

//}


using UnityEngine;

public class Pickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collission)
    {
        // Check if the colliding object is the player
        if (collission.gameObject.CompareTag("Player"))
        {
            // Get the Shoot component on the player
            Shoot playerShoot = gameObject.GetComponent<Shoot>();
            if (playerShoot != null)
            {
                playerShoot.enabled = true;    // Enable shooting
                playerShoot.hasSlingshot = true; // Set your trigger/flag
            }

            // Destroy the pickup item after collection
            Destroy(gameObject);
        }
    }
}
