using UnityEngine;

public class Pickups : MonoBehaviour
{
    public enum PickupType
    {
        Life = 0,
        Score = 1,
        Powerup = 2
    }

    public PickupType pickupType = PickupType.Life;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //Animator pickupAnimator = GetComponent<Animator>();

            //if (pickupAnimator != null)
            //{
            //    pickupAnimator.SetTrigger("PickupGet");
            //}

            switch (pickupType)
            {
                case PickupType.Life:
                    GameManager.Instance.SetLives(GameManager.Instance.Lives + 1);
                    Debug.Log("Life collected! Current lives: " + GameManager.Instance.Lives);
                    break;

                case PickupType.Score:
                    GameManager.Instance.AddScore(1);
                    Debug.Log("Score collected! Current score: " + GameManager.Instance.Score);
                    break;

                case PickupType.Powerup:
                    GameManager.Instance.ActivateJumpForceChange();
                    Debug.Log("Powerup collected! Jump force temporarily boosted.");
                    break;
            }

            Destroy(gameObject, 1f);
        }
    }
}
