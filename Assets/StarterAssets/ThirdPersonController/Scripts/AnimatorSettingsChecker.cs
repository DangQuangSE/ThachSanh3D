using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace StarterAssets
{
    /// <summary>
    /// Script ki?m tra Animator settings ?? debug combo attack
    /// Ch?y script này t? Unity Editor: Tools > Check Animator Settings
    /// </summary>
    public class AnimatorSettingsChecker : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("Tools/Check Animator Settings")]
        public static void CheckAnimatorSettings()
        {
            // Tìm Player GameObject
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player == null)
            {
                Debug.LogError("? KHÔNG TÌM TH?Y PLAYER! Hãy tag GameObject là 'Player'");
                return;
            }

            Animator animator = player.GetComponent<Animator>();
            
            if (animator == null)
            {
                Debug.LogError("? Player không có Animator component!");
                return;
            }

            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError("? Animator không có Controller!");
                return;
            }

            Debug.Log("=== ?? B?T ??U KI?M TRA ANIMATOR SETTINGS ===\n");

            AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
            
            if (controller == null)
            {
                Debug.LogError("? Không th? cast sang AnimatorController!");
                return;
            }

            // Check Parameters
            CheckParameters(controller);
            
            // Check States
            CheckStates(controller);
            
            // Check Transitions
            CheckTransitions(controller);
            
            // Check Root Motion
            CheckRootMotion(animator);

            Debug.Log("\n=== ? HOÀN THÀNH KI?M TRA ===");
        }

        private static void CheckParameters(AnimatorController controller)
        {
            Debug.Log("\n?? KI?M TRA PARAMETERS:");
            Debug.Log("????????????????????????????????????????");

            string[] requiredParams = { "Attack1", "Attack2", "Attack3" };
            bool allParamsExist = true;

            foreach (var paramName in requiredParams)
            {
                var param = System.Array.Find(controller.parameters, p => p.name == paramName);
                
                if (param == null)
                {
                    Debug.LogError($"? THI?U Parameter: {paramName}");
                    allParamsExist = false;
                }
                else
                {
                    if (param.type == AnimatorControllerParameterType.Trigger)
                    {
                        Debug.Log($"? {paramName}: Trigger (?ÚNG)");
                    }
                    else
                    {
                        Debug.LogError($"? {paramName}: {param.type} (SAI - ph?i là Trigger!)");
                        allParamsExist = false;
                    }
                }
            }

            if (allParamsExist)
            {
                Debug.Log("? T?T C? PARAMETERS ?ÚNG!");
            }
        }

        private static void CheckStates(AnimatorController controller)
        {
            Debug.Log("\n?? KI?M TRA STATES:");
            Debug.Log("????????????????????????????????????????");

            var layer = controller.layers[0];
            var stateMachine = layer.stateMachine;

            string[] requiredStates = { "Attack_1", "Attack_2", "Attack_3" };
            bool allStatesExist = true;

            foreach (var stateName in requiredStates)
            {
                var state = System.Array.Find(stateMachine.states, s => s.state.name == stateName);
                
                if (state.state == null)
                {
                    Debug.LogError($"? THI?U State: {stateName}");
                    allStatesExist = false;
                }
                else
                {
                    if (state.state.motion != null)
                    {
                        Debug.Log($"? {stateName}: Có animation '{state.state.motion.name}'");
                    }
                    else
                    {
                        Debug.LogWarning($"?? {stateName}: CH?A GÁN ANIMATION!");
                    }
                }
            }

            if (allStatesExist)
            {
                Debug.Log("? T?T C? STATES T?N T?I!");
            }
        }

        private static void CheckTransitions(AnimatorController controller)
        {
            Debug.Log("\n?? KI?M TRA TRANSITIONS:");
            Debug.Log("????????????????????????????????????????");

            var layer = controller.layers[0];
            var stateMachine = layer.stateMachine;

            // Find states
            var entryState = stateMachine.defaultState;
            var attack1 = System.Array.Find(stateMachine.states, s => s.state.name == "Attack_1");
            var attack2 = System.Array.Find(stateMachine.states, s => s.state.name == "Attack_2");
            var attack3 = System.Array.Find(stateMachine.states, s => s.state.name == "Attack_3");

            Debug.Log($"\n?? Default State: {entryState?.name ?? "KHÔNG CÓ"}");

            // Check Entry ? Attack_1
            if (entryState != null)
            {
                CheckTransition(entryState, "Entry", attack1.state, "Attack_1", 
                    hasExitTime: false, 
                    exitTime: 0f, 
                    duration: 0.05f,
                    interruptionSource: TransitionInterruptionSource.None,
                    hasCondition: true,
                    conditionParam: "Attack1");
            }

            // Check Attack_1 ? Attack_2
            if (attack1.state != null && attack2.state != null)
            {
                CheckTransition(attack1.state, "Attack_1", attack2.state, "Attack_2",
                    hasExitTime: true,
                    exitTime: 0.5f,
                    duration: 0.1f,
                    interruptionSource: TransitionInterruptionSource.SourceThenDestination,
                    hasCondition: true,
                    conditionParam: "Attack2");
            }

            // Check Attack_2 ? Attack_3
            if (attack2.state != null && attack3.state != null)
            {
                CheckTransition(attack2.state, "Attack_2", attack3.state, "Attack_3",
                    hasExitTime: true,
                    exitTime: 0.5f,
                    duration: 0.1f,
                    interruptionSource: TransitionInterruptionSource.SourceThenDestination,
                    hasCondition: true,
                    conditionParam: "Attack3");
            }

            // Check Attack ? Idle transitions
            CheckReturnToIdle(attack1.state, "Attack_1", entryState);
            CheckReturnToIdle(attack2.state, "Attack_2", entryState);
            CheckReturnToIdle(attack3.state, "Attack_3", entryState);
        }

        private static void CheckTransition(
            AnimatorState fromState, 
            string fromName,
            AnimatorState toState, 
            string toName,
            bool hasExitTime,
            float exitTime,
            float duration,
            TransitionInterruptionSource interruptionSource,
            bool hasCondition,
            string conditionParam = "")
        {
            Debug.Log($"\n? Ki?m tra: {fromName} ? {toName}");

            if (fromState == null || toState == null)
            {
                Debug.LogError($"? State không t?n t?i!");
                return;
            }

            var transition = System.Array.Find(fromState.transitions, 
                t => t.destinationState == toState);

            if (transition == null)
            {
                Debug.LogError($"? THI?U TRANSITION t? {fromName} ? {toName}");
                return;
            }

            bool allCorrect = true;

            // Check Has Exit Time
            if (transition.hasExitTime != hasExitTime)
            {
                Debug.LogError($"  ? Has Exit Time: {transition.hasExitTime} (C?n: {hasExitTime})");
                allCorrect = false;
            }
            else
            {
                Debug.Log($"  ? Has Exit Time: {transition.hasExitTime}");
            }

            // Check Exit Time
            if (hasExitTime)
            {
                float diff = Mathf.Abs(transition.exitTime - exitTime);
                if (diff > 0.1f)
                {
                    Debug.LogWarning($"  ?? Exit Time: {transition.exitTime:F2} (Khuy?n ngh?: {exitTime:F2})");
                }
                else
                {
                    Debug.Log($"  ? Exit Time: {transition.exitTime:F2}");
                }
            }

            // Check Duration
            float durationDiff = Mathf.Abs(transition.duration - duration);
            if (durationDiff > 0.05f)
            {
                Debug.LogWarning($"  ?? Transition Duration: {transition.duration:F2} (Khuy?n ngh?: {duration:F2})");
            }
            else
            {
                Debug.Log($"  ? Transition Duration: {transition.duration:F2}");
            }

            // Check Interruption Source
            if (transition.interruptionSource != interruptionSource)
            {
                Debug.LogError($"  ? Interruption Source: {transition.interruptionSource} (C?n: {interruptionSource})");
                allCorrect = false;
            }
            else
            {
                Debug.Log($"  ? Interruption Source: {transition.interruptionSource}");
            }

            // Check Ordered Interruption
            if (interruptionSource == TransitionInterruptionSource.SourceThenDestination)
            {
                if (!transition.orderedInterruption)
                {
                    Debug.LogError($"  ? Ordered Interruption: {transition.orderedInterruption} (C?n: true)");
                    allCorrect = false;
                }
                else
                {
                    Debug.Log($"  ? Ordered Interruption: {transition.orderedInterruption}");
                }
            }

            // Check Conditions
            if (hasCondition)
            {
                if (transition.conditions.Length == 0)
                {
                    Debug.LogError($"  ? THI?U Condition: {conditionParam}");
                    allCorrect = false;
                }
                else
                {
                    bool hasCorrectCondition = System.Array.Exists(transition.conditions, 
                        c => c.parameter == conditionParam);
                    
                    if (hasCorrectCondition)
                    {
                        Debug.Log($"  ? Condition: {conditionParam}");
                    }
                    else
                    {
                        Debug.LogError($"  ? Condition sai: {transition.conditions[0].parameter} (C?n: {conditionParam})");
                        allCorrect = false;
                    }
                }
            }

            if (allCorrect)
            {
                Debug.Log($"  ??? TRANSITION {fromName} ? {toName} HOÀN TOÀN ?ÚNG!");
            }
        }

        private static void CheckReturnToIdle(AnimatorState attackState, string attackName, AnimatorState idleState)
        {
            if (attackState == null || idleState == null) return;

            Debug.Log($"\n? Ki?m tra: {attackName} ? Idle");

            var transition = System.Array.Find(attackState.transitions, 
                t => t.destinationState == idleState || t.isExit);

            if (transition == null)
            {
                Debug.LogWarning($"  ?? KHÔNG CÓ transition v? Idle (có th? dùng Exit)");
                return;
            }

            // Check Has Exit Time
            if (!transition.hasExitTime)
            {
                Debug.LogError($"  ? Has Exit Time: false (C?n: true)");
            }
            else
            {
                Debug.Log($"  ? Has Exit Time: true");
            }

            // Check Exit Time
            if (transition.exitTime < 0.9f)
            {
                Debug.LogWarning($"  ?? Exit Time: {transition.exitTime:F2} (Khuy?n ngh?: 0.9-0.95)");
            }
            else
            {
                Debug.Log($"  ? Exit Time: {transition.exitTime:F2}");
            }

            // Check No Conditions
            if (transition.conditions.Length > 0)
            {
                Debug.LogWarning($"  ?? Có {transition.conditions.Length} conditions (Khuy?n ngh?: Không có)");
            }
            else
            {
                Debug.Log($"  ? No Conditions (t? ??ng)");
            }
        }

        private static void CheckRootMotion(Animator animator)
        {
            Debug.Log("\n?? KI?M TRA ROOT MOTION:");
            Debug.Log("????????????????????????????????????????");

            if (animator.applyRootMotion)
            {
                Debug.LogError("? Apply Root Motion: TRUE (C?n: FALSE vì dùng CharacterController)");
            }
            else
            {
                Debug.Log("? Apply Root Motion: FALSE (?ÚNG!)");
            }

            Debug.Log($"  Update Mode: {animator.updateMode}");
            Debug.Log($"  Culling Mode: {animator.cullingMode}");
        }
#endif
    }
}
