using UnityEngine;

/// <summary>
/// Debug script ?? ki?m tra DayNightController có ho?t ??ng không
/// </summary>
public class DayNightDebugger : MonoBehaviour
{
    public DayNightController controller;

    private void Start()
    {
        if (controller == null)
            controller = GetComponent<DayNightController>();

        if (controller == null)
        {
            Debug.LogError("? KHÔNG TÌM TH?Y DayNightController!");
            return;
        }

        Debug.Log("=== DAY/NIGHT CONTROLLER DEBUG ===");
        
        // Ki?m tra Sun Light
        if (controller.sunLight == null)
        {
            Debug.LogWarning("?? Sun Light CH?A ???C GÁN!");
            
            // Tìm t?t c? Directional Light
            Light[] lights = FindObjectsOfType<Light>();
            Debug.Log("Tìm th?y " + lights.Length + " lights trong scene:");
            
            foreach (Light light in lights)
            {
                Debug.Log("  - " + light.name + " (" + light.type + ")");
                
                if (light.type == LightType.Directional)
                {
                    Debug.Log("    ? ?ây là Directional Light - có th? dùng!");
                }
            }
        }
        else
        {
            Debug.Log("? Sun Light: " + controller.sunLight.name);
        }

        // Ki?m tra Moon Light
        if (controller.moonLight == null)
            Debug.Log("?? Moon Light: Ch?a gán (không b?t bu?c)");
        else
            Debug.Log("? Moon Light: " + controller.moonLight.name);

        // Ki?m tra Auto Update
        if (controller.autoUpdateTime)
            Debug.Log("? Auto Update: B?T - Th?i gian s? t? ??ng ch?y");
        else
            Debug.LogWarning("?? Auto Update: T?T - C?n g?i method ?? thay ??i");

        Debug.Log("Current Time: " + controller.currentTime + "h");
        Debug.Log("Time Speed: " + controller.timeSpeed + "x");
        Debug.Log("=================================");
    }

    [ContextMenu("Test Set Day")]
    public void TestSetDay()
    {
        if (controller != null)
        {
            controller.SetDay();
            Debug.Log("? ?ã chuy?n sang BAN NGÀY");
        }
    }

    [ContextMenu("Test Set Night")]
    public void TestSetNight()
    {
        if (controller != null)
        {
            controller.SetNight();
            Debug.Log("? ?ã chuy?n sang BAN ?ÊM");
        }
    }

    [ContextMenu("Test Make Dark")]
    public void TestMakeDark()
    {
        if (controller != null)
        {
            controller.MakeDark();
            Debug.Log("? ?ã LÀM T?I HOÀN TOÀN");
        }
    }

    [ContextMenu("Check Lights Info")]
    public void CheckLightsInfo()
    {
        Debug.Log("=== LIGHTS INFO ===");
        
        if (controller != null && controller.sunLight != null)
        {
            Light sun = controller.sunLight;
            Debug.Log("Sun Light:");
            Debug.Log("  - Enabled: " + sun.enabled);
            Debug.Log("  - Intensity: " + sun.intensity);
            Debug.Log("  - Color: " + sun.color);
            Debug.Log("  - Type: " + sun.type);
        }

        Debug.Log("Render Settings:");
        Debug.Log("  - Ambient Light: " + RenderSettings.ambientLight);
        Debug.Log("  - Fog: " + RenderSettings.fog);
        Debug.Log("  - Fog Color: " + RenderSettings.fogColor);
        Debug.Log("==================");
    }
}
