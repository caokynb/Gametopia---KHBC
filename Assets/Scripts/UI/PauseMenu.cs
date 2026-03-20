using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("Giao diện")]
    public GameObject pauseMenuUI;
    public GameObject settingsUI;

    [Header("Cấu hình Scene")]
    [Tooltip("Tên chính xác của Scene Main Menu")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Âm thanh UI")]
    public AudioClip clickSound; // Kéo file âm thanh vào đây trong Inspector

    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            Vector3 playPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            // Cho âm lượng nhỏ lại một chút (0.8f) để không bị chói tai
            AudioSource.PlayClipAtPoint(clickSound, playPos, 0.8f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PlayClickSound();

            if (settingsUI != null && settingsUI.activeSelf)
            {
                CloseSettings();
            }
            else if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) PlayClickSound();

        pauseMenuUI.SetActive(false);
        if (settingsUI != null) settingsUI.SetActive(false);

        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void OpenSettings()
    {
        PlayClickSound();
        pauseMenuUI.SetActive(false);
        settingsUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) PlayClickSound();

        settingsUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void LoadMainMenu()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}