using UnityEngine;
using System.Collections;

public class BambooBowl : MonoBehaviour
{
    [Header("Cài đặt Lư Hương")]
    public int targetAmount = 3;
    public float waitTime = 1f;
    public LayerMask bambooLayer;

    [Header("Âm thanh (SFX)")]
    public AudioClip insertSound;  // Tiếng khi nhét 1 đốt tre vào
    public AudioClip correctSound; // Tiếng khi Lư này sáng xanh (Xong 1 phần)

    [Header("Sprites")]
    public Sprite defaultSprite;
    public Sprite wrongSprite;
    public Sprite rightSprite;

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
        if (((1 << collision.gameObject.layer) & bambooLayer.value) != 0)
        {
            Destroy(collision.gameObject);
            if (isSolved) return;

            currentBamboo++;

            // --- PHÁT TIẾNG CẮM TRE ---
            if (insertSound != null) AudioSource.PlayClipAtPoint(insertSound, transform.position);

            if (!isProcessing) StartCoroutine(CheckPuzzleRoutine());
        }
    }

    IEnumerator CheckPuzzleRoutine()
    {
        isProcessing = true;
        yield return new WaitForSeconds(waitTime);

        if (currentBamboo == targetAmount)
        {
            isSolved = true;

            // --- PHÁT TIẾNG XÁC NHẬN ĐÚNG ---
            if (correctSound != null) AudioSource.PlayClipAtPoint(correctSound, transform.position);

            yield return StartCoroutine(BlinkRoutine(rightSprite, true));
        }
        else
        {
            yield return StartCoroutine(BlinkRoutine(wrongSprite, false));
            currentBamboo = 0;
        }

        isProcessing = false;
    }

    IEnumerator BlinkRoutine(Sprite blinkSprite, bool isCorrect)
    {
        for (int i = 0; i < 2; i++)
        {
            sr.sprite = blinkSprite;
            yield return new WaitForSeconds(0.2f);
            sr.sprite = defaultSprite;
            yield return new WaitForSeconds(0.2f);
        }
        sr.sprite = isCorrect ? blinkSprite : defaultSprite;
    }

    public void ResetBowl()
    {
        StopAllCoroutines();
        currentBamboo = 0;
        isSolved = false;
        isProcessing = false;
        if (sr != null && defaultSprite != null) sr.sprite = defaultSprite;
    }
}