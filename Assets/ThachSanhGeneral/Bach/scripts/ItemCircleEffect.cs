using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Item effect circle — when the player stands inside the circle long enough, a buff is activated.
/// Two types: Healing (restores 20% of lost HP) and DamageBoost (increases 20% damage).
/// Shows a progress bar while the player stands inside the circle.
/// Circle auto-expires after a configurable lifetime, gradually fading out before disappearing.
/// </summary>
public class ItemCircleEffect : MonoBehaviour
{
    public enum CircleType
    {
        Healing,      // Restore 20% of lost HP
        DamageBoost   // Increase 20% damage
    }

    [Header("Circle Settings")]
    [Tooltip("Circle type: Healing or DamageBoost")]
    public CircleType circleType = CircleType.Healing;

    [Tooltip("Time required to stand inside the circle to activate (seconds)")]
    public float activationTime = 5f;

    [Tooltip("Circle radius (used for player detection)")]
    public float circleRadius = 3f;

    [Tooltip("Duration of the damage boost buff (seconds)")]
    public float damageBoostDuration = 10f;

    [Tooltip("Heal percentage (0.2 = 20% of lost HP)")]
    [Range(0f, 1f)]
    public float healPercentage = 0.2f;

    [Tooltip("Damage boost percentage (0.2 = 20%)")]
    [Range(0f, 1f)]
    public float damageBoostPercentage = 0.2f;

    [Header("Lifetime Settings")]
    [Tooltip("How long the circle exists before auto-expiring (seconds). 0 = never expires.")]
    public float lifetime = 20f;

    [Tooltip("Duration of the fade-out effect before the circle disappears (seconds)")]
    public float fadeOutDuration = 3f;

    [Header("Progress Bar UI")]
    [Tooltip("Canvas containing the progress bar (auto-created if left empty)")]
    public Canvas progressCanvas;

    [Tooltip("Slider displaying the activation timer")]
    public Slider progressBar;

    [Tooltip("Height of the progress bar above the circle")]
    public float progressBarHeight = 2.5f;

    [Header("Visual Settings")]
    [Tooltip("Healing circle color")]
    public Color healingColor = new Color(0.2f, 1f, 0.2f, 0.5f);

    [Tooltip("DamageBoost circle color")]
    public Color damageBoostColor = new Color(1f, 0.3f, 0.1f, 0.5f);

    [Header("Sound Effects")]
    [Tooltip("Sound played when the buff is activated")]
    public AudioClip activationSFX;

    [Range(0f, 1f)]
    public float activationVolume = 0.8f;

    [Header("Debug")]
    public bool showDebugLogs = true;
    public bool showGizmos = true;

    // Internal state
    private float _currentTime = 0f;
    private bool _playerInside = false;
    private bool _activated = false;
    private Transform _playerTransform;
    private PlayerHealth _playerHealth;
    private PlayerAttack _playerAttack;
    private Camera _mainCamera;

    // Lifetime and fade state
    private float _aliveTime = 0f;
    private bool _isFading = false;
    private bool _isExpired = false;
    private List<Renderer> _allRenderers = new List<Renderer>();
    private List<Color> _originalColors = new List<Color>();
    private List<ParticleSystem> _allParticles = new List<ParticleSystem>();
    private List<float> _originalParticleAlphas = new List<float>();

    // Auto-created UI references
    private GameObject _progressBarObj;

    private void Start()
    {
        _mainCamera = Camera.main;

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerHealth = player.GetComponent<PlayerHealth>();
            _playerAttack = player.GetComponent<PlayerAttack>();
        }
        else
        {
            Debug.LogError("ItemCircleEffect: Player not found (tag 'Player')!");
        }

        // Auto-create progress bar UI if not assigned
        if (progressBar == null)
        {
            CreateProgressBarUI();
        }

        // Hide progress bar initially
        SetProgressBarVisible(false);

        // Set collider
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
        }
        col.isTrigger = true;
        col.radius = circleRadius;

        // Cache all renderers and their original colors for fade-out
        CacheRenderersAndParticles();
    }

    private void Update()
    {
        if (_isExpired) return;

        // Update lifetime timer
        if (lifetime > 0f && !_activated)
        {
            _aliveTime += Time.deltaTime;

            float fadeStartTime = lifetime - fadeOutDuration;

            // Start fading when approaching expiration
            if (_aliveTime >= fadeStartTime && !_isFading)
            {
                _isFading = true;
                if (showDebugLogs)
                    Debug.Log($"Circle fading out... {fadeOutDuration}s until expiration.");
            }

            // Apply fade effect
            if (_isFading)
            {
                float fadeElapsed = _aliveTime - fadeStartTime;
                float fadeProgress = Mathf.Clamp01(fadeElapsed / fadeOutDuration);
                float alpha = 1f - fadeProgress;
                ApplyFade(alpha);
            }

            // Expired — destroy regardless of player position
            if (_aliveTime >= lifetime)
            {
                ExpireCircle();
                return;
            }
        }

        if (_activated) return;

        // Check if player is standing inside the circle
        if (_playerTransform != null)
        {
            float dist = Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(_playerTransform.position.x, 0f, _playerTransform.position.z));

            bool isInside = dist <= circleRadius;

            if (isInside && !_playerInside)
            {
                OnPlayerEnter();
            }
            else if (!isInside && _playerInside)
            {
                OnPlayerExit();
            }
        }

        // Update timer if player is standing inside the circle
        if (_playerInside)
        {
            _currentTime += Time.deltaTime;

            // Update progress bar
            if (progressBar != null)
            {
                progressBar.value = _currentTime / activationTime;
            }

            // Check if activation time has been reached
            if (_currentTime >= activationTime)
            {
                ActivateEffect();
            }
        }

        // Keep progress bar facing the camera
        if (progressCanvas != null && _mainCamera != null)
        {
            progressCanvas.transform.LookAt(
                progressCanvas.transform.position + _mainCamera.transform.forward);
        }
    }

    /// <summary>
    /// Caches all Renderers and ParticleSystems in children for the fade-out effect.
    /// Called once in Start after all visuals are set up.
    /// </summary>
    private void CacheRenderersAndParticles()
    {
        // Cache renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            if (r.material.HasProperty("_Color"))
            {
                _allRenderers.Add(r);
                _originalColors.Add(r.material.color);
            }
        }

        // Cache particle systems
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particles)
        {
            _allParticles.Add(ps);
            var main = ps.main;
            _originalParticleAlphas.Add(main.startColor.color.a);
        }
    }

    /// <summary>
    /// Applies a fade multiplier (0 = fully transparent, 1 = original) to all renderers and particles.
    /// </summary>
    private void ApplyFade(float alpha)
    {
        // Fade renderers
        for (int i = 0; i < _allRenderers.Count; i++)
        {
            if (_allRenderers[i] == null) continue;
            Color c = _originalColors[i];
            c.a *= alpha;
            _allRenderers[i].material.color = c;

            // Fade emission too if present
            if (_allRenderers[i].material.HasProperty("_EmissionColor"))
            {
                Color emission = _allRenderers[i].material.GetColor("_EmissionColor");
                emission.a *= alpha;
                // Dim the emission intensity as well
                emission *= alpha;
                _allRenderers[i].material.SetColor("_EmissionColor", emission);
            }
        }

        // Fade particle systems
        for (int i = 0; i < _allParticles.Count; i++)
        {
            if (_allParticles[i] == null) continue;
            var main = _allParticles[i].main;
            Color startColor = main.startColor.color;
            startColor.a = _originalParticleAlphas[i] * alpha;
            main.startColor = startColor;
        }

        // Fade progress bar UI if visible
        if (progressCanvas != null && progressCanvas.gameObject.activeSelf)
        {
            CanvasGroup cg = progressCanvas.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = progressCanvas.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = alpha;
        }
    }

    /// <summary>
    /// Circle has expired without being activated — fade complete, destroy.
    /// </summary>
    private void ExpireCircle()
    {
        _isExpired = true;
        _playerInside = false;
        SetProgressBarVisible(false);

        if (showDebugLogs)
            Debug.Log($"{(circleType == CircleType.Healing ? "Healing" : "Damage Boost")} circle expired (lifetime {lifetime}s reached).");

        Destroy(gameObject);
    }

    private void OnPlayerEnter()
    {
        if (_activated || _isExpired) return;
        _playerInside = true;
        SetProgressBarVisible(true);

        if (showDebugLogs)
        {
            string typeName = circleType == CircleType.Healing ? "Healing" : "Damage Boost";
            Debug.Log($"Player entered {typeName} circle! Stand for {activationTime}s to activate.");
        }
    }

    private void OnPlayerExit()
    {
        if (_activated || _isExpired) return;
        _playerInside = false;
        _currentTime = 0f; // Reset timer when leaving the circle

        // Reset progress bar
        if (progressBar != null)
        {
            progressBar.value = 0f;
        }
        SetProgressBarVisible(false);

        if (showDebugLogs)
        {
            Debug.Log("Player left the circle. Timer reset.");
        }
    }

    private void ActivateEffect()
    {
        _activated = true;

        // Play SFX
        if (activationSFX != null)
        {
            AudioSource.PlayClipAtPoint(activationSFX, transform.position, activationVolume);
        }

        switch (circleType)
        {
            case CircleType.Healing:
                ApplyHealing();
                break;
            case CircleType.DamageBoost:
                ApplyDamageBoost();
                break;
        }

        // Hide progress bar
        SetProgressBarVisible(false);

        if (showDebugLogs)
        {
            string typeName = circleType == CircleType.Healing ? "Healing" : "Damage Boost";
            Debug.Log($"{typeName} circle activated!");
        }

        // Always destroy circle after buff is applied
        Destroy(gameObject);
    }

    private void ApplyHealing()
    {
        if (_playerHealth == null)
        {
            Debug.LogWarning("ItemCircleEffect: PlayerHealth not found!");
            return;
        }

        // Restore 20% of lost HP
        float maxHP = _playerHealth.GetMaxHealth();
        float currentHP = _playerHealth.GetCurrentHealth();
        float lostHP = maxHP - currentHP;
        float healAmount = lostHP * healPercentage;

        if (healAmount > 0)
        {
            _playerHealth.Heal(healAmount);
            if (showDebugLogs)
            {
                Debug.Log($"Healed {healAmount:F1} HP ({healPercentage * 100}% of {lostHP:F1} lost HP)");
            }
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("Player is at full health, no healing needed.");
        }
    }

    private void ApplyDamageBoost()
    {
        if (_playerAttack == null)
        {
            Debug.LogWarning("ItemCircleEffect: PlayerAttack not found!");
            return;
        }

        // Increase 20% damage
        float bonusDamage = _playerAttack.attackDamage * damageBoostPercentage;
        float bonusESkillDamage = _playerAttack.eskillDamage * damageBoostPercentage;

        _playerAttack.attackDamage += bonusDamage;
        _playerAttack.eskillDamage += bonusESkillDamage;

        if (showDebugLogs)
        {
            Debug.Log($"Damage boosted! Attack: +{bonusDamage:F1}, ESkill: +{bonusESkillDamage:F1} for {damageBoostDuration}s");
        }

        // Run the removal coroutine on the Player so it survives circle destruction
        MonoBehaviour playerMono = _playerAttack;
        playerMono.StartCoroutine(RemoveDamageBoostAfterDuration(bonusDamage, bonusESkillDamage));
    }

    private IEnumerator RemoveDamageBoostAfterDuration(float bonusDamage, float bonusESkillDamage)
    {
        yield return new WaitForSeconds(damageBoostDuration);

        if (_playerAttack != null)
        {
            _playerAttack.attackDamage -= bonusDamage;
            _playerAttack.eskillDamage -= bonusESkillDamage;

            // Ensure damage does not go negative
            _playerAttack.attackDamage = Mathf.Max(_playerAttack.attackDamage, 0f);
            _playerAttack.eskillDamage = Mathf.Max(_playerAttack.eskillDamage, 0f);

            if (showDebugLogs)
            {
                Debug.Log($"Damage boost expired! Attack: {_playerAttack.attackDamage:F1}, ESkill: {_playerAttack.eskillDamage:F1}");
            }
        }
    }

    private void SetProgressBarVisible(bool visible)
    {
        if (progressCanvas != null)
        {
            progressCanvas.gameObject.SetActive(visible);
        }
        else if (_progressBarObj != null)
        {
            _progressBarObj.SetActive(visible);
        }
    }

    /// <summary>
    /// Auto-creates a World Space Canvas + Slider as a progress bar if none is assigned.
    /// </summary>
    private void CreateProgressBarUI()
    {
        // Canvas
        _progressBarObj = new GameObject("ProgressBarCanvas");
        _progressBarObj.transform.SetParent(transform);
        _progressBarObj.transform.localPosition = new Vector3(0f, progressBarHeight, 0f);
        _progressBarObj.transform.localScale = Vector3.one * 0.01f;

        progressCanvas = _progressBarObj.AddComponent<Canvas>();
        progressCanvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = _progressBarObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(_progressBarObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(200f, 25f);
        bgRect.anchoredPosition = Vector2.zero;

        // Slider
        GameObject sliderObj = new GameObject("ProgressSlider");
        sliderObj.transform.SetParent(_progressBarObj.transform, false);
        progressBar = sliderObj.AddComponent<Slider>();
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(200f, 25f);
        sliderRect.anchoredPosition = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0f);
        fillAreaRect.anchorMax = new Vector2(1f, 1f);
        fillAreaRect.offsetMin = new Vector2(5f, 5f);
        fillAreaRect.offsetMax = new Vector2(-5f, -5f);

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = circleType == CircleType.Healing ? healingColor : damageBoostColor;
        // Make fill color fully opaque
        Color fillColor = fillImage.color;
        fillColor.a = 1f;
        fillImage.color = fillColor;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        progressBar.fillRect = fillRect;
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;

        // Label text
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(_progressBarObj.transform, false);
        Text label = labelObj.AddComponent<Text>();
        label.text = circleType == CircleType.Healing ? "Healing" : "Power Up";
        label.fontSize = 18;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(200f, 30f);
        labelRect.anchoredPosition = new Vector2(0f, 25f);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = circleType == CircleType.Healing ? healingColor : damageBoostColor;
        // Draw circle on the ground
        DrawCircleGizmo(transform.position, circleRadius, 32);
    }

    private void DrawCircleGizmo(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0) * radius, 0.05f, Mathf.Sin(0) * radius);

        for (int i = 1; i <= segments; i++)
        {
            angle += angleStep;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(rad) * radius, 0.05f, Mathf.Sin(rad) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
