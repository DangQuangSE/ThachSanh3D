using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossController : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Chase,
        Attack,
        Death
    }

    [Header("Boss Stats")]
    [Tooltip("Máu t?i ?a c?a boss")]
    public float maxHealth = 1000f;
    [Tooltip("Sát th??ng m?i ?òn t?n công")]
    public float attackDamage = 30f;
    [Tooltip("T?c ?? di chuy?n")]
    public float moveSpeed = 3.5f;
    
    [Header("Combat Settings")]
    [Tooltip("Kho?ng cách phát hi?n player")]
    public float detectionRange = 15f;
    [Tooltip("Kho?ng cách t?n công")]
    public float attackRange = 2.5f;
    [Tooltip("Th?i gian h?i gi?a các ?òn ?ánh")]
    public float attackCooldown = 2f;
    [Tooltip("Kho?ng cách t?i ?a ?u?i theo player")]
    public float maxChaseDistance = 30f;
    
    [Header("Attack Hitbox")]
    [Tooltip("V? trí spawn attack hitbox")]
    public Transform attackPoint;
    [Tooltip("Bán kính hitbox t?n công")]
    public float attackRadius = 1.5f;
    [Tooltip("Layer c?a player")]
    public LayerMask playerLayer;
    
    [Header("References")]
    [Tooltip("M?c tiêu (Player)")]
    public Transform target;
    
    [Header("Visual Feedback")]
    [Tooltip("Màu s?c khi nh?n damage")]
    public Color damageColor = Color.red;
    [Tooltip("Th?i gian hi?u ?ng damage")]
    public float damageFlashDuration = 0.1f;

    private BossState currentState = BossState.Idle;
    private float currentHealth;
    private NavMeshAgent agent;
    private Animator animator;
    private float lastAttackTime;
    private bool isDead = false;
    private Renderer[] renderers;
    private Color[] originalColors;
    private bool hasAnimator;
    
    private int animIDSpeed;
    private int animIDAttack;
    private int animIDHit;
    private int animIDDeath;
    
    private Vector3 spawnPosition;

    void Start()
    {
        currentHealth = maxHealth;
        spawnPosition = transform.position;
        
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
        
        AssignAnimationIDs();
        
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
    }

    private void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDAttack = Animator.StringToHash("Attack");
        animIDHit = Animator.StringToHash("Hit");
        animIDDeath = Animator.StringToHash("Death");
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

    private void PerformAttack()
    {
        if (hasAnimator)
        {
            animator.SetTrigger(animIDAttack);
        }
    }

    public void DealDamageToPlayer()
    {
        if (attackPoint == null)
        {
            attackPoint = transform;
        }
        
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayer);
        
        foreach (Collider hitCollider in hitColliders)
        {
            Debug.Log("Boss hit: " + hitCollider.name);
        }
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
        
        if (hasAnimator)
        {
            animator.SetTrigger(animIDHit);
        }
        
        StartCoroutine(DamageFlash());
        
        Debug.Log($"Boss took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentState == BossState.Idle)
        {
            ChangeState(BossState.Chase);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
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
        
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        
        if (hasAnimator)
        {
            animator.SetTrigger(animIDDeath);
        }
        
        Debug.Log("Boss died!");
        
        GetComponent<Collider>().enabled = false;
        
        Destroy(gameObject, 5f);
    }

    private void UpdateAnimator()
    {
        if (!hasAnimator) return;
        
        float speed = 0f;
        
        if (agent != null && !agent.isStopped)
        {
            speed = agent.velocity.magnitude;
        }
        
        animator.SetFloat(animIDSpeed, speed);
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
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxChaseDistance);
    }
}
