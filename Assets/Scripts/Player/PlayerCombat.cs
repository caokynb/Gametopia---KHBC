using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public PlayerAttributes attributes;
    private Vector3 startPosition;
    private Rigidbody2D rb;

    [Header("Cấu hình đòn đánh")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    // --- THÊM MỚI BIẾN COOLDOWN ---
    [Header("Thời gian hồi chiêu")]
    public float attackRate = 2f;    // Số lần đánh trong 1 giây (ví dụ: 2 lần/giây)
    private float nextAttackTime = 0f; // Thời điểm được phép đánh tiếp theo

    [Header("Cài đặt hồi năng lượng")]
    public float bambooRegenRate = 5f;
    private float regenTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    void Update()
    {
        RegenerateBamboo();

        // Kiểm tra phím đánh VÀ thời gian hiện tại phải lớn hơn nextAttackTime
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Attack();
                // Cập nhật thời điểm đánh tiếp theo
                // Công thức: Thời điểm hiện tại + (1 / tốc độ đánh)
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void Attack()
    {
        if (attributes.currentBambooCount < attributes.burnBambooOnAttack)
        {
            Debug.Log("Không đủ tre để đánh!");
            return;
        }

        attributes.currentBambooCount -= attributes.burnBambooOnAttack;
        Debug.Log("Người chơi đánh! Tre còn lại: " + attributes.currentBambooCount);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyAI enemyScript = enemy.GetComponent<EnemyAI>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(transform.position);
            }
        }
    }

    // ... (Giữ nguyên RegenerateBamboo và Respawn) ...
    void RegenerateBamboo()
    {
        if (attributes.currentBambooCount < attributes.maxBambooCount)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= 1f)
            {
                attributes.currentBambooCount = Mathf.Min(attributes.currentBambooCount + (int)bambooRegenRate, attributes.maxBambooCount);
                regenTimer = 0;
            }
        }
    }

    public void Respawn()
    {
        Debug.Log("Đang hồi sinh Player...");
        transform.position = startPosition;
        attributes.healthPoint = 1;
        attributes.currentBambooCount = attributes.maxBambooCount;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}