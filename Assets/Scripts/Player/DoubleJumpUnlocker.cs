using UnityEngine;
using System.Collections;

public class DoubleJumpUnlocker : MonoBehaviour
{
    [Header("Giao diện UI")]
    public GameObject unlockUIPanel;

    [Header("Âm thanh (SFX)")]
    [Tooltip("Tiếng nhạc hào hùng khi nhặt được bí kíp")]
    public AudioClip unlockSound; // THÊM BIẾN NÀY

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();

            if (player != null)
            {
                PlayerMovement.canJumpOnBamboo = true;

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