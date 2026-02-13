using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarWorld : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The boss this health bar tracks")]
    public BossController boss;
    
    [Tooltip("Health bar slider")]
    public Slider healthSlider;
    
    [Tooltip("Boss name text")]
    public Text bossNameText;
    
    [Header("Settings")]
    [Tooltip("Offset above boss")]
    public Vector3 offset = new Vector3(0, 3f, 0);
    
    [Tooltip("Always face camera")]
    public bool billboardToCamera = true;
    
    [Header("Visual")]
    [Tooltip("Fill image of health bar")]
    public Image fillImage;
    
    [Tooltip("Color gradient")]
    public Gradient healthColorGradient;
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (boss == null)
        {
            boss = GetComponentInParent<BossController>();
        }
        
        if (boss == null)
        {
            Debug.LogError("BossHealthBarWorld: No boss assigned!");
            enabled = false;
            return;
        }
        
        if (bossNameText != null)
        {
            bossNameText.text = "Boss";
        }
        
        // Setup default gradient if not set
        if (healthColorGradient == null)
        {
            healthColorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(Color.red, 0f);
            colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f);
            colorKeys[2] = new GradientColorKey(Color.green, 1f);
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);
            
            healthColorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    void Update()
    {
        if (boss == null || boss.IsDead())
        {
            gameObject.SetActive(false);
            return;
        }
        
        // Update position
        transform.position = boss.transform.position + offset;
        
        // Billboard to camera
        if (billboardToCamera && mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
        
        // Update health bar
        float healthPercent = boss.GetHealthPercentage();
        
        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }
        
        if (fillImage != null)
        {
            fillImage.color = healthColorGradient.Evaluate(healthPercent);
        }
    }
}
