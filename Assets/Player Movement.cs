using UnityEngine;

// Tự động thêm các Component này vào Player nếu chưa có, tránh quên gán linh kiện gây lỗi
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Thông số Di chuyển")]
    public float moveSpeed = 8f;     // Tốc độ chạy ngang của nhân vật
    public float jumpForce = 15f;    // Lực nảy lên khi nhảy
    public float normalGravity = 3f; // Lực hút trái đất mặc định (rơi nhanh hơn để có cảm giác game Platformer)

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
        rb.gravityScale = normalGravity;
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
        // Nếu vừa mới nhảy xong (chưa qua 0.1 giây), thì ngưng quét mặt đất.
        // Việc này ngăn lỗi hàm di chuyển ép vận tốc về 0 khi nhân vật chưa kịp bay lên.
        if (Time.time < lastJumpTime + jumpCooldown)
        {
            isGrounded = false;
            isOnSlope = false;
            return;
        }

        // BƯỚC 2: QUÉT MẶT ĐẤT
        // Bắn 1 tia Laser từ giữa bụng cắm xuống qua khỏi chân một chút
        Vector2 origin = cc.bounds.center;
        float rayLength = cc.bounds.extents.y + groundCheckDistance;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, groundLayer);

        if (hit)
        {
            // Nếu tia laser chạm vào thứ gì đó thuộc Ground Layer
            isGrounded = true;

            // Tính toán góc của mặt đất tại điểm chạm
            slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            // Kiểm tra xem góc đó có phải là dốc không (lớn hơn 0 và nhỏ hơn giới hạn leo)
            if (slopeAngle > 0.1f && slopeAngle <= maxSlopeAngle)
            {
                isOnSlope = true;
                // Tính vector vuông góc với mặt dốc để nhân vật biết đường đi chéo lên/xuống
                slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            }
            else
            {
                isOnSlope = false; // Đứng trên mặt phẳng
            }
        }
        else
        {
            // Tia laser bay vào hư không -> Đang lơ lửng trên trời
            isGrounded = false;
            isOnSlope = false;
        }
    }

    void ApplyMovement()
    {
        // 1. LỆNH NHẢY (ƯU TIÊN XỬ LÝ TRƯỚC)
        // Nếu người chơi đang đè phím nhảy và nhân vật đang chạm đất -> Nhảy ngay lập tức
        if (isJumpHeld && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            lastJumpTime = Time.time; // Ghi nhớ lại thời điểm vọt lên
            isGrounded = false;       // Ép rời đất
            isOnSlope = false;
        }

        // 2. LỆNH DI CHUYỂN
        // TRƯỜNG HỢP A: Đang đứng yên trên đất hoặc trên dốc (Phanh tự động chống trượt)
        if (isGrounded && moveInput == 0f)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Chỉ set vận tốc X = 0, giữ nguyên vận tốc Y
            rb.gravityScale = 0f;             // Tắt trọng lực để không bị tuột dốc
        }
        // TRƯỜNG HỢP B: Đang di chuyển trên mặt dốc
        else if (isGrounded && isOnSlope)
        {
            rb.gravityScale = normalGravity;  // Bật lại trọng lực
            // Nhân vận tốc với vector góc nghiêng để đi bám theo mặt dốc
            rb.linearVelocity = new Vector2(-moveInput * moveSpeed * slopeNormalPerp.x,
                                            -moveInput * moveSpeed * slopeNormalPerp.y);
        }
        // TRƯỜNG HỢP C: Đang chạy trên đường phẳng HOẶC đang bay lơ lửng trên không
        else
        {
            rb.gravityScale = normalGravity;
            // Chỉ thay đổi vận tốc chiều ngang (X), giữ nguyên lực rớt của chiều dọc (Y)
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    // Hàm Debug: Vẽ tia laser màu đỏ trong tab Scene để Dev dễ dàng tinh chỉnh độ dài quét đất
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