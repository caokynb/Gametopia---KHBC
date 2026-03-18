using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Trạng thái")]
    public bool isPressed = false; // Manager sẽ đọc biến này

    [Header("Hiệu ứng hình ảnh")]
    public Sprite unpressedSprite;
    public Sprite pressedSprite;
    public float squishSpeed = 10f;
    public float pressedScaleY = 0.5f;

    private SpriteRenderer sr;
    private Vector3 originalScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        if (unpressedSprite != null) sr.sprite = unpressedSprite;
    }

    void Update()
    {
        // Hiệu ứng lún mượt mà
        float targetY = isPressed ? originalScale.y * pressedScaleY : originalScale.y;
        Vector3 targetScale = new Vector3(originalScale.x, targetY, originalScale.z);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * squishSpeed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // CHỈ check "Player" và CHỈ chạy khi nút chưa bị giẫm
        if (!isPressed && collision.CompareTag("Player"))
        {
            isPressed = true;
            if (pressedSprite != null) sr.sprite = pressedSprite;
            Debug.Log("Player đã giẫm 1 nút!");
        }
    }
}