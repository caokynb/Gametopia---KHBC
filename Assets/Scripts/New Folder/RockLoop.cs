using UnityEngine;
using System.Collections;

public class RockLoop : MonoBehaviour
{
    [Header("Cấu hình thời gian")]
    public float respawnDelay = 1f;    // 1 giây sau khi chạm sẽ quay lại vị trí cũ
    public float fallGravity = 10f;    // Tốc độ rơi

    private Vector2 startPosition;
    private Rigidbody2D rb;
    private bool isWaiting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position; // Lưu vị trí ban đầu trên trời
        PrepareForFall();
    }

    // Đưa đá về trạng thái sẵn sàng rơi
    void PrepareForFall()
    {
        transform.position = startPosition;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallGravity;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
        isWaiting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isWaiting) return;

        // 1. Kiểm tra va chạm với Tre
        if (collision.gameObject.TryGetComponent<BambooSegment>(out BambooSegment bamboo))
        {
            bamboo.TakeDamage(1); // Gây sát thương cho tre
            StartCoroutine(ResetRockRoutine());
        }
        // 2. Kiểm tra va chạm với Đất (để đá không nằm lỳ dưới đất)
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StartCoroutine(ResetRockRoutine());
        }
    }

    IEnumerator ResetRockRoutine()
    {
        isWaiting = true;

        // Làm đá "biến mất" tạm thời (vô hiệu hóa hình ảnh và vật lý)
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        transform.position = new Vector2(-9999, -9999); // Đẩy ra xa để không ai thấy

        // Chờ 1 giây
        yield return new WaitForSeconds(respawnDelay);

        // Quay trở lại điểm cũ để rơi tiếp
        PrepareForFall();
    }
}