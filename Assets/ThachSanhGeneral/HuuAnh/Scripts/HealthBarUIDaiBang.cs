using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Health bar UI (HuuAnh) - supports BossDaiBangController or PlayerHealth. Attach to Canvas, assign healthTarget = Boss or Player.
/// </summary>
public class HealthBarUIDaiBang : MonoBehaviour
{
    [Header("References")]
    [Tooltip("BossDaiBangController or PlayerHealth (drag from Hierarchy)")]
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

        // Check if target is BossDaiBangController (HuuAnh)
        if (healthTarget is BossDaiBangController bossDaiBang)
        {
            currentHealth = bossDaiBang.GetHealthPercentage() * 100f;
            maxHealth = 100f;
            if (alwaysUpdate || currentHealth != lastHealth)
            {
                UpdateBar(bossDaiBang.GetHealthPercentage());
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
