using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Cần thiết nếu bạn muốn tương tác với UI

public class MainMenuManager : MonoBehaviour
{
    [Header("Cấu hình UI")]
    public GameObject continueButton;
    public GameObject settingsPanel;

    [Header("Cấu hình Scene")]
    [Tooltip("Tên chính xác của Scene Level 1 (VD: Map1)")]
    public string firstLevelName = "Map1";

    void Start()
    {
        // 1. Đảm bảo Settings Panel bị ẩn khi mới mở game
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 2. Kiểm tra xem có file save không để hiện nút Continue
        // (Chúng ta sẽ lưu 1 biến "SavedLevel" khi người chơi chạm vào Checkpoint)
        if (PlayerPrefs.HasKey("SavedLevel"))
        {
            continueButton.SetActive(true);
        }
        else
        {
            continueButton.SetActive(false);
        }
    }

    // --- CÁC HÀM GẮN VÀO NÚT BẤM ---

    public void OnNewGameClicked()
    {
        // Xóa dữ liệu cũ (reset máu, tre, vị trí)
        PlayerPrefs.DeleteAll();

        // Đặt lại các chỉ số mặc định nếu cần
        PlayerMovement.hasCheckpoint = false;

        // Tải cảnh đầu tiên
        SceneManager.LoadScene(firstLevelName);
    }

    public void OnContinueClicked()
    {
        // Lấy tên Scene đã lưu, nếu không có thì mặc định load Level 1
        string levelToLoad = PlayerPrefs.GetString("SavedLevel", firstLevelName);
        SceneManager.LoadScene(levelToLoad);
    }

    public void OnSettingsClicked()
    {
        // Bật bảng Settings
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnCloseSettingsClicked()
    {
        // Tắt bảng Settings
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OnExitClicked()
    {
        Debug.Log("Đang thoát game... (Sẽ chỉ hoạt động khi build ra file .exe)");
        Application.Quit();
    }
}