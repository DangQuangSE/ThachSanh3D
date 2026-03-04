using UnityEngine;

public class VfxController : MonoBehaviour
{
    [Header("VFX Prefabs - Drag Particle System here")]
    [Tooltip("VFX for Punch attack")]
    [SerializeField] private GameObject punchVfxPrefab;
    [Tooltip("VFX for Swipe attack")]
    [SerializeField] private GameObject swipeVfxPrefab;
    [Tooltip("VFX for Roar attack")]
    [SerializeField] private GameObject roarVfxPrefab;
    [Tooltip("VFX for Jump Attack")]
    [SerializeField] private GameObject jumpAttackVfxPrefab;
    [Tooltip("Ground explosion VFX when Jump Attack lands")]
    [SerializeField] private GameObject jumpAttackGroundVfxPrefab;
    [Tooltip("VFX hiệu ứng hit khi boss đánh trúng player")]
    [SerializeField] private GameObject hitVfxPrefab;

    [Header("Spawn Points - Drag bones from Hierarchy here")]
    [Tooltip("Right hand (Punch + JumpAttack). Find bone: Armature > Spine > RightArm > RightHand")]
    [SerializeField] private Transform rightHandSpawnPoint;
    [Tooltip("Left hand (Swipe + JumpAttack). Find bone: Armature > Spine > LeftArm > LeftHand")]
    [SerializeField] private Transform leftHandSpawnPoint;
    [Tooltip("Mouth (Roar). Find bone: Armature > Spine > Neck > Head")]
    [SerializeField] private Transform mouthSpawnPoint;
    [Tooltip("Boss feet (Ground VFX). Leave empty to use transform.position")]
    [SerializeField] private Transform groundSpawnPoint;

    // Store running VFX instances to avoid duplicate spawns
    private GameObject currentVfxRight;
    private GameObject currentVfxLeft;
    private GameObject currentVfxMouth;
    private GameObject currentVfxGround;

    /// <summary>
    /// Spawn VFX by attack type. Called from ChanTinhBossController or Animation Event.
    /// 0 = Punch (right hand), 1 = Swipe (left hand), 2 = Roar (mouth), 3 = JumpAttack (both hands)
    /// </summary>
    public void PlayAttackVfx(int attackType)
    {
        GameObject prefab = GetVfxPrefab(attackType);
        if (prefab == null) return;

        // JumpAttack: spawn VFX on BOTH HANDS
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
    /// Destroy all running VFX. Called from OnAttackComplete or when VFX needs to be stopped.
    /// </summary>
    public void StopAllVfx()
    {
        DestroyVfx(ref currentVfxRight);
        DestroyVfx(ref currentVfxLeft);
        DestroyVfx(ref currentVfxMouth);
        DestroyVfx(ref currentVfxGround);
    }

    /// <summary>
    /// Spawn ground explosion VFX when Jump Attack lands.
    /// Called from Animation Event or ChanTinhBossController.OnJumpAttackLand().
    /// VFX spawns at world position (not attached to bone), auto-destroys after playing.
    /// </summary>
    public void PlayJumpAttackGroundVfx()
    {
        if (jumpAttackGroundVfxPrefab == null)
        {
            Debug.LogWarning("VfxController: jumpAttackGroundVfxPrefab is not assigned!");
            return;
        }

        // If ground VFX is already playing -> don't spawn again
        if (currentVfxGround != null) return;

        // Get spawn position: use groundSpawnPoint if available, otherwise use current boss position
        Vector3 spawnPos = groundSpawnPoint != null ? groundSpawnPoint.position : transform.position;
        // Place VFX on the ground (y = spawnPos.y)
        currentVfxGround = Instantiate(jumpAttackGroundVfxPrefab, spawnPos, Quaternion.identity);

        // Disable looping so VFX only plays once
        StopLooping(currentVfxGround);

        // Auto-destroy after particle finishes playing
        float duration = GetParticleDuration(currentVfxGround);
        Destroy(currentVfxGround, duration);
    }

    /// <summary>
    /// Spawn VFX at a custom position (e.g. player position).
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
        // If VFX is already playing -> DON'T spawn again (avoid duplicate VFX while animation is still running)
        if (currentVfx != null) return;

        Transform parent = spawnPoint != null ? spawnPoint : transform;

        // Instantiate VFX as child of bone -> VFX follows hand during animation
        currentVfx = Instantiate(prefab, parent);
        currentVfx.transform.localPosition = Vector3.zero;
        currentVfx.transform.localRotation = Quaternion.identity;

        // Disable looping so VFX plays only once, without repeating
        StopLooping(currentVfx);

        // DON'T auto-destroy - VFX instance lives until OnAttackComplete() calls StopAllVfx()
        // Ensures VFX is not destroyed before the animation finishes
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
    /// Disable looping on all child ParticleSystems so VFX plays only once.
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
    /// Get the longest duration of all ParticleSystems (duration + startLifetime).
    /// Only used for PlayAttackVfxAtPosition (VFX not attached to bone).
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
                Debug.LogWarning($"VfxController: No VFX found for attackType {attackType}");
                return null;
        }
    }

    /// <summary>
    /// Spawn hit VFX tại vị trí bị đánh (ví dụ: vị trí player).
    /// VFX spawn tại world position, tự hủy sau khi phát xong.
    /// </summary>
    public void PlayHitVfx(Vector3 position)
    {
        if (hitVfxPrefab == null)
        {
            Debug.LogWarning("VfxController: hitVfxPrefab is not assigned!");
            return;
        }

        GameObject hitVfx = Instantiate(hitVfxPrefab, position, Quaternion.identity);
        StopLooping(hitVfx);

        // Force play ngay lập tức để tránh bị trễ
        var particles = hitVfx.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            var main = ps.main;
            main.startDelay = 0f; // Xóa delay nếu có trong prefab
            ps.Clear();           // Xóa particle cũ
            ps.Play(true);        // Bắt đầu phát ngay
        }

        float duration = GetParticleDuration(hitVfx);
        Destroy(hitVfx, duration);
    }
}