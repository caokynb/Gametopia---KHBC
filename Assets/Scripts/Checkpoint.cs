using UnityEngine;
using System.Collections.Generic; // Bắt buộc phải có để dùng List

public class Checkpoint : MonoBehaviour
{
    // --- QUẢN LÝ TẬP TRUNG ---
    // Danh sách tĩnh chứa tất cả các Miếu trong màn chơi
    private static List<Checkpoint> allCheckpoints = new List<Checkpoint>();

    private bool isPlayerNear = false;
    private PlayerMovement playerMovement;
    private ConstructionMode constructionScript;
    private Animator anim; // Linh kiện Animator của Miếu

    private bool isActivated = false;

    void Awake()
    {
        // Khi một Miếu sinh ra, nó tự ghi tên vào danh sách
        allCheckpoints.Add(this);
    }

    void OnDestroy()
    {
        // Khi Miếu bị xóa (chuyển cảnh), nó tự xóa tên khỏi danh sách
        allCheckpoints.Remove(this);
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        constructionScript = Object.FindFirstObjectByType<ConstructionMode>();

        // Kiểm tra xem đây có phải là Miếu cuối cùng người chơi đã lưu không (dành cho lúc hồi sinh)
        if (PlayerMovement.hasCheckpoint && (Vector2)transform.position == PlayerMovement.respawnPosition)
        {
            SetCheckpointVisual(true);
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            ActivateThisCheckpoint();
        }
    }

    void ActivateThisCheckpoint()
    {
        // 1. Bảo tất cả các Miếu khác TẮT hương
        foreach (Checkpoint cp in allCheckpoints)
        {
            cp.SetCheckpointVisual(false);
        }

        // 2. BẬT hương cho riêng Miếu này
        SetCheckpointVisual(true);

        // 3. Logic nạp năng lượng và lưu vị trí
        if (playerMovement != null)
        {
            playerMovement.stats.currentBambooCount = playerMovement.stats.maxBambooCount;
        }

        if (constructionScript != null)
        {
            constructionScript.ClearAllSpawnedBamboo();
        }

        PlayerMovement.respawnPosition = transform.position;
        PlayerMovement.hasCheckpoint = true;

        Debug.Log("<color=lime>Miếu đã được thắp hương!</color> Vị trí hồi sinh mới đã được lưu.");
    }

    // Hàm bổ trợ để điều khiển Animator
    public void SetCheckpointVisual(bool active)
    {
        isActivated = active;
        if (anim != null)
        {
            // Gửi tín hiệu bool "isSaved" sang Animator (Bạn cần tạo parameter này)
            anim.SetBool("isSaved", active);
        }
    }

    // --- XỬ LÝ VA CHẠM ---
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerMovement = collision.GetComponent<PlayerMovement>();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerMovement = null;
        }
    }
}