using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    [Header("Cài đặt Viên đá")]
    public float speed = 7f;
    public int damage = 1;
    public float lifeTime = 3f;

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

    // ---------------------------------------------------
    // TRƯỜNG HỢP 1: NẾU VIÊN ĐÁ LÀ "IS TRIGGER" (Xuyên thấu)
    // ---------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckHit(collision.gameObject);
    }

    // ---------------------------------------------------
    // TRƯỜNG HỢP 2: NẾU VIÊN ĐÁ LÀ VẬT THỂ CỨNG (Không bật Trigger)
    // ---------------------------------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckHit(collision.gameObject);
    }

    // --- Hàm xử lý trừ máu chung ---
    void CheckHit(GameObject hitObject)
    {
        // Nếu chạm vào Anh Khoai
        if (hitObject.CompareTag("Player"))
        {
            PlayerMovement player = hitObject.GetComponent<PlayerMovement>();

            if (player != null)
            {
                // GỌI HÀM TAKEDAMAGE CỦA BẠN THAY VÌ TỰ TRỪ SỐ!
                player.TakeDamage(damage);
                Debug.Log("💥 ÁU! Khỉ ném trúng rồi!");
            }

            Destroy(gameObject); // Vỡ vụn
        }
        // Nếu đập vào Đất/Tường
        else if (hitObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}