# ?? FIX: Confirm Panel Không Hi?n Th?

## ? V?n ??
Sau khi h?t h?i tho?i, Confirm Panel (ch?n ??ng Ý/T? Ch?i nhi?m v?) không hi?n th?.

## ? Nguyên nhân & Gi?i pháp

### 1. **Confirm Panel b? che b?i Dialogue Panel**
**Nguyên nhân:** DialoguePanel ? phía trên ConfirmPanel trong hierarchy.

**Gi?i pháp ?ã implement:**
```csharp
// Trong OnDialogueEnd()
if (dialoguePanel != null) dialoguePanel.SetActive(false);  // T?t tr??c
if (confirmPanel != null)
{
    confirmPanel.SetActive(true);
    confirmPanel.transform.SetAsLastSibling();  // ??a lên trên cùng
}
```

### 2. **Confirm Panel ch?a ???c gán**
**Nguyên nhân:** Ch?a gán ConfirmPanel trong Inspector.

**Gi?i pháp:** Script ?ã có `AutoFindUIElements()` t? ??ng tìm:
```csharp
// T? ??ng tìm trong Start()
if (confirmPanel == null)
{
    Transform cp = dialogueSystem.transform.Find("ConfirmPanel");
    if (cp != null) confirmPanel = cp.gameObject;
}
```

### 3. **Canvas Sorting Order sai**
**Gi?i pháp ?ã implement:**
```csharp
// Trong DialogueSystemCreator
canvas.sortingOrder = 100;  // Luôn hi?n th? trên cùng
```

### 4. **CanvasGroup blocking**
**Gi?i pháp:** Thêm CanvasGroup vào ConfirmPanel:
```csharp
CanvasGroup canvasGroup = confirmPanel.AddComponent<CanvasGroup>();
canvasGroup.alpha = 1f;
canvasGroup.interactable = true;
canvasGroup.blocksRaycasts = true;
```

---

## ?? Cách Test

### B??c 1: T?o m?i DialogueSystem
```
GameObject > UI > Genshin Dialogue System
```

### B??c 2: Gán vào NPC
1. Ch?n NPC GameObject
2. Add `QuestDialogue` script
3. **KHÔNG C?N GÁN GÌ** - Script s? t? ??ng tìm!

### B??c 3: Test trong Game
1. Play game
2. ??n g?n NPC, nh?n **F**
3. Nh?n **SPACE** ?? ti?p t?c h?i tho?i
4. Sau h?i tho?i cu?i ? **Confirm Panel ph?i hi?n th?**

---

## ?? Debug Checklist

### N?u Confirm Panel v?n không hi?n:

#### ? Check 1: Console Logs
Xem trong Console có thông báo:
```
[QuestDialogue] Hi?n th? Confirm Panel cho scene: PlaygroundB
[QuestDialogue] ? Confirm Panel ?ã ???c b?t
```

N?u th?y:
```
[QuestDialogue] ? Confirm Panel NULL
```
? **Confirm Panel ch?a ???c tìm th?y!**

#### ? Check 2: Hierarchy trong Scene
M? Scene khi ?ang Play, ki?m tra:
```
Canvas
??? DialogueSystem
    ??? BackgroundOverlay
    ??? CharacterPortraitPanel
    ??? CharacterNamePanel
    ??? DialoguePanel
    ??? ConfirmPanel ? Ph?i có cái này!
        ??? Title
        ??? AcceptButton
        ??? DeclineButton
```

#### ? Check 3: ConfirmPanel Inspector
Ch?n `ConfirmPanel` trong Hierarchy, ki?m tra:
- [x] Active checkbox = **CHECKED** (khi ?ang hi?n th?)
- [x] Image component có màu (không trong su?t)
- [x] Canvas Group: `Interactable = true`, `Blocks Raycasts = true`

#### ? Check 4: Target Scene
Trong `QuestDialogue` Inspector, ki?m tra:
- `chanTinhSceneName` = "PlaygroundB" (ho?c tên scene c?a b?n)
- `daiBangSceneName` = "PlaygroundB"

**N?u ?? tr?ng ? Confirm Panel s? KHÔNG hi?n th?!**

---

## ?? Advanced Debug

### Thêm Debug Log
N?u mu?n debug chi ti?t h?n, thêm vào `QuestDialogue.cs`:

```csharp
private void OnDialogueEnd()
{
    StopLineAudio();
    
    Debug.Log($"[DEBUG] _isEndingDialogue = {_isEndingDialogue}");
    Debug.Log($"[DEBUG] _targetScene = '{_targetScene}'");
    Debug.Log($"[DEBUG] confirmPanel = {(confirmPanel != null ? "OK" : "NULL")}");
    
    // ...existing code...
}
```

### Test Manually
M? Console và gõ:
```csharp
// Trong Unity Console Window (khi ?ang Play)
GameObject.Find("DialogueSystem/ConfirmPanel").SetActive(true);
```

N?u Confirm Panel hi?n ? **V?n ?? là logic, không ph?i UI**

---

## ?? Tips

### Tip 1: Recreate UI
N?u v?n không ???c, xóa DialogueSystem c? và t?o l?i:
1. Xóa `DialogueSystem` trong Hierarchy
2. `GameObject > UI > Genshin Dialogue System`
3. Script t? ??ng assign

### Tip 2: Manual Override
N?u auto-find không ho?t ??ng, gán th? công:
1. Ch?n NPC có `QuestDialogue` script
2. Drag & Drop `ConfirmPanel` t? Hierarchy
3. Drag `AcceptButton` và `DeclineButton`

### Tip 3: Check Button Events
??m b?o buttons có s? ki?n:
```csharp
// Trong Start(), ?ã có:
acceptButton.onClick.AddListener(OnAccept);
declineButton.onClick.AddListener(OnDecline);
```

---

## ?? Checklist Hoàn Ch?nh

- [ ] DialogueSystem ?ã ???c t?o b?ng menu `GameObject > UI > Genshin Dialogue System`
- [ ] ConfirmPanel t?n t?i trong Hierarchy
- [ ] QuestDialogue script ?ã ???c add vào NPC
- [ ] `chanTinhSceneName` ho?c `daiBangSceneName` không ?? tr?ng
- [ ] Console có log: `? Confirm Panel ?ã ???c b?t`
- [ ] Khi Play, ConfirmPanel active = true sau h?i tho?i
- [ ] AcceptButton và DeclineButton có th? click ???c
- [ ] Cursor hi?n th? khi Confirm Panel m?

---

## ?? K?t qu? mong ??i

```
1. Player nh?n F ? M? dialogue
2. Dialogue hi?n th? t?ng dòng v?i typewriter
3. Nh?n SPACE ? Dòng ti?p theo
4. Dòng cu?i cùng ? Dialogue Panel t?t
5. ? ConfirmPanel hi?n ra v?i 2 button:
   - [??ng Ý] (màu xanh lá)
   - [T? Ch?i] (màu ??)
6. Click [??ng Ý] ? Load scene boss
7. Click [T? Ch?i] ? ?óng dialogue
```

---

## ?? V?n không ???c?

### Ki?m tra l?i toàn b?:
```csharp
Debug.Log("=== QUEST DIALOGUE DEBUG ===");
Debug.Log($"Dialogue Panel: {dialoguePanel != null}");
Debug.Log($"Confirm Panel: {confirmPanel != null}");
Debug.Log($"Accept Button: {acceptButton != null}");
Debug.Log($"Decline Button: {declineButton != null}");
Debug.Log($"Target Scene: {_targetScene}");
Debug.Log($"Is Ending: {_isEndingDialogue}");
Debug.Log("============================");
```

### N?u t?t c? = true nh?ng v?n không hi?n:
? Ki?m tra Camera Culling Mask
? Ki?m tra Canvas Render Mode
? Ki?m tra Screen Space Overlay

---

**?ã fix trong version này:**
- ? Auto-find UI elements
- ? SetAsLastSibling() ?? ??a lên trên
- ? Debug logs chi ti?t
- ? CanvasGroup proper setup
- ? Canvas sorting order = 100

**Tác gi?:** AI Assistant
**Ngày c?p nh?t:** 2024
