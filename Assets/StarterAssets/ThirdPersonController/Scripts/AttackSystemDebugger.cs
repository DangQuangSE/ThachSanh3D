using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// Helper script to debug Attack System
    /// Attach this script to Player GameObject to view debug information
    /// </summary>
    public class AttackSystemDebugger : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Automatically retrieved from GameObject")]
        private ThirdPersonController _controller;
        private Animator _animator;
        private StarterAssetsInputs _input;

        [Header("Debug Settings")]
        [Tooltip("Display debug info on screen")]
        public bool showOnScreenDebug = true;
        
        [Tooltip("Log to Console")]
        public bool logToConsole = true;

        [Header("Debug Info (Read Only)")]
        [SerializeField] private bool isAttackPressed;
        [SerializeField] private bool isUltimatePressed;
        [SerializeField] private string currentAnimationState;
        [SerializeField] private float animationNormalizedTime;
        [SerializeField] private bool hasAnimator;
        [SerializeField] private bool isUltimateReady;
        [SerializeField] private float ultimateCooldown;

        private int _lastAttackCount = 0;

        private void Start()
        {
            // Automatically get references
            _controller = GetComponent<ThirdPersonController>();
            _animator = GetComponent<Animator>();
            _input = GetComponent<StarterAssetsInputs>();

            if (_controller == null)
                Debug.LogError("AttackSystemDebugger: ThirdPersonController component not found!");
            
            if (_animator == null)
                Debug.LogWarning("AttackSystemDebugger: Animator component not found!");
            
            if (_input == null)
                Debug.LogError("AttackSystemDebugger: StarterAssetsInputs component not found!");
        }

        private void Update()
        {
            if (_input != null)
            {
                isAttackPressed = _input.attack;
                isUltimatePressed = _input.ultimate;
                
                // Log when attack is pressed
                if (_input.attack && logToConsole)
                {
                    Debug.Log($"[Attack Input] Attack button pressed at {Time.time:F2}s");
                }

                // Log when ultimate is pressed
                if (_input.ultimate && logToConsole)
                {
                    Debug.Log($"[Ultimate Input] Ultimate button pressed at {Time.time:F2}s");
                }
            }

            if (_controller != null)
            {
                isUltimateReady = _controller.IsUltimateReady();
                ultimateCooldown = _controller.GetUltimateRemainingCooldown();
            }

            if (_animator != null)
            {
                hasAnimator = true;
                UpdateAnimatorInfo();
            }
            else
            {
                hasAnimator = false;
            }
        }

        private void UpdateAnimatorInfo()
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack_1"))
            {
                currentAnimationState = "Attack_1";
                animationNormalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            }
            else if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack_2"))
            {
                currentAnimationState = "Attack_2";
                animationNormalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            }
            else if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack_3"))
            {
                currentAnimationState = "Attack_3";
                animationNormalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            }
            else if (_animator.GetCurrentAnimatorStateInfo(0).IsName("UntimateAttack"))
            {
                currentAnimationState = "UntimateAttack";
                animationNormalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            }
            else
            {
                currentAnimationState = "Other";
                animationNormalizedTime = 0f;
            }
        }

        private void OnGUI()
        {
            if (!showOnScreenDebug) return;

            // Setup GUI style
            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperLeft;

            // Background box - increased height for ultimate info
            GUI.Box(new Rect(10, 10, 350, 280), "");

            // Display info
            float yPos = 20;
            float lineHeight = 22;

            GUI.Label(new Rect(20, yPos, 330, 20), "=== COMBAT SYSTEM DEBUG ===", style);
            yPos += lineHeight * 1.5f;

            // Input Status
            style.normal.textColor = isAttackPressed ? Color.green : Color.white;
            GUI.Label(new Rect(20, yPos, 330, 20), $"Attack Input: {(isAttackPressed ? "PRESSED" : "Released")}", style);
            yPos += lineHeight;

            // Ultimate Input Status
            style.normal.textColor = isUltimatePressed ? Color.magenta : Color.white;
            GUI.Label(new Rect(20, yPos, 330, 20), $"Ultimate Input: {(isUltimatePressed ? "PRESSED" : "Released")}", style);
            yPos += lineHeight;

            // Ultimate Ready Status
            style.normal.textColor = isUltimateReady ? Color.green : Color.red;
            GUI.Label(new Rect(20, yPos, 330, 20), $"Ultimate Ready: {isUltimateReady}", style);
            yPos += lineHeight;

            // Ultimate Cooldown
            if (!isUltimateReady)
            {
                style.normal.textColor = Color.yellow;
                GUI.Label(new Rect(20, yPos, 330, 20), $"Ultimate Cooldown: {ultimateCooldown:F1}s", style);
                yPos += lineHeight;
            }
            else
            {
                yPos += lineHeight; // Keep spacing consistent
            }

            // Animator Status
            style.normal.textColor = hasAnimator ? Color.green : Color.red;
            GUI.Label(new Rect(20, yPos, 330, 20), $"Has Animator: {hasAnimator}", style);
            yPos += lineHeight;

            // Current Animation
            style.normal.textColor = currentAnimationState == "UntimateAttack" ? Color.magenta : Color.yellow;
            if (hasAnimator)
            {
                GUI.Label(new Rect(20, yPos, 330, 20), $"Current State: {currentAnimationState}", style);
                yPos += lineHeight;

                GUI.Label(new Rect(20, yPos, 330, 20), $"Animation Time: {animationNormalizedTime:F2}", style);
                yPos += lineHeight;
            }

            // Controller Settings
            if (_controller != null)
            {
                style.normal.textColor = Color.cyan;
                GUI.Label(new Rect(20, yPos, 330, 20), $"Combo Window: {_controller.ComboWindow:F2}s", style);
                yPos += lineHeight;

                GUI.Label(new Rect(20, yPos, 330, 20), $"Attack Cooldown: {_controller.AttackCooldown:F2}s", style);
                yPos += lineHeight;

                GUI.Label(new Rect(20, yPos, 330, 20), $"Ultimate Cooldown: {_controller.UltimateCooldown:F2}s", style);
                yPos += lineHeight;
            }

            // Instructions
            yPos += 10;
            style.normal.textColor = Color.gray;
            style.fontSize = 12;
            GUI.Label(new Rect(20, yPos, 330, 20), "Left Click = Attack | R = Ultimate", style);
        }

        // Called from Animation Events to test
        public void OnAttackHit()
        {
            if (logToConsole)
            {
                Debug.Log($"[Attack Event] Hit frame at {Time.time:F2}s");
            }
        }

        public void OnUltimateHit()
        {
            if (logToConsole)
            {
                Debug.Log($"[Ultimate Event] Hit frame at {Time.time:F2}s");
            }
        }
    }
}
