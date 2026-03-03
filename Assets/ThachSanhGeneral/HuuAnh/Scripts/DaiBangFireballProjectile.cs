using UnityEngine;

/// <summary>
/// Eagle Boss Fireball projectile (MagicAttack).
/// Attach to FX_Fireball prefab (HuuAnh). When boss uses MagicAttack, BossDaiBangController spawns this prefab
/// facing the player. This script makes it fly forward and deal damage on player collision.
/// </summary>
public class DaiBangFireballProjectile : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Fireball flight speed")]
    public float speed = 10f;

    [Tooltip("Max distance before auto-destroy")]
    public float maxDistance = 30f;

    [Header("Damage")]
    [Tooltip("Damage dealt to player on hit")]
    public float damage = 20f;

    [Tooltip("Player Layer (must match Player object's layer in scene)")]
    public LayerMask playerLayer;

    [Header("VFX on hit (optional)")]
    [Tooltip("Explosion VFX when fireball hits player / wall")]
    public GameObject hitVfx;

    private Vector3 _startPos;

    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        // Fly straight forward
        transform.position += transform.forward * (speed * Time.deltaTime);

        // Destroy if traveled too far (failsafe when nothing is hit)
        if (Vector3.Distance(_startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only process collision with Player layer (if set)
        if (playerLayer != 0 && ((1 << other.gameObject.layer) & playerLayer) == 0)
            return;

        // Find PlayerHealth on object or its parent
        PlayerHealth ph = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (ph != null && !ph.IsDead())
        {
            ph.TakeDamage(damage);
        }

        // Spawn explosion VFX (if assigned)
        if (hitVfx != null)
        {
            GameObject vfx = Instantiate(hitVfx, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        // Destroy fireball after hit
        Destroy(gameObject);
    }
}

