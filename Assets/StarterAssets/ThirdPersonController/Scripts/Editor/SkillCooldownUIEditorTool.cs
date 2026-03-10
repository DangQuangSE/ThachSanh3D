#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    public class SkillCooldownUIEditorTool
    {
        [MenuItem("Tools/Thach Sanh 3D/Create button countdown skill")]
        public static void CreateSkillCooldownUI()
        {
            // 1. Find or create Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            GameObject canvasGO;
                
            if (canvas == null)
            {
                canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            else
            {
                canvasGO = canvas.gameObject;
            }

            // 2. Create SkillCooldownUI root panel (bottom-right)
            GameObject rootGO = new GameObject("SkillCooldownUI");
            rootGO.transform.SetParent(canvasGO.transform, false);

            RectTransform rootRect = rootGO.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(1f, 0f);
            rootRect.anchorMax = new Vector2(1f, 0f);
            rootRect.pivot = new Vector2(1f, 0f);
            rootRect.anchoredPosition = new Vector2(-20f, 20f);
            rootRect.sizeDelta = new Vector2(280f, 70f);

            HorizontalLayoutGroup hlg = rootGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(5, 5, 3, 3);

            SkillCooldownUI uiScript = rootGO.AddComponent<SkillCooldownUI>();

            float buttonSize = 55f;

            // 3. Create 4 skill buttons
            CreateSkillButton(rootGO.transform, "ESkill_Button", buttonSize, "E",
                out Image eskillIcon, out Image eskillFill, out Text eskillText);

            CreateSkillButton(rootGO.transform, "Protect_Button", buttonSize, "Q",
                out Image protectIcon, out Image protectFill, out Text protectText);

            CreateSkillButton(rootGO.transform, "Roll_Button", buttonSize, "Roll",
                out Image rollIcon, out Image rollFill, out Text rollText);

            CreateSkillButton(rootGO.transform, "Ultimate_Button", buttonSize, "R",
                out Image ultIcon, out Image ultFill, out Text ultText);

            // 4. Assign references
            uiScript.eskillCooldownFill = eskillFill;
            uiScript.eskillCooldownText = eskillText;
            uiScript.eskillIcon = eskillIcon;

            uiScript.protectCooldownFill = protectFill;
            uiScript.protectCooldownText = protectText;
            uiScript.protectIcon = protectIcon;

            uiScript.rollCooldownFill = rollFill;
            uiScript.rollCooldownText = rollText;
            uiScript.rollIcon = rollIcon;

            uiScript.ultimateCooldownFill = ultFill;
            uiScript.ultimateCooldownText = ultText;
            uiScript.ultimateIcon = ultIcon;

            // 5. Auto-find player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                ThirdPersonController controller = playerObj.GetComponent<ThirdPersonController>();
                if (controller != null)
                    uiScript.player = controller;
            }

            Undo.RegisterCreatedObjectUndo(rootGO, "Create Skill Cooldown UI");
            Selection.activeGameObject = rootGO;

            Debug.Log("<color=green>Skill Cooldown UI created successfully!</color>\n" +
                "To set skill icons: expand each button in Hierarchy, select <b>Icon</b>, " +
                "then drag your sprite into <b>Source Image</b> in the Inspector.\n" +
                "Make sure icon textures use <b>Sprite Mode = Single</b>.");
        }

        private static void CreateSkillButton(Transform parent, string name, float size,
            string keyLabel,
            out Image iconImage, out Image fillImage, out Text cooldownText)
        {
            Sprite circleSprite = CreateCircleSprite();

            // Root button container
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            RectTransform btnRect = buttonGO.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(size, size);

            LayoutElement le = buttonGO.AddComponent<LayoutElement>();
            le.preferredWidth = size;
            le.preferredHeight = size;

            // --- Background circle (dark, round frame) ---
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(buttonGO.transform, false);
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.sprite = circleSprite;
            bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
            bgImg.type = Image.Type.Simple;
            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // --- Icon (user drags their sprite here manually) ---
            // Uses a circular mask so any square icon becomes round
            GameObject maskGO = new GameObject("IconMask");
            maskGO.transform.SetParent(buttonGO.transform, false);
            Image maskImg = maskGO.AddComponent<Image>();
            maskImg.sprite = circleSprite;
            maskImg.color = Color.white;
            maskImg.raycastTarget = false;
            Mask mask = maskGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            RectTransform maskRect = maskGO.GetComponent<RectTransform>();
            maskRect.anchorMin = new Vector2(0.08f, 0.08f);
            maskRect.anchorMax = new Vector2(0.92f, 0.92f);
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;

            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(maskGO.transform, false);
            iconImage = iconGO.AddComponent<Image>();
            iconImage.sprite = circleSprite;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            // --- Cooldown fill overlay (Radial360) ---
            GameObject fillGO = new GameObject("CooldownFill");
            fillGO.transform.SetParent(buttonGO.transform, false);
            fillImage = fillGO.AddComponent<Image>();
            fillImage.sprite = circleSprite;
            fillImage.color = new Color(0f, 0f, 0f, 0.7f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Radial360;
            fillImage.fillOrigin = (int)Image.Origin360.Top;
            fillImage.fillClockwise = false;
            fillImage.fillAmount = 0f;
            RectTransform fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // --- Border ring (outer glow) ---
            GameObject borderGO = new GameObject("Border");
            borderGO.transform.SetParent(buttonGO.transform, false);
            Image borderImg = borderGO.AddComponent<Image>();
            borderImg.sprite = circleSprite;
            borderImg.color = new Color(0.8f, 0.85f, 1f, 0.3f);
            borderImg.type = Image.Type.Simple;
            borderImg.raycastTarget = false;
            RectTransform borderRect = borderGO.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-2f, -2f);
            borderRect.offsetMax = new Vector2(2f, 2f);

            // --- Cooldown countdown text (center) ---
            GameObject textGO = new GameObject("CooldownText");
            textGO.transform.SetParent(buttonGO.transform, false);
            cooldownText = textGO.AddComponent<Text>();
            cooldownText.text = "";
            cooldownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (cooldownText.font == null)
                cooldownText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            cooldownText.fontSize = 20;
            cooldownText.fontStyle = FontStyle.Bold;
            cooldownText.alignment = TextAnchor.MiddleCenter;
            cooldownText.color = Color.white;
            cooldownText.raycastTarget = false;
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Outline textOutline = textGO.AddComponent<Outline>();
            textOutline.effectColor = Color.black;
            textOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // --- Key label (below the button) ---
            GameObject labelGO = new GameObject("KeyLabel");
            labelGO.transform.SetParent(buttonGO.transform, false);
            Text labelText = labelGO.AddComponent<Text>();
            labelText.text = keyLabel;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (labelText.font == null)
                labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 11;
            labelText.fontStyle = FontStyle.Bold;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = new Color(1f, 1f, 1f, 0.8f);
            labelText.raycastTarget = false;
            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, -0.05f);
            labelRect.anchorMax = new Vector2(1f, 0.2f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Outline labelOutline = labelGO.AddComponent<Outline>();
            labelOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            labelOutline.effectDistance = new Vector2(1f, -1f);
        }

        /// <summary>
        /// Returns Unity's built-in circle sprite (Knob) for UI elements.
        /// </summary>
        private static Sprite CreateCircleSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }
    }
}
#endif
