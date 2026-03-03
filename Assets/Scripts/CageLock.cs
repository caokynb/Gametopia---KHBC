using UnityEngine;

public class CageLock : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isLocked = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Chế độ này giúp lồng không bị xuyên qua đất khi rơi nhanh
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra nếu lồng chạm vào mặt đất (Dùng Tag hoặc Layer)
        if (!isLocked && (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground")))
        {
            LockCage();
        }
    }

    void LockCage()
    {
        isLocked = true;

        // 1. Dừng mọi vận tốc ngay lập tức
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 2. Chuyển sang Static để lồng đứng yên vĩnh viễn, không thể bị đẩy
        rb.bodyType = RigidbodyType2D.Static;

        Debug.Log("Lồng đã chạm đất và được cố định!");
    }
}