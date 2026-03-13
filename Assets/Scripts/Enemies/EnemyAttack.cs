using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    [Header("Chỉ số chiến đấu")]
    public int health = 5;
    public float attackRange = 1.2f;
    public float detectionRange = 5f;
    public float attackRate = 1.5f;
    private float nextAttackTime;

    [Header("Cài đặt Knockback")]
    public float knockbackForceX = 5f; // Lực đẩy văng ngang
    public float knockbackForceY = 3f; // Lực hất tung lên trời
    public float knockbackDuration = 0.2f; // Thời gian bị văng

    private EnemyMovement movement;
    private SpriteRenderer sr;
    private Transform player;
    private Rigidbody2D rb; // Thêm Rigidbody để xử lý vật lý văng

    void Start()
    {
        movement = GetComponent<EnemyMovement>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); // Lấy Rigidbody
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        // Xóa check isKnockbacked cũ, thay bằng kiểm tra xem script movement có đang bật không
        if (player == null || (movement != null && !movement.enabled)) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Gọi logic di chuyển từ script Movement
        if (movement != null)
        {
            movement.HandleMovement(distanceToPlayer, detectionRange, attackRange);
        }

        // Logic tấn công riêng biệt
        if (distanceToPlayer <= attackRange)
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            Debug.Log("Enemy đang thực hiện đòn đánh!");
            nextAttackTime = Time.time + attackRate;
        }
    }

    public void TakeDamage(Vector2 playerPosition)
    {
        health--;
        Debug.Log("Quái trúng đòn! Máu: " + health);

        StartCoroutine(FlashRed());

        // Gọi thẳng Coroutine văng lùi ở script này thay vì gọi sang EnemyMovement
        StartCoroutine(ApplyKnockbackLogic(playerPosition));

        if (health <= 0) Die();
    }

    // --- LOGIC KNOCKBACK MỚI ---
    private IEnumerator ApplyKnockbackLogic(Vector2 playerPosition)
    {
        if (rb != null)
        {
            // 1. "Tắt não" di chuyển: Disable script EnemyMovement
            if (movement != null) movement.enabled = false;

            // 2. Xóa sạch đà di chuyển cũ
            rb.linearVelocity = Vector2.zero;

            // 3. Tính hướng đẩy (Quái bên phải Anh Khoai -> Đẩy sang phải)
            int pushDirection = transform.position.x > playerPosition.x ? 1 : -1;

            // 4. Áp dụng lực văng
            rb.AddForce(new Vector2(knockbackForceX * pushDirection, knockbackForceY), ForceMode2D.Impulse);

            // 5. Chờ thời gian bay trên không trung
            yield return new WaitForSeconds(knockbackDuration);

            // 6. Dừng lại khi rớt xuống để không bị trượt băng
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            // 7. "Bật não" lại: Bật script EnemyMovement lên để nó đi tiếp
            if (movement != null) movement.enabled = true;
        }
    }

    IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    void Die()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<AttackMode>()?.Respawn();
        }
    }
}