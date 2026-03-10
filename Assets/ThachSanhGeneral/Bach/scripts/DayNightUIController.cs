using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Controller ?? ?i?u khi?n th?i gian và ?? t?i
/// Gán script này lên Canvas ho?c GameObject qu?n lý UI
/// </summary>
public class DayNightUIController : MonoBehaviour
{
    [Header("Controller References")]
    [Tooltip("Script ?i?u khi?n ngày/?êm ??y ??")]
    public DayNightController dayNightController;

    [Tooltip("Script ?i?u khi?n ?? t?i ??n gi?n")]
    public SimpleDarknessController darknessController;

    [Header("UI Buttons")]
    [Tooltip("Nút chuy?n sang ban ngày")]
    public Button dayButton;

    [Tooltip("Nút chuy?n sang ban ?êm")]
    public Button nightButton;

    [Tooltip("Nút chuy?n sang bình minh")]
    public Button sunriseButton;

    [Tooltip("Nút chuy?n sang hoàng hôn")]
    public Button sunsetButton;

    [Tooltip("Nút làm t?i hoàn toàn")]
    public Button darkButton;

    [Tooltip("Nút làm sáng l?i")]
    public Button brightButton;

    [Header("UI Sliders")]
    [Tooltip("Slider ?i?u ch?nh th?i gian (0-24 gi?)")]
    public Slider timeSlider;

    [Tooltip("Slider ?i?u ch?nh ?? t?i (0-1)")]
    public Slider darknessSlider;

    [Header("UI Text")]
    [Tooltip("Text hi?n th? th?i gian hi?n t?i")]
    public Text timeText;

    private void Start()
    {
        // T? ??ng tìm controllers n?u ch?a gán
        if (dayNightController == null)
            dayNightController = FindObjectOfType<DayNightController>();

        if (darknessController == null)
            darknessController = FindObjectOfType<SimpleDarknessController>();

        // Gán s? ki?n cho buttons
        SetupButtons();

        // Gán s? ki?n cho sliders
        SetupSliders();
    }

    private void Update()
    {
        // C?p nh?t UI
        UpdateTimeDisplay();
    }

    private void SetupButtons()
    {
        if (dayButton != null)
            dayButton.onClick.AddListener(OnDayButtonClicked);

        if (nightButton != null)
            nightButton.onClick.AddListener(OnNightButtonClicked);

        if (sunriseButton != null)
            sunriseButton.onClick.AddListener(OnSunriseButtonClicked);

        if (sunsetButton != null)
            sunsetButton.onClick.AddListener(OnSunsetButtonClicked);

        if (darkButton != null)
            darkButton.onClick.AddListener(OnDarkButtonClicked);

        if (brightButton != null)
            brightButton.onClick.AddListener(OnBrightButtonClicked);
    }

    private void SetupSliders()
    {
        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = 24f;
            if (dayNightController != null)
                timeSlider.value = dayNightController.currentTime;
            timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);
        }

        if (darknessSlider != null)
        {
            darknessSlider.minValue = 0f;
            darknessSlider.maxValue = 1f;
            if (darknessController != null)
                darknessSlider.value = darknessController.darknessLevel;
            darknessSlider.onValueChanged.AddListener(OnDarknessSliderChanged);
        }
    }

    // ?????????????????????????????????????????????????????????????????????
    // Button Events
    // ?????????????????????????????????????????????????????????????????????

    private void OnDayButtonClicked()
    {
        if (dayNightController != null)
        {
            dayNightController.SetDay();
            Debug.Log("[DayNightUI] Chuy?n sang ban ngày");
        }
    }

    private void OnNightButtonClicked()
    {
        if (dayNightController != null)
        {
            dayNightController.SetNight();
            Debug.Log("[DayNightUI] Chuy?n sang ban ?êm");
        }
    }

    private void OnSunriseButtonClicked()
    {
        if (dayNightController != null)
        {
            dayNightController.SetSunrise();
            Debug.Log("[DayNightUI] Chuy?n sang bình minh");
        }
    }

    private void OnSunsetButtonClicked()
    {
        if (dayNightController != null)
        {
            dayNightController.SetSunset();
            Debug.Log("[DayNightUI] Chuy?n sang hoàng hôn");
        }
    }

    private void OnDarkButtonClicked()
    {
        if (darknessController != null)
        {
            darknessController.MakePitchDark();
            Debug.Log("[DayNightUI] Làm t?i hoàn toàn");
        }
        else if (dayNightController != null)
        {
            dayNightController.MakeDark();
            Debug.Log("[DayNightUI] Làm t?i hoàn toàn");
        }
    }

    private void OnBrightButtonClicked()
    {
        if (darknessController != null)
        {
            darknessController.MakeBright();
            Debug.Log("[DayNightUI] Làm sáng l?i");
        }
        else if (dayNightController != null)
        {
            dayNightController.SetDay();
            Debug.Log("[DayNightUI] Làm sáng l?i");
        }
    }

    // ?????????????????????????????????????????????????????????????????????
    // Slider Events
    // ?????????????????????????????????????????????????????????????????????

    private void OnTimeSliderChanged(float value)
    {
        if (dayNightController != null)
        {
            dayNightController.SetTime(value);
        }
    }

    private void OnDarknessSliderChanged(float value)
    {
        if (darknessController != null)
        {
            darknessController.SetDarkness(value);
        }
    }

    // ?????????????????????????????????????????????????????????????????????
    // UI Update
    // ?????????????????????????????????????????????????????????????????????

    private void UpdateTimeDisplay()
    {
        if (timeText != null && dayNightController != null)
        {
            float time = dayNightController.currentTime;
            int hours = Mathf.FloorToInt(time);
            int minutes = Mathf.FloorToInt((time - hours) * 60);
            timeText.text = string.Format("{0:00}:{1:00}", hours, minutes);
        }

        // Sync slider v?i controller
        if (timeSlider != null && dayNightController != null)
        {
            timeSlider.value = dayNightController.currentTime;
        }
    }

    // ?????????????????????????????????????????????????????????????????????
    // Public Methods - G?i t? n?i khác
    // ?????????????????????????????????????????????????????????????????????

    public void SetTimeOfDay(string timeOfDay)
    {
        if (dayNightController == null) return;

        switch (timeOfDay.ToLower())
        {
            case "day":
            case "ngày":
                dayNightController.SetDay();
                break;
            case "night":
            case "?êm":
                dayNightController.SetNight();
                break;
            case "sunrise":
            case "bình minh":
                dayNightController.SetSunrise();
                break;
            case "sunset":
            case "hoàng hôn":
                dayNightController.SetSunset();
                break;
        }
    }
}
