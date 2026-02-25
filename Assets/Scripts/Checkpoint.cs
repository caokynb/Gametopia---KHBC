using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isPlayerNear = false;
    private PlayerAttributes playerScript;

    // --- NEW: Thêm một biến để giữ liên lạc với script Xây dựng ---
    private ConstructionMode constructionScript;

    void Update()
    {
        // If the player is standing inside the checkpoint zone AND presses 'E'
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            // Refill the current bamboo to match the max bamboo limit!
            playerScript.currentBambooCount = playerScript.maxBambooCount;

            Debug.Log("Checkpoint Used! Bamboo refilled to: " + playerScript.currentBambooCount);

            // Note: Later on, you can add code here to save the game, heal the player, or play a shiny particle effect!

            // --- NEW: Kích hoạt lệnh xóa tre ---
            if (constructionScript != null)
            {
                constructionScript.ClearAllSpawnedBamboo();
                Debug.Log("Bản đồ đã được dọn dẹp sạch sẽ!");
            }
        }
    }

    // This runs the exact frame the player touches the checkpoint
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object that touched us has the "Player" tag
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;

            // Grab the PlayerMovement script from the player so we can access their 'stats'
            playerScript = collision.GetComponent<PlayerAttributes>();

            // Lấy thêm script Xây dựng từ trên người Player
            constructionScript = collision.GetComponent<ConstructionMode>();

            Debug.Log("Player is near the checkpoint. Press 'F' to rest!");
        }
    }

    // This runs the exact frame the player walks away from the checkpoint
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerScript = null; // Clear the script to prevent errors
            constructionScript = null; // Cắt liên lạc khi rời đi
        }
    }
}