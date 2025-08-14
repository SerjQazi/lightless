using UnityEngine;

// Require basic player components so Unity auto-adds them if missing
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;       
    public float runSpeed = 6f;       
    private float currentSpeed;

    [Header("Jump Settings")]
    public float jumpForce = 8f;    
    public int jumpLimit = 2;          
    private int jumpCount = 0;         

    [Header("Ground Check Settings")]
    public Transform groundCheck;      // Empty object under player feet
    public float groundCheckRadius = 0.2f; // Circle radius for ground detection
    public LayerMask groundLayer;      // What counts as "ground"
    public bool isGrounded;            // Shows in Inspector if grounded

    // Private references
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator; // Animator exists but unused until animations are ready

    private bool wasGroundedLastFrame = false; // For detecting landing

    void Start()
    {
        // Get components from the GameObject
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Start with walking speed
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        // --- 1. Ground Check ---
        CheckIfGrounded();

        // --- 2. Handle Movement ---
        float hValue = Input.GetAxisRaw("Horizontal"); // -1 (left), 0 (idle), 1 (right)
        rb.linearVelocity = new Vector2(hValue * currentSpeed, rb.linearVelocity.y);

        // Flip sprite left/right
        if (hValue < 0) sr.flipX = true;
        else if (hValue > 0) sr.flipX = false;

        // --- 3. Run toggle ---
        if (Input.GetKey(KeyCode.LeftShift))
            currentSpeed = runSpeed;
        else
            currentSpeed = walkSpeed;

        // --- 4. Jump ---
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < jumpLimit)
        {
            Jump();
        }

        // --- 5. Attack ---
        if (Input.GetMouseButtonDown(0)) // Left mouse click
        {
            Debug.Log("Player attack!");
            // animator.SetTrigger("Attack"); // Commented out until animations are ready
        }
    }

    void CheckIfGrounded()
    {
        // Detect if touching ground using OverlapCircle
        bool groundedNow = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Update Inspector variable
        isGrounded = groundedNow;

        // Detect landing (was NOT grounded last frame, but IS grounded this frame)
        if (!wasGroundedLastFrame && groundedNow)
        {
            Debug.Log("Player landed on the ground.");
            jumpCount = 0; // Reset jump count when grounded
        }

        // Save current grounded state for next frame
        wasGroundedLastFrame = groundedNow;
    }

    void Jump()
    {
        // Reset vertical velocity first so jumps are consistent
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Apply upward force
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        jumpCount++; // Increase jump count
        Debug.Log("Player jumped! Jump count: " + jumpCount);

        // animator.SetTrigger("Jump"); // Commented out until animations are ready
    }

    void OnDrawGizmosSelected()
    {
        // Draw ground check radius in Scene view for easier debugging
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
