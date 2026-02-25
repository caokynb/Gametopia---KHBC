using UnityEngine;

[System.Serializable]
public class PlayerAttributes : MonoBehaviour
{
    [Header("Thông số Cơ bản")]
    public int maxBambooCount = 100;
    public int currentBambooCount = 100;
    public int burnBambooOnAttack = 0;
    public int burnBambooPerUnit = 0;
    public int healthPoint = 1;

    [Header("Thông số Di chuyển")]
    public float moveSpeed = 8f;     // Tốc độ chạy ngang của nhân vật
    public float jumpForce = 15f;    // Lực nảy lên khi nhảy
    public float normalGravity = 3f; // Lực hút trái đất mặc định (rơi nhanh hơn để có cảm giác game Platformer)
}
