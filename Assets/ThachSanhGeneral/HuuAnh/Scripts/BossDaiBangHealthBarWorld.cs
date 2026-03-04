using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Eagle Boss health bar (world space). Attach to BossHealthBarCanvas (child of finalv5) or assign boss in Inspector.
/// </summary>
public class BossDaiBangHealthBarWorld : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Boss Dai Bang - leave empty to auto-find BossDaiBangController in parent")]
    public BossDaiBangController boss;

    [Tooltip("Health bar slider")]
    public Slider healthSlider;

    [Tooltip("Boss name text")]
    public Text bossNameText;

    [Header("Settings")]
    [Tooltip("Display name on health bar (e.g. Eagle Boss)")]
    public string bossDisplayName = "Eagle Boss";

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
            Debug.LogWarning("BossDaiBangHealthBarWorld: No boss found! Attach this script to BossHealthBarCanvas (child of finalv5) or assign Boss in Inspector.");
            enabled = false;
            return;
        }

        // Auto-find Slider / Fill in children if not assigned
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>(true);
        if (healthSlider == null)
        {
            Debug.LogWarning("BossDaiBangHealthBarWorld: Slider not found. Assign Health Slider in Inspector or add a Slider as child of Canvas.");
            enabled = false;
            return;
        }
        if (fillImage == null && healthSlider.fillRect != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        if (bossNameText == null)
            bossNameText = GetComponentInChildren<Text>(true);

        if (bossNameText != null)
            bossNameText.text = string.IsNullOrEmpty(bossDisplayName) ? "Boss" : bossDisplayName;

        // Warn if Canvas is not World Space - bar will not float above boss head
        var canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
            Debug.LogWarning("BossDaiBangHealthBarWorld: Set Canvas Render Mode = World Space for the health bar to appear above boss head (Inspector > Canvas > Render Mode).");

        // Health bar color: red (low HP) -> yellow -> green (full HP). Override if gradient is white/not configured.
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
