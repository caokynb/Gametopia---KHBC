using UnityEngine;

[System.Serializable]
public class PlayerAttributes : MonoBehaviour
{
    [Header("Thông số Cơ bản")]
    public int maxBambooCount = 100;
    public int currentBambooCount = 100;
    public int burnBambooOnAttack = 0;
    public int burnBambooPerUnit = 0;
    public int healthPoint = 3;

    [Header("Thông số Di chuyển")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    public float normalGravity = 3f;

    [Header("Trạng thái từ Ngọc (Thêm mới)")]
    public bool isHalfCostActive = false; // Trạng thái giảm nửa tiền
    public bool isInvulnerable = false;   // Trạng thái bất tử


    // Hàm để kẻ địch gọi khi tấn công bạn
    public void TakeDamage(int damage)
    {
        if (isInvulnerable)
        {
            Debug.Log("Đang bất tử! Không nhận sát thương.");
            return;
        }

        healthPoint -= damage;
        if (healthPoint <= 0) Debug.Log("Nhân vật đã chết!");
    }

    // Các hàm tắt hiệu ứng (Dùng để Invoke)
    public void DisableHalfCost() { isHalfCostActive = false; }
    public void DisableInvulnerable() { isInvulnerable = false; }
}