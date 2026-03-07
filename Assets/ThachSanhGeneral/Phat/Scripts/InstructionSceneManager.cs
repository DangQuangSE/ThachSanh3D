using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class InstructionData
{
    public int step;
    public string actionName;
    public string keyBinding;
    public string description;
}

public class InstructionSceneManager : MonoBehaviour
{
    [Header("Scene Navigation")]
    [Tooltip("Tên scene chính của game hoặc scene tiếp theo")]
    public string nextSceneName = "GameScene"; 
    [Tooltip("Tên scene Menu chính để quay lại")]
    public string menuSceneName = "MainMenu";

    [Header("UI Elements (Tùy chọn)")]
    [Tooltip("Text để hiển thị toàn bộ bảng hướng dẫn (nếu dùng 1 Text chung)")]
    public TextMeshProUGUI instructionTextDisplay;

    [Header("Data")]
    public List<InstructionData> instructions = new List<InstructionData>()
    {
        new InstructionData { step = 1, actionName = "Di chuyển", keyBinding = "W A S D", description = "Đi lại tự do" },
        new InstructionData { step = 2, actionName = "Chạy nhanh", keyBinding = "Left Shift (giữ) + WASD", description = "Chạy sprint" },
        new InstructionData { step = 3, actionName = "Nhảy", keyBinding = "Space", description = "Nhảy lên" },
        new InstructionData { step = 4, actionName = "Tấn công", keyBinding = "Chuột Trái", description = "Tấn công" },
        new InstructionData { step = 5, actionName = "Kỹ năng E", keyBinding = "E", description = "Chém xoay 360°" },
        new InstructionData { step = 6, actionName = "Phòng thủ", keyBinding = "Q", description = "Giơ rìu đỡ (bất tử)" },
        new InstructionData { step = 7, actionName = "Lăn tránh", keyBinding = "Chuột Phải", description = "Lăn né đòn" },
        new InstructionData { step = 8, actionName = "Chiêu tuyệt", keyBinding = "R", description = "Ultimate attack" },
        new InstructionData { step = 9, actionName = "Chiêu tuyệt", keyBinding = "R", description = "Ultimate attack" }
    };

    void Start()
    {
        // Tùy chọn: Tự động điền dữ liệu vào 1 TextMeshProUGUI khi bắt đầu Scene
        // Để sử dụng, bạn chỉ cần gán 1 UI Text (TMP) vào biến instructionTextDisplay trong Inspector.
        if (instructionTextDisplay != null)
        {
            GenerateTextTable();
        }
    }

    private void GenerateTextTable()
    {
        // Điều chỉnh lại vị trí pos: Bước(0%), Hành động(8%), Phím(38%), Mô tả(75%)
        // Đẩy cột Phím thụt qua trái 1 chút và kéo cột Mô tả lùi về bên phải để mở rộng diện tích tối đa cho Phím (37% màn hình)
        string tableContent = "<size=110%><b><color=#A0E2FF>Bước</color><pos=8%><color=#A0E2FF>Hành động</color><pos=38%><color=#A0E2FF>Phím</color><pos=75%><color=#A0E2FF>Mô tả</color></b></size>\n";
        
        // Thêm dòng ngăn cách có màu mờ
        tableContent += "<color=#FFFFFF50>----------------------------------------------------------------------------------------------------------------------------</color>\n";

        foreach (var item in instructions)
        {
            tableContent += $"<color=#FFD700>{item.step}</color>" +
                            $"<pos=8%>{item.actionName}" +
                            $"<pos=38%><color=#FF8C00><b>{item.keyBinding}</b></color>" +
                            $"<pos=75%><indent=75%><color=#B0C4DE><i>{item.description}</i></color></indent>\n\n"; // \n\n để cách dòng dễ nhìn
        }

        instructionTextDisplay.text = tableContent;
    }

    /// <summary>
    /// Gắn hàm này vào sự kiện OnClick của nút "Chơi" hoặc "Tiếp tục"
    /// </summary>
    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    /// <summary>
    /// Gắn hàm này vào sự kiện OnClick của nút "Quay lại"
    /// </summary>
    public void LoadMenuScene()
    {
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
