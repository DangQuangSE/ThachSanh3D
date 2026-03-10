# ? QUICK REFERENCE - Genshin Dialogue System

## ?? 3 B??c Setup Nhanh

### 1?? T?o UI (5 giây)
```
GameObject > UI > Genshin Dialogue System
```

### 2?? Add Script Vào NPC (10 giây)
```
Ch?n NPC > Add Component > QuestDialogue
```

### 3?? Play!
```
? Không c?n gán gì - T? ??ng tìm UI
? ??n g?n NPC > Nh?n F
? Nh?n SPACE ?? ti?p t?c
```

---

## ?? Format Dialogue

### ? Có Tên (Recommended)
```csharp
"Lý Thông: Hi?n ?? ?i, ?êm nay ??n phiên ta ph?i ?i canh mi?u th?."
"Th?ch Sanh: Huynh c? ? nhà lo cho m?, vi?c canh mi?u c? ?? ?? lo."
```
? Tên t? ??ng ??i!

### ?? Không Tên
```csharp
"Hi?n ?? ?i, ?êm nay ??n phiên ta ph?i ?i canh mi?u th?."
```
? Dùng tên m?c ??nh trong `npcName`

---

## ?? Troubleshooting 1-Minute

| V?n ?? | Nguyên nhân | Fix |
|--------|-------------|-----|
| UI hi?n khi vào game | DialogueSystem active | Unchecked checkbox trong Hierarchy |
| "Nh?n F" không ?n | InteractHint active | Unchecked checkbox |
| Tên không ??i | Thi?u d?u `:` | Thêm `"Tên: ..."` |
| Coroutine error | DialogueSystem inactive khi g?i animation | ?ã fix - active tr??c khi g?i |
| Confirm Panel không hi?n | `targetScene` tr?ng | ?i?n `chanTinhSceneName` |

---

## ?? Quick Debug Commands

### Check UI Found
```csharp
// Xem Console khi Play:
[QuestDialogue] ? Auto-found DialoguePanel
[QuestDialogue] ? Auto-found ConfirmPanel
[QuestDialogue] ? Auto-found AcceptButton
```

### Force Show ConfirmPanel (Test)
```csharp
// Trong Unity Console khi Play:
GameObject.Find("DialogueSystem").SetActive(true);
GameObject.Find("DialogueSystem/ConfirmPanel").SetActive(true);
```

### Check Active State
```
Hierarchy > DialogueSystem > Inspector:
- Khi ch?a dialogue: ? (unchecked)
- Khi ?ang dialogue: ? (checked)
```

---

## ?? Must-Know Features

### Auto Character Name Parsing
```csharp
"Lý Thông: Text" ? CharacterName = "Lý Thông"
"Th?ch Sanh: Text" ? CharacterName = "Th?ch Sanh"
```

### Skip Typing
```
Nh?n SPACE khi ?ang typing ? Hi?n toàn b? ngay
```

### Multiple Choices
```
H?t dialogue ? Confirm Panel
- ??ng Ý ? Load boss scene
- T? Ch?i ? Close dialogue
```

---

## ?? Customization (Tùy ch?n)

### Colors (GenshinDialogueStyler)
```csharp
primaryYellow = #FFC864      // Thanh tên
darkBrown = #2D1F0F          // Text trên vàng
semiTransparentBlack = rgba(0,0,0,0.7)  // Background
```

### Speeds (QuestDialogue)
```csharp
typeSpeed = 40f              // Ký t?/giây
fadeDuration = 0.3f          // Fade in/out
bounceDuration = 0.4f        // Bounce animation
```

### Ranges
```csharp
interactRange = 3f           // Kho?ng cách nh?n F
```

---

## ? Checklist 30-Second

- [ ] DialogueSystem inactive khi vào game
- [ ] InteractHint inactive khi vào game
- [ ] InteractHint hi?n khi g?n NPC (< 3m)
- [ ] Dialogue m? khi nh?n F
- [ ] Character name ??i theo ng??i nói
- [ ] SPACE ti?p t?c dialogue
- [ ] Confirm Panel hi?n sau dialogue
- [ ] Buttons có th? click

---

## ?? Full Docs

Xem chi ti?t: `GENSHIN_DIALOGUE_COMPLETE_GUIDE.md`

---

**Quick Start:** 1 minute  
**Full Setup:** 3 minutes  
**Status:** ? Production Ready
