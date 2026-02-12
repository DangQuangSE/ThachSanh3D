# ?? H??NG D?N THÊM VFX SLASH EFFECTS CHO RÌU

> **D? án:** ThachSanh3D  
> **M?c ?ích:** Thêm hi?u ?ng VFX khi t?n công b?ng rìu s? d?ng Slash Effects FREE  
> **Th?i gian setup:** 15-30 phút  

---

## ?? M?C L?C

1. [Chu?n b?](#1-chu?n-b?)
2. [T?o VFX Spawn Point](#2-t?o-vfx-spawn-point)
3. [Thêm VFX Manager Component](#3-thêm-vfx-manager-component)
4. [Setup VFX Prefabs](#4-setup-vfx-prefabs)
5. [B?t và S? d?ng Gizmos](#5-b?t-và-s?-d?ng-gizmos)
6. [?i?u ch?nh Rotation cho ?òn chém chéo](#6-?i?u-ch?nh-rotation-cho-?òn-chém-chéo)
7. [?i?u ch?nh Timing](#7-?i?u-ch?nh-timing)
8. [Post-Processing (Tùy ch?n)](#8-post-processing-tùy-ch?n)
9. [Troubleshooting](#9-troubleshooting)

---

## 1. CHU?N B?

### ? Ki?m tra các file ?ã có:

??m b?o b?n ?ã có các file sau trong project:

```
Assets/
??? StarterAssets/ThirdPersonController/Scripts/
?   ??? AttackVFXManager.cs          ? (Script chính)
?   ??? AdvancedAttackVFXManager.cs  ? (Phiên b?n nâng cao)
?   ??? VFXSetupGuide.cs            ? (H??ng d?n)
?
??? ThachSanhGeneral/Quang/Matthew Guz/Slash Effects FREE/
    ??? Prefabs/                      ? (Các VFX prefabs)
    ??? Documentation/
        ??? Readme IMPORTANT.txt      ?
```

### ?? Slash Effects FREE Prefabs có s?n:

Vào th? m?c: `Assets/ThachSanhGeneral/Quang/Matthew Guz/Slash Effects FREE/Prefabs`

B?n s? th?y các prefabs VFX, ví d?:
- `Slash_01` - Chém ngang
- `Slash_02` - Chém chéo
- `Slash_03` - Chém m?nh
- `Slash_04` - Chém xoay
- `Slash_05` - Ultimate slash
- ... và nhi?u prefabs khác

---

## 2. T?O VFX SPAWN POINT

VFX Spawn Point là ?i?m mà hi?u ?ng s? xu?t hi?n (th??ng là ??u l??i rìu).

### B??c 1: Tìm Weapon trong Hierarchy

```
Hierarchy:
PlayerArmature (Animator)
  ?? Hips
      ?? Spine
          ?? Spine1
              ?? Spine2
                  ?? LeftShoulder / RightShoulder
                      ?? LeftArm / RightArm
                          ?? LeftForeArm / RightForeArm
                              ?? LeftHand / RightHand
                                  ?? Weapon (Rìu) ? TÌM OBJECT NÀY
```

### B??c 2: T?o Empty GameObject

1. **Click chu?t ph?i** vào `Weapon` (ho?c object c?m rìu)
2. Ch?n **Create Empty**
3. ??t tên: `VFX_SpawnPoint`

### B??c 3: ??t v? trí cho Spawn Point

1. **Ch?n** `VFX_SpawnPoint` trong Hierarchy
2. Trong **Scene view**, di chuy?n nó ??n:
   - **??u l??i rìu** (cho slash effect ??p nh?t)
   - Ho?c **gi?a l??i rìu** (n?u mu?n VFX r?ng h?n)

3. **Transform position g?i ý:**
   ```
   Position: (0.5, 0, 0)  // Offset t? weapon pivot
   Rotation: (0, 0, 0)    // Gi? nguyên ho?c xoay theo h??ng chém
   Scale: (1, 1, 1)       // Gi? nguyên
   ```

> ?? **Tip:** B?t **Gizmos** ?? d? dàng th?y v? trí spawn point (h??ng d?n ? b??c 5)

---

## 3. THÊM VFX MANAGER COMPONENT

### B??c 1: Ch?n Player GameObject

1. Trong **Hierarchy**, tìm và ch?n GameObject **Player** (ho?c PlayerArmature)
2. ??m b?o object này có **Animator** component

### B??c 2: Add Component

1. Trong **Inspector**, click **Add Component**
2. Search: `AttackVFXManager`
3. Click ?? thêm component

> ?? **L?u ý:** N?u mu?n dùng nhi?u VFX cho 1 ?òn ho?c tính n?ng nâng cao, dùng `AdvancedAttackVFXManager` thay vì `AttackVFXManager`

---

## 4. SETUP VFX PREFABS

Sau khi thêm `AttackVFXManager` component, b?n s? th?y các tr??ng trong Inspector:

### ?? VFX Prefabs - Slash Effects

Kéo các prefab VFX vào các slot t??ng ?ng:

| Slot | VFX Prefab g?i ý | Mô t? |
|------|------------------|-------|
| **Attack 1 VFX** | `Slash_01` ho?c `Slash_02` | ?òn chém ??u tiên (th??ng là chém ngang) |
| **Attack 2 VFX** | `Slash_03` ho?c `Slash_04` | ?òn chém th? 2 (chém chéo) |
| **Attack 3 VFX** | `Slash_05` ho?c `Slash_06` | ?òn k?t thúc combo (m?nh nh?t) |
| **Ultimate VFX** | `Slash_05` ho?c VFX l?n nh?t | Skill ultimate |

**Cách kéo prefab:**
1. M? th? m?c: `Assets/ThachSanhGeneral/Quang/Matthew Guz/Slash Effects FREE/Prefabs`
2. **Kéo** prefab vào slot t??ng ?ng trong Inspector

### ?? VFX Spawn Settings

| Tr??ng | Giá tr? g?i ý | Mô t? |
|--------|---------------|-------|
| **VFX Spawn Point** | ? Kéo `VFX_SpawnPoint` GameObject vào | V? trí spawn VFX |
| **Spawn Offset** | `(0, 0, 0)` | Offset thêm t? spawn point |
| **VFX Scale** | `1.0` | Kích th??c VFX (t?ng n?u VFX quá nh?) |
| **VFX Lifetime** | `2.0` | Th?i gian VFX t?n t?i (giây) |

### ?? VFX Rotation Per Attack

| Tr??ng | Giá tr? g?i ý | Mô t? |
|--------|---------------|-------|
| **Attack 1 Rotation Offset** | `(0, 0, 0)` | Chém ngang th?ng |
| **Attack 2 Rotation Offset** | `(0, 0, 45)` | **Chém chéo 45°** ? |
| **Attack 3 Rotation Offset** | `(0, 0, -45)` ho?c `(0, 0, 0)` | Chém chéo ng??c ho?c ngang |
| **Ultimate Rotation Offset** | `(0, 0, 0)` | Tùy ch?nh theo animation |

### ?? Timing Settings

| Tr??ng | Giá tr? g?i ý | Mô t? |
|--------|---------------|-------|
| **Attack 1 Spawn Time** | `0.4` | VFX spawn ? 40% animation |
| **Attack 2 Spawn Time** | `0.4` | VFX spawn ? 40% animation |
| **Attack 3 Spawn Time** | `0.4` | VFX spawn ? 40% animation |
| **Ultimate Spawn Time** | `0.5` | VFX spawn ? 50% animation |

> ?? **Gi?i thích:** `0.4` = 40% c?a animation, t?c là VFX s? spawn khi animation ch?y ???c 40%

### ??? Advanced Rotation Settings

| Tr??ng | Giá tr? g?i ý | Mô t? |
|--------|---------------|-------|
| **Use Weapon Rotation** | ? B?t | VFX s? xoay theo h??ng weapon |
| **Auto Mirror Rotation** | ? T?t | T? ??ng ??o ng??c rotation (ch?a c?n thi?t) |

### ?? Debug

| Tr??ng | Giá tr? g?i ý | Mô t? |
|--------|---------------|-------|
| **Show Debug Logs** | ? B?t (khi test) | Hi?n th? log trong Console |
| **Show Rotation Gizmos** | ? B?t | **Hi?n th? m?i tên h??ng trong Scene** ? |

---

## 5. B?T VÀ S? D?NG GIZMOS

### ? T?i sao không th?y Gizmos?

Có th? do:
1. ? Ch?a ch?n Player GameObject
2. ? Gizmos b? t?t trong Scene view
3. ? VFX prefabs ch?a ???c assign

### ? Cách b?t Gizmos:

#### B??c 1: B?t Gizmos trong Scene View

Nhìn vào góc trên bên ph?i c?a **Scene view**, b?n s? th?y toolbar:

```
[2D] [Shaded] [Gizmos ?] [??] [??] ...
                  ?
            CLICK VÀO ?ÂY
```

1. Click vào nút **Gizmos** 
2. ??m b?o nó **???C B?T** (màu sáng, không xám)

#### B??c 2: Ch?n Player GameObject

1. Trong **Hierarchy**, click ch?n **Player** (object có component `AttackVFXManager`)
2. Scene view s? t? ??ng focus vào Player

#### B??c 3: Ki?m tra Show Rotation Gizmos

1. Trong **Inspector** c?a `AttackVFXManager`
2. Ph?n **Debug** ? **Show Rotation Gizmos** ph?i ???c ? **B?T**

### ?? Gizmos s? hi?n th? nh? th? nào?

Khi setup ?úng, b?n s? th?y trong **Scene view**:

```
                    VFX_SpawnPoint
                         ?
    ???????????????????????????
    ?      ?? Hình c?u ??     ? ? V? trí spawn
    ?                         ?
    ?  ?? ??????             ? ? M?i tên XANH D??NG = h??ng weapon g?c
    ?  ?? ????               ? ? M?i tên XANH LÁ = Attack 1
    ?  ?? ????               ? ? M?i tên VÀNG = Attack 2 (chém chéo) ?
    ?  ?? ????               ? ? M?i tên XANH CYAN = Attack 3
    ?  ?? ????               ? ? M?i tên TÍM = Ultimate
    ?                         ?
    ???????????????????????????
```

### ?? Ý ngh?a t?ng màu:

| Màu | Ý ngh?a |
|-----|---------|
| ?? **??** | V? trí spawn VFX (hình c?u + ???ng n?i) |
| ?? **Xanh d??ng** | H??ng forward g?c c?a weapon (không có rotation offset) |
| ?? **Xanh lá** | H??ng VFX c?a **Attack 1** |
| ?? **Vàng** | H??ng VFX c?a **Attack 2** (quan tr?ng cho chém chéo!) |
| ?? **Xanh cyan** | H??ng VFX c?a **Attack 3** |
| ?? **Tím** | H??ng VFX c?a **Ultimate** |

> ?? **Tip:** N?u v?n không th?y Gizmos, hãy:
> 1. Zoom vào g?n Player trong Scene view
> 2. Ki?m tra `VFX_SpawnPoint` ?ã ???c assign vào `VFX Spawn Point` slot ch?a
> 3. Ki?m tra ít nh?t 1 VFX prefab ?ã ???c assign (Attack 1, 2, 3 ho?c Ultimate)

---

## 6. ?I?U CH?NH ROTATION CHO ?ÒNG CHÉM CHÉO

### ?? V?n ??: VFX h??ng sai khi chém chéo

Khi animation chém chéo, VFX c?n xoay ?? kh?p v?i h??ng chém.

### ?? Hi?u v? Rotation Offset (X, Y, Z):

```
        Y (Yaw)
         ?
         ?      Z (Roll)
         ?     ?
         ?   ?
         ? ?
         ???????????? X (Pitch)
```

| Tr?c | Tên | Mô t? | Ví d? s? d?ng |
|------|-----|-------|---------------|
| **X** | Pitch | Xoay lên/xu?ng (nghiêng tr??c/sau) | ?òn chém t? trên xu?ng |
| **Y** | Yaw | Xoay trái/ph?i (quay ngang) | ??i h??ng VFX sang bên |
| **Z** | Roll | Xoay nghiêng (roll) | **Chém chéo** ? |

### ?? Các giá tr? Rotation thông d?ng:

#### 1. Chém NGANG (trái ? ph?i ho?c ph?i ? trái)
```
Rotation Offset: (0, 0, 0)
```

#### 2. Chém CHÉO t? trên trái ? d??i ph?i ?
```
Rotation Offset: (0, 0, 45)   ? B?T ??U V?I GIÁ TR? NÀY
ho?c
Rotation Offset: (0, 0, 60)   ? Góc d?c h?n
```

#### 3. Chém CHÉO t? trên ph?i ? d??i trái ?
```
Rotation Offset: (0, 0, -45)
ho?c
Rotation Offset: (0, 0, -60)
```

#### 4. Chém TH?NG ??NG (t? trên ? xu?ng) ?
```
Rotation Offset: (0, 0, 90)
ho?c
Rotation Offset: (90, 0, 0)   ? Th? c? 2 xem cái nào ??p h?n
```

#### 5. ?ÂM TH?NG
```
Rotation Offset: (0, 0, 0)
+ ?i?u ch?nh Spawn Offset ?? VFX ?i theo h??ng ?âm
```

### ?? Cách ?i?u ch?nh t?ng b??c:

#### B??c 1: Xác ??nh h??ng chém

1. **Play** game
2. **Quan sát** animation Attack 2
3. **Xác ??nh** h??ng chém:
   - Chém ngang? ? `(0, 0, 0)`
   - Chém chéo t? trên trái? ? `(0, 0, 45)`
   - Chém chéo t? trên ph?i? ? `(0, 0, -45)`
   - Chém th?ng ??ng? ? `(0, 0, 90)` ho?c `(90, 0, 0)`

#### B??c 2: Nh?p giá tr? vào Inspector

1. **Stop** game
2. Ch?n **Player** trong Hierarchy
3. Trong `AttackVFXManager` ? **VFX Rotation Per Attack**
4. Nh?p giá tr? vào **Attack 2 Rotation Offset**:
   ```
   X: 0
   Y: 0
   Z: 45    ? Th? giá tr? này tr??c
   ```

#### B??c 3: Ki?m tra Gizmos

1. Trong **Scene view**, quan sát **m?i tên màu vàng** (Attack 2)
2. M?i tên này ph?i **h??ng theo chi?u chém** c?a animation
3. N?u sai, ?i?u ch?nh giá tr? Z lên/xu?ng:
   - T?ng Z ? xoay theo chi?u kim ??ng h?
   - Gi?m Z ? xoay ng??c chi?u kim ??ng h?

#### B??c 4: Test trong Play mode

1. **Play** game
2. **Attack** ?? xem VFX
3. **Ki?m tra:**
   - ? VFX có h??ng ?úng chi?u chém?
   - ? VFX có ??p và t? nhiên?
   - ? N?u sai, quay l?i B??c 2

### ?? Ví d? c? th? cho Combo 3 ?òn:

```
Attack 1: Chém ngang t? trái
  ? Rotation Offset: (0, 0, 0)

Attack 2: Chém chéo t? trên ph?i xu?ng d??i trái
  ? Rotation Offset: (0, 0, -45)

Attack 3: Chém th?ng ??ng m?nh
  ? Rotation Offset: (0, 0, 90)
```

### ?? N?u v?n sai h??ng:

#### Th? 1: ??i tr?c
```
N?u (0, 0, 45) không work
? Th? (45, 0, 0)
? Th? (0, 45, 0)
```

#### Th? 2: ?i?u ch?nh VFX_SpawnPoint
1. Ch?n `VFX_SpawnPoint` trong Hierarchy
2. Xoay **Rotation** c?a GameObject này
3. K?t h?p v?i Rotation Offset trong script

#### Th? 3: T?t Use Weapon Rotation
1. Trong `AttackVFXManager`
2. **Advanced Rotation Settings** ? **Use Weapon Rotation** ? ? **T?T**
3. Bây gi? Rotation Offset s? dùng world rotation thay vì weapon rotation

---

## 7. ?I?U CH?NH TIMING

### ?? Spawn Time là gì?

**Spawn Time** = % c?a animation khi VFX xu?t hi?n
- `0.0` = VFX spawn ngay khi animation b?t ??u
- `0.5` = VFX spawn ? gi?a animation
- `1.0` = VFX spawn khi animation k?t thúc

### ?? M?c tiêu: VFX spawn ?úng lúc rìu chém

#### B??c 1: Xác ??nh timing chính xác

1. **M?** Window ? Animation
2. **Ch?n** animation clip (Attack_1, Attack_2, Attack_3)
3. **Play** animation và quan sát:
   - Khi nào rìu b?t ??u swing?
   - Khi nào rìu ch?m m?c tiêu (?i?m chém)?
4. **Ghi nh?** th?i ?i?m ?ó (ví d?: giây th? 0.4 trong animation dài 1 giây = 40%)

#### B??c 2: ?i?u ch?nh trong Inspector

1. Ch?n **Player** trong Hierarchy
2. Trong `AttackVFXManager` ? **Timing Settings**
3. ?i?u ch?nh các giá tr?:

| Animation | Spawn Time g?i ý | Mô t? |
|-----------|------------------|-------|
| Attack 1 | `0.3` - `0.5` | Th??ng spawn ? gi?a swing |
| Attack 2 | `0.3` - `0.5` | Tùy animation |
| Attack 3 | `0.4` - `0.6` | ?òn m?nh th??ng swing ch?m h?n |
| Ultimate | `0.5` - `0.7` | Spawn khi r?t xu?ng ho?c impact |

#### B??c 3: Test và fine-tune

1. **Play** game
2. **Attack** và quan sát:
   - VFX spawn **QUÁ S?M** ? T?ng Spawn Time (vd: `0.4` ? `0.5`)
   - VFX spawn **QUÁ MU?N** ? Gi?m Spawn Time (vd: `0.4` ? `0.3`)
3. **L?p l?i** cho ??n khi hoàn h?o

### ?? Ví d? c? th?:

```
Animation Attack_1 (chém ngang):
- T?ng th?i gian: 1 giây
- Rìu b?t ??u swing: 0.2 giây ? 20%
- Rìu chém (peak): 0.4 giây ? 40% ?
- Animation k?t thúc: 1.0 giây ? 100%

? Spawn Time nên là: 0.4 ho?c 0.3-0.5
```

> ?? **Tip:** B?t `Show Debug Logs` ?? xem trong Console khi VFX spawn, giúp debug timing d? h?n.

---

## 8. POST-PROCESSING (TÙY CH?N)

?? VFX ??p h?n v?i hi?u ?ng Bloom và Ambient Occlusion.

### ?? B??c 1: Cài ??t Post Processing

1. M? **Window** ? **Package Manager**
2. Search: `Post Processing`
3. Click **Install**

### ?? B??c 2: T?o Layer

1. Góc trên ph?i c?a Unity ? **Layers** ? **Edit Layers**
2. Thêm layer m?i: `PostProcessing`

### ?? B??c 3: Setup Main Camera

1. Ch?n **Main Camera** trong Hierarchy
2. **Add Component** ? `Post Processing Layer`
3. Trong component:
   - **Layer**: Ch?n `PostProcessing`
   - **Trigger**: Ch?n `Main Camera` (ho?c ?? None)

### ?? B??c 4: T?o Post Processing Volume

1. **Hierarchy** ? Click ph?i ? **Create Empty**
2. ??t tên: `PP Volume`
3. **Add Component** ? `Post Processing Volume`
4. Trong component:
   - ? **Is Global**: B?T
   - **Profile**: Click **New**

### ? B??c 5: Add Effects

1. Trong `Post Processing Volume` ? Click **Add effect...**
2. Thêm **Ambient Occlusion**:
   - ? **Intensity**: B?T, set giá tr? = `1`
3. Click **Add effect...** l?n n?a
4. Thêm **Bloom**:
   - ? **Intensity**: B?T, set giá tr? = `5`
   - ? **Soft Knee**: B?T
   - ? **Clamp**: B?T
   - ? **Diffusion**: B?T

### ? K?t qu?:

VFX s? có hi?u ?ng phát sáng (Bloom) và bóng t?i ??p h?n (AO).

> ?? **L?u ý:** N?u build game mà Post Processing không ho?t ??ng, th? xóa h?t và setup l?i t? ??u (theo file Readme c?a Slash Effects).

---

## 9. TROUBLESHOOTING

### ? VFX không spawn

#### Nguyên nhân & Gi?i pháp:

1. **VFX prefabs ch?a ???c assign**
   - ? Ki?m tra trong Inspector ? VFX Prefabs
   - ? Kéo prefab vào các slot (Attack 1, 2, 3, Ultimate)

2. **VFX Spawn Point ch?a ???c set**
   - ? Ki?m tra `VFX Spawn Point` có GameObject `VFX_SpawnPoint` ch?a
   - ? N?u ch?a, kéo `VFX_SpawnPoint` t? Hierarchy vào slot này

3. **Animation state name không kh?p**
   - ? M? Animator Controller
   - ? Ki?m tra tên state: `Attack_1`, `Attack_2`, `Attack_3`, `UntimateAttack`
   - ? N?u khác, s?a trong script `AttackVFXManager.cs` dòng:
     ```csharp
     if (currentState.IsName("Attack_1"))  // ??i tên này
     ```

4. **Component ch?a ???c add**
   - ? Ki?m tra Player có component `AttackVFXManager` ch?a

5. **Animator ch?a ???c set**
   - ? Player ph?i có component `Animator`

### ? VFX spawn nh?ng không th?y

1. **VFX quá nh?**
   - ? T?ng `VFX Scale` lên (vd: t? 1 ? 2 ho?c 3)

2. **VFX spawn xa camera**
   - ? Ki?m tra v? trí c?a `VFX_SpawnPoint`
   - ? ?i?u ch?nh `Spawn Offset`

3. **VFX b? destroy quá nhanh**
   - ? T?ng `VFX Lifetime` (vd: t? 2 ? 5 giây)

### ? VFX h??ng sai (quan tr?ng!)

#### Gi?i pháp 1: ?i?u ch?nh Rotation Offset
```
Th? các giá tr? sau theo th? t?:
1. (0, 0, 45)    - Chém chéo th??ng
2. (0, 0, -45)   - Chém chéo ng??c
3. (0, 0, 90)    - Th?ng ??ng
4. (45, 0, 0)    - ??i tr?c sang X
5. (0, 45, 0)    - ??i tr?c sang Y
```

#### Gi?i pháp 2: ?i?u ch?nh VFX_SpawnPoint rotation
```
1. Ch?n VFX_SpawnPoint trong Hierarchy
2. Xoay GameObject này trong Inspector
3. Test l?i
```

#### Gi?i pháp 3: T?t Use Weapon Rotation
```
1. AttackVFXManager ? Advanced Rotation Settings
2. Use Weapon Rotation ? ? T?T
3. Bây gi? dùng world rotation
```

### ? VFX spawn nhi?u l?n

1. **Không nên x?y ra** - script ?ã có ch?ng spam
2. N?u v?n b?:
   - ? Ki?m tra có nhi?u `AttackVFXManager` component không
   - ? Xóa các component th?a

### ? VFX spawn sai timing

1. **Spawn quá s?m**
   - ? T?ng `Spawn Time` (vd: 0.4 ? 0.5)

2. **Spawn quá mu?n**
   - ? Gi?m `Spawn Time` (vd: 0.4 ? 0.3)

3. **Không bi?t timing nào ?úng**
   - ? M? Window ? Animation
   - ? Play animation và quan sát
   - ? Tính toán: (Th?i ?i?m chém / T?ng th?i gian animation) = Spawn Time

### ? Không th?y Gizmos

1. **Gizmos b? t?t**
   - ? Scene view ? Góc trên ph?i ? Click `Gizmos` ?? B?T

2. **Ch?a ch?n Player**
   - ? Click ch?n Player trong Hierarchy

3. **Show Rotation Gizmos t?t**
   - ? Inspector ? Debug ? Show Rotation Gizmos ? ? B?T

4. **VFX prefabs ch?a assign**
   - ? Gizmos ch? hi?n khi có VFX prefab ???c assign
   - ? Assign ít nh?t 1 prefab ?? th?y Gizmos

5. **Zoom quá xa**
   - ? Zoom vào g?n Player trong Scene view

### ? VFX không follow weapon khi swing

**?ây là ?ÚNG!** VFX spawn r?i ??ng yên (gi?ng v?t chém trong không khí).

N?u mu?n VFX follow weapon:
```csharp
// Trong AdvancedAttackVFXManager
VFXEffect effect = new VFXEffect();
effect.parentToSpawnPoint = true;  ? B?T OPTION NÀY
```

### ? Animation không ch?y / Attack không ho?t ??ng

**?ây là v?n ?? c?a Attack System, không ph?i VFX!**

1. ? Ki?m tra `ThirdPersonController` có ho?t ??ng không
2. ? Ki?m tra Input System (Left Click = Attack)
3. ? Ki?m tra Animator Controller có transitions ?úng không

---

## ?? TÀI LI?U THAM KH?O

### Files trong project:

1. **AttackVFXManager.cs** - Script chính (??n gi?n, d? dùng)
2. **AdvancedAttackVFXManager.cs** - Script nâng cao (nhi?u tính n?ng)
3. **VFXSetupGuide.cs** - H??ng d?n chi ti?t (comments ti?ng Vi?t)

### Slash Effects Documentation:

- `Assets/ThachSanhGeneral/Quang/Matthew Guz/Slash Effects FREE/Documentation/Readme IMPORTANT.txt`

---

## ?? CHECKLIST HOÀN THÀNH

### B??c c? b?n:

- [ ] ? ?ã t?o `VFX_SpawnPoint` GameObject
- [ ] ? ?ã thêm `AttackVFXManager` component vào Player
- [ ] ? ?ã assign VFX prefabs vào các slot
- [ ] ? ?ã kéo `VFX_SpawnPoint` vào `VFX Spawn Point` slot
- [ ] ? ?ã B?T Gizmos và th?y các m?i tên màu
- [ ] ? ?ã test VFX spawn trong Play mode

### ?i?u ch?nh nâng cao:

- [ ] ? ?ã ?i?u ch?nh Rotation Offset cho ?òn chém chéo
- [ ] ? ?ã ?i?u ch?nh Timing cho t?ng ?òn
- [ ] ? VFX spawn ?úng lúc rìu chém
- [ ] ? VFX h??ng ?úng chi?u chém
- [ ] ? Kích th??c VFX phù h?p (Scale)

### Tùy ch?n:

- [ ] ? ?ã setup Post-Processing (Bloom + AO)
- [ ] ? ?ã test trên nhi?u animations khác nhau
- [ ] ? ?ã th? AdvancedAttackVFXManager (n?u c?n nhi?u VFX)

---

## ?? K?T QU? MONG ??I

Sau khi hoàn thành, b?n s? có:

? **VFX slash effects ??p m?t** xu?t hi?n khi t?n công  
? **Timing chính xác** - VFX spawn ?úng lúc rìu chém  
? **Rotation ?úng** - VFX h??ng theo chi?u chém (ngang, chéo, d?c)  
? **D? dàng tùy ch?nh** - Có th? thay ??i VFX, timing, rotation b?t c? lúc nào  
? **Không c?n ch?nh animation g?c** - Ho?t ??ng v?i b?t k? animation nào  

---

## ?? H? TR?

N?u g?p v?n ?? không có trong Troubleshooting:

1. **B?t Debug Logs**:
   ```
   AttackVFXManager ? Debug ? Show Debug Logs ?
   ```
   Xem Console ?? bi?t VFX có spawn không

2. **Ki?m tra Animation State Names**:
   - M? Animator Controller
   - Ki?m tra tên state có kh?p v?i code không

3. **Test t?ng b??c**:
   - Assign VFX cho Attack 1 tr??c
   - Test ? OK ? Ti?p t?c Attack 2, 3, Ultimate

4. **Screenshot và ghi log**:
   - Ch?p ?nh Inspector c?a AttackVFXManager
   - Copy log t? Console
   - Xem l?i ?? debug

---

**Chúc b?n setup thành công!** ???

*Last updated: 2024*
*Project: ThachSanh3D*
*Author: AI Assistant*
