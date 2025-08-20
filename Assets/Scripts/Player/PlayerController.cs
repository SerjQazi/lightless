using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Require basic player components so Unity auto-adds them if missing
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float waterDeathDelay = 5f;

    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 6f;
    private float currentSpeed;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public int jumpLimit = 2;
    private int jumpCount = 0;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded;

    // Private references
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;

    private bool wasGroundedLastFrame = false;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck not assigned! Creating one automatically.");
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            groundCheck = gc.transform;
        }

        currentSpeed = walkSpeed;
    }

    void Update()
    {
        if (isDead) return;

        CheckIfGrounded();

        float hValue = Input.GetAxisRaw("Horizontal");
        float vValue = Input.GetAxisRaw("Vertical");

        rb.linearVelocity = new Vector2(hValue * currentSpeed, rb.linearVelocity.y);

        if (hValue < 0) sr.flipX = true;
        else if (hValue > 0) sr.flipX = false;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = runSpeed;
            animator.SetBool("isRolling", true);
        }
        else
        {
            currentSpeed = walkSpeed;
            animator.SetBool("isRolling", false);
        }

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < jumpLimit)
        {
            Jump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("🔪 Player attack animation would play here.");
            animator.SetBool("attack", true); // Trigger the attack animation
        }
        else if (Input.GetMouseButtonUp(0))
        {
            animator.SetBool("attack", false); // Trigger the shoot animation
        }

        // set horizontal and vertical movement animation
        animator.SetFloat("hValue", Mathf.Abs(hValue));
        animator.SetFloat("vValue", rb.linearVelocity.y);
        // set grounded animation state
        animator.SetBool("isGrounded", isGrounded);
    }

    //void CheckIfGrounded()
    //{
    //    bool groundedNow = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    //    isGrounded = groundedNow;

    //    if (!wasGroundedLastFrame && groundedNow)
    //    {
    //        Debug.Log("✅ Player landed (would play 'Land' animation).");
    //        jumpCount = 0;
    //    }

    //    wasGroundedLastFrame = groundedNow;
    //}
    void CheckIfGrounded()
    {
        bool groundedNow = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = groundedNow;

        animator.SetBool("isGrounded", groundedNow);

        if (!wasGroundedLastFrame && groundedNow)
        {
            Debug.Log("✅ Player landed (would play 'Land' animation if added).");
            jumpCount = 0;
        }

        wasGroundedLastFrame = groundedNow;
    }


    //void Jump()
    //{
    //    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    //    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    //    jumpCount++;
    //    Debug.Log("🦘 Player jumped! Jump count: " + jumpCount + " (would play 'Jump' animation).");
    //}

    void Jump()
    {
        // Reset vertical velocity before applying jump force
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        jumpCount++;

        if (jumpCount == 1)
        {
            animator.SetTrigger("Jump");
            Debug.Log("🦘 Normal Jump animation triggered.");
        }
        else if (jumpCount == 2)
        {
            animator.SetTrigger("DoubleJump");
            Debug.Log("🔁 Double Jump animation triggered.");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleDeathCollision(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleDeathCollision(collision.gameObject);
    }

    private void HandleDeathCollision(GameObject obj)
    {
        if (obj.CompareTag("Death"))
        {
            Debug.Log("💀 Player hit a death collider! (would play 'Die' animation).");
            StartCoroutine(RespawnAfterDelay(respawnDelay, "Player died!"));
        }
        else if (obj.CompareTag("Water"))
        {
            Debug.Log("🌊 Player touched water! (would play 'Struggle' animation).");
            StartCoroutine(WaterDeathSequence());
        }
    }

    private IEnumerator WaterDeathSequence()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Debug.Log("🎥 Animation Trigger: Struggle in water");

        yield return new WaitForSeconds(waterDeathDelay);

        Debug.Log("🎥 Animation Trigger: Drowning");

        sr.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        transform.position = respawnPoint.position;
        rb.bodyType = RigidbodyType2D.Dynamic;
        sr.enabled = true;

        isDead = false;
        Debug.Log("🔄 Player respawned!");
    }

    private IEnumerator RespawnAfterDelay(float delay, string deathMessage)
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Debug.Log("🎥 Animation Trigger: Die");

        Debug.Log(deathMessage);

        sr.enabled = false;

        yield return new WaitForSeconds(delay);

        transform.position = respawnPoint.position;
        rb.bodyType = RigidbodyType2D.Dynamic;
        sr.enabled = true;

        isDead = false;
        Debug.Log("🔄 Player respawned!");
    }
}
