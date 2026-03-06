#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarEditorTool
{
    [MenuItem("Tools/Auto-Generate Boss Health Bar UI")]
    public static void CreateBossHealthBarUI()
    {
        // 1. Tìm hoặc tạo Canvas
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        GameObject canvasGO;

        if (canvas == null)
        {
            canvasGO = new GameObject("BossUICanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasGO = canvas.gameObject;
            // Báo lại nếu Canvas đang dùng RenderMode khác (không sao, cứ dùng)
        }

        // 2. Tạo GameObject chính cho Health Bar
        GameObject healthBarGO = new GameObject("ChanTinhBossHealthBar_UI");
        healthBarGO.transform.SetParent(canvasGO.transform, false);

        RectTransform healthBarRect = healthBarGO.AddComponent<RectTransform>();
        // Đặt ở giữa mép trên màn hình
        healthBarRect.anchorMin = new Vector2(0.5f, 1f);
        healthBarRect.anchorMax = new Vector2(0.5f, 1f);
        healthBarRect.pivot = new Vector2(0.5f, 1f);
        healthBarRect.anchoredPosition = new Vector2(0, -30f);
        healthBarRect.sizeDelta = new Vector2(600f, 40f);

        // Gắn Script
        ChanTinhBossHealthBar healthBarScript = healthBarGO.AddComponent<ChanTinhBossHealthBar>();
        healthBarScript.isWorldSpace = false;

        // 3. Tạo Ảnh nền (Background)
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(healthBarGO.transform, false);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // 4. Tạo Vùng Fill Area và ảnh Fill
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(healthBarGO.transform, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(5f, 5f);
        fillAreaRect.offsetMax = new Vector2(-5f, -5f);

        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = Color.red;
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // 5. Thêm Slider Component
        Slider slider = healthBarGO.AddComponent<Slider>();
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.fillRect = fillRect;
        slider.value = 1f;

        // 6. Tạo Text (Tên Boss)
        GameObject textGO = new GameObject("Boss Name");
        textGO.transform.SetParent(healthBarGO.transform, false);
        Text nameText = textGO.AddComponent<Text>();
        nameText.text = "CHẰN TINH";
        nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        nameText.fontStyle = FontStyle.Bold;
        nameText.fontSize = 22;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = Color.white;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Thêm viền cho chữ dễ đọc (Outline)
        Outline outline = textGO.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        // 7. Gán Reference
        healthBarScript.healthSlider = slider;
        healthBarScript.fillImage = fillImage;
        healthBarScript.bossNameText = nameText;

        // Tự động tìm Boss để gán vào nếu có trong Scene
        ChanTinhBossController bossController = GameObject.FindObjectOfType<ChanTinhBossController>();
        if (bossController != null)
        {
            healthBarScript.boss = bossController;
        }

        // Đánh dấu undo để có thể Ctrl+Z
        Undo.RegisterCreatedObjectUndo(healthBarGO, "Create Boss Health Bar UI");
        
        // Chọn GameObject mới tạo để user dễ thấy
        Selection.activeGameObject = healthBarGO;

        Debug.Log("Tạo Boss Health Bar UI thành công! Đã tự động gắn vào giữa mép trên màn hình.");
    }
}
#endif
