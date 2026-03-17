using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class HitVFX : MonoBehaviour
{
    [Header("Các khung hình của vệt chém")]
    public Sprite[] frames;

    [Header("Tốc độ chạy (giây/frame)")]
    public float frameDuration = 0.05f;

    private void Start()
    {
        // Thêm một chút ngẫu nhiên để các nhát chém không bị giống hệt nhau
        RandomizeAppearance();

        // Bắt đầu chiếu ảnh
        StartCoroutine(PlayAnimation());
    }

    private void RandomizeAppearance()
    {
        // Lật ngẫu nhiên theo chiều ngang/dọc để tạo cảm giác chém nhiều góc
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.flipX = Random.value > 0.5f;
        sr.flipY = Random.value > 0.5f;

        // Xoay nghiêng ngẫu nhiên một chút (-15 độ đến 15 độ)
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
    }

    private IEnumerator PlayAnimation()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Lần lượt thay thế từng bức ảnh
        foreach (Sprite frame in frames)
        {
            sr.sprite = frame;
            yield return new WaitForSeconds(frameDuration);
        }

        // Chạy xong 4 frame thì tự hủy Object để dọn rác
        Destroy(gameObject);
    }
}