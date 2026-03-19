using UnityEngine;
using System.Collections;

public class BuffaloFade : MonoBehaviour
{
    private SpriteRenderer sr;
    private bool isFading = false;

    [Header("Tự hủy khi đi xa")]
    public float destroyDistance = 25f; // Khoảng cách so với Camera để biến mất
    private Transform camTransform;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (Camera.main != null) camTransform = Camera.main.transform;
    }

    void Update()
    {
        // Nếu không đang trong quá trình mờ dần và ở quá xa Camera
        if (!isFading && camTransform != null)
        {
            float distance = Mathf.Abs(transform.position.x - camTransform.position.x);
            if (distance > destroyDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    public void StartFadeOut(float duration)
    {
        if (!isFading) StartCoroutine(FadeRoutine(duration));
    }

    IEnumerator FadeRoutine(float duration)
    {
        isFading = true;
        Color col = sr.color;
        float startAlpha = col.a;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            col.a = Mathf.Lerp(startAlpha, 0, t / duration);
            sr.color = col;
            yield return null;
        }
        Destroy(gameObject);
    }
}