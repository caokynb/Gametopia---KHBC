using UnityEngine;

public class Hazard : MonoBehaviour
{
    // Sử dụng OnTriggerEnter2D để phát hiện người chơi rơi vào khoảng không hoặc chạm vào gai
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            InstantKill(collision.gameObject);
        }
    }

    // Dự phòng trường hợp Level Designer quên tick "Is Trigger" và dùng Collider cứng
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            InstantKill(collision.gameObject);
        }
    }

    private void InstantKill(GameObject player)
    {
        // Lấy script PlayerMovement của nhân vật
        PlayerMovement playerScript = player.GetComponent<PlayerMovement>();

        if (playerScript != null)
        {
            // Gọi hàm TakeDamage với 9999 sát thương để đảm bảo máu về 0 ngay lập tức
            playerScript.TakeDamage(9999);

            Debug.Log("<color=red>Tử ẹo!</color> Anh Khoai đã chạm phải bẫy tử thần.");
        }
    }
}