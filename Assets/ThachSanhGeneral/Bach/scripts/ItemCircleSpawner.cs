using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawner that manages random spawning of item effect circles on the map.
/// Circles appear randomly near the player, within a configurable range.
/// Supports 2 types: Healing Circle and Damage Boost Circle.
/// </summary>
public class ItemCircleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Minimum spawn distance from the player")]
    public float minSpawnDistance = 5f;

    [Tooltip("Maximum spawn distance from the player")]
    public float maxSpawnDistance = 15f;

    [Tooltip("Time between each spawn (seconds)")]
    public float spawnInterval = 30f;

    [Tooltip("Maximum number of circles that can exist on the map at the same time")]
    public int maxCirclesOnMap = 3;

    [Tooltip("Delay before first spawn (seconds)")]
    public float initialDelay = 10f;

    [Header("Circle Settings")]
    [Tooltip("Circle radius (adjustable)")]
    public float circleRadius = 3f;

    [Tooltip("Time required to stand inside the circle to activate")]
    public float activationTime = 5f;

    [Tooltip("How long the circle exists before auto-expiring (seconds). 0 = never expires.")]
    public float circleLifetime = 20f;

    [Tooltip("Duration of the fade-out effect before the circle disappears (seconds)")]
    public float fadeOutDuration = 3f;

    [Tooltip("Duration of the damage boost buff")]
    public float damageBoostDuration = 10f;

    [Tooltip("Heal percentage (0.2 = 20% of lost HP)")]
    [Range(0f, 1f)]
    public float healPercentage = 0.2f;

    [Tooltip("Damage boost percentage (0.2 = 20%)")]
    [Range(0f, 1f)]
    public float damageBoostPercentage = 0.2f;

    [Header("Spawn Chances")]
    [Tooltip("Probability of spawning a Healing Circle (0-1). Remainder is DamageBoost")]
    [Range(0f, 1f)]
    public float healingChance = 0.5f;

    [Header("Prefab References (Optional)")]
    [Tooltip("Healing Circle prefab. Auto-created if left empty.")]
    public GameObject healingCirclePrefab;

    [Tooltip("DamageBoost Circle prefab. Auto-created if left empty.")]
    public GameObject damageBoostCirclePrefab;

    [Header("VFX Prefab (Optional)")]
    [Tooltip("Particle System prefab for Healing Circle (drag and drop VFX here)")]
    public GameObject healingVFXPrefab;

    [Tooltip("Particle System prefab for DamageBoost Circle (drag and drop VFX here)")]
    public GameObject damageBoostVFXPrefab;

    [Header("Sound Effects")]
    [Tooltip("Sound played when a circle spawns")]
    public AudioClip spawnSFX;

    [Range(0f, 1f)]
    public float spawnVolume = 0.5f;

    [Tooltip("Sound played when a buff is activated (assigned to each circle)")]
    public AudioClip activationSFX;

    [Range(0f, 1f)]
    public float activationVolume = 0.8f;

    [Header("Ground Detection")]
    [Tooltip("Ground layer (to spawn circles on the ground surface)")]
    public LayerMask groundLayer = ~0;

    [Tooltip("Raycast height for ground detection")]
    public float raycastHeight = 50f;

    [Header("Debug")]
    public bool showDebugLogs = true;
    public bool showSpawnRange = true;

    private Transform _playerTransform;
    private List<GameObject> _activeCircles = new List<GameObject>();
    private float _spawnTimer;
    private bool _spawningStarted = false;

    private void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("ItemCircleSpawner: Player not found (tag 'Player')!");
            enabled = false;
            return;
        }

        // Start spawning after initial delay
        StartCoroutine(StartSpawningAfterDelay());
    }

    private IEnumerator StartSpawningAfterDelay()
    {
        yield return new WaitForSeconds(initialDelay);
        _spawningStarted = true;
        _spawnTimer = 0f; // Spawn immediately on first cycle

        if (showDebugLogs)
            Debug.Log("ItemCircleSpawner: Started spawning item circles!");
    }

    private void Update()
    {
        if (!_spawningStarted || _playerTransform == null) return;

        // Clean up destroyed circles
        _activeCircles.RemoveAll(c => c == null);

        // Update spawn timer
        _spawnTimer += Time.deltaTime;

        if (_spawnTimer >= spawnInterval && _activeCircles.Count < maxCirclesOnMap)
        {
            SpawnRandomCircle();
            _spawnTimer = 0f;
        }
    }

    private void SpawnRandomCircle()
    {
        // Random position near the player
        Vector3 spawnPos = GetRandomSpawnPosition();
        if (spawnPos == Vector3.zero)
        {
            if (showDebugLogs)
                Debug.LogWarning("ItemCircleSpawner: Could not find a valid spawn position!");
            return;
        }

        // Random circle type
        bool isHealing = Random.value < healingChance;
        ItemCircleEffect.CircleType type = isHealing
            ? ItemCircleEffect.CircleType.Healing
            : ItemCircleEffect.CircleType.DamageBoost;

        GameObject circleObj;

        // Use prefab if available, otherwise auto-create
        GameObject prefab = isHealing ? healingCirclePrefab : damageBoostCirclePrefab;
        if (prefab != null)
        {
            circleObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        }
        else
        {
            circleObj = CreateCircleObject(spawnPos, type);
        }

        // Configure ItemCircleEffect
        ItemCircleEffect effect = circleObj.GetComponent<ItemCircleEffect>();
        if (effect == null)
        {
            effect = circleObj.AddComponent<ItemCircleEffect>();
        }
        effect.circleType = type;
        effect.circleRadius = circleRadius;
        effect.activationTime = activationTime;
        effect.lifetime = circleLifetime;
        effect.fadeOutDuration = fadeOutDuration;
        effect.damageBoostDuration = damageBoostDuration;
        effect.healPercentage = healPercentage;
        effect.damageBoostPercentage = damageBoostPercentage;
        effect.activationSFX = activationSFX;
        effect.activationVolume = activationVolume;

        // Attach VFX if available
        GameObject vfxPrefab = isHealing ? healingVFXPrefab : damageBoostVFXPrefab;
        if (vfxPrefab != null)
        {
            GameObject vfxInstance = Instantiate(vfxPrefab, spawnPos, Quaternion.identity, circleObj.transform);
            vfxInstance.transform.localPosition = Vector3.zero;
        }

        _activeCircles.Add(circleObj);

        // Play spawn SFX
        if (spawnSFX != null)
        {
            AudioSource.PlayClipAtPoint(spawnSFX, spawnPos, spawnVolume);
        }

        if (showDebugLogs)
        {
            string typeName = isHealing ? "Healing" : "Damage Boost";
            Debug.Log($"ItemCircleSpawner: Spawned {typeName} circle at {spawnPos}");
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Try up to 10 times to find a valid position
        for (int i = 0; i < 10; i++)
        {
            // Random direction
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );

            Vector3 candidatePos = _playerTransform.position + offset;

            // Raycast down to find exact ground position
            Vector3 rayStart = candidatePos + Vector3.up * raycastHeight;
            RaycastHit hit;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, raycastHeight * 2f, groundLayer))
            {
                return hit.point + Vector3.up * 0.05f; // Raise slightly above ground
            }

            // If no ground hit, use player's Y position as fallback
            candidatePos.y = _playerTransform.position.y;
            return candidatePos;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Creates a default circle GameObject (no prefab required).
    /// Uses a thin cylinder + semi-transparent material.
    /// </summary>
    private GameObject CreateCircleObject(Vector3 position, ItemCircleEffect.CircleType type)
    {
        string name = type == ItemCircleEffect.CircleType.Healing ? "HealingCircle" : "DamageBoostCircle";
        GameObject circleObj = new GameObject(name);
        circleObj.transform.position = position;

        // Create visual — thin cylinder
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "CircleVisual";
        visual.transform.SetParent(circleObj.transform);
        visual.transform.localPosition = Vector3.zero;

        // Scale cylinder into a flat disc
        float diameter = circleRadius * 2f;
        visual.transform.localScale = new Vector3(diameter, 0.05f, diameter);

        // Disable cylinder collider (using SphereCollider on parent instead)
        Collider visualCol = visual.GetComponent<Collider>();
        if (visualCol != null)
        {
            Destroy(visualCol);
        }

        // Semi-transparent material
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            // Fallback if URP shader is not available
            if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
            {
                mat = new Material(Shader.Find("Standard"));
            }

            Color color = type == ItemCircleEffect.CircleType.Healing
                ? new Color(0.2f, 1f, 0.2f, 0.4f)
                : new Color(1f, 0.3f, 0.1f, 0.4f);

            // Enable transparency
            mat.SetFloat("_Surface", 1); // URP Transparent
            mat.SetFloat("_Mode", 3); // Standard Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.color = color;

            // Emission glow
            mat.EnableKeyword("_EMISSION");
            Color emissionColor = color * 2f;
            emissionColor.a = 1f;
            mat.SetColor("_EmissionColor", emissionColor);

            renderer.material = mat;
        }

        return circleObj;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showSpawnRange) return;

        Transform center = _playerTransform != null ? _playerTransform : transform;

        // Draw spawn range
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        DrawCircle(center.position, minSpawnDistance, 32);

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        DrawCircle(center.position, maxSpawnDistance, 32);
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0) * radius, 0.1f, Mathf.Sin(0) * radius);

        for (int i = 1; i <= segments; i++)
        {
            angle += angleStep;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(rad) * radius, 0.1f, Mathf.Sin(rad) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
