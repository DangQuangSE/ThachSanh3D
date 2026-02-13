using UnityEngine;
using System.Collections.Generic;

namespace StarterAssets
{
    /// <summary>
    /// Advanced VFX Manager with multiple effects support
    /// Allows multiple VFX per attack, random selection, and more features
    /// </summary>
    public class AdvancedAttackVFXManager : MonoBehaviour
    {
        [System.Serializable]
        public class VFXEffect
        {
            [Tooltip("VFX prefab to spawn")]
            public GameObject prefab;
            
            [Tooltip("Normalized time (0-1) when this VFX spawns")]
            [Range(0f, 1f)]
            public float spawnTime = 0.4f;
            
            [Tooltip("Offset from spawn point")]
            public Vector3 positionOffset = Vector3.zero;
            
            [Tooltip("Rotation offset")]
            public Vector3 rotationOffset = Vector3.zero;
            
            [Tooltip("Scale multiplier")]
            public float scale = 1f;
            
            [Tooltip("Lifetime before destroying")]
            public float lifetime = 2f;
            
            [Tooltip("Parent VFX to spawn point (VFX will follow weapon)")]
            public bool parentToSpawnPoint = false;
        }

        [System.Serializable]
        public class AttackVFXGroup
        {
            [Tooltip("List of VFX effects for this attack")]
            public List<VFXEffect> effects = new List<VFXEffect>();
            
            [Tooltip("Play effects in sequence or all at once")]
            public bool playInSequence = false;
            
            [Tooltip("Randomly pick one effect from the list")]
            public bool randomSelection = false;
        }

        [Header("VFX Groups")]
        [Tooltip("VFX effects for Attack 1")]
        public AttackVFXGroup attack1VFXGroup = new AttackVFXGroup();
        
        [Tooltip("VFX effects for Attack 2")]
        public AttackVFXGroup attack2VFXGroup = new AttackVFXGroup();
        
        [Tooltip("VFX effects for Attack 3")]
        public AttackVFXGroup attack3VFXGroup = new AttackVFXGroup();
        
        [Tooltip("VFX effects for Ultimate")]
        public AttackVFXGroup ultimateVFXGroup = new AttackVFXGroup();

        [Header("Spawn Points")]
        [Tooltip("Primary spawn point (weapon tip)")]
        public Transform primarySpawnPoint;
        
        [Tooltip("Additional spawn points (optional)")]
        public Transform[] additionalSpawnPoints;
        
        [Tooltip("Use all spawn points or just primary")]
        public bool useAllSpawnPoints = false;

        [Header("Audio (Optional)")]
        [Tooltip("Audio clip for Attack 1")]
        public AudioClip attack1Audio;
        
        [Tooltip("Audio clip for Attack 2")]
        public AudioClip attack2Audio;
        
        [Tooltip("Audio clip for Attack 3")]
        public AudioClip attack3Audio;
        
        [Tooltip("Audio clip for Ultimate")]
        public AudioClip ultimateAudio;
        
        [Range(0f, 1f)]
        public float audioVolume = 0.5f;

        [Header("Advanced Settings")]
        [Tooltip("Enable screen shake on attack")]
        public bool enableScreenShake = false;
        
        [Tooltip("Screen shake intensity")]
        public float shakeIntensity = 0.1f;
        
        [Tooltip("Screen shake duration")]
        public float shakeDuration = 0.2f;

        [Header("Debug")]
        public bool showDebugLogs = false;
        public bool showSpawnPointGizmos = true;

        // Private variables
        private Animator _animator;
        private Dictionary<string, bool> _effectsSpawned = new Dictionary<string, bool>();

        private void Start()
        {
            _animator = GetComponent<Animator>();
            
            if (_animator == null)
            {
                Debug.LogError("AdvancedAttackVFXManager: Animator not found!");
            }
            
            if (primarySpawnPoint == null)
            {
                Debug.LogWarning("AdvancedAttackVFXManager: Primary spawn point not set!");
                primarySpawnPoint = transform;
            }
        }

        private void Update()
        {
            if (_animator == null) return;

            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = currentState.normalizedTime % 1f;

            // Process Attack 1
            if (currentState.IsName("Attack_1"))
            {
                ProcessAttackVFX("Attack1", attack1VFXGroup, normalizedTime, attack1Audio);
            }
            else
            {
                ResetAttackFlags("Attack1");
            }

            // Process Attack 2
            if (currentState.IsName("Attack_2"))
            {
                ProcessAttackVFX("Attack2", attack2VFXGroup, normalizedTime, attack2Audio);
            }
            else
            {
                ResetAttackFlags("Attack2");
            }

            // Process Attack 3
            if (currentState.IsName("Attack_3"))
            {
                ProcessAttackVFX("Attack3", attack3VFXGroup, normalizedTime, attack3Audio);
            }
            else
            {
                ResetAttackFlags("Attack3");
            }

            // Process Ultimate
            if (currentState.IsName("UntimateAttack") || currentState.IsName("UntimateAttack_1"))
            {
                ProcessAttackVFX("Ultimate", ultimateVFXGroup, normalizedTime, ultimateAudio);
            }
            else
            {
                ResetAttackFlags("Ultimate");
            }
        }

        private void ProcessAttackVFX(string attackName, AttackVFXGroup vfxGroup, float normalizedTime, AudioClip audioClip)
        {
            if (vfxGroup.effects == null || vfxGroup.effects.Count == 0)
                return;

            if (vfxGroup.randomSelection)
            {
                // Random selection - spawn one random effect
                string key = $"{attackName}_Random";
                if (!_effectsSpawned.ContainsKey(key) || !_effectsSpawned[key])
                {
                    VFXEffect effect = vfxGroup.effects[Random.Range(0, vfxGroup.effects.Count)];
                    if (normalizedTime >= effect.spawnTime)
                    {
                        SpawnVFX(effect, attackName);
                        PlayAudio(audioClip);
                        if (enableScreenShake) TriggerScreenShake();
                        _effectsSpawned[key] = true;
                    }
                }
                if (normalizedTime < 0.1f) _effectsSpawned[key] = false;
            }
            else
            {
                // Spawn all effects at their designated times
                for (int i = 0; i < vfxGroup.effects.Count; i++)
                {
                    VFXEffect effect = vfxGroup.effects[i];
                    string key = $"{attackName}_Effect{i}";
                    
                    if (!_effectsSpawned.ContainsKey(key) || !_effectsSpawned[key])
                    {
                        if (normalizedTime >= effect.spawnTime)
                        {
                            SpawnVFX(effect, $"{attackName} Effect {i}");
                            
                            // Only play audio and shake once (for first effect)
                            if (i == 0)
                            {
                                PlayAudio(audioClip);
                                if (enableScreenShake) TriggerScreenShake();
                            }
                            
                            _effectsSpawned[key] = true;
                        }
                    }
                    
                    // Reset flag when animation loops
                    if (normalizedTime < effect.spawnTime - 0.1f)
                    {
                        _effectsSpawned[key] = false;
                    }
                }
            }
        }

        private void SpawnVFX(VFXEffect effect, string debugName)
        {
            if (effect.prefab == null) return;

            // Get spawn points to use
            List<Transform> spawnPoints = new List<Transform> { primarySpawnPoint };
            
            if (useAllSpawnPoints && additionalSpawnPoints != null)
            {
                spawnPoints.AddRange(additionalSpawnPoints);
            }

            // Spawn VFX at each spawn point
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint == null) continue;

                Vector3 spawnPos = spawnPoint.position + spawnPoint.TransformDirection(effect.positionOffset);
                Quaternion spawnRot = spawnPoint.rotation * Quaternion.Euler(effect.rotationOffset);

                GameObject vfx = Instantiate(effect.prefab, spawnPos, spawnRot);
                vfx.transform.localScale = Vector3.one * effect.scale;

                if (effect.parentToSpawnPoint)
                {
                    vfx.transform.SetParent(spawnPoint);
                }

                Destroy(vfx, effect.lifetime);

                if (showDebugLogs)
                {
                    Debug.Log($"AdvancedVFXManager: Spawned {debugName} at {spawnPos}");
                }
            }
        }

        private void PlayAudio(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, audioVolume);
            }
        }

        private void TriggerScreenShake()
        {
            // Find camera shake controller if exists
            // This is a placeholder - implement your own camera shake
            if (showDebugLogs)
            {
                Debug.Log("AdvancedVFXManager: Screen shake triggered!");
            }
        }

        private void ResetAttackFlags(string attackName)
        {
            List<string> keysToRemove = new List<string>();
            
            foreach (var key in _effectsSpawned.Keys)
            {
                if (key.StartsWith(attackName))
                {
                    keysToRemove.Add(key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _effectsSpawned.Remove(key);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showSpawnPointGizmos) return;

            // Draw primary spawn point
            if (primarySpawnPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(primarySpawnPoint.position, 0.15f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(primarySpawnPoint.position, primarySpawnPoint.forward * 0.5f);
            }

            // Draw additional spawn points
            if (additionalSpawnPoints != null && useAllSpawnPoints)
            {
                Gizmos.color = Color.cyan;
                foreach (Transform spawnPoint in additionalSpawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 0.1f);
                        Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 0.3f);
                    }
                }
            }
        }
    }
}
