using UnityEngine;

// Tự động thêm các Component này vào Player nếu chưa có, tránh quên gán linh kiện gây lỗi
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    // 1. CHANGED: Gọi Data Class của bạn vào đây thay vì viết lẻ tẻ!
    [Header("Dữ liệu Nhân vật")]
    public PlayerAttributes stats;

    [Header("Cài đặt Dốc (Slope)")]
    public float maxSlopeAngle = 60f;        // Góc dốc tối đa nhân vật có thể leo (đơn vị: Độ)
    public float groundCheckDistance = 0.5f; // Chiều dài tia laser bắn xuống để dò mặt đất
    public LayerMask groundLayer;            // Phân loại Layer nào được coi là "Mặt đất"

    // Các biến chứa linh kiện vật lý
    private Rigidbody2D rb;
    private CapsuleCollider2D cc;

    // Các biến trạng thái của nhân vật
    private float moveInput;         // Lưu giá trị bấm phím A/D (-1 đến 1)
    private bool isGrounded;         // Cờ xác nhận nhân vật đang chạm đất
    private bool isOnSlope;          // Cờ xác nhận nhân vật đang đứng trên dốc
    private Vector2 slopeNormalPerp; // Hướng di chuyển vuông góc với mặt dốc (giúp nhân vật đi chéo ôm theo dốc)
    private float slopeAngle;        // Lưu góc của mặt dốc hiện tại

    // Các biến quản lý Nhảy (Cơ chế Bunny Hop / Auto-hop)
    private bool isJumpHeld;               // Người chơi có ĐANG GIỮ phím nhảy không?
    private float lastJumpTime;            // Lưu mốc thời gian của cú nhảy gần nhất
    private float jumpCooldown = 0.1f;     // Thời gian "mù" tạm thời sau khi nhảy để nhân vật kịp tách khỏi mặt đất

    void Start()
    {
        // Lấy các linh kiện đã gắn trên nhân vật
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<CapsuleCollider2D>();

        // Khóa trục Z để nhân vật không bị lăn lông lốc khi va chạm
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 2. CHANGED: Trỏ vào stats để lấy normalGravity
        rb.gravityScale = stats.normalGravity;
    }

    void Update()
    {
        // Update chạy mỗi khung hình: Dùng để hứng tín hiệu từ bàn phím mượt nhất
        moveInput = Input.GetAxisRaw("Horizontal");

        // Dùng GetKey để lấy trạng thái ĐANG GIỮ phím. Giúp tạo cơ chế "Chạm đất là tự nhảy tiếp" (Bunny Hop)
        isJumpHeld = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space);
    }

    void FixedUpdate()
    {
        // FixedUpdate chạy theo nhịp vật lý cố định: Dùng để tính toán di chuyển/va chạm
        CheckGroundedAndSlope();
        ApplyMovement();
    }

    void CheckGroundedAndSlope()
    {
        // BƯỚC 1: XỬ LÝ NHẢY (JUMP COOLDOWN)
        if (Time.time < lastJumpTime + jumpCooldown)
        {
            isGrounded = false;
            isOnSlope = false;
            return;
        }

        // BƯỚC 2: QUÉT MẶT ĐẤT
        Vector2 origin = cc.bounds.center;
        float rayLength = cc.bounds.extents.y + groundCheckDistance;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, groundLayer);

        if (hit)
        {
            isGrounded = true;
            slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeAngle > 0.1f && slopeAngle <= maxSlopeAngle)
            {
                isOnSlope = true;
                slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            }
            else
            {
                isOnSlope = false;
            }
        }
        else
        {
            isGrounded = false;
            isOnSlope = false;
        }
    }

    void ApplyMovement()
    {
        // 1. LỆNH NHẢY (ƯU TIÊN XỬ LÝ TRƯỚC)
        if (isJumpHeld && isGrounded)
        {
            // CHANGED: Trỏ vào stats để lấy jumpForce
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);
            lastJumpTime = Time.time;
            isGrounded = false;
            isOnSlope = false;
        }

        // 2. LỆNH DI CHUYỂN
        // TRƯỜNG HỢP A: Đang đứng yên trên đất hoặc trên dốc (Phanh tự động chống trượt)
        if (isGrounded && moveInput == 0f)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            rb.gravityScale = 0f;
        }
        // TRƯỜNG HỢP B: Đang di chuyển trên mặt dốc
        else if (isGrounded && isOnSlope)
        {
            // CHANGED: Trỏ vào stats để lấy normalGravity và moveSpeed
            rb.gravityScale = stats.normalGravity;
            rb.linearVelocity = new Vector2(-moveInput * stats.moveSpeed * slopeNormalPerp.x,
                                            -moveInput * stats.moveSpeed * slopeNormalPerp.y);
        }
        // TRƯỜNG HỢP C: Đang chạy trên đường phẳng HOẶC đang bay lơ lửng trên không
        else
        {
            // CHANGED: Trỏ vào stats để lấy normalGravity và moveSpeed
            rb.gravityScale = stats.normalGravity;
            rb.linearVelocity = new Vector2(moveInput * stats.moveSpeed, rb.linearVelocity.y);
        }
    }

    void OnDrawGizmos()
    {
        if (cc == null) cc = GetComponent<CapsuleCollider2D>();
        if (cc != null)
        {
            Gizmos.color = Color.red;
            Vector2 start = cc.bounds.center;
            Vector2 end = start + Vector2.down * (cc.bounds.extents.y + groundCheckDistance);
            Gizmos.DrawLine(start, end);
        }
    }
}