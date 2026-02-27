using UnityEngine;
using System.Collections;

public class DestructibleObject : MonoBehaviour
{
    [Header("Chỉ số")]
    public int health = 2; // Đã chỉnh thành 2 phát đánh để phá hủy

    [Header("Cấu hình vật phẩm rơi ra")]
    public GameObject dropItemPrefab;

    // Hàm nhận sát thương gọi từ PlayerCombat
    public void TakeDamage()
    {
        health -= 1;
        Debug.Log(gameObject.name + " bị trúng đòn! Máu còn: " + health);

        // Hiệu ứng nhấp nháy để người chơi biết đã đánh trúng
        StopAllCoroutines();
        StartCoroutine(FlashWhite());

        if (health <= 0)
        {
            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        Debug.Log("Vật thể đã bị phá hủy hoàn toàn!");
        if (dropItemPrefab != null)
        {
            Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = new Color(1f, 1f, 1f, 0.5f); // Làm nhạt màu hoặc đổi màu trắng
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
        }
    }
}