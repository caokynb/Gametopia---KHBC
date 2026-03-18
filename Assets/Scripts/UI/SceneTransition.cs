using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Bắt buộc có dòng này để dùng thời gian chờ (Coroutine)

public class SceneTransition : MonoBehaviour
{
    [Header("Cài đặt Chuyển Map")]
    public string nextSceneName;

    [Header("Hiệu ứng Rèm Đen")]
    [Tooltip("Kéo cái Fade_Panel có chứa Canvas Group vào đây")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f; // Thời gian mờ (1 giây)

    private bool isTransitioning = false;

    private void Start()
    {
        // KHI VỪA VÀO MAP MỚI: Màn hình từ đen thui sẽ sáng dần lên (Fade In)
        if (fadePanel != null)
        {
            fadePanel.alpha = 1f; // Bắt đầu bằng đen thui
            StartCoroutine(FadeIn());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi Player chạm vào cổng và chưa trong trạng thái chuyển map
        if (collision.CompareTag("Player") && !isTransitioning)
        {
            isTransitioning = true; // Khóa lại không cho chạm nhiều lần

            // Tùy chọn: Đóng băng Player để không chạy lung tung lúc màn hình đang tối
            collision.GetComponent<PlayerMovement>().enabled = false;
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // Bắt đầu hiệu ứng tối dần rồi mới chuyển Map
            StartCoroutine(FadeOutAndLoad());
        }
    }

    // Hiệu ứng Sáng lên
    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Kéo Alpha từ 1 (Đen) về 0 (Trong suốt)
            fadePanel.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null; // Đợi 1 khung hình rồi lặp tiếp
        }
        fadePanel.alpha = 0f; // Đảm bảo trong suốt hoàn toàn
    }

    // Hiệu ứng Tối đi và Load Scene
    IEnumerator FadeOutAndLoad()
    {
        float elapsedTime = 0f;
        if (fadePanel != null)
        {
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                // Kéo Alpha từ 0 (Trong suốt) lên 1 (Đen)
                fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
            fadePanel.alpha = 1f; // Đảm bảo đen hoàn toàn
        }

        // Đợi đen hẳn rồi mới Load sang map mới
        SceneManager.LoadScene(nextSceneName);
    }
}