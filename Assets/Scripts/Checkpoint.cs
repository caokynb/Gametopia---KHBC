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

    private Animator anim;

    private bool isActivated = false;
    private bool isProcessing = false;

    void Awake() { allCheckpoints.Add(this); }
    void OnDestroy() { allCheckpoints.Remove(this); }

    void Start()
    {
        anim = GetComponent<Animator>();
        constructionScript = Object.FindFirstObjectByType<ConstructionMode>();

        // KIỂM TRA LÚC HỒI SINH
        if (PlayerMovement.hasCheckpoint)
        {
            float distance = Vector2.Distance(transform.position, PlayerMovement.respawnPosition);
            if (distance < 1.0f)
            {
                SetCheckpointVisual(true);
                if (anim != null) anim.Play("Saved", 0, 1f);
            }
            else
            {
                SetCheckpointVisual(false);
                if (anim != null) anim.Play("Unsave", 0, 1f);
            }
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F) && !isProcessing)
        {
            StartCoroutine(ProcessCheckpointActivation());
        }
    }

    IEnumerator ProcessCheckpointActivation()
    {
        isProcessing = true;
        float animDuration = 1.0f; // Thời gian Anh Khoai đứng khấn

        if (anim != null) anim.SetTrigger("StartSaving");

        if (playerMovement != null) playerMovement.TriggerWishAnimation(animDuration);

        yield return new WaitForSeconds(animDuration);

        foreach (Checkpoint cp in allCheckpoints)
        {
            if (cp != this) cp.SetCheckpointVisual(false);
        }

        SetCheckpointVisual(true);

        if (playerMovement != null && playerMovement.stats != null)
        {
            playerMovement.stats.currentBambooCount = playerMovement.stats.maxBambooCount;
            playerMovement.stats.healthPoint = playerMovement.stats.maxHealth;
        }

        if (constructionScript != null) constructionScript.ClearAllSpawnedBamboo();

        // ==========================================
        // 7. CHỐT SỔ TIẾN ĐỘ & KỸ NĂNG
        // ==========================================
        PlayerMovement.respawnPosition = transform.position;
        PlayerMovement.hasCheckpoint = true;
        PlayerMovement.checkpointScene = SceneManager.GetActiveScene().name;

        // BÍ KÍP: Kiểm tra xem trên người Anh Khoai lúc này có Buff không. 
        // Có cái nào thì lưu cứng cái đó lại!
        if (PlayerMovement.hasDiscountBuff) PlayerPrefs.SetInt("HasDiscountBuff", 1);
        if (PlayerMovement.canJumpOnBamboo) PlayerPrefs.SetInt("HasDoubleJump", 1);

        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedLevel", currentScene);
        PlayerPrefs.Save();

        isProcessing = false;
        Debug.Log("<color=lime>Đã thắp hương và CHỐT SỔ mọi kỹ năng!</color>");
    }

    public void SetCheckpointVisual(bool active)
    {
        isActivated = active;
        if (anim != null) anim.SetBool("isSaved", active);
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