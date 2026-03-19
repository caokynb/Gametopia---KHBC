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

        int totalProjectedCost = validSegmentCount * finalCostPerSegment;

        // Vẫn giữ cảnh báo Đỏ nếu thả chuột ra là chết (Tre <= 0)
        bool willDieIfBuild = (stats.currentBambooCount - totalProjectedCost) <= 0;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 pos = start + direction * (i * segmentLength + segmentLength / 2f);
            if (!Physics2D.OverlapBox(pos, checkSize, angle, groundLayer))
            {
                GameObject prefabToUse = GetBambooPrefab(i, segmentCount);
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

        int totalCostRequired = validSegmentCount * actualCostPerSegment;

        // --- SỬA TẠI ĐÂY: Tháo đai an toàn ---
        // Đổi <= thành <. Bây giờ chỉ khi thật sự KHÔNG ĐỦ TIỀN mới cấm xây.
        // Còn nếu tiền vừa đủ để về 0, vẫn cho xây bình thường!
        if (validSegmentCount == 0 || stats.currentBambooCount < totalCostRequired)
        {
            Debug.Log("<color=red>Hủy lệnh xây:</color> Cần thêm tre mới xây được dải này!");
            return false;
        }

        GameObject currentChunk = null;
        int totalCost = 0;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 pos = start + direction * (i * segmentLength + segmentLength / 2f);
            if (!Physics2D.OverlapBox(pos, checkSize, angle, groundLayer))
            {
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

                GameObject prefabToUse = GetBambooPrefab(i, segmentCount);
                GameObject segment = Instantiate(prefabToUse, pos, rotation);
                segment.transform.SetParent(currentChunk.transform);

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