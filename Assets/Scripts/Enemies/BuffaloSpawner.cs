using UnityEngine;
using System.Collections;

public class BuffaloSpawner : MonoBehaviour
{
    [Header("Cấu hình Spawner")]
    public GameObject buffaloPrefab;

    [Tooltip("Layer của mặt đất (Để tránh spawn vào tường)")]
    public LayerMask groundLayer;

    [Tooltip("Tỷ lệ xuất hiện mỗi giây (0 đến 100%)")]
    [Range(0f, 100f)]
    public float spawnChancePerSecond = 15f;

    [Tooltip("Khoảng cách xuất hiện ngoài màn hình")]
    public float offScreenDistanceX = 15f;

    [Tooltip("Độ rộng BẮT BUỘC của bãi đất (Nên để 8-10 cho bãi rộng)")]
    public float requiredFlatWidth = 8f;

    [Tooltip("Độ cao khoảng không BẮT BUỘC (Tránh kẹt vách đá)")]
    public float requiredClearanceHeight = 3f;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (Random.Range(0f, 100f) <= spawnChancePerSecond)
            {
                SpawnBuffalo();
            }
        }
    }

    private void SpawnBuffalo()
    {
        if (buffaloPrefab == null) return;

        bool spawnLeft = Random.value > 0.5f;
        float xPos = transform.position.x + (spawnLeft ? -offScreenDistanceX : offScreenDistanceX);

        // 1. Bắn tia chính giữa
        Vector2 rayStart = new Vector2(xPos, transform.position.y + 15f);
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 40f, groundLayer);

        if (hit.collider != null && hit.normal.y > 0.9f)
        {
            // 2. Bắn tia Trái & Phải
            float halfWidth = requiredFlatWidth / 2f;
            Vector2 leftStart = new Vector2(xPos - halfWidth, rayStart.y);
            Vector2 rightStart = new Vector2(xPos + halfWidth, rayStart.y);

            RaycastHit2D hitLeft = Physics2D.Raycast(leftStart, Vector2.down, 40f, groundLayer);
            RaycastHit2D hitRight = Physics2D.Raycast(rightStart, Vector2.down, 40f, groundLayer);

            if (hitLeft.collider != null && hitRight.collider != null)
            {
                // LUẬT THÉP: Độ cao 3 điểm KHÔNG ĐƯỢC LỆCH QUÁ 0.05 đơn vị (Phẳng tuyệt đối)
                bool isStrictlyFlatLeft = Mathf.Abs(hitLeft.point.y - hit.point.y) < 0.05f;
                bool isStrictlyFlatRight = Mathf.Abs(hitRight.point.y - hit.point.y) < 0.05f;

                if (isStrictlyFlatLeft && isStrictlyFlatRight)
                {
                    // 3. Quét một chiếc hộp CỰC TO bên trên mặt đất để dò vách tường
                    // Nâng tâm hộp lên sao cho mép dưới của hộp vừa chạm mặt đất
                    Vector2 boxCenter = hit.point + new Vector2(0f, (requiredClearanceHeight / 2f) + 0.1f);
                    Vector2 boxSize = new Vector2(requiredFlatWidth, requiredClearanceHeight);

                    Collider2D stuckCheck = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer);

                    // Nếu chiếc hộp lớn này hoàn toàn RỖNG -> Vị trí tuyệt đẹp!
                    if (stuckCheck == null)
                    {
                        GameObject buffalo = Instantiate(buffaloPrefab, hit.point, Quaternion.identity);
                        EnemyAI ai = buffalo.GetComponent<EnemyAI>();

                        if (ai != null) ai.SetChargeDirection(spawnLeft);
                    }
                }
            }
        }
    }
    // Thêm vào trong class BuffaloSpawner
    public void StopAndClearBuffalo()
    {
        // 1. Dừng Coroutine sinh trâu
        StopAllCoroutines();

        // 2. Tìm tất cả con trâu đang có trong Scene và làm chúng mờ đi
        // Giả sử Prefab trâu của bạn có gắn script BuffaloFade ở bước 1
        BuffaloFade[] allBuffalo = FindObjectsByType<BuffaloFade>(FindObjectsSortMode.None);
        foreach (BuffaloFade b in allBuffalo)
        {
            b.StartFadeOut(2f); // Mờ dần trong 2 giây
        }
    }

    // Vẽ hộp kiểm tra ra màn hình Scene để bạn dễ gỡ lỗi!
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        // Mô phỏng vị trí hộp bên phải (x + offScreenDistanceX)
        Vector2 dummyHitPoint = new Vector2(transform.position.x + offScreenDistanceX, transform.position.y - 2f);
        Vector2 dummyBoxCenter = dummyHitPoint + new Vector2(0f, (requiredClearanceHeight / 2f) + 0.1f);
        Vector3 dummyBoxSize = new Vector3(requiredFlatWidth, requiredClearanceHeight, 1f);
        Gizmos.DrawCube(dummyBoxCenter, dummyBoxSize);
    }
}