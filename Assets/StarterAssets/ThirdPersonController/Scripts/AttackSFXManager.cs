using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// Manages sound effects for attack animations.
    /// Automatically plays slash sounds at the right timing during attacks.
    /// Works independently — add this component alongside ThirdPersonController.
    /// </summary>
    public class AttackSFXManager : MonoBehaviour
    {
        [Header("Slash Sound Clips")]
        [Tooltip("Sound for Attack 1 slash")]
        public AudioClip attack1SFX;

        [Tooltip("Sound for Attack 2 slash")]
        public AudioClip attack2SFX;

        [Tooltip("Sound for Attack 3 slash")]
        public AudioClip attack3SFX;

        [Header("Volume")]
        [Range(0f, 1f)]
        [Tooltip("Volume for slash sound effects")]
        public float slashVolume = 0.6f;

        [Header("Timing Settings")]
        [Tooltip("Normalized time (0-1) in Attack 1 animation when sound plays")]
        [Range(0f, 1f)]
        public float attack1PlayTime = 0.35f;

        [Tooltip("Normalized time (0-1) in Attack 2 animation when sound plays")]
        [Range(0f, 1f)]
        public float attack2PlayTime = 0.35f;

        [Tooltip("Normalized time (0-1) in Attack 3 animation when sound plays")]
        [Range(0f, 1f)]
        public float attack3PlayTime = 0.35f;

        [Header("Debug")]
        [Tooltip("Show debug logs when sounds play")]
        public bool showDebugLogs = false;

        private Animator _animator;
        private bool _attack1Played;
        private bool _attack2Played;
        private bool _attack3Played;

        private void Start()
        {
            _animator = GetComponent<Animator>();

            if (_animator == null)
            {
                Debug.LogError("AttackSFXManager: Animator component not found!");
            }
        }

        private void Update()
        {
            if (_animator == null) return;

            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            float t = state.normalizedTime % 1f;

            // Attack 1
            if (state.IsName("Attack_1"))
            {
                if (t >= attack1PlayTime && !_attack1Played)
                {
                    PlaySFX(attack1SFX, "Attack 1");
                    _attack1Played = true;
                }
                else if (t < attack1PlayTime)
                {
                    _attack1Played = false;
                }
            }
            else
            {
                _attack1Played = false;
            }

            // Attack 2
            if (state.IsName("Attack_2"))
            {
                if (t >= attack2PlayTime && !_attack2Played)
                {
                    PlaySFX(attack2SFX, "Attack 2");
                    _attack2Played = true;
                }
                else if (t < attack2PlayTime)
                {
                    _attack2Played = false;
                }
            }
            else
            {
                _attack2Played = false;
            }

            // Attack 3
            if (state.IsName("Attack_3"))
            {
                if (t >= attack3PlayTime && !_attack3Played)
                {
                    PlaySFX(attack3SFX, "Attack 3");
                    _attack3Played = true;
                }
                else if (t < attack3PlayTime)
                {
                    _attack3Played = false;
                }
            }
            else
            {
                _attack3Played = false;
            }
        }

        private void PlaySFX(AudioClip clip, string attackName)
        {
            if (clip == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"AttackSFXManager: No AudioClip assigned for {attackName}");
                return;
            }

            AudioSource.PlayClipAtPoint(clip, transform.position, slashVolume);

            if (showDebugLogs)
            {
                Debug.Log($"AttackSFXManager: Played {attackName} slash sound");
            }
        }
    }
}
