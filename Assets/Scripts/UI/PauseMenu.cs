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
    public string mainMenuSceneName = "MainMenu"; // <-- Giờ nó sẽ hiện trong Inspector!

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
        pauseMenuUI.SetActive(false);
        settingsUI.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;

        // Sử dụng tên Scene mà bạn đã nhập trong Inspector
        SceneManager.LoadScene(mainMenuSceneName);
    }
}