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

        [Header("Ultimate Skill Sound Clips")]
        [Tooltip("Voice/shout during ultimate (plays early so peak aligns with axe slam)")]
        public AudioClip ultimateVoiceSFX;

        [Tooltip("Axe slam impact sound (plays when player slams axe into ground)")]
        public AudioClip ultimateAxeSlamSFX;

        [Tooltip("Ice cracking sound (plays when ice emerges from ground at boss position)")]
        public AudioClip ultimateIceCrackingSFX;

        [Header("Roll Sound Clip")]
        [Tooltip("Sound for dodge roll (plays when roll animation starts)")]
        public AudioClip rollSFX;

        [Header("Volume")]
        [Range(0f, 1f)]
        [Tooltip("Volume for slash sound effects")]
        public float slashVolume = 0.6f;

        [Range(0f, 1f)]
        [Tooltip("Volume for voice sound effects")]
        public float voiceVolume = 0.8f;

        [Range(0f, 1f)]
        [Tooltip("Volume for ultimate voice")]
        public float ultimateVoiceVolume = 0.9f;

        [Range(0f, 1f)]
        [Tooltip("Volume for axe slam impact")]
        public float ultimateAxeSlamVolume = 0.8f;

        [Range(0f, 1f)]
        [Tooltip("Volume for ice cracking effect")]
        public float ultimateIceCrackingVolume = 0.7f;

        [Range(0f, 1f)]
        [Tooltip("Volume for roll/dodge sound")]
        public float rollVolume = 0.6f;

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

        [Header("Ultimate Timing Settings")]
        [Tooltip("Normalized time (0-1) when ultimate voice/shout plays (early, so peak aligns with slam)")]
        [Range(0f, 1f)]
        public float ultimateVoicePlayTime = 0.15f;

        [Tooltip("Normalized time (0-1) when axe slam sound plays (moment player hits ground)")]
        [Range(0f, 1f)]
        public float ultimateAxeSlamPlayTime = 0.35f;

        [Tooltip("Normalized time (0-1) when ice cracking sound plays (ice emerges from ground, slightly after slam)")]
        [Range(0f, 1f)]
        public float ultimateIceCrackingPlayTime = 0.5f;

        [Header("Roll Timing Settings")]
        [Tooltip("Normalized time (0-1) in Roll animation when dodge sound plays")]
        [Range(0f, 1f)]
        public float rollPlayTime = 0.05f;

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
        private bool _ultimateVoicePlayed;
        private bool _ultimateAxeSlamPlayed;
        private bool _ultimateIceCrackingPlayed;
        private bool _rollPlayed;

        private bool _eSkillStateEntered;
        private bool _ultimateStateEntered;
        private bool _rollStateEntered;

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

            // Ultimate Skill (UntimateAttack / UntimateAttack_1)
            if (state.IsName("UntimateAttack") || state.IsName("UntimateAttack_1"))
            {
                if (!_ultimateStateEntered)
                {
                    _ultimateStateEntered = true;
                    _ultimateVoicePlayed = false;
                    _ultimateAxeSlamPlayed = false;
                    _ultimateIceCrackingPlayed = false;
                }

                // (1) Voice/shout — plays early so the peak of the shout aligns with the axe slam moment
                if (t >= ultimateVoicePlayTime && !_ultimateVoicePlayed)
                {
                    PlaySFX(ultimateVoiceSFX, "Ultimate Voice", ultimateVoiceVolume);
                    _ultimateVoicePlayed = true;
                }

                // (2) Axe slam — plays at the exact moment player slams axe into ground
                if (t >= ultimateAxeSlamPlayTime && !_ultimateAxeSlamPlayed)
                {
                    PlaySFX(ultimateAxeSlamSFX, "Ultimate Axe Slam", ultimateAxeSlamVolume);
                    _ultimateAxeSlamPlayed = true;
                }

                // (3) Ice cracking — plays slightly after slam when ice emerges from ground at boss
                if (t >= ultimateIceCrackingPlayTime && !_ultimateIceCrackingPlayed)
                {
                    PlaySFX(ultimateIceCrackingSFX, "Ultimate Ice Cracking", ultimateIceCrackingVolume);
                    _ultimateIceCrackingPlayed = true;
                }
            }
            else
            {
                _ultimateStateEntered = false;
                _ultimateVoicePlayed = false;
                _ultimateAxeSlamPlayed = false;
                _ultimateIceCrackingPlayed = false;
            }

            // Roll
            if (state.IsName("Roll"))
            {
                if (!_rollStateEntered)
                {
                    _rollStateEntered = true;
                    _rollPlayed = false;
                }

                if (t >= rollPlayTime && !_rollPlayed)
                {
                    PlaySFX(rollSFX, "Roll", rollVolume);
                    _rollPlayed = true;
                }
            }
            else
            {
                _rollStateEntered = false;
                _rollPlayed = false;
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
