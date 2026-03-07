using UnityEngine;

/// <summary>
/// Script ??n gi?n ?? ?i?u khi?n ?? sáng/t?i c?a scene
/// Dùng ?? làm t?i tr?i nhanh chóng
/// </summary>
public class SimpleDarknessController : MonoBehaviour
{
    [Header("Light Reference")]
    [Tooltip("Directional Light chính (s? t? ??ng tìm n?u ?? tr?ng)")]
    public Light mainLight;

    [Header("Darkness Settings")]
    [Tooltip("?? sáng ban ?êm (0 = t?i hoàn toàn, 1 = sáng bình th??ng)")]
    [Range(0f, 1f)]
    public float darknessLevel = 0.1f;

    [Tooltip("Màu ánh sáng khi t?i")]
    public Color darkLightColor = new Color(0.2f, 0.2f, 0.3f);

    [Tooltip("Màu tr?i khi t?i")]
    public Color darkSkyColor = new Color(0.05f, 0.05f, 0.1f);

    [Tooltip("Màu fog khi t?i")]
    public Color darkFogColor = new Color(0.1f, 0.1f, 0.15f);

    [Tooltip("?? ??m fog khi t?i")]
    [Range(0f, 0.1f)]
    public float darkFogDensity = 0.02f;

    [Header("Original Settings (Auto Saved)")]
    private float _originalIntensity;
    private Color _originalLightColor;
    private Color _originalSkyColor;
    private Color _originalFogColor;
    private float _originalFogDensity;
    private bool _settingsSaved = false;

    private void Start()
    {
        // T? ??ng tìm Directional Light
        if (mainLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    mainLight = light;
                    break;
                }
            }
        }

        SaveOriginalSettings();
    }

    /// <summary>
    /// L?u cài ??t g?c ?? có th? khôi ph?c
    /// </summary>
    private void SaveOriginalSettings()
    {
        if (_settingsSaved || mainLight == null) return;

        _originalIntensity = mainLight.intensity;
        _originalLightColor = mainLight.color;
        _originalSkyColor = RenderSettings.ambientLight;
        _originalFogColor = RenderSettings.fogColor;
        _originalFogDensity = RenderSettings.fogDensity;
        _settingsSaved = true;

        Debug.Log("[SimpleDarknessController] ?ã l?u cài ??t g?c");
    }

    /// <summary>
    /// Làm t?i tr?i v?i m?c ?? ch? ??nh
    /// </summary>
    /// <param name="level">0 = t?i hoàn toàn, 1 = sáng bình th??ng</param>
    public void SetDarkness(float level)
    {
        if (mainLight == null)
        {
            Debug.LogWarning("[SimpleDarknessController] Không tìm th?y Main Light!");
            return;
        }

        darknessLevel = Mathf.Clamp01(level);

        // ?i?u ch?nh c??ng ?? ánh sáng
        mainLight.intensity = Mathf.Lerp(0f, _originalIntensity, darknessLevel);

        // ?i?u ch?nh màu ánh sáng
        mainLight.color = Color.Lerp(darkLightColor, _originalLightColor, darknessLevel);

        // ?i?u ch?nh màu tr?i
        RenderSettings.ambientLight = Color.Lerp(darkSkyColor, _originalSkyColor, darknessLevel);
        RenderSettings.ambientSkyColor = Color.Lerp(darkSkyColor, _originalSkyColor, darknessLevel);

        // ?i?u ch?nh fog
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.Lerp(darkFogColor, _originalFogColor, darknessLevel);
        RenderSettings.fogDensity = Mathf.Lerp(darkFogDensity, _originalFogDensity, darknessLevel);
    }

    /// <summary>
    /// Làm t?i hoàn toàn (g?n nh? không th?y gì)
    /// </summary>
    public void MakePitchDark()
    {
        SetDarkness(0f);
        Debug.Log("[SimpleDarknessController] ?ã làm t?i hoàn toàn");
    }

    /// <summary>
    /// Làm t?i v?a ph?i (nh? ban ?êm)
    /// </summary>
    public void MakeNight()
    {
        SetDarkness(0.1f);
        Debug.Log("[SimpleDarknessController] ?ã chuy?n sang ban ?êm");
    }

    /// <summary>
    /// Làm sáng l?i bình th??ng
    /// </summary>
    public void MakeBright()
    {
        SetDarkness(1f);
        Debug.Log("[SimpleDarknessController] ?ã chuy?n sang ban ngày");
    }

    /// <summary>
    /// Khôi ph?c cài ??t g?c
    /// </summary>
    public void RestoreOriginalSettings()
    {
        if (!_settingsSaved || mainLight == null)
        {
            Debug.LogWarning("[SimpleDarknessController] Không có cài ??t g?c ?? khôi ph?c!");
            return;
        }

        mainLight.intensity = _originalIntensity;
        mainLight.color = _originalLightColor;
        RenderSettings.ambientLight = _originalSkyColor;
        RenderSettings.ambientSkyColor = _originalSkyColor;
        RenderSettings.fogColor = _originalFogColor;
        RenderSettings.fogDensity = _originalFogDensity;

        darknessLevel = 1f;
        Debug.Log("[SimpleDarknessController] ?ã khôi ph?c cài ??t g?c");
    }

    /// <summary>
    /// B?t/t?t ánh sáng chính
    /// </summary>
    public void ToggleLight(bool enable)
    {
        if (mainLight != null)
            mainLight.enabled = enable;
    }

    // ?? test trong Inspector
    private void OnValidate()
    {
        if (Application.isPlaying && _settingsSaved)
        {
            SetDarkness(darknessLevel);
        }
    }
}
