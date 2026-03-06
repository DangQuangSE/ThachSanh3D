using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of player")]
    public float maxHealth = 100f;
    
    [Header("UI References")]
    [Tooltip("Health bar slider UI")]
    public Slider healthBar;
    
    [Tooltip("Health text display (optional)")]
    public Text healthText;
    
    [Header("Visual Feedback")]
    [Tooltip("Color when taking damage")]
    public Color damageColor = Color.red;
    
    [Tooltip("Duration of damage flash effect")]
    public float damageFlashDuration = 0.1f;

    [Header("Sound Effects")]
    [Tooltip("Sound when player takes damage (leave empty to skip)")]
    public AudioClip hurtSFX;

    [Range(0f, 1f)]
    [Tooltip("Volume for hurt sound effect")]
    public float hurtVolume = 0.8f;

    [Tooltip("Minimum time between hurt sounds to prevent spam")]
    public float hurtSFXCooldown = 0.5f;
    
    [Header("Death Settings")]
    [Tooltip("Respawn delay after death")]
    public float respawnDelay = 3f;
    
    private float currentHealth;
    private bool isDead = false;
    private bool isInvincible = false;
    private Renderer[] renderers;
    private Color[] originalColors;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private Animator _animator;
    private float _lastHurtSFXTime;
    
    // Must match the Bool parameter name in Animator exactly
    private static readonly int AnimIDDead = Animator.StringToHash("Dead");
    
    void Start()
    {
        currentHealth = maxHealth;
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        _animator = GetComponent<Animator>();
        
        // Get all renderers for damage flash effect
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
        
        UpdateHealthUI();
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        if (isInvincible) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Play hurt SFX with cooldown
        if (hurtSFX != null && Time.time - _lastHurtSFXTime >= hurtSFXCooldown)
        {
            AudioSource.PlayClipAtPoint(hurtSFX, transform.position, hurtVolume);
            _lastHurtSFXTime = Time.time;
        }
        
        // Visual feedback
        StartCoroutine(DamageFlash());
        
        // Update UI
        UpdateHealthUI();
        
        // Check death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        Debug.Log($"Player healed {amount}. Health: {currentHealth}/{maxHealth}");
        
        UpdateHealthUI();
    }
    
    private System.Collections.IEnumerator DamageFlash()
    {
        // Flash red
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material.HasProperty("_Color"))
            {
                renderer.material.color = damageColor;
            }
        }
        
        yield return new WaitForSeconds(damageFlashDuration);
        
        // Return to original color
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("Player died!");
        
        // Stop damage flash coroutine if running
        StopAllCoroutines();
        
        // 1. Disable player input/movement first
        var controller = GetComponent<StarterAssets.ThirdPersonController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // 2. Play death animation — use Play() to force immediate state change
        //    This bypasses any transition conflicts from lingering attack triggers
        if (_animator != null)
        {
            _animator.ResetTrigger("Attack1");
            _animator.ResetTrigger("Attack2");
            _animator.ResetTrigger("Attack3");
            _animator.ResetTrigger("Ultimate");
            _animator.ResetTrigger("Protect");
            _animator.ResetTrigger("ESkill");
            _animator.ResetTrigger("Roll");
            
            _animator.SetBool(AnimIDDead, true);
            _animator.Play("Dead", 0, 0f);
        }
        
        // 3. Disable CharacterController last (after animation is set)
        var charController = GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
        }
        
        // Restart game after delay
        Invoke(nameof(RestartGame), respawnDelay);
    }
    
    private void RestartGame()
    {
        Debug.Log("Restarting game...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)} / {maxHealth}";
        }
    }
    
    // Public getters
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }
    
    public bool IsInvincible()
    {
        return isInvincible;
    }
}
