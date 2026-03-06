using UnityEngine;
using System.Collections;

public class SceneBackgroundMusic : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Nguồn phát âm thanh (Audio Source) cho nhạc nền")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("File nhạc nền (Audio Clip) muốn phát")]
    [SerializeField] private AudioClip backgroundMusicClip;
    
    [Tooltip("Âm lượng nhạc nền (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;

    void Start()
    {
        // Tự động lấy AudioSource nếu chưa được gán
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            // Nếu GameObject chưa có AudioSource, tự động thêm vào
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Thiết lập và phát nhạc nền
        if (audioSource != null && backgroundMusicClip != null)
        {
            audioSource.clip = backgroundMusicClip;
            audioSource.volume = volume;
            audioSource.loop = true; // Nhạc nền thường lặp lại
            audioSource.Play();
        }
        else if (audioSource != null && audioSource.clip != null)
        {
            // Trường hợp AudioSource đã có sẵn clip nhưng chưa gán ở backgroundMusicClip
            audioSource.volume = volume;
            audioSource.loop = true;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    /// <summary>
    /// Thay đổi volume nhạc nền từ script khác nếu cần (ví dụ: khi vào menu, hay khi boss xuất hiện)
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Giảm dần âm lượng nhạc nền cho đến khi tắt hẳn
    /// </summary>
    /// <param name="duration">Thời gian tắt dần (giây)</param>
    public void FadeOut(float duration)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutCoroutine(duration));
        }
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        
        // Trả lại volume cũ để nếu play lại sẽ không bị tịt
        audioSource.volume = startVolume; 
    }
}
