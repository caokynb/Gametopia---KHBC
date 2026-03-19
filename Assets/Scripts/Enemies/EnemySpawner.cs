using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Cấu hình Spawn Quái")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public float respawnDelay = 2f;
    public int spawnLimit = 2;

    [Header("Cấu hình Vật Thể Mới (Xuất hiện sau khi thắng)")]
    public GameObject objectToAppearPrefab; // Kéo Prefab vật thể mới vào đây
    public Transform appearLocation;        // Kéo một Empty Object làm vị trí chỉ định vào đây

    [Header("Kết nối Spawner Trâu & Vật Cản")]
    public BuffaloSpawner buffaloSpawner;
    public DestructibleObject roadBlock;

    private GameObject currentEnemy;
    private int spawnedCount = 0;
    private int deathCount = 0;
    private bool isWaitingToRespawn = false;
    private bool hasTriggeredFinal = false; // Để đảm bảo chỉ xuất hiện 1 lần

    void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;
        SpawnNextEnemy();
    }

    void Update()
    {
        // Kiểm tra nếu quái hiện tại đã chết
        if (currentEnemy == null && !isWaitingToRespawn && !hasTriggeredFinal)
        {
            deathCount++;
            Debug.Log("Quái đã chết: " + deathCount);

            if (deathCount >= 2)
            {
                HandleVictory();
                hasTriggeredFinal = true;
            }
            else if (spawnedCount < spawnLimit)
            {
                StartCoroutine(RespawnRoutine());
            }
        }
    }

    IEnumerator RespawnRoutine()
    {
        isWaitingToRespawn = true;
        yield return new WaitForSeconds(respawnDelay);

        if (spawnedCount < spawnLimit)
        {
            SpawnNextEnemy();
        }

        isWaitingToRespawn = false;
    }

    void SpawnNextEnemy()
    {
        // 1. Tạo quái
        currentEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedCount++;

        // 2. Ép Scale chuẩn (giữ nguyên kích thước bạn đã chỉnh ở Prefab)
        Vector3 prefScale = enemyPrefab.transform.localScale;
        float dir = (spawnPoint.localScale.x > 0) ? 1f : -1f;
        currentEnemy.transform.localScale = new Vector3(prefScale.x * dir, prefScale.y, prefScale.z);

        // 3. QUAN TRỌNG: Cập nhật lại các tham số AI
        EnemyAI ai = currentEnemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            // Đảm bảo quái biết nó đang nhìn hướng nào để AI chạy đúng
            // (Giả sử bạn đã thêm dòng này vào Start của EnemyAI như tôi hướng dẫn trước đó)

            // Nếu quái to hơn, hãy thử tăng nhẹ tầm đánh bằng code nếu cần
            // ai.attackRange = 2f; 
        }

        // 4. Reset vận tốc để tránh quái bị bay lung tung khi vừa spawn
        Rigidbody2D rb = currentEnemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void HandleVictory()
    {
        // 1. Dừng đàn trâu và làm chúng biến mất
        if (buffaloSpawner != null)
        {
            buffaloSpawner.StopAndClearBuffalo();
        }

        // 2. Làm vật thể mới xuất hiện tại nơi chỉ định
        if (objectToAppearPrefab != null && appearLocation != null)
        {
            Instantiate(objectToAppearPrefab, appearLocation.position, appearLocation.rotation);
            Debug.Log("Vật thể mới đã xuất hiện tại: " + appearLocation.name);
        }

        // 3. Giảm máu vật cản (nếu cần)
        if (roadBlock != null)
        {
            roadBlock.health = 2;
            roadBlock.SendMessage("FlashWhite", SendMessageOptions.DontRequireReceiver);
        }
    }
}