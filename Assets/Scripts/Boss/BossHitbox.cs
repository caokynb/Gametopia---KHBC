using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(1); // Gọi hàm chớp đỏ và trừ máu!
            }
            Destroy(gameObject); // (Dòng này chỉ giữ lại trong BossRock.cs nhé)
        }

        // Húc vỡ tre trên đường đi
        if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Bamboo")) != 0)
        {
            Destroy(collision.gameObject);
        }
    }
}