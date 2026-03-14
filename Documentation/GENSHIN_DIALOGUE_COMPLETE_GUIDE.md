# ?? GENSHIN DIALOGUE SYSTEM - H??NG D?N HOÀN CH?NH

## ? ?ã Fix T?t C? Issues

### 1. ? Màn tr?ng bên trái
**V?n ??:** CharacterPortraitPanel có màu tr?ng trong su?t gây khó ch?u
**Gi?i pháp:** ?ã XÓA CharacterPortraitPanel kh?i DialogueSystemCreator

### 2. ? UI hi?n ngay khi vào game
**V?n ??:** DialogueSystem hi?n luôn khi load scene
**Gi?i pháp:** `dialogueRoot.SetActive(false);` trong CreateDialogueSystem()

### 3. ? InteractHint không ?n
**V?n ??:** "Nh?n F" hi?n luôn
**Gi?i pháp:** `interactHint.SetActive(false);` - ch? hi?n khi g?n NPC

### 4. ? V? trí UI b? l?ch
**V?n ??:** CharacterNamePanel ? góc trái
**Gi?i pháp:** ??i anchor v? center bottom:
```csharp
rect.anchorMin = new Vector2(0.5f, 0f);
rect.anchorMax = new Vector2(0.5f, 0f);
rect.anchoredPosition = new Vector2(0, 310); // Phía trên DialoguePanel
```

### 5. ? Character Name không ??i theo ng??i nói
**V?n ??:** Luôn hi?n "Lý Thông"
**Gi?i pháp:** Parse tên t? dialogue format `"Tên: N?i dung"`
```csharp
// Trong ShowLine()
if (_fullCurrentLine.Contains(":"))
{
    string speakerName = _fullCurrentLine.Substring(0, colonIndex).Trim();
    SetText(npcNameText, npcNameTextTMP, speakerName);
}
```

### 6. ? Coroutine error khi DialogueSystem inactive
**V?n ??:** 
```
Coroutine couldn't be started because the game object 'DialogueSystem' is inactive!
```
**Gi?i pháp:** 
- Active DialogueSystem TR??C khi g?i styler
- Thêm check `gameObject.activeInHierarchy` trong GenshinDialogueStyler

### 7. ? Confirm Panel không hi?n
**V?n ??:** Sau h?i tho?i không có panel ch?n nhi?m v?
**Gi?i pháp:** 
- T?t DialoguePanel tr??c
- Dùng `SetAsLastSibling()` ?? ??a ConfirmPanel lên trên
- Thêm debug logs

---

## ?? C?u Trúc UI M?i

```
Canvas
??? DialogueSystem (INACTIVE khi vào game)
?   ??? BackgroundOverlay
?   ??? CharacterNamePanel (Centered, trên DialoguePanel)
?   ?   ??? CharacterNameText (TMP)
?   ??? DialoguePanel
?   ?   ??? DialogueText (TMP)
?   ?   ??? ContinueHint (INACTIVE ban ??u)
?   ?       ??? ContinueHintText (TMP)
?   ??? ConfirmPanel (INACTIVE ban ??u)
?       ??? Title
?       ??? AcceptButton
?       ??? DeclineButton
??? InteractHint (INACTIVE ban ??u, ? ngoài DialogueSystem)
    ??? Text
```

---

## ?? Format Dialogue ?? Auto-Parse Tên

### ? ?úng - T? ??ng parse tên
```csharp
private readonly string[] _chanTinhLines =
{
    "Lý Thông: Hi?n ?? ?i, ?êm nay ??n phiên ta ph?i ?i canh mi?u th?.",
    "Lý Thông: Kh?n n?i ta ?ang d? m? r??u, m? già l?i ?ang ?au y?u.",
    "Th?ch Sanh: Huynh c? ? nhà lo cho m?, vi?c canh mi?u c? ?? ?? lo.",
};
```

**K?t qu?:**
- CharacterNameText hi?n: "Lý Thông" ho?c "Th?ch Sanh"
- DialogueText hi?n: N?i dung sau d?u `:`

### ? Sai - Không parse ???c
```csharp
"Hi?n ?? ?i, ?êm nay..." // Không có d?u :
```
? S? hi?n tên m?c ??nh trong `npcName` field

---

## ?? Cách S? D?ng

### B??c 1: T?o UI
```
GameObject > UI > Genshin Dialogue System
```

? T? ??ng t?o:
- DialogueSystem (inactive)
- InteractHint (inactive)
- T?t c? UI elements

### B??c 2: Add Script Vào NPC
1. Ch?n NPC GameObject
2. Add Component > `QuestDialogue`
3. **KHÔNG C?N GÁN GÌ** - Script t? ??ng tìm UI!

### B??c 3: C?u Hình (Tùy ch?n)
N?u mu?n thay ??i:
- `interactRange` = 3f (kho?ng cách nh?n F)
- `typeSpeed` = 40f (t?c ?? gõ ch?)
- `npcName` = "Lý Thông" (tên m?c ??nh)
- `chanTinhSceneName` = "PlaygroundB" (tên scene boss)

### B??c 4: Play!
1. Ch?y game
2. ??n g?n NPC ? Hi?n "Nh?n F"
3. Nh?n F ? M? dialogue
4. Nh?n SPACE ho?c Click ? Ti?p t?c
5. H?t dialogue ? Confirm Panel
6. Click ??ng Ý ? Load scene boss

---

## ?? Styling (T? ??ng)

### Màu Genshin Impact
- **Vàng chính:** `#FFC864` (thanh tên)
- **Nâu ??m:** `#2D1F0F` (text trên vàng)
- **?en trong su?t:** `rgba(5, 5, 10, 0.85)` (dialogue box)
- **Vàng outline:** `#FFD700` (vi?n)

### Animations
- ? Fade In/Out (0.3s)
- ? Bounce Character Name (0.4s)
- ? Blink Continue Hint (1.5s speed)

---

## ?? Debug Tips

### Check 1: DialogueSystem Inactive Khi Vào Game
```csharp
// Trong Hierarchy khi ch?a b?t ??u dialogue:
DialogueSystem ? Checkbox ph?i UNCHECKED
InteractHint ? Checkbox ph?i UNCHECKED
```

### Check 2: InteractHint Hi?n Khi G?n NPC
```csharp
// Trong Update() c?a QuestDialogue:
float dist = Vector3.Distance(transform.position, _player.position);
// N?u dist <= 3f ? InteractHint.SetActive(true)
```

### Check 3: Character Name Thay ??i
```csharp
// Xem Console khi dialogue ch?y:
// Ph?i th?y CharacterNameText update theo t?ng dòng
```

### Check 4: Coroutine Không L?i
```csharp
// KHÔNG ???c th?y:
"Coroutine couldn't be started because the game object 'DialogueSystem' is inactive!"

// Ph?i th?y:
[QuestDialogue] ? DialogueSystem activated
```

---

## ?? Common Issues

### Issue 1: UI không tìm th?y
**Tri?u ch?ng:**
```
[QuestDialogue] ? DialogueSystem not found!
```

**Gi?i pháp:**
1. Ki?m tra Hierarchy có `DialogueSystem` không
2. Xem Console có `? Auto-found DialoguePanel`
3. N?u không ? Gán th? công trong Inspector

### Issue 2: Dialogue không ?óng
**Tri?u ch?ng:** Nh?n SPACE mãi không chuy?n dòng

**Gi?i pháp:**
1. Ki?m tra `_canContinue = true` sau khi gõ xong
2. Xem `continueHint` có hi?n không
3. Debug: `Debug.Log($"Can Continue: {_canContinue}");`

### Issue 3: Confirm Panel không hi?n
**Tri?u ch?ng:** H?t dialogue nh?ng không có panel ch?n

**Gi?i pháp:**
1. Ki?m tra `chanTinhSceneName` có ?i?n không
2. Xem Console: `[QuestDialogue] Hi?n th? Confirm Panel`
3. Ki?m tra ConfirmPanel active = true trong Hierarchy

### Issue 4: Character Name không ??i
**Tri?u ch?ng:** Luôn hi?n tên m?c ??nh

**Gi?i pháp:**
1. ??m b?o dialogue có format `"Tên: N?i dung"`
2. D?u `:` ph?i có
3. Tên không quá dài (< 20 ký t?)

---

## ?? Checklist Hoàn Ch?nh

### UI Setup
- [ ] DialogueSystem created (inactive)
- [ ] InteractHint created (inactive, outside DialogueSystem)
- [ ] CharacterNamePanel centered bottom
- [ ] DialoguePanel full width bottom
- [ ] ConfirmPanel center screen
- [ ] All TMP_Text components assigned

### Script Setup
- [ ] QuestDialogue added to NPC
- [ ] Auto-find logs in Console
- [ ] `npcName` filled
- [ ] `chanTinhSceneName` filled
- [ ] Player tagged "Player"

### Functionality
- [ ] UI hidden when game starts
- [ ] InteractHint shows when near NPC
- [ ] Dialogue opens on F press
- [ ] Typewriter effect works
- [ ] Character name changes per line
- [ ] Continue hint blinks
- [ ] Confirm panel shows after dialogue
- [ ] Accept button loads scene
- [ ] Decline button closes dialogue

### Animations
- [ ] No coroutine errors in Console
- [ ] Fade in smooth
- [ ] Character name bounces
- [ ] Continue hint blinks
- [ ] Fade out on close

---

## ?? Pro Tips

### Tip 1: Custom Character Names
Thêm tên vào ??u dòng dialogue:
```csharp
"Tên Nhân V?t: N?i dung h?i tho?i..."
```

### Tip 2: Narrative Text
N?u mu?n text không có tên (VD: ghi chú):
```csharp
"(Nói th?m) Th? là cái m?ng què c?a mình ?ã có ng??i th?!"
```
? S? hi?n tên m?c ??nh + toàn b? text

### Tip 3: Multiple Speakers
```csharp
private readonly string[] _lines =
{
    "Lý Thông: Hi?n ?? ?i...",
    "Th?ch Sanh: Huynh c? ? nhà lo cho m?...",
    "Lý Thông: ?? th?t t?t b?ng!",
};
```
? Tên t? ??ng ??i theo t?ng dòng!

### Tip 4: Skip Typing
Player nh?n SPACE khi ?ang typing ? Skip và hi?n toàn b? ngay l?p t?c

### Tip 5: Audio Support
Gán AudioClip vào:
- `chanTinhBackgroundMusic` - nh?c n?n
- `chanTinhLineAudios[]` - voice cho t?ng dòng
- `typewriterSound` - âm thanh gõ ch?

---

## ?? Flow Diagram

```
[Game Start]
    ?
[DialogueSystem INACTIVE]
[InteractHint INACTIVE]
    ?
[Player ??n g?n NPC]
    ?
[InteractHint ACTIVE] ? "Nh?n F"
    ?
[Player nh?n F]
    ?
[DialogueSystem ACTIVE]
[InteractHint INACTIVE]
    ?
[FadeIn + Bounce Animation]
    ?
[Show Line 0]
    ?
[Typewriter Effect]
    ?
[ContinueHint ACTIVE] ? "? SPACE"
    ?
[Player nh?n SPACE]
    ?
[Show Line 1]
    ?
[Update Character Name] ? Parse t? "Tên:"
    ?
...
    ?
[Last Line]
    ?
[DialoguePanel INACTIVE]
[ConfirmPanel ACTIVE]
    ?
[Player click ??ng Ý]
    ?
[Load Boss Scene]
```

---

## ?? Files Created/Modified

### Created
- `Assets/Editor/DialogueSystemCreator.cs`
- `Assets/ThachSanhGeneral/Bach/scripts/GenshinDialogueStyler.cs`
- `FIX_CONFIRM_PANEL_NOT_SHOWING.md`
- `GENSHIN_DIALOGUE_COMPLETE_GUIDE.md` (this file)

### Modified
- `Assets/ThachSanhGeneral/Bach/scripts/QuestDialogue.cs`
  - ? Added `AutoFindUIElements()`
  - ? Added character name parsing
  - ? Fixed DialogueSystem activation order
  - ? Fixed Confirm Panel showing
  - ? Added debug logs

---

## ?? Still Having Issues?

### Get Full Logs
Thêm vào `Start()` c?a QuestDialogue:
```csharp
Debug.Log("=== QUEST DIALOGUE DEBUG ===");
Debug.Log($"Dialogue System: {(GameObject.Find("DialogueSystem") != null)}");
Debug.Log($"Dialogue Panel: {dialoguePanel != null}");
Debug.Log($"Confirm Panel: {confirmPanel != null}");
Debug.Log($"Accept Button: {acceptButton != null}");
Debug.Log($"Decline Button: {declineButton != null}");
Debug.Log($"CharacterNameText: {npcNameTextTMP != null}");
Debug.Log($"DialogueText: {dialogueTextTMP != null}");
Debug.Log($"ContinueHint: {continueHint != null}");
Debug.Log($"InteractHint: {interactHint != null}");
Debug.Log($"GenshinStyler: {genshinStyler != null}");
Debug.Log("============================");
```

### Manual Override
N?u auto-find không ho?t ??ng:
1. Trong Inspector c?a NPC
2. Drag các UI elements t? Hierarchy
3. Gán th? công

### Recreate Everything
N?u v?n không ???c:
1. Xóa `DialogueSystem` trong Hierarchy
2. `GameObject > UI > Genshin Dialogue System`
3. Xóa `QuestDialogue` component
4. Add l?i `QuestDialogue`
5. Play

---

**Tác gi?:** AI Assistant  
**Version:** 2.0 - Complete Fix  
**Last Updated:** 2024  
**Status:** ? All Issues Fixed
