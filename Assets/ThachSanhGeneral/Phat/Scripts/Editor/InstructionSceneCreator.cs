using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class InstructionSceneCreator
{
    [MenuItem("Tools/Thach Sanh 3D/Tạo Nhanh Giao Diện Hướng Dẫn")]
    public static void CreateInstructionUI()
    {
        // 1. Tìm hoặc tạo Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
                // Sử dụng Input System mới
                eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                // Sử dụng Input System cũ
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
            }
        }

        // 2. Tạo InstructionManager
        GameObject managerObj = new GameObject("InstructionManager");
        InstructionSceneManager manager = managerObj.AddComponent<InstructionSceneManager>();

        // 3. Tạo Panel nền đen mờ (Background)
        GameObject bgObj = new GameObject("BackgroundPanel");
        bgObj.transform.SetParent(canvas.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.85f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // 4. Tạo TextMeshPro để hiển thị hướng dẫn
        GameObject textObj = new GameObject("InstructionText");
        textObj.transform.SetParent(canvas.transform, false);
        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.alignment = TextAlignmentOptions.TopLeft;
        tmpText.fontSize = 32;
        tmpText.color = Color.white;
        tmpText.richText = true;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        // Căn lề giữa màn hình
        textRect.anchorMin = new Vector2(0.15f, 0.2f);
        textRect.anchorMax = new Vector2(0.85f, 0.9f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Gắn vào Manager
        manager.instructionTextDisplay = tmpText;

        // 5. Tạo nút Tiếp Tục (Continue Button)
        GameObject buttonObj = new GameObject("ContinueButton");
        buttonObj.transform.SetParent(canvas.transform, false);
        Image btnImg = buttonObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Màu xanh lá cây
        Button btn = buttonObj.AddComponent<Button>();

        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.05f);
        btnRect.anchorMax = new Vector2(0.5f, 0.05f);
        btnRect.sizeDelta = new Vector2(300, 70);
        btnRect.anchoredPosition = new Vector2(0, 100);

        // Chữ trong nút
        GameObject btnTextObj = new GameObject("Text (TMP)");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "<b>Bắt Đầu / Tiếp Tục</b>";
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        btnText.fontSize = 32;
        
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        // Liên kết sự kiện OnClick gọi tới hàm LoadNextScene của InstructionManager
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, manager.LoadNextScene);

        // Chọn Manager để bạn tiện cấu hình tên Scene tiếp theo
        Selection.activeGameObject = managerObj;

        Debug.Log("<color=green>Đã tạo thành công giao diện Instruction Scene!</color>");
    }
}
