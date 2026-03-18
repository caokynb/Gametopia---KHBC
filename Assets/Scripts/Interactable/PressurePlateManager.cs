using UnityEngine;
using UnityEngine.Events;

public class PressurePlateManager : MonoBehaviour
{
    [Header("Danh sách các nút cần giẫm")]
    public PressurePlate[] plates;

    [Header("Sự kiện khi giẫm ĐỦ tất cả các nút")]
    public UnityEvent onAllPlatesPressed;

    private bool isSolved = false;

    void Update()
    {
        if (isSolved) return; // Xong rồi thì không kiểm tra nữa

        // Quét qua toàn bộ danh sách nút
        bool allPressed = true;
        foreach (PressurePlate plate in plates)
        {
            if (!plate.isPressed)
            {
                allPressed = false; // Chỉ cần 1 nút chưa giẫm là break luôn
                break;
            }
        }

        // Nếu tất cả đều isPressed = true
        if (allPressed)
        {
            isSolved = true;
            Debug.Log("ĐÃ GIẪM ĐỦ TẤT CẢ CÁC NÚT! MỞ CỬA!");
            onAllPlatesPressed.Invoke(); // Kích hoạt sự kiện ở đây!
        }
    }
}