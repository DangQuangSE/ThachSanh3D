# ?? QUEST DIALOGUE SYSTEM - H??NG D?N THI?T L?P CHI TI?T

## ?? M?C L?C
1. [T?ng quan h? th?ng](#t?ng-quan-h?-th?ng)
2. [Thi?t l?p NPC Quest Giver](#thi?t-l?p-npc-quest-giver)
3. [T?o UI Dialogue System](#t?o-ui-dialogue-system)
4. [C?u hình Boss Controller](#c?u-hình-boss-controller)
5. [Thêm Scene vào Build Settings](#thêm-scene-vào-build-settings)
6. [Testing & Debugging](#testing--debugging)
7. [Tu? ch?nh nâng cao](#tu?-ch?nh-nâng-cao)

---

## ?? T?NG QUAN H? TH?NG

### Lu?ng ho?t ??ng
```
Player ??n g?n NPC (trong vòng 3m)
       ?
Hi?n hint "[F] Nói chuy?n"
       ? nh?n F
Ki?m tra ti?n trình PlayerPrefs:
?? Ch?a gi?t boss nào       ? Quest Ch?n Tinh
?? ?ã gi?t Ch?n Tinh        ? Quest ??i Bàng Tinh
?? ?ã gi?t c? 2             ? H?i tho?i k?t thúc (không confirm)
       ?
Hi?n th? dialogue t?ng dòng (typewriter effect)
       ?
Nh?n "Ti?p t?c" ?? ??c dòng ti?p theo
       ?
Khi h?t tho?i ? Hi?n panel ??ng Ý / T? Ch?i
       ?
?? ??ng Ý  ? Chuy?n scene boss
?? T? Ch?i ? ?óng dialogue, có th? nói l?i b?t c? lúc nào
```

### Ti?n trình l?u tr?
| Key PlayerPrefs | Giá tr? | Ý ngh?a |
|---|---|---|
| `BossChanTinhDead` | `0` / `1` | Ch?n Tinh ch?a ch?t / ?ã ch?t |
| `BossDaiBangDead` | `0` / `1` | ??i Bàng Tinh ch?a ch?t / ?ã ch?t |

> ?? **L?u ý:** Khi nh?n Play t? Main Menu ? t? ??ng reset ti?n trình v? `0`.

---

## ?? THI?T L?P NPC QUEST GIVER

### B??c 1: T?o GameObject NPC

```
Hierarchy ? Right Click ? Create Empty
?? ??t tên: "QuestNPC_TienOng"
```

### B??c 2: Thêm Model NPC (tu? ch?n)

N?u có model 3D:
```
Kéo model NPC vào làm con c?a QuestNPC_TienOng:

QuestNPC_TienOng
?? TienOngModel (FBX)
   ?? Animator (n?u có ho?t ?nh idle)
```

### B??c 3: G?n Collider (cho phát hi?n kho?ng cách)

```
Select QuestNPC_TienOng
Add Component ? Sphere Collider (ho?c Box Collider)
?? Is Trigger: ? (checked)
?? Radius: 0.5 (tu? ch?nh theo model)
```

### B??c 4: G?n Script QuestDialogue

```
Select QuestNPC_TienOng
Add Component ? QuestDialogue
```

### B??c 5: C?u hình Inspector

#### **Scene Settings**
| Field | Giá tr? m?u |
|---|---|
| Chan Tinh Scene Name | `PlaygroundB` |
| Dai Bang Scene Name | `PlaygroundB` *(thay b?ng scene boss 2 n?u có)* |

#### **Interaction**
| Field | Giá tr? m?u |
|---|---|
| Interact Range | `3` |
| Interact Key | `F` |
| Player Tag | `Player` |
| Interact Hint | *(kéo UI hint vào ?ây, t?o ? B??c 3)* |

#### **Dialogue UI**
| Field | Giá tr? m?u |
|---|---|
| Dialogue Panel | *(kéo DialoguePanel)* |
| Npc Name Text | *(kéo NPCNameText)* |
| Dialogue Text | *(kéo DialogueText)* |
| Next Button | *(kéo NextButton)* |
| Next Button Text | *(kéo Text con c?a NextButton)* |
| Confirm Panel | *(kéo ConfirmPanel)* |
| Accept Button | *(kéo AcceptButton)* |
| Decline Button | *(kéo DeclineButton)* |

#### **Typewriter Effect**
| Field | Giá tr? m?u |
|---|---|
| Type Speed | `40` *(40 ký t?/giây)* |

#### **NPC Info**
| Field | Giá tr? m?u |
|---|---|
| Npc Name | `Tiên Ông` |

---

## ??? T?O UI DIALOGUE SYSTEM

### C?u trúc Hierarchy hoàn ch?nh

```
Canvas (Screen Space - Overlay)
??? DialoguePanel                    [Panel - Anchor: Full Screen]
?   ??? Background                   [Image - Color: Black ?=180]
?   ??? DialogueBox                  [Image - Anchor: Center]
?   ?   ??? NPCNameBG                [Image - Anchor: Top]
?   ?   ?   ??? NPCNameText          [Text - Font: 24, Bold]
?   ?   ??? DialogueTextBG           [Image]
?   ?   ?   ??? DialogueText         [Text - Font: 18]
?   ?   ??? NextButton               [Button - Anchor: Bottom-Right]
?   ?       ??? Text                 [Text: "Ti?p t?c"]
?   ?? (?n m?c ??nh)
?
??? ConfirmPanel                     [Panel - Anchor: Center]
?   ??? Background                   [Image - Color: Black ?=200]
?   ??? ConfirmBox                   [Image]
?   ?   ??? QuestionText             [Text: "B?n có mu?n nh?n nhi?m v??"]
?   ?   ??? AcceptButton             [Button - Color: Green]
?   ?   ?   ??? Text                 [Text: "??ng ý"]
?   ?   ??? DeclineButton            [Button - Color: Red]
?   ?       ??? Text                 [Text: "T? ch?i"]
?   ?? (?n m?c ??nh)
?
??? InteractHint                     [Text - Anchor: Top-Center]
    ??? Text: "[F] Nói chuy?n"       [Font: 20, Color: Yellow]
    ?? (?n m?c ??nh - script t? hi?n khi player g?n)
```

---

### CHI TI?T T?NG COMPONENT

#### 1?? DialoguePanel

```yaml
GameObject: DialoguePanel
Component: RectTransform
  - Anchor Preset: Full Screen (Stretch - Stretch)
  - Left: 0, Right: 0, Top: 0, Bottom: 0
  - Active: ? (unchecked - script t? b?t)

Component: Image
  - Source Image: None (Sprite)
  - Color: Black (0, 0, 0, 180)
```

##### 1.1 Background (Overlay t?i)
```yaml
GameObject: Background
Parent: DialoguePanel
Component: Image
  - Source Image: None (Sprite)
  - Color: Black (0, 0, 0, 100)
  - Raycast Target: ? (block clicks)
```

##### 1.2 DialogueBox
```yaml
GameObject: DialogueBox
Parent: DialoguePanel
Component: RectTransform
  - Anchor: Center
  - Width: 800
  - Height: 400
  - Pos X: 0, Pos Y: 0

Component: Image
  - Source Image: UISprite (ho?c None)
  - Image Type: Sliced
  - Color: White (255, 255, 255, 255)
```

> ?? **Sprite tùy ch?n:**
> - `UISprite` — Unity built-in (ô vuông bo tròn)
> - `Background` — Unity built-in
> - `None` — dùng màu thu?n
> - Ho?c import sprite t? asset pack

###### 1.2.1 NPCNameBG
```yaml
GameObject: NPCNameBG
Parent: DialogueBox
Component: RectTransform
  - Anchor: Top-Center
  - Width: 250
  - Height: 50
  - Pos Y: 25 (nhô lên trên cùng)

Component: Image
  - Source Image: UISprite (ho?c None)
  - Color: Brown (139, 90, 43, 255)
```

**NPCNameText**
```yaml
GameObject: NPCNameText
Parent: NPCNameBG
Component: RectTransform
  - Anchor: Full Stretch
  - Left: 10, Right: 10, Top: 5, Bottom: 5

Component: Text
  - Text: "Tiên Ông"
  - Font: Arial Bold
  - Font Size: 24
  - Alignment: Center-Middle
  - Color: White
  - Best Fit: ?
```

###### 1.2.2 DialogueTextBG
```yaml
GameObject: DialogueTextBG
Parent: DialogueBox
Component: RectTransform
  - Anchor: Full Stretch
  - Left: 20, Right: 20, Top: 80, Bottom: 80

Component: Image
  - Source Image: None (Sprite)
  - Color: Light Yellow (255, 250, 220, 255)
```

**DialogueText**
```yaml
GameObject: DialogueText
Parent: DialogueTextBG
Component: RectTransform
  - Anchor: Full Stretch
  - Left: 15, Right: 15, Top: 15, Bottom: 15

Component: Text
  - Text: "" (tr?ng - script ?i?n)
  - Font: Arial
  - Font Size: 18
  - Alignment: Top-Left
  - Color: Black
  - Line Spacing: 1.2
  - Rich Text: ?
```

###### 1.2.3 NextButton
```yaml
GameObject: NextButton
Parent: DialogueBox
Component: RectTransform
  - Anchor: Bottom-Right
  - Width: 120
  - Height: 40
  - Pos X: -30, Pos Y: 30

Component: Button
  - Navigation: None
  - Transition: Color Tint
    - Normal: White
    - Highlighted: Light Blue (200, 230, 255)
    - Pressed: Gray (150, 150, 150)
  - OnClick: (script t? gán)
```

**Text (con c?a NextButton)**
```yaml
GameObject: Text
Parent: NextButton
Component: RectTransform
  - Anchor: Full Stretch
  - Margins: 0

Component: Text
  - Text: "Ti?p t?c"
  - Font Size: 16
  - Alignment: Center-Middle
  - Color: Black
```

---

#### 2?? ConfirmPanel

```yaml
GameObject: ConfirmPanel
Component: RectTransform
  - Anchor: Full Screen
  - Active: ? (unchecked)

Component: Image
  - Source Image: None (Sprite)
  - Color: Black (0, 0, 0, 200)
```

##### 2.1 Background
*(T??ng t? DialoguePanel Background)*

##### 2.2 ConfirmBox
```yaml
GameObject: ConfirmBox
Parent: ConfirmPanel
Component: RectTransform
  - Anchor: Center
  - Width: 500
  - Height: 250

Component: Image
  - Source Image: UISprite (ho?c None)
  - Color: White (255, 255, 255, 255)
```

###### 2.2.1 QuestionText
```yaml
GameObject: QuestionText
Parent: ConfirmBox
Component: RectTransform
  - Anchor: Top
  - Width: 450
  - Height: 100
  - Pos Y: -60

Component: Text
  - Text: "B?n có mu?n nh?n nhi?m v? này không?"
  - Font Size: 20
  - Alignment: Center-Middle
  - Color: Black
```

###### 2.2.2 AcceptButton
```yaml
GameObject: AcceptButton
Parent: ConfirmBox
Component: RectTransform
  - Anchor: Bottom
  - Width: 200
  - Height: 50
  - Pos X: -110, Pos Y: 40

Component: Image
  - Source Image: None (Sprite)
  - Color: Green (100, 200, 100, 255)

Component: Button
  - OnClick: (script t? gán)
```

**Text: "??ng ý"**
```yaml
Font Size: 18
Alignment: Center
Color: White
Bold: ?
```

###### 2.2.3 DeclineButton
```yaml
GameObject: DeclineButton
Parent: ConfirmBox
Component: RectTransform
  - Anchor: Bottom
  - Width: 200
  - Height: 50
  - Pos X: 110, Pos Y: 40

Component: Image
  - Source Image: None (Sprite)
  - Color: Red (200, 100, 100, 255)

Component: Button
  - OnClick: (script t? gán)
```

**Text: "T? ch?i"**
```yaml
Font Size: 18
Alignment: Center
Color: White
Bold: ?
```

---

#### 3?? InteractHint

```yaml
GameObject: InteractHint
Parent: Canvas
Component: RectTransform
  - Anchor: Top-Center
  - Width: 200
  - Height: 50
  - Pos Y: -100
  - Active: ? (script t? hi?n)

Component: Text
  - Text: "[F] Nói chuy?n"
  - Font: Arial Bold
  - Font Size: 20
  - Alignment: Center-Middle
  - Color: Yellow (255, 255, 0, 255)

Component: Shadow (optional)
  - Effect Distance: (2, -2)
  - Color: Black
```

---

## ?? T?O SPRITE TÙY CH?NH (N?U C?N)

### Cách 1: Dùng Unity built-in sprites

Unity có s?n sprite `UISprite`:
```
Assets ? Create ? Sprites ? UISprite
```

### Cách 2: T?o sprite t? texture ??n gi?n

1. T?o file PNG tr?ng 64x64px (dùng Paint/Photoshop)
2. Import vào Unity
3. Select texture ? Inspector:
   ```
   Texture Type: Sprite (2D and UI)
   Sprite Mode: Single
   Apply
   ```
4. Kéo vào Source Image field

### Cách 3: Dùng Sliced Image (khung tho?i co giãn)

1. T?o texture PNG v?i border (vi?n)
2. Import ? Texture Type: Sprite (2D and UI)
3. Click "Sprite Editor"
4. Kéo border t? 4 c?nh vào trong
5. Apply
6. Trong Image component:
   ```
   Source Image: [Your Sprite]
   Image Type: Sliced
   ```

---

## ?? GÁN CÁC REFERENCES VÀO INSPECTOR

### M? QuestNPC_TienOng ? Inspector ? QuestDialogue

#### Dialogue UI Section
```
???????????????????????????????????????????
? Dialogue Panel:    [DialoguePanel]     ? ? kéo t? Hierarchy
? Npc Name Text:     [NPCNameText]       ?
? Dialogue Text:     [DialogueText]      ?
? Next Button:       [NextButton]        ?
? Next Button Text:  [Text (NextButton)] ? ? Text con c?a NextButton
? Confirm Panel:     [ConfirmPanel]      ?
? Accept Button:     [AcceptButton]      ?
? Decline Button:    [DeclineButton]     ?
???????????????????????????????????????????
```

#### Interaction Section
```
???????????????????????????????????????????
? Interact Hint:     [InteractHint]      ?
???????????????????????????????????????????
```

---

## ?? C?U HÌNH BOSS CONTROLLER

### M? scene boss ? Ch?n Boss GameObject

```
Inspector ? BossController ? Quest Integration
???????????????????????????????????????????
? Boss Type:  [ChanTinh ?]               ? ? boss Ch?n Tinh
???????????????????????????????????????????
```

**Các tu? ch?n:**
- `None` — không l?u ti?n trình
- `ChanTinh` — boss Ch?n Tinh (nhi?m v? 1)
- `DaiBangTinh` — boss ??i Bàng Tinh (nhi?m v? 2)

> ?? **Quan tr?ng:** Scene boss ph?i có BossController v?i `bossType` ?úng thì ti?n trình m?i l?u.

---

## ?? THÊM SCENE VÀO BUILD SETTINGS

### File ? Build Settings

```
???????????????????????????????????????????????????????
? Scenes In Build:                                    ?
???????????????????????????????????????????????????????
? ? Main Menu                               [Index 0]? ? Scene menu chính
? ? PlaygroundB                             [Index 1]? ? Scene boss Ch?n Tinh
? ? PlaygroundDaiBang (n?u có)              [Index 2]? ? Scene boss ??i Bàng
???????????????????????????????????????????????????????
```

**Cách thêm scene:**
1. M? scene c?n thêm trong Hierarchy
2. `File ? Build Settings`
3. `Add Open Scenes`

> ?? N?u scene không có trong Build Settings ? s? crash khi `SceneManager.LoadScene()`.

---

## ?? TESTING & DEBUGGING

### 1. Test trong Unity Editor

#### Test 1: Ki?m tra hi?n th? UI
```
Play Mode
? Di chuy?n player ??n g?n NPC
? Ki?m tra:
   ? "[F] Nói chuy?n" hi?n khi g?n (? 3m)
   ? Hint bi?n m?t khi xa (> 3m)
```

#### Test 2: Ki?m tra h?i tho?i
```
Nh?n F khi g?n NPC
? DialoguePanel hi?n
? Text "Tiên Ông" ? trên cùng
? H?i tho?i hi?n t?ng ký t? (typewriter)
? Nh?n Next ? dòng m?i
? H?t tho?i ? ConfirmPanel hi?n
```

#### Test 3: Ki?m tra ??ng Ý
```
ConfirmPanel ? Nh?n "??ng ý"
? Scene chuy?n sang PlaygroundB
? Player có th? ?i?u khi?n bình th??ng
```

#### Test 4: Ki?m tra T? Ch?i
```
ConfirmPanel ? Nh?n "T? ch?i"
? Panel ?óng
? Player có th? di chuy?n
? ??n g?n l?i ? v?n nói chuy?n ???c
```

#### Test 5: Ki?m tra ti?n trình
```
?ánh boss Ch?n Tinh ? ch?t
? Quay l?i NPC ? h?i tho?i ??i thành nhi?m v? ??i Bàng Tinh
? ?ánh boss ??i Bàng ? ch?t
? Quay l?i NPC ? h?i tho?i k?t thúc (không confirm)
```

### 2. Debug Console Logs

Khi ch?y ?úng, Console s? hi?n th?:
```
[QuestDialogue] Ch?n Tinh ?ã b? tiêu di?t — ti?n trình l?u.
[QuestDialogue] ??i Bàng Tinh ?ã b? tiêu di?t — ti?n trình l?u.
[QuestDialogue] Ti?n trình ?ã ???c reset.
```

### 3. Ki?m tra PlayerPrefs th? công

**Windows:**
```
Registry Editor ? HKEY_CURRENT_USER\Software\[CompanyName]\[ProductName]
```

**Mac:**
```
~/Library/Preferences/com.[CompanyName].[ProductName].plist
```

Ho?c dùng script debug:
```csharp
// Paste vào Console ho?c EditorWindow
Debug.Log("Ch?n Tinh: " + PlayerPrefs.GetInt("BossChanTinhDead", 0));
Debug.Log("??i Bàng: " + PlayerPrefs.GetInt("BossDaiBangDead", 0));

// Reset th? công
PlayerPrefs.DeleteAll();
PlayerPrefs.Save();
```

---

## ?? TROUBLESHOOTING

### ? V?n ??: Hint không hi?n khi ??n g?n NPC

**Nguyên nhân:**
- Player không có tag `"Player"`
- InteractHint không ???c gán vào Inspector
- InteractHint ?ang active = true lúc ??u

**Gi?i pháp:**
```
1. Select Player ? Inspector ? Tag: "Player"
2. QuestNPC ? QuestDialogue ? Interact Hint: gán InteractHint
3. InteractHint ? Inspector ? uncheck Active (?)
```

---

### ? V?n ??: Nh?n F không m? dialogue

**Nguyên nhân:**
- Phím Interact Key không kh?p
- Player quá xa (> 3m)
- Dialogue Panel ?ang active = true

**Gi?i pháp:**
```
1. Inspector ? Interact Key: ??m b?o là F
2. ??ng sát NPC (< 3m)
3. DialoguePanel ? uncheck Active (?)
```

---

### ? V?n ??: Text không hi?n (typewriter không ch?y)

**Nguyên nhân:**
- DialogueText không gán vào Inspector
- Font size = 0 ho?c màu = trong su?t

**Gi?i pháp:**
```
1. Gán DialogueText vào Inspector
2. DialogueText ? Font Size: 18, Color: Black
```

---

### ? V?n ??: Nút không click ???c

**Nguyên nhân:**
- Canvas ? GraphicRaycaster b? thi?u
- Button.Interactable = false
- Panel phía trên che m?t

**Gi?i pháp:**
```
1. Canvas ? Add Component ? Graphic Raycaster
2. Button ? Inspector ? Interactable: ?
3. Ki?m tra Hierarchy: Button ph?i ? d??i cùng (render sau cùng)
```

---

### ? V?n ??: Scene không load (crash)

**Nguyên nhân:**
- Scene name sai chính t?
- Scene không có trong Build Settings

**Gi?i pháp:**
```
1. QuestDialogue ? Chan Tinh Scene Name: "PlaygroundB" (chính xác 100%)
2. File ? Build Settings ? Add Open Scenes
```

---

### ? V?n ??: Ti?n trình không l?u sau khi gi?t boss

**Nguyên nhân:**
- BossController.bossType = None
- Boss b? destroy tr??c khi g?i Die()

**Gi?i pháp:**
```
1. Boss ? Inspector ? Boss Type: ChanTinh / DaiBangTinh
2. BossController.Die() ? Destroy(gameObject, 5f) (??i 5s m?i destroy)
```

---

### ? V?n ??: Player b? lock movement sau khi ?óng dialogue

**Nguyên nhân:**
- SetPlayerMovement(true) không ???c g?i
- ThirdPersonController b? disable

**Gi?i pháp:**
```
Ki?m tra CloseDialogue() có dòng:
SetPlayerMovement(true);

Ho?c th? công:
Player ? ThirdPersonController ? Enabled: ?
Player ? CharacterController ? Enabled: ?
```

---

## ?? TU? CH?NH NÂNG CAO

### 1. Thêm avatar NPC

```csharp
[Header("NPC Avatar")]
public Image npcAvatar;
public Sprite chanTinhQuestAvatar;
public Sprite daiBangQuestAvatar;
public Sprite completedAvatar;
```

Trong `StartDialogue()`:
```csharp
if (npcAvatar != null)
{
    if (_targetScene == chanTinhSceneName)
        npcAvatar.sprite = chanTinhQuestAvatar;
    else if (_targetScene == daiBangSceneName)
        npcAvatar.sprite = daiBangQuestAvatar;
    else
        npcAvatar.sprite = completedAvatar;
}
```

---

### 2. Thêm âm thanh dialogue

```csharp
[Header("Audio")]
public AudioClip dialogueOpenSound;
public AudioClip typewriterSound;
public AudioClip buttonClickSound;

private AudioSource _audioSource;

private void Awake()
{
    _audioSource = GetComponent<AudioSource>();
    if (_audioSource == null)
        _audioSource = gameObject.AddComponent<AudioSource>();
}

private void StartDialogue(...)
{
    if (dialogueOpenSound != null)
        _audioSource.PlayOneShot(dialogueOpenSound);
    // ...
}

private IEnumerator TypeLine(string line)
{
    foreach (char c in line)
    {
        if (typewriterSound != null)
            _audioSource.PlayOneShot(typewriterSound, 0.3f);
        // ...
    }
}
```

---

### 3. Thêm ho?t ?nh NPC

```csharp
private Animator _npcAnimator;

private void Start()
{
    _npcAnimator = GetComponent<Animator>();
}

private void StartDialogue(...)
{
    if (_npcAnimator != null)
        _npcAnimator.SetTrigger("Talk");
    // ...
}

private void CloseDialogue()
{
    if (_npcAnimator != null)
        _npcAnimator.SetTrigger("Idle");
    // ...
}
```

---

### 4. Làm m? background khi dialogue m?

Thêm vào DialoguePanel:
```
Add Component ? CanvasGroup
Alpha: 0 (ban ??u)
```

Script:
```csharp
private CanvasGroup _dialogueCanvasGroup;

private void Start()
{
    _dialogueCanvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
}

private IEnumerator FadeIn()
{
    float elapsed = 0f;
    float duration = 0.3f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        _dialogueCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
        yield return null;
    }
}
```

---

### 5. Thêm l?a ch?n nhi?u câu tr? l?i

```csharp
[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public string[] followUpLines;
    public string targetScene;
}

public DialogueChoice[] choices;
```

T?o UI:
```
ConfirmPanel
?? Choice1Button ? "Tôi s?n sàng"
?? Choice2Button ? "?? tôi chu?n b? thêm"
?? Choice3Button ? "Tôi t? ch?i"
```

---

## ?? CHECKLIST HOÀN CH?NH

### Setup NPC
- [ ] T?o GameObject QuestNPC
- [ ] G?n model NPC (tu? ch?n)
- [ ] Thêm Collider (Is Trigger = true)
- [ ] G?n script QuestDialogue
- [ ] Gán Chan Tinh Scene Name
- [ ] Gán Dai Bang Scene Name
- [ ] ??t Interact Range = 3
- [ ] ??t Interact Key = F
- [ ] ??t Player Tag = "Player"
- [ ] ??t NPC Name = "Tiên Ông"

### Setup UI
- [ ] T?o Canvas
- [ ] T?o DialoguePanel (?n ban ??u)
- [ ] T?o NPCNameText
- [ ] T?o DialogueText
- [ ] T?o NextButton + Text
- [ ] T?o ConfirmPanel (?n ban ??u)
- [ ] T?o AcceptButton + Text "??ng ý"
- [ ] T?o DeclineButton + Text "T? ch?i"
- [ ] T?o InteractHint (?n ban ??u)
- [ ] Gán t?t c? vào Inspector QuestDialogue

### Setup Boss
- [ ] M? scene boss Ch?n Tinh
- [ ] Boss ? Boss Type = ChanTinh
- [ ] M? scene boss ??i Bàng (n?u có)
- [ ] Boss ? Boss Type = DaiBangTinh

### Build Settings
- [ ] Thêm Main Menu (index 0)
- [ ] Thêm PlaygroundB (index 1)
- [ ] Thêm scene boss ??i Bàng (n?u có)

### Testing
- [ ] Hint hi?n khi g?n NPC
- [ ] Nh?n F ? dialogue m?
- [ ] Text hi?n th? typewriter
- [ ] Nh?n Next ? dòng m?i
- [ ] H?t tho?i ? confirm hi?n
- [ ] ??ng ý ? chuy?n scene
- [ ] T? ch?i ? ?óng dialogue
- [ ] Gi?t boss ? ti?n trình l?u
- [ ] Quay l?i NPC ? nhi?m v? ti?p theo
- [ ] Gi?t c? 2 boss ? dialogue k?t thúc

---

## ?? HOÀN THÀNH!

Bây gi? b?n ?ã có:
- ? H? th?ng dialogue hoàn ch?nh
- ? Quest system 2 boss
- ? Ti?n trình l?u t? ??ng
- ? UI ??p m?t v?i typewriter effect
- ? T??ng tác m??t mà

**Chúc b?n phát tri?n game thành công! ??**
