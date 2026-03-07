# ?? H??NG D?N S? D?NG H? TH?NG NGÀY/?ÊM VÀ ?? T?I

## ?? T?NG QUAN

Có 3 scripts chính:
1. **DayNightController.cs** - H? th?ng ngày/?êm ??y ?? v?i chu k? th?i gian
2. **SimpleDarknessController.cs** - ?i?u khi?n ?? t?i ??n gi?n
3. **DayNightUIController.cs** - UI ?? ?i?u khi?n t? button/slider

---

## ?? CÁCH S? D?NG NHANH

### **Option 1: Ch? c?n làm t?i tr?i (??N GI?N NH?T)**

1. T?o GameObject m?i: `Create Empty GameObject` ? ??t tên "DarknessController"
2. Add Component: `SimpleDarknessController`
3. Trong Inspector:
   - `Main Light`: ?? tr?ng (s? t? ??ng tìm Directional Light)
   - `Darkness Level`: 0.1 (0 = t?i nh?t, 1 = sáng nh?t)

#### G?i t? code:
```csharp
// Tìm controller
SimpleDarknessController darkness = FindObjectOfType<SimpleDarknessController>();

// Làm t?i hoàn toàn
darkness.MakePitchDark();

// Làm t?i v?a ph?i (nh? ban ?êm)
darkness.MakeNight();

// Làm sáng l?i
darkness.MakeBright();

// Tùy ch?nh m?c ?? t?i (0-1)
darkness.SetDarkness(0.3f);
```

#### G?i t? Button:
1. T?o Button trong Canvas
2. Trong Inspector c?a Button ? `On Click()`
3. Click `+` ? Kéo GameObject "DarknessController" vào
4. Ch?n function: `SimpleDarknessController > MakePitchDark()`

---

### **Option 2: H? th?ng ngày/?êm ??y ??**

1. T?o GameObject: "DayNightSystem"
2. Add Component: `DayNightController`
3. Trong Inspector:
   - `Sun Light`: Kéo Directional Light vào (ho?c ?? tr?ng t? tìm)
   - `Current Time`: 12 (gi? hi?n t?i 0-24)
   - `Auto Update Time`: ?? (n?u mu?n t? ??ng ch?y)
   - `Time Speed`: 1 (t?c ?? trôi th?i gian)

#### G?i t? code:
```csharp
DayNightController dayNight = FindObjectOfType<DayNightController>();

// Chuy?n sang ban ngày (12h)
dayNight.SetDay();

// Chuy?n sang ban ?êm (0h)
dayNight.SetNight();

// Chuy?n sang bình minh
dayNight.SetSunrise();

// Chuy?n sang hoàng hôn
dayNight.SetSunset();

// ??t th?i gian c? th? (0-24)
dayNight.SetTime(18f); // 6 gi? chi?u

// Chuy?n ??i m??t mà
dayNight.TransitionToTime(0f); // Chuy?n t? t? v? 0h

// Làm t?i hoàn toàn
dayNight.MakeDark();
```

---

### **Option 3: ?i?u khi?n t? UI (BUTTON + SLIDER)**

1. T?o GameObject: "DayNightUI"
2. Add Component: `DayNightUIController`
3. T?o các Button trong Canvas:
   - Button "Ngày" ? Gán vào `Day Button`
   - Button "?êm" ? Gán vào `Night Button`
   - Button "T?i" ? Gán vào `Dark Button`
   - Button "Sáng" ? Gán vào `Bright Button`

4. T?o Slider (tùy ch?n):
   - Slider ?i?u ch?nh th?i gian ? Gán vào `Time Slider`
   - Slider ?i?u ch?nh ?? t?i ? Gán vào `Darkness Slider`

5. Trong Inspector c?a `DayNightUIController`:
   - `Day Night Controller`: Kéo GameObject có script DayNightController vào
   - `Darkness Controller`: Kéo GameObject có script SimpleDarknessController vào
   - Gán các Button và Slider

---

## ?? S? D?NG V?I QUESTDIALOGUE

N?u mu?n làm t?i tr?i khi vào hang ??ng ho?c khi boss xu?t hi?n:

### **Cách 1: Thêm vào QuestDialogue.cs**

```csharp
private void OnAccept()
{
    // ...existing code...

    // Làm t?i tr?i tr??c khi chuy?n scene
    SimpleDarknessController darkness = FindObjectOfType<SimpleDarknessController>();
    if (darkness != null)
    {
        darkness.MakeNight();
    }

    if (!string.IsNullOrEmpty(_targetScene))
    {
        StartCoroutine(LoadScene(_targetScene));
    }
}
```

### **Cách 2: Trigger khi vào khu v?c**

T?o script m?i `DarknessZone.cs`:

```csharp
using UnityEngine;

public class DarknessZone : MonoBehaviour
{
    public float darknessLevel = 0.1f;
    public bool makeItDark = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SimpleDarknessController darkness = FindObjectOfType<SimpleDarknessController>();
            if (darkness != null)
            {
                if (makeItDark)
                    darkness.SetDarkness(darknessLevel);
                else
                    darkness.MakeBright();
            }
        }
    }
}
```

Gán script này vào m?t GameObject có BoxCollider (Is Trigger = ??) ? c?a hang ??ng.

---

## ?? TÙY CH?NH MÀU S?C VÀ HI?U ?NG

### **SimpleDarknessController**
```
Darkness Settings:
?? Darkness Level: 0.1 (0 = t?i nh?t, 1 = sáng nh?t)
?? Dark Light Color: (0.2, 0.2, 0.3) - Màu ánh sáng khi t?i
?? Dark Sky Color: (0.05, 0.05, 0.1) - Màu tr?i khi t?i
?? Dark Fog Color: (0.1, 0.1, 0.15) - Màu s??ng mù
?? Dark Fog Density: 0.02 - ?? ??m s??ng mù
```

### **DayNightController**
```
Day Settings:
?? Day Light Color: (1, 0.95, 0.84) - Màu sáng ban ngày
?? Day Intensity: 1.0 - ?? sáng ban ngày
?? Day Sky Color: (0.5, 0.7, 1) - Màu tr?i ban ngày

Night Settings:
?? Night Light Color: (0.3, 0.4, 0.6) - Màu sáng ban ?êm
?? Night Intensity: 0.1 - ?? sáng ban ?êm
?? Night Sky Color: (0.05, 0.05, 0.15) - Màu tr?i ban ?êm

Sunset/Sunrise:
?? Sunset Light Color: (1, 0.6, 0.3) - Màu hoàng hôn
?? Sunrise Start: 5 gi?
?? Sunrise End: 7 gi?
?? Sunset Start: 18 gi?
?? Sunset End: 20 gi?
```

---

## ?? X? LÝ L?I

### **Không tìm th?y Directional Light**
```
? Gi?i pháp:
1. T?o Directional Light m?i: GameObject > Light > Directional Light
2. Gán vào field "Sun Light" ho?c "Main Light"
```

### **Tr?i không ??i màu**
```
? Ki?m tra:
1. Skybox Material có ?ang dùng không?
   - Window > Rendering > Lighting Settings
   - Th? t?t Skybox ?? dùng Ambient Color
2. ??m b?o script ?ã ???c g?i (check Console logs)
```

### **Fog không hi?n th?**
```
? Gi?i pháp:
1. Window > Rendering > Lighting Settings
2. B?t "Fog" trong Environment
3. Ho?c ?? script t? b?t: useFog = true
```

---

## ?? VÍ D? TÍCH H?P VÀO GAME

### **K?ch b?n: Vào hang ??ng Ch?n Tinh**

```csharp
// Trong BossController.cs ho?c QuestDialogue.cs

private void EnterBossCave()
{
    // Làm t?i tr?i khi vào hang
    SimpleDarknessController darkness = FindObjectOfType<SimpleDarknessController>();
    if (darkness != null)
    {
        darkness.MakeNight(); // Làm t?i v?a ph?i
        // Ho?c darkness.SetDarkness(0.2f); // Tùy ch?nh m?c ??
    }
    
    Debug.Log("?ã vào hang ??ng, tr?i t?i l?i...");
}

private void ExitBossCave()
{
    // Làm sáng l?i khi ra kh?i hang
    SimpleDarknessController darkness = FindObjectOfType<SimpleDarknessController>();
    if (darkness != null)
    {
        darkness.MakeBright();
    }
    
    Debug.Log("Ra kh?i hang ??ng, tr?i sáng l?i...");
}
```

---

## ? TIPS & TRICKS

1. **Làm t?i t? t?**: Dùng Coroutine
```csharp
IEnumerator FadeToNight(float duration)
{
    SimpleDarknessController darkness = FindObjectOfType<SimpleDarknessController>();
    float elapsed = 0f;
    
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        darkness.SetDarkness(Mathf.Lerp(1f, 0.1f, t));
        yield return null;
    }
}
```

2. **Thay ??i theo v? trí**: 
   - Dùng Trigger Zones ? các khu v?c khác nhau
   - M?i zone có m?c ?? t?i riêng

3. **K?t h?p v?i Post Processing**:
   - Thêm Bloom effect cho ban ngày
   - Thêm Vignette cho ban ?êm

---

## ?? CHECKLIST SETUP

- [ ] T?o GameObject ch?a script
- [ ] Gán Directional Light (ho?c ?? t? tìm)
- [ ] Ch?nh màu s?c và c??ng ?? ánh sáng
- [ ] Test b?ng button ho?c code
- [ ] Tích h?p vào QuestDialogue/BossController
- [ ] Ki?m tra fog và ambient light
- [ ] L?u scene

---

**Chúc b?n thành công! ???**
