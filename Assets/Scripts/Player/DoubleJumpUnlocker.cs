using UnityEngine;

public class DoubleJumpUnlocker : MonoBehaviour
{
    // Cảm biến phát hiện khi Anh Khoai chạm vào vật phẩm
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem người chạm vào có đúng là Player không
        if (collision.CompareTag("Player"))
        {
            // Lấy script di chuyển của người chơi
            PlayerMovement player = collision.GetComponent<PlayerMovement>();

            if (player != null)
            {
                // MỞ KHÓA DOUBLE JUMP!
                player.canJumpOnBamboo = true;

                // Báo cáo ra Console để kiểm tra
                Debug.Log("<color=green>ĐÃ MỞ KHÓA:</color> Kỹ năng Double Jump (Nhảy trên tre lơ lửng)!");

                // Hủy vật phẩm này biến mất khỏi Scene
                Destroy(gameObject);
            }
        }
    }
}