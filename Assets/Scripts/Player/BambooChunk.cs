using UnityEngine;

public class BambooChunk : MonoBehaviour
{
    public void SplitChunk(Transform brokenSegment)
    {
        int brokenIndex = brokenSegment.GetSiblingIndex();
        int totalChildren = transform.childCount;

        // Bắt đầu từ ngọn tre, đi ngược xuống vị trí bị vỡ
        for (int i = totalChildren - 1; i > brokenIndex; i--)
        {
            Transform topPiece = transform.GetChild(i);

            // 1. Tách đốt tre này ra khỏi Chunk cha để nó hoàn toàn tự do
            topPiece.SetParent(null);

            // 2. Kiểm tra xem nó có Rigidbody chưa, nếu chưa thì thêm vào
            Rigidbody2D rb = topPiece.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = topPiece.gameObject.AddComponent<Rigidbody2D>();
            }

            // 3. Bật trọng lực để nó rớt tự do xuống đất
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1.5f;

            // 4. Thêm một chút lực xoay ngẫu nhiên để nó lộn vòng khi rơi cho đẹp
            rb.AddTorque(Random.Range(-15f, 15f));
        }

        // Xóa đốt tre bị trúng đá
        brokenSegment.SetParent(null);
        Destroy(brokenSegment.gameObject);
    }
}