using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// Manages VFX effects for attack animations
    /// Automatically spawns slash effects at the right timing during attacks
    /// Works with Mixamo/Unity animations without needing Animation Events
    /// </summary>
    public class AttackVFXManager : MonoBehaviour
    {
        [Header("VFX Prefabs - Slash Effects")]
        [Tooltip("VFX prefab for Attack 1 (first swing)")]
        public GameObject attack1VFX;
        
        [Tooltip("VFX prefab for Attack 2 (second swing)")]
        public GameObject attack2VFX;
        
        [Tooltip("VFX prefab for Attack 3 (third swing)")]
        public GameObject attack3VFX;
        
        [Tooltip("VFX prefab for Ultimate Attack")]
        public GameObject ultimateVFX;

        [Header("VFX Spawn Settings")]
        [Tooltip("Transform where VFX will spawn (usually weapon tip or hand)")]
        public Transform vfxSpawnPoint;
        
        [Tooltip("Global offset position from spawn point")]
        public Vector3 spawnOffset = new Vector3(0f, 0f, 0.5f);
        
        [Tooltip("VFX scale multiplier")]
        public float vfxScale = 1f;
        
        [Tooltip("Time to destroy VFX after spawning")]
        public float vfxLifetime = 2f;

        [Header("VFX Rotation Per Attack")]
        [Tooltip("Rotation offset for Attack 1 (X=pitch, Y=yaw, Z=roll)")]
        public Vector3 attack1RotationOffset = Vector3.zero;
        
        [Tooltip("Rotation offset for Attack 2")]
        public Vector3 attack2RotationOffset = Vector3.zero;
        
        [Tooltip("Rotation offset for Attack 3")]
        public Vector3 attack3RotationOffset = Vector3.zero;
        
        [Tooltip("Rotation offset for Ultimate")]
        public Vector3 ultimateRotationOffset = Vector3.zero;

        [Header("VFX Playback Settings")]
        [Tooltip("Auto-play particle systems on spawn (enable if VFX doesn't show)")]
        public bool autoPlayParticleSystems = true;
        
        [Tooltip("Use weapon rotation or world rotation")]
        public bool useWeaponRotation = true;

        [Header("Timing Settings")]
        [Tooltip("Normalized time (0-1) in Attack 1 animation when VFX spawns")]
        [Range(0f, 1f)]
        public float attack1SpawnTime = 0.4f;
        
        [Tooltip("Normalized time (0-1) in Attack 2 animation when VFX spawns")]
        [Range(0f, 1f)]
        public float attack2SpawnTime = 0.4f;
        
        [Tooltip("Normalized time (0-1) in Attack 3 animation when VFX spawns")]
        [Range(0f, 1f)]
        public float attack3SpawnTime = 0.4f;
        
        [Tooltip("Normalized time (0-1) in Ultimate animation when VFX spawns")]
        [Range(0f, 1f)]
        public float ultimateSpawnTime = 0.5f;

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebugLogs = false;
        
        [Tooltip("Show detailed VFX spawn info")]
        public bool showDetailedDebug = false;
        
        [Tooltip("Show rotation gizmos in Scene view")]
        public bool showRotationGizmos = true;

        // Private variables
        private Animator _animator;
        private bool _attack1VFXSpawned = false;
        private bool _attack2VFXSpawned = false;
        private bool _attack3VFXSpawned = false;
        private bool _ultimateVFXSpawned = false;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            
            if (_animator == null)
            {
                Debug.LogError("AttackVFXManager: Animator component not found!");
            }
            
            if (vfxSpawnPoint == null)
            {
                Debug.LogWarning("AttackVFXManager: VFX Spawn Point not set! Using player position as fallback.");
                vfxSpawnPoint = transform;
            }

            // Validate VFX prefabs at start
            if (showDetailedDebug)
            {
                ValidateVFXPrefab(attack1VFX, "Attack 1");
                ValidateVFXPrefab(attack2VFX, "Attack 2");
                ValidateVFXPrefab(attack3VFX, "Attack 3");
                ValidateVFXPrefab(ultimateVFX, "Ultimate");
            }
        }

        private void Update()
        {
            if (_animator == null) return;

            // Get current animation state
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = currentState.normalizedTime % 1f;

            // Check Attack 1
            if (currentState.IsName("Attack_1"))
            {
                if (normalizedTime >= attack1SpawnTime && !_attack1VFXSpawned)
                {
                    SpawnVFX(attack1VFX, attack1RotationOffset, "Attack 1");
                    _attack1VFXSpawned = true;
                }
                else if (normalizedTime < attack1SpawnTime)
                {
                    _attack1VFXSpawned = false;
                }
            }
            else
            {
                _attack1VFXSpawned = false;
            }

            // Check Attack 2
            if (currentState.IsName("Attack_2"))
            {
                if (normalizedTime >= attack2SpawnTime && !_attack2VFXSpawned)
                {
                    SpawnVFX(attack2VFX, attack2RotationOffset, "Attack 2");
                    _attack2VFXSpawned = true;
                }
                else if (normalizedTime < attack2SpawnTime)
                {
                    _attack2VFXSpawned = false;
                }
            }
            else
            {
                _attack2VFXSpawned = false;
            }

            // Check Attack 3
            if (currentState.IsName("Attack_3"))
            {
                if (normalizedTime >= attack3SpawnTime && !_attack3VFXSpawned)
                {
                    SpawnVFX(attack3VFX, attack3RotationOffset, "Attack 3");
                    _attack3VFXSpawned = true;
                }
                else if (normalizedTime < attack3SpawnTime)
                {
                    _attack3VFXSpawned = false;
                }
            }
            else
            {
                _attack3VFXSpawned = false;
            }

            // Check Ultimate Attack
            if (currentState.IsName("UntimateAttack") || currentState.IsName("UntimateAttack_1"))
            {
                if (normalizedTime >= ultimateSpawnTime && !_ultimateVFXSpawned)
                {
                    SpawnVFX(ultimateVFX, ultimateRotationOffset, "Ultimate Attack");
                    _ultimateVFXSpawned = true;
                }
                else if (normalizedTime < ultimateSpawnTime)
                {
                    _ultimateVFXSpawned = false;
                }
            }
            else
            {
                _ultimateVFXSpawned = false;
            }
        }

        /// <summary>
        /// Spawns VFX at the spawn point with custom rotation
        /// </summary>
        private void SpawnVFX(GameObject vfxPrefab, Vector3 rotationOffset, string attackName)
        {
            if (vfxPrefab == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"AttackVFXManager: No VFX prefab assigned for {attackName}");
                return;
            }

            // Calculate spawn position
            Vector3 spawnPosition = vfxSpawnPoint.position + vfxSpawnPoint.TransformDirection(spawnOffset);
            
            // Calculate spawn rotation based on settings
            Quaternion spawnRotation;
            if (useWeaponRotation)
            {
                // Use weapon rotation + offset
                spawnRotation = vfxSpawnPoint.rotation * Quaternion.Euler(rotationOffset);
            }
            else
            {
                // Use world rotation + offset
                spawnRotation = Quaternion.Euler(rotationOffset);
            }

            // Spawn VFX
            GameObject vfxInstance = Instantiate(vfxPrefab, spawnPosition, spawnRotation);
            
            // Apply scale
            vfxInstance.transform.localScale = Vector3.one * vfxScale;

            // IMPORTANT: Play all particle systems manually
            if (autoPlayParticleSystems)
            {
                PlayAllParticleSystems(vfxInstance);
            }

            // Destroy after lifetime
            Destroy(vfxInstance, vfxLifetime);

            if (showDebugLogs)
            {
                Debug.Log($"AttackVFXManager: Spawned VFX for {attackName} at {spawnPosition} with rotation {spawnRotation.eulerAngles}");
            }

            if (showDetailedDebug)
            {
                DebugVFXInstance(vfxInstance, attackName);
            }
        }

        /// <summary>
        /// Play all particle systems in the VFX GameObject
        /// This fixes the issue where VFX doesn't show when spawned at runtime
        /// </summary>
        private void PlayAllParticleSystems(GameObject vfxObject)
        {
            // Get all particle systems in this object and its children
            ParticleSystem[] particleSystems = vfxObject.GetComponentsInChildren<ParticleSystem>();
            
            if (particleSystems.Length == 0)
            {
                if (showDetailedDebug)
                    Debug.LogWarning($"AttackVFXManager: No ParticleSystem found in {vfxObject.name}. VFX might not show!");
                return;
            }

            // Play each particle system
            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps != null)
                {
                    // Stop first to reset, then play
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.Play(true);

                    if (showDetailedDebug)
                    {
                        Debug.Log($"AttackVFXManager: Playing ParticleSystem '{ps.name}' (IsPlaying: {ps.isPlaying}, IsEmitting: {ps.isEmitting})");
                    }
                }
            }
        }

        /// <summary>
        /// Validate VFX prefab structure
        /// </summary>
        private void ValidateVFXPrefab(GameObject prefab, string attackName)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"AttackVFXManager: {attackName} VFX prefab is NULL!");
                return;
            }

            // Check for Particle Systems
            ParticleSystem[] particleSystems = prefab.GetComponentsInChildren<ParticleSystem>();
            
            if (particleSystems.Length == 0)
            {
                Debug.LogWarning($"AttackVFXManager: {attackName} VFX '{prefab.name}' has NO ParticleSystem components!");
            }
            else
            {
                Debug.Log($"AttackVFXManager: {attackName} VFX '{prefab.name}' has {particleSystems.Length} ParticleSystem(s)");
                
                // Check each particle system settings
                foreach (ParticleSystem ps in particleSystems)
                {
                    var main = ps.main;
                    Debug.Log($"  - {ps.name}: PlayOnAwake={main.playOnAwake}, Loop={main.loop}, Duration={main.duration}s");
                }
            }
        }

        /// <summary>
        /// Debug VFX instance after spawn
        /// </summary>
        private void DebugVFXInstance(GameObject instance, string attackName)
        {
            Debug.Log($"=== VFX DEBUG: {attackName} ===");
            Debug.Log($"Instance Name: {instance.name}");
            Debug.Log($"Position: {instance.transform.position}");
            Debug.Log($"Rotation: {instance.transform.rotation.eulerAngles}");
            Debug.Log($"Scale: {instance.transform.localScale}");
            Debug.Log($"Active: {instance.activeSelf}");

            ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>();
            Debug.Log($"Particle Systems found: {particleSystems.Length}");
            
            foreach (ParticleSystem ps in particleSystems)
            {
                Debug.Log($"  - {ps.name}: IsPlaying={ps.isPlaying}, IsEmitting={ps.isEmitting}, ParticleCount={ps.particleCount}");
            }
        }

        /// <summary>
        /// Manual spawn VFX - Can be called from Animation Events if needed
        /// </summary>
        public void SpawnAttack1VFX()
        {
            SpawnVFX(attack1VFX, attack1RotationOffset, "Attack 1 (Manual)");
        }

        public void SpawnAttack2VFX()
        {
            SpawnVFX(attack2VFX, attack2RotationOffset, "Attack 2 (Manual)");
        }

        public void SpawnAttack3VFX()
        {
            SpawnVFX(attack3VFX, attack3RotationOffset, "Attack 3 (Manual)");
        }

        public void SpawnUltimateVFX()
        {
            SpawnVFX(ultimateVFX, ultimateRotationOffset, "Ultimate (Manual)");
        }

        // Visual debug in Scene view
        private void OnDrawGizmosSelected()
        {
            if (vfxSpawnPoint == null) return;

            // Draw spawn point
            Gizmos.color = Color.red;
            Vector3 spawnPosition = vfxSpawnPoint.position + vfxSpawnPoint.TransformDirection(spawnOffset);
            Gizmos.DrawWireSphere(spawnPosition, 0.1f);
            Gizmos.DrawLine(vfxSpawnPoint.position, spawnPosition);
            
            if (showRotationGizmos)
            {
                // Draw weapon forward direction (original)
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(spawnPosition, vfxSpawnPoint.forward * 0.5f);
                
                // Draw Attack 1 rotation
                if (attack1VFX != null)
                {
                    Gizmos.color = Color.green;
                    Quaternion rot1 = vfxSpawnPoint.rotation * Quaternion.Euler(attack1RotationOffset);
                    Gizmos.DrawRay(spawnPosition, rot1 * Vector3.forward * 0.4f);
                }
                
                // Draw Attack 2 rotation
                if (attack2VFX != null)
                {
                    Gizmos.color = Color.yellow;
                    Quaternion rot2 = vfxSpawnPoint.rotation * Quaternion.Euler(attack2RotationOffset);
                    Gizmos.DrawRay(spawnPosition, rot2 * Vector3.forward * 0.4f);
                }
                
                // Draw Attack 3 rotation
                if (attack3VFX != null)
                {
                    Gizmos.color = Color.cyan;
                    Quaternion rot3 = vfxSpawnPoint.rotation * Quaternion.Euler(attack3RotationOffset);
                    Gizmos.DrawRay(spawnPosition, rot3 * Vector3.forward * 0.4f);
                }
                
                // Draw Ultimate rotation
                if (ultimateVFX != null)
                {
                    Gizmos.color = Color.magenta;
                    Quaternion rotUlt = vfxSpawnPoint.rotation * Quaternion.Euler(ultimateRotationOffset);
                    Gizmos.DrawRay(spawnPosition, rotUlt * Vector3.forward * 0.4f);
                }
            }
        }
    }
}
