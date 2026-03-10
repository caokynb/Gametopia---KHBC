using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    public float rotationSpeed = 300f;
    public float lifeTime = 4f;

    // Cờ đánh dấu: Ngăn chặn lỗi xuyên thấu 1-frame
    private bool hasHit = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu viên đá ĐÃ chạm vào một vật thể trước đó trong frame này, lập tức bỏ qua!
        if (hasHit) return;

        if (collision.CompareTag("Player"))
        {
            hasHit = true; // Khóa va chạm
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Bamboo"))
        {
            hasHit = true; // Khóa va chạm
            BambooSegment bamboo = collision.GetComponent<BambooSegment>();
            if (bamboo != null)
            {
                bamboo.TakeDamage(1);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasHit = true; // Khóa va chạm
            Destroy(gameObject);
        }
    }
}