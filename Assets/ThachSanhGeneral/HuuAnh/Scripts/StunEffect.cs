using UnityEngine;

/// <summary>
/// Reusable stun effect — disables player input/movement for a duration, then auto-removes.
/// No modifications to player scripts required.
/// 
/// Usage:  StunEffect.Apply(targetGameObject, duration);
///         StunEffect.Remove(targetGameObject);
///         StunEffect.IsStunned(targetGameObject);
/// </summary>
public class StunEffect : MonoBehaviour
{
    private float _remainingDuration;
    private float _originalAnimatorSpeed;
    private MonoBehaviour _inputComponent;
    private Animator _animator;
    private bool _inputWasEnabled;
    private bool _tpcWasEnabled;

    public static StunEffect Apply(GameObject target, float duration)
    {
        if (target == null || duration <= 0f) return null;

        StunEffect existing = target.GetComponent<StunEffect>();
        if (existing != null)
        {
            if (duration > existing._remainingDuration)
                existing._remainingDuration = duration;
            return existing;
        }

        StunEffect stun = target.AddComponent<StunEffect>();
        stun._remainingDuration = duration;
        stun.BeginStun();
        return stun;
    }

    public static void Remove(GameObject target)
    {
        if (target == null) return;
        StunEffect existing = target.GetComponent<StunEffect>();
        if (existing != null)
            existing.EndStun();
    }

    public static bool IsStunned(GameObject target)
    {
        if (target == null) return false;
        return target.GetComponent<StunEffect>() != null;
    }

    public float GetRemainingDuration() => _remainingDuration;

    private void BeginStun()
    {
        _inputComponent = FindInputComponent();
        if (_inputComponent != null)
        {
            _inputWasEnabled = _inputComponent.enabled;
            _inputComponent.enabled = false;
        }

        _animator = GetComponentInChildren<Animator>();
        if (_animator != null)
        {
            _originalAnimatorSpeed = _animator.speed;
            _animator.speed = 0f;
        }

        var tpc = GetComponent<StarterAssets.ThirdPersonController>();
        if (tpc != null && tpc.enabled)
        {
            tpc.enabled = false;
            _tpcWasEnabled = true;
        }
    }

    private void EndStun()
    {
        if (_inputComponent != null)
            _inputComponent.enabled = _inputWasEnabled;

        if (_animator != null)
            _animator.speed = _originalAnimatorSpeed;

        if (_tpcWasEnabled)
        {
            var tpc = GetComponent<StarterAssets.ThirdPersonController>();
            if (tpc != null)
                tpc.enabled = true;
        }

        Destroy(this);
    }

    private void Update()
    {
        _remainingDuration -= Time.deltaTime;
        if (_remainingDuration <= 0f)
            EndStun();
    }

    private void OnDestroy()
    {
        if (_inputComponent != null && !_inputComponent.enabled && _inputWasEnabled)
            _inputComponent.enabled = true;

        if (_animator != null && _animator.speed == 0f)
            _animator.speed = _originalAnimatorSpeed;

        if (_tpcWasEnabled)
        {
            var tpc = GetComponent<StarterAssets.ThirdPersonController>();
            if (tpc != null && !tpc.enabled)
                tpc.enabled = true;
        }
    }

    private MonoBehaviour FindInputComponent()
    {
        var starterInput = GetComponent<StarterAssets.StarterAssetsInputs>();
        if (starterInput != null) return starterInput;

        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null || components[i] == this) continue;
            if (components[i].GetType().Name.IndexOf("Input", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return components[i];
        }
        return null;
    }
}
