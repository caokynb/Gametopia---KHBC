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

    [Header("Thời gian hồi chiêu (Số giây phải chờ)")]
    // SỬA TẠI ĐÂY: Đổi attackRate thành attackCooldown
    // Ví dụ: Nhập 2 nghĩa là phải chờ đúng 2 giây mới được đánh tiếp
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

        if (Time.time >= nextAttackTime)
        {
            canAttack = true;
        }

        if (Input.GetButtonDown("Fire1") && canAttack)
        {
            ExecuteCombat();
        }
    }

    void ExecuteCombat()
    {
        if (attributes.currentBambooCount < attributes.burnBambooOnAttack) return;

        canAttack = false;

        // SỬA TẠI ĐÂY: Không chia nữa, cộng thẳng số giây cooldown vào thời gian hiện tại
        nextAttackTime = Time.time + attackCooldown;

        Attack();
    }

    void Attack()
    {
        attributes.currentBambooCount -= attributes.burnBambooOnAttack;
        // Debug lại cho dễ nhìn
        Debug.Log("Đã đánh! Cần chờ " + attackCooldown + " giây để đánh tiếp.");

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D obj in hitObjects)
        {
            EnemyAttack enemy = obj.GetComponent<EnemyAttack>();
            if (enemy != null)
            {
                enemy.TakeDamage(transform.position);
                continue;
            }

            DestructibleObject destructible = obj.GetComponent<DestructibleObject>();
            if (destructible != null)
            {
                destructible.TakeDamage();
            }
        }
    }

    // --- CÁC HÀM KHÁC GIỮ NGUYÊN ---
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