using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Transform firePoint; // Kéo FirePoint vào đây
    public GameObject bulletPrefab; // Kéo file Đạn (Prefab) vào đây

    void Update()
    {
        // Nhấn nút chuột trái hoặc phím Ctrl để bắn
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Tạo ra viên đạn tại vị trí FirePoint
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}