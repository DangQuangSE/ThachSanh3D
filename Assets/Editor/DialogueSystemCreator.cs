using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor Script t?o Genshin-style Dialogue System t? ??ng
/// Menu: GameObject > UI > Genshin Dialogue System
/// Tích h?p ??y ?? v?i QuestDialogue script
/// </summary>
public static class DialogueSystemCreator
{
    [MenuItem("GameObject/UI/Genshin Dialogue System", false, 10)]
    public static void CreateDialogueSystem()
    {
        // Tìm ho?c t?o Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("[DialogueCreator] ? ?ã t?o Canvas m?i");
        }

        // T?o EventSystem n?u ch?a có
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[DialogueCreator] ? ?ã t?o EventSystem");
        }

        // T?o Dialogue System Root - ?N BAN ??U
        GameObject dialogueRoot = new GameObject("DialogueSystem");
        dialogueRoot.transform.SetParent(canvas.transform, false);
        dialogueRoot.SetActive(false); // ?N TOÀN B? SYSTEM KHI VÀO GAME
        
        RectTransform rootRect = dialogueRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        // Add CanvasGroup
        CanvasGroup canvasGroup = dialogueRoot.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // T?o Background Overlay (t?i màn hình)
        GameObject bgOverlay = CreateBackgroundOverlay(dialogueRoot.transform);

        // KHÔNG T?O Character Portrait Panel - Gây màn tr?ng bên trái
        // GameObject portraitPanel = CreateCharacterPortraitPanel(dialogueRoot.transform);
        
        // T?o Character Name Panel (thanh tên vàng) - ?I?U CH?NH V? TRÍ
        GameObject namePanel = CreateCharacterNamePanel(dialogueRoot.transform);
        
        // T?o Dialogue Panel (h?p tho?i chính)
        GameObject dialoguePanel = CreateDialoguePanel(dialogueRoot.transform);
        
        // T?o Continue Hint (d?u m?i tên nh?p nháy)
        GameObject continueHint = CreateContinueHint(dialoguePanel.transform);
        
        // T?o Confirm Panel (ch?n nhi?m v?)
        GameObject confirmPanel = CreateConfirmPanel(dialogueRoot.transform);
        confirmPanel.SetActive(false);
        confirmPanel.transform.SetAsLastSibling();

        // T?o Interact Hint (nh?n F) - ??T NGOÀI DialogueSystem
        GameObject interactHint = CreateInteractHint(canvas.transform);
        interactHint.SetActive(false); // ?n ban ??u, ch? hi?n khi g?n NPC

        // Add GenshinDialogueStyler
        GenshinDialogueStyler styler = dialogueRoot.AddComponent<GenshinDialogueStyler>();
        styler.characterNamePanel = namePanel;
        styler.characterNameText = namePanel.GetComponentInChildren<TMP_Text>();
        styler.dialoguePanel = dialoguePanel;
        styler.dialogueText = dialoguePanel.transform.Find("DialogueText").GetComponent<TMP_Text>();
        styler.autoApplyStyle = true;

        // Select the created object
        Selection.activeGameObject = dialogueRoot;

        Debug.Log("=== GENSHIN DIALOGUE SYSTEM CREATED ===");
        Debug.Log("? Dialogue System ?ã ???c t?o!");
        Debug.Log("? DialogueSystem ?N ban ??u, ch? hi?n khi b?t ??u h?i tho?i");
        Debug.Log("? InteractHint ?N ban ??u, hi?n khi g?n NPC");
        Debug.Log("? Character Name t? ??ng thay ??i theo ng??i nói");
        Debug.Log("=======================================");

        EditorUtility.DisplayDialog(
            "Genshin Dialogue System Created", 
            "? ?ã t?o Genshin-style Dialogue System!\n\n" +
            "? DialogueSystem ?N khi vào game\n" +
            "? InteractHint hi?n khi g?n NPC\n" +
            "? Character Name t? ??ng ??i theo ng??i nói\n\n" +
            "Ti?p theo:\n" +
            "1. Add QuestDialogue vào NPC\n" +
            "2. Script t? ??ng tìm UI\n" +
            "3. Play ?? test!",
            "OK"
        );
    }

    // ???????????????????????????????????????????????????????????
    // Background Overlay (t?i màn hình khi h?i tho?i)
    // ???????????????????????????????????????????????????????????

    private static GameObject CreateBackgroundOverlay(Transform parent)
    {
        GameObject overlay = new GameObject("BackgroundOverlay");
        overlay.transform.SetParent(parent, false);

        RectTransform rect = overlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Image image = overlay.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.4f); // ?en trong su?t

        Debug.Log("[DialogueCreator] ? Background Overlay created");
        return overlay;
    }

    // ???????????????????????????????????????????????????????????
    // Character Portrait Panel (bên trái - gi?ng Genshin)
    // ???????????????????????????????????????????????????????????

    private static GameObject CreateCharacterPortraitPanel(Transform parent)
    {
        GameObject portraitPanel = new GameObject("CharacterPortraitPanel");
        portraitPanel.transform.SetParent(parent, false);

        RectTransform rect = portraitPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(50, 0);
        rect.sizeDelta = new Vector2(400, 600);

        Image image = portraitPanel.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.1f); // Placeholder cho portrait
        
        // Thêm border vàng Genshin-style
        GameObject border = new GameObject("Border");
        border.transform.SetParent(portraitPanel.transform, false);
        
        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = new Vector2(10, 10);
        
        Outline outline = border.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.4f); // Vàng Genshin
        outline.effectDistance = new Vector2(4, 4);

        Debug.Log("[DialogueCreator] ? Character Portrait Panel created");
        return portraitPanel;
    }

    // ???????????????????????????????????????????????????????????
    // Character Name Panel (thanh tên vàng) - ?I?U CH?NH V? TRÍ
    // ???????????????????????????????????????????????????????????

    private static GameObject CreateCharacterNamePanel(Transform parent)
    {
        GameObject namePanel = new GameObject("CharacterNamePanel");
        namePanel.transform.SetParent(parent, false);

        RectTransform rect = namePanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f); // Anchor gi?a ?áy màn hình
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0, 310); // Phía trên DialoguePanel
        rect.sizeDelta = new Vector2(400, 60);

        Image image = namePanel.AddComponent<Image>();
        image.color = new Color(1f, 0.85f, 0.4f); // Vàng Genshin

        // Shadow cho depth
        Shadow shadow = namePanel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(3, -3);

        // Text
        GameObject textObj = new GameObject("CharacterNameText");
        textObj.transform.SetParent(namePanel.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 0);
        textRect.offsetMax = new Vector2(-20, 0);

        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Lý Thông";
        text.fontSize = 32;
        text.color = new Color(0.18f, 0.12f, 0.06f); // Nâu ??m
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center; // CENTER ?? d? nhìn
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;

        // Thêm outline cho text
        Outline textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = new Color(1f, 1f, 1f, 0.5f);
        textOutline.effectDistance = new Vector2(2, 2);

        Debug.Log("[DialogueCreator] ? Character Name Panel created (centered)");
        return namePanel;
    }

    // ???????????????????????????????????????????????????????????
    // Dialogue Panel (h?p tho?i chính)
    // ???????????????????????????????????????????????????????????

    private static GameObject CreateDialoguePanel(Transform parent)
    {
        GameObject dialoguePanel = new GameObject("DialoguePanel");
        dialoguePanel.transform.SetParent(parent, false);

        RectTransform rect = dialoguePanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0, 50);
        rect.sizeDelta = new Vector2(-100, 250);

        Image image = dialoguePanel.AddComponent<Image>();
        image.color = new Color(0.05f, 0.05f, 0.1f, 0.85f); // ?en xanh trong su?t

        // Border vàng Genshin-style
        Outline outline = dialoguePanel.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.4f);
        outline.effectDistance = new Vector2(3, -3);

        // Shadow cho depth
        Shadow shadow = dialoguePanel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(5, -5);

        // Dialogue Text
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(dialoguePanel.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(50, 70);
        textRect.offsetMax = new Vector2(-50, -30);

        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Hi?n ?? ?i, ?êm nay ??n phiên ta ph?i ?i canh mi?u th?...";
        text.fontSize = 32;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.lineSpacing = 5;

        // Text shadow
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        textShadow.effectDistance = new Vector2(2, -2);

        Debug.Log("[DialogueCreator] ? Dialogue Panel created");
        return dialoguePanel;
    }

    // ???????????????????????????????????????????????????????????
    // Continue Hint (d?u m?i tên nh?p nháy)
    // ???????????????????????????????????????????????????????????

    private static GameObject CreateContinueHint(Transform parent)
    {
        GameObject hintObj = new GameObject("ContinueHint");
        hintObj.transform.SetParent(parent, false);

        RectTransform rect = hintObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-50, 15);
        rect.sizeDelta = new Vector2(400, 40);

        TMP_Text text = hintObj.AddComponent<TextMeshProUGUI>();
        text.text = "? SPACE ho?c Click";
        text.fontSize = 24;
        text.color = new Color(1f, 0.92f, 0.23f); // Vàng sáng Genshin
        text.alignment = TextAlignmentOptions.Right;
        text.fontStyle = FontStyles.Bold;
        text.enableWordWrapping = false;

        // Thêm glow effect
        Outline outline = hintObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.4f, 0.8f);
        outline.effectDistance = new Vector2(2, 2);

        // Shadow
        Shadow shadow = hintObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);

        // Add blink script
        hintObj.AddComponent<ContinueHintBlink>();

        // Thêm text riêng ?? QuestDialogue có th? update
        GameObject hintTextObj = new GameObject("ContinueHintText");
        hintTextObj.transform.SetParent(hintObj.transform, false);
        
        RectTransform hintTextRect = hintTextObj.AddComponent<RectTransform>();
        hintTextRect.anchorMin = Vector2.zero;
        hintTextRect.anchorMax = Vector2.one;
        hintTextRect.sizeDelta = Vector2.zero;
        
        TMP_Text hintText = hintTextObj.AddComponent<TextMeshProUGUI>();
        hintText.text = "? SPACE ho?c Click";
        hintText.fontSize = 24;
        hintText.color = new Color(1f, 0.92f, 0.23f);
        hintText.alignment = TextAlignmentOptions.Right;
        hintText.fontStyle = FontStyles.Bold;

        hintObj.SetActive(false);

        Debug.Log("[DialogueCreator] ? Continue Hint created");
        return hintObj;
    }

    // ???????????????????????????????????????????????????????????
    // Confirm Panel (ch?n nhi?m v?)
    // ???????????????????????????????????????????????????????????

    private static GameObject CreateConfirmPanel(Transform parent)
    {
        GameObject confirmPanel = new GameObject("ConfirmPanel");
        confirmPanel.transform.SetParent(parent, false);

        RectTransform rect = confirmPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(700, 400); // L?n h?n, ??p h?n

        Image image = confirmPanel.AddComponent<Image>();
        image.color = new Color(0.08f, 0.08f, 0.12f, 0.98f); // ?en xanh ??m h?n
        image.raycastTarget = true;

        // Add CanvasGroup
        CanvasGroup canvasGroup = confirmPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Border vàng ??p h?n
        Outline outline = confirmPanel.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.4f);
        outline.effectDistance = new Vector2(5, -5);

        // Shadow m?nh h?n
        Shadow shadow = confirmPanel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
        shadow.effectDistance = new Vector2(10, -10);

        // Decorative Top Bar (thanh vàng phía trên)
        GameObject topBar = new GameObject("TopBar");
        topBar.transform.SetParent(confirmPanel.transform, false);
        
        RectTransform topBarRect = topBar.AddComponent<RectTransform>();
        topBarRect.anchorMin = new Vector2(0f, 1f);
        topBarRect.anchorMax = new Vector2(1f, 1f);
        topBarRect.pivot = new Vector2(0.5f, 1f);
        topBarRect.anchoredPosition = Vector2.zero;
        topBarRect.sizeDelta = new Vector2(0, 8);
        
        Image topBarImg = topBar.AddComponent<Image>();
        topBarImg.color = new Color(1f, 0.85f, 0.4f); // Vàng Genshin

        // Icon/Symbol (d?u sao ho?c symbol)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(confirmPanel.transform, false);
        
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 1f);
        iconRect.anchorMax = new Vector2(0.5f, 1f);
        iconRect.pivot = new Vector2(0.5f, 1f);
        iconRect.anchoredPosition = new Vector2(0, -25);
        iconRect.sizeDelta = new Vector2(60, 60);
        
        TMP_Text iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = "?"; // Star icon
        iconText.fontSize = 48;
        iconText.color = new Color(1f, 0.85f, 0.4f);
        iconText.alignment = TextAlignmentOptions.Center;

        // Title - ??p h?n
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(confirmPanel.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -95);
        titleRect.sizeDelta = new Vector2(0, 70);

        TMP_Text titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "NH?N NHI?M V?";
        titleText.fontSize = 48;
        titleText.color = new Color(1f, 0.92f, 0.23f); // Vàng sáng
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;

        // Title outline l?n h?n
        Outline titleOutline = titleObj.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.5f, 0.4f, 0.2f);
        titleOutline.effectDistance = new Vector2(4, -4);

        // Subtitle/Description
        GameObject subtitleObj = new GameObject("Subtitle");
        subtitleObj.transform.SetParent(confirmPanel.transform, false);
        
        RectTransform subtitleRect = subtitleObj.AddComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0f, 0.5f);
        subtitleRect.anchorMax = new Vector2(1f, 0.5f);
        subtitleRect.pivot = new Vector2(0.5f, 0.5f);
        subtitleRect.anchoredPosition = new Vector2(0, 20);
        subtitleRect.sizeDelta = new Vector2(-80, 80);

        TMP_Text subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
        subtitleText.text = "B?n có mu?n nh?n nhi?m v? này không?";
        subtitleText.fontSize = 28;
        subtitleText.color = new Color(0.9f, 0.9f, 0.9f);
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.enableWordWrapping = true;

        // Buttons - ??p h?n, xa nhau h?n
        GameObject acceptBtn = CreateButton(confirmPanel.transform, "AcceptButton", 
            new Vector2(-130, -110), "? ??NG Ý", new Color(0.2f, 0.8f, 0.3f), 220, 70);

        GameObject declineBtn = CreateButton(confirmPanel.transform, "DeclineButton", 
            new Vector2(130, -110), "? T? CH?I", new Color(0.8f, 0.2f, 0.2f), 220, 70);

        Debug.Log("[DialogueCreator] ? Confirm Panel created (Redesigned Genshin Style)");
        return confirmPanel;
    }

    // ???????????????????????????????????????????????????????????
    // Interact Hint (nh?n F)
    // ???????????????????????????????????????????????????????????

    private static GameObject CreateInteractHint(Transform parent)
    {
        GameObject hintObj = new GameObject("InteractHint");
        hintObj.transform.SetParent(parent, false);

        RectTransform rect = hintObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, -250);
        rect.sizeDelta = new Vector2(250, 80);

        Image image = hintObj.AddComponent<Image>();
        image.color = new Color(0.05f, 0.05f, 0.1f, 0.8f);

        // Border vàng
        Outline outline = hintObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.4f);
        outline.effectDistance = new Vector2(3, -3);

        // Shadow
        Shadow shadow = hintObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(4, -4);

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(hintObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Nh?n [F]";
        text.fontSize = 32;
        text.color = new Color(1f, 0.92f, 0.23f); // Vàng Genshin
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;

        // Text outline
        Outline textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = new Color(0.5f, 0.4f, 0.1f);
        textOutline.effectDistance = new Vector2(2, -2);

        Debug.Log("[DialogueCreator] ? Interact Hint created");
        return hintObj;
    }

    // ???????????????????????????????????????????????????????????
    // Helper: Create Button
    // ???????????????????????????????????????????????????????????

    private static GameObject CreateButton(Transform parent, string name, Vector2 position, string label, Color color, int width = 180, int height = 60)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(width, height);

        Image image = btnObj.AddComponent<Image>();
        image.color = color;

        Button button = btnObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.3f;
        colors.pressedColor = color * 0.7f;
        colors.selectedColor = color * 1.2f;
        button.colors = colors;

        // Border cho button - ??p h?n
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.4f, 0.9f);
        outline.effectDistance = new Vector2(3, -3);

        // Shadow m?nh h?n
        Shadow shadow = btnObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        shadow.effectDistance = new Vector2(4, -4);

        // Text - Font l?n h?n
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 32; // L?n h?n
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;

        // Text outline ??m h?n
        Outline textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        textOutline.effectDistance = new Vector2(3, -3);

        return btnObj;
    }
}

// ???????????????????????????????????????????????????????????
// Continue Hint Blink Animation
// ???????????????????????????????????????????????????????????

public class ContinueHintBlink : MonoBehaviour
{
    public float blinkSpeed = 1.5f;
    private TMP_Text _text;
    private float _alpha;

    void Start()
    {
        _text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (_text == null) return;

        _alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        Color color = _text.color;
        color.a = Mathf.Lerp(0.4f, 1f, _alpha);
        _text.color = color;
    }
}
#endif
