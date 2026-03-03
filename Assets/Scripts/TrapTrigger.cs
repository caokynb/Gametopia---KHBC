using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    [Header("Cấu hình bẫy")]
    public Rigidbody2D cageRigidbody;
    public float fallGravity = 3f;

    public void OnBlockDestroyed()
    {
        if (cageRigidbody != null)
        {
            // Kích hoạt rơi
            cageRigidbody.bodyType = RigidbodyType2D.Dynamic;
            cageRigidbody.gravityScale = fallGravity;

            // Đảm bảo lồng không bị xoay khi rơi (tùy chọn)
            cageRigidbody.freezeRotation = true;

            Debug.Log("Bẫy lồng đã được kích hoạt!");
        }

        Destroy(gameObject); // Xóa mồi nhử
    }
}