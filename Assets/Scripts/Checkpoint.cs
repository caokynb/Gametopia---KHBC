using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    private static List<Checkpoint> allCheckpoints = new List<Checkpoint>();
    private bool isPlayerNear = false;
    private PlayerMovement playerMovement;
    private ConstructionMode constructionScript;

    [Header("Cài đặt Hình ảnh Miếu")]
    public Sprite unsavedSprite; // Ảnh miếu bẩn (chưa lưu)
    public Sprite savedSprite;   // Ảnh miếu sạch (đã lưu)

    private SpriteRenderer sr;
    private bool isActivated = false;
    private bool isProcessing = false;

    void Awake() { allCheckpoints.Add(this); }
    void OnDestroy() { allCheckpoints.Remove(this); }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        constructionScript = Object.FindFirstObjectByType<ConstructionMode>();

        // Set ảnh mặc định ban đầu là chưa lau
        if (sr != null && unsavedSprite != null)
        {
            sr.sprite = unsavedSprite;
        }

        // KIỂM TRA LÚC HỒI SINH: Nếu đúng là miếu đã lưu thì đổi ảnh luôn
        if (PlayerMovement.hasCheckpoint)
        {
            float distance = Vector2.Distance(transform.position, PlayerMovement.respawnPosition);
            if (distance < 0.1f)
            {
                SetCheckpointVisual(true);
            }
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F) && !isProcessing && !isActivated)
        {
            StartCoroutine(ProcessCheckpointActivation());
        }
    }

    IEnumerator ProcessCheckpointActivation()
    {
        isProcessing = true;
        float animDuration = 1.0f; // Thời gian Anh Khoai đứng khấn/lau miếu

        if (playerMovement != null)
        {
            playerMovement.TriggerWishAnimation(animDuration);
        }

        // Chờ Anh Khoai diễn xong hoạt ảnh
        yield return new WaitForSeconds(animDuration);

        // Đặt tất cả các miếu khác về ảnh cũ
        foreach (Checkpoint cp in allCheckpoints)
        {
            cp.SetCheckpointVisual(false);
        }

        // Đổi miếu này thành ảnh sạch sẽ
        SetCheckpointVisual(true);

        // HỒI PHỤC CHỈ SỐ
        if (playerMovement != null && playerMovement.stats != null)
        {
            playerMovement.stats.currentBambooCount = playerMovement.stats.maxBambooCount;
            playerMovement.stats.healthPoint = playerMovement.stats.maxHealth;
        }

        if (constructionScript != null)
        {
            constructionScript.ClearAllSpawnedBamboo();
        }

        // LƯU TIẾN ĐỘ
        PlayerMovement.respawnPosition = transform.position;
        PlayerMovement.hasCheckpoint = true;

        PlayerMovement.checkpointScene = SceneManager.GetActiveScene().name;

        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedLevel", currentScene);
        PlayerPrefs.Save();

        isProcessing = false;
        Debug.Log("<color=lime>Đã lưu tiến độ bằng Sprite!</color>");
    }

    public void SetCheckpointVisual(bool active)
    {
        isActivated = active;
        if (sr != null)
        {
            // Nếu active = true thì dùng ảnh sạch, ngược lại dùng ảnh bẩn
            sr.sprite = active ? savedSprite : unsavedSprite;
        }
    }

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