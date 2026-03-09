using UnityEngine;
using System.Collections;

public class BambooSegment : MonoBehaviour
{
    [Header("Chỉ số Đốt Tre")]
    public int health = 3;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            BreakThisPiece();
        }
    }

    void BreakThisPiece()
    {
        Transform myChunk = transform.parent;

        if (myChunk != null)
        {
            int myIndex = transform.GetSiblingIndex();
            int totalPieces = myChunk.childCount;

            // 1. Tạo Chunk mới chứa nửa trên
            GameObject topChunk = new GameObject("Broken_Bamboo_Top");
            topChunk.transform.position = myChunk.position;
            topChunk.transform.rotation = myChunk.rotation; // Giữ nguyên góc nghiêng
            topChunk.layer = myChunk.gameObject.layer;      // Giữ nguyên Layer Tre

            // 2. Cấp vật lý để nó rớt xuống
            Rigidbody2D topRb = topChunk.AddComponent<Rigidbody2D>();
            topRb.bodyType = RigidbodyType2D.Dynamic;
            topRb.gravityScale = 1.5f;

            // 3. Chuyển các đốt ngọn sang Chunk mới
            for (int i = totalPieces - 1; i > myIndex; i--)
            {
                Transform pieceAbove = myChunk.GetChild(i);
                pieceAbove.SetParent(topChunk.transform);
            }
        }

        // 4. Xóa đốt bị vỡ
        Destroy(gameObject);
    }
}