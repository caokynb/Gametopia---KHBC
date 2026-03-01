using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    [Header("Cấu hình bẫy")]
    public Rigidbody2D cageRigidbody; // Kéo cái lồng từ Hierarchy vào đây
    public float fallGravity = 3f;

    public void OnBlockDestroyed()
    {
        if (cageRigidbody != null)
        {
            // Chuyển lồng sang trạng thái rơi
            cageRigidbody.bodyType = RigidbodyType2D.Dynamic;
            cageRigidbody.gravityScale = fallGravity;
            Debug.Log("Bẫy lồng đã được kích hoạt!");
        }
    }
}