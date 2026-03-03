using UnityEngine;
using UnityEngine.AI;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Boss Đại Bàng - Animator HuuAnh: isWalking, Punch, Uppercut, JumpAttack, MagicAttack, MutantRoaring, Die.
/// Gắn vào: GameObject Boss (finalv5) - cùng object có Animator + NavMeshAgent.
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
    [Tooltip("Khoảng cách phát hiện player - bắt đầu đuổi")]
    public float detectionRange = 15f;
    [Tooltip("Khoảng cách đứng lại và đánh")]
    public float attackRange = 2.5f;
    [Tooltip("Thời gian hồi giữa các đòn")]
    public float attackCooldown = 2f;
    [Tooltip("Đuổi xa tối đa, quá thì quay về")]
    public float maxChaseDistance = 30f;

    [Header("Attack Hitbox")]
    public Transform attackPoint;
    public float attackRadius = 1.5f;
    public LayerMask playerLayer;

    [Header("References")]
    [Tooltip("Gắn: Player (object người chơi) từ Hierarchy")]
    public Transform target;

    [Header("VFX - Spawn khi dùng skill (gắn prefab từ HuuAnh/VFX/Free Game VFX/Prefab)")]
    [Tooltip("Quả cầu lửa (Magic Attack) – gắn điểm spawn (vd tay/staff)")]
    public Transform magicSpawnPoint;
    [Tooltip("Điểm tham chiếu cho damage Roar (gần miệng boss). Để trống thì dùng Magic Spawn Point.")]
    public Transform fireBreathSpawnPoint;
    [Tooltip("Điểm spawn VFX cho Jump Attack (đặt ở chân boss, sát mặt đất). Để trống thì dùng transform.position.")]
    public Transform jumpAttackSpawnPoint;
    public GameObject fxGreenHit;
    public GameObject fxFireball;
    public GameObject fxWeaponEffect;

    [Header("Visual Feedback")]
    public Color damageColor = Color.red;
    public float damageFlashDuration = 0.1f;

    [Header("Mutant Roaring (chiêu 6 – không có hiệu ứng lửa)")]
    [Tooltip("Thời gian boss đứng yên Roar (giây). Hết thời gian này mới đánh đòn tiếp theo. Chỉnh cho khớp độ dài animation Roar.")]
    public float roarDuration = 2.2f;
    [Tooltip("Sau khi dùng quả cầu lửa (Magic), đợi bao nhiêu giây rồi mới được dùng phun lửa (Roar). Tránh 2 chiêu lửa dùng cùng lúc.")]
    public float magicToRoarDelay = 1.5f;

    [Header("Player đánh trừ máu Boss")]
    [Tooltip("Khoảng cách player đứng gần boss để đòn đánh tính trừ máu")]
    public float playerHitRange = 3f;
    [Tooltip("Damage mỗi đòn nếu không tìm thấy PlayerAttack trên player (mặc định 25)")]
    public float fallbackPlayerDamage = 25f;

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

    // Animator HuuAnh (BossDaiBang): isWalking, Punch, Uppercut, JumpAttack, MagicAttack, Die
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int AnimIDPunch = Animator.StringToHash("Punch");
    private static readonly int AnimIDUppercut = Animator.StringToHash("Uppercut");
    private static readonly int AnimIDJumpAttack = Animator.StringToHash("JumpAttack");
    private static readonly int AnimIDMagicAttack = Animator.StringToHash("MagicAttack");
    private static readonly int AnimIDMutantRoaring = Animator.StringToHash("MutantRoaring");
    private static readonly int AnimIDDie = Animator.StringToHash("Die");

    /// <summary>Vòng chiêu: 0=Punch, 1=Punch, 2=Uppercut, 3=JumpAttack, 4=MagicAttack, 5=Mutant Roaring, rồi lặp.</summary>
    private int _attackRotationIndex;

    private Collider[] _hitBuffer = new Collider[8];

    private float _lastPlayerDamageTime;
    private static readonly string[] PlayerAttackStates = { "Attack_1", "Attack_2", "Attack_3", "UntimateAttack", "UntimateAttack_1", "Attack360" };

    private int _lastBossAttackStateHash;
    private bool _lastBossAttackHitDone;
    private bool _magicAttackSpawnedThisCast;
    private bool _roarDamageDoneThisCast;
    private int _lastDebugRoarStateHash;
    private float _roarLockUntil = -1f;       // Trong khoảng này boss đứng yên, không xoay không đánh (đang Roar)
    private static readonly string[] BossMeleeStates = { "Punch", "Uppercut", "JumpAttack" };
    private bool _playerFlashInProgress;
    private System.Collections.Generic.List<(Renderer r, int matIndex, string prop, Color original)> _playerNormalColorsCache;

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
        }

        hasAnimator = TryGetComponent(out animator);

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
    }

    void Update()
    {
        if (isDead) return;

        UpdateStateMachine();
        UpdateAnimator();
        TryReceiveDamageFromPlayer();
        TryDealDamageToPlayerFromAnimation();

        if (!_playerFlashInProgress && target != null)
            CachePlayerNormalColors(target);
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

    /// <summary>Boss đánh trừ máu player theo animation (không cần Animation Event) - để test.</summary>
    private void TryDealDamageToPlayerFromAnimation()
    {
        if (target == null || !hasAnimator || isDead) return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float nt = state.normalizedTime % 1f;
        int stateHash = state.fullPathHash;

        // MagicAttack: chỉ spawn fireball một lần theo animation, không cần Animation Event
        if (state.IsName("MagicAttack"))
        {
            if (nt < 0.05f)
                _magicAttackSpawnedThisCast = false;

            if (!_magicAttackSpawnedThisCast && nt >= 0.2f && nt <= 0.6f)
            {
                SpawnMagicAttack();
                _magicAttackSpawnedThisCast = true;
            }

            return;
        }

        // Mutant Roaring: chỉ tính damage khi đang state Roar (không spawn hiệu ứng lửa)
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
        if (state.IsName("Punch") || state.IsName("Uppercut"))
            SpawnGreenHitOnPlayer();
        else if (state.IsName("JumpAttack"))
            SpawnJumpAttackMagic();
        StartCoroutine(FlashPlayerRed(target));
        _lastBossAttackHitDone = true;
        _lastBossAttackStateHash = stateHash;
    }

    /// <summary>Nhấp nháy đỏ khi boss đánh trúng, xong trở lại bình thường (không đỏ suốt). Dùng cache màu bình thường để restore đúng.</summary>
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

    /// <summary>Spawn hiệu ứng Green Hit khi boss đánh trúng player (gọi từ code hoặc Animation Event).</summary>
    private void SpawnGreenHitOnPlayer()
    {
        if (fxGreenHit == null) return;
        Vector3 pos = target != null ? target.position + Vector3.up * 1f : (attackPoint != null ? attackPoint.position : transform.position + Vector3.up * 1f);
        Quaternion rot = Quaternion.identity;
        GameObject vfx = Instantiate(fxGreenHit, pos, rot);
        Destroy(vfx, 3f);
    }

    /// <summary>Boss tự nhận damage khi player đang đánh và đứng gần (chỉ HuuAnh, không cần sửa Bach).</summary>
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
                // Không reset _attackRotationIndex: giữ vòng chiêu để tới được chiêu 6 (Mutant Roaring) dù player ra vào attack range
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

        // Đang trong lúc phun lửa: đứng yên, không xoay, không đánh đòn tiếp theo
        if (Time.time < _roarLockUntil)
            return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            PerformAttack();
        }
    }

    /// <summary>Vòng chiêu cố định: Punch x2 → Uppercut → Jump Attack → Fireball (MagicAttack), rồi lặp.</summary>
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
                lastAttackTime = Time.time + magicToRoarDelay; // Quả cầu lửa xong, đợi magicToRoarDelay giây rồi mới tới phun lửa
                break;
            case 5:
                animator.SetTrigger(AnimIDMutantRoaring);
                _roarLockUntil = Time.time + roarDuration;   // Đứng yên Roar xong rồi mới đánh tiếp
                lastAttackTime = Time.time + roarDuration;  // Cooldown đòn tiếp theo = hết thời gian Roar
                break;
            default:
                animator.SetTrigger(AnimIDPunch);
                break;
        }

        _attackRotationIndex = (_attackRotationIndex + 1) % 6;
    }

    /// <summary>Gọi từ Animation Event trên clip Punch / Uppercut / Jump Attack (frame đánh trúng).</summary>
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
        // Dùng jumpAttackSpawnPoint (chân boss, sát đất) thay vì magicSpawnPoint (tay/staff trên cao)
        Transform point = jumpAttackSpawnPoint != null ? jumpAttackSpawnPoint : transform;

        // Spawn hiệu ứng sạt lở tại vị trí boss tiếp đất (ground level)
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
