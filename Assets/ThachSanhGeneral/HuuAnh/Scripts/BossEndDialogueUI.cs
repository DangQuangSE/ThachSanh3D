using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BossEndDialogueUI : MonoBehaviour
{
    private GameObject canvasObj;
    private Text speakerText;
    private Text dialogueText;
    private Text continueText;
    private Image background;

    [Header("Settings")]
    public float autoSkipTime = 3f; // Thời gian chờ tự động chuyển sang câu tiếp theo
    public float skipDelay = 0.5f; // Thời gian delay tối thiểu để có thể ấn next sang câu khác (giây)

    private bool isWaitingForInput = false;

    private struct Line
    {
        public string speakerName;
        public string text;
        public bool isNarration;
    }

    private List<Line> storyLines = new List<Line>()
    {
        new Line { speakerName = "Thạch Sanh", text = "Anh Lý Thông ơi! Tôi đã hạ được Đại Bàng tinh và cứu được công chúa rồi. Anh hãy thả dây xuống để đưa công chúa lên trước, tôi sẽ lên sau!" },
        new Line { speakerName = "Lý Thông", text = "(Mắt sáng rực, giọng vồn vã) Hiền đệ giỏi lắm! Mau buộc dây vào người công chúa, ta sẽ kéo nàng lên ngay. Đất nước mãi mãi ghi ơn người anh hùng như đệ!" },
        new Line { speakerName = "", text = "(Sau khi kéo được công chúa lên, Lý Thông nhìn xuống hang tối, nảy ra ý đồ độc ác)", isNarration = true },
        new Line { speakerName = "Thạch Sanh", text = "Công chúa đã an toàn chưa anh? Giờ hãy thả dây xuống cho tôi nhé!" },
        new Line { speakerName = "Lý Thông", text = "(Giọng trở nên lạnh lùng, xảo quyệt) Thạch Sanh à, đệ đã làm rất tốt. Chằn Tinh và Đại Bàng Tinh đều đã bị tiêu diệt. Nhưng công lao này, một mình ta hưởng là đủ rồi. Đệ cứ ở lại dưới đó mà vui vầy với hang đá nhé!" },
        new Line { speakerName = "Thạch Sanh", text = "(Bàng hoàng) Anh nói gì vậy? Anh Lý Thông!" },
        new Line { speakerName = "Lý Thông", text = "(Cười lớn) Quân đâu! Mau lăn đá lấp kín cửa hang lại. Ta phải về triều báo tin vui là chính ta đã diệt quái vật cứu công chúa. Mọi việc đã hoàn tất đúng như kế hoạch của ta!" },
        new Line { speakerName = "Thạch Sanh", text = "(Tiếng kêu vọng ra từ đống đất đá) Lý Thông... tại sao anh lại đối xử với tôi như vậy..." },
        new Line { speakerName = "", text = "Còn tiếp...", isNarration = true }
    };

    public void SetupUI()
    {
        // 1. Create Canvas
        canvasObj = new GameObject("BossDialogueCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Main Dialogue Panel (Bottom Container)
        GameObject panelObj = new GameObject("DialoguePanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        background = panelObj.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.75f); // Semi-transparent black
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0.28f); // Bottom 28% of screen
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.sizeDelta = Vector2.zero;

        // 3. Name Box Container
        GameObject nameBoxObj = new GameObject("NameBox");
        nameBoxObj.transform.SetParent(panelObj.transform, false);
        Image nameBg = nameBoxObj.AddComponent<Image>();
        nameBg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        RectTransform nameBoxRect = nameBoxObj.GetComponent<RectTransform>();
        nameBoxRect.anchorMin = new Vector2(0.1f, 1.0f);
        nameBoxRect.anchorMax = new Vector2(0.3f, 1.15f); // Sits slightly above the main panel
        nameBoxRect.pivot = new Vector2(0.5f, 0);
        nameBoxRect.sizeDelta = Vector2.zero;

        // 4. Speaker Text
        GameObject speakerObj = new GameObject("SpeakerText");
        speakerObj.transform.SetParent(nameBoxObj.transform, false);
        speakerText = speakerObj.AddComponent<Text>();
        speakerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        speakerText.fontSize = 45;
        speakerText.fontStyle = FontStyle.Bold;
        speakerText.alignment = TextAnchor.MiddleCenter;
        speakerText.color = Color.white;
        speakerText.supportRichText = true;
        RectTransform spkRect = speakerObj.GetComponent<RectTransform>();
        spkRect.anchorMin = Vector2.zero;
        spkRect.anchorMax = Vector2.one;
        spkRect.sizeDelta = Vector2.zero;

        // 5. Dialogue Text
        GameObject dialogObj = new GameObject("DialogueText");
        dialogObj.transform.SetParent(panelObj.transform, false);
        dialogueText = dialogObj.AddComponent<Text>();
        dialogueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        dialogueText.fontSize = 42;
        dialogueText.alignment = TextAnchor.UpperLeft;
        dialogueText.supportRichText = true;
        dialogueText.color = Color.white;
        RectTransform dlgRect = dialogObj.GetComponent<RectTransform>();
        dlgRect.anchorMin = new Vector2(0.12f, 0.25f);
        dlgRect.anchorMax = new Vector2(0.88f, 0.85f);
        dlgRect.sizeDelta = Vector2.zero;

        // 6. Continue Prompt
        GameObject continueObj = new GameObject("ContinueText");
        continueObj.transform.SetParent(panelObj.transform, false);
        continueText = continueObj.AddComponent<Text>();
        continueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        continueText.fontSize = 32;
        continueText.fontStyle = FontStyle.Bold;
        continueText.color = new Color(1f, 0.85f, 0f, 1f); // Vibrant Yellow
        continueText.alignment = TextAnchor.LowerRight;
        continueText.text = "▼ SPACE hoặc Click";
        RectTransform contRect = continueObj.GetComponent<RectTransform>();
        contRect.anchorMin = new Vector2(0.7f, 0.08f);
        contRect.anchorMax = new Vector2(0.95f, 0.22f);
        contRect.sizeDelta = Vector2.zero;

        canvasObj.SetActive(false);
    }

    public IEnumerator PlayDialogue()
    {
        if (canvasObj == null) SetupUI();

        canvasObj.SetActive(true);

        foreach (var line in storyLines)
        {
            if (line.isNarration)
            {
                speakerText.text = "";
                dialogueText.alignment = TextAnchor.MiddleCenter;
                dialogueText.color = new Color(0.8f, 0.8f, 0.8f);

                if (line.text == "Còn tiếp...")
                {
                    dialogueText.fontSize = 80;
                    dialogueText.fontStyle = FontStyle.Bold;
                    dialogueText.color = Color.white;
                }
                else
                {
                    dialogueText.fontSize = 45;
                    dialogueText.fontStyle = FontStyle.Italic;
                }
            }
            else
            {
                speakerText.text = line.speakerName;
                speakerText.color = Color.white;
                dialogueText.alignment = TextAnchor.UpperLeft;
                dialogueText.fontSize = 42;
                dialogueText.fontStyle = FontStyle.Normal;
                dialogueText.color = Color.white;
            }

            bool wasSkipped = false;
            yield return StartCoroutine(TypewriterEffect(line.text, (skipped) => wasSkipped = skipped));

            // Nếu người dùng skip typewriter bằng phím, đợi họ nhả phím ra trước
            if (wasSkipped)
            {
                while (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return))
                    yield return null;
            }

            // Thêm delay skipDelay trước khi nhận input tiếp
            yield return new WaitForSeconds(skipDelay);

            isWaitingForInput = true;
            float waitTimer = 0f;
            continueText.gameObject.SetActive(true);

            while (isWaitingForInput)
            {
                waitTimer += Time.deltaTime;

                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    isWaitingForInput = false;
                }

                if (waitTimer >= autoSkipTime)
                {
                    isWaitingForInput = false;
                }
                yield return null;
            }
        }

        // Fading out or little delay
        continueText.gameObject.SetActive(false);
        yield return new WaitForSeconds(1.0f);

        canvasObj.SetActive(false);
    }

    private IEnumerator TypewriterEffect(string fullText, System.Action<bool> onDone = null)
    {
        dialogueText.text = "";
        continueText.gameObject.SetActive(false);

        string currentText = "";
        bool inTag = false;
        float typeSpeed = 0.03f;
        bool skipped = false;

        for (int i = 0; i < fullText.Length; i++)
        {
            // Allow skip typewriter only
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                dialogueText.text = fullText;
                skipped = true;
                break;
            }

            char c = fullText[i];

            if (c == '<') inTag = true;

            currentText += c;

            if (c == '>') inTag = false;

            if (!inTag)
            {
                dialogueText.text = currentText;
                yield return new WaitForSeconds(typeSpeed);
            }
            else
            {
                dialogueText.text = currentText;
            }
        }

        onDone?.Invoke(skipped);
        continueText.gameObject.SetActive(true);
        yield return null;
    }
}
