using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MusicTriggerZone : MonoBehaviour
{
    [Header("Cài đặt Chức năng")]
    [Tooltip("Tích vào ô này nếu bạn muốn cục này dùng để TẮT nhạc")]
    public bool isStopTrigger = false;

    [Tooltip("Kéo file nhạc vào đây (Chỉ cần thiết nếu KHÔNG TÍCH ô Stop ở trên)")]
    public AudioClip musicClip;

    [Range(0f, 1f)]
    public float volume = 0.5f;

    // Cái loa dùng chung cho tất cả các vùng nhạc
    private static AudioSource sharedMusicSource;

    void Start()
    {
        // Tự động tạo 1 cái loa tàng hình nếu hệ thống chưa có
        if (sharedMusicSource == null)
        {
            GameObject audioObj = new GameObject("ZoneMusicPlayer");
            sharedMusicSource = audioObj.AddComponent<AudioSource>();
            sharedMusicSource.loop = true; // Chơi nhạc vòng lặp (Loop)
            DontDestroyOnLoad(audioObj);   // Sang Map khác cũng không bị mất loa
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chỉ kích hoạt khi người chạm vào là Anh Khoai (Player)
        if (collision.CompareTag("Player"))
        {
            if (isStopTrigger)
            {
                // Hành động: TẮT NHẠC
                if (sharedMusicSource.isPlaying)
                {
                    sharedMusicSource.Stop();
                }
            }
            else
            {
                // Hành động: BẬT NHẠC
                if (musicClip != null)
                {
                    // Nếu đang phát đúng bài này rồi thì không phát lại từ đầu nữa
                    if (sharedMusicSource.clip == musicClip && sharedMusicSource.isPlaying) return;

                    sharedMusicSource.clip = musicClip;
                    sharedMusicSource.volume = volume;
                    sharedMusicSource.Play();
                }
            }
        }
    }
    public static void StopMusic()
    {
        if (sharedMusicSource != null && sharedMusicSource.isPlaying)
        {
            sharedMusicSource.Stop();
        }
    }
}