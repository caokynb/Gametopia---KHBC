using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Cần thêm cái này để dùng Coroutine

public class Checkpoint : MonoBehaviour
{
    private static List<Checkpoint> allCheckpoints = new List<Checkpoint>();
    private bool isPlayerNear = false;
    private PlayerMovement playerMovement;
    private ConstructionMode constructionScript;
    private Animator anim;
    private bool isActivated = false;

    // --- NEW: Biến để tránh người chơi bấm F liên tục khi đang chạy anim ---
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
        float animDuration = 1.0f; // Độ dài hoạt ảnh thắp hương (bạn hãy chỉnh theo giây)

        // 1. Gọi Anh Khoai diễn hoạt ảnh thắp hương
        if (playerMovement != null)
        {
            playerMovement.TriggerWishAnimation(animDuration);
        }

        // 2. Chạy hoạt ảnh "Saving" trên Miếu (Nếu bạn có Trigger riêng)
        if (anim != null)
        {
            anim.SetTrigger("StartSaving");
        }

        // 3. CHỜ hoạt ảnh chạy hết 1 vòng
        yield return new WaitForSeconds(animDuration);

        // 4. THỰC HIỆN LƯU (Logic cũ của bạn)
        foreach (Checkpoint cp in allCheckpoints)
        {
            cp.SetCheckpointVisual(false);
        }

        SetCheckpointVisual(true);

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

        isProcessing = false;
        Debug.Log("<color=lime>Checkpoint chính thức được lưu sau khi thắp hương!</color>");
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