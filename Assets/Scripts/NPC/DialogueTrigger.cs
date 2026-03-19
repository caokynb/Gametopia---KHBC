using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Cài đặt Chế độ Kích hoạt")]
    [Tooltip("Tích vào đây nếu muốn hộp thoại tự bật khi đi ngang qua (Unskippable)")]
    public bool isAutoTrigger = false;
    public GameObject glowEffect;

    [Header("Cài đặt Xuất hiện (VD: Ông Bụt hiện lên)")]
    [Tooltip("Tích vào đây để nhân vật tàng hình lúc đầu và hiện dần lên khi kích hoạt")]
    public bool fadeInOnTrigger = false;
    [Tooltip("Thời gian hiện dần lên (giây)")]
    public float fadeInDuration = 1f;

    [Header("Cài đặt Sau Hội Thoại")]
    [Tooltip("Tích vào đây để nhân vật biến mất sau khi nói chuyện xong")]
    public bool disappearAfterDialogue = false;
    [Tooltip("Thời gian mờ dần trước khi biến mất (giây)")]
    public float fadeDuration = 1.5f;
    [Tooltip("Prefab hiệu ứng khói/phép thuật khi biến mất (Tùy chọn)")]
    public GameObject poofEffectPrefab;

    [Header("Lưu Trữ Trong Lần Chơi (Chống lặp lại khi chết)")]
    [Tooltip("Tích vào đây để game nhớ và xóa NPC này sau khi đã nói chuyện xong (Reset khi tắt Play)")]
    public bool saveState = false;
    [Tooltip("BẮT BUỘC ĐIỀN TÊN KHÁC NHAU cho mỗi NPC (VD: OngBut_Map1_KhuA)")]
    public string uniqueID = "OngBut_01";

    // --- MỚI THÊM: MENU CHỌN CẮT CẢNH ENDING ---
    public enum EndingSequenceType { Khong_Co_Gi, KhacNhap_Phan1, KhacXuat_Phan2 }

    [Header("Cài đặt Ending (Chỉ dùng cho cảnh kết thúc)")]
    [Tooltip("Chọn xem đoạn hội thoại này xong thì gọi Cắt cảnh nào")]
    public EndingSequenceType endingType = EndingSequenceType.Khong_Co_Gi;

    [Header("Nội dung cuộc trò chuyện")]
    public DialogueLine[] dialogueLines;

    private bool playerInRange = false;
    private DialogueManager manager;
    private float interactCooldown = 0f;
    private bool hasTriggeredAuto = false;

    private SpriteRenderer[] renderers;
    private Collider2D[] colliders;
    private bool isAppearing = false;
    private bool hasAppeared = false;

    // Biến static này sẽ tồn tại xuyên suốt các Scene, nhưng sẽ tự reset khi tắt Play
    public static List<string> deletedNPCsThisSession = new List<string>();

    void Start()
    {
        // 1. KIỂM TRA TRÍ NHỚ NGẮN HẠN
        if (saveState && deletedNPCsThisSession.Contains(uniqueID))
        {
            Destroy(gameObject);
            return;
        }

        manager = FindFirstObjectByType<DialogueManager>();

        renderers = GetComponentsInChildren<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();

        if (fadeInOnTrigger)
        {
            SetRenderersAlpha(0f);
            SetCollidersState(false, false);
            if (glowEffect != null) glowEffect.SetActive(false);
        }
        else
        {
            hasAppeared = true;
        }
    }

    void Update()
    {
        if (interactCooldown > 0) interactCooldown -= Time.deltaTime;

        if (glowEffect != null && hasAppeared && !isAppearing)
        {
            bool shouldGlow = playerInRange && !isAutoTrigger && (manager != null && !manager.dialogueBox.activeInHierarchy);

            if (glowEffect.activeSelf != shouldGlow)
            {
                glowEffect.SetActive(shouldGlow);
            }
        }

        if (!isAutoTrigger && playerInRange && Input.GetKeyDown(KeyCode.F) && interactCooldown <= 0f)
        {
            if (manager != null && !manager.dialogueBox.activeInHierarchy)
            {
                if (fadeInOnTrigger && !hasAppeared && !isAppearing)
                {
                    StartCoroutine(AppearAndStartDialogue(false));
                }
                else if (hasAppeared)
                {
                    manager.StartDialogue(dialogueLines, false);
                    CheckAndHandleDialogueEnd(); // Đã sửa tên hàm
                }
                interactCooldown = 0.5f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;

            if (isAutoTrigger && !hasTriggeredAuto)
            {
                if (manager != null && !manager.dialogueBox.activeInHierarchy)
                {
                    hasTriggeredAuto = true;

                    if (fadeInOnTrigger && !hasAppeared && !isAppearing)
                    {
                        StartCoroutine(AppearAndStartDialogue(true));
                    }
                    else
                    {
                        manager.StartDialogue(dialogueLines, true);
                        CheckAndHandleDialogueEnd(); // Đã sửa tên hàm
                    }
                }
            }
            else if (!isAutoTrigger && hasAppeared)
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

    // --- ĐÃ NÂNG CẤP HÀM NÀY ---
    // Bây giờ nó sẽ kích hoạt nếu CÓ TÍCH BIẾN MẤT HOẶC CÓ CHỌN ENDING
    private void CheckAndHandleDialogueEnd()
    {
        if (disappearAfterDialogue || endingType != EndingSequenceType.Khong_Co_Gi)
        {
            StartCoroutine(WaitAndHandleDialogueEnd());
        }
    }

    private IEnumerator AppearAndStartDialogue(bool autoMode)
    {
        isAppearing = true;

        if (glowEffect != null) glowEffect.SetActive(false);

        Color[] startColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            startColors[i] = renderers[i].color;
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                Color newColor = startColors[i];
                newColor.a = alpha;
                renderers[i].color = newColor;
            }
            yield return null;
        }

        SetRenderersAlpha(1f);
        SetCollidersState(true, true);

        isAppearing = false;
        hasAppeared = true;

        manager.StartDialogue(dialogueLines, autoMode);
        CheckAndHandleDialogueEnd(); // Đã sửa tên hàm
    }

    // --- COROUTINE CHÍNH ĐỂ XỬ LÝ SAU KHI TẮT HỘI THOẠI ---
    private IEnumerator WaitAndHandleDialogueEnd()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !manager.dialogueBox.activeInHierarchy);

        // --- 1. GỌI ĐẠO DIỄN LÀM ENDING (NẾU CÓ) ---
        if (endingType == EndingSequenceType.KhacNhap_Phan1 && EndingManager.instance != null)
        {
            EndingManager.instance.PlayEndingPart1();
        }
        else if (endingType == EndingSequenceType.KhacXuat_Phan2 && EndingManager.instance != null)
        {
            EndingManager.instance.PlayEndingPart2();
        }

        // --- 2. XỬ LÝ LÀM MỜ BIẾN MẤT (NẾU CÓ TÍCH) ---
        if (disappearAfterDialogue)
        {
            if (poofEffectPrefab != null)
            {
                Instantiate(poofEffectPrefab, transform.position, Quaternion.identity);
            }

            SetCollidersState(false, true);

            Color[] startColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                startColors[i] = renderers[i].color;
            }

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

                for (int i = 0; i < renderers.Length; i++)
                {
                    Color newColor = startColors[i];
                    newColor.a = alpha;
                    renderers[i].color = newColor;
                }
                yield return null;
            }

            // Lưu tên vào danh sách đã xóa
            if (saveState && !deletedNPCsThisSession.Contains(uniqueID))
            {
                deletedNPCsThisSession.Add(uniqueID);
            }

            Destroy(gameObject);
        }
    }

    private void SetRenderersAlpha(float alpha)
    {
        if (renderers == null) return;
        foreach (SpriteRenderer ren in renderers)
        {
            Color c = ren.color;
            c.a = alpha;
            ren.color = c;
        }
    }

    private void SetCollidersState(bool state, bool affectTriggers)
    {
        if (colliders == null) return;
        foreach (Collider2D col in colliders)
        {
            if (!affectTriggers && col.isTrigger)
            {
                continue;
            }
            col.enabled = state;
        }
    }
}