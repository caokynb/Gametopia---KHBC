using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [Header("Hiệu ứng Mờ")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f; // Thời gian mờ (1 giây)

    void Start()
    {
        // Khi màn chơi vừa load xong, màn hình đang đen thui. 
        // Chúng ta sẽ cho nó từ từ sáng lên để bắt đầu chơi.
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f;
            StartCoroutine(FadeInRoutine());
        }
    }

    // Hàm này sẽ được gọi khi Anh Khoai chết
    public void FadeOutAndRestart()
    {
        if (fadeGroup != null)
        {
            StartCoroutine(FadeOutRoutine());
        }
        else
        {
            // Dự phòng nếu quên gắn UI
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // Hiệu ứng Sáng dần lên (Khi bắt đầu game)
    IEnumerator FadeInRoutine()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration); // Từ 1 giảm về 0
            yield return null;
        }
        fadeGroup.alpha = 0f;
    }

    // Hiệu ứng Tối dần đi (Khi chết) -> Chuyển cảnh
    IEnumerator FadeOutRoutine()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration); // Từ 0 tăng lên 1
            yield return null;
        }
        fadeGroup.alpha = 1f;

        // Chuyển cảnh ngay khi màn hình đã đen đặc
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}