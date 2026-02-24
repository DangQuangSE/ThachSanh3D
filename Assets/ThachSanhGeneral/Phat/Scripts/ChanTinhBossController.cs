using UnityEngine;
using UnityEngine.AI;

public class ChanTinhBossController : MonoBehaviour
{
    [Header("References")]
    private NavMeshAgent agent; // NavMeshAgent để điều khiển di chuyển
    private Animator animator; // Animator để điều khiển animation
    private Transform player; // Tham chiếu đến vị trí của player

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2.5f; // Khoảng cách để bắt đầu tấn công
    [SerializeField] private float detectionRange = 15f; // Khoảng cách để phát hiện player và bắt đầu theo đuổi
    [SerializeField] private float walkSpeed = 2f; // Tốc độ đi bộ
    [SerializeField] private float runSpeed = 5f; // Tốc độ chạy khi xa player

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
        // Lay tham chieu den NavMeshAgent, Animator va player
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Cache animation parameter hashes
        speedHash = Animator.StringToHash("Speed");
        isDeadHash = Animator.StringToHash("IsDead");
        isInCombatHash = Animator.StringToHash("IsInCombat");
        roarHash = Animator.StringToHash("Roar");
        swipeHash = Animator.StringToHash("Swipe");
        punchHash = Animator.StringToHash("Punch");
        jumpAttackHash = Animator.StringToHash("JumpAttack");
    
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

    //xu ly trang thai Idle: dung yen, khong lam gi, nhung neu player vao trong detection range
    //thi chuyen sang trang thai Chase
    void HandleIdleState(float distanceToPlayer)
    {
        agent.isStopped = true; //dung yen khi o trang thai Idle
        animator.SetBool(isInCombatHash, false); //tat animation combat khi o trang thai Idle

        //kiem tra player co vao trong detection range hay khong
        if (distanceToPlayer <= detectionRange)
        {
            currentState = BossState.Chase;
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
            currentState = BossState.Idle;
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
            currentState = BossState.Attack;
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
            currentState = BossState.Chase;
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
        currentState = BossState.Chase;
    }

    public void TakeDamage(float damage)
    {
        
    }

    public void Die()
    {
        isDead = true;
        animator.SetBool(isDeadHash, true);
        agent.isStopped = true;
        agent.enabled = false;
    }

    // hien thi vong tron detection range va attack range
    // trong editor de de dang dieu chinh
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
