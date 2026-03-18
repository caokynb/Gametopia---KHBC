using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections; // Bắt buộc thêm để dùng Coroutine

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
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

    [Header("Cài đặt iFrames")]
    public float iFrameDuration = 1.5f;
    private bool isInvincible = false;


    [Header("Mở Khóa Kỹ Năng")]
    public static bool hasDiscountBuff = false;
    public static bool canJumpOnBamboo = false;
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

    // --- NEW: Biến khóa trạng thái khi đang thắp hương ---
    private bool isInteracting = false;

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
    void Awake()
    {
        // Đưa toàn bộ GetComponent lên Awake để lấy dữ liệu ngay giây 0
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Start chỉ dùng để cài đặt thông số sau khi Component đã lấy xong
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Thêm an toàn: Kiểm tra stats trước khi gán để chống crash cục bộ
        if (stats != null)
        {
            rb.gravityScale = stats.normalGravity;
        }

        if (hasCheckpoint) transform.position = respawnPosition;
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    // ==========================================
    // ZONE 3: INPUT & ANIMATION CALLS
    // ==========================================
    void Update()
    {
        if (DialogueManager.instance != null && DialogueManager.instance.isDialogueActive)
        {
            // Dừng hẳn nhân vật lại (Dùng linearVelocity vì bạn đang dùng Unity bản mới)
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            // Nếu bạn có Animator, bỏ comment dòng dưới để set tốc độ về 0 (đứng im)
            // GetComponent<Animator>().SetFloat("Speed", 0f); 

            return; // Lệnh return này sẽ ngăn các dòng code di chuyển bên dưới chạy!
        }
        if (stats.currentBambooCount <= 0 && !isDead) Die();

        // CHỈNH SỬA: Nếu đang chết HOẶC đang thắp hương thì không nhận Input
        if (isDead || isInteracting) return;

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
        // Nếu đang thắp hương, dừng mọi vật lý di chuyển ngang
        if (isInteracting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        CheckGroundedBambooAndSlope();
        ApplyMovement();
    }

    // ==========================================
    // ZONE 4: SENSORS & PHYSICS (Giữ nguyên logic cũ)
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
            else { isOnSlope = isSteepSlope = false; }
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
            if (isStandingOnBambooOnly) hasUsedFloatingJump = true;
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

    // ==========================================
    // ZONE 6: ANIMATION SYSTEM
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
            anim.SetBool("isAttackMode", PlayerModeManager.isAttackMode);
        }
    }

    // ==========================================
    // ZONE 7: INTERACTION & HELPERS
    // ==========================================

    // Hàm gọi từ Checkpoint.cs
    public void TriggerWishAnimation(float duration)
    {
        StartCoroutine(WishSequence(duration));
    }

    private IEnumerator WishSequence(float duration)
    {
        isInteracting = true;
        // Dừng vận tốc ngang ngay lập tức
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (anim != null)
        {
            // Kích hoạt Trigger Wish trong Animator
            anim.SetTrigger("Wish");
        }

        yield return new WaitForSeconds(duration);
        isInteracting = false;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
    }

    public void TakeDamage(int damage)
    {
        // Nếu đang bất tử hoặc đã chết thì không nhận thêm sát thương
        if (isInvincible || isDead) return;

        stats.healthPoint -= damage;

        if (stats.healthPoint <= 0)
        {
            Die();
        }
        else
        {
            // Bật trạng thái bất tử và bắt đầu nhấp nháy
            StartCoroutine(InvincibilityRoutine());
        }
    }

    // --- COROUTINE NHẤP NHÁY KIỂU TERRARIA ---
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        float blinkInterval = 0.1f; // Tốc độ chớp tắt

        while (elapsed < iFrameDuration)
        {
            // Tắt hình
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(blinkInterval);

            // Bật hình
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(blinkInterval);

            elapsed += (blinkInterval * 2);
        }

        // Đảm bảo hiện hình 100% khi hết thời gian
        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    private IEnumerator FlashRedEffect()
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
        rb.simulated = false; // Tắt vật lý để không rơi xuyên đất

        // --- 1. KÍCH HOẠT ANIMATION CHẾT ---
        if (anim != null)
        {
            // Xóa dòng anim.speed = 0f đi để nhân vật được cử động
            anim.SetTrigger("Die");
        }

        // --- 2. HẸN GIỜ LÀM MỜ MÀN HÌNH ---
        // Cho Anh Khoai 1 giây để diễn cảnh ngã xuống, sau đó mới gọi hàm làm đen màn hình
        Invoke(nameof(CallFader), 0.1f);
    }

    // Hàm phụ trợ để gọi tấm màn đen sau khi đã diễn xong animation
    void CallFader()
    {
        SceneFader fader = Object.FindFirstObjectByType<SceneFader>();
        if (fader != null)
        {
            fader.FadeOutAndRestart();
        }
        else
        {
            RestartLevel();
        }
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