using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 4f;
    [SerializeField] private float jumpForce = 5f;

    private Rigidbody2D rb;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float dirX = Input.GetAxisRaw("Horizontal");

        // 1. JUMPING FIRST
        // We do this first so if we jump, isGrounded immediately becomes false
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }

        // 2. HORIZONTAL MOVEMENT & SLOPES SECOND
        // Because jump happened first, if we just jumped, this "if" safely skips 
        if (dirX == 0 && isGrounded)
        {
            // Standing completely still on the ground: turn to a statue
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            // Moving OR in the air: unfreeze and apply normal movement
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = new Vector2(dirX * speed, rb.linearVelocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}