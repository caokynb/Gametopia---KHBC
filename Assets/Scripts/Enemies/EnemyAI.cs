using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    [Header("Loại Quái")]
    public bool isTiger = false;
    public bool isMonkey = false;
    public bool isBat = false;
    public bool isBoyOnBuffalo = false;
    [Tooltip("Tích vào đây nếu là Bọ Ngựa (Đi tuần & Lướt chém tàng hình 3 lần)")]
    public bool isMantis = false;

    [Header("Chỉ số cơ bản")]
    public int health = 5;
    public float chaseSpeed = 4f;
    public float returnSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1.2f;

    [Header("Cảm biến Chạm (Đi xuyên)")]
    private float touchCooldownTimer = 0f;

    [Header("Cấu hình Đẩy lùi")]
    public float knockbackForce = 12f;
    public float knockbackDuration = 0.25f;

    [Header("Cấu hình Tiger Jump")]
    public float tigerJumpForwardForce = 6f;
    public float tigerJumpUpwardForce = 5f;
    public float tigerAttackCooldown = 1.5f;

    [Header("Cấu hình Monkey Throw")]
    public GameObject rockPrefab;
    public Transform throwPoint;
    public int rocksToThrow = 3;
    public float delayBetweenRocks = 0.3f;
    public float rockThrowForce = 8f;
    public float monkeyAttackCooldown = 2.5f;

    [Header("Cấu hình Bat Flight")]
    public float batWobbleSpeed = 6f;
    public float batWobbleAmount = 2f;
    public float hoverHeight = 2.5f;
    public float batDiveSpeed = 10f;
    public float batDiveDelay = 0.4f;
    private bool isHanging = true;
    private bool isPreparingDive = false;
    private bool isDiving = false;

    [Header("Cấu hình Cậu Bé Cưỡi Trâu")]
    public float buffaloChargeSpeed = 8f;
    public float buffaloWallCheckDist = 1f;

    [Header("Cấu hình Mantis (Bọ Ngựa)")]
    public float mantisPatrolRange = 3f;
    public float mantisDashSpeed = 16f;
    public float mantisWindupTime = 0.3f;
    public float mantisDashDuration = 0.2f;
    public float mantisVanishDuration = 0.4f;
    public float mantisTeleportDist = 4.5f;
    public float mantisAttackCooldown = 2.5f;
    private bool isMantisAttacking = false;
    private bool isDashingMantis = false;

    [Header("Âm thanh (SFX)")]
    public AudioClip moveSound;
    public AudioClip attackSound;
    public AudioClip throwSound;  // Tiếng quăng đá
    public AudioClip hurtSound;   // Tiếng kêu la khi bị chém
    public AudioClip deathSound;  // Tiếng lúc bốc hơi

    private AudioSource audioSource; // Cái loa gắn trên người con khỉ

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckLength = 1.2f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;

    private Animator anim;

    private Vector3 homePosition;
    private float attackTimer;
    private bool isKnockbacked = false;
    private bool isWaiting = false;
    private bool movingRight = true;
    private bool isGrounded;
    private bool isThrowingRocks = false;

    private Collider2D myCol;
    private Collider2D playerCol;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        homePosition = transform.position;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Nếu quên gắn loa ngoài Unity, code sẽ tự động gắn dùm luôn cho đỡ lỗi!
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;

            Collider2D[] myCols = GetComponents<Collider2D>();
            Collider2D[] playerCols = playerObj.GetComponents<Collider2D>();

            foreach (var mCol in myCols)
            {
                foreach (var pCol in playerCols)
                {
                    Physics2D.IgnoreCollision(mCol, pCol, true);
                }
            }

            if (myCols.Length > 0) myCol = myCols[0];
            if (playerCols.Length > 0) playerCol = playerCols[0];
        }

        if (isMonkey) rb.constraints = RigidbodyConstraints2D.FreezeAll;
        else if (isBat) rb.gravityScale = 0f;
        else if (isBoyOnBuffalo)
        {
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 30f, groundLayer);
            if (hit.collider != null)
            {
                float bottomOffset = col != null ? col.bounds.extents.y : 0.5f;
                transform.position = new Vector3(transform.position.x, hit.point.y + bottomOffset, transform.position.z);
            }
        }
        if (isBoyOnBuffalo && moveSound != null)
        {
            audioSource.clip = moveSound; // Gắn cuộn băng tiếng bước chân vào loa
            audioSource.loop = true;      // Bật chế độ lặp đi lặp lại
            audioSource.volume = 0.6f;    // Chỉnh âm lượng hơi nhỏ một chút để làm nền
            audioSource.Play();           // Bấm nút Play
        }

        movingRight = transform.localScale.x > 0;
    }

    void Update()
    {
        if (player == null || isKnockbacked || isWaiting) return;

        if (anim != null)
        {
            float currentSpeed = isBat ? rb.linearVelocity.magnitude : Mathf.Abs(rb.linearVelocity.x);
            anim.SetFloat("Speed", currentSpeed);
        }

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckLength, groundLayer);
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (touchCooldownTimer > 0) touchCooldownTimer -= Time.deltaTime;

        if (myCol != null && playerCol != null)
        {
            if (myCol.bounds.Intersects(playerCol.bounds) && !isMonkey && touchCooldownTimer <= 0)
            {
                PlayerMovement pm = player.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    pm.TakeDamage(1);
                    touchCooldownTimer = 1.5f;
                }
            }
        }

        // --- Logic Bat ---
        if (isBat)
        {
            if (isHanging)
            {
                rb.linearVelocity = Vector2.zero;
                if (distanceToPlayer <= detectionRange) isHanging = false;
            }
            else if (!isPreparingDive) BatChasePlayer();
            return;
        }

        // --- Logic Buffalo ---
        if (isBoyOnBuffalo)
        {
            float direction = movingRight ? 1f : -1f;
            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, new Vector2(direction, 0f), buffaloWallCheckDist, groundLayer);
            if (wallHit.collider != null)
            {
                Flip();
                direction = movingRight ? 1f : -1f;
            }
            rb.linearVelocity = new Vector2(direction * buffaloChargeSpeed, 0f);
            return;
        }

        // --- Logic Mantis ---
        if (isMantis)
        {
            if (attackTimer > 0) attackTimer -= Time.deltaTime;

            if (isMantisAttacking)
            {
                if (isDashingMantis)
                {
                    float direction = movingRight ? 1f : -1f;
                    rb.linearVelocity = new Vector2(direction * mantisDashSpeed, 0f);
                }
                return;
            }

            if (distanceToPlayer <= detectionRange && attackTimer <= 0 && Mathf.Abs(player.position.y - transform.position.y) < 1.5f)
            {
                StartCoroutine(MantisComboRoutine());
            }
            else
            {
                // [ĐÃ THÊM] Đóng băng hướng nhìn và hành động nếu đang bị hất tung trên không
                if (!isGrounded) return;

                float direction = movingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

                Vector2 frontPos = (Vector2)transform.position + new Vector2(direction * 0.6f, 0f);
                bool wallAhead = Physics2D.Raycast(transform.position, new Vector2(direction, 0f), 1f, groundLayer);
                bool groundAhead = Physics2D.Raycast(frontPos, Vector2.down, groundCheckLength, groundLayer);

                bool outOfRange = Mathf.Abs(transform.position.x - homePosition.x) > mantisPatrolRange;
                bool movingAway = (transform.position.x > homePosition.x && movingRight) || (transform.position.x < homePosition.x && !movingRight);

                if (wallAhead || !groundAhead || (outOfRange && movingAway)) Flip();
            }
            return;
        }

        // --- Logic Tiger/Monkey ---
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        if (distanceToPlayer <= attackRange)
        {
            if (isTiger) TigerAttack();
            else if (isMonkey) MonkeyAttack();
            else AttackPlayer();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            if (isMonkey) FacePlayer();
            else ChasePlayer();
        }
        else if (Vector2.Distance(transform.position, homePosition) > 0.5f)
        {
            if (!isWaiting && isGrounded && !isMonkey) StartCoroutine(WaitAndGoHome());
        }
        else
        {
            if (isGrounded) StopMoving();
        }
    }

    private IEnumerator MantisComboRoutine()
    {
        isMantisAttacking = true;
        rb.linearVelocity = Vector2.zero;

        for (int i = 0; i < 3; i++)
        {
            if (i > 0)
            {
                if (sr != null) sr.enabled = false;
                float randomSide = (Random.value > 0.5f) ? 1f : -1f;
                transform.position = new Vector3(player.position.x + (randomSide * mantisTeleportDist), transform.position.y, transform.position.z);
                yield return new WaitForSeconds(mantisVanishDuration);
                if (sr != null) sr.enabled = true;
            }

            FacePlayer();
            if (sr != null) sr.color = Color.green;
            yield return new WaitForSeconds(mantisWindupTime);

            if (sr != null) sr.color = Color.white;
            isDashingMantis = true;

            if (anim != null) anim.SetTrigger("Attack");

            float oldGravity = rb.gravityScale;
            rb.gravityScale = 0f;
            yield return new WaitForSeconds(mantisDashDuration);

            isDashingMantis = false;
            rb.gravityScale = oldGravity;
            rb.linearVelocity = Vector2.zero;
        }

        isMantisAttacking = false;
        attackTimer = mantisAttackCooldown;
    }

    public void SetChargeDirection(bool faceRight)
    {
        if (faceRight && !movingRight) Flip();
        else if (!faceRight && movingRight) Flip();
    }

    void BatChasePlayer()
    {
        if (isDiving)
        {
            Vector2 diveDirection = (player.position - transform.position).normalized;
            rb.linearVelocity = diveDirection * batDiveSpeed;

            if (rb.linearVelocity.x > 0 && !movingRight) Flip();
            else if (rb.linearVelocity.x < 0 && movingRight) Flip();

            if (isGrounded || transform.position.y < player.position.y - 0.5f) isDiving = false;
            return;
        }

        Vector3 targetPos = new Vector3(player.position.x, player.position.y + hoverHeight, player.position.z);
        float wobbleY = Mathf.Sin(Time.time * batWobbleSpeed) * batWobbleAmount;
        float extraChaosY = Mathf.Cos(Time.time * batWobbleSpeed * 1.3f) * (batWobbleAmount * 0.5f);
        float wobbleX = Mathf.Sin(Time.time * batWobbleSpeed * 0.8f) * (batWobbleAmount * 0.5f);

        if (Mathf.Abs(player.position.x - transform.position.x) < 2f && transform.position.y > player.position.y)
        {
            StartCoroutine(PrepareBatDive());
            return;
        }

        Vector2 hoverDirection = (targetPos - transform.position).normalized;
        rb.linearVelocity = new Vector2((hoverDirection.x * chaseSpeed) + wobbleX, (hoverDirection.y * chaseSpeed) + wobbleY + extraChaosY);

        if (rb.linearVelocity.x > 0 && !movingRight) Flip();
        else if (rb.linearVelocity.x < 0 && movingRight) Flip();
    }

    private IEnumerator PrepareBatDive()
    {
        isPreparingDive = true;
        rb.linearVelocity = new Vector2(0, 3f);

        if (anim != null) anim.SetTrigger("Attack");

        yield return new WaitForSeconds(batDiveDelay);
        isPreparingDive = false;
        isDiving = true;
    }

    public void TakeDamage(Vector2 playerPosition)
    {
        if (isMantis && sr != null && !sr.enabled) return;

        health--;

        if (hurtSound != null && audioSource != null && health > 0)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        if (anim != null) anim.SetTrigger("Hurt");

        StopCoroutine("FlashRed");
        StartCoroutine(FlashRed());

        if (!isMonkey && !isBoyOnBuffalo && !(isMantis && isDashingMantis))
        {
            StopCoroutine("ApplyKnockback");
            StartCoroutine(ApplyKnockback(playerPosition));
        }

        if (health <= 0) Die();
    }

    private IEnumerator ApplyKnockback(Vector2 playerPos)
    {
        isKnockbacked = true;
        Vector2 direction = ((Vector2)transform.position - playerPos).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(direction.x, 0.5f) * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        isKnockbacked = false;
    }

    IEnumerator FlashRed()
    {
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            sr.color = Color.white;
        }
    }

    void ChasePlayer() { if (isGrounded) MoveTowards(player.position.x, chaseSpeed); }
    void FacePlayer() { float direction = (player.position.x > transform.position.x) ? 1 : -1; if (direction > 0 && !movingRight) Flip(); else if (direction < 0 && movingRight) Flip(); }
    void MoveTowards(float targetX, float speed)
    {
        float distance = targetX - transform.position.x;

        // Nếu đã đứng rất gần mục tiêu (khoảng 0.1 đơn vị) thì không cần di chuyển hay xoay hướng nữa
        if (Mathf.Abs(distance) < 0.1f) return;

        float direction = (distance > 0) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        // Kiểm tra để Flip Sprite
        if (direction > 0 && !movingRight) Flip();
        else if (direction < 0 && movingRight) Flip();
    }
    void StopMoving() { rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); }
    void Flip() { movingRight = !movingRight; transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z); }

    IEnumerator WaitAndGoHome()
    {
        isWaiting = true; StopMoving(); yield return new WaitForSeconds(3f);
        while (Mathf.Abs(transform.position.x - homePosition.x) > 0.2f)
        {
            if (isKnockbacked || (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)) { isWaiting = false; yield break; }
            if (isGrounded) MoveTowards(homePosition.x, returnSpeed);
            yield return null;
        }
        StopMoving(); transform.position = new Vector3(homePosition.x, transform.position.y, transform.position.z); isWaiting = false;
    }

    void AttackPlayer() { StopMoving(); }

    void TigerAttack()
    {
        if (isGrounded && attackTimer <= 0)
        {
            attackTimer = tigerAttackCooldown;
            rb.linearVelocity = Vector2.zero;

            if (anim != null) anim.SetTrigger("Attack");

            float direction = (player.position.x > transform.position.x) ? 1 : -1;
            if (direction > 0 && !movingRight) Flip(); else if (direction < 0 && movingRight) Flip();
            rb.AddForce(new Vector2(direction * tigerJumpForwardForce, tigerJumpUpwardForce), ForceMode2D.Impulse);
        }
        else if (isGrounded && attackTimer > 0 && attackTimer < tigerAttackCooldown - 0.2f) StopMoving();
    }

    void MonkeyAttack()
    {
        FacePlayer();

        if (attackTimer <= 0 && !isThrowingRocks)
        {
            isThrowingRocks = true;

            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }
        }
        else if (isGrounded)
        {
            StopMoving();
        }
    }

    public void SpawnRockEvent()
    {
        if (throwSound != null && audioSource != null) audioSource.PlayOneShot(throwSound);
        if (rockPrefab != null && throwPoint != null && player != null)
        {
            GameObject rock = Instantiate(rockPrefab, throwPoint.position, Quaternion.identity);
            Rigidbody2D rockRb = rock.GetComponent<Rigidbody2D>();

            if (rockRb != null)
            {
                Vector3 targetPosition = player.position;
                targetPosition.y -= 0.5f;

                Vector2 throwDir = (targetPosition - throwPoint.position).normalized;
                rockRb.AddForce(throwDir * rockThrowForce, ForceMode2D.Impulse);
            }
        }
    }

    public void FinishAttackEvent()
    {
        attackTimer = monkeyAttackCooldown;
        isThrowingRocks = false;
    }

    void Die()
    {
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        Destroy(gameObject);
    }

    // ==========================================
    // TRƯỜNG HỢP 1: VA CHẠM CỨNG (Không bật Is Trigger)
    // ==========================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isMonkey)
        {
            PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            if (playerMovement != null && touchCooldownTimer <= 0)
            {
                playerMovement.TakeDamage(1);
                touchCooldownTimer = 1.5f;

            }
        }

        if (isMantis && isDashingMantis)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0 || collision.gameObject.GetComponent<DestructibleObject>() != null)
            {
                isDashingMantis = false;
                rb.linearVelocity = Vector2.zero;
            }
        }

        if (isBoyOnBuffalo)
        {
            BambooSegment bamboo = collision.gameObject.GetComponent<BambooSegment>();
            if (bamboo != null)
            {
                bamboo.TakeDamage(999);
            }
        }
    }

    // ==========================================
    // TRƯỜNG HỢP 2: ĐI XUYÊN (Có bật Is Trigger)
    // ==========================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log($"<color=yellow>Dơi vừa chạm vào: {collision.gameObject.name} (Tag: {collision.gameObject.tag})</color>");
        if (collision.CompareTag("Player") && !isMonkey)
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null && touchCooldownTimer <= 0)
            {
                playerMovement.TakeDamage(1);
                touchCooldownTimer = 1.5f;

            }
        }

        if (isBoyOnBuffalo)
        {
            BambooSegment bamboo = collision.GetComponent<BambooSegment>();
            if (bamboo != null) bamboo.TakeDamage(999);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckLength);
    }
}