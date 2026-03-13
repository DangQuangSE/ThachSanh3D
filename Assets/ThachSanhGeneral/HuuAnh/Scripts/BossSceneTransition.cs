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
    [TextArea(2, 5)]
    public string message;
    [Tooltip("Âm thanh cho dòng hội thoại này (tùy chọn)")]
    public AudioClip voiceClip;
}

/// <summary>
/// Scene transition after boss dies.
/// Plays cinematic dialogue with avatars before switching scene.
/// Attach this script to Boss GameObject.
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

    [Header("Avatar Settings")]
    [Tooltip("Avatar sprite cho Thạch Sanh (bên trái)")]
    public Sprite thachSanhAvatarSprite;

    [Tooltip("Avatar sprite cho Lý Thông (bên phải)")]
    public Sprite lyThongAvatarSprite;

    [Header("Dialogue Audio Settings")]
    [Tooltip("Âm lượng cho âm thanh hội thoại (0-1)")]
    [Range(0f, 1f)]
    public float dialogueVolume = 0.8f;

    [Tooltip("Âm thanh nền cho đoạn hội thoại phản bội (tùy chọn)")]
    public AudioClip betrayalBackgroundMusic;

    [Tooltip("Âm lượng cho background music (0-1)")]
    [Range(0f, 1f)]
    public float backgroundMusicVolume = 0.3f;

    [Tooltip("Tốc độ gõ chữ (giây/ký tự)")]
    [Range(0.01f, 0.2f)]
    public float typewriterSpeed = 0.04f;

    [Tooltip("Thời gian chờ tự động chuyển sang câu tiếp theo")]
    public float autoSkipTime = 3f;

    [Tooltip("Thời gian delay tối thiểu để có thể ấn next sang câu khác (giây)")]
    public float skipDelay = 0.5f;

    [Header("References")]
    [Tooltip("Auto-find BossDaiBangController if not assigned")]
    public BossDaiBangController bossDaiBang;

    [Tooltip("Auto-find BossController if not assigned")]
    public BossController bossController;

    [Header("Optional UI Feedback")]
    [Tooltip("Victory panel to show when boss dies (optional)")]
    public GameObject victoryPanel;

    [Header("Debug Settings")]
    [Tooltip("Enable detailed debug logs")]
    public bool enableDebugLogs = true;

    private bool _transitionStarted = false;
    private float _checkTimer = 0f;
    private const float CHECK_INTERVAL = 0.5f;
    private bool _loggedEmptyLinesWarning = false;

    // Audio sources — moved to CoroutineRunner so they survive boss destruction
    private static AudioSource _dialogueAudioSource;
    private static AudioSource _backgroundMusicSource;

    // Static snapshots to survive Boss destruction
    private static string s_nextSceneName;
    private static bool s_playBetrayalDialogue;
    private static List<DialogueLine> s_betrayalDialogue;
    private static Sprite s_thachSanhAvatar;
    private static Sprite s_lyThongAvatar;
    private static AudioClip s_betrayalBGM;
    private static float s_typewriterSpeed;
    private static float s_autoSkipTime;
    private static float s_skipDelay;
    private static GameObject s_victoryPanel;

    // Coroutine host — the persistent CoroutineRunner, not the boss object
    private static MonoBehaviour _coroutineHost;

    // Audio snapshots
    private static float snapshot_dialogueVolume;
    private static float snapshot_bgmVolume;
    private static bool s_transitionInProgress = false; // Prevent multiple triggers/overwrites
    private GameObject _activeCanvas; // Track current dialogue canvas for cleanup
    private UnityEngine.UI.Text _continueText; // Track continue prompt

    // ── Dialogue state (QuestDialogue pattern) ────────────────────
    private static bool  _dlg_isTyping       = false;
    private static bool  _dlg_canContinue    = false;
    private static bool  _dlg_dialogueDone   = false;
    private static string _dlg_fullLine       = "";
    private static UnityEngine.UI.Text _dlg_msgText = null;
    private static Coroutine _dlg_typeCoroutine    = null;
    private static int   _dlg_currentIndex   = 0;
    // Callback invoked by OnContinuePressed to advance to next line
    private static System.Action _dlg_onAdvance = null;

    void Start()
    {
        DebugLog("=== BossSceneTransition Start ===");
        DebugLog($"Next Scene Name: '{nextSceneName}'");

        // NOTE: Audio sources are set up on CoroutineRunner when boss dies

        if (bossDaiBang == null)
            bossDaiBang = GetComponent<BossDaiBangController>();

        if (bossController == null)
            bossController = GetComponent<BossController>();

        if (bossDaiBang == null && bossController == null)
            Debug.LogError("[BossSceneTransition] NO BOSS CONTROLLER FOUND!");

        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    private void SetupAudioSources(GameObject host)
    {
        _dialogueAudioSource = host.AddComponent<AudioSource>();
        _dialogueAudioSource.playOnAwake = false;
        _dialogueAudioSource.volume = s_betrayalDialogue != null ? snapshot_dialogueVolume : 0.8f; // Use snapshotted volume
        _dialogueAudioSource.spatialBlend = 0f;

        _backgroundMusicSource = host.AddComponent<AudioSource>();
        _backgroundMusicSource.playOnAwake = false;
        _backgroundMusicSource.loop = true;
        _backgroundMusicSource.volume = s_betrayalDialogue != null ? snapshot_bgmVolume : 0.3f;
        _backgroundMusicSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (_transitionStarted) return;

        _checkTimer += Time.deltaTime;
        if (_checkTimer >= CHECK_INTERVAL)
        {
            _checkTimer = 0f;
            CheckBossStatus();
        }
    }

    private void CheckBossStatus()
    {
        if (s_transitionInProgress) return; // Global lock check (already someone doing it)

        bool isDead = false;
        if (bossDaiBang != null && bossDaiBang.IsDead()) isDead = true;
        if (bossController != null && bossController.IsDead()) isDead = true;

        if (isDead)
        {
            // CRITICAL CHECK: Ignore scripts that are set to play dialogue but have 0 lines configured.
            // This prevents duplicate/misconfigured scripts (like on 'GameManager') from hijacking the real Boss.
            if (playBetrayalDialogue && (betrayalDialogue == null || betrayalDialogue.Count == 0))
            {
                DebugLog($"'{gameObject.name}' is trying to trigger dialogue but HAS NO LINES! Ignoring it to let the real Boss script take control.");
                return; // Stop right here, don't lock anything.
            }

            // ACQUIRE LOCK: Only one valid script instance can proceed past this point permanently.
            if (s_transitionInProgress) return;
            s_transitionInProgress = true; 
            _transitionStarted = true;

            Debug.Log($"<color=red><b>[BossSceneTransition] BOSS DIED!</b> {gameObject.name} taking control of transition.</color>");

            // Snapshot data BEFORE the boss is destroyed
            s_nextSceneName = nextSceneName;
            s_playBetrayalDialogue = playBetrayalDialogue;
            
            s_betrayalDialogue = (betrayalDialogue != null) ? new List<DialogueLine>(betrayalDialogue) : new List<DialogueLine>();
            s_thachSanhAvatar = thachSanhAvatarSprite;
            s_lyThongAvatar = lyThongAvatarSprite;
            s_betrayalBGM = betrayalBackgroundMusic;
            s_typewriterSpeed = typewriterSpeed;
            s_autoSkipTime = autoSkipTime;
            s_skipDelay = skipDelay;
            s_victoryPanel = victoryPanel;
            snapshot_dialogueVolume = dialogueVolume;
            snapshot_bgmVolume = backgroundMusicVolume;

            // Create persistent runner — survives boss destruction
            GameObject runnerObj = new GameObject("BossTransitionRunner");
            DontDestroyOnLoad(runnerObj);
            var runner = runnerObj.AddComponent<CoroutineRunner>();

            // Set global host & move audio sources HERE (onto the persistent object)
            _coroutineHost = runner;
            SetupAudioSources(runnerObj);

            runner.RunCoroutine(TransitionToNextScene());
        }
    }

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

        // CoroutineRunner sống suốt cả dialogue (DontDestroyOnLoad).
        // Đọc input ở đây mỗi frame — đúng như QuestDialogue.Update().
        void Update()
        {
            bool pressed = false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            pressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                      (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame ||
                                                    Keyboard.current.enterKey.wasPressedThisFrame));
#else
            pressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
#endif
            if (pressed) OnContinuePressed();
        }
    }

    // ── Giống OnContinueClicked() của QuestDialogue ──────────────
    // Nếu đang gõ typewriter → skip toàn bộ text
    // Nếu đã xong và _dlg_canContinue → gọi callback advance
    private static void OnContinuePressed()
    {
        if (_dlg_isTyping)
        {
            // Skip typewriter: dừng coroutine, hiện toàn bộ text
            if (_dlg_typeCoroutine != null && _coroutineHost != null)
                _coroutineHost.StopCoroutine(_dlg_typeCoroutine);
            _dlg_typeCoroutine = null;
            _dlg_isTyping = false;
            _dlg_canContinue = true;
            if (_dlg_msgText != null) _dlg_msgText.text = _dlg_fullLine;
            return;
        }

        if (_dlg_canContinue)
        {
            _dlg_canContinue = false;
            _dlg_onAdvance?.Invoke();
        }
    }


    private IEnumerator TransitionToNextScene()
    {
        Debug.Log("<color=cyan>[BossSceneTransition] TransitionToNextScene started on CoroutineRunner</color>");

        if (s_victoryPanel != null) s_victoryPanel.SetActive(true);

        if (s_playBetrayalDialogue && s_betrayalDialogue != null && s_betrayalDialogue.Count > 0)
        {
            Debug.Log($"<color=cyan>[BossSceneTransition] Starting dialogue — {s_betrayalDialogue.Count} lines</color>");
            // DisablePlayerInput(); // Removed from here, moved to after 3s delay in PlayDialogueSequence
            // Use _coroutineHost (CoroutineRunner) NOT this — boss may be destroyed!
            yield return _coroutineHost.StartCoroutine(PlayDialogueSequence());
            ResetPlayerInput();
        }
        else
        {
            DebugLog($"Dialogue skipped — playBetrayalDialogue={s_playBetrayalDialogue}, count={s_betrayalDialogue?.Count}");
        }

        Time.timeScale = 1f;

        // Give Unity a split second to initiate scene load
        if (string.IsNullOrEmpty(s_nextSceneName))
        {
            Debug.LogError("[ERROR] Next Scene Name is EMPTY!");
            if (_activeCanvas != null) Destroy(_activeCanvas); // Cleanup if fail
            s_transitionInProgress = false;
            yield break;
        }

        Debug.Log($"<color=green>LOADING SCENE: {s_nextSceneName}</color>");
        try 
        { 
            SceneManager.LoadScene(s_nextSceneName); 
        }
        catch (System.Exception e) 
        { 
            Debug.LogError($"[ERROR] Failed to load scene: {e.Message}");
            if (_activeCanvas != null) Destroy(_activeCanvas);
            s_transitionInProgress = false; // Reset lock on error
            yield break; // Stop coroutine on error
        }

        // If synchronous load, the next lines may not run in this scene.
        // If they do (e.g. before next frame), we wait a bit before destroying the "Con Tiep" screen.
        yield return new WaitForSecondsRealtime(0.5f);
        if (_activeCanvas != null) Destroy(_activeCanvas);
    }

    // ─────────────────────────────────────────────────────────────
    //  CINEMATIC DIALOGUE SEQUENCE
    // ─────────────────────────────────────────────────────────────
    private IEnumerator PlayDialogueSequence()
    {
        Debug.Log("<color=cyan>[BossSceneTransition] PlayDialogueSequence started!</color>");
        
        // 1. Initial 3 second delay after boss dies/disappears (HUD stays visible during this)
        yield return new WaitForSecondsRealtime(3.0f); 

        // Now disable input and hide HUD for the dialogue
        DisablePlayerInput();

        // Freeze gameplay — dialogue uses WaitForSecondsRealtime so it isn't affected
        Time.timeScale = 0f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (s_victoryPanel != null) s_victoryPanel.SetActive(false);

        // Play background music
        if (s_betrayalBGM != null)
        {
            _backgroundMusicSource.clip = s_betrayalBGM;
            _backgroundMusicSource.Play();
        }

        // ── Build Canvas ──────────────────────────────────────────
        _activeCanvas = new GameObject("BetrayalDialogueCanvas");
        DontDestroyOnLoad(_activeCanvas);
        Canvas canvas = _activeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = _activeCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        _activeCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        Font uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null) uiFont = Font.CreateDynamicFontFromOSFont("Arial", 50);

        // ── FULL OVERLAYS REMOVED TO KEEP MAP VISIBLE ─────────────
        /*
        GameObject bgObj = CreateUIImage(_activeCanvas.transform, "Background",
            new Color(0.04f, 0.04f, 0.07f, 1f),
            Vector2.zero, Vector2.one);
        */

        /*
        GameObject letterTop = CreateUIImage(_activeCanvas.transform, "LetterboxTop",
            Color.black,
            new Vector2(0f, 0.88f), new Vector2(1f, 1f));

        // ── LETTERBOX — bottom bar ────────────────────────────────
        GameObject letterBot = CreateUIImage(_activeCanvas.transform, "LetterboxBottom",
            Color.black,
            new Vector2(0f, 0f), new Vector2(1f, 0.12f));
        */

        // ── PORTRAITS REMOVED PER IMAGE 2 ─────────────────────────
        Sprite circleSprite = CreateCircleSprite(256);
        /* 
        // Side portraits are hidden to match Image 2 style
        // Outer glow ring (left)
        GameObject leftRingObj = CreateUIImage(_activeCanvas.transform, "LeftRing",
            new Color(0.3f, 0.7f, 1f, 0.85f),
            new Vector2(0.03f, 0.25f), new Vector2(0.26f, 0.75f));
        leftRingObj.GetComponent<UnityEngine.UI.Image>().sprite = circleSprite;
        leftRingObj.GetComponent<UnityEngine.UI.Image>().type = UnityEngine.UI.Image.Type.Simple;

        // Circular mask container (left)
        GameObject leftMaskObj = new GameObject("LeftAvatarMask");
        leftMaskObj.transform.SetParent(_activeCanvas.transform, false);
        var leftMaskImg = leftMaskObj.AddComponent<UnityEngine.UI.Image>();
        leftMaskImg.sprite = circleSprite;
        leftMaskImg.color = Color.white;
        var leftMask = leftMaskObj.AddComponent<UnityEngine.UI.Mask>();
        leftMask.showMaskGraphic = false;
        var leftMaskRect = leftMaskObj.GetComponent<RectTransform>();
        leftMaskRect.anchorMin = new Vector2(0.04f, 0.26f);
        leftMaskRect.anchorMax = new Vector2(0.25f, 0.74f);
        leftMaskRect.offsetMin = Vector2.zero;
        leftMaskRect.offsetMax = Vector2.zero;

        // Actual avatar image inside mask (left)
        GameObject leftAvatarObj = new GameObject("LeftAvatar");
        leftAvatarObj.transform.SetParent(leftMaskObj.transform, false);
        var leftAvatarImg = leftAvatarObj.AddComponent<UnityEngine.UI.Image>();
        leftAvatarImg.preserveAspect = true;
        if (s_thachSanhAvatar != null) leftAvatarImg.sprite = s_thachSanhAvatar;
        var leftAvatarRect = leftAvatarObj.GetComponent<RectTransform>();
        leftAvatarRect.anchorMin = Vector2.zero;
        leftAvatarRect.anchorMax = Vector2.one;
        leftAvatarRect.offsetMin = Vector2.zero;
        leftAvatarRect.offsetMax = Vector2.zero;

        // ── RIGHT AVATAR (Lý Thông) — circular ───────────────────
        // Outer glow ring (right)
        GameObject rightRingObj = CreateUIImage(_activeCanvas.transform, "RightRing",
            new Color(1f, 0.3f, 0.3f, 0.85f),
            new Vector2(0.74f, 0.25f), new Vector2(0.97f, 0.75f));
        rightRingObj.GetComponent<UnityEngine.UI.Image>().sprite = circleSprite;
        rightRingObj.GetComponent<UnityEngine.UI.Image>().type = UnityEngine.UI.Image.Type.Simple;

        // Circular mask container (right)
        GameObject rightMaskObj = new GameObject("RightAvatarMask");
        rightMaskObj.transform.SetParent(_activeCanvas.transform, false);
        var rightMaskImg = rightMaskObj.AddComponent<UnityEngine.UI.Image>();
        rightMaskImg.sprite = circleSprite;
        rightMaskImg.color = Color.white;
        var rightMask = rightMaskObj.AddComponent<UnityEngine.UI.Mask>();
        rightMask.showMaskGraphic = false;
        var rightMaskRect = rightMaskObj.GetComponent<RectTransform>();
        rightMaskRect.anchorMin = new Vector2(0.75f, 0.26f);
        rightMaskRect.anchorMax = new Vector2(0.96f, 0.74f);
        rightMaskRect.offsetMin = Vector2.zero;
        rightMaskRect.offsetMax = Vector2.zero;

        // Actual avatar image inside mask (right)
        GameObject rightAvatarObj = new GameObject("RightAvatar");
        rightAvatarObj.transform.SetParent(rightMaskObj.transform, false);
        var rightAvatarImg = rightAvatarObj.AddComponent<UnityEngine.UI.Image>();
        rightAvatarImg.preserveAspect = true;
        if (s_lyThongAvatar != null) rightAvatarImg.sprite = s_lyThongAvatar;
        var rightAvatarRect = rightAvatarObj.GetComponent<RectTransform>();
        rightAvatarRect.anchorMin = Vector2.zero;
        rightAvatarRect.anchorMax = Vector2.one;
        rightAvatarRect.offsetMin = Vector2.zero;
        rightAvatarRect.offsetMax = Vector2.zero;

        // Ring image refs for glow animation
        var leftRingImg  = leftRingObj.GetComponent<UnityEngine.UI.Image>();
        var rightRingImg = rightRingObj.GetComponent<UnityEngine.UI.Image>();
        */

        // ── SLIM CENTERED DIALOGUE PANEL (Near Bottom, Translucent)
        GameObject dialogPanel = CreateUIImage(_activeCanvas.transform, "DialoguePanel",
            new Color(0f, 0f, 0f, 0.55f), 
            new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.20f)); // 90% width, standard slim height
        dialogPanel.transform.SetAsLastSibling();

        // ── Name Box Background (Centered Tag) ───────────────────
        GameObject nameBox = CreateUIImage(dialogPanel.transform, "NameBoxBg",
            new Color(0.1f, 0.1f, 0.1f, 0.85f), 
            new Vector2(0.42f, 1.0f), new Vector2(0.58f, 1.35f)); // Centered and adjusted

        // Speaker name text
        GameObject nameObj = new GameObject("SpeakerName");
        nameObj.transform.SetParent(nameBox.transform, false); // Parented to NameBox
        var nameText = nameObj.AddComponent<UnityEngine.UI.Text>();
        nameText.font = uiFont;
        nameText.fontSize = 48;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.supportRichText = true;
        var nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = Vector2.zero;
        nameRect.anchorMax = Vector2.one;
        nameRect.sizeDelta = Vector2.zero;

        // Dialogue message text
        GameObject msgObj = new GameObject("MessageText");
        msgObj.transform.SetParent(dialogPanel.transform, false);
        var msgText = msgObj.AddComponent<UnityEngine.UI.Text>();
        msgText.font = uiFont;
        msgText.fontSize = 42;
        msgText.lineSpacing = 1.3f;
        msgText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        msgText.alignment = TextAnchor.UpperLeft;
        msgText.horizontalOverflow = HorizontalWrapMode.Wrap;
        msgText.verticalOverflow = VerticalWrapMode.Truncate;
        msgText.supportRichText = true;
        var msgRect = msgObj.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.05f, 0.25f); // Less horizontal padding needed for 90% width
        msgRect.anchorMax = new Vector2(0.95f, 0.85f);
        msgRect.sizeDelta = Vector2.zero;

        // ── Continue Prompt ───────────────────────────────────────
        GameObject contObj = new GameObject("ContinueText");
        contObj.transform.SetParent(dialogPanel.transform, false);
        _continueText = contObj.AddComponent<UnityEngine.UI.Text>();
        _continueText.font = uiFont;
        _continueText.fontSize = 32;
        _continueText.fontStyle = FontStyle.Bold;
        _continueText.color = new Color(1f, 0.85f, 0f, 1f); // Vibrant Yellow
        _continueText.alignment = TextAnchor.LowerRight;
        _continueText.text = "▼ SPACE hoặc Click";
        _continueText.gameObject.SetActive(false); // Initially hidden
        var contRect = contObj.GetComponent<RectTransform>();
        contRect.anchorMin = new Vector2(0.7f, 0.08f);
        contRect.anchorMax = new Vector2(0.95f, 0.22f);
        contRect.sizeDelta = Vector2.zero;

        /*
        yield return _coroutineHost.StartCoroutine(AnimateLetterbox(letterTop, letterBot, true));
        */

        // ── DIALOGUE LOOP (QuestDialogue pattern) ─────────────────
        // Coroutine KHÔNG bao giờ yield chờ input.
        // Input được đọc trong CoroutineRunner.Update() → OnContinuePressed() → _dlg_onAdvance().
        if (s_betrayalDialogue == null || s_betrayalDialogue.Count == 0)
        {
            DebugLog("PlayDialogueSequence: s_betrayalDialogue is EMPTY!");
            yield break;
        }

        _dlg_dialogueDone = false;
        _dlg_msgText = msgText;

        int lineCount = s_betrayalDialogue.Count;
        // lineIndex được điều khiển bởi callback _dlg_onAdvance, không bởi foreach
        _dlg_currentIndex = 0;

        // Hàm hiển thị 1 dòng (giống ShowLine của QuestDialogue)
        System.Action<int> showLine = null;
        showLine = (idx) =>
        {
            if (idx >= lineCount)
            {
                // Hết dialogue
                _dlg_isTyping     = false;
                _dlg_canContinue  = false;
                _dlg_dialogueDone = true;
                return;
            }

            var line       = s_betrayalDialogue[idx];
            bool isNarration = string.IsNullOrEmpty(line.speaker);
            bool isLyThong   = line.speaker == "Lý Thông";
            bool isConTiep   = (line.message == "Còn tiếp" || line.message == "Còn tiếp...");

            // ── "Còn tiếp" special display ─────────────────────────
            if (isConTiep)
            {
                dialogPanel.SetActive(false);
                GameObject conTiepObj = new GameObject("ConTiepText");
                conTiepObj.transform.SetParent(_activeCanvas.transform, false);
                var ctText = conTiepObj.AddComponent<UnityEngine.UI.Text>();
                ctText.font = uiFont;
                ctText.fontSize = 100;
                ctText.fontStyle = FontStyle.Bold;
                ctText.alignment = TextAnchor.MiddleCenter;
                ctText.color = new Color(1f, 0.85f, 0.3f, 0f);
                ctText.text = "Còn tiếp...";
                GameObject endOverlay = CreateUIImage(_activeCanvas.transform, "EndOverlay",
                    new Color(0,0,0, 0.4f), Vector2.zero, Vector2.one);
                endOverlay.transform.SetAsFirstSibling();
                var ctRect = conTiepObj.GetComponent<RectTransform>();
                ctRect.anchorMin = new Vector2(0.1f, 0.35f);
                ctRect.anchorMax = new Vector2(0.9f, 0.65f);
                ctRect.offsetMin = Vector2.zero;
                ctRect.offsetMax = Vector2.zero;
                var ctShadow = conTiepObj.AddComponent<UnityEngine.UI.Shadow>();
                ctShadow.effectColor = new Color(0.8f, 0.5f, 0f, 0.8f);
                ctShadow.effectDistance = new Vector2(3, -3);

                // Fade in rồi kết thúc — dùng coroutine riêng
                _coroutineHost.StartCoroutine(ShowConTiepAndFinish(ctText));
                return;
            }

            // ── Style text theo loại dòng ──────────────────────────
            if (isNarration)
            {
                nameText.text = "";
                msgText.fontSize = 44;
                msgText.fontStyle = FontStyle.Italic;
                msgText.alignment = TextAnchor.MiddleCenter;
                msgText.color = new Color(0.75f, 0.85f, 1f, 1f);
            }
            else
            {
                nameText.text = line.speaker;
                nameBox.SetActive(true);
                msgText.fontSize = 42;
                msgText.fontStyle = FontStyle.Normal;
                msgText.alignment = TextAnchor.UpperLeft;
                msgText.lineSpacing = 1.35f;
                bool isBetrayalLine = isLyThong &&
                    (line.message.Contains("một mình ta hưởng") ||
                     line.message.Contains("lấp kín") ||
                     line.message.Contains("kế hoạch"));
                msgText.color = isBetrayalLine
                    ? new Color(1f, 0.85f, 0.85f, 1f)
                    : new Color(0.95f, 0.95f, 0.95f, 1f);
            }

            // ── Ẩn continue hint ───────────────────────────────────
            if (_continueText != null) _continueText.gameObject.SetActive(false);

            // ── Play audio ─────────────────────────────────────────
            if (line.voiceClip != null)
            {
                if (_dialogueAudioSource.isPlaying) _dialogueAudioSource.Stop();
                _dialogueAudioSource.clip = line.voiceClip;
                _dialogueAudioSource.Play();
            }

            // ── Set advance callback TRƯỚC khi bắt đầu typewriter ──
            _dlg_onAdvance = () =>
            {
                _dlg_currentIndex = idx + 1;
                showLine(_dlg_currentIndex);
            };

            // ── Bắt đầu typewriter (giống TypeLine của QuestDialogue)
            _dlg_fullLine = line.message;
            _dlg_isTyping = true;
            _dlg_canContinue = false;
            if (_dlg_typeCoroutine != null) _coroutineHost.StopCoroutine(_dlg_typeCoroutine);
            _dlg_typeCoroutine = _coroutineHost.StartCoroutine(TypewriterAndThenWait(line.message, msgText));
        };

        // Bắt đầu câu đầu tiên
        showLine(0);

        // Coroutine chỉ yield chờ _dlg_dialogueDone — KHÔNG chờ input
        while (!_dlg_dialogueDone)
            yield return null;

        // ── Cleanup ───────────────────────────────────────────────
        _dlg_onAdvance = null;
        if (_backgroundMusicSource.isPlaying) _backgroundMusicSource.Stop();
        if (_dialogueAudioSource.isPlaying) _dialogueAudioSource.Stop();

        yield return new WaitForSecondsRealtime(0.3f);
        if (_activeCanvas != null) Object.Destroy(_activeCanvas);
    }

    // Typewriter rồi hiện continue hint — giống TypeLine() của QuestDialogue
    private IEnumerator TypewriterAndThenWait(string fullText, UnityEngine.UI.Text target)
    {
        target.text = "";
        foreach (char c in fullText)
        {
            target.text += c;
            yield return new WaitForSecondsRealtime(s_typewriterSpeed);
        }
        // Typewriter xong
        _dlg_isTyping    = false;
        _dlg_canContinue = true;
        if (_continueText != null) _continueText.gameObject.SetActive(true);
    }

    // Fade in "Còn tiếp..." rồi đánh dấu xong
    private IEnumerator ShowConTiepAndFinish(UnityEngine.UI.Text ctText)
    {
        yield return _coroutineHost.StartCoroutine(FadeTextAlpha(ctText, 0f, 1f, 1.2f));
        yield return new WaitForSecondsRealtime(1.8f);
        if (_backgroundMusicSource != null && _backgroundMusicSource.isPlaying)
            _backgroundMusicSource.Stop();
        _dlg_isTyping     = false;
        _dlg_canContinue  = false;
        _dlg_dialogueDone = true;
    }



    // ─────────────────────────────────────────────────────────────
    //  ANIMATION HELPERS
    // ─────────────────────────────────────────────────────────────

    /// <summary>Lerp avatar alpha and scale: active = bright + big, inactive = dim + small</summary>
    private IEnumerator LerpAvatarState(UnityEngine.UI.Image avatarImg, bool isActive)
    {
        if (avatarImg == null) yield break;

        float targetAlpha = isActive ? 1f : 0.38f;
        float targetScale = isActive ? 1.08f : 0.90f;

        Color startColor = avatarImg.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        Vector3 startScale = avatarImg.rectTransform.localScale;
        Vector3 endScale = Vector3.one * targetScale;

        float t = 0f;
        float duration = 0.11f; // Speed up avatar swap (was 0.22s)
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float smooth = p * p * (3f - 2f * p); // smoothstep
            avatarImg.color = Color.Lerp(startColor, endColor, smooth);
            avatarImg.rectTransform.localScale = Vector3.Lerp(startScale, endScale, smooth);
            yield return null;
        }

        avatarImg.color = endColor;
        avatarImg.rectTransform.localScale = endScale;
    }

    /// <summary>Slide dialogue panel in from left or right</summary>
    private IEnumerator SlideDialogPanel(GameObject panel, float directionSign)
    {
        var rt = panel.GetComponent<RectTransform>();
        if (rt == null) yield break;

        float slideAmount = 60f * directionSign;
        Vector2 startPos = new Vector2(slideAmount, rt.anchoredPosition.y);
        Vector2 endPos = new Vector2(0f, rt.anchoredPosition.y);

        // Also fade the panel alpha
        var img = panel.GetComponent<UnityEngine.UI.Image>();
        Color startColor = new Color(img.color.r, img.color.g, img.color.b, 0f);
        Color endColor = new Color(img.color.r, img.color.g, img.color.b, 0.5f);

        rt.anchoredPosition = startPos;
        img.color = startColor;

        float t = 0f;
        float duration = 0.10f; // Speed up bubble slide (was 0.18s)
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float smooth = p * p * (3f - 2f * p);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, smooth);
            img.color = Color.Lerp(startColor, endColor, smooth);
            yield return null;
        }

        rt.anchoredPosition = endPos;
        img.color = endColor;
    }

    /// <summary>Slide letterbox bars in/out</summary>
    private IEnumerator AnimateLetterbox(GameObject top, GameObject bot, bool slideIn)
    {
        var topRT = top.GetComponent<RectTransform>();
        var botRT = bot.GetComponent<RectTransform>();

        // Top bar: anchorMin.y goes from 1 (hidden above) to 0.88 (visible)
        // Bot bar: anchorMax.y goes from 0 (hidden below) to 0.12 (visible)
        float startTop = slideIn ? 1f : 0.88f;
        float endTop   = slideIn ? 0.88f : 1f;
        float startBot = slideIn ? 0f : 0.12f;
        float endBot   = slideIn ? 0.12f : 0f;

        float t = 0f;
        float duration = 0.5f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float smooth = p * p * (3f - 2f * p);

            topRT.anchorMin = new Vector2(0f, Mathf.Lerp(startTop, endTop, smooth));
            botRT.anchorMax = new Vector2(1f, Mathf.Lerp(startBot, endBot, smooth));
            yield return null;
        }
    }

    /// <summary>Fade a Text component's alpha over time</summary>
    private IEnumerator FadeTextAlpha(UnityEngine.UI.Text text, float from, float to, float duration)
    {
        float t = 0f;
        Color c = text.color;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            text.color = new Color(c.r, c.g, c.b, Mathf.Lerp(from, to, p));
            yield return null;
        }
        text.color = new Color(c.r, c.g, c.b, to);
    }

    // ─────────────────────────────────────────────────────────────
    //  RING GLOW ANIMATION
    // ─────────────────────────────────────────────────────────────
    private IEnumerator LerpRingGlow(UnityEngine.UI.Image ring, Color targetColor)
    {
        if (ring == null) yield break;
        Color startColor = ring.color;
        float t = 0f;
        float duration = 0.22f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float smooth = p * p * (3f - 2f * p);
            ring.color = Color.Lerp(startColor, targetColor, smooth);
            yield return null;
        }
        ring.color = targetColor;
    }

    // ─────────────────────────────────────────────────────────────
    //  CIRCLE SPRITE GENERATOR
    // ─────────────────────────────────────────────────────────────
    /// <summary>Generate a filled circle Sprite programmatically (for circular mask & ring)</summary>
    private Sprite CreateCircleSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;
        float cx = size / 2f;
        float cy = size / 2f;
        float r  = size / 2f - 1f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                // Anti-aliased edge
                float alpha = Mathf.Clamp01(r - dist + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            size);
    }

    // ─────────────────────────────────────────────────────────────
    //  UI BUILDER HELPER
    // ─────────────────────────────────────────────────────────────
    private GameObject CreateUIImage(Transform parent, string name, Color color,
                                     Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var img = obj.AddComponent<UnityEngine.UI.Image>();
        img.color = color;
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return obj;
    }

    // ─────────────────────────────────────────────────────────────
    //  PLAYER INPUT FREEZE
    // ─────────────────────────────────────────────────────────────
    private void DisablePlayerInput()
    {
        // 1. Disable Key Control Scripts
        MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script == null) continue;
            string n = script.GetType().Name;
            
            if (n == "PlayerAttack" || n == "PlayerInput" || n.Contains("ThirdPersonUserControl"))
            {
                script.enabled = false;
                DebugLog($"Disabled Input: {n}");
            }
            // 2. Hide specific Health Bar scripts by name or containing "HealthBar"
            else if (n.Contains("HealthBar") || n == "HealthBarUI" || n == "PlayerHealthBarSync" || n == "BossHealthBarFixedTop")
            {
                script.gameObject.SetActive(false);
                DebugLog($"Hidden script-based HUD: {n}");
            }
        }

        // 3. Fallback: Find all Canvases and hide them if they aren't the dialogue one
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (c.name != "BetrayalDialogueCanvas" && c.name != "Background" && !c.name.Contains("Dialogue"))
            {
                // Only hide if it's active
                if (c.gameObject.activeSelf)
                {
                    c.gameObject.SetActive(false);
                    // Tag it so we know to turn it back on
                    if (!c.gameObject.name.EndsWith("_HiddenByTransition"))
                        c.gameObject.name += "_HiddenByTransition";
                    
                    DebugLog($"Hidden Canvas: {c.name}");
                }
            }
        }
    }

    private void ResetPlayerInput()
    {
        MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var script in scripts)
        {
            if (script == null) continue;
            string n = script.GetType().Name;
            
            if (n == "PlayerAttack" || n == "PlayerInput" || n.Contains("ThirdPersonUserControl"))
            {
                script.enabled = true;
            }
            else if (n.Contains("HealthBar") || n == "HealthBarUI" || n == "PlayerHealthBarSync" || n == "BossHealthBarFixedTop")
            {
                script.gameObject.SetActive(true);
            }
        }

        // Restore Canvases
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.name.EndsWith("_HiddenByTransition"))
            {
                obj.name = obj.name.Replace("_HiddenByTransition", "");
                obj.SetActive(true);
                DebugLog($"Restored HUD: {obj.name}");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  PUBLIC / EDITOR
    // ─────────────────────────────────────────────────────────────
    [ContextMenu("Test Hội Thoại Bằng Tay")]
    public void TriggerSceneTransition()
    {
        if (s_transitionInProgress) 
        {
            Debug.LogWarning("[BossSceneTransition] Transition already in progress. Ignoring manual trigger.");
            return;
        }

        s_transitionInProgress = true;
        _transitionStarted = true;
        Debug.Log($"<color=orange>[BossSceneTransition] MANUAL TEST TRIGGERED from '{gameObject.name}'</color>");

        // Snapshot data for testing
        s_nextSceneName = nextSceneName;
        s_playBetrayalDialogue = playBetrayalDialogue;
        s_betrayalDialogue = (betrayalDialogue != null) ? new List<DialogueLine>(betrayalDialogue) : new List<DialogueLine>();
        s_thachSanhAvatar = thachSanhAvatarSprite;
        s_lyThongAvatar = lyThongAvatarSprite;
        s_betrayalBGM = betrayalBackgroundMusic;
        s_typewriterSpeed = typewriterSpeed;
        s_victoryPanel = victoryPanel;
        snapshot_dialogueVolume = dialogueVolume;
        snapshot_bgmVolume = backgroundMusicVolume;

        // Create persistent runner
        GameObject runnerObj = new GameObject("BossTransitionRunner_Test");
        DontDestroyOnLoad(runnerObj);
        var runner = runnerObj.AddComponent<CoroutineRunner>();
        _coroutineHost = runner;
        SetupAudioSources(runnerObj);

        runner.RunCoroutine(TransitionToNextScene());
    }

    public void TransitionImmediately()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (betrayalDialogue == null || betrayalDialogue.Count < 9) return;

        int audioIndex = 1;
        for (int i = 0; i < betrayalDialogue.Count; i++)
        {
            if (string.IsNullOrEmpty(betrayalDialogue[i].speaker)) continue;
            if (betrayalDialogue[i].message == "Còn tiếp" || betrayalDialogue[i].message == "Còn tiếp...") continue;
            if (audioIndex > 7) break;

            string path = $"Assets/ThachSanhGeneral/HuuAnh/Sounds/{audioIndex}.mp3";
            var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
            {
                path = $"Assets/ThachSanhGeneral/HuuAnh/Sounds/{audioIndex}.wav";
                clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            if (clip != null) betrayalDialogue[i].voiceClip = clip;
            audioIndex++;
        }

        // Auto-assign avatar sprites
        var ts = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/ThachSanhGeneral/HuuAnh/Sprites/ThachSanh_Avatar.png");
        var lt = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/ThachSanhGeneral/HuuAnh/Sprites/LyThong_Avatar.png");

        if (ts != null && thachSanhAvatarSprite == null) thachSanhAvatarSprite = ts;
        if (lt != null && lyThongAvatarSprite == null) lyThongAvatarSprite = lt;
    }
#endif

    private void DebugLog(string message)
    {
        if (enableDebugLogs) Debug.Log($"[BossSceneTransition] {message}");
    }

    private void OnDestroy()
    {
        if (_transitionStarted)
            DebugLog("Destroyed (transition running on CoroutineRunner)");
    }
}
