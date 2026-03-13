using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))] // Bắt buộc phải có Animator
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

    [Header("Mở Khóa Kỹ Năng (Team Feature)")]
    public static bool canJumpOnBamboo = false;
    private bool isStandingOnBambooOnly;
    private bool hasUsedFloatingJump = false; // Token chống spam nhảy lơ lửng

    [Header("Cài đặt Dốc (Slope)")]
    public float maxSlopeAngle = 60f;
    public float steepSlideSpeed = 12f;
    public float gentleSlideForce = 3f;

    [Header("Quán tính (Momentum)")]
    public float acceleration = 35f;
    public float deceleration = 45f;

    [Header("Game Feel")]
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;
    public float jumpCutMultiplier = 0.5f;

    [Header("Thời gian Cooldown")]
    public float restartDelay = 1.5f;

    [Header("Giao diện & Hiệu ứng")]
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Animator anim; // Linh kiện Animator từ code của bạn

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
        anim = GetComponent<Animator>(); // Lấy Animator
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = stats.normalGravity;

        if (hasCheckpoint) transform.position = respawnPosition;
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    // ==========================================
    // ZONE 3: INPUT & ANIMATION CALLS
    // ==========================================
    void Update()
    {
        if (stats.currentBambooCount <= 0 && !isDead) Die();
        if (isDead) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0 && !isFacingRight) Flip();
        else if (moveInput < 0 && isFacingRight) Flip();

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

        // Cập nhật Animation mỗi frame (Từ code của bạn)
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        CheckGroundedBambooAndSlope();
        ApplyMovement();
    }

    // ==========================================
    // ZONE 4: SENSORS (Team Logic + Grounded Fix)
    // ==========================================
    void CheckGroundedBambooAndSlope()
    {
        if (Time.time < lastJumpTime + jumpCooldown)
        {
            isGrounded = false;
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
                    if (IsBambooChainGrounded(bambooRB)) touchingGround = true;
                }
            }
        }

        isStandingOnBambooOnly = touchingBamboo && !touchingGround;
        if (touchingGround) hasUsedFloatingJump = false; // Reset token khi chạm đất an toàn

        if (isGrounded)
        {
            slopeAngle = Vector2.Angle(validHit.normal, Vector2.up);
            if (slopeAngle > 0.1f)
            {
                slopeNormalPerp = Vector2.Perpendicular(validHit.normal).normalized;
                isOnSlope = slopeAngle <= maxSlopeAngle;
                isSteepSlope = slopeAngle > maxSlopeAngle;
            }
            else { isOnSlope = isSteepSlope = false; }
        }

        // Kiểm tra quyền nhảy (Team Logic)
        bool hasJumpPermission = !isSteepSlope && (!isStandingOnBambooOnly || (canJumpOnBamboo && !hasUsedFloatingJump));

        if (isGrounded && hasJumpPermission)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.fixedDeltaTime;
    }

    // ==========================================
    // ZONE 5: MOVEMENT (Combined Logic)
    // ==========================================
    void ApplyMovement()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isSteepSlope)
        {
            if (isStandingOnBambooOnly) hasUsedFloatingJump = true;

            rb.gravityScale = stats.normalGravity; // Reset gravity khi nhảy (Code của bạn)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            lastJumpTime = Time.time;
            isGrounded = false;
            return;
        }

        float targetSpeed = moveInput * stats.moveSpeed;
        float accelRate = (Mathf.Abs(moveInput) > 0) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

        if (isGrounded && isSteepSlope)
        {
            Vector2 slideDir = slopeNormalPerp.y < 0 ? slopeNormalPerp : -slopeNormalPerp;
            rb.gravityScale = stats.normalGravity;
            rb.linearVelocity = slideDir * steepSlideSpeed;
        }
        else if (isGrounded && moveInput == 0f && Mathf.Abs(currentSpeed) < 0.1f && !isOnSlope)
        {
            // Fix đứng yên không bị trượt (Code của bạn)
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
    // ZONE 6: ANIMATION SYSTEM (From your code)
    // ==========================================
    void UpdateAnimations()
    {
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            anim.SetBool("isGrounded", isGrounded);

            float verticalSpeed = rb.linearVelocity.y;
            if (Mathf.Abs(verticalSpeed) < 0.05f) verticalSpeed = 0f;
            anim.SetFloat("vSpeed", verticalSpeed);

            // Animator luôn phải cập nhật biến isAttackMode từ Manager mỗi khung hình
            anim.SetBool("isAttackMode", PlayerModeManager.isAttackMode);
        }
    }

    // ==========================================
    // ZONE 7: HELPERS & COMBAT (Team Features)
    // ==========================================
    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
    }

    public void TakeDamage(int damage)
    {
        stats.healthPoint -= damage;
        if (spriteRenderer != null) StartCoroutine(FlashRedEffect());
        if (stats.healthPoint <= 0) Die();
    }

    private IEnumerator<WaitForSeconds> FlashRedEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;
    }

    private bool IsBambooChainGrounded(Rigidbody2D startRB)
    {
        if (startRB == null) return false;
        List<Rigidbody2D> visited = new List<Rigidbody2D>();
        return CheckBambooNodeRB(startRB, visited);
    }

    private bool CheckBambooNodeRB(Rigidbody2D currentRB, List<Rigidbody2D> visited)
    {
        if (visited.Contains(currentRB)) return false;
        visited.Add(currentRB);
        if (currentRB.IsTouchingLayers(groundLayer)) return true;

        List<Collider2D> contacts = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D { useLayerMask = true, layerMask = bambooLayer };
        currentRB.GetContacts(filter, contacts);

        foreach (var contact in contacts)
        {
            Rigidbody2D neighbor = contact.attachedRigidbody;
            if (neighbor != null && neighbor != currentRB && CheckBambooNodeRB(neighbor, visited)) return true;
        }
        return false;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        Invoke(nameof(RestartLevel), restartDelay);
    }

    void RestartLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    /* // ==========================================
    // ZONE 8: DEBUG & GIZMOS
    // ==========================================
    private void OnDrawGizmosSelected()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null) return;

        Vector2 center = collider.bounds.center;
        float rayLength = collider.bounds.extents.y + groundCheckDistance;

        Gizmos.color = Color.red; // Màu tia laser

        // Vẽ chính xác các tia raycast đang được dùng trong CheckGroundedBambooAndSlope
        for (int i = 0; i < groundCheckRayCount; i++)
        {
            float xOffset = Mathf.Lerp(-groundCheckWidth / 2, groundCheckWidth / 2, (float)i / (groundCheckRayCount - 1));
            Vector2 rayOrigin = new Vector2(center.x + xOffset, center.y);

            // Vẽ đường thẳng từ chân xuống đất
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * rayLength);
        }
    } */
}