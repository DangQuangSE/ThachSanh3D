using UnityEngine;

public class VfxController : MonoBehaviour
{
    [Header("VFX Prefabs - Keo tha Particle System vao day")]
    [Tooltip("VFX cho don Punch")]
    [SerializeField] private GameObject punchVfxPrefab;
    [Tooltip("VFX cho don Swipe")]
    [SerializeField] private GameObject swipeVfxPrefab;
    [Tooltip("VFX cho don Roar")]
    [SerializeField] private GameObject roarVfxPrefab;
    [Tooltip("VFX cho don Jump Attack")]
    [SerializeField] private GameObject jumpAttackVfxPrefab;

    [Header("Spawn Points - Keo bone tu Hierarchy vao day")]
    [Tooltip("Tay phai (Punch + JumpAttack). Tim bone: Armature > Spine > RightArm > RightHand")]
    [SerializeField] private Transform rightHandSpawnPoint;
    [Tooltip("Tay trai (Swipe + JumpAttack). Tim bone: Armature > Spine > LeftArm > LeftHand")]
    [SerializeField] private Transform leftHandSpawnPoint;
    [Tooltip("Mieng (Roar). Tim bone: Armature > Spine > Neck > Head")]
    [SerializeField] private Transform mouthSpawnPoint;

    // Luu instance VFX dang chay de tranh spawn trung
    private GameObject currentVfxRight;
    private GameObject currentVfxLeft;
    private GameObject currentVfxMouth;

    /// <summary>
    /// Spawn VFX theo loai tan cong. Goi tu ChanTinhBossController hoac Animation Event.
    /// 0 = Punch (tay phai), 1 = Swipe (tay trai), 2 = Roar (mieng), 3 = JumpAttack (hai tay)
    /// </summary>
    public void PlayAttackVfx(int attackType)
    {
        GameObject prefab = GetVfxPrefab(attackType);
        if (prefab == null) return;

        // JumpAttack: spawn VFX o CA HAI TAY
        if (attackType == 3)
        {
            SpawnVfxAtPoint(prefab, rightHandSpawnPoint, ref currentVfxRight);
            SpawnVfxAtPoint(prefab, leftHandSpawnPoint, ref currentVfxLeft);
            return;
        }

        Transform spawnPoint = GetSpawnPoint(attackType);
        ref GameObject currentVfx = ref GetCurrentVfxRef(attackType);
        SpawnVfxAtPoint(prefab, spawnPoint, ref currentVfx);
    }

    /// <summary>
    /// Huy tat ca VFX dang chay. Goi tu OnAttackComplete hoac khi can dung VFX.
    /// </summary>
    public void StopAllVfx()
    {
        DestroyVfx(ref currentVfxRight);
        DestroyVfx(ref currentVfxLeft);
        DestroyVfx(ref currentVfxMouth);
    }

    /// <summary>
    /// Spawn VFX tai vi tri tuy chinh (vi du: vi tri player).
    /// </summary>
    public void PlayAttackVfxAtPosition(int attackType, Vector3 position)
    {
        GameObject prefab = GetVfxPrefab(attackType);
        if (prefab == null) return;

        GameObject vfxInstance = Instantiate(prefab, position, Quaternion.identity);
        StopLooping(vfxInstance);
        Destroy(vfxInstance, GetParticleDuration(vfxInstance));
    }

    private void SpawnVfxAtPoint(GameObject prefab, Transform spawnPoint, ref GameObject currentVfx)
    {
        // Neu VFX dang chay -> KHONG spawn lai (tranh lap VFX khi animation chua het)
        if (currentVfx != null) return;

        Transform parent = spawnPoint != null ? spawnPoint : transform;

        // Instantiate VFX lam con cua bone -> VFX bam theo tay khi animation chay
        currentVfx = Instantiate(prefab, parent);
        currentVfx.transform.localPosition = Vector3.zero;
        currentVfx.transform.localRotation = Quaternion.identity;

        // Tat looping de VFX chi phat 1 lan duy nhat, KHONG lap lai
        StopLooping(currentVfx);

        // KHONG tu huy - VFX instance ton tai cho den khi OnAttackComplete() goi StopAllVfx()
        // Dam bao VFX khong bi destroy truoc khi animation ket thuc
    }

    private void DestroyVfx(ref GameObject vfx)
    {
        if (vfx != null)
        {
            Destroy(vfx);
            vfx = null;
        }
    }

    /// <summary>
    /// Tat looping tren tat ca ParticleSystem con de VFX chi chay 1 lan.
    /// </summary>
    private void StopLooping(GameObject vfxInstance)
    {
        var particles = vfxInstance.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            var main = ps.main;
            main.loop = false;
        }
    }

    /// <summary>
    /// Lay thoi gian dai nhat cua tat ca ParticleSystem (duration + startLifetime).
    /// Chi dung cho PlayAttackVfxAtPosition (VFX khong gan bone).
    /// </summary>
    private float GetParticleDuration(GameObject vfxInstance)
    {
        float maxDuration = 1f;
        var particles = vfxInstance.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            var main = ps.main;
            float total = main.duration + main.startLifetime.constantMax;
            if (total > maxDuration)
                maxDuration = total;
        }
        return maxDuration;
    }

    private ref GameObject GetCurrentVfxRef(int attackType)
    {
        switch (attackType)
        {
            case 0: return ref currentVfxRight;
            case 1: return ref currentVfxLeft;
            case 2: return ref currentVfxMouth;
            default: return ref currentVfxRight;
        }
    }

    private Transform GetSpawnPoint(int attackType)
    {
        switch (attackType)
        {
            case 0: return rightHandSpawnPoint;
            case 1: return leftHandSpawnPoint;
            case 2: return mouthSpawnPoint;
            default: return null;
        }
    }

    private GameObject GetVfxPrefab(int attackType)
    {
        switch (attackType)
        {
            case 0: return punchVfxPrefab;
            case 1: return swipeVfxPrefab;
            case 2: return roarVfxPrefab;
            case 3: return jumpAttackVfxPrefab;
            default:
                Debug.LogWarning($"VfxController: Khong tim thay VFX cho attackType {attackType}");
                return null;
        }
    }
}