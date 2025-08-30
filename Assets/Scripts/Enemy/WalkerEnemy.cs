using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WalkerEnemy : Enemy
{
    private Rigidbody2D rb;
    private Transform player;

    [Header("Movement Settings")]
    [SerializeField, Range(0.5f, 5f)] private float xVelocity = 1.1f;
    [SerializeField] private float turnPauseTime = 0.8f; // optional pause after turn
    [SerializeField] private float idleTime = 3.0f;
    [SerializeField] private float idleChance = 0.2f;
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5.0f;
    [SerializeField] private float attackRadius = 1.2f;

    private bool isWalking = true;
    private bool isTurning = false;
    private bool isAttacking = false;
    private bool isIdling = false;
    private bool isDead = false;

    private int moveDirection = 1;

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (isDead) { rb.linearVelocity = Vector2.zero; return; }

        if (player == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Stop movement if mid-action
        if (isAttacking || isTurning || isIdling)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (distanceToPlayer <= attackRadius)
        {
            StartCoroutine(AttackRoutine());
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // ------------------ PATROL ------------------
    private void Patrol()
    {
        if (!isWalking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("isWalking", false);
            return;
        }

        rb.linearVelocity = new Vector2(moveDirection * xVelocity, rb.linearVelocity.y);
        anim.SetBool("isWalking", true);

        // Random idle behavior
        if (Random.value < idleChance * Time.deltaTime)
        {
            StartCoroutine(IdleRoutine());
        }

        // Flip sprite depending on direction
        sr.flipX = spriteFacesRight ? moveDirection > 0 : moveDirection < 0;
    }

    private System.Collections.IEnumerator IdleRoutine()
    {
        isIdling = true;
        isWalking = false;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isWalking", false);

        yield return new WaitForSeconds(idleTime);

        isWalking = true;
        isIdling = false;
    }

    // ------------------ CHASE ------------------
    private void ChasePlayer()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * xVelocity, rb.linearVelocity.y);
        anim.SetBool("isWalking", true);
        sr.flipX = spriteFacesRight ? dir > 0 : dir < 0;
    }

    // ------------------ ATTACK ------------------
    private System.Collections.IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.5f);

        if (Vector2.Distance(transform.position, player.position) > attackRadius)
        {
            isAttacking = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                isAttacking = false;
            }
        }
    }

    // ------------------ DAMAGE & DEATH ------------------
    public override void TakeDamage(int damageValue, DamageType damageType = DamageType.Default)
    {
        if (isDead) return;

        // Play hit animation locally
        anim.SetTrigger("Hit");

        // Apply HP change + death decision (handled in Enemy)
        base.TakeDamage(damageValue, damageType);

        // If base reduced HP to 0, freeze this enemy while death anim plays
        if (CurrentHealth <= 0)
        {
            isDead = true;
            isWalking = false;
            isAttacking = false;
            isTurning = false;
            isIdling = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ------------------ TURN ------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Barrier") && !isDead)
        {
            StartCoroutine(TurnRoutine());
        }
    }

    private System.Collections.IEnumerator TurnRoutine()
    {
        isTurning = true;
        anim.SetTrigger("Turn");

        moveDirection *= -1;
        rb.linearVelocity = new Vector2(moveDirection * xVelocity, rb.linearVelocity.y);

        // brief pause after turning (uses turnPauseTime so the warning goes away)
        yield return new WaitForSeconds(turnPauseTime);

        isTurning = false;
        if (isWalking) anim.SetBool("isWalking", true);
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
