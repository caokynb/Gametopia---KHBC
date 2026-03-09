using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Loại Quái")]
    [Tooltip("Tích vào đây nếu con quái này là Tiger (Bám đuổi & Nhảy vồ)")]
    public bool isTiger = false;
    [Tooltip("Tích vào đây nếu con quái này là Monkey (Đứng im & Ném đá)")]
    public bool isMonkey = false;

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
    public GameObject rockPrefab;       // Kéo Prefab viên đá vào đây
    public Transform throwPoint;        // Vị trí ném (tay của khỉ)
    public int rocksToThrow = 3;        // Số lượng đá ném mỗi đợt
    public float delayBetweenRocks = 0.3f; // Thời gian giữa các viên đá
    public float rockThrowForce = 8f;   // Lực ném
    public float monkeyAttackCooldown = 2.5f; // Thời gian nghỉ giữa các đợt ném

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckLength = 1.2f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;

    private Vector3 homePosition;
    private float attackTimer; // Dùng chung cho cả Tiger và Monkey
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

        // Nếu là Khỉ, khóa cứng hoàn toàn vị trí (X, Y) và góc xoay (Z)
        if (isMonkey)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    void Update()
    {
        if (player == null || isKnockbacked || isWaiting) return;

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckLength, groundLayer);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            if (isTiger) TigerAttack();
            else if (isMonkey) MonkeyAttack();
            else AttackPlayer();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            if (isMonkey) FacePlayer(); // Khỉ chỉ xoay mặt nhìn theo, không chạy
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

    public void TakeDamage(Vector2 playerPosition)
    {
        health--;
        StopCoroutine("FlashRed");
        StartCoroutine(FlashRed());

        // Chỉ áp dụng lực đẩy lùi nếu KHÔNG phải là Khỉ
        if (!isMonkey)
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
        FacePlayer(); // Khỉ luôn nhìn theo người chơi khi ném
        if (attackTimer <= 0 && !isThrowingRocks)
        {
            StartCoroutine(ThrowRocksRoutine());
        }
        else if (isGrounded)
        {
            StopMoving(); // Đứng im tại chỗ
        }
    }

    private IEnumerator ThrowRocksRoutine()
    {
        isThrowingRocks = true;

        for (int i = 0; i < rocksToThrow; i++)
        {
            if (rockPrefab != null && throwPoint != null && player != null)
            {
                // Tạo viên đá
                GameObject rock = Instantiate(rockPrefab, throwPoint.position, Quaternion.identity);
                Rigidbody2D rockRb = rock.GetComponent<Rigidbody2D>();

                if (rockRb != null)
                {
                    // Tính toán hướng ném thẳng vào Anh Khoai và thêm một chút độ cong (arc) lên trên
                    Vector2 throwDirection = (player.position - throwPoint.position).normalized;
                    throwDirection.y += 0.25f; // Ném bổng lên một chút cho đẹp

                    rockRb.AddForce(throwDirection * rockThrowForce, ForceMode2D.Impulse);
                }
            }

            yield return new WaitForSeconds(delayBetweenRocks);
        }

        attackTimer = monkeyAttackCooldown;
        isThrowingRocks = false;
    }

    void Die() { Destroy(gameObject); }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isMonkey) // Khỉ ném đá không cần gây sát thương khi chạm vào người
        {
            collision.gameObject.GetComponent<AttackMode>()?.Respawn();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckLength);
    }
}