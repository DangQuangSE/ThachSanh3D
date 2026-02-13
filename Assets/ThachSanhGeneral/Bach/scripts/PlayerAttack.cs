using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Damage dealt per attack")]
    public float attackDamage = 25f;
    
    [Tooltip("Attack hitbox center (child transform)")]
    public Transform attackPoint;
    
    [Tooltip("Radius of attack hitbox")]
    public float attackRadius = 1.5f;
    
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
            // Try to damage Boss
            BossController boss = hitCollider.GetComponent<BossController>();
            if (boss != null && !boss.IsDead())
            {
                boss.TakeDamage(attackDamage);
                Debug.Log($"Player hit {hitCollider.name} for {attackDamage} damage!");
                continue;
            }
            
            // Try to damage other enemies (if you have Enemy script)
            // EnemyHealth enemy = hitCollider.GetComponent<EnemyHealth>();
            // if (enemy != null)
            // {
            //     enemy.TakeDamage(attackDamage);
            // }
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
    }
}
