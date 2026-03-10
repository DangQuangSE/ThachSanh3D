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
        canvas.sortingOrder = 999; // Render on top of everything

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Background
        GameObject bgObj = new GameObject("BackgroundDialog");
        bgObj.transform.SetParent(canvasObj.transform, false);
        background = bgObj.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.95f); // Very dark screen
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // 3. Speaker Text
        GameObject speakerObj = new GameObject("SpeakerText");
        speakerObj.transform.SetParent(bgObj.transform, false);
        speakerText = speakerObj.AddComponent<Text>();
        speakerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        speakerText.fontSize = 60;
        speakerText.fontStyle = FontStyle.Bold;
        speakerText.alignment = TextAnchor.LowerLeft;
        speakerText.supportRichText = true;
        RectTransform spkRect = speakerObj.GetComponent<RectTransform>();
        spkRect.anchorMin = new Vector2(0.15f, 0.65f);
        spkRect.anchorMax = new Vector2(0.85f, 0.75f);
        spkRect.sizeDelta = Vector2.zero;

        // 4. Dialogue Text
        GameObject dialogObj = new GameObject("DialogueText");
        dialogObj.transform.SetParent(bgObj.transform, false);
        dialogueText = dialogObj.AddComponent<Text>();
        dialogueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        dialogueText.fontSize = 50;
        dialogueText.alignment = TextAnchor.UpperLeft;
        dialogueText.supportRichText = true;
        RectTransform dlgRect = dialogObj.GetComponent<RectTransform>();
        dlgRect.anchorMin = new Vector2(0.15f, 0.25f);
        dlgRect.anchorMax = new Vector2(0.85f, 0.60f);
        dlgRect.sizeDelta = Vector2.zero;

        // 5. Continue Prompt
        GameObject continueObj = new GameObject("ContinueText");
        continueObj.transform.SetParent(bgObj.transform, false);
        continueText = continueObj.AddComponent<Text>();
        continueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        continueText.fontSize = 35;
        continueText.fontStyle = FontStyle.Italic;
        continueText.color = new Color(1, 1, 1, 0.6f);
        continueText.alignment = TextAnchor.LowerRight;
        continueText.text = "Nhấn chuột hoặc phím Space để tiếp tục...";
        RectTransform contRect = continueObj.GetComponent<RectTransform>();
        contRect.anchorMin = new Vector2(0.5f, 0.05f);
        contRect.anchorMax = new Vector2(0.95f, 0.15f);
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
                speakerText.text = line.speakerName + ":";
                speakerText.color = line.speakerName == "Thạch Sanh" ? new Color(0.3f, 0.8f, 1f) : new Color(1f, 0.3f, 0.2f); // Blue for Thach Sanh, Red for Ly Thong
                dialogueText.alignment = TextAnchor.UpperLeft;
                dialogueText.fontSize = 50;
                dialogueText.fontStyle = FontStyle.Normal;
                dialogueText.color = Color.white;
            }

            yield return StartCoroutine(TypewriterEffect(line.text));

            // Wait a tiny bit so player doesn't accidentally skip immediately
            yield return new WaitForSeconds(0.15f);

            isWaitingForInput = true;
            // Clear any lingering input queue
            Input.ResetInputAxes();

            while (isWaitingForInput)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
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

    private IEnumerator TypewriterEffect(string fullText)
    {
        dialogueText.text = "";
        continueText.gameObject.SetActive(false);

        string currentText = "";
        bool inTag = false;
        float typeSpeed = 0.03f;

        for (int i = 0; i < fullText.Length; i++)
        {
            // Allow skip
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                dialogueText.text = fullText;
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

        continueText.gameObject.SetActive(true);
        yield return null;
    }
}
