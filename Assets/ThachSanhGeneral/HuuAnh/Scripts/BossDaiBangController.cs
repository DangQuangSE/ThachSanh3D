using UnityEngine;
using UnityEngine.AI;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Eagle Boss - Animator HuuAnh: isWalking, Punch, Uppercut, JumpAttack, MagicAttack, MutantRoaring, Die.
/// Attach to: Boss GameObject (finalv5) - same object with Animator + NavMeshAgent.
/// </summary>
public class BossDaiBangController : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Chase,
        Attack,
        Death
    }

    [Header("Boss Stats")]
    public float maxHealth = 1000f;
    public float attackDamage = 30f;
    public float moveSpeed = 3.5f;

    [Header("Combat Settings")]
    [Tooltip("Detection range - starts chasing player")]
    public float detectionRange = 15f;
    [Tooltip("Range to stop and attack")]
    public float attackRange = 2.5f;
    [Tooltip("Cooldown between attacks")]
    public float attackCooldown = 2f;
    [Tooltip("Max chase distance, returns to spawn if exceeded")]
    public float maxChaseDistance = 30f;

    [Header("Attack Hitbox")]
    public Transform attackPoint;
    public float attackRadius = 1.5f;
    public LayerMask playerLayer;

    [Header("References")]
    [Tooltip("Assign: Player object from Hierarchy")]
    public Transform target;

    [Header("VFX - Spawned on skill use (assign prefab from HuuAnh/VFX/Free Game VFX/Prefab)")]
    [Tooltip("Fireball (Magic Attack) - assign spawn point (e.g. hand/staff)")]
    public Transform magicSpawnPoint;
    [Tooltip("Reference point for Roar damage (near boss mouth). Leave empty to use Magic Spawn Point.")]
    public Transform fireBreathSpawnPoint;
    [Tooltip("Spawn point for Jump Attack VFX (placed at boss feet, near ground). Leave empty to use transform.position.")]
    public Transform jumpAttackSpawnPoint;
    public GameObject fxGreenHit;
    public GameObject fxFireball;
    public GameObject fxWeaponEffect;

    [Header("Visual Feedback")]
    public Color damageColor = Color.red;
    public float damageFlashDuration = 0.1f;

    [Header("Mutant Roaring (skill 6 - no fire VFX)")]
    [Tooltip("Duration boss stands still during Roar (seconds). Next attack starts after this. Adjust to match Roar animation length.")]
    public float roarDuration = 2.2f;
    [Tooltip("Delay after Fireball (Magic) before Roar can be used (seconds). Prevents two fire skills at once.")]
    public float magicToRoarDelay = 1.5f;

    [Header("Roar Stun")]
    [Tooltip("Enable stun on player when Roar is used")]
    public bool roarStunEnabled = true;
    [Tooltip("Stun duration in seconds")]
    public float roarStunDuration = 5f;

    [Header("Player Damage to Boss")]
    [Tooltip("Range within which player attacks can damage boss")]
    public float playerHitRange = 3f;
    [Tooltip("Damage per hit if PlayerAttack component not found on player (default 25)")]
    public float fallbackPlayerDamage = 25f;

    // ==================== SFX ====================
    [Header("SFX (drag AudioClip here - leave empty to skip)")]
    [Tooltip("Roar scream (Mutant Roaring) - synced to animation, not trigger")]
    public AudioClip sfxRoar;
    [Tooltip("JumpAttack sound (ground slam / heavy impact)")]
    public AudioClip sfxJumpAttack;
    [Tooltip("MagicAttack / Fireball cast sound")]
    public AudioClip sfxMagicAttack;
    [Tooltip("Hit impact when attack lands on player")]
    public AudioClip sfxHit;
    [Tooltip("Boss hurt grunt when taking damage")]
    public AudioClip sfxHurt;
    [Tooltip("Boss death cry")]
    public AudioClip sfxDeath;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    // ==================== END SFX ====================

    private AudioSource _audioSource;
    private float _lastHurtSoundTime;
    private bool _jumpAttackSoundPlayedThisCast;
    private bool _magicAttackSoundPlayedThisCast;

    private BossState currentState = BossState.Idle;
    private float currentHealth;
    private NavMeshAgent agent;
    private Animator animator;
    private float lastAttackTime;
    private bool isDead = false;
    private Renderer[] renderers;
    private Color[] originalColors;
    private bool hasAnimator;
    private Vector3 spawnPosition;

    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int AnimIDPunch = Animator.StringToHash("Punch");
    private static readonly int AnimIDUppercut = Animator.StringToHash("Uppercut");
    private static readonly int AnimIDJumpAttack = Animator.StringToHash("JumpAttack");
    private static readonly int AnimIDMagicAttack = Animator.StringToHash("MagicAttack");
    private static readonly int AnimIDMutantRoaring = Animator.StringToHash("MutantRoaring");
    private static readonly int AnimIDDie = Animator.StringToHash("Die");

    /// <summary>Attack rotation: 0=Punch, 1=Punch, 2=Uppercut, 3=JumpAttack, 4=MagicAttack, 5=Mutant Roaring, then loops.</summary>
    private int _attackRotationIndex;

    private Collider[] _hitBuffer = new Collider[8];

    private float _lastPlayerDamageTime;
    private static readonly string[] PlayerAttackStates = { "Attack_1", "Attack_2", "Attack_3", "UntimateAttack", "UntimateAttack_1", "Attack360" };

    private int _lastBossAttackStateHash;
    private bool _lastBossAttackHitDone;
    private bool _magicAttackSpawnedThisCast;
    private bool _roarDamageDoneThisCast;
    private int _lastDebugRoarStateHash;
    private float _roarLockUntil = -1f;       // During this period boss stands still, no rotation or attacks (Roaring)
    private static readonly string[] BossMeleeStates = { "Punch", "Uppercut", "JumpAttack" };
    private bool _playerFlashInProgress;
    private System.Collections.Generic.List<(Renderer r, int matIndex, string prop, Color original)> _playerNormalColorsCache;
    private bool _isInJumpAttack;
    private Vector3 _jumpAttackTargetPos;
    private CharacterController _playerCC;

    void Start()
    {
        currentHealth = maxHealth;
        spawnPosition = transform.position;
        _attackRotationIndex = 0;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        hasAnimator = TryGetComponent(out animator);

        // Auto-create AudioSource if missing, configure for 3D spatial sound
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f;
        _audioSource.minDistance = 2f;
        _audioSource.maxDistance = 25f;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;

        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        // Cache player CharacterController for separation
        if (target != null)
            _playerCC = target.GetComponent<CharacterController>();

        // Ignore physics collision between boss collider and player collider/CC
        // so NavMeshAgent position changes don't push CharacterController via physics
        Collider bossCol = GetComponent<Collider>();
        if (target != null && bossCol != null)
        {
            Collider playerCol = target.GetComponent<Collider>();
            if (playerCol != null)
                Physics.IgnoreCollision(bossCol, playerCol);
            // Also ignore the CharacterController collider (it IS a Collider)
            if (_playerCC != null && _playerCC != playerCol)
                Physics.IgnoreCollision(bossCol, _playerCC);
        }
    }

    void Update()
    {
        if (isDead) return;

        UpdateStateMachine();
        UpdateAnimator();
        SyncJumpAttackRootMotion();
        TryReceiveDamageFromPlayer();
        TryDealDamageToPlayerFromAnimation();

        if (!_playerFlashInProgress && target != null)
            CachePlayerNormalColors(target);
    }

    void LateUpdate()
    {
        if (isDead) return;
        EnforceSeparation();
    }

    /// <summary>Play a one-shot SFX clip via the boss AudioSource (3D positioned). Null-safe.</summary>
    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || _audioSource == null) return;
        _audioSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// Enforce minimum distance between boss and player every frame in LateUpdate.
    /// When player is attacking, only the player is pushed back (boss stands firm).
    /// When boss is chasing/moving, only the boss is pushed back (player stands firm).
    /// Otherwise both are separated equally.
    /// </summary>
    private void EnforceSeparation()
    {
        if (target == null) return;

        float minDist = 1.2f;

        Vector3 bossPos = transform.position;
        Vector3 playerPos = target.position;
        Vector3 diff = new Vector3(bossPos.x - playerPos.x, 0f, bossPos.z - playerPos.z);
        float dist = diff.magnitude;

        if (dist >= minDist) return;

        // Compute push direction (boss-to-player)
        Vector3 pushDir;
        if (dist > 0.01f)
        {
            pushDir = diff / dist;
        }
        else
        {
            pushDir = transform.forward;
            pushDir.y = 0f;
            pushDir = pushDir.normalized;
            if (pushDir.sqrMagnitude < 0.001f) pushDir = Vector3.forward;
        }

        float overlap = minDist - dist;

        // Determine who caused the overlap:
        // If player is in an attack/skill animation ? player lunged into boss ? push only player
        // If boss is actively chasing (agent moving) ? boss ran into player ? push only boss
        // Otherwise ? split equally
        bool playerIsAttacking = false;
        Animator playerAnim = target.GetComponentInChildren<Animator>();
        if (playerAnim != null)
        {
            AnimatorStateInfo pst = playerAnim.GetCurrentAnimatorStateInfo(0);
            playerIsAttacking = pst.IsName("Attack_1") || pst.IsName("Attack_2") || pst.IsName("Attack_3")
                             || pst.IsName("UntimateAttack") || pst.IsName("UntimateAttack_1")
                             || pst.IsName("Attack360") || pst.IsName("Roll");
        }

        bool bossIsMoving = agent != null && agent.enabled && !agent.isStopped && agent.velocity.sqrMagnitude > 0.01f;

        if (playerIsAttacking && !bossIsMoving)
        {
            // Player lunged into boss ? push only player back, boss stands firm
            if (_playerCC != null && _playerCC.enabled)
            {
                Vector3 playerPush = -pushDir * overlap;
                playerPush.y = 0f;
                _playerCC.Move(playerPush);
            }
        }
        else if (bossIsMoving && !playerIsAttacking)
        {
            // Boss ran into player ? push only boss back
            Vector3 bossNewPos = bossPos + pushDir * overlap;
            bossNewPos.y = bossPos.y;
            transform.position = bossNewPos;
            if (agent.isOnNavMesh)
                agent.Warp(bossNewPos);
        }
        else
        {
            // Neither or both ? split equally
            float halfPush = overlap * 0.5f;

            Vector3 bossNewPos = bossPos + pushDir * halfPush;
            bossNewPos.y = bossPos.y;
            transform.position = bossNewPos;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
                agent.Warp(bossNewPos);

            if (_playerCC != null && _playerCC.enabled)
            {
                Vector3 playerPush = -pushDir * halfPush;
                playerPush.y = 0f;
                _playerCC.Move(playerPush);
            }
        }
    }

    private const string BaseColorProp = "_BaseColor";
    private const string ColorProp = "_Color";

    private void CachePlayerNormalColors(Transform playerTransform)
    {
        Renderer[] renderers = playerTransform.GetComponentsInChildren<Renderer>();
        var list = new System.Collections.Generic.List<(Renderer, int, string, Color)>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            for (int m = 0; m < mats.Length; m++)
            {
                Material mat = mats[m];
                if (mat.HasProperty(BaseColorProp))
                    list.Add((renderers[i], m, BaseColorProp, mat.GetColor(BaseColorProp)));
                else if (mat.HasProperty(ColorProp))
                    list.Add((renderers[i], m, ColorProp, mat.GetColor(ColorProp)));
            }
        }
        _playerNormalColorsCache = list;
    }

    /// <summary>Deal damage to player based on animation state (no Animation Event required) - for testing.</summary>
    private void TryDealDamageToPlayerFromAnimation()
    {
        if (target == null || !hasAnimator || isDead) return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float nt = state.normalizedTime % 1f;
        int stateHash = state.fullPathHash;

        // MagicAttack: spawn fireball once per animation, play sound synced to cast moment
        if (state.IsName("MagicAttack"))
        {
            if (nt < 0.05f)
            {
                _magicAttackSpawnedThisCast = false;
                _magicAttackSoundPlayedThisCast = false;
            }

            if (!_magicAttackSoundPlayedThisCast && nt >= 0.1f && nt <= 0.4f)
            {
                PlaySFX(sfxMagicAttack);
                _magicAttackSoundPlayedThisCast = true;
            }

            if (!_magicAttackSpawnedThisCast && nt >= 0.2f && nt <= 0.6f)
            {
                SpawnMagicAttack();
                _magicAttackSpawnedThisCast = true;
            }

            return;
        }

        // Mutant Roaring: damage only (sound is played from PerformAttack via PlayClipAtPoint)
        bool isRoarState =
            state.IsName("Mutant Roaring") ||
            state.IsName("MutantRoaring") ||
            state.IsName("Roar");

        if (isRoarState)
        {
            if (nt < 0.05f)
                _roarDamageDoneThisCast = false;

            if (!_roarDamageDoneThisCast && nt >= 0.25f && nt <= 0.55f)
            {
                float roarScale = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3f;
                float roarRadius = attackRadius * Mathf.Max(roarScale, 0.5f) * 3f;
                Transform roarPoint = fireBreathSpawnPoint != null ? fireBreathSpawnPoint : (magicSpawnPoint != null ? magicSpawnPoint : transform);
                float roarDist = Vector3.Distance(roarPoint.position, target.position);
                if (roarDist <= roarRadius)
                {
                    PlayerHealth roarPh = target.GetComponent<PlayerHealth>() ?? target.GetComponentInChildren<PlayerHealth>();
                    if (roarPh != null && !roarPh.IsDead())
                    {
                        roarPh.TakeDamage(attackDamage * 0.8f);
                        PlaySFX(sfxHit);
                        StartCoroutine(FlashPlayerRed(target));
                        _roarDamageDoneThisCast = true;
                    }
                }
            }

            return;
        }
        else
        {
            _magicAttackSpawnedThisCast = false;
        }

        // JumpAttack: play sound at 5-20% (early, synced to jump/descend)
        if (state.IsName("JumpAttack"))
        {
            if (nt < 0.03f)
                _jumpAttackSoundPlayedThisCast = false;
            if (!_jumpAttackSoundPlayedThisCast && nt >= 0.05f && nt <= 0.2f)
            {
                PlaySFX(sfxJumpAttack);
                _jumpAttackSoundPlayedThisCast = true;
            }
        }

        bool isMeleeState = false;
        for (int i = 0; i < BossMeleeStates.Length; i++)
        {
            if (state.IsName(BossMeleeStates[i])) { isMeleeState = true; break; }
        }

        if (nt < 0.2f || !isMeleeState)
        {
            _lastBossAttackHitDone = false;
            return;
        }

        if (nt > 0.7f) return;
        if (_lastBossAttackHitDone && _lastBossAttackStateHash == stateHash) return;

        float scaleFactor = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3f;
        float effectiveRadius = attackRadius * Mathf.Max(scaleFactor, 0.5f);
        float dist = Vector3.Distance(attackPoint != null ? attackPoint.position : transform.position, target.position);
        if (dist > effectiveRadius * 2.5f) return;

        PlayerHealth ph = target.GetComponent<PlayerHealth>() ?? target.GetComponentInChildren<PlayerHealth>();
        if (ph == null || ph.IsDead()) return;

        ph.TakeDamage(attackDamage);

        // Only play sfxHit for Punch/Uppercut (JumpAttack has its own sfxJumpAttack)
        if (state.IsName("Punch") || state.IsName("Uppercut"))
        {
            PlaySFX(sfxHit);
            SpawnGreenHitOnPlayer();
        }
        else if (state.IsName("JumpAttack"))
        {
            SpawnJumpAttackMagic();
        }

        StartCoroutine(FlashPlayerRed(target));
        _lastBossAttackHitDone = true;
        _lastBossAttackStateHash = stateHash;
    }

    /// <summary>Flash player red on hit, then restore original colors. Uses cached normal colors to restore correctly.</summary>
    private IEnumerator FlashPlayerRed(Transform playerTransform)
    {
        if (playerTransform == null || _playerFlashInProgress) yield break;
        _playerFlashInProgress = true;

        Renderer[] renderers = playerTransform.GetComponentsInChildren<Renderer>();
        Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
        float duration = 0.12f;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            for (int m = 0; m < mats.Length; m++)
            {
                Material mat = mats[m];
                if (mat.HasProperty(BaseColorProp))
                    mat.SetColor(BaseColorProp, flashColor);
                else if (mat.HasProperty(ColorProp))
                    mat.SetColor(ColorProp, flashColor);
            }
            renderers[i].materials = mats;
        }

        yield return new WaitForSeconds(duration);

        if (_playerNormalColorsCache != null)
        {
            var seen = new System.Collections.Generic.HashSet<Renderer>();
            foreach (var t in _playerNormalColorsCache)
            {
                if (t.r == null || seen.Contains(t.r)) continue;
                seen.Add(t.r);
                Material[] mats = t.r.materials;
                foreach (var x in _playerNormalColorsCache)
                {
                    if (x.r != t.r) continue;
                    if (x.matIndex < mats.Length && mats[x.matIndex].HasProperty(x.prop))
                        mats[x.matIndex].SetColor(x.prop, x.original);
                }
                t.r.materials = mats;
            }
        }

        _playerFlashInProgress = false;
    }

    /// <summary>Spawn Green Hit VFX on player when boss melee lands (called from code or Animation Event).</summary>
    private void SpawnGreenHitOnPlayer()
    {
        if (fxGreenHit == null) return;
        Vector3 pos = target != null ? target.position + Vector3.up * 1f : (attackPoint != null ? attackPoint.position : transform.position + Vector3.up * 1f);
        Quaternion rot = Quaternion.identity;
        GameObject vfx = Instantiate(fxGreenHit, pos, rot);
        Destroy(vfx, 3f);
    }

    /// <summary>Boss receives damage when player is attacking nearby (HuuAnh only, no changes to Bach).</summary>
    private void TryReceiveDamageFromPlayer()
    {
        if (target == null || isDead) return;
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > playerHitRange) return;
        if (Time.time - _lastPlayerDamageTime < 0.35f) return;

        Animator playerAnim = target.GetComponentInChildren<Animator>();
        if (playerAnim == null) return;

        AnimatorStateInfo state = playerAnim.GetCurrentAnimatorStateInfo(0);
        float nt = state.normalizedTime % 1f;
        bool inHitWindow = nt >= 0.2f && nt <= 0.65f;

        if (!inHitWindow) return;

        bool isAttackState = false;
        for (int i = 0; i < PlayerAttackStates.Length; i++)
        {
            if (state.IsName(PlayerAttackStates[i])) { isAttackState = true; break; }
        }
        if (!isAttackState) return;

        float damage = fallbackPlayerDamage;
        PlayerAttack pa = target.GetComponent<PlayerAttack>();
        if (pa != null) damage = pa.attackDamage;

        TakeDamage(damage);
        _lastPlayerDamageTime = Time.time;
    }

    private void UpdateStateMachine()
    {
        if (target == null) return;

        // Lock state while any attack animation is playing — boss finishes attack before transitioning
        if (currentState == BossState.Attack && IsInAnyAttackAnimation())
            return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        float distanceFromSpawn = Vector3.Distance(transform.position, spawnPosition);

        switch (currentState)
        {
            case BossState.Idle:
                if (distanceToTarget <= detectionRange)
                {
                    ChangeState(BossState.Chase);
                }
                break;

            case BossState.Chase:
                if (distanceToTarget > maxChaseDistance || distanceFromSpawn > maxChaseDistance)
                {
                    ReturnToSpawn();
                }
                else if (distanceToTarget <= attackRange)
                {
                    ChangeState(BossState.Attack);
                }
                else
                {
                    ChaseTarget();
                }
                break;

            case BossState.Attack:
                if (distanceToTarget > attackRange * 1.5f)
                {
                    ChangeState(BossState.Chase);
                }
                else
                {
                    AttackTarget();
                }
                break;
        }
    }

    private void ChangeState(BossState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case BossState.Idle:
                if (agent != null) agent.isStopped = true;
                break;

            case BossState.Chase:
                if (agent != null) agent.isStopped = false;
                break;

            case BossState.Attack:
                if (agent != null) agent.isStopped = true;
                break;
        }
    }

    private void ChaseTarget()
    {
        if (agent != null && target != null)
        {
            agent.SetDestination(target.position);

            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
        }
    }

    private void AttackTarget()
    {
        if (agent != null) agent.isStopped = true;

        // Currently roaring: stand still, no rotation, no next attack
        if (Time.time < _roarLockUntil)
            return;

        // Only rotate toward player when NOT in any attack animation
        if (!IsInAnyAttackAnimation())
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
                transform.rotation = lookRotation;
            }
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            PerformAttack();
        }
    }

    /// <summary>Returns true if the boss animator is currently in any attack/skill state.</summary>
    private bool IsInAnyAttackAnimation()
    {
        if (!hasAnimator) return false;
        AnimatorStateInfo st = animator.GetCurrentAnimatorStateInfo(0);
        return st.IsName("Punch") || st.IsName("Uppercut") || st.IsName("JumpAttack") ||
               st.IsName("MagicAttack") || st.IsName("Mutant Roaring") ||
               st.IsName("MutantRoaring") || st.IsName("Roar");
    }

    /// <summary>Fixed attack rotation: Punch x2 -> Uppercut -> Jump Attack -> Fireball (MagicAttack) -> Mutant Roaring, then loops.</summary>
    private void PerformAttack()
    {
        if (!hasAnimator || target == null) return;

        switch (_attackRotationIndex)
        {
            case 0:
            case 1:
                animator.SetTrigger(AnimIDPunch);
                break;
            case 2:
                animator.SetTrigger(AnimIDUppercut);
                break;
            case 3:
                animator.SetTrigger(AnimIDJumpAttack);
                break;
            case 4:
                animator.SetTrigger(AnimIDMagicAttack);
                lastAttackTime = Time.time + magicToRoarDelay;
                break;
            case 5:
                animator.SetTrigger(AnimIDMutantRoaring);
                // Play roar sound immediately via PlayClipAtPoint (independent of AudioSource/animator state)
                if (sfxRoar != null)
                    AudioSource.PlayClipAtPoint(sfxRoar, transform.position, sfxVolume);
                // Stun player when roar is triggered
                if (roarStunEnabled && target != null)
                    StunEffect.Apply(target.gameObject, roarStunDuration);
                _roarLockUntil = Time.time + roarDuration;
                lastAttackTime = Time.time + roarDuration;
                break;
            default:
                animator.SetTrigger(AnimIDPunch);
                break;
        }

        _attackRotationIndex = (_attackRotationIndex + 1) % 6;
    }

    /// <summary>Called from Animation Event on Punch / Uppercut / JumpAttack clips (damage frame).</summary>
    public void DealDamageToPlayer()
    {
        Transform point = attackPoint != null ? attackPoint : transform;
        bool didHit = false;
        float scaleFactor = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3f;
        float effectiveRadius = attackRadius * Mathf.Max(scaleFactor, 0.5f);

        if (playerLayer != 0)
        {
            int count = Physics.OverlapSphereNonAlloc(point.position, effectiveRadius, _hitBuffer, playerLayer);
            for (int i = 0; i < count; i++)
            {
                PlayerHealth ph = _hitBuffer[i].GetComponent<PlayerHealth>() ?? _hitBuffer[i].GetComponentInParent<PlayerHealth>();
                if (ph != null && !ph.IsDead())
                {
                    ph.TakeDamage(attackDamage);
                    didHit = true;
                }
            }
        }

        if (!didHit && target != null)
        {
            float dist = Vector3.Distance(point.position, target.position);
            if (dist <= effectiveRadius * 1.5f)
            {
                PlayerHealth ph = target.GetComponent<PlayerHealth>() ?? target.GetComponentInChildren<PlayerHealth>();
                if (ph != null && !ph.IsDead())
                {
                    ph.TakeDamage(attackDamage);
                    didHit = true;
                }
            }
        }

        if (didHit)
        {
            PlaySFX(sfxHit);
            if (hasAnimator)
            {
                AnimatorStateInfo st = animator.GetCurrentAnimatorStateInfo(0);
                if (st.IsName("Punch") || st.IsName("Uppercut"))
                    SpawnGreenHitOnPlayer();
                else if (st.IsName("JumpAttack"))
                    SpawnJumpAttackMagic();
            }
            if (target != null) StartCoroutine(FlashPlayerRed(target));
        }
    }

    public void SpawnPunchMagic()
    {
        if (fxGreenHit == null) return;
        Transform point = attackPoint != null ? attackPoint : (magicSpawnPoint != null ? magicSpawnPoint : transform);
        GameObject vfx = Instantiate(fxGreenHit, point.position, point.rotation);
        Destroy(vfx, 3f);
    }

    public void SpawnJumpAttackMagic()
    {
        if (fxWeaponEffect == null) return;
        Transform point = jumpAttackSpawnPoint != null ? jumpAttackSpawnPoint : transform;

        Vector3 spawnPos = point.position;
        Quaternion spawnRot = point.rotation;
        if (target != null)
        {
            Vector3 toTarget = target.position - point.position;
            toTarget.y = 0f;
            if (toTarget != Vector3.zero)
                spawnRot = Quaternion.LookRotation(toTarget);
        }

        GameObject vfx = Instantiate(fxWeaponEffect, spawnPos, spawnRot);
        Destroy(vfx, 4f);
    }

    public void SpawnMagicAttack()
    {
        if (fxFireball == null || target == null) return;
        Transform point = magicSpawnPoint != null ? magicSpawnPoint : transform;
        Vector3 dir = (target.position - point.position).normalized;
        dir.y = 0;
        Quaternion rot = dir != Vector3.zero ? Quaternion.LookRotation(dir) : point.rotation;
        GameObject vfx = Instantiate(fxFireball, point.position, rot);
        Destroy(vfx, 5f);
    }

    private void ReturnToSpawn()
    {
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(spawnPosition);

            if (Vector3.Distance(transform.position, spawnPosition) < 1f)
            {
                ChangeState(BossState.Idle);
                currentHealth = maxHealth;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Hurt sound with 0.5s cooldown to prevent spam during player combos
        if (Time.time - _lastHurtSoundTime >= 0.5f)
        {
            PlaySFX(sfxHurt);
            _lastHurtSoundTime = Time.time;
        }

        StartCoroutine(DamageFlash());
        if (currentState == BossState.Idle) ChangeState(BossState.Chase);
        if (currentHealth <= 0) Die();
    }

    private IEnumerator DamageFlash()
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material.HasProperty("_Color"))
            {
                renderer.material.color = damageColor;
            }
        }

        yield return new WaitForSeconds(damageFlashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    private void Die()
    {
        isDead = true;
        currentState = BossState.Death;

        // Use PlayClipAtPoint so death sound keeps playing after GameObject is destroyed
        if (sfxDeath != null)
            AudioSource.PlayClipAtPoint(sfxDeath, transform.position, sfxVolume);

        if (agent != null) { agent.isStopped = true; agent.enabled = false; }
        if (hasAnimator) animator.SetTrigger(AnimIDDie);
        if (TryGetComponent<Collider>(out var col)) col.enabled = false;
        Destroy(gameObject, 5f);
    }

    private void UpdateAnimator()
    {
        if (!hasAnimator) return;
        float speed = (agent != null && !agent.isStopped) ? agent.velocity.magnitude : 0f;
        animator.SetBool(IsWalking, speed > 0.1f);
    }

    /// <summary>
    /// During JumpAttack, disable NavMeshAgent so Root Motion controls the boss.
    /// Clamp horizontal movement so boss lands near player instead of passing through.
    /// </summary>
    private void SyncJumpAttackRootMotion()
    {
        if (!hasAnimator || agent == null) return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        bool inJumpAttack = state.IsName("JumpAttack");

        if (inJumpAttack && !_isInJumpAttack)
        {
            // Entering JumpAttack: snapshot target position and let Root Motion drive
            _isInJumpAttack = true;
            _jumpAttackTargetPos = target != null ? target.position : transform.position + transform.forward * 3f;
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
        else if (!inJumpAttack && _isInJumpAttack)
        {
            // Exiting JumpAttack: sync agent to landed position
            _isInJumpAttack = false;
            agent.Warp(transform.position);
            agent.updatePosition = true;
            agent.updateRotation = true;
        }

        if (_isInJumpAttack)
        {
            // Clamp: stop horizontal root motion when boss is close enough to player
            float distToTarget = Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(_jumpAttackTargetPos.x, 0f, _jumpAttackTargetPos.z));

            float stopDistance = attackRadius * 1.2f;
            if (distToTarget <= stopDistance)
            {
                // Freeze XZ position, allow only Y (jump arc) from root motion
                Vector3 frozenPos = transform.position;
                Vector3 toTarget = _jumpAttackTargetPos - frozenPos;
                toTarget.y = 0f;
                // Push boss back to edge of stop zone facing target
                if (toTarget.magnitude > 0.01f)
                {
                    Vector3 stopPos = _jumpAttackTargetPos - toTarget.normalized * stopDistance;
                    frozenPos.x = stopPos.x;
                    frozenPos.z = stopPos.z;
                    transform.position = frozenPos;
                }
            }

            agent.nextPosition = transform.position;
        }
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public BossState GetCurrentState()
    {
        return currentState;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            float scaleFactor = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3f;
            float effectiveRadius = attackRadius * Mathf.Max(scaleFactor, 0.5f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, effectiveRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxChaseDistance);
    }
}
