using UnityEngine;

public class AttackMode : MonoBehaviour
{
    public PlayerAttributes attributes;
    private Vector3 startPosition;
    private Rigidbody2D rb;

    [Header("Cấu hình đòn đánh")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int bambooCostPerAttack = 5; // Ép cứng hoặc chỉnh ở đây

    [Header("Thời gian hồi chiêu")]
    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;
    private bool canAttack = true;

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

        // Kiểm tra hồi chiêu
        if (Time.time >= nextAttackTime)
        {
            canAttack = true;
        }

        // Thực hiện đánh khi nhấn Fire1 (Chuột trái)
        if (Input.GetButtonDown("Fire1") && canAttack)
        {
            if (attributes.currentBambooCount >= bambooCostPerAttack)
            {
                ExecuteCombat();
            }
            else
            {
                Debug.Log("Không đủ Bamboo để đánh!");
            }
        }
    }

    void ExecuteCombat()
    {
        canAttack = false;
        nextAttackTime = Time.time + attackCooldown;
        Attack();
    }

    void Attack()
    {
        // Trừ 5 Bamboo mỗi lần đánh
        attributes.currentBambooCount -= bambooCostPerAttack;

        // Quét tất cả vật thể trong tầm đánh
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        if (hitObjects.Length == 0)
        {
            Debug.Log("Hụt! Không trúng đối tượng nào ở Layer được chọn.");
        }

        foreach (Collider2D obj in hitObjects)
        {
            Debug.Log("Đã chạm vào: " + obj.name);

            // 1. Xử lý Enemy
            EnemyAttack enemy = obj.GetComponent<EnemyAttack>();
            if (enemy != null)
            {
                enemy.TakeDamage(transform.position);
            }

            // 2. Xử lý Vật thể phá hủy (Thùng, hòm...)
            DestructibleObject destructible = obj.GetComponent<DestructibleObject>();
            if (destructible != null)
            {
                destructible.TakeDamage();
            }

            // 3. Xử lý Bẫy mồi nhử (TrapTrigger)
            TrapTrigger trap = obj.GetComponent<TrapTrigger>();
            if (trap != null)
            {
                trap.OnBlockDestroyed();
            }
        }
    }

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
        transform.position = startPosition;
        attributes.healthPoint = 1;
        attributes.currentBambooCount = attributes.maxBambooCount;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        canAttack = true;
    }

    public void UpdateCheckpoint(Vector3 newPos)
    {
        startPosition = newPos;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}