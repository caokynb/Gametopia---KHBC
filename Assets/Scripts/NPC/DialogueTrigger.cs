using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Cài đặt Chế độ")]
    [Tooltip("Tích vào đây nếu muốn hộp thoại tự bật khi đi ngang qua (Unskippable)")]
    public bool isAutoTrigger = false;
    public GameObject glowEffect; // [MỚI] Biến chứa vầng sáng

    [Header("Nội dung cuộc trò chuyện")]
    public DialogueLine[] dialogueLines;

    private bool playerInRange = false;
    private DialogueManager manager;
    private float interactCooldown = 0f;

    // [MỚI] Đảm bảo Auto Trigger chỉ chạy 1 lần duy nhất, tránh kẹt loop
    private bool hasTriggeredAuto = false;

    void Start()
    {
        manager = FindFirstObjectByType<DialogueManager>();
    }

    void Update()
    {
        if (interactCooldown > 0) interactCooldown -= Time.deltaTime;

        if (glowEffect != null)
        {
            // Logic: Bật sáng KHI VÀ CHỈ KHI Anh Khoai ở gần + Không phải Auto + Hộp thoại đang TẮT
            bool shouldGlow = playerInRange && !isAutoTrigger && (manager != null && !manager.dialogueBox.activeInHierarchy);

            // Cập nhật trạng thái của Glow cho khớp với logic trên
            if (glowEffect.activeSelf != shouldGlow)
            {
                glowEffect.SetActive(shouldGlow);
            }
        }
        // Nếu là NPC bình thường (Không phải Auto), mới cho phép bấm F
        if (!isAutoTrigger && playerInRange && Input.GetKeyDown(KeyCode.F) && interactCooldown <= 0f)
        {
            if (manager != null && !manager.dialogueBox.activeInHierarchy)
            {
                manager.StartDialogue(dialogueLines, false); // false = Manual Mode
                interactCooldown = 0.5f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;

            // [MỚI] Nếu là Auto Trigger và chưa từng chạy -> Tự động bật luôn
            if (isAutoTrigger && !hasTriggeredAuto)
            {
                if (manager != null && !manager.dialogueBox.activeInHierarchy)
                {
                    manager.StartDialogue(dialogueLines, true); // true = Auto Mode
                    hasTriggeredAuto = true; // Đánh dấu là đã chạy
                }
            }
            else if (!isAutoTrigger)
            {
                Debug.Log("Anh Khoai đã tới gần. Bấm F để nói chuyện!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}