using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    // Biến này sẽ được điều khiển bởi Animation
    public bool isDangerous = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu chạm vào người chơi VÀ bẫy đang ở trạng thái nguy hiểm
        if (collision.CompareTag("Player") && isDangerous)
        {
            PlayerCombat pc = collision.GetComponent<PlayerCombat>();
            if (pc != null)
            {
                pc.Respawn();
                Debug.Log("Bị gai đâm rồi!");
            }
        }
    }

    // Hàm này dùng để check va chạm liên tục nếu người chơi đứng im trên bẫy
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isDangerous)
        {
            collision.GetComponent<PlayerCombat>()?.Respawn();
        }
    }
}