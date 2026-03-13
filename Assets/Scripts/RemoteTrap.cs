using UnityEngine;
using System.Collections;

public class RemoteTrap : MonoBehaviour
{
    [Header("Cài đặt Vật triệu hồi")]
    public GameObject projectilePrefab; // Kéo Prefab bước 1 vào đây
    public Transform spawnPoint;       // Kéo điểm xuất hiện vào đây

    [Header("Hiệu ứng Bẹp")]
    public Transform visualTransform;  // Hình ảnh hiển thị của bẫy
    public float squishY = 0.3f;       // Độ bẹp khi giẫm vào

    private Vector3 originalScale;
    private bool isActivated = false;

    void Start()
    {
        if (visualTransform != null) originalScale = visualTransform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isActivated)
        {
            StartCoroutine(TriggerSequence());
        }
    }

    IEnumerator TriggerSequence()
    {
        isActivated = true;

        // 1. Hiệu ứng bẹp xuống
        if (visualTransform != null)
            visualTransform.localScale = new Vector3(originalScale.x, originalScale.y * squishY, originalScale.z);

        // 2. Bắn vật thể
        if (projectilePrefab != null && spawnPoint != null)
        {
            Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        }

        // 3. Chờ 3 giây rồi hồi lại bẫy
        yield return new WaitForSeconds(3f);

        if (visualTransform != null) visualTransform.localScale = originalScale;
        isActivated = false;
    }
}