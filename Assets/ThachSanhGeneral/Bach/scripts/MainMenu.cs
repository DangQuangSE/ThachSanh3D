using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Tên scene game chính c?n load (ph?i có trong Build Settings)")]
    public string gameSceneName = "PlaygroundB";

    [Header("Background Music")]
    [Tooltip("AudioClip nh?c n?n main menu")]
    public AudioClip menuMusic;

    [Tooltip("Âm l??ng nh?c n?n (0 - 1)")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    [Tooltip("Fade out nh?c khi chuy?n scene (giây)")]
    public float musicFadeOutDuration = 0.5f;

    [Header("UI References")]
    [Tooltip("Nút Play")]
    public Button playButton;

    [Tooltip("Nút Quit")]
    public Button quitButton;

    [Tooltip("Nút Settings (tu? ch?n)")]
    public Button settingsButton;

    [Tooltip("Panel Settings (tu? ch?n)")]
    public GameObject settingsPanel;

    [Tooltip("Loading screen object (tu? ch?n, ?n ?i khi ch?a load)")]
    public GameObject loadingScreen;

    [Tooltip("Loading progress bar (tu? ch?n)")]
    public Slider loadingBar;

    [Tooltip("Slider ch?nh âm l??ng nh?c (tu? ch?n)")]
    public Slider volumeSlider;

    [Tooltip("Nút b?t/t?t nh?c (tu? ch?n)")]
    public Button muteButton;

    [Tooltip("Icon loa khi ?ang b?t nh?c (tu? ch?n)")]
    public GameObject muteOffIcon;

    [Tooltip("Icon loa khi ?ang t?t nh?c (tu? ch?n)")]
    public GameObject muteOnIcon;

    private AudioSource _audioSource;
    private bool _isMuted = false;

    private void Awake()
    {
        // T?o AudioSource t? ??ng n?u ch?a có
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.clip = menuMusic;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.volume = musicVolume;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        // Phát nh?c n?n
        if (menuMusic != null)
            _audioSource.Play();

        // ?n loading screen khi m?i vào menu
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        // ?n settings panel khi m?i vào
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Gán s? ki?n nút
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ToggleSettings);

        if (muteButton != null)
            muteButton.onClick.AddListener(ToggleMute);

        // ??ng b? slider âm l??ng
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = musicVolume;
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        UpdateMuteIcons();
    }

    // ?? Nh?c n?n ???????????????????????????????????????????????????????????

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (!_isMuted)
            _audioSource.volume = musicVolume;
    }

    public void ToggleMute()
    {
        _isMuted = !_isMuted;
        _audioSource.volume = _isMuted ? 0f : musicVolume;
        UpdateMuteIcons();
    }

    private void UpdateMuteIcons()
    {
        if (muteOffIcon != null) muteOffIcon.SetActive(!_isMuted);
        if (muteOnIcon != null)  muteOnIcon.SetActive(_isMuted);
    }

    // ?? Ch?i game ??????????????????????????????????????????????????????????

    public void PlayGame()
    {
        // Reset ti?n trình boss khi b?t ??u game m?i t? Main Menu
        QuestDialogue.ResetProgress();

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            StartCoroutine(LoadSceneAsync(gameSceneName));
        }
        else
        {
            StartCoroutine(FadeOutAndLoad(gameSceneName));
        }
    }

    private System.Collections.IEnumerator FadeOutAndLoad(string sceneName)
    {
        // Fade out nh?c
        float startVolume = _audioSource.volume;
        float elapsed = 0f;
        while (elapsed < musicFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeOutDuration);
            yield return null;
        }
        _audioSource.Stop();
        SceneManager.LoadScene(sceneName);
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        // Fade out nh?c song song v?i loading
        StartCoroutine(FadeOutMusic());

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (loadingBar != null)
                loadingBar.value = progress;

            if (operation.progress >= 0.9f)
            {
                if (loadingBar != null)
                    loadingBar.value = 1f;

                yield return new WaitForSeconds(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private System.Collections.IEnumerator FadeOutMusic()
    {
        float startVolume = _audioSource.volume;
        float elapsed = 0f;
        while (elapsed < musicFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeOutDuration);
            yield return null;
        }
        _audioSource.Stop();
    }

    // ?? Settings ???????????????????????????????????????????????????????????

    public void ToggleSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // ?? Thoát game ?????????????????????????????????????????????????????????

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
