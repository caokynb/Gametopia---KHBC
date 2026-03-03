using UnityEngine;

public class BossRock : MonoBehaviour
{
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
        // SỬA TẠI ĐÂY: Bỏ qua nếu đá chạm vào bất kỳ ai thuộc Layer "Enemy" (bao gồm cả Boss)
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
        // Xử lý khi rơi xuống đất
        else if (((1 << hitObject.layer) & LayerMask.GetMask("Ground")) != 0)
        {
            Destroy(gameObject); // Đá vỡ
        }
    }
}