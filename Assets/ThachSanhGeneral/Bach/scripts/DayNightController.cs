using UnityEngine;
using System.Collections;

/// <summary>
/// Controller ?i?u khi?n chu k? ngày/?êm và th?i ti?t
/// Có th? ch?nh ?? sáng, màu tr?i, góc m?t tr?i/m?t tr?ng
/// H? tr? chuy?n ??i m??t mà gi?a các th?i ?i?m
/// </summary>
public class DayNightController : MonoBehaviour
{
    [Header("Lighting References")]
    [Tooltip("Directional Light chính (M?t tr?i)")]
    public Light sunLight;

    [Tooltip("Directional Light ph? (M?t tr?ng - tùy ch?n)")]
    public Light moonLight;

    [Header("Time of Day")]
    [Tooltip("Th?i gian hi?n t?i trong ngày (0-24 gi?)")]
    [Range(0f, 24f)]
    public float currentTime = 12f;

    [Tooltip("T?c ?? trôi th?i gian (1 = th?c t?, 10 = nhanh g?p 10)")]
    public float timeSpeed = 1f;

    [Tooltip("T? ??ng c?p nh?t th?i gian")]
    public bool autoUpdateTime = false;

    [Header("Day Settings")]
    [Tooltip("Màu ánh sáng ban ngày")]
    public Color dayLightColor = new Color(1f, 1f, 1f); // Tr?ng thu?n

    [Tooltip("C??ng ?? ánh sáng ban ngày")]
    [Range(0f, 3f)]
    public float dayIntensity = 1.0f;

    [Tooltip("Màu ambient ban ngày (gi? màu trung tính)")]
    public Color dayAmbientColor = new Color(0.8f, 0.8f, 0.8f); // Xám nh?t

    [Header("Night Settings")]
    [Tooltip("Màu ánh sáng ban ?êm")]
    public Color nightLightColor = new Color(0.4f, 0.5f, 0.7f); // Xanh nh?t

    [Tooltip("C??ng ?? ánh sáng ban ?êm")]
    [Range(0f, 1f)]
    public float nightIntensity = 0.2f;

    [Tooltip("Màu ambient ban ?êm")]
    public Color nightAmbientColor = new Color(0.2f, 0.2f, 0.3f); // Xanh t?i

    [Header("Sunset/Sunrise Settings")]
    [Tooltip("Màu ánh sáng hoàng hôn/bình minh")]
    public Color sunsetLightColor = new Color(1f, 0.8f, 0.6f); // Vàng nh?t h?n

    [Tooltip("C??ng ?? ánh sáng hoàng hôn/bình minh")]
    [Range(0f, 2f)]
    public float sunsetIntensity = 0.8f;

    [Tooltip("Màu ambient hoàng hôn/bình minh")]
    public Color sunsetAmbientColor = new Color(0.9f, 0.75f, 0.6f); // Vàng cam nh?t

    [Header("Time Ranges")]
    [Tooltip("Gi? b?t ??u bình minh (4-7)")]
    [Range(0f, 12f)]
    public float sunriseStart = 5f;

    [Tooltip("Gi? k?t thúc bình minh")]
    [Range(0f, 12f)]
    public float sunriseEnd = 7f;

    [Tooltip("Gi? b?t ??u hoàng hôn (17-19)")]
    [Range(12f, 24f)]
    public float sunsetStart = 18f;

    [Tooltip("Gi? k?t thúc hoàng hôn")]
    [Range(12f, 24f)]
    public float sunsetEnd = 20f;

    [Header("Fog Settings")]
    [Tooltip("B?t/t?t fog")]
    public bool useFog = true;

    [Tooltip("Màu fog ban ngày")]
    public Color dayFogColor = new Color(0.8f, 0.85f, 0.9f);

    [Tooltip("Màu fog ban ?êm")]
    public Color nightFogColor = new Color(0.1f, 0.1f, 0.2f);

    [Tooltip("?? ??m fog ban ngày")]
    [Range(0f, 0.1f)]
    public float dayFogDensity = 0.005f;

    [Tooltip("?? ??m fog ban ?êm")]
    [Range(0f, 0.1f)]
    public float nightFogDensity = 0.02f;

    [Header("Transition Settings")]
    [Tooltip("Th?i gian chuy?n ??i m??t (giây)")]
    public float transitionDuration = 2f;

    [Tooltip("Cho phép chuy?n ??i m??t mà")]
    public bool smoothTransition = true;

    [Header("Advanced Settings")]
    [Tooltip("Ch? thay ??i Directional Light (không ??i Ambient)")]
    public bool onlyChangeLightNotAmbient = false;

    [Tooltip("C??ng ?? Ambient (0 = không ?nh h??ng màu v?t th?)")]
    [Range(0f, 1f)]
    public float ambientIntensity = 0.5f;

    // Private variables
    private float _targetTime = -1f;
    private Coroutine _transitionCoroutine;
    
    // L?u giá tr? ban ??u
    private Color _originalAmbient;
    private bool _settingsSaved = false;

    private void Start()
    {
        // T? ??ng tìm Directional Light n?u ch?a gán
        if (sunLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    sunLight = light;
                    Debug.Log("[DayNightController] ?ã tìm th?y Directional Light: " + light.name);
                    break;
                }
            }
        }

        // L?u ambient g?c
        if (!_settingsSaved)
        {
            _originalAmbient = RenderSettings.ambientLight;
            _settingsSaved = true;
        }

        // Apply settings ngay l?p t?c
        UpdateLighting();
    }

    private void Update()
    {
        if (autoUpdateTime)
        {
            // T? ??ng c?p nh?t th?i gian
            currentTime += Time.deltaTime * timeSpeed / 3600f * 24f;
            if (currentTime >= 24f)
                currentTime -= 24f;

            UpdateLighting();
        }
    }

    // ?????????????????????????????????????????????????????????????????????
    // Public Methods - ?i?u khi?n th?i gian
    // ?????????????????????????????????????????????????????????????????????

    public void SetTime(float hour)
    {
        currentTime = Mathf.Clamp(hour, 0f, 24f);
        UpdateLighting();
    }

    public void SetDay()
    {
        if (smoothTransition)
            TransitionToTime(12f);
        else
            SetTime(12f);
    }

    public void SetNight()
    {
        if (smoothTransition)
            TransitionToTime(0f);
        else
            SetTime(0f);
    }

    public void SetSunrise()
    {
        if (smoothTransition)
            TransitionToTime(sunriseStart);
        else
            SetTime(sunriseStart);
    }

    public void SetSunset()
    {
        if (smoothTransition)
            TransitionToTime(sunsetStart);
        else
            SetTime(sunsetStart);
    }

    public void TransitionToTime(float targetHour)
    {
        if (_transitionCoroutine != null)
            StopCoroutine(_transitionCoroutine);

        _transitionCoroutine = StartCoroutine(SmoothTransition(targetHour));
    }

    public void MakeDark()
    {
        if (sunLight != null)
        {
            sunLight.intensity = 0f;
            sunLight.color = Color.black;
        }

        if (!onlyChangeLightNotAmbient)
        {
            RenderSettings.ambientLight = Color.black;
        }
        
        if (useFog)
        {
            RenderSettings.fogColor = Color.black;
        }
    }

    public void ResetLighting()
    {
        SetDay();
    }

    public void RestoreOriginalAmbient()
    {
        if (_settingsSaved)
        {
            RenderSettings.ambientLight = _originalAmbient;
            Debug.Log("[DayNightController] ?ã khôi ph?c Ambient g?c");
        }
    }

    // ?????????????????????????????????????????????????????????????????????
    // Private Methods - C?p nh?t ánh sáng
    // ?????????????????????????????????????????????????????????????????????

    private void UpdateLighting()
    {
        if (sunLight == null) return;

        // Xác ??nh th?i ?i?m trong ngày
        TimeOfDay timeOfDay = GetTimeOfDay();

        // C?p nh?t rotation c?a m?t tr?i
        UpdateSunRotation();

        // C?p nh?t màu s?c và c??ng ??
        switch (timeOfDay)
        {
            case TimeOfDay.Sunrise:
                UpdateSunriseSettings();
                break;
            case TimeOfDay.Day:
                UpdateDaySettings();
                break;
            case TimeOfDay.Sunset:
                UpdateSunsetSettings();
                break;
            case TimeOfDay.Night:
                UpdateNightSettings();
                break;
        }

        // C?p nh?t fog
        if (useFog)
            UpdateFog(timeOfDay);
    }

    private void UpdateSunRotation()
    {
        // Xoay m?t tr?i theo th?i gian (0h = -90°, 12h = 90°, 24h = -90°)
        float angle = (currentTime / 24f) * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(angle, 170f, 0f);

        // C?p nh?t m?t tr?ng n?u có
        if (moonLight != null)
        {
            moonLight.transform.rotation = Quaternion.Euler(angle + 180f, 170f, 0f);
            moonLight.enabled = currentTime < sunriseStart || currentTime > sunsetEnd;
        }
    }

    private void UpdateDaySettings()
    {
        sunLight.color = dayLightColor;
        sunLight.intensity = dayIntensity;

        if (!onlyChangeLightNotAmbient)
        {
            Color targetAmbient = Color.Lerp(_originalAmbient, dayAmbientColor, ambientIntensity);
            RenderSettings.ambientLight = targetAmbient;
            RenderSettings.ambientSkyColor = targetAmbient;
            RenderSettings.ambientEquatorColor = targetAmbient;
        }

        if (sunLight != null) sunLight.enabled = true;
        if (moonLight != null) moonLight.enabled = false;
    }

    private void UpdateNightSettings()
    {
        sunLight.color = nightLightColor;
        sunLight.intensity = nightIntensity;

        if (!onlyChangeLightNotAmbient)
        {
            Color targetAmbient = Color.Lerp(_originalAmbient, nightAmbientColor, ambientIntensity);
            RenderSettings.ambientLight = targetAmbient;
            RenderSettings.ambientSkyColor = targetAmbient;
            RenderSettings.ambientEquatorColor = targetAmbient;
        }

        if (sunLight != null) sunLight.enabled = true; // V?n b?t nh?ng intensity th?p
        if (moonLight != null) moonLight.enabled = true;
    }

    private void UpdateSunriseSettings()
    {
        float t = Mathf.InverseLerp(sunriseStart, sunriseEnd, currentTime);
        
        sunLight.color = Color.Lerp(sunsetLightColor, dayLightColor, t);
        sunLight.intensity = Mathf.Lerp(sunsetIntensity, dayIntensity, t);

        if (!onlyChangeLightNotAmbient)
        {
            Color baseAmbient = Color.Lerp(sunsetAmbientColor, dayAmbientColor, t);
            Color targetAmbient = Color.Lerp(_originalAmbient, baseAmbient, ambientIntensity);
            RenderSettings.ambientLight = targetAmbient;
            RenderSettings.ambientSkyColor = targetAmbient;
            RenderSettings.ambientEquatorColor = targetAmbient;
        }

        if (sunLight != null) sunLight.enabled = true;
        if (moonLight != null) moonLight.enabled = t < 0.5f;
    }

    private void UpdateSunsetSettings()
    {
        float t = Mathf.InverseLerp(sunsetStart, sunsetEnd, currentTime);
        
        sunLight.color = Color.Lerp(dayLightColor, sunsetLightColor, t);
        sunLight.intensity = Mathf.Lerp(dayIntensity, sunsetIntensity, t);

        if (!onlyChangeLightNotAmbient)
        {
            Color baseAmbient = Color.Lerp(dayAmbientColor, sunsetAmbientColor, t);
            Color targetAmbient = Color.Lerp(_originalAmbient, baseAmbient, ambientIntensity);
            RenderSettings.ambientLight = targetAmbient;
            RenderSettings.ambientSkyColor = targetAmbient;
            RenderSettings.ambientEquatorColor = targetAmbient;
        }

        if (sunLight != null) sunLight.enabled = true;
        if (moonLight != null) moonLight.enabled = t > 0.5f;
    }

    private void UpdateFog(TimeOfDay timeOfDay)
    {
        RenderSettings.fog = true;

        switch (timeOfDay)
        {
            case TimeOfDay.Day:
            case TimeOfDay.Sunrise:
                RenderSettings.fogColor = dayFogColor;
                RenderSettings.fogDensity = dayFogDensity;
                break;
            case TimeOfDay.Night:
            case TimeOfDay.Sunset:
                RenderSettings.fogColor = nightFogColor;
                RenderSettings.fogDensity = nightFogDensity;
                break;
        }
    }

    private TimeOfDay GetTimeOfDay()
    {
        if (currentTime >= sunriseStart && currentTime < sunriseEnd)
            return TimeOfDay.Sunrise;
        else if (currentTime >= sunriseEnd && currentTime < sunsetStart)
            return TimeOfDay.Day;
        else if (currentTime >= sunsetStart && currentTime < sunsetEnd)
            return TimeOfDay.Sunset;
        else
            return TimeOfDay.Night;
    }

    // ?????????????????????????????????????????????????????????????????????
    // Coroutines
    // ?????????????????????????????????????????????????????????????????????

    private IEnumerator SmoothTransition(float targetHour)
    {
        float startTime = currentTime;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            currentTime = Mathf.Lerp(startTime, targetHour, t);
            UpdateLighting();

            yield return null;
        }

        currentTime = targetHour;
        UpdateLighting();
        _transitionCoroutine = null;
    }

    // ?????????????????????????????????????????????????????????????????????
    // Helper Enum
    // ?????????????????????????????????????????????????????????????????????

    private enum TimeOfDay
    {
        Sunrise,
        Day,
        Sunset,
        Night
    }

    // ?????????????????????????????????????????????????????????????????????
    // Gizmos
    // ?????????????????????????????????????????????????????????????????????

    private void OnDrawGizmosSelected()
    {
        if (sunLight != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(sunLight.transform.position, 0.5f);
            Gizmos.DrawRay(sunLight.transform.position, sunLight.transform.forward * 5f);
        }
    }
}
