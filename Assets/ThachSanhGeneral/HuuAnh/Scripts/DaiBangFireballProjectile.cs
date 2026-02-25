using UnityEngine;

/// <summary>
/// Đạn Fireball của boss Đại Bàng (MagicAttack).
/// Gắn vào prefab FX_Fireball (HuuAnh). Khi boss dùng MagicAttack, BossDaiBangController đã spawn prefab
/// quay về phía player, script này sẽ cho nó bay thẳng và gây sát thương khi chạm player.
/// </summary>
public class DaiBangFireballProjectile : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Tốc độ bay của fireball")]
    public float speed = 10f;

    [Tooltip("Khoảng cách tối đa trước khi tự hủy")]
    public float maxDistance = 30f;

    [Header("Damage")]
    [Tooltip("Sát thương gây ra cho player khi trúng")]
    public float damage = 20f;

    [Tooltip("Layer của Player (phải trùng layer object Player trong scene)")]
    public LayerMask playerLayer;

    [Header("VFX khi trúng (tuỳ chọn)")]
    [Tooltip("Hiệu ứng nổ khi fireball chạm player / tường")]
    public GameObject hitVfx;

    private Vector3 _startPos;

    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        // Bay thẳng theo forward
        transform.position += transform.forward * (speed * Time.deltaTime);

        // Hủy nếu bay quá xa (phòng trường hợp không trúng gì)
        if (Vector3.Distance(_startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ xử lý va chạm với layer Player (nếu có set)
        if (playerLayer != 0 && ((1 << other.gameObject.layer) & playerLayer) == 0)
            return;

        // Tìm PlayerHealth trên object hoặc cha của nó
        PlayerHealth ph = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (ph != null && !ph.IsDead())
        {
            ph.TakeDamage(damage);
        }

        // Spawn hiệu ứng nổ (nếu có)
        if (hitVfx != null)
        {
            GameObject vfx = Instantiate(hitVfx, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        // Hủy fireball sau khi trúng
        Destroy(gameObject);
    }
}

