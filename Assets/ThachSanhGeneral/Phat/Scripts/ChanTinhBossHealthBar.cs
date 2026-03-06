using UnityEngine;
using UnityEngine.UI;

public class ChanTinhBossHealthBar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The boss this health bar tracks")]
    public ChanTinhBossController boss;

    [Tooltip("Health bar slider")]
    public Slider healthSlider;

    [Tooltip("Boss name text")]
    public Text bossNameText;

    [Header("Settings")]
    [Tooltip("If true, health bar will follow boss in world space. Set false for UI overlay.")]
    public bool isWorldSpace = false;

    [Tooltip("Offset above boss (only used if isWorldSpace is true)")]
    public Vector3 offset = new Vector3(0, 3f, 0);

    [Tooltip("Always face camera (only used if isWorldSpace is true)")]
    public bool billboardToCamera = true;

    [Header("Visual")]
    [Tooltip("Fill image of health bar")]
    public Image fillImage;

    [Tooltip("Color gradient")]
    public Gradient healthColorGradient;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (boss == null)
        {
            boss = GetComponentInParent<ChanTinhBossController>();
        }

        // Tự động tìm kiếm các tham chiếu UI nếu bạn quên kéo thả vào Inspector
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }

        if (bossNameText == null)
        {
            Transform nameTransform = transform.Find("Boss Name");
            if (nameTransform != null) bossNameText = nameTransform.GetComponent<Text>();
        }

        if (fillImage == null)
        {
            Transform fillTransform = transform.Find("Fill Area/Fill");
            if (fillTransform != null) fillImage = fillTransform.GetComponent<Image>();
        }

        if (boss == null)
        {
            Debug.LogWarning("BossHealthBarWorld: No boss assigned! Try to find one...");
            enabled = false;
            return;
        }

        if (bossNameText != null && string.IsNullOrEmpty(bossNameText.text))
        {
            bossNameText.text = "Chằn Tinh";
        }

        // Bỏ qua phần ghi đè healthColorGradient nếu inspector đang rỗng
        // Vì hiện tại ta dùng fillImage.color tĩnh từ Editor thay vì dải gradient
        // Nêu cần, hãy thiết lập gradient trực tiếp từ Inspector.
    }

    void Update()
    {
        if (boss == null || boss.IsDead())
        {
            gameObject.SetActive(false);
            return;
        }

        if (isWorldSpace)
        {
            // Update position
            transform.position = boss.transform.position + offset;

            // Billboard to camera
            if (billboardToCamera && mainCamera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            }
        }

        // Update health bar
        float healthPercent = boss.GetHealthPercentage();

        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }

        // Chỉ đổi màu theo Gradient nếu Gradient đã được thiết lập các keys
        if (fillImage != null && healthColorGradient != null && healthColorGradient.colorKeys.Length > 0)
        {
            fillImage.color = healthColorGradient.Evaluate(healthPercent);
        }
    }
}
