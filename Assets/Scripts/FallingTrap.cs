using UnityEngine;

public class FallingTrap : MonoBehaviour
{
    [Header("Cấu hình Bẫy")]
    public Rigidbody2D trapRB;

    [Tooltip("Trọng lực càng cao vật rơi càng nhanh. Nên để từ 8-12.")]
    public float fallGravity = 10f;

    [Tooltip("Vận tốc đẩy xuống ban đầu để bẫy rơi nhanh ngay lập tức.")]
    public float initialPushForce = 2f;

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu người chơi dẫm vào vùng kích hoạt
        if (collision.CompareTag("Player") && !isTriggered)
        {
            TriggerFall();
        }
    }

    void TriggerFall()
    {
        isTriggered = true;

        if (trapRB != null)
        {
            // Chuyển sang Dynamic để 'kích hoạt' trọng lực đã cài sẵn (15-20)
            trapRB.bodyType = RigidbodyType2D.Dynamic;

            // Cung cấp một lực đẩy xuống ban đầu để bẫy sập dứt khoát
            trapRB.linearVelocity = new Vector2(0, -initialPushForce);

            Debug.Log("Bẫy đã sập!");
        }
    }
}