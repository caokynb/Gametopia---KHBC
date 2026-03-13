using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    public static Vector2 respawnPosition;
    public static bool hasCheckpoint = false;

    [Header("Dữ liệu Nhân vật")]
    public PlayerAttributes stats;

    [Header("Cài đặt Dò tia (Raycast)")]
    public float groundCheckWidth = 0.8f;
    public int groundCheckRayCount = 3;
    public float groundCheckDistance = 0.5f;

    public LayerMask groundLayer;
    public LayerMask dirtLayer; // ⭐ THÊM
    public LayerMask bambooLayer;

    [Header("Mở Khóa Kỹ Năng (Team Feature)")]
    public bool canJumpOnBamboo = false;
    private bool isStandingOnBambooOnly;
    private bool hasUsedFloatingJump = false;

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
    private Animator anim;

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

    void Start()
    {
        stats = GetComponent<PlayerAttributes>();
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = stats.normalGravity;

        if (hasCheckpoint) transform.position = respawnPosition;
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (stats.currentBambooCount <= 0 && !isDead) Die();
        if (isDead) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0 && !isFacingRight) Flip();
        else if (moveInput < 0 && isFacingRight) Flip();

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

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        CheckGroundedBambooAndSlope();
        ApplyMovement();
    }

    // Tìm hàm CheckGroundedBambooAndSlope và thay thế logic vòng lặp bằng đoạn này:
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
        float minDistance = float.MaxValue; // Để tìm điểm va chạm gần nhất

        LayerMask combinedLayer = groundLayer | dirtLayer | bambooLayer;
        bool touchingGround = false;
        bool touchingBamboo = false;

        for (int i = 0; i < groundCheckRayCount; i++)
        {
            float xOffset = Mathf.Lerp(-groundCheckWidth / 2, groundCheckWidth / 2, (float)i / (groundCheckRayCount - 1));
            Vector2 rayOrigin = new Vector2(center.x + xOffset, center.y);

            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, rayLength, combinedLayer);

            foreach (RaycastHit2D hit in hits)
            {
                // Chỉ xử lý nếu hit.distance nhỏ nhất (tránh lấy nhầm các đốt tre nằm dưới)
                if (hit.distance < minDistance)
                {
                    minDistance = hit.distance;
                    isGrounded = true;

                    // Kiểm tra va chạm với Ground/Dirt
                    if (((1 << hit.collider.gameObject.layer) & (groundLayer.value | dirtLayer.value)) != 0)
                    {
                        touchingGround = true;
                        validHit = hit;
                    }
                    // Kiểm tra va chạm với Bamboo
                    else if (((1 << hit.collider.gameObject.layer) & bambooLayer.value) != 0)
                    {
                        touchingBamboo = true;
                        validHit = hit;

                        Rigidbody2D bambooRB = hit.collider.attachedRigidbody;
                        if (IsBambooChainGrounded(bambooRB))
                            touchingGround = true;
                    }
                }
            }
        }

        // Logic xử lý Slope và Coyote Time giữ nguyên như cũ của bạn...
        isStandingOnBambooOnly = touchingBamboo && !touchingGround;
        if (touchingGround) hasUsedFloatingJump = false;

        if (isGrounded)
        {
            slopeAngle = Vector2.Angle(validHit.normal, Vector2.up);
            if (slopeAngle > 0.1f)
            {
                slopeNormalPerp = Vector2.Perpendicular(validHit.normal).normalized;
                isOnSlope = slopeAngle <= maxSlopeAngle;
                isSteepSlope = slopeAngle > maxSlopeAngle;
            }
            else
            {
                isOnSlope = false;
                isSteepSlope = false;
            }
        }

        bool hasJumpPermission = !isSteepSlope && (!isStandingOnBambooOnly || (canJumpOnBamboo && !hasUsedFloatingJump));
        if (isGrounded && hasJumpPermission)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.fixedDeltaTime;
    }

    void ApplyMovement()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isSteepSlope)
        {
            if (isStandingOnBambooOnly)
                hasUsedFloatingJump = true;

            rb.gravityScale = stats.normalGravity;
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
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            rb.gravityScale = 0f;
        }
        else
        {
            rb.gravityScale = stats.normalGravity;
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
        }
    }

    void UpdateAnimations()
    {
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            anim.SetBool("isGrounded", isGrounded);

            float verticalSpeed = rb.linearVelocity.y;

            if (Mathf.Abs(verticalSpeed) < 0.05f)
                verticalSpeed = 0f;

            anim.SetFloat("vSpeed", verticalSpeed);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;

        transform.localScale = new Vector3(
            transform.localScale.x * -1,
            transform.localScale.y,
            1
        );
    }

    public void TakeDamage(int damage)
    {
        stats.healthPoint -= damage;

        if (spriteRenderer != null)
            StartCoroutine(FlashRedEffect());

        if (stats.healthPoint <= 0)
            Die();
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

        // ⭐ SỬA: bamboo chạm ground HOẶC dirt
        if (currentRB.IsTouchingLayers(groundLayer | dirtLayer))
            return true;

        List<Collider2D> contacts = new List<Collider2D>();

        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = bambooLayer
        };

        currentRB.GetContacts(filter, contacts);

        foreach (var contact in contacts)
        {
            Rigidbody2D neighbor = contact.attachedRigidbody;

            if (neighbor != null && neighbor != currentRB)
            {
                if (CheckBambooNodeRB(neighbor, visited))
                    return true;
            }
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

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}