using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Thông số di chuyển")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Kiểm tra mặt đất")]
    public Transform groundCheck; // Ô trống để kéo Object GroundCheck vào
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer; // Chọn Layer của mặt đất (ví dụ: Ground)

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;

    void Start()
    {
        // Lấy thành phần Rigidbody2D từ nhân vật
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Nhận đầu vào từ bàn phím (A/D hoặc Mũi tên)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Kiểm tra xem nhân vật có đang chạm mặt đất không
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 3. Xử lý Nhảy (Chỉ nhảy khi nhấn phím Jump và đang đứng trên đất)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 4. Xoay mặt nhân vật theo hướng đi
        if (horizontalInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        if (isGrounded) Debug.DrawRay(groundCheck.position, Vector2.down * 0.2f, Color.green);
        else Debug.DrawRay(groundCheck.position, Vector2.down * 0.2f, Color.red);
    }

    void FixedUpdate()
    {
        // Áp dụng vận tốc di chuyển vật lý
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
}