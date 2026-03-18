using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Cài đặt Sát thương")]
    [Tooltip("Số máu (hoặc điểm) bị trừ khi Anh Khoai đụng phải bẫy này")]
    public int damageAmount = 1;

    // Sử dụng OnTriggerEnter2D để phát hiện người chơi rơi vào khoảng không hoặc chạm vào gai
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DealDamage(collision.gameObject);
        }
    }

    // Dự phòng trường hợp Level Designer quên tick "Is Trigger" và dùng Collider cứng
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DealDamage(collision.gameObject);
        }
    }

    private void DealDamage(GameObject player)
    {
        // Lấy script PlayerMovement của nhân vật
        PlayerMovement playerScript = player.GetComponent<PlayerMovement>();

        if (playerScript != null)
        {
            // Gọi hàm TakeDamage giống hệt như cách quái vật tấn công Anh Khoai
            playerScript.TakeDamage(damageAmount);

            Debug.Log($"<color=orange>Đạp bẫy!</color> Anh Khoai vừa mất {damageAmount} máu.");
        }
    }
}