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
    [SerializeField] GameObject singleBambooPrefab;
    [SerializeField] GameObject startBambooPrefab;
    [SerializeField] GameObject middleBambooPrefab;
    [SerializeField] GameObject endBambooPrefab;

    [Header("Hệ thống")]
    [SerializeField] GameObject chunkBambooPrefab;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask dirtLayer;
    [SerializeField] Vector2 checkSize = new Vector2(0.9f, 0.15f);

    // --- THÊM BIẾN ÂM THANH Ở ĐÂY ---
    [Header("Âm thanh (SFX)")]
    [Tooltip("Tiếng tre đập vào nhau khi xây xong")]
    public AudioClip buildSound;

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

                // --- PHÁT ÂM THANH TẠI ĐÂY ---
                // Phát ra âm thanh ở vị trí con trỏ chuột lúc thả chuột ra
                if (buildSound != null)
                {
                    AudioSource.PlayClipAtPoint(buildSound, finalMousePos);
                }
            }
            ClearPreviews();
            isDragging = false;
        }
    }

    private int CalculateCost(bool isOnDirt)
    {
        int baseCost = isOnDirt ? (costPerSegment * 2) : costPerSegment;
        if (PlayerMovement.hasDiscountBuff)
        {
            return Mathf.CeilToInt(baseCost / 2f);
        }
        return baseCost;
    }

    void UpdatePreview(Vector2 start, Vector2 end)
    {
        ClearPreviews();
        Vector2 direction = end - start;
        float distance = direction.magnitude;
        int segmentCount = Mathf.FloorToInt(distance / segmentLength);

        if (segmentCount == 0) return;

        Collider2D hitDirt = Physics2D.OverlapPoint(start, dirtLayer);
        bool isOnDirt = hitDirt != null;
        int finalCostPerSegment = CalculateCost(isOnDirt);

        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        int validSegmentCount = 0;
        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 pos = start + direction * (i * segmentLength + segmentLength / 2f);
            if (!Physics2D.OverlapBox(pos, checkSize, angle, groundLayer))
            {
                validSegmentCount++;
            }
        }

        if (validSegmentCount == 0) return;

        // --- BÍ KÍP TA: CẮT BẢN NHÁP VỪA KHÍT VỚI SỐ TIỀN ---
        int maxAffordable = stats.currentBambooCount / finalCostPerSegment;
        int actualBuildCount = Mathf.Min(validSegmentCount, maxAffordable);
        bool willDieIfBuild = false;

        // Xử lý ngoại lệ: Không đủ tiền mua dù chỉ 1 đốt
        if (actualBuildCount == 0)
        {
            actualBuildCount = 1; // Ép vẽ 1 đốt
            willDieIfBuild = true; // Chắc chắn bị đỏ
        }
        else
        {
            int totalActualCost = actualBuildCount * finalCostPerSegment;
            willDieIfBuild = (stats.currentBambooCount - totalActualCost) <= 0;
        }

        int builtCount = 0; // Biến đếm số đốt thực tế được vẽ ra

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 pos = start + direction * (i * segmentLength + segmentLength / 2f);
            if (!Physics2D.OverlapBox(pos, checkSize, angle, groundLayer))
            {
                // Nếu đã vẽ chạm giới hạn tiền -> Ngừng vẽ các phần kéo dư
                if (builtCount >= actualBuildCount) break;

                // Tự động dùng đúng Prefab Khởi đầu / Thân / Kết thúc dựa trên độ dài đã cắt
                GameObject prefabToUse = GetBambooPrefab(builtCount, actualBuildCount);
                GameObject preview = Instantiate(prefabToUse, pos, rotation);

                var renderer = preview.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    if (willDieIfBuild)
                    {
                        renderer.color = new Color(1, 0, 0, 0.4f);
                    }
                    else
                    {
                        if (finalCostPerSegment < costPerSegment) renderer.color = new Color(0, 1, 0, 0.4f);
                        else if (finalCostPerSegment > costPerSegment) renderer.color = new Color(1, 0.6f, 0, 0.4f);
                        else renderer.color = new Color(1, 1, 1, 0.4f);
                    }
                }

                Collider2D[] colliders = preview.GetComponentsInChildren<Collider2D>();
                foreach (Collider2D col in colliders)
                {
                    col.enabled = false;
                }

                activePreviews.Add(preview);
                builtCount++; // Cộng dồn
            }
        }
    }

    bool SpawnSolidBamboo(Vector2 start, Vector2 end)
    {
        Vector2 direction = end - start;
        float distance = direction.magnitude;
        int segmentCount = Mathf.FloorToInt(distance / segmentLength);

        if (segmentCount == 0) return false;

        Collider2D hitDirt = Physics2D.OverlapPoint(start, dirtLayer);
        bool isOnDirt = hitDirt != null;
        int actualCostPerSegment = CalculateCost(isOnDirt);

        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        int validSegmentCount = 0;
        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 pos = start + direction * (i * segmentLength + segmentLength / 2f);
            if (!Physics2D.OverlapBox(pos, checkSize, angle, groundLayer))
            {
                validSegmentCount++;
            }
        }

        if (validSegmentCount == 0) return false;

        // --- CẮT GIỚI HẠN XÂY THEO TÚI TIỀN ---
        int maxAffordable = stats.currentBambooCount / actualCostPerSegment;
        int actualBuildCount = Mathf.Min(validSegmentCount, maxAffordable);

        // Thật sự nghèo không mua nổi 1 đốt -> Hủy lệnh
        if (actualBuildCount == 0) return false;

        GameObject currentChunk = null;
        int totalCost = 0;
        int builtCount = 0;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 pos = start + direction * (i * segmentLength + segmentLength / 2f);
            if (!Physics2D.OverlapBox(pos, checkSize, angle, groundLayer))
            {
                // Vắt kiệt tiền rồi thì dừng lại, không xây thêm đoạn kéo dư nữa
                if (builtCount >= actualBuildCount) break;

                if (currentChunk == null)
                {
                    currentChunk = Instantiate(chunkBambooPrefab, Vector3.zero, Quaternion.identity);
                    currentChunk.GetComponent<Rigidbody2D>().gravityScale = gravityScale;

                    if (isOnDirt)
                    {
                        FixedJoint2D joint = currentChunk.AddComponent<FixedJoint2D>();
                        joint.autoConfigureConnectedAnchor = false;
                        joint.anchor = currentChunk.transform.InverseTransformPoint(start);
                        if (hitDirt.attachedRigidbody != null)
                        {
                            joint.connectedBody = hitDirt.attachedRigidbody;
                        }
                        joint.connectedAnchor = start;
                        joint.breakForce = Mathf.Infinity;
                    }

                    spawnedChunks.Add(currentChunk);
                }

                // Dùng actualBuildCount để chốt ngọn tre chuẩn xác
                GameObject prefabToUse = GetBambooPrefab(builtCount, actualBuildCount);
                GameObject segment = Instantiate(prefabToUse, pos, rotation);
                segment.transform.SetParent(currentChunk.transform);

                totalCost += actualCostPerSegment;
                builtCount++;
            }
            else { currentChunk = null; }
        }

        if (totalCost > 0)
        {
            stats.currentBambooCount -= totalCost;
            return true; // Trả về true để báo là đã xây thành công
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

    void ClearPreviews() { foreach (var p in activePreviews) Destroy(p); activePreviews.Clear(); }
    void CancelDragging() { isDragging = false; ClearPreviews(); }
    public void ClearAllSpawnedBamboo()
    {
        foreach (var c in spawnedChunks) if (c != null) Destroy(c);
        spawnedChunks.Clear();
    }
}