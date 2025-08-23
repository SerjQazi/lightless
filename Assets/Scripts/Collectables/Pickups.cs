using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Pickups : MonoBehaviour
{
    public enum PickupType
    {
        Life,
        Score,
        Powerup
    }

    [Header("Pickup Settings")]
    public PickupType pickupType = PickupType.Life;
    public int scoreAmount = 10;       // how much score to add
    public float destroyDelay = 0.25f; // delay before destroy to let animation play

    private Animator pickupAnimator;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        pickupAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerController pc = collision.GetComponent<PlayerController>();
        Animator playerAnimator = collision.GetComponent<Animator>();

        // Trigger pickup animation if it exists
        if (pickupAnimator != null)
        {
            pickupAnimator.SetTrigger("PickupGet");
        }

        switch (pickupType)
        {
            case PickupType.Life:
                if (pc != null)
                {
                    pc.Lives++;
                    Debug.Log("❤️ Life collected! Current lives: " + pc.Lives);
                }
                break;

            case PickupType.Score:
                if (pc != null)
                {
                    pc.Score += scoreAmount;
                    Debug.Log("⭐ Score collected! Current score: " + pc.Score);
                }
                break;

            case PickupType.Powerup:
                if (pc != null)
                {
                    pc.ActivateJumpForceChange(); // temporary jump boost
                }

                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("hasSlingShot", true);
                }

                Debug.Log("💥 Powerup collected! Player can now use the slingshot.");
                break;
        }

        // delay destroy so pickup animation can play
        Destroy(gameObject, destroyDelay);
    }
}
