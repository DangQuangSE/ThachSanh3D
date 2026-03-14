using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// H? th?ng h?i tho?i nhân nhi?m v? ?ánh boss.
/// G?n script này lên NPC/GameObject t??ng tác.
/// Dùng PlayerPrefs ?? l?u ti?n trình gi?a các scene.
/// H? tr? c? Legacy Text và TextMeshPro.
/// T??ng thích v?i New Input System.
/// KHÔNG CẦN BUTTON - Dùng SPACE hoặc CLICK để tiếp tục
/// </summary>
public class QuestDialogue : MonoBehaviour
{
    // ?? Key l?u tr?ng thái boss (PlayerPrefs) ?????????????????????????????
    public const string KEY_CHAN_TINH_DEAD  = "BossChanTinhDead";
    public const string KEY_DAI_BANG_DEAD   = "BossDaiBangDead";

    // ?? Tên scene ??????????????????????????????????????????????????????????
    [Header("Scene Settings")]
    [Tooltip("Tên scene ?ánh Ch?n Tinh")]
    public string chanTinhSceneName = "PlaygroundB";

    [Tooltip("Tên scene ?ánh ??i Bàng Tinh")]
    public string daiBangSceneName  = "PlaygroundB";

    [Tooltip("Tên scene Main Menu (quay về sau khi hoàn thành)")]
    public string mainMenuSceneName = "MainMenu";

    // ?? Kho?ng cách t??ng tác ??????????????????????????????????????????????
    [Header("Interaction")]
    [Tooltip("Kho?ng cách ?? player có th? nói chuy?n v?i NPC")]
    public float interactRange = 3f;

    [Tooltip("Phím t??ng tác")]
    public KeyCode interactKey = KeyCode.F;

    [Tooltip("Phím tiếp tục hội thoại (Enter hoặc Click chuột)")]
    public KeyCode continueKey = KeyCode.Return;

    [Tooltip("Tag c?a player")]
    public string playerTag = "Player";

    [Tooltip("Hi?n g?i ý nhấn F khi ??n gàn")]
    public GameObject interactHint;

    [Tooltip("Hint tiếp tục - VD: 'Nhấn ENTER hoặc Click để tiếp tục'")]
    public GameObject continueHint;

    // ?? UI (Legacy Text) ???????????????????????????????????????????????????
    [Header("Dialogue UI - Legacy Text")]
    [Tooltip("Panel ch?a toàn b? h?i tho?i")]
    public GameObject dialoguePanel;

    [Tooltip("Panel tên nhân vật")]
    public GameObject characterNamePanel; // ← THÊM FIELD MỚI

    [Tooltip("Tên NPC (Legacy Text)")]
    public Text npcNameText;

    [Tooltip("N?i dung h?i tho?i (Legacy Text)")]
    public Text dialogueText;

    [Tooltip("Panel xác nh?n ??ng Ý / T? Ch?i")]
    public GameObject confirmPanel;

    [Tooltip("Nút ??ng Ý")]
    public Button acceptButton;

    [Tooltip("Nút T? Ch?i")]
    public Button declineButton;

    // ?? UI (TextMeshPro) ???????????????????????????????????????????????????
    [Header("Dialogue UI - TextMeshPro (Optional)")]
    [Tooltip("Tên NPC (TextMeshPro)")]
    public TMP_Text npcNameTextTMP;

    [Tooltip("N?i dung h?i tho?i (TextMeshPro)")]
    public TMP_Text dialogueTextTMP;

    [Tooltip("Text gợi ý tiếp tục (TMP) - VD: '▼ Nhấn SPACE'")]
    public TMP_Text continueHintTextTMP;

    // ?? T?c ?? hi?u ?ng typewriter ????????????????????????????????????????
    [Header("Typewriter Effect")]
    [Tooltip("S? ký t? hi?n m?i giây")]
    public float typeSpeed = 40f;

    // ?? Audio Settings ????????????????????????????????????????????????????
    [Header("Dialogue Audio - Chặn Tinh Quest")]
    [Tooltip("Âm thanh nền cho đoạn hội thoại Chặn Tinh (tùy chọn)")]
    public AudioClip chanTinhBackgroundMusic;

    [Tooltip("Âm thanh cho từng dòng hội thoại Chặn Tinh (6 dòng)")]
    public AudioClip[] chanTinhLineAudios = new AudioClip[6];

    [Header("Dialogue Audio - Đại Bàng Quest")]
    [Tooltip("Âm thanh nền cho đoạn hội thoại Đại Bàng (tùy chọn)")]
    public AudioClip daiBangBackgroundMusic;

    [Tooltip("Âm thanh cho từng dòng hội thoại Đại Bàng (5 dòng)")]
    public AudioClip[] daiBangLineAudios = new AudioClip[5];

    [Header("Dialogue Audio - All Done")]
    [Tooltip("Âm thanh nền cho đoạn hội thoại kết thúc (tùy chọn)")]
    public AudioClip allDoneBackgroundMusic;

    [Tooltip("Âm thanh cho từng dòng hội thoại kết thúc (7 dòng)")]
    public AudioClip[] allDoneLineAudios = new AudioClip[7];

    [Header("Audio Settings")]
    [Tooltip("Âm lượng cho âm thanh hội thoại (0-1)")]
    [Range(0f, 1f)]
    public float dialogueVolume = 0.7f;

    [Tooltip("Âm lượng cho background music (0-1)")]
    [Range(0f, 1f)]
    public float backgroundMusicVolume = 0.3f;

    [Tooltip("Phát âm thanh cho từng ký tự khi typewriter (tùy chọn)")]
    public AudioClip typewriterSound;

    [Tooltip("Âm lượng typewriter sound")]
    [Range(0f, 1f)]
    public float typewriterVolume = 0.2f;

    [Header("Scene Background Music")]
    [Tooltip("Nhạc nền cho scene này (phát khi scene bắt đầu)")]
    public AudioClip sceneBackgroundMusic;

    [Tooltip("Âm lượng nhạc nền scene (0-1)")]
    [Range(0f, 1f)]
    public float sceneBackgroundMusicVolume = 0.3f;

    [Tooltip("Tự động phát nhạc nền khi scene bắt đầu")]
    public bool autoPlaySceneMusic = true;

    [Header("Ending Settings")]
    [Tooltip("Thời gian chờ trước khi chuyển về Main Menu (giây)")]
    public float delayBeforeMainMenu = 3f;

    [Header("Genshin Style (Optional)")]
    [Tooltip("Genshin Dialogue Styler để thêm animations")]
    public GenshinDialogueStyler genshinStyler;

    // ?? N?i dung h?i tho?i ????????????????????????????????????????????????
    [Header("NPC Info")]
    public string npcName = "Lý Thông";

    // ?????????????????????????????????????????????????????????????????????
    // K?ch b?n h?i tho?i
    // ?????????????????????????????????????????????????????????????????????

    private readonly string[] _chanTinhLines =
    {
        "Hiền đệ ơi, đêm nay đến phiên ta phải đi canh miếu thờ.",
        "Khốn nỗi ta đang dở mẻ rượu, mẹ già lại đang đau yếu.",
        "Liệu đệ có thể chịu khó đi canh miếu thay ta một đêm được không?",
        "Thạch Sanh: Huynh cứ ở nhà lo cho mẹ, việc canh miếu cứ để đệ lo.",
        "Đệ thật tốt bụng, xong việc về đây anh em ta cùng uống rượu!",
        "(Nói thầm) Thế là cái mạng què của mình đã có người thế!"
    };

    private readonly string[] _daiBangLines =
    {
        "Ôi hiền đệ! Thật may quá, anh em ta cuối cùng cũng gặp lại nhau.",
        "Bấy lâu nay ta lo cho đệ khôn nguôi, cứ ngỡ đệ đã gặp chuyện chẳng lành.",
        "Nay nhà vua giao trọng trách tìm Công chúa, nếu không xong chắc ta mất mạng.",
        "Đệ vốn là người nghĩa hiệp, xin hãy cứu lấy người anh này một lần nữa!" ,
        "Ta biết chỉ có bản lĩnh của đệ mới có thể tìm được tung tích quái vật."
    };

    private readonly string[] _allDoneLines =
    {
       "Thạch Sanh: Anh Lý Thông ơi! Tôi đã hạ được Đại Bàng tinh và cứu được công chúa rồi. Hãy thả dây xuống đưa nàng lên trước!",
      "Lý Thông: Hiền đệ giỏi lắm! Mau buộc dây vào người công chúa, ta sẽ kéo nàng lên ngay. Đất nước mãi mãi ghi ơn người anh hùng!",
      "Thạch Sanh: Công chúa đã an toàn chưa anh? Giờ hãy thả dây xuống cho tôi nhé!",
      "Lý Thông: Thạch Sanh à, đệ làm rất tốt. Chằn Tinh và Đại Bàng Tinh đều đã bị tiêu diệt. Nhưng công lao này, một mình ta hưởng là đủ rồi!",
      "Lý Thông: Quân đâu! Mau lăn đá lấp kín cửa hang lại. Ta phải về triều báo tin vui là chính ta đã diệt quái vật cứu công chúa.",
      "Thạch Sanh: Lý Thông... tại sao anh lại đối xử với tôi như vậy...",
      "--- HẾT CHƯƠNG 1 ---"
    };

    // ?????????????????????????????????????????????????????????????????????
    // Private state
    // ═════════════════════════════════════════════════════════
    private Transform _player;
    private bool      _isDialogueOpen    = false;
    private int       _currentLineIndex  = 0;
    private string[]  _currentLines;
    private AudioClip[] _currentLineAudios;
    private string    _targetScene;
    private bool      _isTyping          = false;
    private string    _fullCurrentLine   = "";
    private string    _currentDialogueContent = "";
    private Coroutine _typeCoroutine;
    private bool      _isEndingDialogue  = false;
    private bool      _canContinue       = false;

    // Audio sources
    private AudioSource _dialogueAudioSource;
    private AudioSource _backgroundMusicSource;
    private AudioSource _typewriterAudioSource;
    private AudioSource _sceneBackgroundMusicSource;

    // Cache DialogueSystem reference để tránh Find() inactive object
    private GameObject _dialogueSystemCache;

    // ?????????????????????????????????????????????????????????????????????
    // Unity lifecycle
    // ?????????????????????????????????????????????????????????????????????

    private void Start()
    {
        SetupAudioSources();
        
        // Auto-find UI elements nếu chưa được gán
        AutoFindUIElements();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (confirmPanel  != null) confirmPanel.SetActive(false);
        if (interactHint  != null) interactHint.SetActive(false);
        if (continueHint  != null) continueHint.SetActive(false);

        if (acceptButton  != null) acceptButton.onClick.AddListener(OnAccept);
        if (declineButton != null) declineButton.onClick.AddListener(OnDecline);

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null) _player = playerObj.transform;

        // Phát nhạc nền cho scene (nếu có thiết lập)
        if (sceneBackgroundMusic != null && autoPlaySceneMusic)
        {
            PlaySceneBackgroundMusic();
        }
    }

    private void AutoFindUIElements()
    {
        // Tìm DialogueSystem trong scene (kể cả khi inactive)
        GameObject dialogueSystem = null;
        
        // Tìm trong tất cả objects (bao gồm inactive)
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "DialogueSystem" && obj.scene.isLoaded)
            {
                dialogueSystem = obj;
                _dialogueSystemCache = obj; // CACHE REFERENCE
                break;
            }
        }

        if (dialogueSystem == null)
        {
            Debug.LogWarning("[QuestDialogue] Không tìm thấy DialogueSystem trong scene. Vui lòng gán UI thủ công.");
            return;
        }

        Debug.Log($"[QuestDialogue] ✓ Tìm thấy DialogueSystem (Active: {dialogueSystem.activeSelf})");

        // Auto-find các UI elements
        if (dialoguePanel == null)
        {
            Transform dp = dialogueSystem.transform.Find("DialoguePanel");
            if (dp != null)
            {
                dialoguePanel = dp.gameObject;
                Debug.Log("[QuestDialogue] ✓ Auto-found DialoguePanel");
            }
        }

        // AUTO-FIND CHARACTER NAME PANEL
        if (characterNamePanel == null && dialogueSystem != null)
        {
            Transform cnp = dialogueSystem.transform.Find("CharacterNamePanel");
            if (cnp != null)
            {
                characterNamePanel = cnp.gameObject;
                Debug.Log("[QuestDialogue] ✓ Auto-found CharacterNamePanel");
            }
        }

        if (confirmPanel == null)
        {
            Transform cp = dialogueSystem.transform.Find("ConfirmPanel");
            if (cp != null)
            {
                confirmPanel = cp.gameObject;
                Debug.Log("[QuestDialogue] ✓ Auto-found ConfirmPanel");
            }
        }

        if (acceptButton == null && confirmPanel != null)
        {
            Transform ab = confirmPanel.transform.Find("AcceptButton");
            if (ab != null)
            {
                acceptButton = ab.GetComponent<Button>();
                Debug.Log("[QuestDialogue] ✓ Auto-found AcceptButton");
            }
        }

        if (declineButton == null && confirmPanel != null)
        {
            Transform db = confirmPanel.transform.Find("DeclineButton");
            if (db != null)
            {
                declineButton = db.GetComponent<Button>();
                Debug.Log("[QuestDialogue] ✓ Auto-found DeclineButton");
            }
        }

        if (npcNameTextTMP == null && characterNamePanel != null)
        {
            npcNameTextTMP = characterNamePanel.GetComponentInChildren<TMP_Text>();
            Debug.Log("[QuestDialogue] ✓ Auto-found CharacterNameText");
        }

        if (dialogueTextTMP == null && dialoguePanel != null)
        {
            Transform dt = dialoguePanel.transform.Find("DialogueText");
            if (dt != null)
            {
                dialogueTextTMP = dt.GetComponent<TMP_Text>();
                Debug.Log("[QuestDialogue] ✓ Auto-found DialogueText");
            }
        }

        if (continueHint == null && dialoguePanel != null)
        {
            Transform ch = dialoguePanel.transform.Find("ContinueHint");
            if (ch != null)
            {
                continueHint = ch.gameObject;
                Debug.Log("[QuestDialogue] ✓ Auto-found ContinueHint");
            }
        }

        if (continueHintTextTMP == null && continueHint != null)
        {
            Transform cht = continueHint.transform.Find("ContinueHintText");
            if (cht != null)
            {
                continueHintTextTMP = cht.GetComponent<TMP_Text>();
                Debug.Log("[QuestDialogue] ✓ Auto-found ContinueHintText");
            }
        }

        if (interactHint == null)
        {
            // InteractHint ở ngoài DialogueSystem, tìm toàn cục
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "InteractHint" && obj.scene.isLoaded)
                {
                    interactHint = obj;
                    Debug.Log("[QuestDialogue] ✓ Auto-found InteractHint");
                    break;
                }
            }
        }

        if (genshinStyler == null)
        {
            genshinStyler = dialogueSystem.GetComponent<GenshinDialogueStyler>();
            if (genshinStyler != null)
            {
                Debug.Log("[QuestDialogue] ✓ Auto-found GenshinDialogueStyler");
            }
        }

        Debug.Log("[QuestDialogue] ✓ Auto-find UI elements hoàn tất");
    }

    private void SetupAudioSources()
    {
        _dialogueAudioSource = gameObject.AddComponent<AudioSource>();
        _dialogueAudioSource.playOnAwake = false;
        _dialogueAudioSource.volume = dialogueVolume;

        _backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        _backgroundMusicSource.playOnAwake = false;
        _backgroundMusicSource.loop = true;
        _backgroundMusicSource.volume = backgroundMusicVolume;

        _typewriterAudioSource = gameObject.AddComponent<AudioSource>();
        _typewriterAudioSource.playOnAwake = false;
        _typewriterAudioSource.volume = typewriterVolume;

        // Setup scene background music source
        _sceneBackgroundMusicSource = gameObject.AddComponent<AudioSource>();
        _sceneBackgroundMusicSource.playOnAwake = false;
        _sceneBackgroundMusicSource.loop = true;
        _sceneBackgroundMusicSource.volume = sceneBackgroundMusicVolume;

        // Auto play scene music if enabled
        if (autoPlaySceneMusic && sceneBackgroundMusic != null)
        {
            PlaySceneBackgroundMusic();
        }
    }

    private void Update()
    {
        if (_player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        bool  inRange = dist <= interactRange;

        // Hiển thị interact hint khi chưa mở dialogue
        if (interactHint != null)
            interactHint.SetActive(inRange && !_isDialogueOpen);

        // Mở dialogue bằng phím F
        if (inRange && !_isDialogueOpen && GetInteractKeyDown())
        {
            OpenDialogue();
        }

        // Tiếp tục dialogue bằng SPACE hoặc Click chuột
        if (_isDialogueOpen && _canContinue)
        {
            if (GetContinueKeyDown() || GetMouseLeftClick())
            {
                OnContinueClicked();
            }
        }
    }

    // ?????????????????????????????????????????????????????????????????????
    // Input handling (h? tr? c? Old và New Input System)
    // ?????????????????????????????????????????????????????????????????????

    private bool GetInteractKeyDown()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return false;

        switch (interactKey)
        {
            case KeyCode.F:         return keyboard.fKey.wasPressedThisFrame;
            case KeyCode.E:         return keyboard.eKey.wasPressedThisFrame;
            case KeyCode.Space:     return keyboard.spaceKey.wasPressedThisFrame;
            case KeyCode.Return:    return keyboard.enterKey.wasPressedThisFrame;
            default:                return false;
        }
#else
        return Input.GetKeyDown(interactKey);
#endif
    }

    private bool GetContinueKeyDown()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return false;
        return keyboard.enterKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(continueKey);
#endif
    }

    private bool GetMouseLeftClick()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse == null) return false;
        return mouse.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    // ?????????????????????????????????????????????????????????????????????
    // Logic m? h?i tho?i
    // ?????????????????????????????????????????????????????????????????????

    private void OpenDialogue()
    {
        bool chanTinhDead = PlayerPrefs.GetInt(KEY_CHAN_TINH_DEAD, 0) == 1;
        bool daiBangDead  = PlayerPrefs.GetInt(KEY_DAI_BANG_DEAD,  0) == 1;

        if (daiBangDead)
        {
            _isEndingDialogue = true;
            StartDialogue(_allDoneLines, allDoneLineAudios, allDoneBackgroundMusic, null);
        }
        else if (chanTinhDead)
        {
            _isEndingDialogue = false;
            StartDialogue(_daiBangLines, daiBangLineAudios, daiBangBackgroundMusic, daiBangSceneName);
        }
        else
        {
            _isEndingDialogue = false;
            StartDialogue(_chanTinhLines, chanTinhLineAudios, chanTinhBackgroundMusic, chanTinhSceneName);
        }
    }

    private void StartDialogue(string[] lines, AudioClip[] lineAudios, AudioClip backgroundMusic, string targetScene)
    {
        _currentLines      = lines;
        _currentLineAudios = lineAudios;
        _targetScene       = targetScene;
        _currentLineIndex  = 0;
        _isDialogueOpen    = true;
        _canContinue       = false;

        // HIỆN TOÀN BỘ DIALOGUE SYSTEM - DÙNG CACHED REFERENCE
        if (_dialogueSystemCache != null)
        {
            _dialogueSystemCache.SetActive(true);
            Debug.Log("[QuestDialogue] ✓ DialogueSystem activated (from cache)");
        }
        else
        {
            // Fallback: Tìm lại nếu cache mất
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "DialogueSystem" && obj.scene.isLoaded)
                {
                    _dialogueSystemCache = obj;
                    _dialogueSystemCache.SetActive(true);
                    Debug.Log("[QuestDialogue] ✓ DialogueSystem activated (fallback find)");
                    break;
                }
            }
            
            if (_dialogueSystemCache == null)
            {
                Debug.LogError("[QuestDialogue] ✗ DialogueSystem not found!");
            }
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (confirmPanel  != null) confirmPanel.SetActive(false);
        if (interactHint  != null) interactHint.SetActive(false);

        // Set NPC name mặc định (sẽ thay đổi khi parse dialogue)
        SetText(npcNameText, npcNameTextTMP, npcName);

        // Hiện và unlock cursor để click button
        SetCursorState(true);

        // Dừng movement player
        SetPlayerMovement(false);

        // Tạm dừng nhạc nền scene khi bắt đầu dialogue
        if (_sceneBackgroundMusicSource != null && _sceneBackgroundMusicSource.isPlaying)
        {
            _sceneBackgroundMusicSource.Pause();
            Debug.Log("[QuestDialogue] ⏸ Scene music paused for dialogue");
        }

        // Play background music
        PlayBackgroundMusic(backgroundMusic);

        // Genshin style animation - GỌI SAU KHI ĐÃ ACTIVE
        if (genshinStyler != null)
        {
            genshinStyler.FadeIn();
            genshinStyler.BounceCharacterName();
        }

        ShowLine(_currentLineIndex);
    }

    private void ShowLine(int index)
    {
        if (index >= _currentLines.Length)
        {
            OnDialogueEnd();
            return;
        }

        _fullCurrentLine = _currentLines[index];
        _canContinue = false;

        // PARSE TÊN NHÂN VẬT TỪ DIALOGUE
        string speakerName = npcName; // Mặc định
        string dialogueContent = _fullCurrentLine;

        // Kiểm tra format: "Tên: Nội dung"
        if (_fullCurrentLine.Contains(":"))
        {
            int colonIndex = _fullCurrentLine.IndexOf(':');
            string potentialName = _fullCurrentLine.Substring(0, colonIndex).Trim();
            
            // Chỉ parse nếu tên ngắn (< 20 ký tự) và không có số
            if (potentialName.Length < 20 && !System.Text.RegularExpressions.Regex.IsMatch(potentialName, @"\d"))
            {
                speakerName = potentialName;
                dialogueContent = _fullCurrentLine.Substring(colonIndex + 1).Trim();
            }
        }

        // LƯU DIALOGUE CONTENT ĐÃ PARSE
        _currentDialogueContent = dialogueContent;

        // Update character name
        SetText(npcNameText, npcNameTextTMP, speakerName);

        // Ẩn continue hint khi bắt đầu dòng mới
        if (continueHint != null) continueHint.SetActive(false);

        StopLineAudio();
        PlayLineAudio(index);

        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        _typeCoroutine = StartCoroutine(TypeLine(dialogueContent));
    }

    private IEnumerator TypeLine(string line)
    {
        _isTyping = true;
        SetText(dialogueText, dialogueTextTMP, "");

        foreach (char c in line)
        {
            string currentText = GetText(dialogueText, dialogueTextTMP);
            SetText(dialogueText, dialogueTextTMP, currentText + c);
            
            PlayTypewriterSound();
            
            yield return new WaitForSeconds(1f / typeSpeed);
        }

        _isTyping = false;
        _canContinue = true;

        // Hiển thị continue hint sau khi đánh xong
        if (continueHint != null) continueHint.SetActive(true);
        
        // Update continue hint text
        UpdateContinueHintText();
    }

    private void UpdateContinueHintText()
    {
        bool isLastLine = (_currentLineIndex == _currentLines.Length - 1);
        
        string hintText;
        if (_isEndingDialogue && isLastLine)
        {
            hintText = "▼ ENTER - Quay về Main Menu";
        }
        else if (isLastLine)
        {
            hintText = "▼ ENTER - Kết thúc";
        }
        else
        {
            hintText = "▼ ENTER hoặc Click";
        }
        
        SetText(null, continueHintTextTMP, hintText);
    }

    // ?????????????????????????????????????????????????????????????????????
    // S? kiện nút
    // ?????????????????????????????????????????????????????????????????????

    private void OnContinueClicked()
    {
        // Nếu đang typing → Skip và hiện toàn bộ NỘI DUNG (không có tên)
        if (_isTyping)
        {
            if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
            _isTyping = false;
            _canContinue = true;
            SetText(dialogueText, dialogueTextTMP, _currentDialogueContent);
            
            if (continueHint != null) continueHint.SetActive(true);
            UpdateContinueHintText();
            return;
        }

        // Nếu không đang typing và có thể tiếp tục → Next line
        if (_canContinue)
        {
            _currentLineIndex++;
            ShowLine(_currentLineIndex);
        }
    }

    private void OnDialogueEnd()
    {
        StopLineAudio();
        
        if (continueHint != null) continueHint.SetActive(false);

        if (_isEndingDialogue)
        {
            Debug.Log("[QuestDialogue] Hoàn thành! Đang chuyển về Main Menu...");
            StartCoroutine(ReturnToMainMenu());
            return;
        }
        
        if (!string.IsNullOrEmpty(_targetScene))
        {
            Debug.Log($"[QuestDialogue] Kết thúc hội thoại - Tự động chuyển scene: {_targetScene}");
            
            // Hiển thị thông báo đang chuyển scene
            SetText(dialogueText, dialogueTextTMP, "Đang chuẩn bị xuất phát...");
            
            // Đợi 1.5 giây rồi tự động chuyển scene
            StartCoroutine(AutoLoadScene(_targetScene));
        }
        else
        {
            Debug.Log("[QuestDialogue] Không có target scene, đóng dialogue");
            CloseDialogue();
        }
    }

    private IEnumerator AutoLoadScene(string sceneName)
    {
        // Đợi một chút để người chơi thấy thông báo
        yield return new WaitForSeconds(1.5f);
        
        StopBackgroundMusic();
        StopLineAudio();
        StopSceneBackgroundMusic(); // Dừng nhạc nền scene khi chuyển scene
        
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (characterNamePanel != null) characterNamePanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        SetPlayerMovement(true);
        _isDialogueOpen = false;

        Debug.Log($"[QuestDialogue] Đang tải scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator ReturnToMainMenu()
    {
        SetText(dialogueText, dialogueTextTMP, "Đang trở về Main Menu...");
        yield return new WaitForSeconds(delayBeforeMainMenu);
        
        StopBackgroundMusic();
        StopLineAudio();
        StopSceneBackgroundMusic(); // Dừng nhạc nền scene khi về Main Menu
        
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        Debug.Log("[QuestDialogue] Chuyển về Main Menu: " + mainMenuSceneName);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnAccept()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
        
        StopBackgroundMusic();
        StopLineAudio();
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        SetPlayerMovement(true);
        _isDialogueOpen = false;

        if (!string.IsNullOrEmpty(_targetScene))
        {
            StartCoroutine(LoadScene(_targetScene));
        }
    }

    private void OnDecline()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
        
        // HIỆN LẠI CHARACTER NAME PANEL KHI DECLINE
        if (characterNamePanel != null) characterNamePanel.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        
        // Quay lại dòng cuối để player có thể đọc lại
        _currentLineIndex = _currentLines.Length - 1;
        ShowLine(_currentLineIndex);
    }

    private void CloseDialogue()
    {
        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        _isTyping       = false;
        _isDialogueOpen = false;
        _canContinue    = false;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (confirmPanel  != null) confirmPanel.SetActive(false);
        if (continueHint  != null) continueHint.SetActive(false);

        // ẨN TOÀN BỘ DIALOGUE SYSTEM - DÙNG CACHED REFERENCE
        if (_dialogueSystemCache != null)
        {
            _dialogueSystemCache.SetActive(false);
            Debug.Log("[QuestDialogue] ✓ DialogueSystem deactivated");
        }

        StopBackgroundMusic();
        StopLineAudio();

        // Tiếp tục phát nhạc nền scene sau khi đóng dialogue
        if (_sceneBackgroundMusicSource != null && !_sceneBackgroundMusicSource.isPlaying && sceneBackgroundMusic != null)
        {
            _sceneBackgroundMusicSource.UnPause();
            Debug.Log("[QuestDialogue] ▶ Scene music resumed");
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        SetPlayerMovement(true);

        // Genshin fade out
        if (genshinStyler != null)
        {
            genshinStyler.FadeOut();
        }
    }

    // ?????????????????????????????????????????????????????????????????????
    // Chuy?n scene
    // ?????????????????????????????????????????????????????????????????????

    private IEnumerator LoadScene(string sceneName)
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(sceneName);
    }

    // ?????????????????????????????????????????????????????????????????????
    // Khoá/m? movement player
    // ?????????????????????????????????????????????????????????????????????

    private void SetPlayerMovement(bool enabled)
    {
        if (_player == null) return;

        var controller = _player.GetComponent<StarterAssets.ThirdPersonController>();
        if (controller != null) controller.enabled = enabled;

        var charController = _player.GetComponent<CharacterController>();
        if (charController != null) charController.enabled = enabled;
    }

    // ?????????????????????????????????????????????????????????????????????
    // Hi?n/?n con tr? chu?t (ch? dùng khi M? dialog)
    // ?????????????????????????????????????????????????????????????????????

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // ?????????????????????????????????????????????????????????????????????
    // Helper: Set text (h? tr? c? Text và TextMeshPro)
    // ?????????????????????????????????????????????????????????????????????

    private void SetText(Text legacyText, TMP_Text tmpText, string value)
    {
        if (legacyText != null) legacyText.text = value;
        if (tmpText != null)    tmpText.text    = value;
    }

    private string GetText(Text legacyText, TMP_Text tmpText)
    {
        if (legacyText != null) return legacyText.text;
        if (tmpText != null)    return tmpText.text;
        return "";
    }

    // ?????????????????????????????????????????????????????????????????????
    // Static helpers — g?i t? BossController khi boss ch?t
    // ?????????????????????????????????????????????????????????????????????

    public static void MarkChanTinhDead()
    {
        PlayerPrefs.SetInt(KEY_CHAN_TINH_DEAD, 1);
        PlayerPrefs.Save();
        Debug.Log("[QuestDialogue] Ch?n Tinh ?ã b? tiêu di?t — ti?n trình l?u.");
    }

    public static void MarkDaiBangDead()
    {
        PlayerPrefs.SetInt(KEY_DAI_BANG_DEAD, 1);
        PlayerPrefs.Save();
        Debug.Log("[QuestDialogue] ??i Bàng Tinh ?ã b? tiêu di?t — ti?n trình l?u.");
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(KEY_CHAN_TINH_DEAD);
        PlayerPrefs.DeleteKey(KEY_DAI_BANG_DEAD);
        PlayerPrefs.Save();
        Debug.Log("[QuestDialogue] Ti?n trình ?ã ???c reset.");
    }

    // ?????????????????????????????????????????????????????????????????????

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }

    // ?????????????????????????????????????????????????????????????????????
    // Audio Methods
    // ?????????????????????????????????????????????????????????????????????

    private void PlayBackgroundMusic(AudioClip clip)
    {
        if (clip == null || _backgroundMusicSource == null) return;
        _backgroundMusicSource.clip = clip;
        _backgroundMusicSource.Play();
    }

    private void StopBackgroundMusic()
    {
        if (_backgroundMusicSource != null && _backgroundMusicSource.isPlaying)
            _backgroundMusicSource.Stop();
    }

    private void PlayLineAudio(int lineIndex)
    {
        if (_dialogueAudioSource == null) return;
        if (_currentLineAudios == null || lineIndex >= _currentLineAudios.Length) return;

        AudioClip clip = _currentLineAudios[lineIndex];
        if (clip != null)
        {
            _dialogueAudioSource.clip = clip;
            _dialogueAudioSource.Play();
        }
    }

    private void StopLineAudio()
    {
        if (_dialogueAudioSource != null && _dialogueAudioSource.isPlaying)
            _dialogueAudioSource.Stop();
    }

    private void PlayTypewriterSound()
    {
        if (typewriterSound != null && _typewriterAudioSource != null)
            _typewriterAudioSource.PlayOneShot(typewriterSound);
    }

    private void PlaySceneBackgroundMusic()
    {
        if (sceneBackgroundMusic == null || _sceneBackgroundMusicSource == null) return;
        
        _sceneBackgroundMusicSource.clip = sceneBackgroundMusic;
        _sceneBackgroundMusicSource.Play();
        Debug.Log("[QuestDialogue] ✓ Scene background music started");
    }

    private void StopSceneBackgroundMusic()
    {
        if (_sceneBackgroundMusicSource != null && _sceneBackgroundMusicSource.isPlaying)
        {
            _sceneBackgroundMusicSource.Stop();
            Debug.Log("[QuestDialogue] ✓ Scene background music stopped");
        }
    }
}
