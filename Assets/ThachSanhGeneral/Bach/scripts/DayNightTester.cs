using UnityEngine;

/// <summary>
/// Script test ?? ?i?u khi?n DayNightController
/// Gán vào GameObject b?t k? và nh?n phím ?? test
/// </summary>
public class DayNightTester : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Kéo DayNightController vào ?ây")]
    public DayNightController dayNightController;

    [Header("Test Keys")]
    public KeyCode dayKey = KeyCode.Alpha1;      // Phím 1 = Ngày
    public KeyCode nightKey = KeyCode.Alpha2;    // Phím 2 = ?êm
    public KeyCode sunriseKey = KeyCode.Alpha3;  // Phím 3 = Bình minh
    public KeyCode sunsetKey = KeyCode.Alpha4;   // Phím 4 = Hoàng hôn
    public KeyCode darkKey = KeyCode.Alpha0;     // Phím 0 = T?i hoàn toàn

    private void Start()
    {
        // T? ??ng tìm controller n?u ch?a gán
        if (dayNightController == null)
        {
            dayNightController = FindObjectOfType<DayNightController>();
            
            if (dayNightController != null)
                Debug.Log("[DayNightTester] ?ã tìm th?y DayNightController");
            else
                Debug.LogError("[DayNightTester] Không tìm th?y DayNightController!");
        }

        Debug.Log("=== DAY/NIGHT TESTER ===");
        Debug.Log("Nh?n 1: Ban Ngày");
        Debug.Log("Nh?n 2: Ban ?êm");
        Debug.Log("Nh?n 3: Bình Minh");
        Debug.Log("Nh?n 4: Hoàng Hôn");
        Debug.Log("Nh?n 0: T?i Hoàn Toàn");
    }

    private void Update()
    {
        if (dayNightController == null) return;

        // Test phím
        if (Input.GetKeyDown(dayKey))
        {
            dayNightController.SetDay();
            Debug.Log("[Test] Chuy?n sang BAN NGÀY");
        }

        if (Input.GetKeyDown(nightKey))
        {
            dayNightController.SetNight();
            Debug.Log("[Test] Chuy?n sang BAN ?ÊM");
        }

        if (Input.GetKeyDown(sunriseKey))
        {
            dayNightController.SetSunrise();
            Debug.Log("[Test] Chuy?n sang BÌNH MINH");
        }

        if (Input.GetKeyDown(sunsetKey))
        {
            dayNightController.SetSunset();
            Debug.Log("[Test] Chuy?n sang HOÀNG HÔN");
        }

        if (Input.GetKeyDown(darkKey))
        {
            dayNightController.MakeDark();
            Debug.Log("[Test] LÀM T?I HOÀN TOÀN");
        }
    }

    private void OnGUI()
    {
        // Hi?n th? h??ng d?n trên màn hình
        GUI.Box(new Rect(10, 10, 200, 120), "Day/Night Tester");
        GUI.Label(new Rect(20, 35, 180, 20), "1: Ban Ngày");
        GUI.Label(new Rect(20, 55, 180, 20), "2: Ban ?êm");
        GUI.Label(new Rect(20, 75, 180, 20), "3: Bình Minh");
        GUI.Label(new Rect(20, 95, 180, 20), "4: Hoàng Hôn");
        GUI.Label(new Rect(20, 115, 180, 20), "0: T?i Hoàn Toàn");

        if (dayNightController != null)
        {
            GUI.Box(new Rect(10, 140, 200, 40), "");
            GUI.Label(new Rect(20, 145, 180, 20), "Th?i gian: " + dayNightController.currentTime.ToString("F1") + "h");
        }
    }
}
