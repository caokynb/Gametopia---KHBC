using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    public float speed = 12f;
    public Vector3 direction = Vector3.left; // Mặc định bay sang trái
    public float destroyTime = 5f; // Tự xóa sau 5 giây để nhẹ máy

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // Di chuyển liên tục theo hướng chỉ định
        transform.position += direction * speed * Time.deltaTime;
    }
}