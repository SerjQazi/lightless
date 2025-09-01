using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TurretGhost : Enemy
{
    private Rigidbody2D rb;
    private Transform player;

    [Header("Movement Settings")]
    [SerializeField, Range(0.5f, 5f)] private float xVelocity = 1.0f;
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 6.0f;
    [SerializeField] private float attackRadius = 4.0f;

    [Header("Attack Settings")]
    [SerializeField] private float fireRate = 2.0f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 5f; // easy tuning in Inspector

    private float timeSinceLastShot = 0f;
    private int moveDirection = 1;
    private bool isDead = false;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // ghost floats
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (fireRate <= 0) fireRate = 2.0f;
    }

    private void Update()
    {
        if (isDead) { rb.linearVelocity = Vector2.zero; return; }
        if (player == null) { Patrol(); return; }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRadius)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("Attack", true); // play attack animation
        }
        else
        {
            anim.SetBool("Attack", false);

            if (distanceToPlayer <= detectionRadius)
            {
                // float toward player
                float dir = Mathf.Sign(player.position.x - transform.position.x);
                rb.linearVelocity = new Vector2(dir * xVelocity, rb.linearVelocity.y);
                sr.flipX = spriteFacesRight ? dir > 0 : dir < 0;
            }
            else
            {
                Patrol();
            }
        }
    }

    // ------------------ PATROL ------------------
    private void Patrol()
    {
        rb.linearVelocity = new Vector2(moveDirection * xVelocity, rb.linearVelocity.y);
        sr.flipX = spriteFacesRight ? moveDirection > 0 : moveDirection < 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Barrier") && !isDead)
        {
            moveDirection *= -1;
        }
    }

    // ------------------ SHOOTING (called from Animation Event) ------------------
    public void ShootProjectile()
    {
        if (Time.time < timeSinceLastShot + fireRate) return;

        if (projectilePrefab != null && firePoint != null && player != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Always shoot directly at player, regardless of facing
            Vector2 dir = (player.position - firePoint.position).normalized;

            proj.GetComponent<Projectile>().SetVelocity(dir * projectileSpeed);
        }

        timeSinceLastShot = Time.time;
    }

    // ------------------ DAMAGE & DEATH ------------------
    public override void TakeDamage(int damageValue, DamageType damageType = DamageType.Default)
    {
        if (isDead) return;
        anim.SetTrigger("Hit");
        base.TakeDamage(damageValue, damageType);

        if (CurrentHealth <= 0)
        {
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            anim.SetTrigger("Death");
        }
    }

    // ------------------ GIZMOS ------------------
    private void OnDrawGizmos()
    {
        Vector3 offset = new Vector3(0f, 0.5f, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + offset, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + offset, attackRadius);
    }
}
