using UnityEngine;
using UnityEngine.InputSystem;
public class playerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public InputSystem_Actions actions;
    public float moveSpeed=5f;
    public float jumpForce=5f;

    public Transform groundCheckTransform;
    public Vector2 groundCheckSize = new Vector2(1f,1f);
    public LayerMask groundLayer;
    
    bool isGrounded;
    float move;

    Rigidbody2D rb;

    void Awake()
    {
        actions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        if (actions == null)
        {
            actions = new InputSystem_Actions();
        }
        actions.Player.Enable();
        actions.Player.Move.performed += Movement;
        actions.Player.Jump.performed += Jumping;

        actions.Player.Move.canceled += Movement;
        actions.Player.Jump.canceled += Jumping;
    }

    void OnDisable()
    {
        actions.Player.Disable();
        actions.Player.Move.performed -= Movement;
        actions.Player.Jump.performed -= Jumping;
    }

    void Movement(InputAction.CallbackContext ctx)
    {
        move = ctx.ReadValue<Vector2>().x;
    }

    void Jumping(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && isGrounded)
        {
            rb.linearVelocityY = jumpForce;
        }
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();    
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics2D.OverlapBox(groundCheckTransform.position, groundCheckSize, 0f, groundLayer);
        rb.linearVelocityX = move * moveSpeed;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckTransform.position, groundCheckSize);
    }
}
