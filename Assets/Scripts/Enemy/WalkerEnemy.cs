using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WalkerEnemy : Enemy
{
    private Rigidbody2D rb;
    private Transform player;

    [Header("Movement Settings")]
    [SerializeField, Range(0.5f, 5f)] private float xVelocity = 1.1f;
    [SerializeField] private float turnPauseTime = 0.8f;
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

    private int moveDirection = 1;

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

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

        if (Random.value < idleChance * Time.deltaTime)
        {
            StartCoroutine(IdleRoutine());
        }

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

    // ------------------ DAMAGE ------------------
    public override void TakeDamage(int damageValue, DamageType damageType = DamageType.Default)
    {
        Debug.Log($"WalkerEnemy took {damageValue} damage, type: {damageType}");

        if (damageType == DamageType.Default)
        {
            anim.SetTrigger("Hit");
            base.TakeDamage(damageValue, damageType);
            return;
        }

        if (damageType == DamageType.JumpedOn)
        {
            anim.SetTrigger("Death");
            rb.linearVelocity = Vector2.zero;
            isWalking = false;
            Destroy(transform.parent != null ? transform.parent.gameObject : gameObject, 0.5f);
        }
    }

    // ------------------ TURN ------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Barrier"))
        {
            anim.SetTrigger("Turn");
            moveDirection *= -1;
            rb.linearVelocity = new Vector2(moveDirection * xVelocity, rb.linearVelocity.y);

            if (isWalking)
            {
                anim.SetBool("isWalking", true);
            }
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
