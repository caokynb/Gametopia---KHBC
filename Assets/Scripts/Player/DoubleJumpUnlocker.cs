using UnityEngine;
using System.Collections;

public class DoubleJumpUnlocker : MonoBehaviour
{
    [Header("Giao diện UI")]
    public GameObject unlockUIPanel;

    [Header("Âm thanh (SFX)")]
    public AudioClip unlockSound;

    void Start()
    {
        // Kiểm tra xem ổ cứng đã lưu việc ăn bí kíp này chưa. 1 là rồi, 0 là chưa.
        if (PlayerPrefs.GetInt("HasDoubleJump", 0) == 1)
        {
            Destroy(gameObject); // Đã ăn rồi thì xóa luôn cục bí kíp, không cho ăn lại
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();

            if (player != null)
            {
                PlayerMovement.canJumpOnBamboo = true;

                // --- LƯU CHẾT VÀO Ổ CỨNG ---
                PlayerPrefs.SetInt("HasDoubleJump", 1);
                PlayerPrefs.Save();

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

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));

        if (unlockUIPanel != null) unlockUIPanel.SetActive(false);
        Time.timeScale = 1f;
        Destroy(gameObject);
    }
}