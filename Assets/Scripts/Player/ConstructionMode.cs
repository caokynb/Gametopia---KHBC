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

    [Header("Prefabs Cây Tre")]
    [SerializeField] GameObject singleBambooPrefab;
    [SerializeField] GameObject startBambooPrefab;
    [SerializeField] GameObject middleBambooPrefab;
    [SerializeField] GameObject endBambooPrefab;

    [Header("Hệ thống")]
    [SerializeField] GameObject chunkBambooPrefab;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask dirtLayer;
    [SerializeField] Vector2 checkSize = new Vector2(0.9f, 0.15f);

    private Vector2 dragStartPos;
    private bool isDragging = false;

    private List<GameObject> activePreviews = new List<GameObject>();
    private List<GameObject> spawnedChunks = new List<GameObject>();

    private void Start()
    {
        // Lấy script quản lý chỉ số nhân vật
        stats = GetComponent<PlayerAttributes>();
    }

    void Update()
    {
        HandleConstructionInput();
    }

    void HandleConstructionInput()
    {
        // Kiểm tra thời gian chờ giữa mỗi lần xây
        if (Time.time < nextSpawnTime) return;

        // Hủy quá trình kéo bằng chuột phải
        if (Input.GetMouseButtonDown(1) && isDragging)
        {
            CancelDragging();
            return;
        }

        // Bắt đầu kéo
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        // Đang kéo
        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            UpdatePreview(dragStartPos, currentMousePos);
        }

        // Thả chuột để xây
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

        // KIỂM TRA DIRT ĐỂ ĐỔI MÀU PREVIEW
        Collider2D hitDirt = Physics2D.OverlapPoint(start, dirtLayer);
        bool isPreviewOnDirt = hitDirt != null;

        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 pos = start + direction * (i * segmentLength + segmentLength / 2f);

            // Kiểm tra xem đốt tre có bị vướng vào địa hình không
            if (!Physics2D.OverlapBox(pos, checkSize, angle, groundLayer))
            {
                GameObject prefabToUse = GetBambooPrefab(i, segmentCount);
                GameObject preview = Instantiate(prefabToUse, pos, rotation);

                // Thiết lập màu sắc hiển thị
                SpriteRenderer renderer = preview.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    // ƯU TIÊN MÀU SẮC KHI CÓ NGỌC GIẢM GIÁ
                    if (stats.isHalfCostActive)
                    {
                        // Màu xanh lá nhạt báo hiệu đang được buff giảm giá
                        renderer.color = new Color(0f, 1f, 0f, 0.4f);
                    }
                    else if (isPreviewOnDirt)
                    {
                        // Màu cam báo hiệu tốn gấp đôi (màu cam nhạt)
                        renderer.color = new Color(1f, 0.6f, 0f, 0.4f);
                    }
                    else
                    {
                        // Màu trắng bình thường
                        renderer.color = new Color(1f, 1f, 1f, 0.4f);
                    }
                }

                // Tắt Collider của Preview để không va chạm vật lý
                Collider2D[] cols = preview.GetComponentsInChildren<Collider2D>();
                foreach (Collider2D c in cols)
                    c.enabled = false;

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

        // 1. XÁC ĐỊNH LOẠI ĐẤT
        Collider2D hitDirt = Physics2D.OverlapPoint(start, dirtLayer);
        bool isOnDirt = hitDirt != null;

        // 2. TÍNH TOÁN GIÁ TIỀN CƠ BẢN
        int baseCost = isOnDirt ? (costPerSegment * 2) : costPerSegment;

        // 3. KIỂM TRA NGỌC GIẢM GIÁ (HALF COST)
        int finalCostPerSegment = baseCost;
        if (stats.isHalfCostActive)
        {
            // Giảm 1 nửa chi phí, dùng Mathf.Max để đảm bảo không bị miễn phí (tối thiểu là 1)
            finalCostPerSegment = Mathf.Max(1, baseCost / 2);
        }

        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject currentChunk = null;
        int bambooCreatedCount = 0;
        int actualTotalCost = 0;

        // 4. VÒNG LẶP TẠO TRE
        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 pos = start + direction * (i * segmentLength + segmentLength / 2f);

            // Chỉ tạo nếu vị trí không bị vướng vật cản Ground
            if (!Physics2D.OverlapBox(pos, checkSize, angle, groundLayer))
            {
                // Kiểm tra xem nhân vật còn đủ tiền để tạo đốt tiếp theo không
                // Sử dụng finalCostPerSegment đã được tính toán qua ngọc
                if (stats.currentBambooCount < (actualTotalCost + finalCostPerSegment))
                {
                    break; // Ngừng tạo nếu hết tiền
                }

                // Khởi tạo Chunk (vật chứa) cho cây tre nếu chưa có
                if (currentChunk == null)
                {
                    currentChunk = Instantiate(chunkBambooPrefab, Vector3.zero, Quaternion.identity);
                    Rigidbody2D rb = currentChunk.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.gravityScale = gravityScale;
                    }

                    // Nếu trồng trên Dirt, gắn khớp nối để tre đứng vững
                    if (isOnDirt)
                    {
                        FixedJoint2D joint = currentChunk.AddComponent<FixedJoint2D>();
                        joint.autoConfigureConnectedAnchor = false;
                        joint.anchor = currentChunk.transform.InverseTransformPoint(start);

                        // Kết nối với Rigidbody của Dirt nếu có
                        if (hitDirt.attachedRigidbody != null)
                        {
                            joint.connectedBody = hitDirt.attachedRigidbody;
                        }

                        joint.connectedAnchor = start;
                        joint.breakForce = Mathf.Infinity; // Tre đứng vững vĩnh viễn
                    }
                    spawnedChunks.Add(currentChunk);
                }

                // Tạo đốt tre thật
                GameObject segment = Instantiate(GetBambooPrefab(i, segmentCount), pos, rotation);
                segment.transform.SetParent(currentChunk.transform);

                // Cộng dồn chi phí thực tế (đã bao gồm giảm giá nếu có)
                actualTotalCost += finalCostPerSegment;
                bambooCreatedCount++;
            }
        }

        // 5. TRỪ TIỀN VÀ XÁC NHẬN XÂY XONG
        if (bambooCreatedCount > 0)
        {
            stats.currentBambooCount -= actualTotalCost;
            return true;
        }

        return false;
    }

    GameObject GetBambooPrefab(int index, int total)
    {
        if (total == 1) return singleBambooPrefab;
        if (index == 0) return startBambooPrefab;
        if (index == total - 1) return endBambooPrefab;
        return middleBambooPrefab;
    }

    void ClearPreviews()
    {
        foreach (var p in activePreviews)
        {
            if (p != null) Destroy(p);
        }
        activePreviews.Clear();
    }

    void CancelDragging()
    {
        isDragging = false;
        ClearPreviews();
    }

    public void ClearAllSpawnedBamboo()
    {
        foreach (var c in spawnedChunks)
        {
            if (c != null) Destroy(c);
        }
        spawnedChunks.Clear();
    }
}