using UnityEngine;

public class BossRock : MonoBehaviour
{
    void Start()
    {
        // Tự động vỡ vụn sau 5 giây dù có trúng ai hay không
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject hitObject)
    {
        // Bỏ qua nếu đá chạm vào Boss hoặc quái khác
        if (hitObject.layer == LayerMask.NameToLayer("Enemy")) return;

        // Xử lý khi trúng Anh Khoai
        if (hitObject.CompareTag("Player"))
        {
            PlayerMovement pMovement = hitObject.GetComponent<PlayerMovement>();
            if (pMovement != null)
            {
                pMovement.TakeDamage(1);
                Debug.Log("<color=yellow>BỐP!</color> Trúng đá tảng!");
            }
            Destroy(gameObject); // Đá vỡ
        }
        // Xử lý khi trúng khiên Tre
        else if (((1 << hitObject.layer) & LayerMask.GetMask("Bamboo")) != 0)
        {
            Debug.Log("Đá đập trúng khiên Tre và vỡ vụn!");
            Destroy(hitObject); // Phá vỡ tre
            Destroy(gameObject); // Đá vỡ
        }

        // ĐÃ XÓA: Đoạn code phá hủy đá khi chạm đất (Ground)
        // Bây giờ đá sẽ rơi xuống, nảy/lăn trên mặt đất, và tự biến mất sau 5s nhờ hàm Start()
    }
}