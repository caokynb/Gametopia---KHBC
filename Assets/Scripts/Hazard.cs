using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Cài đặt loại bẫy")]
    [SerializeField] bool isAbyss = false; // Tích vào nếu đây là Vực thẳm

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TryKillPlayer(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryKillPlayer(collision.gameObject);
        }
    }

    private void TryKillPlayer(GameObject player)
    {
        // Lấy thông số của người chơi
        PlayerAttributes stats = player.GetComponent<PlayerAttributes>();

        if (stats != null)
        {
            // KIỂM TRA CƠ CHẾ BẤT TỬ
            // Nếu là Vực thẳm (isAbyss) thì CHẾT LUÔN
            // Nếu là bẫy thường (Gai) mà KHÔNG bất tử thì mới CHẾT
            if (isAbyss || !stats.isInvulnerable)
            {
                KillPlayer(player, stats);
            }
            else
            {
                Debug.Log("<color=yellow>Đang bất tử, bẫy này không làm gì được!</color>");
            }
        }
    }

    private void KillPlayer(GameObject player, PlayerAttributes stats)
    {
        Debug.Log("<color=red>Chít rồi!</color>");

        // Trừ máu hoặc tre tùy theo logic của bạn
        stats.currentBambooCount = 0;
        stats.healthPoint = 0;

        // Nếu bạn có hàm xử lý chết riêng (như Load lại Scene) thì gọi ở đây
    }
}