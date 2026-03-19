using UnityEngine;
using System.Collections;

public class RockLoop : MonoBehaviour
{
    [Header("Cấu hình thời gian")]
    public float respawnDelay = 1f;
    public float fallGravity = 10f;

    [Header("Cấu hình Sát thương")]
    [Tooltip("Số máu tre bị trừ mỗi lần đá rớt trúng (Mặc định tre có 3 máu)")]
    public int rockDamage = 1;

    private Vector2 startPosition;
    private Rigidbody2D rb;
    private bool isWaiting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        PrepareForFall();
    }

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isWaiting) return;

        // 1. Kiểm tra va chạm với Tre
        if (collision.gameObject.TryGetComponent<BambooSegment>(out BambooSegment bamboo))
        {
            // SỬA TẠI ĐÂY: Trừ đúng x sát thương thay vì phá hủy ngay lập tức
            bamboo.TakeDamage(rockDamage);
            StartCoroutine(ResetRockRoutine());
        }
        // 2. Kiểm tra va chạm với Đất cứng (Ground) hoặc Đất mềm (Dirt)
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("DirtLayer"))
        {
            StartCoroutine(ResetRockRoutine());
        }
        // 3. Nếu trúng Anh Khoai
        else if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(ResetRockRoutine());
        }
    }

    IEnumerator ResetRockRoutine()
    {
        isWaiting = true;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        transform.position = new Vector2(-9999, -9999);

        yield return new WaitForSeconds(respawnDelay);

        PrepareForFall();
    }
}