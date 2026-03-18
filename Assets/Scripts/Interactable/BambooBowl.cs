using UnityEngine;
using System.Collections;

public class BambooBowl : MonoBehaviour
{
    [Header("Cài đặt Lư Hương")]
    [Tooltip("Số lượng tre cần thiết cho CÁI LƯ NÀY")]
    public int targetAmount = 3;
    public float waitTime = 1f;
    public LayerMask bambooLayer; // Vẫn dùng LayerMask như code gốc của team

    [Header("Sprites")]
    public Sprite defaultSprite;
    public Sprite wrongSprite;
    public Sprite rightSprite;

    // Biến công khai để TrapManager có thể kiểm tra
    public bool isSolved = false;
    [HideInInspector] public int currentBamboo = 0;

    private SpriteRenderer sr;
    private bool isProcessing = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (defaultSprite != null) sr.sprite = defaultSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nhận diện tre bằng Layer (Code gốc của bạn kia)
        if (((1 << collision.gameObject.layer) & bambooLayer.value) != 0)
        {
            // Chạm vào là mất ngay
            Destroy(collision.gameObject);

            // Nếu đã giải xong (màu Xanh) thì lư cứ nuốt tre nhưng không đếm nữa
            if (isSolved) return;

            currentBamboo++;

            // Bắt đầu chờ 1 giây để người chơi thả hết tre vào (Code của bạn)
            if (!isProcessing)
            {
                StartCoroutine(CheckPuzzleRoutine());
            }
        }
    }

    IEnumerator CheckPuzzleRoutine()
    {
        isProcessing = true;

        // Đợi 1 giây để gom đủ "1 phát" tre rơi xuống
        yield return new WaitForSeconds(waitTime);

        // Chốt sổ!
        if (currentBamboo == targetAmount)
        {
            isSolved = true;
            yield return StartCoroutine(BlinkRoutine(rightSprite, true));
        }
        else
        {
            yield return StartCoroutine(BlinkRoutine(wrongSprite, false));
            currentBamboo = 0; // Sai thì tự reset số lượng về 0
        }

        isProcessing = false;
    }

    IEnumerator BlinkRoutine(Sprite blinkSprite, bool isCorrect)
    {
        // Nhấp nháy 2 lần
        for (int i = 0; i < 2; i++)
        {
            sr.sprite = blinkSprite;
            yield return new WaitForSeconds(0.2f);
            sr.sprite = defaultSprite;
            yield return new WaitForSeconds(0.2f);
        }

        // Chốt màu cuối cùng
        if (isCorrect)
        {
            sr.sprite = blinkSprite; // Giữ luôn màu Xanh
        }
        else
        {
            sr.sprite = defaultSprite; // Quay về Default
        }
    }

    // Giữ lại hàm ResetBowl phòng trường hợp Manager cần gọi khẩn cấp
    public void ResetBowl()
    {
        StopAllCoroutines();
        currentBamboo = 0;
        isSolved = false;
        isProcessing = false;
        if (sr != null && defaultSprite != null) sr.sprite = defaultSprite;
    }
}