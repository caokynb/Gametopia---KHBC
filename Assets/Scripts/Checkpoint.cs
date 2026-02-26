using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isPlayerNear = false;

    // ĐÃ SỬA: Phải lấy PlayerMovement thay vì PlayerAttributes
    private PlayerMovement playerMovement;
    private ConstructionMode constructionScript;

    // --- NEW: Cờ đánh dấu Checkpoint đã được lưu ---
    private bool isActivated = false;

    void Start()
    {
        // ĐÃ SỬA: Vì ConstructionMode nằm trên một object khác (ConstructionMode_Player),
        // ta yêu cầu Unity tự tìm nó trong màn chơi ngay từ lúc bắt đầu!
        constructionScript = Object.FindFirstObjectByType<ConstructionMode>();
    }

    void Update()
    {
        // Khi người chơi đứng trong vùng Checkpoint và bấm phím F
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            // 1. Refill Bamboo (Trỏ qua stats của PlayerMovement)
            playerMovement.stats.currentBambooCount = playerMovement.stats.maxBambooCount;
            Debug.Log("Checkpoint Used! Bamboo refilled to: " + playerMovement.stats.currentBambooCount);

            // 2. Dọn dẹp bản đồ
            if (constructionScript != null)
            {
                constructionScript.ClearAllSpawnedBamboo();
                Debug.Log("Bản đồ đã được dọn dẹp sạch sẽ!");
            }

            // 3. LƯU VỊ TRÍ HỒI SINH (Static Memory)
            PlayerMovement.respawnPosition = transform.position;
            PlayerMovement.hasCheckpoint = true;

            if (!isActivated)
            {
                isActivated = true;
                Debug.Log("<color=cyan>Checkpoint Activated!</color> Vị trí hồi sinh đã được lưu.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;

            // Lấy PlayerMovement từ nhân vật chạm vào
            playerMovement = collision.GetComponent<PlayerMovement>();

            Debug.Log("Player is near the checkpoint. Press 'F' to rest!");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;

            // Xóa bộ nhớ tạm để tránh lỗi
            playerMovement = null;
        }
    }
}