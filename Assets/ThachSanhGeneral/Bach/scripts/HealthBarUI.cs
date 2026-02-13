using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The entity this health bar tracks (Boss or Player)")]
    public MonoBehaviour healthTarget;
    
    [Tooltip("Health bar slider")]
    public Slider healthSlider;
    
    [Tooltip("Health text (optional)")]
    public Text healthText;
    
    [Tooltip("Name text (optional)")]
    public Text nameText;
    
    [Header("Settings")]
    [Tooltip("Display name")]
    public string displayName = "Player";
    
    [Tooltip("Update every frame or only when changed")]
    public bool alwaysUpdate = true;
    
    [Header("Visual")]
    [Tooltip("Health bar fill color")]
    public Image fillImage;
    
    [Tooltip("Color when health is full")]
    public Color fullHealthColor = Color.green;
    
    [Tooltip("Color when health is low")]
    public Color lowHealthColor = Color.red;
    
    [Tooltip("Threshold for low health color")]
    [Range(0f, 1f)]
    public float lowHealthThreshold = 0.3f;
    
    private float lastHealth = -1f;
    
    void Start()
    {
        if (nameText != null)
        {
            nameText.text = displayName;
        }
        
        if (healthTarget == null)
        {
            Debug.LogError($"HealthBarUI: No health target assigned for {displayName}!");
        }
    }
    
    void Update()
    {
        if (healthTarget == null) return;
        
        float currentHealth = 0f;
        float maxHealth = 0f;
        
        // Check if target is BossController
        if (healthTarget is BossController boss)
        {
            currentHealth = boss.GetHealthPercentage() * 100f; // Approximate
            maxHealth = 100f; // Will calculate from percentage
            
            if (alwaysUpdate || currentHealth != lastHealth)
            {
                UpdateBar(boss.GetHealthPercentage());
                lastHealth = currentHealth;
            }
        }
        // Check if target is PlayerHealth
        else if (healthTarget is PlayerHealth player)
        {
            currentHealth = player.GetCurrentHealth();
            maxHealth = player.GetMaxHealth();
            
            if (alwaysUpdate || currentHealth != lastHealth)
            {
                UpdateBar(player.GetHealthPercentage());
                
                if (healthText != null)
                {
                    healthText.text = $"{Mathf.Ceil(currentHealth)} / {maxHealth}";
                }
                
                lastHealth = currentHealth;
            }
        }
    }
    
    private void UpdateBar(float healthPercentage)
    {
        if (healthSlider != null)
        {
            healthSlider.value = healthPercentage;
        }
        
        // Update color based on health percentage
        if (fillImage != null)
        {
            if (healthPercentage <= lowHealthThreshold)
            {
                fillImage.color = lowHealthColor;
            }
            else
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, 
                    (healthPercentage - lowHealthThreshold) / (1f - lowHealthThreshold));
            }
        }
    }
    
    // Public method to manually update
    public void ForceUpdate()
    {
        lastHealth = -1f;
        Update();
    }
}
