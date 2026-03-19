using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Cấu hình Spawn")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public float respawnDelay = 2f;
    public int spawnLimit = 2;

    [Header("Cấu hình Vật Cản")]
    public DestructibleObject roadBlock;

    private GameObject currentEnemy;
    private int spawnedCount = 0;
    private int deathCount = 0;
    private bool isWaitingToRespawn = false;
    private bool hasTriggered = false;

    void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;
        SpawnNextEnemy();
    }

    void Update()
    {
        if (currentEnemy == null && !isWaitingToRespawn && !hasTriggered)
        {
            deathCount++;
            Debug.Log("Quái đã chết: " + deathCount);

            if (deathCount >= 2)
            {
                HandleRoadBlock();
                hasTriggered = true; // 🔥 chặn gọi lại
            }

            if (spawnedCount < spawnLimit)
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
        currentEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedCount++;

        // FIX SCALE
        Vector3 originalScale = enemyPrefab.transform.localScale;
        float direction = (spawnPoint.localScale.x > 0) ? 1f : -1f;

        currentEnemy.transform.localScale = new Vector3(
            originalScale.x * direction,
            originalScale.y,
            originalScale.z
        );
    }

    void HandleRoadBlock()
    {
        if (roadBlock != null)
        {
            Debug.Log("Đủ 2 quái → giảm máu vật cản xuống 2!");

            // ✅ Set máu trực tiếp
            roadBlock.health = 2;

            // Hiệu ứng cho đẹp
            roadBlock.SendMessage("FlashWhite", SendMessageOptions.DontRequireReceiver);
        }
    }
}