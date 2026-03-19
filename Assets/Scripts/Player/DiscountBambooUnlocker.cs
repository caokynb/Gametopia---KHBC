using UnityEngine;
using System.Collections;

public class DiscountBambooUnlocker : MonoBehaviour
{
    [Header("Giao diện UI (Giống Double Jump)")]
    public GameObject unlockUIPanel;

    [Header("Âm thanh (SFX)")]
    [Tooltip("Tiếng nhạc khi nhặt được buff giảm tiêu hao")]
    public AudioClip unlockSound; // THÊM BIẾN NÀY

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();

            if (player != null)
            {
                PlayerMovement.hasDiscountBuff = true;

                // --- PHÁT ÂM THANH TẠI ĐÂY ---
                if (unlockSound != null)
                {
                    AudioSource.PlayClipAtPoint(unlockSound, transform.position);
                }

                StartCoroutine(ShowUnlockUI());
            }
        }
    }

    IEnumerator ShowUnlockUI()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        if (unlockUIPanel != null) unlockUIPanel.SetActive(true);

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);

        // Chờ phím F để đóng
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));

        if (unlockUIPanel != null) unlockUIPanel.SetActive(false);
        Time.timeScale = 1f;
        Destroy(gameObject);
    }
}