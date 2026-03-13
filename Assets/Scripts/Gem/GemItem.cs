using UnityEngine;

public class GemItem : MonoBehaviour
{
    public enum GemType { HalfCost, Invincible }
    public GemType gemType; // Chọn loại ngọc trong Inspector
    public float duration = 10f; // Thời gian tác dụng (giây)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu vật chạm vào là Player (Nhớ đặt Tag nhân vật là Player)
        if (collision.CompareTag("Player"))
        {
            PlayerAttributes stats = collision.GetComponent<PlayerAttributes>();

            if (stats != null)
            {
                if (gemType == GemType.HalfCost)
                {
                    stats.isHalfCostActive = true;
                    stats.CancelInvoke("DisableHalfCost"); // Reset thời gian nếu nhặt thêm
                    stats.Invoke("DisableHalfCost", duration);
                    Debug.Log("Nhặt ngọc GIẢM CHI PHÍ!");
                }
                else if (gemType == GemType.Invincible)
                {
                    stats.isInvulnerable = true;
                    stats.CancelInvoke("DisableInvulnerable");
                    stats.Invoke("DisableInvulnerable", duration);
                    Debug.Log("Nhặt ngọc BẤT TỬ!");
                }

                Destroy(gameObject); // Nhặt xong cục ngọc biến mất
            }
        }
    }
}