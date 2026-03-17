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

    // ==========================================
    // [MỚI] HỆ THỐNG RADAR SÁT THƯƠNG
    // ==========================================
    [Header("Cảm biến Chạm (Đi xuyên)")]
    public float touchDamageRadius = 0.8f; // Khoảng cách quét trúng Anh Khoai
    private float touchCooldownTimer = 0f; // Bộ đếm giờ để không cắn liên tục 60 lần/giây

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

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckLength = 1.2f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;

    private Collider2D myCol;
    private Collider2D playerCol;

    private Vector3 homePosition;
    private float attackTimer;
    private bool isKnockbacked = false;
    private bool isWaiting = false;
    private bool movingRight = true;
    private bool isGrounded;
    private bool isThrowingRocks = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        homePosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;

            // ==========================================
            // [MỚI] LỆNH ÉP QUÁI VÀ PLAYER ĐI XUYÊN QUA NHAU
            // ==========================================
            myCol = GetComponent<Collider2D>();
            playerCol = playerObj.GetComponent<Collider2D>();

            if (myCol != null && playerCol != null)
            {
                Physics2D.IgnoreCollision(myCol, playerCol, true);
            }
        }

        if (isMonkey)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else if (isBat)
        {
            rb.gravityScale = 0f;
        }
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
    }

    void Update()
    {
        if (player == null || isKnockbacked || isWaiting) return;

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckLength, groundLayer);
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // ==========================================
        // [CẬP NHẬT] TỰ ĐỘNG GÂY SÁT THƯƠNG
        // ==========================================
        // Xóa dòng check touchCooldownTimer cũ
        if (myCol != null && playerCol != null)
        {
            if (myCol.bounds.Intersects(playerCol.bounds) && !isMonkey)
            {
                PlayerMovement pm = player.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    // Gọi hàm sát thương. PlayerMovement sẽ tự lo việc bất tử (iFrames)
                    pm.TakeDamage(1);
                }
            }
        }

        // ==========================================
        // LOGIC CHO BAT
        // ==========================================
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

        // ==========================================
        // LOGIC CHO CẬU BÉ CƯỠI TRÂU
        // ==========================================
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

        // ==========================================
        // LOGIC CHO MANTIS (BỌ NGỰA)
        // ==========================================
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
                float direction = movingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

                Vector2 frontPos = (Vector2)transform.position + new Vector2(direction * 0.6f, 0f);
                bool wallAhead = Physics2D.Raycast(transform.position, new Vector2(direction, 0f), 1f, groundLayer);
                bool groundAhead = Physics2D.Raycast(frontPos, Vector2.down, groundCheckLength, groundLayer);

                bool outOfRange = Mathf.Abs(transform.position.x - homePosition.x) > mantisPatrolRange;
                bool movingAway = (transform.position.x > homePosition.x && movingRight) || (transform.position.x < homePosition.x && !movingRight);

                if (wallAhead || !groundAhead || (outOfRange && movingAway))
                {
                    Flip();
                }
            }
            return;
        }

        // ==========================================
        // LOGIC CHO TIGER VÀ MONKEY
        // ==========================================
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
                Vector3 newPos = new Vector3(player.position.x + (randomSide * mantisTeleportDist), transform.position.y, transform.position.z);
                transform.position = newPos;
                yield return new WaitForSeconds(mantisVanishDuration);
                if (sr != null) sr.enabled = true;
            }

            FacePlayer();
            if (sr != null) sr.color = Color.green;
            yield return new WaitForSeconds(mantisWindupTime);

            if (sr != null) sr.color = Color.white;
            isDashingMantis = true;

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

    private void OnBecameInvisible()
    {
        if (isBoyOnBuffalo) Destroy(gameObject);
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
        yield return new WaitForSeconds(batDiveDelay);
        isPreparingDive = false;
        isDiving = true;
    }

    public void TakeDamage(Vector2 playerPosition)
    {
        if (isMantis && sr != null && !sr.enabled) return;

        health--;
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
    void MoveTowards(float targetX, float speed) { float direction = (targetX > transform.position.x) ? 1 : -1; rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y); if (direction > 0 && !movingRight) Flip(); else if (direction < 0 && movingRight) Flip(); }
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
            attackTimer = tigerAttackCooldown; rb.linearVelocity = Vector2.zero;
            float direction = (player.position.x > transform.position.x) ? 1 : -1;
            if (direction > 0 && !movingRight) Flip(); else if (direction < 0 && movingRight) Flip();
            rb.AddForce(new Vector2(direction * tigerJumpForwardForce, tigerJumpUpwardForce), ForceMode2D.Impulse);
        }
        else if (isGrounded && attackTimer > 0 && attackTimer < tigerAttackCooldown - 0.2f) StopMoving();
    }

    void MonkeyAttack()
    {
        FacePlayer();
        if (attackTimer <= 0 && !isThrowingRocks) StartCoroutine(ThrowRocksRoutine());
        else if (isGrounded) StopMoving();
    }

    private IEnumerator ThrowRocksRoutine()
    {
        isThrowingRocks = true;
        for (int i = 0; i < rocksToThrow; i++)
        {
            if (rockPrefab != null && throwPoint != null && player != null)
            {
                GameObject rock = Instantiate(rockPrefab, throwPoint.position, Quaternion.identity);
                Rigidbody2D rockRb = rock.GetComponent<Rigidbody2D>();
                if (rockRb != null) { Vector2 throwDir = (player.position - throwPoint.position).normalized; throwDir.y += 0.25f; rockRb.AddForce(throwDir * rockThrowForce, ForceMode2D.Impulse); }
            }
            yield return new WaitForSeconds(delayBetweenRocks);
        }
        attackTimer = monkeyAttackCooldown; isThrowingRocks = false;
    }

    void Die() { Destroy(gameObject); }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isMonkey)
        {
            PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            // CHỈ GỌI TAKEDAMAGE NẾU TIMER CHO PHÉP (Dự phòng va chạm)
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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isMonkey)
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            // CHỈ GỌI TAKEDAMAGE NẾU TIMER CHO PHÉP (Dự phòng va chạm)
            if (playerMovement != null && touchCooldownTimer <= 0)
            {
                playerMovement.TakeDamage(1);
                touchCooldownTimer = 1.5f;
            }
        }

        if (isBoyOnBuffalo)
        {
            DestructibleObject bamboo = collision.GetComponent<DestructibleObject>();
            if (bamboo != null) bamboo.TakeDamage();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // [MỚI] Vẽ vòng tròn đỏ vùng cắn để bạn dễ căn chỉnh ngoài Scene
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, touchDamageRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckLength);

        if (isBoyOnBuffalo)
        {
            Gizmos.color = Color.blue;
            float dir = movingRight ? 1f : -1f;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(dir * buffaloWallCheckDist, 0f, 0f));
        }

        if (isMantis)
        {
            Gizmos.color = Color.yellow;
            Vector3 leftLimit = Application.isPlaying ? homePosition : transform.position;
            leftLimit.x -= mantisPatrolRange;
            Vector3 rightLimit = Application.isPlaying ? homePosition : transform.position;
            rightLimit.x += mantisPatrolRange;

            Gizmos.DrawLine(leftLimit + Vector3.up * 0.5f, leftLimit + Vector3.down * 0.5f);
            Gizmos.DrawLine(rightLimit + Vector3.up * 0.5f, rightLimit + Vector3.down * 0.5f);
            Gizmos.DrawLine(leftLimit, rightLimit);
        }
    }
}