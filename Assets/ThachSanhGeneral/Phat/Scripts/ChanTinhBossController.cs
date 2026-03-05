using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Required for Coroutine

public class ChanTinhBossController : MonoBehaviour
{
    [Header("References")]
    private NavMeshAgent agent; // NavMeshAgent for movement control
    private Animator animator; // Animator for animation control
    private Transform player; // Reference to the player's position

    // ===== HEALTH SYSTEM =====
    [Header("Boss Stats")]
    [Tooltip("Full hp of boss")]
    [SerializeField] private float maxHealth = 1000f;
    private float currentHealth;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2.5f; // Distance to start attacking
    [SerializeField] private float detectionRange = 15f; // Distance to detect player and start chasing
    [SerializeField] private float walkSpeed = 2f; // Walk speed
    [SerializeField] private float runSpeed = 5f; // Run speed when far from player

    // Deal damage
    [Header("Attack Damage Settings")]
    [Tooltip("Damage per attack")]
    [SerializeField] private float attackDamage = 30f;
    [Tooltip("Attack hitbox spawn position")]
    [SerializeField] private Transform attackPoint;
    [Tooltip("Attack hitbox radius")]
    [SerializeField] private float attackRadius = 1.5f;
    [Tooltip("Player layer")]
    [SerializeField] private LayerMask playerLayer;

    // ===== VISUAL FEEDBACK =====
    [Header("Visual Feedback")]
    [Tooltip("Color when taking damage")]
    [SerializeField] private Color damageColor = Color.red;
    [Tooltip("Damage flash effect duration")]
    [SerializeField] private float damageFlashDuration = 0.1f;

    private Renderer[] renderers;
    private Color[] originalColors;

    [Header("VFX")]
    [Tooltip("Drag and drop the GameObject containing VfxController here")]
    [SerializeField] private VfxController vfxController;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip punchSound;
    [SerializeField] private AudioClip swipeSound;
    [SerializeField] private AudioClip roarSound;
    [SerializeField] private AudioClip jumpAttackSound;
    [SerializeField] private AudioClip flexingSound;
    [SerializeField] private AudioClip dieSound;

    [Header("Footstep Sound")]
    [SerializeField] private AudioClip footstepSound;
    [Tooltip("Volume khi Walk (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float walkFootstepVolume = 0.4f;
    [Tooltip("Volume khi Run (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float runFootstepVolume = 0.7f;
    [Tooltip("Pitch khi Walk")]
    [SerializeField] private float walkFootstepPitch = 0.9f;
    [Tooltip("Pitch khi Run")]
    [SerializeField] private float runFootstepPitch = 1.1f;

    [Header("Attack Cooldowns")]
    [SerializeField] private float roarCooldown = 10f; // Cooldown time for roar attack
    [SerializeField] private float swipeCooldown = 3f;
    [SerializeField] private float punchCooldown = 2f;
    [SerializeField] private float jumpAttackCooldown = 8f;

    private float lastAttackTime; // Time of the last attack performed
    private float currentCooldown; // Current cooldown of the ongoing attack
    private bool isAttacking = false; // Flag to check if boss is currently attacking
    private bool isDead = false; // Flag to check if boss is dead

    // Animation parameter hashes
    private int speedHash; // Hash for "Speed" parameter in Animator
    private int isDeadHash;
    private int isInCombatHash;
    private int roarHash;
    private int swipeHash;
    private int punchHash;
    private int jumpAttackHash;
    private int hitHash; // Hit animation hash

    private enum BossState
    {
        Idle, // Standing still, doing nothing
        Patrol, // Patrol state (if applicable)
        Chase, // Chasing player when detected
        Attack // Attacking when close to player
    }
    private BossState currentState = BossState.Idle;

    void Start()
    {
        // ===== INITIALIZE HEALTH =====
        currentHealth = maxHealth;
        // Get references to NavMeshAgent, Animator and player
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // ===== IGNORE PHYSICAL COLLISION BETWEEN BOSS AND PLAYER =====
        Collider bossCollider = GetComponent<Collider>();
        Collider playerCollider = player.GetComponent<Collider>();
        if (bossCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(bossCollider, playerCollider);
        }

        // ===== GET RENDERERS FOR DAMAGE FLASH =====
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
        hitHash = Animator.StringToHash("Hit");

        // Auto-get AudioSource if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        agent.updateRotation = true; // Allow NavMeshAgent to auto-rotate towards movement direction
        agent.updatePosition = true; // Allow NavMeshAgent to auto-update boss position during movement
    }

    void Update()
    {
        if (isDead) return; // If boss is dead, do nothing

        // Calculate distance to player
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

    // State change handler
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
                agent.autoBraking = false; // Disable auto braking for smooth movement
                animator.SetBool(isInCombatHash, true);
                break;

            case BossState.Attack:
                agent.isStopped = true;
                agent.autoBraking = true; // Enable auto braking for precise stopping
                break;
        }
    }

    // Handle Idle state: stand still, do nothing, but if player enters detection range
    // then switch to Chase state
    void HandleIdleState(float distanceToPlayer)
    {
        agent.isStopped = true; // Stop when in Idle state
        animator.SetBool(isInCombatHash, false); // Disable combat animation in Idle state

        // Check if player is within detection range
        if (distanceToPlayer <= detectionRange)
        {
            //currentState = BossState.Chase;
            ChangeState(BossState.Chase);
            animator.SetBool(isInCombatHash, true);
        }
    }

    // Handle Chase state: chase the player, if player leaves detection range switch to Idle,
    // if player enters attack range switch to Attack state
    void HandleChaseState(float distanceToPlayer)
    {
        // If player leaves detection range, switch to Idle
        if (distanceToPlayer > detectionRange)
        {
            //currentState = BossState.Idle;
            ChangeState(BossState.Idle);
            return;
        }

        agent.isStopped = false; // Allow movement
        agent.SetDestination(player.position); // Move towards player position

        // Adjust movement speed: run when far, walk when close
        if (distanceToPlayer > attackRange * 2)
        {
            agent.speed = runSpeed; // Run when far
        }
        else
        {
            agent.speed = walkSpeed; // Walk when close
        }

        // Check if player is within attack range to switch to Attack state
        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            //currentState = BossState.Attack;
            ChangeState(BossState.Attack);
        }
    }

    void HandleAttackState(float distanceToPlayer)
    {
        agent.isStopped = true; // Stop when attacking

        // Face the player
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        // If player leaves attack range, switch to Chase
        if (distanceToPlayer > attackRange)
        {
            //currentState = BossState.Chase;
            ChangeState(BossState.Chase);
            return;
        }
        // If not attacking and cooldown has expired, perform a random attack
        if (!isAttacking && Time.time >= lastAttackTime + currentCooldown)
        {
            PerformRandomAttack();
        }
    }

    // Perform a random attack from the 4 available attack types
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

        // Spawn VFX for the attack
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

    // This method is called from animation event at the end of each attack
    // to notify that the attack has finished and the boss can transition to another state
    public void OnAttackComplete()
    {
        isAttacking = false;
        //currentState = BossState.Chase;
        ChangeState(BossState.Chase);

        // Stop VFX when attack animation ends
        if (vfxController != null)
        {
            vfxController.StopAllVfx();
        }

        Debug.Log("Chan Tinh Boss attack animation completed");
    }

    // Called from Animation Event at the landing frame of Jump Attack
    // to spawn ground explosion VFX
    public void OnJumpAttackLand()
    {
        if (vfxController != null)
        {
            vfxController.PlayJumpAttackGroundVfx();
        }

        Debug.Log("Chan Tinh Boss Jump Attack landed - Ground VFX spawned!");
    }

    // Called from Animation Event when the attack animation reaches the damage frame
    public void DealDamageToPlayer()
    {
        // If no attackPoint is set, use the boss position
        if (attackPoint == null)
        {
            attackPoint = transform;
        }

        // Find all colliders within attack radius
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider hitCollider in hitColliders)
        {
            Debug.Log("Chan Tinh Boss hit: " + hitCollider.name);

            // Try to deal damage to player
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsDead())
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Chan Tinh Boss dealt {attackDamage} damage to {hitCollider.name}!");

                // Spawn hit VFX at player center (bounds.center = giữa thân player)
                if (vfxController != null)
                {
                    vfxController.PlayHitVfx(hitCollider.bounds.center);
                }
            }

            // Can add other components if the player has multiple health scripts
            // Example: ThirdPersonController health system
        }
    }

    public void Die()
    {
        isDead = true;
        animator.SetBool(isDeadHash, true);
        agent.isStopped = true;
        agent.enabled = false;

        Debug.Log("Chan Tinh Boss died!");

        // Disable collider to stop receiving damage
        GetComponent<Collider>().enabled = false;

        // Destroy after 5 seconds
        Destroy(gameObject, 5f);
    }

    public bool IsDead()
    {
        return isDead;
    }

    // ===== TAKEDAMAGE METHOD =====
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Trigger Hit animation if available
        if (animator != null)
        {
            animator.SetTrigger(hitHash);
        }

        // Red flash effect
        StartCoroutine(DamageFlash());

        Debug.Log($"Chan Tinh Boss took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // If in Idle state, switch to Chase
        if (currentState == BossState.Idle)
        {
            ChangeState(BossState.Chase);
        }

        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    // ===== RED FLASH EFFECT WHEN TAKING DAMAGE =====
    private IEnumerator DamageFlash()
    {
        // Change color to red
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material.HasProperty("_Color"))
            {
                renderer.material.color = damageColor;
            }
        }

        yield return new WaitForSeconds(damageFlashDuration);

        // Restore original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    // ===== GET HEALTH PERCENTAGE METHOD =====
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }


    // ===== SOUND EFFECTS (call from Animation Events) =====
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // --- Gọi từ Animation Event tại frame bạn muốn ---
    public void PlayPunchSound()      { PlaySound(punchSound); }
    public void PlaySwipeSound()      { PlaySound(swipeSound); }
    public void PlayRoarSound()       { PlaySound(roarSound); }
    public void PlayJumpAttackSound() { PlaySound(jumpAttackSound); }
    public void PlayFlexingSound()    { PlaySound(flexingSound); }
    public void PlayDieSound()        { PlaySound(dieSound); }

    // --- Footstep: gọi từ Animation Event tại frame chân chạm đất ---
    public void PlayWalkFootstep()
    {
        if (audioSource != null && footstepSound != null)
        {
            audioSource.pitch = walkFootstepPitch;
            audioSource.PlayOneShot(footstepSound, walkFootstepVolume);
            audioSource.pitch = 1f; // reset pitch
        }
    }

    public void PlayRunFootstep()
    {
        if (audioSource != null && footstepSound != null)
        {
            audioSource.pitch = runFootstepPitch;
            audioSource.PlayOneShot(footstepSound, runFootstepVolume);
            audioSource.pitch = 1f; // reset pitch
        }
    }

    // Display detection range and attack range circles
    // in the editor for easy adjustment
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // ===== Display attack hitbox =====
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
