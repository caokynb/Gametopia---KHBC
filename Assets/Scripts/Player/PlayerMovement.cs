using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // --- NEW: Bắt buộc phải có để dùng List<> ---

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

    // --- NEW FEATURE: BAMBOO JUMP LOCK ---
    [Header("Mở Khóa Kỹ Năng")]
    public bool canJumpOnBamboo = false;
    private bool isStandingOnBambooOnly;

    // --- NEW FEATURE: ANTI-GLIDING ---
    private bool hasUsedFloatingJump = false; // Token để chống spam nhảy lơ lửng

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

    [Header("Giao diện & Hiệu ứng")]
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private bool isFacingRight = true;

    private Rigidbody2D rb;
    private BoxCollider2D cc;

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

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = stats.normalGravity;

        if (hasCheckpoint)
        {
            transform.position = respawnPosition;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
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

        bool touchingGround = false;
        bool touchingBamboo = false;

        for (int i = 0; i < groundCheckRayCount; i++)
        {
            float xOffset = Mathf.Lerp(-groundCheckWidth / 2, groundCheckWidth / 2, (float)i / (groundCheckRayCount - 1));
            Vector2 rayOrigin = new Vector2(center.x + xOffset, center.y);

            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, rayLength, combinedLayer);

            foreach (RaycastHit2D hit in hits)
            {
                isGrounded = true;

                if (((1 << hit.collider.gameObject.layer) & groundLayer.value) != 0)
                {
                    touchingGround = true;
                    validHit = hit;
                }
                else if (((1 << hit.collider.gameObject.layer) & bambooLayer.value) != 0)
                {
                    touchingBamboo = true;
                    if (!touchingGround) validHit = hit;

                    Rigidbody2D bambooRB = hit.collider.attachedRigidbody;

                    if (IsBambooChainGrounded(bambooRB))
                    {
                        touchingGround = true;
                    }
                }
            }
        }

        // Xác định xem có ĐANG ĐỨNG HOÀN TOÀN TRÊN TRE hay không
        isStandingOnBambooOnly = touchingBamboo && !touchingGround;

        // --- NEW FEATURE: ANTI-GLIDING (NẠP LẠI TOKEN) ---
        // Nếu chạm đất thật (hoặc khối tre an toàn cắm xuống đất), nạp lại quyền nhảy!
        if (touchingGround)
        {
            hasUsedFloatingJump = false;
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

        // --- NEW FEATURE: ANTI-GLIDING (KIỂM TRA QUYỀN NHẢY) ---
        // Cho phép nhảy nếu: Không ở trên dốc đứng VÀ (Đứng an toàn HOẶC (Có kỹ năng VÀ chưa xài token))
        bool hasJumpPermission = !isSteepSlope && (!isStandingOnBambooOnly || (canJumpOnBamboo && !hasUsedFloatingJump));

        if (isGrounded && hasJumpPermission)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.fixedDeltaTime;
    }

    // ==========================================
    // ZONE 6: ACTIONS & MOVEMENT
    // ==========================================
    void ApplyMovement()
    {
        if ((jumpBufferCounter > 0f || isJumpHeld) && coyoteTimeCounter > 0f && !isSteepSlope)
        {
            // --- NEW FEATURE: ANTI-GLIDING (TRỪ TOKEN) ---
            if (isStandingOnBambooOnly)
            {
                hasUsedFloatingJump = true; // Đã xài quyền nhảy lơ lửng, không cho nhảy tiếp cho đến khi chạm đất!
            }

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

    // ==========================================
    // ZONE 8: BAMBOO NETWORK (RIGIDBODY TRAVERSAL)
    // ==========================================
    private bool IsBambooChainGrounded(Rigidbody2D startBambooRB)
    {
        if (startBambooRB == null) return false;

        List<Rigidbody2D> visitedBamboos = new List<Rigidbody2D>();
        return CheckBambooNodeRB(startBambooRB, visitedBamboos);
    }

    private bool CheckBambooNodeRB(Rigidbody2D currentRB, List<Rigidbody2D> visited)
    {
        if (visited.Contains(currentRB)) return false;
        visited.Add(currentRB);

        if (currentRB.IsTouchingLayers(groundLayer)) return true;

        List<Collider2D> contacts = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = bambooLayer;

        currentRB.GetContacts(filter, contacts);

        foreach (Collider2D contact in contacts)
        {
            Rigidbody2D neighborRB = contact.attachedRigidbody;

            if (neighborRB != null && neighborRB != currentRB)
            {
                if (CheckBambooNodeRB(neighborRB, visited))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // ==========================================
    // ZONE: HỆ THỐNG NHẬN SÁT THƯƠNG (COMBAT)
    // ==========================================
    public void TakeDamage(int damageAmount)
    {
        stats.healthPoint -= damageAmount;
        Debug.Log($"<color=red>AU!</color> Anh Khoai bị thương! Máu còn: {stats.healthPoint}");

        // Chớp đỏ
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRedEffect());
        }

        // Kiểm tra chết (Ép tre về 0 để hệ thống tự reset theo logic cũ của bạn)
        if (stats.healthPoint <= 0)
        {
            stats.currentBambooCount = 0;
            Debug.Log("Anh Khoai đã gục ngã!");
        }
    }

    private System.Collections.IEnumerator FlashRedEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f); // Nháy đỏ trong 0.15 giây
        spriteRenderer.color = originalColor;
    }
}