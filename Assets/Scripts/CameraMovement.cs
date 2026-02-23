using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject VirtualCam;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // CHỈ xử lý khi vật thể chạm vào ĐÚNG là Player
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            VirtualCam.SetActive(true); // Bật camera của phòng này lên
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // CHỈ xử lý khi vật thể đi ra ĐÚNG là Player
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            VirtualCam.SetActive(false); // Tắt camera của phòng này đi khi Player rời khỏi
        }
    }
}