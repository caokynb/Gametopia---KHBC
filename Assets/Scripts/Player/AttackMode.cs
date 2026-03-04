using UnityEngine;

public class AttackMode : MonoBehaviour
{
    public PlayerAttributes attributes;
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private Animator anim; // Thêm biến Animator

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
        anim = GetComponent<Animator>(); // Lấy component Animator từ nhân vật
        startPosition = transform.position;
    }

    void Update()
    {
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
        }
    }

    void ExecuteCombat()
    {
        canAttack = false;
        nextAttackTime = Time.time + attackCooldown;

        // KÍCH HOẠT ANIMATION SLASH
        if (anim != null)
        {
            anim.SetTrigger("Slash");
        }

        Attack();
    }

    void Attack()
    {
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

            if (hitValidTarget) hasHitAnything = true;
        }

        if (hasHitAnything)
        {
            attributes.currentBambooCount -= bambooCostPerAttack;
            if (attributes.currentBambooCount < 0) attributes.currentBambooCount = 0;
            Debug.Log("Đã đánh trúng! Trừ tre.");
        }
    }

    // Các hàm Respawn và UpdateCheckpoint giữ nguyên...
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

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}