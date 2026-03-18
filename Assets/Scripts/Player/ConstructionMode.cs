using System.Collections.Generic;
using UnityEngine;

public class ConstructionMode : MonoBehaviour
{
    [Header("Dữ liệu Nhân vật")]
    private PlayerAttributes stats;

    [Header("Cài đặt Đốt Tre")]
    [SerializeField] float segmentLength = 1f;
    [SerializeField] int costPerSegment = 5;
    [SerializeField] float gravityScale = 1f;

    [Header("Cài đặt Cooldown")]
    [SerializeField] float spawnDelay = 0.5f;
    private float nextSpawnTime = 0f;

    [Header("Prefabs Cây Tre (4 Loại)")]
    [SerializeField] GameObject singleBambooPrefab; // Dùng khi chỉ có 1 đốt
    [SerializeField] GameObject startBambooPrefab;  // Đốt đầu (Gốc)
    [SerializeField] GameObject middleBambooPrefab; // Đốt giữa (Thân)
    [SerializeField] GameObject endBambooPrefab;    // Đốt cuối (Ngọn)

    [Header("Hệ thống")]
    [SerializeField] GameObject chunkBambooPrefab;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Vector2 checkSize = new Vector2(0.9f, 0.15f);

    private Vector2 dragStartPos;
    private bool isDragging = false;
    private List<GameObject> activePreviews = new List<GameObject>();
    private List<GameObject> spawnedChunks = new List<GameObject>();

    private void Start() => stats = GetComponent<PlayerMovement>().stats;

    void Update() => HandleConstructionInput();

    void HandleConstructionInput()
    {
        if (Time.time < nextSpawnTime) return;

        if (Input.GetMouseButtonDown(1) && isDragging) { CancelDragging(); return; }

        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            UpdatePreview(dragStartPos, currentMousePos);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector2 finalMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (SpawnSolidBamboo(dragStartPos, finalMousePos))
            {
                nextSpawnTime = Time.time + spawnDelay;
            }
            ClearPreviews();
            isDragging = false;
        }
    }

    void UpdatePreview(Vector2 start, Vector2 end)
    {
        ClearPreviews();
        Vector2 direction = end - start;
        float distance = direction.magnitude;
        int segmentCount = Mathf.FloorToInt(distance / segmentLength);

        if (segmentCount == 0) return;

        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 pos = start + direction * (i * segmentLength + segmentLength / 2f);
            if (!Physics2D.OverlapBox(pos, checkSize, angle, groundLayer))
            {
                GameObject prefabToUse = GetBambooPrefab(i, segmentCount);
                GameObject preview = Instantiate(prefabToUse, pos, rotation);

                // Làm mờ Preview 
                var renderer = preview.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.color = new Color(1, 1, 1, 0.4f);

                // SỬA TẠI ĐÂY: Tắt toàn bộ va chạm (Collider) của bản Preview
                Collider2D[] colliders = preview.GetComponentsInChildren<Collider2D>();
                foreach (Collider2D col in colliders)
                {
                    col.enabled = false;
                }

                activePreviews.Add(preview);
            }
        }
    }

    bool SpawnSolidBamboo(Vector2 start, Vector2 end)
    {
        Vector2 direction = end - start;
        float distance = direction.magnitude;
        int segmentCount = Mathf.FloorToInt(distance / segmentLength);

        if (segmentCount == 0) return false;

        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject currentChunk = null;
        int totalCost = 0;

        // BÍ MẬT NẰM Ở ĐÂY: Tính giá tiền của 1 đốt tre. Nếu có buff thì cưa đôi giá!
        int actualCostPerSegment = costPerSegment;
        if (PlayerMovement.hasDiscountBuff)
        {
            actualCostPerSegment = Mathf.CeilToInt(costPerSegment / 2f);
        }

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 pos = start + direction * (i * segmentLength + segmentLength / 2f);
            if (!Physics2D.OverlapBox(pos, checkSize, angle, groundLayer))
            {
                // Dùng giá thực tế đã được sale để kiểm tra xem có đủ tiền xây không
                if (stats.currentBambooCount < totalCost + actualCostPerSegment) break;

                if (currentChunk == null)
                {
                    currentChunk = Instantiate(chunkBambooPrefab, Vector3.zero, Quaternion.identity);
                    currentChunk.GetComponent<Rigidbody2D>().gravityScale = gravityScale;
                    spawnedChunks.Add(currentChunk);
                }

                GameObject prefabToUse = GetBambooPrefab(i, segmentCount);
                GameObject segment = Instantiate(prefabToUse, pos, rotation);
                segment.transform.SetParent(currentChunk.transform);

                // Cộng dồn chi phí đã sale
                totalCost += actualCostPerSegment;
            }
            else { currentChunk = null; }
        }

        if (totalCost > 0)
        {
            stats.currentBambooCount -= totalCost;
            return true;
        }
        return false;
    }

    // --- LOGIC CHỌN ĐỐT TRE (Theo yêu cầu của Loc) ---
    GameObject GetBambooPrefab(int index, int total)
    {
        if (total == 1) return singleBambooPrefab;             // Chỉ vẽ được 1 đốt
        if (index == 0) return startBambooPrefab;               // Đốt đầu tiên (chuột nhấn)
        if (index == total - 1) return endBambooPrefab;         // Đốt cuối cùng (nhả chuột)
        return middleBambooPrefab;                              // Các đốt ở giữa
    }

    void ClearPreviews() { foreach (var p in activePreviews) Destroy(p); activePreviews.Clear(); }
    void CancelDragging() { isDragging = false; ClearPreviews(); }
    public void ClearAllSpawnedBamboo()
    {
        foreach (var c in spawnedChunks) if (c != null) Destroy(c);
        spawnedChunks.Clear();
    }
}