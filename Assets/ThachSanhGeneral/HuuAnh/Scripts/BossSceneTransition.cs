using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea(2,5)]
    public string message;
    [Tooltip("Âm thanh cho dòng hội thoại này (tùy chọn)")]
    public AudioClip voiceClip;
}

/// <summary>
/// Scene transition after boss dies (7 second delay).
/// Attach this script to Boss GameObject (along with BossDaiBangController or BossController).
/// </summary>
public class BossSceneTransition : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("Target scene name to load after boss dies")]
    public string nextSceneName = "MainScene";

    [Tooltip("Delay before scene transition (seconds)")]
    public float delayBeforeTransition = 7f;

    [Header("Post-Boss Dialogue Settings")]
    [Tooltip("Play betrayal dialogue sequence before bridging to next scene")]
    public bool playBetrayalDialogue = true;

    [Tooltip("Danh sách các dòng hội thoại phản bội")]
    public List<DialogueLine> betrayalDialogue = new List<DialogueLine>();

    [Header("Dialogue Audio Settings")]
    [Tooltip("Âm lượng cho âm thanh hội thoại (0-1)")]
    [Range(0f, 1f)]
    public float dialogueVolume = 0.8f;
    
    [Tooltip("Âm thanh nền cho đoạn hội thoại phản bội (tùy chọn)")]
    public AudioClip betrayalBackgroundMusic;
    
    [Tooltip("Âm lượng cho background music (0-1)")]
    [Range(0f, 1f)]
    public float backgroundMusicVolume = 0.3f;
    
    [Tooltip("Phát âm thanh cho từng ký tự khi typewriter (tùy chọn)")]
    public AudioClip typewriterSound;
    
    [Tooltip("Âm lượng typewriter sound")]
    [Range(0f, 1f)]
    public float typewriterVolume = 0.15f;

    [Tooltip("Tốc độ gõ chữ (giây/ký tự)")]
    [Range(0.01f, 0.2f)]
    public float typewriterSpeed = 0.06f;

    [Header("References")]
    [Tooltip("Auto-find BossDaiBangController if not assigned")]
    public BossDaiBangController bossDaiBang;

    [Tooltip("Auto-find BossController if not assigned")]
    public BossController bossController;

    [Header("Optional UI Feedback")]
    [Tooltip("Countdown text display (optional)")]
    public UnityEngine.UI.Text countdownText;

    [Tooltip("Victory panel to show when boss dies (optional)")]
    public GameObject victoryPanel;

    [Header("Debug Settings")]
    [Tooltip("Enable detailed debug logs")]
    public bool enableDebugLogs = true;

    private bool _transitionStarted = false;
    private float _checkTimer = 0f;
    private const float CHECK_INTERVAL = 0.5f;
    
    // Audio sources
    private AudioSource _dialogueAudioSource;
    private AudioSource _backgroundMusicSource;
    private AudioSource _typewriterAudioSource;

    void Start()
    {
        DebugLog("=== BossSceneTransition Start ===");
        DebugLog($"GameObject name: {gameObject.name}");
        DebugLog($"Next Scene Name: '{nextSceneName}'");
        DebugLog($"Delay: {delayBeforeTransition}s");

        // Setup audio sources
        SetupAudioSources();

        // Auto-find boss controller if not assigned
        if (bossDaiBang == null)
        {
            bossDaiBang = GetComponent<BossDaiBangController>();
            DebugLog($"Auto-find BossDaiBangController: {(bossDaiBang != null ? "FOUND" : "NOT FOUND")}");
        }
        else
        {
            DebugLog($"BossDaiBangController: ALREADY ASSIGNED");
        }

        if (bossController == null)
        {
            bossController = GetComponent<BossController>();
            DebugLog($"Auto-find BossController: {(bossController != null ? "FOUND" : "NOT FOUND")}");
        }
        else
        {
            DebugLog($"BossController: ALREADY ASSIGNED");
        }

        // Check if no controller found
        if (bossDaiBang == null && bossController == null)
        {
            Debug.LogError("[BossSceneTransition] NO BOSS CONTROLLER FOUND! Script will not work.");
        }

        // Hide victory panel initially
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
            DebugLog("Victory Panel: HIDDEN");
        }
        else
        {
            DebugLog("Victory Panel: NOT ASSIGNED");
        }

        // Hide countdown text initially
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
            DebugLog("Countdown Text: HIDDEN");
        }
        else
        {
            DebugLog("Countdown Text: NOT ASSIGNED");
        }

        DebugLog("=== BossSceneTransition Start Complete ===\n");
    }

    private void SetupAudioSources()
    {
        // Dialogue AudioSource
        _dialogueAudioSource = gameObject.AddComponent<AudioSource>();
        _dialogueAudioSource.playOnAwake = false;
        _dialogueAudioSource.volume = dialogueVolume;
        _dialogueAudioSource.spatialBlend = 0f; // 2D sound
        
        // Background Music AudioSource
        _backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        _backgroundMusicSource.playOnAwake = false;
        _backgroundMusicSource.loop = true;
        _backgroundMusicSource.volume = backgroundMusicVolume;
        _backgroundMusicSource.spatialBlend = 0f; // 2D sound
        
        // Typewriter Sound AudioSource
        _typewriterAudioSource = gameObject.AddComponent<AudioSource>();
        _typewriterAudioSource.playOnAwake = false;
        _typewriterAudioSource.volume = typewriterVolume;
        _typewriterAudioSource.spatialBlend = 0f; // 2D sound
        
        DebugLog("Audio sources setup complete");
    }

    void Update()
    {
        // Check if boss is dead
        if (_transitionStarted)
            return;

        // Periodic check for boss status
        _checkTimer += Time.deltaTime;
        if (_checkTimer >= CHECK_INTERVAL)
        {
            _checkTimer = 0f;
            CheckBossStatus();
        }
    }

    private void CheckBossStatus()
    {
        bool isDead = false;
        string bossType = "UNKNOWN";

        // Check BossDaiBangController
        if (bossDaiBang != null)
        {
            bool daiBangDead = bossDaiBang.IsDead();
            DebugLog($"[Check] BossDaiBang.IsDead() = {daiBangDead}");
            
            if (daiBangDead)
            {
                isDead = true;
                bossType = "DaiBang";
            }
        }

        // Check BossController (Bach)
        if (bossController != null)
        {
            bool bossControllerDead = bossController.IsDead();
            DebugLog($"[Check] BossController.IsDead() = {bossControllerDead}");
            
            if (bossControllerDead)
            {
                isDead = true;
                bossType = "BossController";
            }
        }

        // If boss is dead, start countdown
        if (isDead)
        {
            Debug.Log($"<color=red>═════════════════════════════════════════</color>");
            Debug.Log($"<color=red>║  BOSS DIED! ({bossType})              ║</color>");
            Debug.Log($"<color=red>║  playBetrayalDialogue = {playBetrayalDialogue}  ║</color>");
            Debug.Log($"<color=red>║  Starting scene transition...        ║</color>");
            Debug.Log($"<color=red>═════════════════════════════════════════</color>");
            
            _transitionStarted = true;
            
            // Create a persistent GameObject to run the transition
            // so it won't be destroyed when Boss dies
            GameObject transitionRunner = new GameObject("BossTransitionRunner");
            DontDestroyOnLoad(transitionRunner);
            
            // Transfer the coroutine to the new GameObject
            var runner = transitionRunner.AddComponent<CoroutineRunner>();
            runner.RunCoroutine(TransitionToNextScene());
        }
    }
    
    // Helper class to run coroutines on a persistent GameObject
    private class CoroutineRunner : MonoBehaviour
    {
        public void RunCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(RunAndDestroy(coroutine));
        }
        
        private IEnumerator RunAndDestroy(IEnumerator coroutine)
        {
            yield return StartCoroutine(coroutine);
            Destroy(gameObject);
        }
    }

    private IEnumerator TransitionToNextScene()
    {
        DebugLog("=== BEGIN SCENE TRANSITION COROUTINE ===");
        DebugLog($"Target Scene: '{nextSceneName}'");
        DebugLog($"Delay: {delayBeforeTransition}s");

        // Show victory panel if assigned
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            DebugLog("Victory Panel: SHOWN");
        }

        if (playBetrayalDialogue && betrayalDialogue != null && betrayalDialogue.Count > 0)
        {
            // Lock cursor during dialogue
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            DisablePlayerInput();
            yield return StartCoroutine(PlayDialogueSequence());
            ResetPlayerInput();
        }
        else
        {
            // Show countdown text if assigned
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                DebugLog("Countdown Text: SHOWN");
            }

            // Countdown
            float remainingTime = delayBeforeTransition;
            DebugLog($"Starting countdown from {remainingTime}s...");
            
            while (remainingTime > 0)
            {
                // Update countdown text
                if (countdownText != null)
                {
                    countdownText.text = $"Scene transition in: {Mathf.CeilToInt(remainingTime)}s";
                }

                Debug.Log($"<color=yellow>[Countdown] {Mathf.CeilToInt(remainingTime)}s remaining...</color>");

                yield return new WaitForSeconds(1f);
                remainingTime -= 1f;
            }

            DebugLog("Countdown complete!");
        }

        // Ensure cursor is visible and unlocked before scene transition
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Setup time scale back to normal before loading next scene
        Time.timeScale = 1f;
        DebugLog("Cursor unlocked, timeScale reset to 1");

        // Check if scene name is empty
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("<color=red>[ERROR] Next Scene Name is EMPTY! Cannot load scene.</color>");
            yield break;
        }

        // Check if scene exists in Build Settings
        int sceneIndex = SceneManager.GetSceneByName(nextSceneName).buildIndex;
        if (sceneIndex == -1)
        {
            Debug.LogWarning($"<color=orange>[WARNING] Scene '{nextSceneName}' not found by name. Attempting to load anyway...</color>");
        }

        // Load scene
        Debug.Log($"<color=green>?????????????????????????????????????????</color>");
        Debug.Log($"<color=green>?  LOADING SCENE: {nextSceneName,-20} ?</color>");
        Debug.Log($"<color=green>?????????????????????????????????????????</color>");

        try
        {
            SceneManager.LoadScene(nextSceneName);
            DebugLog("SceneManager.LoadScene() called successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>[ERROR] Failed to load scene '{nextSceneName}': {e.Message}</color>");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-assign audio clips exactly as requested (1-7 matching spoken lines)
        if (Application.isPlaying) return;

        if (betrayalDialogue != null && betrayalDialogue.Count >= 9)
        {
            int audioIndex = 1;
            for (int i = 0; i < betrayalDialogue.Count; i++)
            {
                if (string.IsNullOrEmpty(betrayalDialogue[i].speaker) || betrayalDialogue[i].message == "Còn tiếp")
                {
                    continue; // Skip narration or "Còn tiếp"
                }

                if (audioIndex <= 7)
                {
                    string path = $"Assets/ThachSanhGeneral/HuuAnh/Sounds/{audioIndex}.mp3";
                    var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    // Also try wav if mp3 not found
                    if (clip == null)
                    {
                        path = $"Assets/ThachSanhGeneral/HuuAnh/Sounds/{audioIndex}.wav";
                        clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    }
                    
                    if (clip != null)
                    {
                        betrayalDialogue[i].voiceClip = clip;
                    }
                    audioIndex++;
                }
            }
        }
    }
#endif

    private IEnumerator PlayDialogueSequence()
    {
        // Add a short delay after boss dies before dialogue starts
        yield return new WaitForSeconds(2f);

        // FREEZE THE GAMEPLAY
        Time.timeScale = 0f;

        // Hide victory panel if it's there so dialogue is clean
        if (victoryPanel != null) victoryPanel.SetActive(false);

        // Play background music for betrayal scene
        PlayBackgroundMusic(betrayalBackgroundMusic);

        // Dynamically build UI Canvas
        GameObject canvasObj = new GameObject("BetrayalDialogueCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f; // Ensures proportions are kept better across aspects
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // FULL SCREEN DIMMED BACKGROUND (Optional, but helps focus)
        GameObject bgObj = new GameObject("DimBackground");
        bgObj.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image bgImg = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0, 0, 0, 0.4f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // WHITE PANEL -> Fullscreen background
        GameObject whitePanelObj = new GameObject("WhitePanel");
        whitePanelObj.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image whiteImg = whitePanelObj.AddComponent<UnityEngine.UI.Image>();
        whiteImg.color = new Color(0.05f, 0.05f, 0.08f, 1f); // Dark elegant fully-opaque background
        
        RectTransform whiteRect = whitePanelObj.GetComponent<RectTransform>();
        whiteRect.anchorMin = Vector2.zero;
        whiteRect.anchorMax = Vector2.one;
        whiteRect.sizeDelta = Vector2.zero;
        whiteRect.anchoredPosition = Vector2.zero;

        // Try getting a font mapping to standard fonts
        Font uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null) uiFont = Resources.Load<Font>("Arial");
        if (uiFont == null) uiFont = Font.CreateDynamicFontFromOSFont("Arial", 45);

        // CREATE SPEAKER NAME TEXT
        GameObject nameObj = new GameObject("SpeakerNameText");
        nameObj.transform.SetParent(whitePanelObj.transform, false);
        UnityEngine.UI.Text nameText = nameObj.AddComponent<UnityEngine.UI.Text>();
        nameText.font = uiFont;
        nameText.fontSize = 75;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleCenter;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.7f);
        nameRect.anchorMax = new Vector2(1f, 0.9f); // Top part of the screen
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        // CREATE MESSAGE TEXT
        GameObject msgObj = new GameObject("MessageText");
        msgObj.transform.SetParent(whitePanelObj.transform, false);
        UnityEngine.UI.Text msgText = msgObj.AddComponent<UnityEngine.UI.Text>();
        msgText.font = uiFont;
        msgText.fontSize = 60;
        msgText.lineSpacing = 1.3f;
        msgText.color = new Color(0.9f, 0.9f, 0.9f, 1f); 
        msgText.alignment = TextAnchor.MiddleCenter;
        msgText.horizontalOverflow = HorizontalWrapMode.Wrap;
        msgText.verticalOverflow = VerticalWrapMode.Truncate;
        msgText.supportRichText = true;
        
        // Add subtle drop shadow to text for readability
        UnityEngine.UI.Shadow textShadow = msgObj.AddComponent<UnityEngine.UI.Shadow>();
        textShadow.effectColor = Color.black;
        textShadow.effectDistance = new Vector2(2, -2);
        
        RectTransform msgRect = msgObj.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.1f, 0.2f);
        msgRect.anchorMax = new Vector2(0.9f, 0.7f); // Center part of the screen
        msgRect.offsetMin = Vector2.zero; 
        msgRect.offsetMax = Vector2.zero;

        // CREATE "TIẾP TỤC" PROMPT (Simple blinking or subtle text at bottom right instead of a clunky box)
        GameObject btnObj = new GameObject("ContinuePrompt");
        btnObj.transform.SetParent(whitePanelObj.transform, false);
        UnityEngine.UI.Text btnTextRef = btnObj.AddComponent<UnityEngine.UI.Text>();
        btnTextRef.font = uiFont;
        btnTextRef.fontSize = 40;
        btnTextRef.fontStyle = FontStyle.Italic;
        btnTextRef.color = new Color(1f, 1f, 1f, 0.6f);
        btnTextRef.alignment = TextAnchor.LowerRight;
        btnTextRef.text = "Nhấn [Chuột trái] hoặc [Space] ⯈";
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.7f, 0.05f);
        btnRect.anchorMax = new Vector2(0.95f, 0.15f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        // Toggles visibility of Continue prompt
        btnObj.SetActive(false);

        foreach (var line in betrayalDialogue)
        {
            btnObj.SetActive(false); // hide while typing
            bool isStory = string.IsNullOrEmpty(line.speaker);
            
            if (isStory) 
            {
                nameText.text = "";
                msgText.fontStyle = FontStyle.Italic;
                msgText.alignment = TextAnchor.MiddleCenter;
                
                if (line.message == "Còn tiếp")
                {
                    // Special styling for "Còn tiếp"
                    msgText.alignment = TextAnchor.MiddleCenter;
                    msgText.fontSize = 85;
                    msgText.fontStyle = FontStyle.Bold;
                    msgText.color = new Color(0.9f, 0.3f, 0.3f); // Reddish for "To be continued"
                }
                else
                {
                    msgText.fontSize = 60;
                    msgText.color = new Color(0.7f, 0.8f, 0.9f);
                }
            }
            else
            {
                // Format speaker name
                string colorHex = line.speaker == "Thạch Sanh" ? "#4D94FF" : "#FF4D4D"; // Bright Blue vs Bright Red for dark background
                nameText.text = $"<color={colorHex}>{line.speaker}</color>";
                
                msgText.fontStyle = FontStyle.Normal;
                msgText.alignment = TextAnchor.MiddleCenter;
                msgText.fontSize = 60;
                msgText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            }

            msgText.text = "";

            // Play voice audio for this line (if assigned)
            PlayLineAudio(line.voiceClip);

            // Typewriter effect
            for (int i = 0; i < line.message.Length; i++)
            {
                if (CheckSkipInput())
                {
                    // For skipping
                    msgText.text = line.message;
                    yield return null;
                    break;
                }
                
                msgText.text += line.message[i];
                
                // Play typewriter sound effect
                PlayTypewriterSound();
                
                yield return new WaitForSecondsRealtime(typewriterSpeed);
            }

            // Show "Tiếp tục" button after typing finishes
            btnObj.SetActive(true);

            // Nếu là dòng "Còn tiếp" thì đợi click để chuyển scene
            if (line.message == "Còn tiếp")
            {
                btnTextRef.text = "Chuyển Scene";
                while (true)
                {
                    if (CheckSkipInput()) break;
                    yield return null;
                }
                break;
            }

            // Chờ tương tác người dùng
            yield return new WaitForSecondsRealtime(0.2f);
            float waitTime = Mathf.Max(2f, line.message.Length * 0.05f);
            float elapsed = 0f;
            while (elapsed < waitTime)
            {
                elapsed += Time.unscaledDeltaTime;
                if (CheckSkipInput())
                {
                    StopLineAudio();
                    break;
                }
                
                yield return null;
            }
            
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // Stop all audio when dialogue ends
        StopBackgroundMusic();
        StopLineAudio();

        // Clean up UI
        if (canvasObj != null) Destroy(canvasObj);
    }

    /// <summary>
    /// Play background music for the dialogue scene.
    /// </summary>
    private void PlayBackgroundMusic(AudioClip clip)
    {
        if (clip == null || _backgroundMusicSource == null) return;

        _backgroundMusicSource.clip = clip;
        _backgroundMusicSource.Play();
        DebugLog("Background music started");
    }

    /// <summary>
    /// Stop background music.
    /// </summary>
    private void StopBackgroundMusic()
    {
        if (_backgroundMusicSource != null && _backgroundMusicSource.isPlaying)
        {
            _backgroundMusicSource.Stop();
            DebugLog("Background music stopped");
        }
    }

    /// <summary>
    /// Play voice audio for a dialogue line.
    /// </summary>
    private void PlayLineAudio(AudioClip clip)
    {
        if (_dialogueAudioSource == null) return;
        
        // Stop previous audio before playing new one
        if (_dialogueAudioSource.isPlaying)
        {
            _dialogueAudioSource.Stop();
        }
        
        if (clip != null)
        {
            _dialogueAudioSource.clip = clip;
            _dialogueAudioSource.Play();
            DebugLog($"Playing line audio: {clip.name}");
        }
    }

    /// <summary>
    /// Stop currently playing line audio.
    /// </summary>
    private void StopLineAudio()
    {
        if (_dialogueAudioSource != null && _dialogueAudioSource.isPlaying)
        {
            _dialogueAudioSource.Stop();
            DebugLog("Line audio stopped");
        }
    }

    /// <summary>
    /// Play typewriter sound effect (per character).
    /// </summary>
    private void PlayTypewriterSound()
    {
        if (typewriterSound != null && _typewriterAudioSource != null)
        {
            _typewriterAudioSource.PlayOneShot(typewriterSound);
        }
    }

    /// <summary>
    /// Public method to trigger scene transition manually (if needed).
    /// </summary>
    [ContextMenu("Test Hội Thoại Bằng Tay")]
    public void TriggerSceneTransition()
    {
        DebugLog("TriggerSceneTransition() called manually");
        
        if (!_transitionStarted)
        {
            _transitionStarted = true;
            StartCoroutine(TransitionToNextScene());
        }
        else
        {
            DebugLog("Transition already started, ignoring manual trigger");
        }
    }

    /// <summary>
    /// Load scene immediately (no delay).
    /// </summary>
    public void TransitionImmediately()
    {
        DebugLog("TransitionImmediately() called");
        
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"<color=cyan>[IMMEDIATE] Loading scene: {nextSceneName}</color>");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("[ERROR] Cannot load scene - nextSceneName is empty!");
        }
    }

    /// <summary>
    /// Check if player wants to skip dialogue (Space, Enter, Mouse Click, or Gamepad Button)
    /// </summary>
    private bool CheckSkipInput()
    {
#if ENABLE_INPUT_SYSTEM
        // New Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || 
                Keyboard.current.enterKey.wasPressedThisFrame)
                return true;
        }
        
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;
        
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            return true;
#else
        // Legacy Input System
        if (Input.GetKeyDown(KeyCode.Space) || 
            Input.GetKeyDown(KeyCode.Return) || 
            Input.GetKeyDown(KeyCode.KeypadEnter))
            return true;
        
        if (Input.GetMouseButtonDown(0))
            return true;
#endif
        
        return false;
    }

    /// <summary>
    /// Helper for togglable debug logs
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[BossSceneTransition] {message}");
        }
    }

    private void OnEnable()
    {
    }

    private void DisablePlayerInput()
    {
        // Try finding player and disabling the script that causes attack
        MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>();
        foreach(var script in scripts)
        {
            if (script.GetType().Name == "PlayerAttack" || script.GetType().Name == "PlayerInput" || script.GetType().Name.Contains("ThirdPersonUserControl"))
            {
                script.enabled = false;
                DebugLog($"Disabled Player script: {script.GetType().Name} during dialogue.");
            }
        }
    }

    private void ResetPlayerInput()
    {
        MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>(true);
        foreach(var script in scripts)
        {
            if (script.GetType().Name == "PlayerAttack" || script.GetType().Name == "PlayerInput" || script.GetType().Name.Contains("ThirdPersonUserControl"))
            {
                script.enabled = true;
            }
        }
    }

    // Detect when GameObject is destroyed
    private void OnDestroy()
    {
        // Chỉ warning nếu transition chưa được chuyển sang CoroutineRunner
        // Sau khi đã tạo CoroutineRunner thì việc Boss bị destroy là bình thường
        if (_transitionStarted)
        {
            // Transition đã được chuyển sang CoroutineRunner, không cần warning
            DebugLog("BossSceneTransition GameObject destroyed (transition running on CoroutineRunner)");
        }
        else
        {
            DebugLog("BossSceneTransition GameObject destroyed (transition not started)");
        }
    }
}
