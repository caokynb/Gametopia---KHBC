using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    [Header("Loại Quái")]
    [Tooltip("Tích vào đây nếu con quái này là Tiger (Bám đuổi & Nhảy vồ)")]
    public bool isTiger = false;
    [Tooltip("Tích vào đây nếu con quái này là Monkey (Đứng im & Ném đá)")]
    public bool isMonkey = false;
    [Tooltip("Tích vào đây nếu con quái này là Bat (Treo trần & Bay lượn)")]
    public bool isBat = false;
    [Tooltip("Tích vào đây nếu là Rết khổng lồ (Bò theo điểm Waypoint)")]
    public bool isCentipede = false;

    [Header("Chỉ số cơ bản")]
    public int health = 5;
    public float chaseSpeed = 4f;
    public float returnSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1.2f;

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

    [Header("Cấu hình Centipede (Waypoint)")]
    public Transform[] waypoints; // Kéo các điểm (Point) vào đây
    public float centipedeSpeed = 3f;
    public float rotationSpeed = 10f; // Tốc độ xoay đầu khi đến góc cua
    private int currentWaypointIndex = 0;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckLength = 1.2f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;

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
        if (playerObj != null) player = playerObj.transform;

        if (isMonkey)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else if (isBat)
        {
            rb.gravityScale = 0f;
        }
        else if (isCentipede)
        {
            // Rết bò trên background nên không cần trọng lực và không bị đẩy vật lý
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Update()
    {
        if (player == null || isKnockbacked || isWaiting) return;

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckLength, groundLayer);
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // ==========================================
        // LOGIC RIÊNG CHO BAT
        // ==========================================
        if (isBat)
        {
            if (isHanging)
            {
                rb.linearVelocity = Vector2.zero;
                if (distanceToPlayer <= detectionRange) isHanging = false;
            }
            else if (!isPreparingDive)
            {
                BatChasePlayer();
            }
            return;
        }

        // ==========================================
        // LOGIC RIÊNG CHO CENTIPEDE
        // ==========================================
        if (isCentipede)
        {
            CentipedeMove();
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

    // --- HÀM XỬ LÝ CENTIPEDE BÒ THEO ĐIỂM ---
    void CentipedeMove()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // BÍ QUYẾT: Ép tất cả về Vector2 để loại bỏ hoàn toàn trục Z.
        // Điều này ngăn con rết bị kẹt và xoay mòng mòng nếu các điểm bị lệch chiều sâu (Z).
        Vector2 currentPos = transform.position;
        Vector2 targetPos = waypoints[currentWaypointIndex].position;

        // 1. Tính khoảng cách trong không gian 2D
        float distance = Vector2.Distance(currentPos, targetPos);

        // 2. Di chuyển tịnh tiến
        transform.position = Vector2.MoveTowards(currentPos, targetPos, centipedeSpeed * Time.deltaTime);

        // 3. Chỉ xoay đầu nếu khoảng cách đủ xa (tránh lỗi chia cho 0 làm quái giật cục)
        if (distance > 0.05f)
        {
            Vector2 direction = (targetPos - currentPos).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Nếu rết của bạn vẽ hướng đầu lên trên (thay vì hướng sang phải), 
            // bạn có thể trừ đi 90 ở đây: targetAngle - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. Nếu đã đến điểm đích, chuyển sang điểm tiếp theo
        if (distance < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length) currentWaypointIndex = 0;
        }
    }

    // --- HÀM XỬ LÝ DƠI BAY ---
    void BatChasePlayer()
    {
        if (isDiving)
        {
            Vector2 diveDirection = (player.position - transform.position).normalized;
            rb.linearVelocity = diveDirection * batDiveSpeed;

            if (rb.linearVelocity.x > 0 && !movingRight) Flip();
            else if (rb.linearVelocity.x < 0 && movingRight) Flip();

            if (isGrounded || transform.position.y < player.position.y - 0.5f)
            {
                isDiving = false;
            }
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
        float finalXVelocity = (hoverDirection.x * chaseSpeed) + wobbleX;
        float finalYVelocity = (hoverDirection.y * chaseSpeed) + wobbleY + extraChaosY;

        rb.linearVelocity = new Vector2(finalXVelocity, finalYVelocity);

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
        health--;
        StopCoroutine("FlashRed");
        StartCoroutine(FlashRed());

        // Rết và Khỉ không bị đẩy lùi
        if (!isMonkey && !isCentipede)
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

    void ChasePlayer()
    {
        if (!isGrounded) return;
        MoveTowards(player.position.x, chaseSpeed);
    }

    void FacePlayer()
    {
        float direction = (player.position.x > transform.position.x) ? 1 : -1;
        if (direction > 0 && !movingRight) Flip();
        else if (direction < 0 && movingRight) Flip();
    }

    void MoveTowards(float targetX, float speed)
    {
        float direction = (targetX > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        if (direction > 0 && !movingRight) Flip();
        else if (direction < 0 && movingRight) Flip();
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void Flip()
    {
        movingRight = !movingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    IEnumerator WaitAndGoHome()
    {
        isWaiting = true;
        StopMoving();
        yield return new WaitForSeconds(3f);

        float stopDistance = 0.2f;
        while (Mathf.Abs(transform.position.x - homePosition.x) > stopDistance)
        {
            if (isKnockbacked || (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange))
            {
                isWaiting = false;
                yield break;
            }
            if (isGrounded) MoveTowards(homePosition.x, returnSpeed);
            yield return null;
        }

        StopMoving();
        transform.position = new Vector3(homePosition.x, transform.position.y, transform.position.z);
        isWaiting = false;
    }

    void AttackPlayer() { StopMoving(); }

    void TigerAttack()
    {
        if (isGrounded && attackTimer <= 0)
        {
            attackTimer = tigerAttackCooldown;
            rb.linearVelocity = Vector2.zero;
            float direction = (player.position.x > transform.position.x) ? 1 : -1;

            if (direction > 0 && !movingRight) Flip();
            else if (direction < 0 && movingRight) Flip();

            Vector2 jumpForce = new Vector2(direction * tigerJumpForwardForce, tigerJumpUpwardForce);
            rb.AddForce(jumpForce, ForceMode2D.Impulse);
        }
        else if (isGrounded && attackTimer > 0 && attackTimer < tigerAttackCooldown - 0.2f)
        {
            StopMoving();
        }
    }

    void MonkeyAttack()
    {
        FacePlayer();
        if (attackTimer <= 0 && !isThrowingRocks)
        {
            StartCoroutine(ThrowRocksRoutine());
        }
        else if (isGrounded)
        {
            StopMoving();
        }
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

                if (rockRb != null)
                {
                    Vector2 throwDirection = (player.position - throwPoint.position).normalized;
                    throwDirection.y += 0.25f;

                    rockRb.AddForce(throwDirection * rockThrowForce, ForceMode2D.Impulse);
                }
            }

            yield return new WaitForSeconds(delayBetweenRocks);
        }

        attackTimer = monkeyAttackCooldown;
        isThrowingRocks = false;
    }

    void Die()
    {
        Destroy(gameObject);
    }

    // --- XỬ LÝ SÁT THƯƠNG ---
    // Dành cho quái xài Collider thường (Cọp, Dơi)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isMonkey)
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null) player.TakeDamage(1);
        }
    }

    // Dành cho quái xài Is Trigger (Rết bò ở Background)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isMonkey)
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null) player.TakeDamage(1);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckLength);
    }
}