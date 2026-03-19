using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class SceneAudioData
{
    public string sceneName;    // Tên Scene trong Unity
    public AudioClip walkSound;  // Tiếng đi bộ của Scene đó
    public AudioClip ambienceSound; // [THÊM] Tiếng môi trường (Gió, mưa, chim hót...)
}

public class SceneSoundManager : MonoBehaviour
{
    public static SceneSoundManager Instance;
    public List<SceneAudioData> sceneSounds;

    // Loa chuyên biệt để phát tiếng Ambience lặp đi lặp lại
    private AudioSource ambienceSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Tự tạo một cái loa tàng hình chuyên phát tiếng nền
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.loop = true; // Luôn luôn lặp lại
            ambienceSource.playOnAwake = false;
        }
        else { Destroy(gameObject); }
    }

    void OnEnable()
    {
        // Đăng ký sự kiện: Mỗi khi Load Scene xong thì tự động đổi tiếng Ambience
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateAmbience();
    }

    public void UpdateAmbience()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        foreach (var data in sceneSounds)
        {
            if (data.sceneName == currentScene)
            {
                // Nếu Map này có tiếng Ambience và nó khác với tiếng đang phát
                if (data.ambienceSound != null && ambienceSource.clip != data.ambienceSound)
                {
                    ambienceSource.clip = data.ambienceSound;
                    ambienceSource.volume = 0.4f; // Âm lượng nền vừa phải
                    ambienceSource.Play();
                }
                else if (data.ambienceSound == null)
                {
                    ambienceSource.Stop(); // Nếu Map này không có Ambience thì tắt
                }
                return;
            }
        }
    }

    public AudioClip GetWalkSound()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        foreach (var data in sceneSounds)
        {
            if (data.sceneName == currentScene) return data.walkSound;
        }
        return null;
    }
}