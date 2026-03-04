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

    [Header("SFX")]
    [Tooltip("Fireball flying sound (looping) - auto-plays on spawn")]
    public AudioClip sfxFlyLoop;
    [Tooltip("Explosion sound when fireball hits target")]
    public AudioClip sfxHitExplosion;
    [Range(0f, 1f)] public float sfxVolume = 0.6f;

    private Vector3 _startPos;
    private AudioSource _audioSource;

    private void Start()
    {
        _startPos = transform.position;

        // Setup AudioSource for fly loop sound
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f;
        _audioSource.minDistance = 1f;
        _audioSource.maxDistance = 20f;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;

        if (sfxFlyLoop != null)
        {
            _audioSource.clip = sfxFlyLoop;
            _audioSource.loop = true;
            _audioSource.volume = sfxVolume * 0.5f;
            _audioSource.Play();
        }
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

        // Play explosion sound at impact position (persists after Destroy)
        if (sfxHitExplosion != null)
        {
            AudioSource.PlayClipAtPoint(sfxHitExplosion, transform.position, sfxVolume);
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

