using UnityEngine;
using UnityEngine.AI;

public class BossAI_NavMesh : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform target; // Player

    [Header("Ranges")]
    [SerializeField] private float aggroRange = 12f;
    [SerializeField] private float attackRange = 2.2f;
    [SerializeField] private float jumpAttackRange = 6f;

    [Header("Timings")]
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float roarChanceOnEngage = 0.6f;

    [Header("Anim Tuning")]
    [SerializeField] private float speedDamp = 0.1f; // smoothing for Speed param

    private float nextAttackTime;
    private bool engaged;
    private bool isDead;
    private bool isAttacking;

    void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponentInChildren<Animator>();

        // NavMeshAgent drives movement/rotation
        agent.updatePosition = true;
        agent.updateRotation = true;

        // Root Motion OFF on Animator component (set in Inspector)
    }

    void Update()
    {
        if (isDead || !target) return;

        float dist = Vector3.Distance(transform.position, target.position);

        // 1) Engage
        if (!engaged && dist <= aggroRange)
        {
            engaged = true;

            // Optional: roar once when first engage
            if (Random.value < roarChanceOnEngage)
                TriggerAttack("Roar");
        }

        // 2) Decide state
        if (isAttacking)
        {
            // While attacking, stop agent
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        else
        {
            if (dist > aggroRange)
            {
                // lost target -> idle
                agent.isStopped = true;
                agent.ResetPath();
            }
            else if (dist > attackRange)
            {
                // chase
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
            else
            {
                // in melee range
                agent.isStopped = true;
                agent.ResetPath();

                FaceTargetQuick(target.position);

                if (Time.time >= nextAttackTime)
                {
                    ChooseAndTriggerAttack(dist);
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }

        // 3) Update Animator params every frame
        UpdateAnimatorParams();
    }

    private void UpdateAnimatorParams()
    {
        // Speed: based on agent velocity magnitude normalized by agent speed
        float v = agent.velocity.magnitude;
        float normalizedSpeed = (agent.speed > 0.01f) ? Mathf.Clamp01(v / agent.speed) : 0f;

        // Smooth set Speed
        anim.SetFloat("Speed", normalizedSpeed, speedDamp, Time.deltaTime);

        // TurnAngle: angle between forward and desired movement direction (or target direction when stopped)
        Vector3 desiredDir;

        if (agent.velocity.sqrMagnitude > 0.05f)
            desiredDir = agent.velocity.normalized;
        else if (engaged)
            desiredDir = (target.position - transform.position).normalized;
        else
            desiredDir = transform.forward;

        float turnAngle = Vector3.SignedAngle(transform.forward, new Vector3(desiredDir.x, 0, desiredDir.z), Vector3.up);
        anim.SetFloat("TurnAngle", turnAngle);
    }

    private void ChooseAndTriggerAttack(float dist)
    {
        // Simple logic:
        // If target a bit far but still in combat -> JumpAttack sometimes
        if (dist <= jumpAttackRange && dist > attackRange + 0.5f && Random.value < 0.4f)
        {
            TriggerAttack("JumpAttack");
            return;
        }

        // Random between Swipe and Punch
        if (Random.value < 0.5f) TriggerAttack("Swipe");
        else TriggerAttack("Punch");
    }

    private void TriggerAttack(string triggerName)
    {
        // Set trigger -> AnyState transition to that attack
        anim.ResetTrigger("Roar");
        anim.ResetTrigger("Swipe");
        anim.ResetTrigger("Punch");
        anim.ResetTrigger("JumpAttack");
        anim.ResetTrigger("Flex");

        anim.SetTrigger(triggerName);

        // Mark attacking so agent stops until animation ends (we'll end via Animation Event)
        isAttacking = true;
    }

    // Called by Animation Event at the END of each attack clip
    public void AE_AttackFinished()
    {
        isAttacking = false;
    }

    // Optional: called by event near the HIT frame
    public void AE_DoDamage()
    {
        // TODO: check overlap sphere / hitbox here
        // Keep it simple now.
    }

    private void FaceTargetQuick(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 12f);
    }

    // Call this when boss HP <= 0
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        agent.isStopped = true;
        agent.ResetPath();

        anim.SetBool("IsDead", true);
    }
}
