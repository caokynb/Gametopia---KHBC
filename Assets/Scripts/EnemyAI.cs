using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Thông số di chuyển")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;
    public Transform wallCheck;
    public float wallCheckDistance = 0.3f;
    public LayerMask obstacleLayer;

    [Header("Phát hiện & Tấn công")]
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public float attackRate = 1.5f;
    public float damage = 10f;

    [Header("Chỉ số Máu")]
    public int health = 5;

    [Header("Cấu hình Đẩy lùi (Knockback)")]
    public float knockbackForce = 7f;      // Lực đẩy mạnh hay nhẹ
    public float knockbackDuration = 0.2f; // Thời gian quái bị "đơ" khi bị đẩy
    private bool isKnockbacked = false;    // Kiểm tra quái có đang bị đẩy không

    private Rigidbody2D rb;
    private Transform player;
    private bool movingRight = true;
    private float nextAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        // Nếu đang bị đẩy lùi thì không thực hiện các hành động di chuyển/AI
        if (player == null || isKnockbacked) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange) AttackPlayer();
        else if (distanceToPlayer <= detectionRange) ChasePlayer();
        else Patrol();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AttackMode pc = collision.gameObject.GetComponent<AttackMode>();
            if (pc != null)
            {
                pc.Respawn();
                Debug.Log("Player chạm quái và đã hồi sinh!");
            }
        }
    }

    // --- CẬP NHẬT HÀM NHẬN SÁT THƯƠNG CÓ KNOCKBACK ---
    public void TakeDamage(Vector2 playerPosition)
    {
        health -= 1;
        Debug.Log("Quái bị trúng đòn! Máu còn lại: " + health);

        // Chạy hiệu ứng nhấp nháy đỏ
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) StartCoroutine(FlashRed(sr));

        // Thực hiện đẩy lùi
        StartCoroutine(ApplyKnockback(playerPosition));

        if (health <= 0) Die();
    }

    private IEnumerator ApplyKnockback(Vector2 playerPosition)
    {
        isKnockbacked = true;

        // Tính toán hướng: Lấy vị trí quái trừ vị trí người chơi = hướng văng ra xa
        Vector2 direction = ((Vector2)transform.position - playerPosition).normalized;

        // Reset vận tốc cũ để lực đẩy ổn định hơn
        rb.linearVelocity = Vector2.zero;

        // Tác động lực đẩy (Sử dụng Impulse cho lực tức thời)
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

        // Chờ hết thời gian bị đẩy
        yield return new WaitForSeconds(knockbackDuration);

        isKnockbacked = false;
    }

    IEnumerator FlashRed(SpriteRenderer sr)
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    void Die()
    {
        Debug.Log("Quái đã trúng đủ 5 đòn và hẹo!");
        Destroy(gameObject);
    }

    // --- CÁC HÀM DI CHUYỂN GIỮ NGUYÊN ---
    void Patrol()
    {
        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);
        RaycastHit2D wallInfo = Physics2D.Raycast(wallCheck.position, movingRight ? Vector2.right : Vector2.left, wallCheckDistance, obstacleLayer);
        if (wallInfo.collider != null) Flip();
    }

    void ChasePlayer()
    {
        float direction = (player.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
        if (direction > 0 && !movingRight) Flip();
        else if (direction < 0 && movingRight) Flip();
    }

    void AttackPlayer()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (Time.time >= nextAttackTime)
        {
            Debug.Log("Enemy đang đánh!");
            nextAttackTime = Time.time + attackRate;
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}