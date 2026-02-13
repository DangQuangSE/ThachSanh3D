using UnityEngine;

/// <summary>
/// Script debug và test BossController
/// Attach vào Boss GameObject ?? test các tính n?ng
/// </summary>
public class BossDebugger : MonoBehaviour
{
    [Header("References")]
    public BossController boss;
    
    [Header("Debug Settings")]
    [Tooltip("Hi?n th? thông tin debug trên màn hình")]
    public bool showDebugUI = true;
    [Tooltip("Hi?n th? debug rays trong Scene")]
    public bool showDebugRays = true;
    [Tooltip("Log state changes vào Console")]
    public bool logStateChanges = true;
    
    [Header("Test Hotkeys")]
    [Tooltip("Phím gây damage cho Boss")]
    public KeyCode damageKey = KeyCode.K;
    [Tooltip("Phím kill Boss ngay l?p t?c")]
    public KeyCode killKey = KeyCode.L;
    [Tooltip("Phím heal Boss full máu")]
    public KeyCode healKey = KeyCode.H;
    
    [Header("Test Values")]
    [Tooltip("L??ng damage khi test")]
    public float testDamage = 100f;
    
    private BossController.BossState lastState;
    private GUIStyle guiStyle;

    void Start()
    {
        if (boss == null)
        {
            boss = GetComponent<BossController>();
        }
        
        if (boss == null)
        {
            Debug.LogError("BossDebugger: Không tìm th?y BossController!");
            enabled = false;
            return;
        }
        
        lastState = boss.GetCurrentState();
        
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 16;
        guiStyle.normal.textColor = Color.white;
        
        Debug.Log("=== BOSS DEBUGGER ACTIVATED ===");
        Debug.Log($"Press {damageKey} ?? gây {testDamage} damage");
        Debug.Log($"Press {killKey} ?? kill Boss");
        Debug.Log($"Press {healKey} ?? heal Boss");
    }

    void Update()
    {
        if (boss == null) return;
        
        HandleTestInput();
        CheckStateChange();
        
        if (showDebugRays)
        {
            DrawDebugRays();
        }
    }

    void OnGUI()
    {
        if (!showDebugUI || boss == null) return;
        
        int yPos = 10;
        int lineHeight = 25;
        
        GUI.Label(new Rect(10, yPos, 300, 20), "=== BOSS DEBUG INFO ===", guiStyle);
        yPos += lineHeight;
        
        GUI.color = GetStateColor(boss.GetCurrentState());
        GUI.Label(new Rect(10, yPos, 300, 20), $"State: {boss.GetCurrentState()}", guiStyle);
        yPos += lineHeight;
        
        GUI.color = Color.white;
        float healthPercent = boss.GetHealthPercentage() * 100f;
        Color healthColor = healthPercent > 50 ? Color.green : (healthPercent > 25 ? Color.yellow : Color.red);
        GUI.color = healthColor;
        GUI.Label(new Rect(10, yPos, 300, 20), $"Health: {healthPercent:F1}%", guiStyle);
        yPos += lineHeight;
        
        GUI.color = Color.white;
        GUI.Label(new Rect(10, yPos, 300, 20), $"Is Dead: {boss.IsDead()}", guiStyle);
        yPos += lineHeight;
        
        if (boss.target != null)
        {
            float distance = Vector3.Distance(boss.transform.position, boss.target.position);
            GUI.Label(new Rect(10, yPos, 300, 20), $"Distance to Target: {distance:F2}m", guiStyle);
            yPos += lineHeight;
        }
        
        yPos += 10;
        GUI.color = Color.cyan;
        GUI.Label(new Rect(10, yPos, 400, 20), "=== HOTKEYS ===", guiStyle);
        yPos += lineHeight;
        
        GUI.color = Color.white;
        GUI.Label(new Rect(10, yPos, 400, 20), $"[{damageKey}] Damage {testDamage}", guiStyle);
        yPos += lineHeight;
        GUI.Label(new Rect(10, yPos, 400, 20), $"[{killKey}] Kill Boss", guiStyle);
        yPos += lineHeight;
        GUI.Label(new Rect(10, yPos, 400, 20), $"[{healKey}] Heal Full", guiStyle);
    }

    private void HandleTestInput()
    {
        if (Input.GetKeyDown(damageKey))
        {
            boss.TakeDamage(testDamage);
            Debug.Log($"[TEST] Boss nh?n {testDamage} damage!");
        }
        
        if (Input.GetKeyDown(killKey))
        {
            boss.TakeDamage(9999);
            Debug.Log("[TEST] Boss b? kill!");
        }
        
        if (Input.GetKeyDown(healKey))
        {
            float maxHealth = boss.maxHealth;
            float currentHealth = boss.GetHealthPercentage() * maxHealth;
            float healAmount = maxHealth - currentHealth;
            
            boss.TakeDamage(-healAmount);
            Debug.Log("[TEST] Boss ???c heal full!");
        }
    }

    private void CheckStateChange()
    {
        BossController.BossState currentState = boss.GetCurrentState();
        
        if (currentState != lastState && logStateChanges)
        {
            Debug.Log($"[BOSS STATE] {lastState} ? {currentState}");
            lastState = currentState;
        }
    }

    private void DrawDebugRays()
    {
        if (boss.target == null) return;
        
        Debug.DrawLine(boss.transform.position + Vector3.up, 
                      boss.target.position + Vector3.up, 
                      Color.red);
        
        Vector3 forward = boss.transform.forward;
        Debug.DrawRay(boss.transform.position + Vector3.up, 
                     forward * boss.detectionRange, 
                     Color.yellow);
    }

    private Color GetStateColor(BossController.BossState state)
    {
        switch (state)
        {
            case BossController.BossState.Idle:
                return Color.white;
            case BossController.BossState.Chase:
                return Color.yellow;
            case BossController.BossState.Attack:
                return Color.red;
            case BossController.BossState.Death:
                return Color.gray;
            default:
                return Color.white;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (boss == null) return;
        
        Gizmos.color = Color.white;
        Vector3 bossPos = boss.transform.position;
        
        Gizmos.DrawWireSphere(bossPos, boss.detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bossPos, boss.attackRange);
        
        if (boss.target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(bossPos + Vector3.up, boss.target.position + Vector3.up);
            
            float distance = Vector3.Distance(bossPos, boss.target.position);
            Gizmos.DrawWireSphere(boss.target.position, 0.5f);
        }
    }
}
