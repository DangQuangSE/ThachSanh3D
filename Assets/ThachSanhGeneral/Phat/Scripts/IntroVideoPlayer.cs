using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.InputSystem;

public class IntroVideoPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("The VideoPlayer component playing the intro video. If null, will try to find one on this GameObject.")]
    public VideoPlayer videoPlayer;

    [Header("Scene Transition Settings")]
    [Tooltip("The name of the scene to load after the video finishes (e.g., 'SnakeBossMap').")]
    public string nextSceneName = "SnakeBossMap";
    
    [Tooltip("Allow the user to skip the video by pressing a key?")]
    public bool allowSkip = true;

    private bool isTransitioning = false;

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
        }
        else
        {
            Debug.LogError("IntroVideoPlayer: No VideoPlayer component found! Loading next scene immediately.", this);
            LoadNextScene();
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
        LoadNextScene();
    }

    public void SkipVideo()
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        Debug.Log("Intro video ended/skipped. Loading scene: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }
}
