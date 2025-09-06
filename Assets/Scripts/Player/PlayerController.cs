using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private GameObject clawHitbox;
    [SerializeField] private float clawCooldown = 0.5f;
    public bool IsAttacking { get; private set; } = false;
    private bool canClaw = true;
    private Vector3 clawHitboxDefaultLocalPos;

    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 6f;
    private float currentSpeed;
    private int facingSign = 1;

    [Header("Jump Settings")]
    public int jumpForce = 8;
    public int jumpLimit = 2;
    private int jumpCount = 0;

    // References
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;
    private GroundCheck groundCheck;
    private Shoot shoot;

    private bool wasGroundedLastFrame = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        groundCheck = GetComponentInChildren<GroundCheck>();
        shoot = GetComponent<Shoot>();
        currentSpeed = walkSpeed;

        if (groundCheck == null) Debug.LogError("GroundCheck missing!");
        if (shoot == null) Debug.LogWarning("Shoot component missing!");

        if (clawHitbox != null)
            clawHitboxDefaultLocalPos = clawHitbox.transform.localPosition;
    }

    void Update()
    {
        bool isGrounded = groundCheck != null && groundCheck.IsGrounded;
        float hValue = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(hValue * currentSpeed, rb.linearVelocity.y);

        // Handle facing
        if (hValue > 0f) { facingSign = 1; sr.flipX = false; }
        else if (hValue < 0f) { facingSign = -1; sr.flipX = true; }

        UpdateClawHitboxFacing();

        // Run toggle
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        animator.SetBool("isRolling", Input.GetKey(KeyCode.LeftShift));

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < jumpLimit) Jump();

        // Left-click claw attack
        if (Input.GetMouseButtonDown(0) && canClaw)
        {
            animator.SetTrigger("attack");
            StartClawAttack();
            StartCoroutine(ClawCooldownRoutine());
        }

        // Right-click shoot
        if (Input.GetMouseButtonDown(1) && animator.GetBool("hasSlingShot"))
        {
            animator.SetTrigger("shoot");
            shoot?.Fire();
        }

        // Animator params
        animator.SetFloat("hValue", Mathf.Abs(hValue));
        animator.SetFloat("vValue", rb.linearVelocity.y);
        animator.SetBool("isGrounded", isGrounded);

        // Reset jump on landing
        if (!wasGroundedLastFrame && isGrounded) jumpCount = 0;
        wasGroundedLastFrame = isGrounded;
    }

    private float clawLeftOffset = 0.3f;

    private void UpdateClawHitboxFacing()
    {
        if (clawHitbox == null) return;

        Vector3 pos = clawHitboxDefaultLocalPos;
        pos.x = Mathf.Abs(pos.x) * facingSign;
        if (facingSign == -1)
            pos.x += clawLeftOffset;

        clawHitbox.transform.localPosition = pos;
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpCount++;
        animator.SetTrigger(jumpCount == 1 ? "Jump" : "DoubleJump");
    }

    // Animation events
    public void StartClawAttack()
    {
        IsAttacking = true;
        if (clawHitbox != null) clawHitbox.SetActive(true);
    }

    public void EndClawAttack()
    {
        IsAttacking = false;
        if (clawHitbox != null) clawHitbox.SetActive(false);
    }

    private IEnumerator ClawCooldownRoutine()
    {
        canClaw = false;
        yield return new WaitForSeconds(clawCooldown);
        canClaw = true;
        IsAttacking = false;
    }

    // --- NEW: Collision handling just calls GameManager ---
    private void OnTriggerEnter2D(Collider2D collision) => HandleCollision(collision.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => HandleCollision(collision.gameObject);

    private void HandleCollision(GameObject obj)
    {
        //Debug.Log("Collided with: " + obj.tag);

        if (obj.CompareTag("Death"))
            GameManager.Instance?.HandlePlayerDeath();
        else if (obj.CompareTag("Water"))
            GameManager.Instance?.HandleWaterDeath();
        Debug.Log("Collided with: " + obj.tag);

    }

    // --- Respawn Reset ---
    public void ResetPlayer()
    {
        rb.linearVelocity = Vector2.zero;
        jumpCount = 0;
        animator.SetTrigger("Respawn");
    }

    // -------------------------------------------------
    // TEMP WRAPPERS for compatibility with old scripts
    // -------------------------------------------------
    public int Lives
    {
        get => GameManager.Instance.Lives;
        set => GameManager.Instance.SetLives(value);
    }

    public int Score
    {
        get => GameManager.Instance.Score;
        set => GameManager.Instance.AddScore(value - GameManager.Instance.Score);
    }

    public void ActivateJumpForceChange()
    {
        GameManager.Instance.ActivateJumpForceChange();
    }
}
