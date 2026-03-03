using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
// Bắt buộc phải có Animator gắn trên người
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    // ==========================================
    // ZONE 1: VARIABLES & SETTINGS
    // ==========================================

    public static Vector2 respawnPosition;
    public static bool hasCheckpoint = false;

    [Header("Dữ liệu Nhân vật")]
    public PlayerAttributes stats;

    [Header("Cài đặt Dò tia (Raycast)")]
    public float groundCheckWidth = 0.8f;
    public int groundCheckRayCount = 3;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;
    public LayerMask bambooLayer;

    [Header("Cài đặt Dốc (Slope)")]
    public float maxSlopeAngle = 60f;
    public float steepSlideSpeed = 12f;
    public float gentleSlideForce = 3f;

    [Header("Quán tính (Momentum)")]
    public float acceleration = 35f;
    public float deceleration = 45f;

    [Header("Game Feel (Cảm giác bay nhảy)")]
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;
    public float jumpCutMultiplier = 0.5f;

    [Header("Thời gian Cooldown Restart Level")]
    public float restartDelay = 1.5f;

    private bool isFacingRight = true;

    // --- Linh kiện (Components) ---
    private Rigidbody2D rb;
    private BoxCollider2D cc;
    private Animator anim; // Khai báo biến Animator

    // --- Trạng thái (States) ---
    private float moveInput;
    private float currentSpeed;
    private bool isJumpHeld;
    private bool isDead = false;

    private bool isGrounded;
    private bool isOnSlope;
    private bool isSteepSlope;
    private float slopeAngle;
    private Vector2 slopeNormalPerp;

    private float lastJumpTime;
    private float jumpCooldown = 0.1f;

    // ==========================================
    // ZONE 2: INITIALIZATION
    // ==========================================
    void Start()
    {
        stats = GetComponent<PlayerAttributes>();
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<BoxCollider2D>();

        anim = GetComponent<Animator>(); // Lấy linh kiện Animator từ nhân vật

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = stats.normalGravity;

        if (hasCheckpoint)
        {
            transform.position = respawnPosition;
        }
    }

    // ==========================================
    // ZONE 3: INPUT (Runs every frame)
    // ==========================================
    void Update()
    {
        if (stats.currentBambooCount <= 0 && !isDead)
        {
            Die();
        }
        if (isDead) return;
        moveInput = Input.GetAxisRaw("Horizontal");

        // --- Lật mặt ---
        if (moveInput > 0 && !isFacingRight)
            Flip();
        else if (moveInput < 0 && isFacingRight)
            Flip();

        isJumpHeld = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Space))
        {
            if (rb.linearVelocity.y > 0)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            stats.currentBambooCount = 0;
            Debug.Log("Bamboo Cleared!");
        }

        // Gọi hàm cập nhật hình ảnh mỗi khung hình
        UpdateAnimations();
    }

    // ==========================================
    // ZONE 4: PHYSICS (Runs on a fixed timer)
    // ==========================================
    void FixedUpdate()
    {
        CheckGroundedBambooAndSlope();
        ApplyMovement();
    }

    // ==========================================
    // ZONE 5: SENSORS & MATH
    // ==========================================
    void CheckGroundedBambooAndSlope()
    {
        if (Time.time < lastJumpTime + jumpCooldown)
        {
            isGrounded = false;
            isOnSlope = false;
            isSteepSlope = false;
            coyoteTimeCounter = 0f;
            return;
        }

        Vector2 center = cc.bounds.center;
        float rayLength = cc.bounds.extents.y + groundCheckDistance;

        isGrounded = false;
        RaycastHit2D validHit = new RaycastHit2D();

        LayerMask combinedLayer = groundLayer | bambooLayer;

        for (int i = 0; i < groundCheckRayCount; i++)
        {
            float xOffset = Mathf.Lerp(-groundCheckWidth / 2, groundCheckWidth / 2, (float)i / (groundCheckRayCount - 1));
            Vector2 rayOrigin = new Vector2(center.x + xOffset, center.y);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, combinedLayer);

            if (hit)
            {
                isGrounded = true;
                validHit = hit;
                break;
            }
        }

        if (isGrounded)
        {
            slopeAngle = Vector2.Angle(validHit.normal, Vector2.up);

            if (slopeAngle > 0.1f)
            {
                slopeNormalPerp = Vector2.Perpendicular(validHit.normal).normalized;

                if (slopeAngle <= maxSlopeAngle)
                {
                    isOnSlope = true;
                    isSteepSlope = false;
                }
                else
                {
                    isOnSlope = false;
                    isSteepSlope = true;
                }
            }
            else
            {
                isOnSlope = false;
                isSteepSlope = false;
            }
        }
        else
        {
            isOnSlope = false;
            isSteepSlope = false;
        }

        if (isGrounded && !isSteepSlope)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.fixedDeltaTime;
    }

    // ==========================================
    // ZONE 6: ACTIONS & MOVEMENT
    // ==========================================
    void ApplyMovement()
    {
        // [SỬA LỖI 1]: Bỏ "|| isJumpHeld" để tránh nhân vật bị cộng dồn lực khi đè phím
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isSteepSlope)
        {
            // [SỬA LỖI 2]: Bắt buộc mở lại trọng lực NGAY LẬP TỨC để physics hoạt động
            rb.gravityScale = stats.normalGravity;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            lastJumpTime = Time.time;
            isGrounded = false;
            isOnSlope = false;
            isSteepSlope = false;
            return;
        }

        float targetSpeed = moveInput * stats.moveSpeed;
        float accelRate = (Mathf.Abs(moveInput) > 0) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

        if (isGrounded && isSteepSlope)
        {
            Vector2 slideDownDirection = slopeNormalPerp.y < 0 ? slopeNormalPerp : -slopeNormalPerp;
            rb.gravityScale = stats.normalGravity;
            rb.linearVelocity = slideDownDirection * steepSlideSpeed;
        }
        else if (isGrounded && isOnSlope)
        {
            Vector2 slideDownDirection = slopeNormalPerp.y < 0 ? slopeNormalPerp : -slopeNormalPerp;
            rb.gravityScale = stats.normalGravity;

            Vector2 moveVelocity = new Vector2(-currentSpeed * slopeNormalPerp.x,
                                               -currentSpeed * slopeNormalPerp.y);

            if (moveInput == 0f && Mathf.Abs(currentSpeed) < 0.1f)
            {
                rb.linearVelocity = slideDownDirection * gentleSlideForce;
            }
            else
            {
                rb.linearVelocity = moveVelocity + (slideDownDirection * gentleSlideForce);
            }
        }
        else if (isGrounded && moveInput == 0f && Mathf.Abs(currentSpeed) < 0.1f && !isOnSlope && !isSteepSlope)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            rb.gravityScale = 0f;
        }
        else
        {
            rb.gravityScale = stats.normalGravity;
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
        }
    }

    // ==========================================
    // ZONE 7: HELPER FUNCTIONS
    // ==========================================

    void UpdateAnimations()
    {
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            anim.SetBool("isGrounded", isGrounded);

            // [SỬA LỖI 3]: Lọc bớt nhiễu số thập phân để đảm bảo vSpeed nhảy số dứt khoát
            float verticalSpeed = rb.linearVelocity.y;
            if (Mathf.Abs(verticalSpeed) < 0.05f)
            {
                verticalSpeed = 0f;
            }

            anim.SetFloat("vSpeed", verticalSpeed);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        Debug.Log("Hết Bamboo! Nhân vật đã chết.");
        Invoke(nameof(RestartLevel), restartDelay);
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnDrawGizmos()
    {
        if (cc == null) cc = GetComponent<BoxCollider2D>();
        if (cc != null && groundCheckRayCount > 1)
        {
            Gizmos.color = Color.yellow;
            Vector2 center = cc.bounds.center;
            float height = cc.bounds.extents.y + groundCheckDistance;

            for (int i = 0; i < groundCheckRayCount; i++)
            {
                float xOffset = Mathf.Lerp(-groundCheckWidth / 2, groundCheckWidth / 2, (float)i / (groundCheckRayCount - 1));
                Vector2 start = new Vector2(center.x + xOffset, center.y);
                Vector2 end = new Vector2(center.x + xOffset, center.y - height);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}