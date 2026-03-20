using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class SceneAudioData
{
    public string sceneName;       // Tên Scene
    public AudioClip walkSound;     // Tiếng bước chân
    public AudioClip ambienceSound; // Tiếng môi trường (gió, chim hót...)
}

public class SceneSoundManager : MonoBehaviour
{
    public static SceneSoundManager Instance;
    public List<SceneAudioData> sceneSounds;

    private AudioSource ambienceSource; // Loa tàng hình phát tiếng môi trường

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Tự tạo loa phát Ambience
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.loop = true;
            ambienceSource.playOnAwake = false;
        }
        else { Destroy(gameObject); }
    }

    void OnEnable()
    {
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
                if (data.ambienceSound != null && ambienceSource.clip != data.ambienceSound)
                {
                    ambienceSource.clip = data.ambienceSound;
                    ambienceSource.volume = 0.4f; // Âm lượng môi trường
                    ambienceSource.Play();
                }
                else if (data.ambienceSound == null)
                {
                    ambienceSource.Stop();
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