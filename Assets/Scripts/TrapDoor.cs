using UnityEngine;
using System.Collections;

public class TrapDoor : MonoBehaviour
{
    [Header("Cấu hình")]
    public float fallDelay = 0.5f;      // Thời gian chờ sập
    public float restoreDelay = 2.0f;   // Thời gian sàn hiện lại (để người chơi chơi tiếp)

    private SpriteRenderer sr;
    private Collider2D col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Khi người chơi dẫm lên (kiểm tra Tag)
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Collapse());
        }
    }

    IEnumerator Collapse()
    {
        // Có thể thêm hiệu ứng rung nhẹ hoặc đổi màu đỏ cảnh báo ở đây
        sr.color = Color.red;

        // Chờ 0.5 giây
        yield return new WaitForSeconds(fallDelay);

        // Biến mất (tắt hình ảnh và va chạm)
        sr.enabled = false;
        col.enabled = false;

        // Chờ một lúc rồi hiện lại
        yield return new WaitForSeconds(restoreDelay);
        sr.enabled = true;
        col.enabled = true;
        sr.color = Color.white; // Trả lại màu bình thường
    }
}