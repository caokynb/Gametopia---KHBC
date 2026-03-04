using UnityEngine;

public class AttackMode : MonoBehaviour
{
    public PlayerAttributes attributes;
    private Vector3 startPosition;
    private Rigidbody2D rb;
<<<<<<< HEAD
    private Animator anim; // Thêm biến Animator

=======

    // [THÊM MỚI]: Khai báo biến Animator
    private Animator anim;

>>>>>>> 4da0d32631e27279c5559850c9e40425a99c9b33
    [Header("Cấu hình đòn đánh")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
<<<<<<< HEAD
    public int bambooCostPerAttack = 5;

    [Header("Thời gian hồi chiêu")]
=======

    [Header("Thời gian hồi chiêu (Số giây phải chờ)")]
>>>>>>> 4da0d32631e27279c5559850c9e40425a99c9b33
    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;
    private bool canAttack = true;

<<<<<<< HEAD
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // Lấy component Animator từ nhân vật
=======
    [Header("Cài đặt hồi năng lượng")]
    public float bambooRegenRate = 5f;
    private float regenTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // [THÊM MỚI]: Lấy component Animator từ nhân vật
        anim = GetComponent<Animator>();
>>>>>>> 4da0d32631e27279c5559850c9e40425a99c9b33
        startPosition = transform.position;
    }

    void Update()
    {
<<<<<<< HEAD
        if (Time.time >= nextAttackTime)
        {
            canAttack = true;
        }

        if (Input.GetButtonDown("Fire1") && canAttack)
        {
            if (attributes.currentBambooCount >= bambooCostPerAttack)
            {
                ExecuteCombat();
            }
            else
            {
                Debug.Log("Hết Bamboo rồi, không thể đánh!");
            }
=======
        RegenerateBamboo();

        if (Time.time >= nextAttackTime)
        {
            canAttack = true;
        }

        if (Input.GetButtonDown("Fire1") && canAttack)
        {
            ExecuteCombat();
>>>>>>> 4da0d32631e27279c5559850c9e40425a99c9b33
        }
    }

    void ExecuteCombat()
    {
<<<<<<< HEAD
        canAttack = false;
        nextAttackTime = Time.time + attackCooldown;

        // KÍCH HOẠT ANIMATION SLASH
=======
        if (attributes.currentBambooCount < attributes.burnBambooOnAttack) return;

        canAttack = false;
        nextAttackTime = Time.time + attackCooldown;

        // [THÊM MỚI]: Gửi tín hiệu Trigger có tên "Slash" sang Animator để phát hình ảnh chém
>>>>>>> 4da0d32631e27279c5559850c9e40425a99c9b33
        if (anim != null)
        {
            anim.SetTrigger("Slash");
        }

        Attack();
    }

    void Attack()
    {
<<<<<<< HEAD
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        bool hasHitAnything = false;

        foreach (Collider2D obj in hitObjects)
        {
            bool hitValidTarget = false;

            TrapTrigger trap = obj.GetComponent<TrapTrigger>();
            if (trap == null) trap = obj.GetComponentInParent<TrapTrigger>();
            if (trap != null)
            {
                trap.OnBlockDestroyed();
                hitValidTarget = true;
            }

            BossAI boss = obj.GetComponent<BossAI>();
            if (boss != null)
            {
                boss.TakeDamage(transform.position);
                hitValidTarget = true;
            }

            EnemyAI enemy = obj.GetComponent<EnemyAI>();
=======
        attributes.currentBambooCount -= attributes.burnBambooOnAttack;
        Debug.Log("Đã đánh! Cần chờ " + attackCooldown + " giây để đánh tiếp.");

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D obj in hitObjects)
        {
            EnemyAttack enemy = obj.GetComponent<EnemyAttack>();
>>>>>>> 4da0d32631e27279c5559850c9e40425a99c9b33
            if (enemy != null)
            {
                enemy.TakeDamage(transform.position);
                hitValidTarget = true;
            }

            DestructibleObject destructible = obj.GetComponent<DestructibleObject>();
            if (destructible != null)
            {
                destructible.TakeDamage();
                hitValidTarget = true;
            }

<<<<<<< HEAD
            if (hitValidTarget) hasHitAnything = true;
        }

        if (hasHitAnything)
=======
    void RegenerateBamboo()
    {
        if (attributes.currentBambooCount < attributes.maxBambooCount)
>>>>>>> 4da0d32631e27279c5559850c9e40425a99c9b33
        {
            attributes.currentBambooCount -= bambooCostPerAttack;
            if (attributes.currentBambooCount < 0) attributes.currentBambooCount = 0;
            Debug.Log("Đã đánh trúng! Trừ tre.");
        }
    }

<<<<<<< HEAD
    // Các hàm Respawn và UpdateCheckpoint giữ nguyên...
=======
>>>>>>> 4da0d32631e27279c5559850c9e40425a99c9b33
    public void Respawn()
    {
        if (PlayerMovement.hasCheckpoint)
            transform.position = PlayerMovement.respawnPosition;
        else
            transform.position = startPosition;

        attributes.healthPoint = 1;
        attributes.currentBambooCount = attributes.maxBambooCount;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        // Reset animation khi hồi sinh (để tránh bị kẹt ở pose đánh)
        if (anim != null) anim.Rebind();

        canAttack = true;
    }

<<<<<<< HEAD
=======
    public void UpdateCheckpoint(Vector3 newPos)
    {
        startPosition = newPos;
    }

>>>>>>> 4da0d32631e27279c5559850c9e40425a99c9b33
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}