using UnityEngine;
using System.Collections.Generic;

public class AttackMode : MonoBehaviour
{
    // [CẬP NHẬT] Đổi từ việc dùng biến attributes riêng sang lấy thẳng từ PlayerMovement
    private PlayerMovement playerMovement;

    private Vector3 startPosition;
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Cấu hình đòn đánh")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int bambooCostPerAttack = 5;

    [Header("Thời gian hồi chiêu")]
    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;
    private bool canAttack = true;

    [Header("Hiệu ứng (VFX)")]
    public GameObject hitVFXPrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Tìm túi tre chính chủ của Anh Khoai để dùng chung
        playerMovement = GetComponent<PlayerMovement>();

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
            // Kiểm tra túi tre chính
            if (playerMovement.stats.currentBambooCount >= bambooCostPerAttack)
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

        if (anim != null)
        {
            anim.SetTrigger("Slash");
        }

        Attack();
    }

    void Attack()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayers);
        filter.useLayerMask = true;
        filter.useTriggers = true;

        List<Collider2D> hitObjects = new List<Collider2D>();
        Physics2D.OverlapCircle(attackPoint.position, attackRange, filter, hitObjects);

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

            if (hitValidTarget)
            {
                hasHitAnything = true;
                if (hitVFXPrefab != null)
                {
                    Instantiate(hitVFXPrefab, obj.transform.position, Quaternion.identity);
                }
            }
        }

        if (hasHitAnything)
        {
            // TRỪ VÀO TÚI TRE CHÍNH
            playerMovement.stats.currentBambooCount -= bambooCostPerAttack;
            if (playerMovement.stats.currentBambooCount < 0) playerMovement.stats.currentBambooCount = 0;
            Debug.Log("Đã đánh trúng! Số tre hiện tại: " + playerMovement.stats.currentBambooCount);
        }
    }

    public void Respawn()
    {
        if (PlayerMovement.hasCheckpoint)
            transform.position = PlayerMovement.respawnPosition;
        else
            transform.position = startPosition;

        // RESET VÀO TÚI TRE CHÍNH
        playerMovement.stats.healthPoint = 1;
        playerMovement.stats.currentBambooCount = playerMovement.stats.maxBambooCount;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

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