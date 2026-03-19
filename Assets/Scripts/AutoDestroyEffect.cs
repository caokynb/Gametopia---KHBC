using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    // Thời gian tồn tại của hiệu ứng (nên bằng hoặc dài hơn thời gian animation 1 chút)
    [SerializeField] float lifetime = 1f;

    void Start()
    {
        // Hẹn giờ xóa chính GameObject này sau khoảng thời gian lifetime
        Destroy(gameObject, lifetime);
    }
}