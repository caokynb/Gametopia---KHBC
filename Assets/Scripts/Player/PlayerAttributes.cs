using UnityEngine;

[System.Serializable]
public class PlayerAttributes 
{
    [Header("Thông số Cơ bản")]
    public int maxBambooCount = 100;
    public int currentBambooCount = 100;
    public int burnBambooOnAttack = 0;
    public int burnBambooPerUnit = 0;

    // Đã tách riêng Máu Tối Đa và Máu Hiện Tại
    public int maxHealth = 4;
    public int healthPoint = 4;

    [Header("Thông số Di chuyển")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    public float normalGravity = 3f;
}