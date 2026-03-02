using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Chỉ số cơ bản")]
    public int health = 5;
    public float chaseSpeed = 4f;
    public float returnSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public float attackRate = 1.5f;

    [Header("Cấu hình Đẩy lùi")]
    public float knockbackForce = 12f; // Tăng lên 12 để thấy rõ hơn
    public float knockbackDuration = 0.25f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;

    private Vector3 homePosition;
    private float nextAttackTime;
    private bool isKnockbacked = false;
    private bool isWaiting = false;
    private bool movingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        homePosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        // Nếu đang bị bật lùi thì thoát hẳn Update, không cho phép di chuyển
        if (player == null || isKnockbacked || isWaiting) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else if (Vector2.Distance(transform.position, homePosition) > 0.5f)
        {
            if (!isWaiting) StartCoroutine(WaitAndGoHome());
        }
        else
        {
            StopMoving();
        }
    }

    public void TakeDamage(Vector2 playerPosition)
    {
        health--;
        Debug.Log("Quái bị trúng đòn! Máu còn: " + health);

        // Hiệu ứng màu đỏ
        StopCoroutine("FlashRed");
        StartCoroutine(FlashRed());

        // Hiệu ứng đẩy lùi
        StopCoroutine("ApplyKnockback");
        StartCoroutine(ApplyKnockback(playerPosition));

        if (health <= 0) Die();
    }

    private IEnumerator ApplyKnockback(Vector2 playerPos)
    {
        isKnockbacked = true;

        // Tính toán hướng đẩy
        Vector2 direction = ((Vector2)transform.position - playerPos).normalized;

        // Quan trọng: Phải reset vận tốc và thêm lực tức thời
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(direction.x, 0.5f) * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockbacked = false;
    }

    IEnumerator FlashRed()
    {
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            sr.color = Color.white;
        }
    }

    void ChasePlayer()
    {
        MoveTowards(player.position.x, chaseSpeed);
    }

    void MoveTowards(float targetX, float speed)
    {
        float direction = (targetX > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        if (direction > 0 && !movingRight) Flip();
        else if (direction < 0 && movingRight) Flip();
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void Flip()
    {
        movingRight = !movingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    IEnumerator WaitAndGoHome()
    {
        isWaiting = true;
        StopMoving();
        yield return new WaitForSeconds(3f);

        float stopDistance = 0.2f;
        while (Mathf.Abs(transform.position.x - homePosition.x) > stopDistance)
        {
            if (isKnockbacked || (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange))
            {
                isWaiting = false;
                yield break;
            }

            MoveTowards(homePosition.x, returnSpeed);
            yield return null;
        }

        StopMoving();
        transform.position = new Vector3(homePosition.x, transform.position.y, transform.position.z);
        isWaiting = false;
    }

    void AttackPlayer() { StopMoving(); }
    void Die() { Destroy(gameObject); }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<AttackMode>()?.Respawn();
        }
    }
}