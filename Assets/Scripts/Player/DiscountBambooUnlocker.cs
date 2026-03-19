using UnityEngine;
using System.Collections;

public class DiscountBambooUnlocker : MonoBehaviour
{
    [Header("Giao diện UI (Giống Double Jump)")]
    public GameObject unlockUIPanel;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();

            if (player != null)
            {
                // 1. MỞ KHÓA BUFF VĨNH VIỄN BẰNG BIẾN STATIC
                PlayerMovement.hasDiscountBuff = true;
                Debug.Log("<color=lime>ĐÃ MỞ KHÓA:</color> Buff Giảm 50% Tiêu Hao Tre Vĩnh Viễn!");

                // 2. Hiện bảng thông báo
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