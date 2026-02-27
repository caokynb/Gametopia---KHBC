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

    private EnemyMovement movement;
    private SpriteRenderer sr;
    private Transform player;

    void Start()
    {
        movement = GetComponent<EnemyMovement>();
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null || movement.isKnockbacked) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Gọi logic di chuyển từ script Movement
        movement.HandleMovement(distanceToPlayer, detectionRange, attackRange);

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
        movement.ApplyKnockback(playerPosition);

        if (health <= 0) Die();
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
            collision.gameObject.GetComponent<PlayerCombat>()?.Respawn();
        }
    }
}