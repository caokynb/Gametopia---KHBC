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

    // --- ĐÃ SỬA: Đổi tên thành Cooldown cho đúng ngữ nghĩa ---
    [Header("Cài đặt Thời gian (Cooldown)")]
    [SerializeField] float spawnDelay = 0.5f; // Thời gian nghỉ SAU KHI xây xong
    private float nextSpawnTime = 0f;         // Lưu mốc thời gian được phép xây tiếp

    [Header("Prefabs (Kéo thả từ thư mục Prefabs)")]
    [SerializeField] GameObject previewBambooPrefab;
    [SerializeField] GameObject solidBambooPrefab;
    [SerializeField] GameObject chunkBambooPrefab;

    [Header("Kiểm tra Mặt đất")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Vector2 checkSize = new Vector2(0.9f, 0.15f);

    private Vector2 dragStartPos;
    private bool isDragging = false;

    private List<GameObject> activePreviews = new List<GameObject>();

    // --- NEW: "Cuốn sổ tay" ghi nhớ các khối tre đã sinh ra ---
    private List<GameObject> spawnedChunks = new List<GameObject>();

    private void Start()
    {
        stats = GetComponent<PlayerAttributes>();
    }

    void Update()
    {
        HandleConstructionInput();
    }

    void HandleConstructionInput()
    {
        // --- ĐÃ SỬA: Chặn toàn bộ thao tác (kể cả Preview) nếu đang trong thời gian Cooldown ---
        if (Time.time < nextSpawnTime) return;

        // 1. CHUỘT PHẢI: Hủy bỏ thao tác
        if (Input.GetMouseButtonDown(1) && isDragging)
        {
            CancelDragging();
            return;
        }

        // 2. NHẤN CHUỘT TRÁI: Bắt đầu vẽ
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        // 3. GIỮ CHUỘT TRÁI: Hiển thị Preview
        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            UpdatePreview(dragStartPos, currentMousePos);
        }

        // 4. NHẢ CHUỘT TRÁI: Chốt đơn! Sinh tre ngay lập tức
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector2 finalMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Nếu xây thành công ít nhất 1 đốt tre, thì mới bắt đầu tính thời gian Cooldown
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
            Vector2 segmentCenter = start + direction * (i * segmentLength + segmentLength / 2f);

            bool isHittingGround = Physics2D.OverlapBox(segmentCenter, checkSize, angle, groundLayer);

            if (!isHittingGround)
            {
                GameObject preview = Instantiate(previewBambooPrefab, segmentCenter, rotation);
                preview.transform.localScale = new Vector3(segmentLength, preview.transform.localScale.y, 1f);
                activePreviews.Add(preview);
            }
        }
    }

    // --- ĐÃ SỬA: Đổi kiểu trả về thành 'bool' để báo cho hệ thống biết có xây thành công hay không ---
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

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 segmentCenter = start + direction * (i * segmentLength + segmentLength / 2f);
            bool isHittingGround = Physics2D.OverlapBox(segmentCenter, checkSize, angle, groundLayer);

            if (!isHittingGround)
            {
                if (stats.currentBambooCount < totalCost + costPerSegment)
                {
                    Debug.Log("<color=red>HẾT TRE!</color> Không thể xây thêm.");
                    break;
                }

                if (currentChunk == null)
                {
                    currentChunk = Instantiate(chunkBambooPrefab, Vector3.zero, Quaternion.identity);
                    currentChunk.GetComponent<Rigidbody2D>().gravityScale = gravityScale;

                    // --- NEW: Ghi chú khối tre mới này vào sổ tay ---
                    spawnedChunks.Add(currentChunk);
                }

                GameObject solidSegment = Instantiate(solidBambooPrefab, segmentCenter, rotation);
                solidSegment.transform.localScale = new Vector3(segmentLength, solidSegment.transform.localScale.y, 1f);
                solidSegment.transform.SetParent(currentChunk.transform);

                totalCost += costPerSegment;
            }
            else
            {
                currentChunk = null;
            }
        }

        if (totalCost > 0)
        {
            stats.currentBambooCount -= totalCost;
            Debug.Log($"<color=green>XÂY DỰNG THÀNH CÔNG!</color> Đã tiêu hao: {totalCost} đốt. Tre còn lại: {stats.currentBambooCount}");
            return true; // Trả về true để kích hoạt Cooldown
        }

        return false; // Nếu không tốn đồng nào (vd: vẽ toàn đâm vào tường) thì không bị Cooldown
    }

    void CancelDragging()
    {
        isDragging = false;
        ClearPreviews();
    }

    void ClearPreviews()
    {
        foreach (GameObject preview in activePreviews)
        {
            Destroy(preview);
        }
        activePreviews.Clear();
    }

    // --- NEW: Hàm công khai để Checkpoint gọi khi cần dọn dẹp ---
    public void ClearAllSpawnedBamboo()
    {
        foreach (GameObject chunk in spawnedChunks)
        {
            // Kiểm tra xem nó còn tồn tại không trước khi xóa (tránh lỗi null)
            if (chunk != null)
            {
                Destroy(chunk);
            }
        }
        // Xóa sạch sổ tay
        spawnedChunks.Clear();
    }
}