using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// Manages sound effects for attack animations.
    /// Automatically plays slash sounds and voice sounds at the right timing during attacks.
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

        [Tooltip("Sound for E Skill (Attack360) slash")]
        public AudioClip eSkillSFX;

        [Header("Voice Sound Clips")]
        [Tooltip("Voice sound for Attack 1 (character shout/grunt)")]
        public AudioClip attack1VoiceSFX;

        [Tooltip("Voice sound for Attack 2 (character shout/grunt)")]
        public AudioClip attack2VoiceSFX;

        [Tooltip("Voice sound for Attack 3 (character shout/grunt)")]
        public AudioClip attack3VoiceSFX;

        [Tooltip("Voice sound for E Skill (character shout/grunt)")]
        public AudioClip eSkillVoiceSFX;

        [Header("Volume")]
        [Range(0f, 1f)]
        [Tooltip("Volume for slash sound effects")]
        public float slashVolume = 0.6f;

        [Range(0f, 1f)]
        [Tooltip("Volume for voice sound effects")]
        public float voiceVolume = 0.8f;

        [Header("Slash Timing Settings")]
        [Tooltip("Normalized time (0-1) in Attack 1 animation when slash sound plays")]
        [Range(0f, 1f)]
        public float attack1PlayTime = 0.35f;

        [Tooltip("Normalized time (0-1) in Attack 2 animation when slash sound plays")]
        [Range(0f, 1f)]
        public float attack2PlayTime = 0.35f;

        [Tooltip("Normalized time (0-1) in Attack 3 animation when slash sound plays")]
        [Range(0f, 1f)]
        public float attack3PlayTime = 0.35f;

        [Tooltip("Normalized time (0-1) in E Skill animation when slash sound plays")]
        [Range(0f, 1f)]
        public float eSkillPlayTime = 0.3f;

        [Header("Voice Timing Settings")]
        [Tooltip("Normalized time (0-1) in Attack 1 animation when voice plays")]
        [Range(0f, 1f)]
        public float attack1VoicePlayTime = 0.1f;

        [Tooltip("Normalized time (0-1) in Attack 2 animation when voice plays")]
        [Range(0f, 1f)]
        public float attack2VoicePlayTime = 0.1f;

        [Tooltip("Normalized time (0-1) in Attack 3 animation when voice plays")]
        [Range(0f, 1f)]
        public float attack3VoicePlayTime = 0.1f;

        [Tooltip("Normalized time (0-1) in E Skill animation when voice plays")]
        [Range(0f, 1f)]
        public float eSkillVoicePlayTime = 0.05f;

        [Header("Debug")]
        [Tooltip("Show debug logs when sounds play")]
        public bool showDebugLogs = false;

        private Animator _animator;
        private bool _attack1Played;
        private bool _attack2Played;
        private bool _attack3Played;
        private bool _eSkillPlayed;
        private bool _attack1VoicePlayed;
        private bool _attack2VoicePlayed;
        private bool _attack3VoicePlayed;
        private bool _eSkillVoicePlayed;

        // Track whether we're already inside the E Skill state to prevent
        // re-triggering SFX when the animation loops (normalizedTime wraps around)
        private bool _eSkillStateEntered;

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
                // Voice
                if (t >= attack1VoicePlayTime && !_attack1VoicePlayed)
                {
                    PlaySFX(attack1VoiceSFX, "Attack 1 Voice", voiceVolume);
                    _attack1VoicePlayed = true;
                }
                else if (t < attack1VoicePlayTime)
                {
                    _attack1VoicePlayed = false;
                }

                // Slash
                if (t >= attack1PlayTime && !_attack1Played)
                {
                    PlaySFX(attack1SFX, "Attack 1 Slash", slashVolume);
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
                _attack1VoicePlayed = false;
            }

            // Attack 2
            if (state.IsName("Attack_2"))
            {
                // Voice
                if (t >= attack2VoicePlayTime && !_attack2VoicePlayed)
                {
                    PlaySFX(attack2VoiceSFX, "Attack 2 Voice", voiceVolume);
                    _attack2VoicePlayed = true;
                }
                else if (t < attack2VoicePlayTime)
                {
                    _attack2VoicePlayed = false;
                }

                // Slash
                if (t >= attack2PlayTime && !_attack2Played)
                {
                    PlaySFX(attack2SFX, "Attack 2 Slash", slashVolume);
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
                _attack2VoicePlayed = false;
            }

            // Attack 3
            if (state.IsName("Attack_3"))
            {
                // Voice
                if (t >= attack3VoicePlayTime && !_attack3VoicePlayed)
                {
                    PlaySFX(attack3VoiceSFX, "Attack 3 Voice", voiceVolume);
                    _attack3VoicePlayed = true;
                }
                else if (t < attack3VoicePlayTime)
                {
                    _attack3VoicePlayed = false;
                }

                // Slash
                if (t >= attack3PlayTime && !_attack3Played)
                {
                    PlaySFX(attack3SFX, "Attack 3 Slash", slashVolume);
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
                _attack3VoicePlayed = false;
            }

            // E Skill (Attack360)
            if (state.IsName("Attack360"))
            {
                if (!_eSkillStateEntered)
                {
                    // First frame entering the state — reset flags
                    _eSkillStateEntered = true;
                    _eSkillPlayed = false;
                    _eSkillVoicePlayed = false;
                }

                // Voice (play once per activation)
                if (t >= eSkillVoicePlayTime && !_eSkillVoicePlayed)
                {
                    PlaySFX(eSkillVoiceSFX, "E Skill Voice", voiceVolume);
                    _eSkillVoicePlayed = true;
                }

                // Slash (play once per activation)
                if (t >= eSkillPlayTime && !_eSkillPlayed)
                {
                    PlaySFX(eSkillSFX, "E Skill Slash", slashVolume);
                    _eSkillPlayed = true;
                }
            }
            else
            {
                _eSkillStateEntered = false;
                _eSkillPlayed = false;
                _eSkillVoicePlayed = false;
            }
        }

        private void PlaySFX(AudioClip clip, string sfxName, float volume)
        {
            if (clip == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"AttackSFXManager: No AudioClip assigned for {sfxName}");
                return;
            }

            AudioSource.PlayClipAtPoint(clip, transform.position, volume);

            if (showDebugLogs)
            {
                Debug.Log($"AttackSFXManager: Played {sfxName} sound");
            }
        }
    }
}
