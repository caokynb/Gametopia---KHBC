using UnityEngine;

public class NPCFacePlayer : MonoBehaviour
{
    [Header("Cấu hình mục tiêu")]
    public Transform player; // Kéo Anh Khoai vào đây

    [Header("Trạng thái ban đầu")]
    [SerializeField] private bool isFacingRight = true; // NPC của bạn lúc vẽ đang nhìn về bên nào?

    void Start()
    {
        // Tự động tìm Anh Khoai nếu bạn quên kéo vào Inspector
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // BÍ KÍP TA: So sánh vị trí X để quyết định Flip
        // Nếu người chơi ở bên phải NPC và NPC đang nhìn bên trái
        if (player.position.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }
        // Nếu người chơi ở bên trái NPC và NPC đang nhìn bên phải
        else if (player.position.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;

        // Kỹ thuật lật mặt bằng localScale (giữ nguyên Pivot)
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}