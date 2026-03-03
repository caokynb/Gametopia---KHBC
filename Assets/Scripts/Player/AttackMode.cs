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
    public int bambooCostPerAttack = 5;

    [Header("Thời gian hồi chiêu")]
    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;
    private bool canAttack = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    void Update()
    {
        // Kiểm tra hồi chiêu
        if (Time.time >= nextAttackTime)
        {
            canAttack = true;
        }

        // Thực hiện đánh khi nhấn Fire1 (Chuột trái)
        if (Input.GetButtonDown("Fire1") && canAttack)
        {
            // Kiểm tra xem còn đủ 5 Bamboo để đánh không
            if (attributes.currentBambooCount >= bambooCostPerAttack)
            {
                ExecuteCombat();
            }
            else
            {
                Debug.Log("Hết Bamboo rồi, không thể đánh!");
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
        // Bỏ dòng trừ tre ở đây
        // attributes.currentBambooCount -= bambooCostPerAttack; 

        // Quét tất cả vật thể trong tầm đánh
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        bool hasHitAnything = false; // Biến đánh dấu xem có đánh trúng gì không

        foreach (Collider2D obj in hitObjects)
        {
            bool hitValidTarget = false; // Kiểm tra mục tiêu hiện tại có hợp lệ không

            // 1. Kiểm tra TrapTrigger
            TrapTrigger trap = obj.GetComponent<TrapTrigger>();
            if (trap == null) trap = obj.GetComponentInParent<TrapTrigger>();
            if (trap != null)
            {
                trap.OnBlockDestroyed();
                hitValidTarget = true;
            }

            // 2. Kiểm tra EnemyAI
            EnemyAI enemy = obj.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(transform.position);
                hitValidTarget = true;
            }

            // 3. Kiểm tra Vật thể phá hủy
            DestructibleObject destructible = obj.GetComponent<DestructibleObject>();
            if (destructible != null)
            {
                destructible.TakeDamage();
                hitValidTarget = true;
            }

            // Nếu trúng bất kỳ cái gì ở trên, đánh dấu là đã trúng đòn
            if (hitValidTarget)
            {
                hasHitAnything = true;
            }
        }

        // CHỈ TRỪ TRE KHI CÓ ĐÁNH TRÚNG ÍT NHẤT 1 THỨ
        if (hasHitAnything)
        {
            attributes.currentBambooCount -= bambooCostPerAttack;
            // Đảm bảo tre không bị âm
            if (attributes.currentBambooCount < 0) attributes.currentBambooCount = 0;

            Debug.Log("Đã đánh trúng! Trừ " + bambooCostPerAttack + " tre. Còn lại: " + attributes.currentBambooCount);
        }
        else
        {
            Debug.Log("Đánh hụt, không mất tre.");
        }
    }

    // Khi hồi sinh thì mới nạp đầy lại Bamboo
    public void Respawn()
    {
        // KIỂM TRA CHECKPOINT TRƯỚC
        if (PlayerMovement.hasCheckpoint)
        {
            transform.position = PlayerMovement.respawnPosition;
            Debug.Log("Hồi sinh tại Checkpoint!");
        }
        else
        {
            // Nếu chưa ăn checkpoint nào thì về vị trí khởi đầu của Level
            transform.position = startPosition;
            Debug.Log("Hồi sinh tại điểm bắt đầu màn chơi!");
        }

        // Reset các chỉ số khác
        attributes.healthPoint = 1;
        attributes.currentBambooCount = attributes.maxBambooCount;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic; // Đảm bảo người chơi không bị Static
        }

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