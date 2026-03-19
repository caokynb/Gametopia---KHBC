using UnityEngine;
using System.Collections; // Bắt buộc phải có để dùng Coroutine

public class TrapManager : MonoBehaviour
{
    [Header("Danh sách Lư Hương (Bát)")]
    public BambooBowl bowl1;
    public BambooBowl bowl2;
    public BambooBowl bowl3;

    [Header("Cài đặt chướng ngại vật")]
    public GameObject objectToDestroy;
    [Tooltip("Thời gian để vật thể mờ dần rồi biến mất (giây)")]
    public float fadeDuration = 1.5f; // Thêm biến chỉnh thời gian mờ

    private bool trapTriggered = false;

    void Update()
    {
        if (trapTriggered) return;

        CheckResult();
    }

    void CheckResult()
    {
        // Chỉ cần 1 dòng kiểm tra cực gọn: Cả 3 Lư đều Xanh chưa?
        if (bowl1.isSolved && bowl2.isSolved && bowl3.isSolved)
        {
            TriggerTrap();
        }
    }

    void TriggerTrap()
    {
        trapTriggered = true;
        Debug.Log("Giải đố thành công cả 3 Lư Hương! Bắt đầu mở đường...");

        if (objectToDestroy != null)
        {
            // Thay vì Destroy ngay, ta gọi Coroutine làm mờ
            StartCoroutine(FadeAndDestroy(objectToDestroy, fadeDuration));
        }
    }

    // --- COROUTINE: LÀM MỜ VÀ PHÁ HỦY ---
    private IEnumerator FadeAndDestroy(GameObject target, float duration)
    {
        // Lấy toàn bộ SpriteRenderer của vật thể (bao gồm cả các vật thể con bên trong nó)
        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>();

        // Tắt va chạm ngay lập tức để người chơi có thể đi qua trong lúc nó đang mờ dần
        Collider2D[] colliders = target.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Lưu trữ lại màu sắc ban đầu của các mảng hình ảnh
        Color[] startColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            startColors[i] = renderers[i].color;
        }

        float elapsedTime = 0f;

        // Vòng lặp từ từ giảm Alpha theo thời gian
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Tính toán độ mờ dần từ 1 (đậm) về 0 (tàng hình)
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            // Áp dụng độ mờ mới cho tất cả các mảnh hình ảnh
            for (int i = 0; i < renderers.Length; i++)
            {
                Color newColor = startColors[i];
                newColor.a = alpha;
                renderers[i].color = newColor;
            }

            yield return null; // Chờ đến frame tiếp theo rồi lặp lại
        }

        // Sau khi đã mờ 100%, chính thức xóa vật thể khỏi bộ nhớ
        Destroy(target);
    }
}