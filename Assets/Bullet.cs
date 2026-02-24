using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 30f;      // Tăng tốc độ mặc định
    public float damage = 20f;
    public float lifeTime = 5f;    // Thêm biến thời gian tồn tại
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (rb != null)
        {
            // Đảm bảo Rigidbody2D là Kinematic hoặc Gravity Scale = 0 để đạn bay thẳng
            rb.linearVelocity = transform.right * speed;
        }

        // Đạn biến mất sau lifeTime nếu không chạm gì
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. Tránh đạn va chạm với chính người bắn (giả sử Player có Tag là "Player")
        if (hitInfo.CompareTag("Player"))
        {
            return;
        }

        // 2. Xử lý gây sát thương
        Health target = hitInfo.GetComponent<Health>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }

        // 3. Hiệu ứng nổ (nếu có) rồi mới hủy
        Destroy(gameObject);
    }
}