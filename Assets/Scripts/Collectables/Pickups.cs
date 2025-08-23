using UnityEngine;
public class Pickups : MonoBehaviour
{
    public enum PickupType
    {
        Life = 0,
        Score = 1,
        Powerup = 2
    }

    public PickupType pickupType = PickupType.Life; // Type of the pickup

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController pc = collision.GetComponent<PlayerController>();
            Animator animator = collision.GetComponent<Animator>();
            Animator pickupAnimator = GetComponent<Animator>();

            if (pickupAnimator != null)
            {
                pickupAnimator.SetTrigger("PickupGet");
            }

            switch (pickupType)
            {
                case PickupType.Life:
                    pc.Lives++;
                    Debug.Log("Life collected! Current lives: " + pc.Lives);
                    break;
                case PickupType.Score:
                    pc.Score++;
                    //if (animator != null)
                    Debug.Log("Score collected! Current score: " + pc.Score);
                    break;
                case PickupType.Powerup:
                    pc.ActivateJumpForceChange();
                    break;
            }

            Destroy(gameObject, 1f); // Destroy the pickup after collection
        }
    }
}
