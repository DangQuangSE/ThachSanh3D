using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Auto setup Dialogue Box theo style Genshin Impact
/// Gán script này vào DialoguePanel ?? t? ??ng style
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class GenshinDialogueStyler : MonoBehaviour
{
    [Header("Color Palette - Genshin Impact")]
    [Tooltip("Màu vàng ch? ??o")]
    public Color primaryYellow = new Color(1f, 0.78f, 0.39f); // #FFC864

    [Tooltip("Màu nâu ??m cho text trên vàng")]
    public Color darkBrown = new Color(0.18f, 0.12f, 0.06f); // #2D1F0F

    [Tooltip("Màu ?en trong su?t cho background")]
    public Color semiTransparentBlack = new Color(0f, 0f, 0f, 0.7f); // Alpha: 180/255

    [Tooltip("Màu vàng outline")]
    public Color goldOutline = new Color(1f, 0.84f, 0f); // #FFD700

    [Header("UI References")]
    [Tooltip("Panel tên nhân v?t")]
    public GameObject characterNamePanel;

    [Tooltip("Text tên nhân v?t (TMP)")]
    public TMP_Text characterNameText;

    [Tooltip("Panel h?i tho?i chính")]
    public GameObject dialoguePanel;

    [Tooltip("Text h?i tho?i (TMP)")]
    public TMP_Text dialogueText;

    [Tooltip("Nút Next")]
    public Button nextButton;

    [Header("Animation Settings")]
    [Tooltip("Th?i gian fade in/out")]
    public float fadeDuration = 0.3f;

    [Tooltip("Th?i gian bounce name")]
    public float bounceDuration = 0.4f;

    [Tooltip("Scale bounce amount")]
    public float bounceScale = 1.15f;

    [Header("Auto Style On Start")]
    [Tooltip("T? ??ng apply style khi Start()")]
    public bool autoApplyStyle = true;

    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        if (autoApplyStyle)
        {
            ApplyGenshinStyle();
        }
    }

    /// <summary>
    /// Áp d?ng toàn b? style Genshin Impact
    /// </summary>
    [ContextMenu("Apply Genshin Style")]
    public void ApplyGenshinStyle()
    {
        StyleCharacterNamePanel();
        StyleCharacterNameText();
        StyleDialoguePanel();
        StyleDialogueText();
        StyleNextButton();

        Debug.Log("[GenshinStyler] ? ?ã áp d?ng Genshin Impact style!");
    }

    // ???????????????????????????????????????????????????????????
    // Character Name Panel
    // ???????????????????????????????????????????????????????????

    private void StyleCharacterNamePanel()
    {
        if (characterNamePanel == null) return;

        Image image = characterNamePanel.GetComponent<Image>();
        if (image != null)
        {
            image.color = primaryYellow;
        }

        // Add Outline
        Outline outline = characterNamePanel.GetComponent<Outline>();
        if (outline == null)
            outline = characterNamePanel.AddComponent<Outline>();

        outline.effectColor = new Color(0.25f, 0.16f, 0.05f, 1f); // Nâu ??m
        outline.effectDistance = new Vector2(2, -2);

        Debug.Log("[GenshinStyler] ? Character Name Panel styled");
    }

    private void StyleCharacterNameText()
    {
        if (characterNameText == null) return;

        characterNameText.fontSize = 32;
        characterNameText.color = darkBrown;
        characterNameText.fontStyle = FontStyles.Bold;
        characterNameText.alignment = TextAlignmentOptions.Center;

        // Add Shadow
        var shadow = characterNameText.gameObject.GetComponent<Shadow>();
        if (shadow == null)
            shadow = characterNameText.gameObject.AddComponent<Shadow>();

        shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
        shadow.effectDistance = new Vector2(1, -1);

        Debug.Log("[GenshinStyler] ? Character Name Text styled");
    }

    // ???????????????????????????????????????????????????????????
    // Dialogue Panel
    // ???????????????????????????????????????????????????????????

    private void StyleDialoguePanel()
    {
        if (dialoguePanel == null) return;

        Image image = dialoguePanel.GetComponent<Image>();
        if (image != null)
        {
            image.color = semiTransparentBlack;
        }

        // Add Gold Outline
        Outline outline = dialoguePanel.GetComponent<Outline>();
        if (outline == null)
            outline = dialoguePanel.AddComponent<Outline>();

        outline.effectColor = goldOutline;
        outline.effectDistance = new Vector2(3, -3);

        Debug.Log("[GenshinStyler] ? Dialogue Panel styled");
    }

    private void StyleDialogueText()
    {
        if (dialogueText == null) return;

        dialogueText.fontSize = 28;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        dialogueText.enableWordWrapping = true;
        dialogueText.overflowMode = TextOverflowModes.Overflow;

        // Add Shadow
        var shadow = dialogueText.gameObject.GetComponent<Shadow>();
        if (shadow == null)
            shadow = dialogueText.gameObject.AddComponent<Shadow>();

        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);

        Debug.Log("[GenshinStyler] ? Dialogue Text styled");
    }

    // ???????????????????????????????????????????????????????????
    // Next Button
    // ???????????????????????????????????????????????????????????

    private void StyleNextButton()
    {
        if (nextButton == null) return;

        Image buttonImage = nextButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = primaryYellow;
        }

        // Color transitions
        ColorBlock colors = nextButton.colors;
        colors.normalColor = primaryYellow;
        colors.highlightedColor = new Color(1f, 0.85f, 0.5f); // Lighter yellow
        colors.pressedColor = new Color(0.8f, 0.62f, 0.3f);   // Darker yellow
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
        nextButton.colors = colors;

        // Style button text
        TMP_Text buttonText = nextButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.fontSize = 24;
            buttonText.color = darkBrown;
            buttonText.fontStyle = FontStyles.Bold;
        }

        Debug.Log("[GenshinStyler] ? Next Button styled");
    }

    // ???????????????????????????????????????????????????????????
    // Animations - KI?M TRA ACTIVE TR??C KHI CH?Y COROUTINE
    // ???????????????????????????????????????????????????????????

    /// <summary>
    /// Fade in dialogue panel
    /// </summary>
    public void FadeIn()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[GenshinStyler] Cannot FadeIn - GameObject is inactive!");
            return;
        }
        StartCoroutine(FadeInCoroutine());
    }

    /// <summary>
    /// Fade out dialogue panel
    /// </summary>
    public void FadeOut()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[GenshinStyler] Cannot FadeOut - GameObject is inactive!");
            return;
        }
        StartCoroutine(FadeOutCoroutine());
    }

    /// <summary>
    /// Bounce animation cho character name
    /// </summary>
    public void BounceCharacterName()
    {
        if (characterNamePanel == null) return;
        
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[GenshinStyler] Cannot Bounce - GameObject is inactive!");
            return;
        }
        
        StartCoroutine(BounceCoroutine(characterNamePanel.transform));
    }

    private IEnumerator FadeInCoroutine()
    {
        if (_canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutCoroutine()
    {
        if (_canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 0f;
    }

    private IEnumerator BounceCoroutine(Transform target)
    {
        Vector3 originalScale = target.localScale;

        // Scale up
        float elapsed = 0f;
        float halfDuration = bounceDuration / 2f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            target.localScale = Vector3.Lerp(originalScale, originalScale * bounceScale, t);
            yield return null;
        }

        // Scale back
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            target.localScale = Vector3.Lerp(originalScale * bounceScale, originalScale, t);
            yield return null;
        }

        target.localScale = originalScale;
    }

    // ???????????????????????????????????????????????????????????
    // Helper Methods
    // ???????????????????????????????????????????????????????????

    /// <summary>
    /// Update character name
    /// </summary>
    public void SetCharacterName(string name)
    {
        if (characterNameText != null)
        {
            characterNameText.text = name;
            BounceCharacterName();
        }
    }

    /// <summary>
    /// Update dialogue text
    /// </summary>
    public void SetDialogueText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
        }
    }

    /// <summary>
    /// Show dialogue v?i animation
    /// </summary>
    public void ShowDialogue(string characterName, string dialogue)
    {
        SetCharacterName(characterName);
        SetDialogueText(dialogue);
        FadeIn();
    }

    /// <summary>
    /// Hide dialogue v?i animation
    /// </summary>
    public void HideDialogue()
    {
        FadeOut();
    }
}
