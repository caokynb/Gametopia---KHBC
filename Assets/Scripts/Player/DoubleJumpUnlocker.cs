using UnityEngine;
using System.Collections; // Cần thêm dòng này để dùng Coroutine

public class DoubleJumpUnlocker : MonoBehaviour
{
    [Header("Giao diện UI")]
    [Tooltip("Kéo bảng Panel thông báo vào đây")]
    public GameObject unlockUIPanel;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();

            if (player != null)
            {
                // 1. MỞ KHÓA KỸ NĂNG NGAY LẬP TỨC
                PlayerMovement.canJumpOnBamboo = true;
                Debug.Log("<color=green>ĐÃ MỞ KHÓA:</color> Kỹ năng Double Jump!");

                // 2. Chạy Coroutine hiển thị UI
                StartCoroutine(ShowUnlockUI());
            }
        }
    }

    IEnumerator ShowUnlockUI()
    {
        // Tắt hình ảnh và khung va chạm của bí kíp đi ngay lập tức (nhưng chưa xóa vội)
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // Bật cái bảng UI to đùng lên
        if (unlockUIPanel != null)
        {
            unlockUIPanel.SetActive(true);
        }

        // ĐÓNG BĂNG THỜI GIAN (Game tạm dừng hoàn toàn)
        Time.timeScale = 0f;

        // Đợi 0.5 giây (đời thực) để người chơi kịp nhìn thấy bảng UI
        // Dùng WaitForSecondsRealtime vì TimeScale đang bằng 0
        yield return new WaitForSecondsRealtime(0.5f);

        // Chờ người chơi bấm phím BẤT KỲ (Space, Enter, Click chuột...) để tắt UI
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));

        // Tắt bảng UI
        if (unlockUIPanel != null)
        {
            unlockUIPanel.SetActive(false);
        }

        // RÃ ĐÔNG THỜI GIAN (Game chạy tiếp)
        Time.timeScale = 1f;

        // Xóa hoàn toàn vật phẩm khỏi Scene
        Destroy(gameObject);
    }
}