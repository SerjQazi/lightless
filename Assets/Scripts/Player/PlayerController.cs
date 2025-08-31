using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float waterDeathDelay = 5f;

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
    private int facingSign = 1; // 1 = right, -1 = left

    [Header("Jump Settings")]
    public int jumpForce = 8;
    public int jumpLimit = 2;
    private int jumpCount = 0;

    // Stats
    private int _score = 0;
    private int _lives = 3;
    public int maxLives = 9;

    public int Score
    {
        get => _score;
        set => _score = Mathf.Max(0, value);
    }

    public int Lives
    {
        get => _lives;
        set
        {
            if (value < 0)
            {
                Debug.Log("Game Over! No lives left.");
                _lives = 0;
            }
            else if (value > maxLives) _lives = maxLives;
            else _lives = value;
        }
    }

    // Powerups
    private Coroutine jumpForceChange = null;

    public void ActivateJumpForceChange()
    {
        if (jumpForceChange != null)
        {
            StopCoroutine(jumpForceChange);
            jumpForceChange = null;
            jumpForce = 8;
        }
        jumpForceChange = StartCoroutine(ChangeJumpForce());
    }

    private IEnumerator ChangeJumpForce()
    {
        jumpForce = 14;
        Debug.Log("💥 Jump force increased!");
        yield return new WaitForSeconds(5f);
        jumpForce = 8;
        Debug.Log("⏳ Jump force reset.");
        jumpForceChange = null;
    }

    // References
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;
    private GroundCheck groundCheck;
    private Shoot shoot;

    private bool wasGroundedLastFrame = false;
    private bool isDead = false;

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
        if (isDead) return;

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
            StartClawAttack(); // hitbox shows up
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

    private float clawLeftOffset = 0.3f; // tweak in Inspector

    private void UpdateClawHitboxFacing()
    {
        if (clawHitbox == null) return;

        Vector3 pos = clawHitboxDefaultLocalPos;
        pos.x = Mathf.Abs(pos.x) * facingSign;
        if (facingSign == -1)
            pos.x += clawLeftOffset; // nudges it closer when facing left

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

    private void OnTriggerEnter2D(Collider2D collision) => HandleDeathCollision(collision.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => HandleDeathCollision(collision.gameObject);

    private void HandleDeathCollision(GameObject obj)
    {
        if (obj.CompareTag("Death")) StartCoroutine(RespawnAfterDelay(respawnDelay, "Player died!"));
        else if (obj.CompareTag("Water")) StartCoroutine(WaterDeathSequence());
    }

    private IEnumerator WaterDeathSequence()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        animator.SetTrigger("struggleInWater");
        yield return new WaitForSeconds(waterDeathDelay);

        sr.enabled = false;
        transform.position = respawnPoint.position;
        rb.bodyType = RigidbodyType2D.Dynamic;
        sr.enabled = true;
        isDead = false;
    }

    private IEnumerator RespawnAfterDelay(float delay, string deathMessage)
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        sr.enabled = false;
        yield return new WaitForSeconds(delay);

        transform.position = respawnPoint.position;
        rb.bodyType = RigidbodyType2D.Dynamic;
        sr.enabled = true;
        isDead = false;
    }
}
