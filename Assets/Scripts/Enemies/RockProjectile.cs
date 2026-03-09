using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    public float rotationSpeed = 300f;
    public float lifeTime = 4f;

    void Start()
    {
        // Tự động hủy viên đá sau 4 giây để dọn rác bộ nhớ
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Làm viên đá xoay vòng tròn trên không trung
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Nếu trúng Anh Khoai
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
            Destroy(gameObject); // Đá vỡ
        }
        // 2. Nếu trúng đốt tre (Bamboo)
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Bamboo"))
        {
            // Phá hủy chính đốt tre đó
            Destroy(collision.gameObject);

            // Chú ý: Nếu Loc thiết kế đốt tre có máu (ví dụ cần ném trúng 2-3 lần mới vỡ),
            // bạn có thể đổi dòng Destroy ở trên thành gọi script của tre, ví dụ:
            // collision.GetComponent<BambooScript>()?.TakeDamage(1);

            Destroy(gameObject); // Đá vỡ sau khi phá tre
        }
        // 3. Nếu trúng mặt đất (Ground) tường đá bình thường
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject); // Đá vỡ vô hại
        }
    }
}