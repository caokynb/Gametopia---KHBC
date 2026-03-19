using UnityEngine;
using UnityEngine.UI; // Bắt buộc phải có để điều khiển UI Image
using System.Collections;

public class SuperSceneFader : MonoBehaviour
{
    [Header("Cài đặt rèm cửa")]
    [Tooltip("Kéo bức ảnh BlackScreen vào đây")]
    public Image fadeImage;
    [Tooltip("Tốc độ mờ (Số càng lớn mờ càng nhanh)")]
    public float fadeSpeed = 1.5f;

    void Start()
    {
        // Đảm bảo rèm cửa luôn mở (trong suốt) lúc mới vào game
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    // Làm đen màn hình
    public void FadeOut()
    {
        if (fadeImage != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(1f)); // Đen 100%
        }
    }

    // Làm sáng màn hình
    public void FadeIn()
    {
        if (fadeImage != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(0f)); // Trong suốt 100%
        }
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;

        while (Mathf.Abs(c.a - targetAlpha) > 0.01f)
        {
            c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
            fadeImage.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        fadeImage.color = c;

        if (targetAlpha == 0f)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }
}