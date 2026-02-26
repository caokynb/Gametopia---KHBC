using UnityEngine;

public class Hazard : MonoBehaviour
{
    // Sử dụng OnTriggerEnter2D để phát hiện người chơi rơi vào khoảng không hoặc chạm vào gai
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem vật thể vừa chạm vào có phải là Player không
        if (collision.CompareTag("Player"))
        {
            KillPlayer(collision.gameObject);
        }
    }

    // Dự phòng trường hợp Level Designer quên tick "Is Trigger" và dùng Collider cứng
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            KillPlayer(collision.gameObject);
        }
    }

    private void KillPlayer(GameObject player)
    {
        Debug.Log("<color=red>Chít rồi</color>");

        // Lấy script PlayerMovement của nhân vật
        PlayerMovement playerScript = player.GetComponent<PlayerMovement>();

        if (playerScript != null)
        {
            // Dựa theo GDD của bạn (1 HP / Chết khi hết tre), chúng ta ép số lượng tre về 0 
            // để hệ thống hiện tại của bạn tự động kích hoạt trạng thái Chết/Chơi lại.
            // (Nếu team bạn có viết riêng hàm playerScript.Die() thì gọi hàm đó ở đây!)
            playerScript.stats.currentBambooCount = 0;
        }
    }
}