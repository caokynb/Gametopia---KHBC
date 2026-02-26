using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
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

    // --- NEW UPDATE: WALL CHECK ---
    private bool isTouchingWall;

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

        if (moveInput > 0 && !isFacingRight)
            Flip();
        else if (moveInput < 0 && isFacingRight)
            Flip();

        isJumpHeld = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
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

        // --- NEW UPDATE: WALL CHECK (CẢM BIẾN BỤNG & CHÂN) ---
        Vector2 wallCheckDir = isFacingRight ? Vector2.right : Vector2.left;
        float wallCheckDistance = cc.bounds.extents.x + 0.15f;

        Vector2 waistOrigin = center;
        Vector2 feetOrigin = new Vector2(center.x, cc.bounds.min.y + 0.05f);

        RaycastHit2D wallHitWaist = Physics2D.Raycast(waistOrigin, wallCheckDir, wallCheckDistance, combinedLayer);
        RaycastHit2D wallHitFeet = Physics2D.Raycast(feetOrigin, wallCheckDir, wallCheckDistance, combinedLayer);

        isTouchingWall = false;

        if (wallHitWaist && Vector2.Angle(wallHitWaist.normal, Vector2.up) > maxSlopeAngle)
        {
            isTouchingWall = true;
        }
        else if (wallHitFeet && Vector2.Angle(wallHitFeet.normal, Vector2.up) > maxSlopeAngle)
        {
            isTouchingWall = true;
        }
    }

    // ==========================================
    // ZONE 6: ACTIONS & MOVEMENT
    // ==========================================
    void ApplyMovement()
    {
        if ((jumpBufferCounter > 0f || isJumpHeld) && coyoteTimeCounter > 0f && !isSteepSlope)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            lastJumpTime = Time.time;
            isGrounded = false;
            isOnSlope = false;
            isSteepSlope = false;
            return;
        }

        // --- NEW UPDATE: WALL CHECK MATH ---
        float targetSpeed = moveInput * stats.moveSpeed;

        // Nếu đang cắm mặt vào tường/vách đứng và cố tình đi tiếp về hướng đó -> CẮT ĐỘNG CƠ!
        if (isTouchingWall && ((isFacingRight && moveInput > 0) || (!isFacingRight && moveInput < 0)))
        {
            targetSpeed = 0f;
            currentSpeed = 0f; // Triệt tiêu quán tính ngay lập tức để không bị leo lên vách!
        }

        float accelRate = (Mathf.Abs(moveInput) > 0) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);


        // B. XỬ LÝ DỐC ĐỨNG
        if (isGrounded && isSteepSlope)
        {
            currentSpeed = 0f; // Thêm 1 lớp bảo vệ: Không cho phép có quán tính ngang khi đang đứng trên dốc đứng
            Vector2 slideDownDirection = slopeNormalPerp.y < 0 ? slopeNormalPerp : -slopeNormalPerp;
            rb.gravityScale = stats.normalGravity;
            rb.linearVelocity = slideDownDirection * steepSlideSpeed;
        }

        // C. XỬ LÝ LEO DỐC THOẢI
        else if (isGrounded && isOnSlope)
        {
            Vector2 slideDownDirection = slopeNormalPerp.y < 0 ? slopeNormalPerp : -slopeNormalPerp;
            rb.gravityScale = stats.normalGravity;

            Vector2 moveVelocity = new Vector2(-currentSpeed * slopeNormalPerp.x, -currentSpeed * slopeNormalPerp.y);

            if (moveInput == 0f && Mathf.Abs(currentSpeed) < 0.1f)
            {
                rb.linearVelocity = slideDownDirection * gentleSlideForce;
            }
            else
            {
                rb.linearVelocity = moveVelocity + (slideDownDirection * gentleSlideForce);
            }
        }

        // D. ĐỨNG YÊN TRÊN ĐƯỜNG PHẲNG
        else if (isGrounded && moveInput == 0f && Mathf.Abs(currentSpeed) < 0.1f && !isOnSlope && !isSteepSlope)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            rb.gravityScale = 0f;
        }

        // E. CHẠY THƯỜNG / BAY TRÊN KHÔNG
        else
        {
            rb.gravityScale = stats.normalGravity;
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
        }
    }

    // ==========================================
    // ZONE 7: HELPER FUNCTIONS
    // ==========================================
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

        // Vẽ tia đất
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

            // Vẽ 2 tia tường (Bụng và Chân)
            Gizmos.color = Color.red;
            float wallCheckDistance = cc.bounds.extents.x + 0.05f;
            Vector2 wallCheckDir = isFacingRight ? Vector2.right : Vector2.left;

            Vector2 waistOrigin = center;
            Vector2 feetOrigin = new Vector2(center.x, cc.bounds.min.y);

            Gizmos.DrawLine(waistOrigin, new Vector2(waistOrigin.x + (wallCheckDir.x * wallCheckDistance), waistOrigin.y));
            Gizmos.DrawLine(feetOrigin, new Vector2(feetOrigin.x + (wallCheckDir.x * wallCheckDistance), feetOrigin.y));
        }
    }
}