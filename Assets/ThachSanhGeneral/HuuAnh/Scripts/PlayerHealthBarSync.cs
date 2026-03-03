using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Syncs player health bar with PlayerHealth (HuuAnh). Attach to Canvas/object containing the player health bar Slider.
/// If health bar stays full when taking damage: attach this script, assign Slider (or leave empty to auto-find), script auto-finds Player by "Player" tag.
/// </summary>
public class PlayerHealthBarSync : MonoBehaviour
{
    [Tooltip("Health bar Slider - leave empty to auto-find in children")]
    public Slider healthSlider;
    [Tooltip("Text displaying health value (optional)")]
    public Text healthText;
    [Tooltip("Player with PlayerHealth - leave empty to auto-find GameObject with Player tag")]
    public PlayerHealth playerHealth;

    private void Start()
    {
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>(true);
        if (playerHealth == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
                playerHealth = go.GetComponent<PlayerHealth>() ?? go.GetComponentInChildren<PlayerHealth>();
        }
        if (playerHealth == null)
            Debug.LogWarning("PlayerHealthBarSync: PlayerHealth not found. Assign 'Player' tag to the object with PlayerHealth or assign manually in Inspector.");
    }

    private void Update()
    {
        if (playerHealth == null) return;

        float pct = playerHealth.GetHealthPercentage();
        if (healthSlider != null)
            healthSlider.value = pct;
        if (healthText != null)
            healthText.text = $"{Mathf.Ceil(playerHealth.GetCurrentHealth())} / {playerHealth.GetMaxHealth()}";
    }
}
