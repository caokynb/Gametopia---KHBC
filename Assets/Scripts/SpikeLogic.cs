using UnityEngine;

public class SpikeLogic : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float activeTime = 0.5f;
    private Vector3 targetPosition;

    void Start()
    {
        // Lưu vị trí đích (vị trí hiện tại khi vừa sinh ra)
        targetPosition = transform.position;
        // Đặt vị trí bắt đầu thấp xuống một chút để tạo hiệu ứng trồi lên
        transform.position += Vector3.down * 1.5f;
    }

    void Update()
    {
        // Gai trồi lên vị trí đích
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }
}