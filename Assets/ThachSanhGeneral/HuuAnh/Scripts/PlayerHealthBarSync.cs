using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Đồng bộ thanh máu player với PlayerHealth (HuuAnh). Gắn vào Canvas/object có Slider thanh máu player.
/// Nếu thanh máu vẫn full khi bị trừ máu: gắn script này, gán Slider (hoặc để trống để tự tìm), script tự tìm Player theo tag "Player".
/// </summary>
public class PlayerHealthBarSync : MonoBehaviour
{
    [Tooltip("Slider thanh máu - để trống sẽ tự tìm trong con")]
    public Slider healthSlider;
    [Tooltip("Text hiển thị số máu (optional)")]
    public Text healthText;
    [Tooltip("Player có PlayerHealth - để trống sẽ tìm GameObject có tag Player")]
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
            Debug.LogWarning("PlayerHealthBarSync: Không tìm thấy PlayerHealth. Gán tag 'Player' cho object có PlayerHealth hoặc gán tay trong Inspector.");
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
