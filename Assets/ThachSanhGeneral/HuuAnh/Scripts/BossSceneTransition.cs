using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
    private const float CHECK_INTERVAL = 0.5f; // Check every 0.5 seconds

    void Start()
    {
        DebugLog("=== BossSceneTransition Start ===");
        DebugLog($"GameObject name: {gameObject.name}");
        DebugLog($"Next Scene Name: '{nextSceneName}'");
        DebugLog($"Delay: {delayBeforeTransition}s");

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
            Debug.Log($"<color=red>?????????????????????????????????????????</color>");
            Debug.Log($"<color=red>?  BOSS DIED! ({bossType})              ?</color>");
            Debug.Log($"<color=red>?  Starting scene transition...        ?</color>");
            Debug.Log($"<color=red>?????????????????????????????????????????</color>");
            
            _transitionStarted = true;
            StartCoroutine(TransitionToNextScene());
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

        // Ensure cursor is visible and unlocked before scene transition
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        DebugLog("Cursor unlocked and visible");

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

    /// <summary>
    /// Public method to trigger scene transition manually (if needed).
    /// </summary>
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
    /// Helper for togglable debug logs
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[BossSceneTransition] {message}");
        }
    }

    // Detect when GameObject is destroyed
    private void OnDestroy()
    {
        if (_transitionStarted)
        {
            Debug.LogWarning("<color=orange>[WARNING] BossSceneTransition GameObject destroyed during transition!</color>");
        }
        else
        {
            DebugLog("BossSceneTransition GameObject destroyed (transition not started)");
        }
    }
}
