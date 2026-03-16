using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement; // BẮT BUỘC THÊM DÒNG NÀY ĐỂ LƯU TÊN MAP

public class Checkpoint : MonoBehaviour
{
    private static List<Checkpoint> allCheckpoints = new List<Checkpoint>();
    private bool isPlayerNear = false;
    private PlayerMovement playerMovement;
    private ConstructionMode constructionScript;
    private Animator anim;
    private bool isActivated = false;

    // --- Biến để tránh người chơi bấm F liên tục khi đang chạy anim ---
    private bool isProcessing = false;

    void Awake() { allCheckpoints.Add(this); }
    void OnDestroy() { allCheckpoints.Remove(this); }

    void Start()
    {
        anim = GetComponent<Animator>();
        constructionScript = Object.FindFirstObjectByType<ConstructionMode>();
        if (PlayerMovement.hasCheckpoint && (Vector2)transform.position == PlayerMovement.respawnPosition)
        {
            SetCheckpointVisual(true);
        }
    }

    void Update()
    {
        // Kiểm tra thêm !isProcessing
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F) && !isProcessing && !isActivated)
        {
            StartCoroutine(ProcessCheckpointActivation());
        }
    }

    IEnumerator ProcessCheckpointActivation()
    {
        isProcessing = true;
        float animDuration = 1.0f; // Độ dài hoạt ảnh thắp hương 

        // 1. Gọi Anh Khoai diễn hoạt ảnh thắp hương
        if (playerMovement != null)
        {
            playerMovement.TriggerWishAnimation(animDuration);
        }

        // 2. Chạy hoạt ảnh "Saving" trên Miếu 
        if (anim != null)
        {
            anim.SetTrigger("StartSaving");
        }

        // 3. CHỜ hoạt ảnh chạy hết 1 vòng
        yield return new WaitForSeconds(animDuration);

        // 4. Đặt lại toàn bộ Checkpoint khác thành chưa kích hoạt
        foreach (Checkpoint cp in allCheckpoints)
        {
            cp.SetCheckpointVisual(false);
        }

        SetCheckpointVisual(true);

        // 5. Hồi phục Tre và Dọn dẹp map
        if (playerMovement != null)
        {
            playerMovement.stats.currentBambooCount = playerMovement.stats.maxBambooCount;
        }

        if (constructionScript != null)
        {
            constructionScript.ClearAllSpawnedBamboo();
        }

        // 6. LƯU VỊ TRÍ HỒI SINH TẠM THỜI (Soft Save)
        PlayerMovement.respawnPosition = transform.position;
        PlayerMovement.hasCheckpoint = true;

        // 7. LƯU TIẾN ĐỘ VÀO Ổ CỨNG (Hard Save cho Main Menu)
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedLevel", currentScene);
        PlayerPrefs.Save();

        isProcessing = false;
        Debug.Log("<color=lime>Checkpoint chính thức được lưu sau khi thắp hương!</color> Đã lưu Map: " + currentScene);
    }

    public void SetCheckpointVisual(bool active)
    {
        isActivated = active;
        if (anim != null)
        {
            anim.SetBool("isSaved", active);
        }
    }

    // --- VA CHẠM GIỮ NGUYÊN ---
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