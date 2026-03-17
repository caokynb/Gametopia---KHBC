using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    [Header("Cài đặt Viên đá")]
    public float speed = 7f;
    public int damage = 1;
    public float lifeTime = 3f;

    [Header("Tên Layer của Tre")]
    public string bambooLayerName = "Bamboo"; // <--- Nhớ đổi tên này thành Layer Tre của bạn trong Unity

    void Start()
    {
        // Tự hủy sau 3 giây để tránh làm nặng game
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Đá bay thẳng
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckHit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckHit(collision.gameObject);
    }

    // --- Hàm xử lý va chạm ---
    void CheckHit(GameObject hitObject)
    {
        // 1. Nếu chạm vào Anh Khoai
        if (hitObject.CompareTag("Player"))
        {
            PlayerMovement player = hitObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("💥 ÁU! Khỉ ném trúng rồi!");
            }
            Destroy(gameObject);
            return;
        }

        // 2. [MỚI] Nếu chạm vào Layer Tre
        if (hitObject.layer == LayerMask.NameToLayer(bambooLayerName))
        {
            Debug.Log("💥 Đá khỉ đã đập trúng Layer Tre!");

            // Tìm script BambooSegment gắn trên đốt tre bị ném trúng
            BambooSegment bamboo = hitObject.GetComponent<BambooSegment>();

            if (bamboo != null)
            {
                // Gọi hàm TakeDamage của đốt tre và truyền lượng damage của viên đá vào
                bamboo.TakeDamage(damage);
            }

            // Dù tre chưa vỡ thì viên đá đập vào cũng phải vỡ vụn
            Destroy(gameObject);
            return;
        }

        
    }
}