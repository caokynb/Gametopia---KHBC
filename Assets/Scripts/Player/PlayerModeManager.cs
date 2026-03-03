using UnityEngine;

public class PlayerModeManager : MonoBehaviour
{
    [Header("Các Chế Độ (Kéo thả script từ Player vào đây)")]
    public AttackMode attackModeScript;         // Kéo chữ AttackMode ở Inspector vào đây
    public ConstructionMode constructionModeScript; // Kéo chữ ConstructionMode ở Inspector vào đây

    [Header("Giao diện (Tùy chọn)")]
    // Nếu bạn làm 2 cái Sprite con nằm trong Player cho 2 chế độ thì kéo vào đây
    // Nếu dùng chung 1 Sprite và chỉ đổi bằng Animator thì để trống 2 ô này
    public GameObject attackVisual;
    public GameObject constructionVisual;

    private bool isAttackMode = false; // Mặc định vừa vào game là chế độ Khắc

    // Thêm biến animator ở đầu class PlayerModeManager
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>(); // Lấy component Animator từ Player
        // Set trạng thái chuẩn ngay từ khung hình đầu tiên
        UpdateModeState();
    }

    void Update()
    {
        // Lắng nghe phím E
        if (Input.GetKeyDown(KeyCode.E))
        {
            isAttackMode = !isAttackMode; // Đảo chiều công tắc (True thành False và ngược lại)
            UpdateModeState();

            // In ra Console để dễ debug
            string modeName = isAttackMode ? "KHẮC (Tấn Công)" : "NHẬP (Xây Dựng)";
            Debug.Log($"<color=cyan>ĐÃ CHUYỂN CHẾ ĐỘ:</color> {modeName}");
        }
    }

    void UpdateModeState()
    {
        // 1. Tắt/Bật chức năng (Code)
        if (attackModeScript != null)
            attackModeScript.enabled = isAttackMode;

        if (constructionModeScript != null)
            constructionModeScript.enabled = !isAttackMode;

        // 2. Tắt/Bật hình ảnh (Nếu có tách riêng 2 file hình)
        if (attackVisual != null)
            attackVisual.SetActive(isAttackMode);

        if (constructionVisual != null)
            constructionVisual.SetActive(!isAttackMode);

        if (anim != null)
        {
            // Gửi trực tiếp biến bool isAttackMode sang Animator
            anim.SetBool("isAttackMode", isAttackMode);
        }
    }
}