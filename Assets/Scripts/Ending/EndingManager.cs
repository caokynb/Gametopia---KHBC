using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // QUAN TRỌNG: Thêm thư viện này để load Scene

public class EndingManager : MonoBehaviour
{
    public static EndingManager instance;

    [Header("Hệ Thống Tắt/Bật Đèn")]
    public SuperSceneFader fader;
    public float waitBeforeFade = 0.5f; // Chờ nửa giây cho mượt trước khi làm đen màn hình

    [Header("Giao Diện (UI Panels)")]
    public GameObject panelKhacNhap;
    public GameObject panelKhacXuat;
    public GameObject panelCredit;
    public float textDisplayTime = 3.5f; // Thời gian hiện chữ để người chơi kịp đọc

    [Header("Diễn Viên (GameObjects)")]
    public GameObject oldNPCs; // Phú Ông & Cậu Cả đứng dưới đất
    public GameObject bambooNPCs; // Phú Ông & Cậu Cả bị dính trên tre

    [Header("Chuyển Cảnh Cuối")]
    public float creditDisplayTime = 5f; // Thời gian hiện bảng Credit (chỉnh tùy ý)
    public string mainMenuSceneName = "MainMenu"; // Tên Scene Menu Chính của bạn

    void Awake()
    {
        // Đảm bảo chỉ có 1 Đạo diễn trong Scene
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Khi mới vào game, giấu hết các bảng chữ và giấu luôn cảnh dính trên tre
        if (panelKhacNhap != null) panelKhacNhap.SetActive(false);
        if (panelKhacXuat != null) panelKhacXuat.SetActive(false);
        if (panelCredit != null) panelCredit.SetActive(false);
        if (bambooNPCs != null) bambooNPCs.SetActive(false);
    }

    // ==========================================
    // CẢNH 1: KHẮC NHẬP (Gọi sau khi chửi xong Phú Ông)
    // ==========================================
    public void PlayEndingPart1()
    {
        StartCoroutine(RoutinePart1());
    }

    private IEnumerator RoutinePart1()
    {
        yield return new WaitForSeconds(waitBeforeFade);

        // 1. Kéo rèm đen
        fader.FadeOut();
        yield return new WaitForSeconds(1.5f); // Chờ 1.5s cho màn hình đen hoàn toàn

        // 2. Hiện chữ Khắc Nhập! Khắc Nhập!
        if (panelKhacNhap != null) panelKhacNhap.SetActive(true);
        yield return new WaitForSeconds(textDisplayTime); // Dừng lại cho người chơi đọc
        if (panelKhacNhap != null) panelKhacNhap.SetActive(false); // Tắt chữ đi

        // 3. Tráo đổi diễn viên (Xóa ông cũ, hiện ông dính tre)
        if (oldNPCs != null) oldNPCs.SetActive(false);
        if (bambooNPCs != null) bambooNPCs.SetActive(true);

        // 4. Mở rèm sáng lên để thấy kết quả
        fader.FadeIn();
    }

    // ==========================================
    // CẢNH 2: KHẮC XUẤT & CREDIT (Gọi sau khi Phú Ông xin lỗi)
    // ==========================================
    public void PlayEndingPart2()
    {
        StartCoroutine(RoutinePart2());
    }

    private IEnumerator RoutinePart2()
    {
        yield return new WaitForSeconds(waitBeforeFade);

        // 1. Kéo rèm đen lần cuối
        fader.FadeOut();
        yield return new WaitForSeconds(1.5f);

        AudioSource[] allAudioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audio in allAudioSources)
        {
            if (audio != null && audio.isPlaying)
            {
                audio.Stop();
            }
        }

        // 2. Hiện chữ Khắc Xuất + Anh Khoai lấy chị Mùi
        if (panelKhacXuat != null) panelKhacXuat.SetActive(true);
        yield return new WaitForSeconds(textDisplayTime);
        if (panelKhacXuat != null) panelKhacXuat.SetActive(false);

        // 3. Bật Credit lên (Vẫn giữ màn hình đen làm nền cho Credit)
        if (panelCredit != null) panelCredit.SetActive(true);

        // 4. CHỜ NGƯỜI CHƠI XEM CREDIT
        yield return new WaitForSeconds(creditDisplayTime);

        // Optional: Xóa file save để người chơi phải chơi lại từ đầu khi New Game
        // PlayerPrefs.DeleteAll(); 

        // 5. QUAY VỀ MAIN MENU
        SceneManager.LoadScene(mainMenuSceneName);
    }
}