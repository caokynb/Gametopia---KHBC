using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Thông số di chuyển")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;
    public Transform wallCheck;
    public float wallCheckDistance = 0.3f;
    public LayerMask obstacleLayer;

    [Header("Phát hiện & Tấn công")]
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public float attackRate = 1.5f;
    public float damage = 10f;

    private Rigidbody2D rb;
    private Transform player;
    private bool movingRight = true;
    private float nextAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // TÌM PLAYER: Quan trọng nhất là Player phải có Tag "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("KHÔNG TÌM THẤY PLAYER! Hãy đặt Tag 'Player' cho nhân vật của bạn.");
        }
    }

    void Update()
    {
        // Nếu không có player, chỉ đi tuần tra bình thường
        if (player == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // ƯU TIÊN TRẠNG THÁI
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
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
        // Di chuyển dựa trên hướng movingRight
        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);

        // Check tường để quay đầu
        RaycastHit2D wallInfo = Physics2D.Raycast(wallCheck.position, movingRight ? Vector2.right : Vector2.left, wallCheckDistance, obstacleLayer);

        if (wallInfo.collider != null)
        {
            Flip();
        }
    }

    void ChasePlayer()
    {
        // Tính hướng x tới player (-1 hoặc 1)
        float direction = (player.position.x > transform.position.x) ? 1 : -1;

        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

        // Quay mặt theo hướng đuổi
        if (direction > 0 && !movingRight) Flip();
        else if (direction < 0 && movingRight) Flip();
    }

    void AttackPlayer()
    {
        // Dừng lại khi đủ gần để đánh
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (Time.time >= nextAttackTime)
        {
            Debug.Log("Enemy đang đánh!");
            // Thực hiện logic gây sát thương ở đây
            nextAttackTime = Time.time + attackRate;
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}