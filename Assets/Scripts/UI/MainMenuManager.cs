using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Cấu hình UI")]
    public GameObject continueButton;
    public GameObject settingsPanel;

    [Header("Cấu hình Scene")]
    [Tooltip("Tên chính xác của Scene Level 1 (VD: Map1)")]
    public string firstLevelName = "Map1";

    [Header("Âm thanh UI")]
    public AudioClip clickSound; // Kéo file âm thanh vào đây trong Inspector

    void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (PlayerPrefs.HasKey("SavedLevel"))
        {
            continueButton.SetActive(true);
        }
        else
        {
            continueButton.SetActive(false);
        }
    }

    // Tự phát âm thanh độc lập, không cần gọi ai khác
    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            // Phát tại vị trí Camera để nghe rõ nhất
            Vector3 playPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(clickSound, playPos);
        }
    }

    public void OnNewGameClicked()
    {
        PlayClickSound();
        PlayerPrefs.DeleteAll();
        PlayerMovement.hasCheckpoint = false;
        SceneManager.LoadScene(firstLevelName);
    }

    public void OnContinueClicked()
    {
        PlayClickSound();
        string levelToLoad = PlayerPrefs.GetString("SavedLevel", firstLevelName);
        SceneManager.LoadScene(levelToLoad);
    }

    public void OnSettingsClicked()
    {
        PlayClickSound();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnCloseSettingsClicked()
    {
        PlayClickSound();
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OnExitClicked()
    {
        PlayClickSound();
        Debug.Log("Đang thoát game...");
        Application.Quit();
    }
}