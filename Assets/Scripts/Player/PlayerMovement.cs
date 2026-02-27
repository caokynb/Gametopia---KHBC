using UnityEngine;
using UnityEngine.SceneManagement; // NEW: Thư viện để load lại màn chơi

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    // ==========================================
    // ZONE 1: VARIABLES & SETTINGS
    // ==========================================

    // --- NEW: Hệ thống Checkpoint (Static để sống sót qua Scene Reload) ---
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

    // --- NEW: Các thông số Quán tính (Momentum) ---
    [Header("Quán tính (Momentum)")]
    public float acceleration = 35f;  // Tốc độ đạp ga (Số càng lớn, tăng tốc càng nhanh)
    public float deceleration = 45f;  // Tốc độ bóp phanh (Số càng lớn, dừng càng khựng)

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
    private float currentSpeed; // NEW: Lưu tốc độ ngang thực tế đang thay đổi mượt mà
    private bool isJumpHeld;    // NEW: Cờ kiểm tra đè nút nhảy
    private bool isDead = false; // NEW: Cờ xác nhận đã chết chưa

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

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = stats.normalGravity;

        // --- NEW: Dịch chuyển người chơi đến Checkpoint nếu có ---
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

        // --- NEW: Lấy tín hiệu giữ nút nhảy (Auto-hop) ---
        isJumpHeld = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space);

        // --- Jump Buffering (Tapping) ---
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // --- Short Hop (Thả tay sớm) ---
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Space))
        {
            if (rb.linearVelocity.y > 0)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        // Debug Checkpoint
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

        // --- NEW UPDATE: Gộp Layer Ground và Bamboo ---
        // Sử dụng toán tử Bitwise (|) để tạo ra một LayerMask gộp cả 2
        LayerMask combinedLayer = groundLayer | bambooLayer;

        for (int i = 0; i < groundCheckRayCount; i++)
        {
            float xOffset = Mathf.Lerp(-groundCheckWidth / 2, groundCheckWidth / 2, (float)i / (groundCheckRayCount - 1));
            Vector2 rayOrigin = new Vector2(center.x + xOffset, center.y);

            // Bắn tia laser kiểm tra CẢ ĐẤT LẪN TRE cùng một lúc
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
        // A. XỬ LÝ NHẢY (Bao gồm cả Auto-Hop bằng isJumpHeld)
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

        // --- NEW: TÍNH TOÁN QUÁN TÍNH ---
        // Xác định tốc độ người chơi MUỐN đạt được
        float targetSpeed = moveInput * stats.moveSpeed;

        // Chọn dùng gia tốc (nếu đang giữ phím) hay dùng lực phanh (nếu đã buông phím)
        float accelRate = (Mathf.Abs(moveInput) > 0) ? acceleration : deceleration;

        // Di chuyển dần currentSpeed về phía targetSpeed một cách mượt mà
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);


        // B. XỬ LÝ DỐC ĐỨNG
        if (isGrounded && isSteepSlope)
        {
            Vector2 slideDownDirection = slopeNormalPerp.y < 0 ? slopeNormalPerp : -slopeNormalPerp;
            rb.gravityScale = stats.normalGravity;
            rb.linearVelocity = slideDownDirection * steepSlideSpeed;
        }

        // C. XỬ LÝ LEO DỐC THOẢI
        else if (isGrounded && isOnSlope)
        {
            Vector2 slideDownDirection = slopeNormalPerp.y < 0 ? slopeNormalPerp : -slopeNormalPerp;
            rb.gravityScale = stats.normalGravity;

            // Truyền currentSpeed vào thay vì moveInput để áp dụng quán tính trên dốc!
            Vector2 moveVelocity = new Vector2(-currentSpeed * slopeNormalPerp.x,
                                               -currentSpeed * slopeNormalPerp.y);

            // Chỉ trượt xuống khi thả tay VÀ đã phanh dừng hẳn
            if (moveInput == 0f && Mathf.Abs(currentSpeed) < 0.1f)
            {
                rb.linearVelocity = slideDownDirection * gentleSlideForce;
            }
            else
            {
                rb.linearVelocity = moveVelocity + (slideDownDirection * gentleSlideForce);
            }
        }

        // D. ĐỨNG YÊN TRÊN ĐƯỜNG PHẲNG (Đã phanh dừng hẳn)
        else if (isGrounded && moveInput == 0f && Mathf.Abs(currentSpeed) < 0.1f && !isOnSlope && !isSteepSlope)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            rb.gravityScale = 0f;
        }

        // E. CHẠY THƯỜNG / BAY TRÊN KHÔNG
        else
        {
            rb.gravityScale = stats.normalGravity;
            // Áp dụng currentSpeed mượt mà vào trục X
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
    // --- NEW: HỆ THỐNG CHẾT VÀ HỒI SINH ---
    void Die()
    {
        isDead = true;

        // Ép vận tốc về 0 để nhân vật không trượt đi tiếp
        rb.linearVelocity = Vector2.zero;

        // Tắt mô phỏng vật lý để nhân vật không bị rơi xuyên qua sàn
        rb.simulated = false;

        Debug.Log("Hết Bamboo! Nhân vật đã chết.");

        // Gọi hàm RestartLevel sau restartDelay giây
        Invoke(nameof(RestartLevel), restartDelay);
    }

    void RestartLevel()
    {
        // Load lại chính xác màn chơi hiện tại (Reset toàn bộ)
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