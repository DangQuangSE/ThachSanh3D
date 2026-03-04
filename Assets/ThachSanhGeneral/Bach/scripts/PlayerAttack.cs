using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Damage dealt per attack")]
    public float attackDamage = 25f;

    [Tooltip("Damage dealt by E Skill (Attack360)")]
    public float eskillDamage = 50f;
    
    [Tooltip("Attack hitbox center (child transform)")]
    public Transform attackPoint;
    
    [Tooltip("Radius of attack hitbox")]
    public float attackRadius = 1.5f;

    [Tooltip("Radius of E Skill hitbox (larger than normal attack)")]
    public float eskillRadius = 3f;
    
    [Tooltip("Layers that can be damaged (Boss, Enemy, etc.)")]
    public LayerMask damageableLayers;
    
    [Header("Debug")]
    [Tooltip("Show attack range in Scene view")]
    public bool showDebugGizmos = true;
    
    private Animator animator;
    private bool hasAnimator;
    
    void Start()
    {
        hasAnimator = TryGetComponent(out animator);
        
        // If no attack point assigned, use player position
        if (attackPoint == null)
        {
            // Create attack point in front of player
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(0, 1f, 1.5f);
            attackPoint = attackPointObj.transform;
            
            Debug.LogWarning("No attack point assigned! Created default attack point.");
        }
    }

    private void Update()
    {
        if (!hasAnimator) return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float nt = state.normalizedTime % 1f;

        // E Skill damage at hit window
        if (state.IsName("Attack360"))
        {
            if (nt >= 0.3f && nt <= 0.7f && !_eskillDamageDealt)
            {
                DealESkillDamage();
                _eskillDamageDealt = true;
            }
        }
        else
        {
            _eskillDamageDealt = false;
        }
    }

    private bool _eskillDamageDealt = false;
    
    // Called by Animation Event during attack animation
    public void OnAttackHit()
    {
        DealDamage();
    }
    
    // Can also be called manually
    public void DealDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRadius, damageableLayers);
        
        foreach (Collider hitCollider in hitColliders)
        {
            // SendMessage works with any boss/enemy that has TakeDamage(float) and IsDead()
            hitCollider.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"Player hit {hitCollider.name} for {attackDamage} damage!");
        }
    }

    public void DealESkillDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, eskillRadius, damageableLayers);

        foreach (Collider hitCollider in hitColliders)
        {
            hitCollider.SendMessage("TakeDamage", eskillDamage, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"E Skill hit {hitCollider.name} for {eskillDamage} damage!");
        }
    }
    
    // For testing in Inspector
    [ContextMenu("Test Attack")]
    private void TestAttack()
    {
        DealDamage();
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        if (attackPoint == null) return;
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

        // Draw E Skill range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, eskillRadius);
    }
}
