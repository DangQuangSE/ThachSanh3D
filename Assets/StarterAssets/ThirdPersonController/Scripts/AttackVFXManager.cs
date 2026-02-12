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
        
        [Tooltip("Offset position from spawn point")]
        public Vector3 spawnOffset = new Vector3(0f, 0f, 0.5f);
        
        [Tooltip("Offset rotation from spawn point")]
        public Vector3 rotationOffset = Vector3.zero;
        
        [Tooltip("VFX scale multiplier")]
        public float vfxScale = 1f;
        
        [Tooltip("Time to destroy VFX after spawning")]
        public float vfxLifetime = 2f;

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
                    SpawnVFX(attack1VFX, "Attack 1");
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
                    SpawnVFX(attack2VFX, "Attack 2");
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
                    SpawnVFX(attack3VFX, "Attack 3");
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
                    SpawnVFX(ultimateVFX, "Ultimate Attack");
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
        /// Spawns VFX at the spawn point
        /// </summary>
        private void SpawnVFX(GameObject vfxPrefab, string attackName)
        {
            if (vfxPrefab == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"AttackVFXManager: No VFX prefab assigned for {attackName}");
                return;
            }

            // Calculate spawn position and rotation
            Vector3 spawnPosition = vfxSpawnPoint.position + vfxSpawnPoint.TransformDirection(spawnOffset);
            Quaternion spawnRotation = vfxSpawnPoint.rotation * Quaternion.Euler(rotationOffset);

            // Spawn VFX
            GameObject vfxInstance = Instantiate(vfxPrefab, spawnPosition, spawnRotation);
            
            // Apply scale
            vfxInstance.transform.localScale = Vector3.one * vfxScale;

            // Destroy after lifetime
            Destroy(vfxInstance, vfxLifetime);

            if (showDebugLogs)
            {
                Debug.Log($"AttackVFXManager: Spawned VFX for {attackName} at {spawnPosition}");
            }
        }

        /// <summary>
        /// Manual spawn VFX - Can be called from Animation Events if needed
        /// </summary>
        public void SpawnAttack1VFX()
        {
            SpawnVFX(attack1VFX, "Attack 1 (Manual)");
        }

        public void SpawnAttack2VFX()
        {
            SpawnVFX(attack2VFX, "Attack 2 (Manual)");
        }

        public void SpawnAttack3VFX()
        {
            SpawnVFX(attack3VFX, "Attack 3 (Manual)");
        }

        public void SpawnUltimateVFX()
        {
            SpawnVFX(ultimateVFX, "Ultimate (Manual)");
        }

        // Visual debug in Scene view
        private void OnDrawGizmosSelected()
        {
            if (vfxSpawnPoint != null)
            {
                Gizmos.color = Color.red;
                Vector3 spawnPosition = vfxSpawnPoint.position + vfxSpawnPoint.TransformDirection(spawnOffset);
                Gizmos.DrawWireSphere(spawnPosition, 0.1f);
                Gizmos.DrawLine(vfxSpawnPoint.position, spawnPosition);
                
                // Draw direction arrow
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(spawnPosition, vfxSpawnPoint.forward * 0.5f);
            }
        }
    }
}
