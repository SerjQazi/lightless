using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]  
[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileType projectileType = ProjectileType.Player;
    [SerializeField, Range(0.1f, 5f)] private float lifetime = 1f;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private bool hasImpacted = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(LifetimeCoroutine());
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>(); // safety check
        rb.linearVelocity = velocity;

        // Defensive orientation: flip based on horizontal velocity
        if (sr != null && Mathf.Abs(velocity.x) > 0.01f) // ignore tiny values
        {
            sr.flipX = velocity.x < 0f; // face left if moving left
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TriggerImpact();

        // Apply damage only if this is a player projectile hitting an enemy
        if (projectileType == ProjectileType.Player)
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(10);
            }
        }
    }

    private IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(lifetime);

        if (!hasImpacted)
        {
            TriggerImpact();
        }
    }

    private void TriggerImpact()
    {
        if (hasImpacted) return;
        hasImpacted = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        col.enabled = false;

        animator.SetTrigger("Impact");
        Debug.Log("Projectile triggered impact animation.");
        StartCoroutine(WaitForImpactAnimation());
    }

    private IEnumerator WaitForImpactAnimation()
    {
        // Assuming the impact animation is 0.5 seconds long, adjust as necessary
        yield return new WaitForSeconds(0.5f);
        DestroyAfterImpact();
    }

    //// This MUST be called from an Animation Event at the end of the Impact animation
    public void DestroyAfterImpact()
    {
        Destroy(gameObject);
        Debug.Log("Projectile destroyed after impact animation finished.");
    }
}

public enum ProjectileType
{
    Player,
    Enemy
}
