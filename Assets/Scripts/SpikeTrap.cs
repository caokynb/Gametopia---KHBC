using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    [Header("Cài đặt Gai")]
    [SerializeField] GameObject spikePrefab;
    [SerializeField] Transform spikeSpawnPoint; // Điểm gai mọc lên (nên nằm trên mặt bẫy)

    [Header("Hiệu ứng Bẹp")]
    [SerializeField] Transform trapVisual; // Kéo object hình ảnh bẫy vào đây
    [SerializeField] float squishAmount = 0.3f; // Độ bẹp (0.3 nghĩa là còn 30% chiều cao)
    [SerializeField] float squishSpeed = 10f;

    private bool triggered = false;
    private Vector3 originalScale;

    private void Start()
    {
        if (trapVisual != null)
            originalScale = trapVisual.localScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;

        if (collision.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(ActivateTrap());
        }
    }

    private IEnumerator ActivateTrap()
    {
        // 1. Hiệu ứng bẹp xuống
        if (trapVisual != null)
        {
            Vector3 targetScale = new Vector3(originalScale.x, originalScale.y * squishAmount, originalScale.z);
            float elapsed = 0;
            while (elapsed < 0.1f)
            {
                trapVisual.localScale = Vector3.Lerp(trapVisual.localScale, targetScale, Time.deltaTime * squishSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }
            trapVisual.localScale = targetScale;
        }

        // 2. Kích hoạt gai
        // Spawn gai ngay tại vị trí bẫy (thường là mọc từ dưới lên)
        if (spikePrefab != null)
        {
            Vector3 spawnPos = spikeSpawnPoint != null ? spikeSpawnPoint.position : transform.position;
            GameObject spikes = Instantiate(spikePrefab, spawnPos, Quaternion.identity);

            // Nếu muốn gai biến mất sau 1 khoảng thời gian
            Destroy(spikes, 2f);
        }

        // 3. (Tùy chọn) Hồi lại bẫy sau vài giây
        yield return new WaitForSeconds(3f);
        ResetTrap();
    }

    private void ResetTrap()
    {
        if (trapVisual != null) trapVisual.localScale = originalScale;
        triggered = false;
    }
}