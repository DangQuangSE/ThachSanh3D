using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically creates and manages a FIXED health bar at the TOP of the screen for the Eagle Boss.
/// Attach this script to any persistent object or the Boss itself.
/// </summary>
public class BossHealthBarFixedTop : MonoBehaviour
{
    [Header("Settings")]
    public string bossDisplayName = "Đại Bàng Tinh";
    public Color barColor = new Color(0.8f, 0, 0, 1f); // Deep red
    public Vector2 barSize = new Vector2(1000, 45); // Wider and taller
    public float verticalOffset = -65f; // Slightly lower from top edge

    [Header("References (Auto-found if empty)")]
    public BossDaiBangController boss;

    private GameObject _canvasObj;
    private Slider _healthSlider;
    private Text _nameText;
    private Image _fillImage;
    private bool _isInitialized = false;

    void Start()
    {
        if (boss == null)
            boss = FindFirstObjectByType<BossDaiBangController>();

        if (boss != null)
        {
            SetupUI();
        }
    }

    void Update()
    {
        // Try to find boss if not found at start
        if (boss == null)
        {
            boss = FindFirstObjectByType<BossDaiBangController>();
            if (boss != null && !_isInitialized) SetupUI();
        }

        if (boss == null || boss.IsDead())
        {
            if (_canvasObj != null && _canvasObj.activeSelf) _canvasObj.SetActive(false);
            return;
        }

        if (!_isInitialized) return;

        if (!_canvasObj.activeSelf) _canvasObj.SetActive(true);

        // Sync Health
        float healthPercent = boss.GetHealthPercentage();
        if (_healthSlider != null) _healthSlider.value = healthPercent;
        
        // Optional: change color based on health
        if (_fillImage != null)
        {
            _fillImage.color = Color.Lerp(Color.red, barColor, healthPercent);
        }
    }

    private void SetupUI()
    {
        if (_isInitialized) return;

        // 1. Create Canvas
        _canvasObj = new GameObject("BossFixedHealthBarCanvas");
        Canvas canvas = _canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Above most gameplay UI
        
        CanvasScaler scaler = _canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        _canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Create Background (Black border)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(_canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.7f);
        RectTransform bgRect = bgImg.rectTransform;
        bgRect.anchorMin = new Vector2(0.5f, 1f);
        bgRect.anchorMax = new Vector2(0.5f, 1f);
        bgRect.pivot = new Vector2(0.5f, 1f);
        bgRect.anchoredPosition = new Vector2(0, verticalOffset);
        bgRect.sizeDelta = barSize + new Vector2(10, 10);

        // 3. Create Slider
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(bgObj.transform, false);
        _healthSlider = sliderObj.AddComponent<Slider>();
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.sizeDelta = new Vector2(-10, -10);
        
        // Remove bits of slider we don't need
        _healthSlider.interactable = false;
        _healthSlider.transition = Selectable.Transition.None;
        _healthSlider.navigation = new Navigation { mode = Navigation.Mode.None };

        // 4. Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        _fillImage = fillObj.AddComponent<Image>();
        _fillImage.color = barColor;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        _healthSlider.fillRect = fillRect;
        _healthSlider.minValue = 0f;
        _healthSlider.maxValue = 1f;
        _healthSlider.value = 1f;

        // 5. Boss Name Text
        GameObject nameObj = new GameObject("BossNameText");
        nameObj.transform.SetParent(_canvasObj.transform, false);
        _nameText = nameObj.AddComponent<Text>();
        _nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_nameText.font == null) _nameText.font = Font.CreateDynamicFontFromOSFont("Arial", 50);
        _nameText.text = bossDisplayName;
        _nameText.fontSize = 32; // Larger font
        _nameText.fontStyle = FontStyle.Bold;
        _nameText.alignment = TextAnchor.MiddleCenter;
        _nameText.color = Color.white;
        
        Shadow shadow = nameObj.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(2, -2);

        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1f);
        nameRect.anchorMax = new Vector2(0.5f, 1f);
        nameRect.pivot = new Vector2(0.5f, 1f);
        nameRect.anchoredPosition = new Vector2(0, verticalOffset + 50f); // Higher above bar
        nameRect.sizeDelta = new Vector2(800, 60);

        _isInitialized = true;
    }

    private void OnDestroy()
    {
        if (_canvasObj != null) Destroy(_canvasObj);
    }
}
