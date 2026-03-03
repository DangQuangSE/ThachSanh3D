using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Thêm để dùng Coroutine

public class ChanTinhBossController : MonoBehaviour
{
    [Header("References")]
    private NavMeshAgent agent; // NavMeshAgent để điều khiển di chuyển
    private Animator animator; // Animator để điều khiển animation
    private Transform player; // Tham chiếu đến vị trí của player

    // ===== THÊM HEALTH SYSTEM =====
    [Header("Boss Stats")]
    [Tooltip("Full hp of boss")]
    [SerializeField] private float maxHealth = 1000f;
    private float currentHealth;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2.5f; // Khoảng cách để bắt đầu tấn công
    [SerializeField] private float detectionRange = 15f; // Khoảng cách để phát hiện player và bắt đầu theo đuổi
    [SerializeField] private float walkSpeed = 2f; // Tốc độ đi bộ
    [SerializeField] private float runSpeed = 5f; // Tốc độ chạy khi xa player

    //gay dame
    [Header("Attack Damage Settings")]
    [Tooltip("Sát thương mỗi đòn tấn công")]
    [SerializeField] private float attackDamage = 30f;
    [Tooltip("Vị trí spawn attack hitbox")]
    [SerializeField] private Transform attackPoint;
    [Tooltip("Bán kính hitbox tấn công")]
    [SerializeField] private float attackRadius = 1.5f;
    [Tooltip("Layer của player")]
    [SerializeField] private LayerMask playerLayer;

    // ===== THÊM VISUAL FEEDBACK hp =====
    [Header("Visual Feedback")]
    [Tooltip("Màu sắc khi nhận damage")]
    [SerializeField] private Color damageColor = Color.red;
    [Tooltip("Thời gian hiệu ứng damage")]
    [SerializeField] private float damageFlashDuration = 0.1f;

    private Renderer[] renderers;
    private Color[] originalColors;

    [Header("VFX")]
    [Tooltip("Kéo thả GameObject chứa VfxController vào đây")]
    [SerializeField] private VfxController vfxController;

    [Header("Attack Cooldowns")]
    [SerializeField] private float roarCooldown = 10f; // Thời gian cooldown cho đòn gầm thét
    [SerializeField] private float swipeCooldown = 3f;
    [SerializeField] private float punchCooldown = 2f;
    [SerializeField] private float jumpAttackCooldown = 8f;

    private float lastAttackTime; // Thời gian lần cuối cùng thực hiện một đòn tấn công
    private float currentCooldown; // Cooldown hiện tại của đòn tấn công đang thực hiện
    private bool isAttacking = false; // Flag để kiểm tra xem boss đang trong trạng thái tấn công hay không
    private bool isDead = false; // Flag để kiểm tra xem boss đã chết hay chưa

    // Animation parameter hashes
    private int speedHash; // Hash cho parameter "Speed" trong Animator
    private int isDeadHash;
    private int isInCombatHash;
    private int roarHash;
    private int swipeHash;
    private int punchHash;
    private int jumpAttackHash;
    private int hitHash; // Thêm animation Hit

    private enum BossState
    {
        Idle, // Trạng thái đứng yên, không làm gì
        Patrol, // Trạng thái đi tuần tra (nếu có)
        Chase, // Trạng thái theo đuổi player khi phát hiện
        Attack // Trạng thái tấn công khi ở gần player
    }
    private BossState currentState = BossState.Idle;

    void Start()
    {
        // ===== KHỞI TẠO HEALTH =====
        currentHealth = maxHealth;
        // Lay tham chieu den NavMeshAgent, Animator va player
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;


        // ===== LẤY RENDERER ĐỂ LÀM DAMAGE FLASH =====
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }

        // Cache animation parameter hashes
        speedHash = Animator.StringToHash("Speed");
        isDeadHash = Animator.StringToHash("IsDead");
        isInCombatHash = Animator.StringToHash("IsInCombat");
        roarHash = Animator.StringToHash("Roar");
        swipeHash = Animator.StringToHash("Swipe");
        punchHash = Animator.StringToHash("Punch");
        jumpAttackHash = Animator.StringToHash("JumpAttack");
        hitHash = Animator.StringToHash("Hit"); // Thêm

        agent.updateRotation = true; // Cho phép NavMeshAgent tự động xoay theo hướng di chuyển
        agent.updatePosition = true; // Cho phép NavMeshAgent tự động cập nhật vị trí của boss khi di chuyển
    }

    void Update()
    {
        if (isDead) return; // Nếu boss đã chết, không thực hiện bất kỳ hành động nào

        // Tính khoảng cách đến player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // State Machine
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState(distanceToPlayer);
                break;
            case BossState.Chase:
                HandleChaseState(distanceToPlayer);
                break;
            case BossState.Attack:
                HandleAttackState(distanceToPlayer);
                break;
        }

        UpdateAnimator();
    }

    // Thêm method này sau Update()
    private void ChangeState(BossState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case BossState.Idle:
                agent.isStopped = true;
                agent.autoBraking = true;
                animator.SetBool(isInCombatHash, false);
                break;

            case BossState.Chase:
                agent.isStopped = false;
                agent.autoBraking = false; // Tắt auto braking để di chuyển mượt
                animator.SetBool(isInCombatHash, true);
                break;

            case BossState.Attack:
                agent.isStopped = true;
                agent.autoBraking = true; // Bật auto braking để dừng chính xác
                break;
        }
    }

    //xu ly trang thai Idle: dung yen, khong lam gi, nhung neu player vao trong detection range
    //thi chuyen sang trang thai Chase
    void HandleIdleState(float distanceToPlayer)
    {
        agent.isStopped = true; //dung yen khi o trang thai Idle
        animator.SetBool(isInCombatHash, false); //tat animation combat khi o trang thai Idle

        //kiem tra player co vao trong detection range hay khong
        if (distanceToPlayer <= detectionRange)
        {
            //currentState = BossState.Chase;
            ChangeState(BossState.Chase);
            animator.SetBool(isInCombatHash, true);
        }
    }

    //xu ly trang thai Chase: theo duoi player, neu player ra khoi detection range thi chuyen ve Idle,
    //neu player vao trong attack range thi chuyen sang trang thai Attack
    void HandleChaseState(float distanceToPlayer)
    {
        //neu player ra khoi detection range thi chuyen ve Idle
        if (distanceToPlayer > detectionRange)
        {
            //currentState = BossState.Idle;
            ChangeState(BossState.Idle);
            return;
        }

        agent.isStopped = false; //cho phep di chuyen
        agent.SetDestination(player.position); //di chuyen den vi tri player

        // dieu chinh toc do di chuyen: neu xa player thi chay, neu gan player thi di bo
        if (distanceToPlayer > attackRange * 2)
        {
            agent.speed = runSpeed; // Run khi xa
        }
        else
        {
            agent.speed = walkSpeed; // Walk khi gan
        }

        //kiem tra neu player vao trong attack range thi chuyen sang trang thai Attack
        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            //currentState = BossState.Attack;
            ChangeState(BossState.Attack);
        }
    }

    void HandleAttackState(float distanceToPlayer)
    {
        agent.isStopped = true; // Dung yen khi tan cong

        // Quay mat theo player
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        // Neu player ra khoi attack range thi chuyen ve Chase
        if (distanceToPlayer > attackRange)
        {
            //currentState = BossState.Chase;
            ChangeState(BossState.Chase);
            return;
        }
        // Neu dang khong tan cong va da het cooldown thi thuc hien mot dot tan cong ngau nhien
        if (!isAttacking && Time.time >= lastAttackTime + currentCooldown)
        {
            PerformRandomAttack();
        }
    }

    // Thuc hien mot dot tan cong ngau nhien trong 4 dot tan cong co san
    void PerformRandomAttack()
    {
        isAttacking = true;

        // Random attack type
        int attackType = Random.Range(0, 4);

        switch (attackType)
        {
            case 0: // Punch
                animator.SetTrigger(punchHash);
                currentCooldown = punchCooldown;
                break;
            case 1: // Swipe
                animator.SetTrigger(swipeHash);
                currentCooldown = swipeCooldown;
                break;
            case 2: // Roar
                animator.SetTrigger(roarHash);
                currentCooldown = roarCooldown;
                break;
            case 3: // Jump Attack
                animator.SetTrigger(jumpAttackHash);
                currentCooldown = jumpAttackCooldown;
                break;
        }

        // Spawn VFX cho đòn tấn công
        if (vfxController != null)
        {
            vfxController.PlayAttackVfx(attackType);
        }

        lastAttackTime = Time.time;
    }

    void UpdateAnimator()
    {
        // Update Speed parameter cho Blend Tree (Idle/Walk/Run)
        float speed = agent.velocity.magnitude;
        animator.SetFloat(speedHash, speed);
    }

    // ham nay se duoc goi tu animation event o cuoi moi dot tan cong
    // de thong bao rang dot tan cong da ket thuc va boss co the chuyen sang trang thai khac
    public void OnAttackComplete()
    {
        isAttacking = false;
        //currentState = BossState.Chase;
        ChangeState(BossState.Chase);

        // Dừng VFX khi animation tấn công kết thúc
        if (vfxController != null)
        {
            vfxController.StopAllVfx();
        }

        Debug.Log("Chan Tinh Boss attack animation completed");
    }

    // Method này sẽ được gọi từ Animation Event khi animation tấn công đến frame gây damage
    public void DealDamageToPlayer()
    {
        // Nếu không có attackPoint thì dùng vị trí của boss
        if (attackPoint == null)
        {
            attackPoint = transform;
        }

        // Tìm tất cả collider trong bán kính attack
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider hitCollider in hitColliders)
        {
            Debug.Log("Chan Tinh Boss hit: " + hitCollider.name);

            // Thử gây damage cho player
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsDead())
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Chan Tinh Boss dealt {attackDamage} damage to {hitCollider.name}!");
            }

            // Có thể thêm các component khác nếu player có nhiều script health
            // Ví dụ: ThirdPersonController health system
        }
    }

    public void Die()
    {
        isDead = true;
        animator.SetBool(isDeadHash, true);
        agent.isStopped = true;
        agent.enabled = false;

        Debug.Log("Chan Tinh Boss died!");

        // Tắt collider để không nhận damage nữa
        GetComponent<Collider>().enabled = false;

        // Destroy sau 5 giây
        Destroy(gameObject, 5f);
    }

    public bool IsDead()
    {
        return isDead;
    }

    // ===== THÊM METHOD TAKEDAMAGE =====
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Trigger animation Hit nếu có
        if (animator != null)
        {
            animator.SetTrigger(hitHash);
        }

        // Hiệu ứng flash màu đỏ
        StartCoroutine(DamageFlash());

        Debug.Log($"Chan Tinh Boss took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Nếu đang Idle thì chuyển sang Chase
        if (currentState == BossState.Idle)
        {
            ChangeState(BossState.Chase);
        }

        // Kiểm tra chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    // ===== HIỆU ỨNG FLASH MÀU ĐỎ KHI BỊ DAMAGE =====
    private IEnumerator DamageFlash()
    {
        // Đổi màu sang đỏ
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material.HasProperty("_Color"))
            {
                renderer.material.color = damageColor;
            }
        }

        yield return new WaitForSeconds(damageFlashDuration);

        // Trả về màu gốc
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    // ===== METHOD LẤY % HEALTH =====
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }


    // hien thi vong tron detection range va attack range
    // trong editor de de dang dieu chinh
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // ===== THÊM MỚI: Hiển thị attack hitbox =====
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
