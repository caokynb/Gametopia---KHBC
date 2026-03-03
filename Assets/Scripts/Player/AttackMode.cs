using UnityEngine;

public class AttackMode : MonoBehaviour
{
    [Header("Liên kết Dữ liệu")]
    public PlayerAttributes attributes;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 startPosition;

    [Header("Cấu hình Hitbox (Hình Chữ Nhật)")]
    // Điểm xuất phát của tâm hình chữ nhật
    public Transform attackPoint;

    // SỬA TẠI ĐÂY: Dùng Vector2 để điều chỉnh Rộng (x) và Cao (y)
    // Bạn có thể nhập (2.0, 1.0) để có hình chữ nhật dài trước mặt
    public Vector2 attackBoxSize = new Vector2(2.0f, 1.0f);

    // Góc xoay của hình chữ nhật (mặc định là 0)
    public float attackBoxAngle = 0f;

    public LayerMask enemyLayers;   // Lớp vật thể là kẻ địch

    [Header("Thời gian hồi chiêu")]
    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;
    private bool canAttack = true;

    [Header("Cài đặt hồi Bamboo")]
    public float bambooRegenRate = 5f;
    private float regenTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startPosition = transform.position;
    }

    void Update()
    {
        HandleBambooRegen();

        // Kiểm tra cooldown
        if (Time.time >= nextAttackTime) canAttack = true;

        // BƯỚC 1: NHẬN INPUT - CHỈ KÍCH HOẠT HOẠT ẢNH
        if (Input.GetButtonDown("Fire1") && canAttack)
        {
            StartAttackSequence();
        }
    }

    void StartAttackSequence()
    {
        // Kiểm tra năng lượng trước khi vung kiếm
        if (attributes.currentBambooCount < attributes.burnBambooOnAttack) return;

        canAttack = false;
        nextAttackTime = Time.time + attackCooldown;

        // Trừ năng lượng ngay khi quyết định vung kiếm
        attributes.currentBambooCount -= attributes.burnBambooOnAttack;

        // Kích hoạt animation Slash
        if (anim != null)
        {
            anim.SetTrigger("Slash");
        }
    }

    // BƯỚC 2: GÂY SÁT THƯƠNG THỰC TẾ (ĐƯỢC GỌI BỞI ANIMATION EVENT)
    // Bạn hãy gán hàm này vào frame đẹp nhất nhé!
    public void TriggerDamage()
    {
        Debug.Log("<color=lime>Anh Khoai đã chém trúng tầm đánh (Hình Chữ Nhật)!</color>");

        // SỬA TẠI ĐÂY: Dùng OverlapBoxAll để quét hình chữ nhật
        Collider2D[] hitObjects = Physics2D.OverlapBoxAll(
            attackPoint.position, // Tâm của hình chữ nhật
            attackBoxSize,        // Kích thước (Rộng, Cao)
            attackBoxAngle,       // Góc xoay
            enemyLayers           // Lớp kẻ địch
        );

        bool hasHitAnything = false; // Biến đánh dấu xem có đánh trúng gì không

        foreach (Collider2D obj in hitObjects)
        {
            // Kiểm tra Kẻ địch
            EnemyAttack enemy = obj.GetComponent<EnemyAttack>();
            if (enemy != null)
            {
                enemy.TakeDamage(transform.position);
                continue; // Chém trúng rồi thì bỏ qua các check bên dưới
            }

            // 2. CHÉM BOSS (Đây là đoạn code bạn đang thiếu!)
            BossAI boss = obj.GetComponent<BossAI>();
            if (boss != null)
            {
                boss.TakeDamage(transform.position);
                continue; // Chém trúng rồi thì bỏ qua các check bên dưới
            }

            // 3. Phá vật thể môi trường
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
            EnemyAI enemy1 = obj.GetComponent<EnemyAI>();
            if (enemy1 != null)
            {
                enemy1.TakeDamage(transform.position);
                hitValidTarget = true;
            }

            // Kiểm tra vật thể phá hủy (thùng, cây...)
            DestructibleObject destructible = obj.GetComponent<DestructibleObject>();
            if (destructible != null)
            {
                destructible.TakeDamage();
                hitValidTarget = true;
            }

    void HandleBambooRegen()
    {
        if (attributes.currentBambooCount < attributes.maxBambooCount)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= 1f)
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

    // --- Các hàm hỗ trợ hệ thống ---
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

    public void UpdateCheckpoint(Vector3 newPos) => startPosition = newPos;

    // SỬA TẠI ĐÂY: Cập nhật Gizmos để vẽ hình chữ nhật
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.cyan; // Đổi màu cho dễ phân biệt

        // Lấy ma trận xoay của attackPoint để hình chữ nhật quay theo nhân vật
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(
            attackPoint.position,
            attackPoint.rotation,
            Vector3.one
        );
        Gizmos.matrix = rotationMatrix;

        // Vẽ hình chữ nhật rỗng
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);

        // Khôi phục ma trận Gizmos về mặc định
        Gizmos.matrix = Matrix4x4.identity;
    }
}