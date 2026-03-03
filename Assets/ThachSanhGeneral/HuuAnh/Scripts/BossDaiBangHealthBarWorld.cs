using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thanh máu boss Đại Bàng (world space). Gắn vào BossHealthBarCanvas (con của finalv5) hoặc gán boss trong Inspector.
/// </summary>
public class BossDaiBangHealthBarWorld : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Boss Đại Bàng - để trống sẽ tự tìm BossDaiBangController ở parent")]
    public BossDaiBangController boss;

    [Tooltip("Health bar slider")]
    public Slider healthSlider;

    [Tooltip("Boss name text")]
    public Text bossNameText;

    [Header("Settings")]
    [Tooltip("Tên hiển thị trên thanh máu (vd: Dai Bang)")]
    public string bossDisplayName = "Dai Bang";

    [Tooltip("Offset above boss")]
    public Vector3 offset = new Vector3(0, 3f, 0);

    [Tooltip("Always face camera")]
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
            boss = GetComponentInParent<BossDaiBangController>();
        if (boss == null)
            boss = FindFirstObjectByType<BossDaiBangController>();

        if (boss == null)
        {
            Debug.LogWarning("BossDaiBangHealthBarWorld: No boss! Gắn script này vào BossHealthBarCanvas (con của finalv5) hoặc gán Boss trong Inspector.");
            enabled = false;
            return;
        }

        // Tự tìm Slider / Fill trong con nếu chưa gán (chỉ trong HuuAnh)
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>(true);
        if (healthSlider == null)
        {
            Debug.LogWarning("BossDaiBangHealthBarWorld: Không tìm thấy Slider. Gán Health Slider trong Inspector hoặc thêm Slider vào con của Canvas.");
            enabled = false;
            return;
        }
        if (fillImage == null && healthSlider.fillRect != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        if (bossNameText == null)
            bossNameText = GetComponentInChildren<Text>(true);

        if (bossNameText != null)
            bossNameText.text = string.IsNullOrEmpty(bossDisplayName) ? "Boss" : bossDisplayName;

        // Cảnh báo nếu Canvas không phải World Space → thanh sẽ không nổi trên đầu boss
        var canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
            Debug.LogWarning("BossDaiBangHealthBarWorld: Đặt Canvas Render Mode = World Space để thanh máu hiện trên đầu boss (Inspector → Canvas → Render Mode).");

        // Màu thanh máu: đỏ (ít máu) -> vàng -> xanh (đầy máu). Ghi đè nếu gradient trắng/chưa chỉnh.
        bool useDefaultGradient = healthColorGradient == null;
        if (!useDefaultGradient)
        {
            Color mid = healthColorGradient.Evaluate(0.5f);
            if (mid.r > 0.9f && mid.g > 0.9f && mid.b > 0.9f)
                useDefaultGradient = true;
        }
        if (useDefaultGradient)
        {
            healthColorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(Color.red, 0f);
            colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f);
            colorKeys[2] = new GradientColorKey(Color.green, 1f);
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);
            healthColorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    void Update()
    {
        if (boss == null || boss.IsDead())
        {
            gameObject.SetActive(false);
            return;
        }

        // Update position
        transform.position = boss.transform.position + offset;

        // Billboard to camera
        if (billboardToCamera && mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }

        // Update health bar
        float healthPercent = boss.GetHealthPercentage();

        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }

        if (fillImage != null)
        {
            fillImage.color = healthColorGradient.Evaluate(healthPercent);
        }
    }
}
