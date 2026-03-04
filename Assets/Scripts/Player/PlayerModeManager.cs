using UnityEngine;

public class PlayerModeManager : MonoBehaviour
{
    [Header("Các Chế Độ (Kéo thả script từ Player vào đây)")]
    public AttackMode attackModeScript;
    public ConstructionMode constructionModeScript;

    [Header("Giao diện (Tùy chọn)")]
    public GameObject attackVisual;
    public GameObject constructionVisual;

    // Dùng 'static' để biến này tồn tại xuyên suốt các màn chơi/hồi sinh ---
    public static bool isAttackMode = false;

    private Animator anim;

    void Start()
    {
        // Lấy component Animator từ Player
        anim = GetComponent<Animator>();

        // Rất quan trọng: Khi vừa hồi sinh (Start lại), 
        // phải cập nhật ngay hình ảnh theo trạng thái static đã lưu!
        UpdateModeState();
    }

    void Update()
    {
        // Lắng nghe phím E để chuyển đổi chế độ
        if (Input.GetKeyDown(KeyCode.E))
        {
            isAttackMode = !isAttackMode;
            UpdateModeState();

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
            // Báo cho Animator biết để chuyển bộ Animation (Idle, Run, Jump...)
            anim.SetBool("isAttackMode", isAttackMode);
        }
    }
}