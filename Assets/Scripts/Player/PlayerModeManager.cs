using UnityEngine;

public class PlayerModeManager : MonoBehaviour
{
    [Header("Các Chế Độ (Kéo thả script từ Player vào đây)")]
    public AttackMode attackModeScript;
    public ConstructionMode constructionModeScript;

    [Header("Giao diện (Tùy chọn)")]
    public GameObject attackVisual;
    public GameObject constructionVisual;

    // --- ĐÃ TÁCH THÀNH 2 ÂM THANH RIÊNG BIỆT ---
    [Header("Âm thanh (SFX)")]
    [Tooltip("Tiếng rút kiếm khi chuyển sang đánh")]
    public AudioClip khacSound;
    [Tooltip("Tiếng cất kiếm / tre kêu khi chuyển sang xây")]
    public AudioClip nhapSound;

    private AudioSource audioSource;

    // Dùng 'static' để biến này tồn tại xuyên suốt các màn chơi/hồi sinh ---
    public static bool isAttackMode = false;

    private Animator anim;

    void Start()
    {
        // Lấy component Animator từ Player
        anim = GetComponent<Animator>();

        // Tự động tìm hoặc gắn Loa cho Anh Khoai
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Cập nhật ngay hình ảnh theo trạng thái static đã lưu!
        UpdateModeState();
    }

    void Update()
    {
        // Lắng nghe phím E để chuyển đổi chế độ
        if (Input.GetKeyDown(KeyCode.E))
        {
            isAttackMode = !isAttackMode;
            UpdateModeState();

            // --- PHÁT ÂM THANH TÙY THEO CHẾ ĐỘ HIỆN TẠI ---
            if (audioSource != null)
            {
                if (isAttackMode && khacSound != null)
                {
                    // Chuyển sang Khắc -> Kêu tiếng rút kiếm
                    audioSource.PlayOneShot(khacSound);
                }
                else if (!isAttackMode && nhapSound != null)
                {
                    // Chuyển sang Nhập -> Kêu tiếng cất kiếm/xây tre
                    audioSource.PlayOneShot(nhapSound);
                }
            }

            // Debug để bạn kiểm tra trong Console
            string modeName = isAttackMode ? "KHẮC (Tấn Công)" : "NHẬP (Xây Dựng)";
            Debug.Log($"<color=cyan>ĐÃ CHUYỂN CHẾ ĐỘ:</color> {modeName}");
        }
    }

    void UpdateModeState()
    {
        // 1. Bật/Tắt logic của các Script chức năng
        if (attackModeScript != null)
            attackModeScript.enabled = isAttackMode;

        if (constructionModeScript != null)
            constructionModeScript.enabled = !isAttackMode;

        // 2. Bật/Tắt các vật thể hình ảnh (nếu bạn có tách riêng object con)
        if (attackVisual != null)
            attackVisual.SetActive(isAttackMode);

        if (constructionVisual != null)
            constructionVisual.SetActive(!isAttackMode);

        // 3. Cập nhật Parameter cho Animator
        if (anim != null)
        {
            anim.SetBool("isAttackMode", isAttackMode);
        }

        // 4. CẬP NHẬT GIAO DIỆN HUD TẠI ĐÂY
        HUDManager hud = Object.FindFirstObjectByType<HUDManager>();
        if (hud != null)
        {
            // Truyền biến isAttackMode thẳng vào HUDManager
            hud.ChangeModeAppearance(isAttackMode);
        }
    }
}