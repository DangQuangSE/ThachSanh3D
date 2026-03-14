# ?? H??NG D?N T?O DIALOGUE BOX GI?NG GENSHIN IMPACT

## ?? MÔ T?

Dialogue box theo phong cách Genshin Impact:
- **Tên nhân v?t**: Hi?n th? ? gi?a d??i nhân v?t, màu vàng (RGB: 255, 200, 100)
- **H?p tho?i**: N?m phía d??i màn hình, có background ?en trong su?t
- **Text**: Màu tr?ng, font rõ ràng, có shadow
- **Animation**: Fade in/out m??t mà

---

## ?? C?U TRÚC UI HIERARCHY

```
Canvas (Screen Space - Overlay)
?? DialogueSystem (GameObject)
   ?? CharacterNamePanel (Panel)
   ?  ?? Background (Image) - Màu vàng gradient
   ?  ?? CharacterNameText (TextMeshPro)
   ?
   ?? DialoguePanel (Panel)
      ?? Background (Image) - ?en trong su?t có vi?n
      ?? DialogueText (TextMeshPro) - N?i dung h?i tho?i
      ?? ButtonPanel
         ?? NextButton (Button)
         ?? ConfirmPanel (hidden)
            ?? AcceptButton
            ?? DeclineButton
```

---

## ??? H??NG D?N SETUP CHI TI?T

### **B??C 1: T?o Canvas chính**

1. Hierarchy ? Right-click ? `UI > Canvas`
2. ??t tên: `DialogueCanvas`
3. Canvas Settings:
   - Render Mode: `Screen Space - Overlay`
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`

---

### **B??C 2: T?o Character Name Panel (Tên nhân v?t)**

1. Right-click Canvas ? `UI > Panel`
2. ??t tên: `CharacterNamePanel`
3. **RectTransform:**
   - Anchor: `Bottom Center` (X: 0.5, Y: 0)
   - Position: X: 0, Y: 300 (trên dialogue box)
   - Width: 300
   - Height: 50

4. **Image Component (Background):**
   - Color: `#FFC864FF` (vàng Genshin)
   - Material: UI/Default
   - Alpha: 230

5. **Thêm Outline:**
   - Add Component ? `UI > Effects > Outline`
   - Effect Color: `#402A0DFF` (nâu ??m)
   - Effect Distance: X: 2, Y: -2

---

### **B??C 3: T?o Character Name Text**

1. Right-click `CharacterNamePanel` ? `UI > Text - TextMeshPro`
2. ??t tên: `CharacterNameText`
3. **RectTransform:** Stretch Full (Fill parent)
4. **TextMeshProUGUI:**
   - Text: "Paimon"
   - Font: Arial Bold (ho?c font game)
   - Font Size: 32
   - Color: `#2D1F0FFF` (nâu ??m)
   - Alignment: Center Middle
   - Auto Size: OFF

5. **Thêm Shadow:**
   - Add Component ? `UI > Effects > Shadow`
   - Color: Black (Alpha: 100)
   - Distance: X: 1, Y: -1

---

### **B??C 4: T?o Dialogue Panel (H?p tho?i chính)**

1. Right-click Canvas ? `UI > Panel`
2. ??t tên: `DialoguePanel`
3. **RectTransform:**
   - Anchor: `Bottom Stretch`
   - Position: Y: 100
   - Height: 200

4. **Image Component (Background):**
   - Color: `#000000B4` (?en, Alpha: 180)
   - Material: UI/Default

5. **Thêm vi?n vàng:**
   - Add Component ? `UI > Effects > Outline`
   - Color: `#FFC864FF` (vàng)
   - Distance: X: 3, Y: -3

---

### **B??C 5: T?o Dialogue Text**

1. Right-click `DialoguePanel` ? `UI > Text - TextMeshPro`
2. ??t tên: `DialogueText`
3. **RectTransform:**
   - Anchor: Stretch
   - Left: 40, Right: -40
   - Top: -20, Bottom: 60

4. **TextMeshProUGUI:**
   - Text: "She'd probably help if she knew what was going on..."
   - Font Size: 28
   - Color: White
   - Alignment: Top Left
   - Wrapping: Enabled
   - Overflow: Overflow

5. **Text Shadow:**
   - Add Component ? `Shadow`
   - Color: Black (Alpha: 200)
   - Distance: X: 2, Y: -2

---

### **B??C 6: T?o Next Button**

1. Right-click `DialoguePanel` ? `UI > Button - TextMeshPro`
2. ??t tên: `NextButton`
3. **RectTransform:**
   - Anchor: Bottom Right
   - Position: X: -50, Y: 20
   - Width: 150, Height: 40

4. **Button Image:**
   - Color: `#FFC864FF` (vàng)
   - Transition: Color Tint
   - Highlighted: Lighter Yellow
   - Pressed: Darker Yellow

5. **Button Text:**
   - Text: "Ti?p T?c ?"
   - Font Size: 24
   - Color: `#2D1F0FFF` (nâu)
   - Bold: ON

---

### **B??C 7: Gán vào QuestDialogue Script**

1. Ch?n GameObject có `QuestDialogue` script
2. Trong Inspector, gán:

```
Dialogue UI - TextMeshPro:
?? Dialogue Panel: [DialoguePanel]
?? Npc Name Text TMP: [CharacterNameText]
?? Dialogue Text TMP: [DialogueText]
?? Next Button: [NextButton]
?? Next Button Text TMP: [NextButton > Text (TMP)]
```

---

## ?? MÀU S?C GENSHIN IMPACT STYLE

### **Palette chính:**
```css
/* Vàng ch? ??o */
Primary Yellow: #FFC864

/* Nâu ??m (text trên vàng) */
Dark Brown: #2D1F0F

/* ?en trong su?t (background) */
Black Semi-Transparent: #000000B4 (Alpha: 180)

/* Tr?ng (text chính) */
White: #FFFFFF

/* Outline vàng */
Gold Outline: #FFD700
```

### **Gradient cho Name Panel (Optional):**
```
Top Color: #FFE6A0
Bottom Color: #FFC864
```

---

## ?? V? TRÍ VÀ KÍCH TH??C CHU?N

### **Reference Resolution: 1920x1080**

| Element | Position (Y) | Size |
|---------|-------------|------|
| Character Name | Y: 300 | 300 x 50 |
| Dialogue Panel | Y: 100 | Full Width x 200 |
| Dialogue Text | Margin: 40px | Auto Height |
| Next Button | Bottom-Right | 150 x 40 |

---

## ?? SETTINGS NÂNG CAO

### **1. Typewriter Effect (?ã có s?n trong QuestDialogue.cs)**
```csharp
[Header("Typewriter Effect")]
public float typeSpeed = 40f; // T?c ?? ?ánh ch?
```

### **2. Fade Animation (Thêm vào QuestDialogue.cs)**

Thêm method fade in/out cho dialogue:

```csharp
[Header("Animation Settings")]
public float fadeDuration = 0.3f;

private IEnumerator FadeInDialogue()
{
    CanvasGroup canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
    if (canvasGroup == null)
        canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();

    float elapsed = 0f;
    while (elapsed < fadeDuration)
    {
        elapsed += Time.deltaTime;
        canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
        yield return null;
    }
    canvasGroup.alpha = 1f;
}
```

### **3. Character Name Animation (Bounce effect)**

```csharp
private IEnumerator BounceCharacterName()
{
    RectTransform nameRect = npcNameTextTMP.GetComponent<RectTransform>();
    Vector3 originalScale = Vector3.one;
    
    // Scale up
    float t = 0f;
    while (t < 0.2f)
    {
        t += Time.deltaTime;
        nameRect.localScale = Vector3.Lerp(originalScale, originalScale * 1.1f, t / 0.2f);
        yield return null;
    }
    
    // Scale back
    t = 0f;
    while (t < 0.2f)
    {
        t += Time.deltaTime;
        nameRect.localScale = Vector3.Lerp(originalScale * 1.1f, originalScale, t / 0.2f);
        yield return null;
    }
}
```

---

## ?? ANIMATIONS (OPTIONAL)

### **T?o Animator cho Dialogue Panel:**

1. Create Animator Controller: `DialogueAnimator.controller`
2. T?o các Animation Clips:
   - `DialogueFadeIn.anim` - Fade in t? Alpha 0 ? 1
   - `DialogueFadeOut.anim` - Fade out t? Alpha 1 ? 0
   - `NameBounce.anim` - Scale name panel

---

## ? CHECKLIST HOÀN THÀNH

- [ ] T?o Canvas v?i Screen Space - Overlay
- [ ] T?o Character Name Panel (màu vàng)
- [ ] T?o Character Name Text (TMP)
- [ ] T?o Dialogue Panel (?en trong su?t, vi?n vàng)
- [ ] T?o Dialogue Text (TMP, màu tr?ng, shadow)
- [ ] T?o Next Button (vàng, text nâu)
- [ ] Gán t?t c? vào QuestDialogue script
- [ ] Thêm Outline cho Character Name Panel
- [ ] Thêm Shadow cho text
- [ ] Test fade in/out animation
- [ ] Ki?m tra responsive trên các ?? phân gi?i

---

## ?? TROUBLESHOOTING

### **Character Name không hi?n:**
- Check CanvasGroup Alpha = 1
- Check active in hierarchy
- Check Z-position (ph?i tr??c Dialogue Panel)

### **Text b? c?t:**
- B?t Overflow mode trong TextMeshPro
- T?ng Preferred Height
- Check RectTransform margins

### **Màu không ?úng:**
- ??m b?o dùng HDR color picker
- Check Alpha channel (255 = không trong su?t)
- Material ph?i là UI/Default

### **Button không click ???c:**
- Check Raycast Target = ON trong Image
- Check EventSystem có trong scene
- Canvas có GraphicRaycaster component

---

## ?? SCREENSHOT REFERENCE

**Layout cu?i cùng:**
```
??????????????????????????????????????
?                                    ?
?         [Game Viewport]            ?
?                                    ?
?            Paimon                  ? ? Character Name (Vàng)
?  ???????????????????????????????? ?
?  ? She'd probably help if she   ? ? ? Dialogue Text (Tr?ng)
?  ? knew what was going on...    ? ?
?  ?                  [Ti?p T?c ?]? ? ? Next Button (Vàng)
?  ???????????????????????????????? ?
??????????????????????????????????????
```

---

## ?? TIPS

1. **Font:** Dùng font sans-serif nh? `Noto Sans`, `Roboto` ho?c `Arial Bold`
2. **Size:** Dialogue text nên 24-28px, Name text nên 32-36px
3. **Spacing:** Padding ít nh?t 40px cho text
4. **Shadow:** Luôn thêm shadow cho text ?? d? ??c
5. **Animation:** Fade in/out 0.3s, bounce 0.4s
6. **Responsive:** Test trên 1920x1080, 1280x720, 2560x1440

---

**Chúc b?n t?o dialogue box ??p nh? Genshin! ?**
