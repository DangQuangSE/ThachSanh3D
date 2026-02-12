using UnityEngine;
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
    
    [Header("Death Settings")]
    [Tooltip("Respawn delay after death")]
    public float respawnDelay = 3f;
    
    private float currentHealth;
    private bool isDead = false;
    private Renderer[] renderers;
    private Color[] originalColors;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    
    void Start()
    {
        currentHealth = maxHealth;
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        
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
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
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
        
        // Disable movement
        var controller = GetComponent<StarterAssets.ThirdPersonController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // Disable character controller
        var charController = GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
        }
        
        // Play death animation if available
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            // Assuming you have a "Death" trigger parameter
            animator.SetTrigger("Death");
        }
        
        // Respawn after delay
        Invoke(nameof(Respawn), respawnDelay);
    }
    
    private void Respawn()
    {
        // Reset health
        currentHealth = maxHealth;
        isDead = false;
        
        // Reset position
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        
        // Re-enable components
        var controller = GetComponent<StarterAssets.ThirdPersonController>();
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        var charController = GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = true;
        }
        
        // Update UI
        UpdateHealthUI();
        
        Debug.Log("Player respawned!");
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
}
