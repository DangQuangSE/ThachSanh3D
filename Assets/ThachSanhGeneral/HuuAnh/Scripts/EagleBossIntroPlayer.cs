using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.InputSystem;

public class EagleBossIntroPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("The VideoPlayer component playing the intro video. If null, will try to find one on this GameObject.")]
    public VideoPlayer videoPlayer;

    [Header("Scene Transition Settings")]
    [Tooltip("The name of the scene to load after the video finishes (e.g., 'Map_EagleBoss').")]
    public string nextSceneName = "Map_EagleBoss";

    [Tooltip("Allow the user to skip the video by pressing a key?")]
    public bool allowSkip = true;

    private AsyncOperation asyncLoad;
    private bool isTransitioning = false; // Added to avoid missing identifier errors

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            // Subscribe to the loop point reached event to know when the video finishes
            videoPlayer.loopPointReached += OnVideoEnd;
            
            // Start preloading the scene immediately in the background
            StartCoroutine(PreloadSceneAsync());
        }
        else
        {
            Debug.LogError("EagleBossIntroPlayer: No VideoPlayer component found! Loading next scene immediately.", this);
            LoadNextScene(true);
        }
    }

    private IEnumerator PreloadSceneAsync()
    {
        // Start loading the scene asynchronously
        asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        
        // Prevent the scene from activating until we are ready
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads (it stops at 0.9 progress when allowSceneActivation is false)
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
    }

    void Update()
    {
        // Allow the player to skip the video if enabled
        bool skipPressed = false;

        if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            skipPressed = true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            skipPressed = true;
        }

        if (allowSkip && !isTransitioning && skipPressed)
        {
            SkipVideo();
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        LoadNextScene(false);
    }

    public void SkipVideo()
    {
        LoadNextScene(false);
    }

    private void LoadNextScene(bool immediate)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        Debug.Log("Intro video ended/skipped. Activating preloaded scene: " + nextSceneName);
        
        if (immediate || asyncLoad == null)
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Allow the preloaded scene to activate instantly
            asyncLoad.allowSceneActivation = true;
        }
    }
}
