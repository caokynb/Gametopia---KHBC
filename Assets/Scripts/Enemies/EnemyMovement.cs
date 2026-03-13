using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    [Header("Thông số di chuyển")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;
    public Transform wallCheck;
    public float wallCheckDistance = 0.3f;
    public LayerMask obstacleLayer;

    [Header("Cấu hình Đẩy lùi")]
    public float knockbackForce = 7f;
    public float knockbackDuration = 0.2f;

    [HideInInspector] public bool isKnockbacked = false;
    [HideInInspector] public bool movingRight = true;

    private Rigidbody2D rb;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void HandleMovement(float distanceToPlayer, float detectionRange, float attackRange)
    {
        if (isKnockbacked || player == null) return;

        if (distanceToPlayer <= attackRange)
        {
            StopMoving();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);
        RaycastHit2D wallInfo = Physics2D.Raycast(wallCheck.position, movingRight ? Vector2.right : Vector2.left, wallCheckDistance, obstacleLayer);
        if (wallInfo.collider != null) Flip();
    }

    void ChasePlayer()
    {
        float direction = (player.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
        if ((direction > 0 && !movingRight) || (direction < 0 && movingRight)) Flip();
    }

    public void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void ApplyKnockback(Vector2 playerPosition)
    {
        StartCoroutine(KnockbackCoroutine(playerPosition));
    }

    private IEnumerator KnockbackCoroutine(Vector2 playerPos)
    {
        isKnockbacked = true;
        Vector2 direction = ((Vector2)transform.position - playerPos).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        isKnockbacked = false;
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}