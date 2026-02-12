# ?? H??NG D?N SETUP BOSS CONTROLLER

## ?? M?C L?C
1. [Yêu c?u h? th?ng](#yêu-c?u-h?-th?ng)
2. [Chu?n b? Boss GameObject](#chu?n-b?-boss-gameobject)
3. [Setup NavMesh](#setup-navmesh)
4. [C?u hình BossController](#c?u-hình-bosscontroller)
5. [Setup Animation (Optional)](#setup-animation-optional)
6. [T?o Attack Point](#t?o-attack-point)
7. [Setup Layer và Collision](#setup-layer-và-collision)
8. [Test Boss](#test-boss)
9. [Troubleshooting](#troubleshooting)

---

## ? YÊU C?U H? TH?NG

### Packages c?n thi?t:
- ? Unity NavMesh (AI Navigation)
- ?? Animator (Optional - cho animation)

### Components Boss c?n có:
- ? **BossController** script
- ? **NavMeshAgent** component
- ? **Collider** (Box/Capsule/Sphere)
- ?? **Animator** (Optional)
- ?? **Rigidbody** (Optional - n?u c?n physics)

---

## ?? B??C 1: CHU?N B? BOSS GAMEOBJECT

### 1.1 T?o Boss GameObject
```
1. Trong Scene, t?o Empty GameObject
   - Right click > Create Empty > ??t tên "Boss"
   
2. Ho?c dùng model 3D có s?n
   - Kéo model Boss vào Scene
```

### 1.2 Thêm Collider
```
1. Select Boss GameObject
2. Add Component > Box Collider (ho?c Capsule Collider)
3. ?i?u ch?nh size ?? v?a v?i model:
   - Center: (0, 1, 0)  // Tùy theo model
   - Size: (1, 2, 1)    // Tùy theo model
```

### 1.3 Thêm NavMeshAgent
```
1. Select Boss GameObject
2. Add Component > Nav Mesh Agent
3. ?? m?c ??nh settings (s? config sau)
```

### 1.4 Thêm BossController Script
```
1. Select Boss GameObject
2. Add Component > BossController
3. Ho?c kéo script vào GameObject
```

---

## ??? B??C 2: SETUP NAVMESH

### 2.1 Bake NavMesh cho Scene

```
1. M? NavMesh window:
   Window > AI > Navigation
   
2. Tab "Bake":
   - Agent Radius: 0.5
   - Agent Height: 2
   - Max Slope: 45
   - Step Height: 0.4
   
3. Click "Bake" ? cu?i window
   
4. Ki?m tra:
   - Vùng có th? ?i s? hi?n màu xanh
   - Boss ph?i ??ng trên vùng xanh này
```

### 2.2 ?ánh d?u Ground là Static
```
1. Select t?t c? Ground/Floor objects
2. Tick "Navigation Static" ? Inspector
3. Bake l?i NavMesh
```

---

## ?? B??C 3: C?U HÌNH BOSSCONTROLLER

### 3.1 Boss Stats
```
Select Boss > Inspector > BossController:

[Boss Stats]
?? Max Health: 1000          // Máu boss
?? Attack Damage: 30         // Sát th??ng m?i ?òn
?? Move Speed: 3.5           // T?c ?? di chuy?n
```

### 3.2 Combat Settings
```
[Combat Settings]
?? Detection Range: 15       // Phát hi?n player trong 15m
?? Attack Range: 2.5         // ?ánh player trong 2.5m
?? Attack Cooldown: 2        // H?i chiêu 2 giây
?? Max Chase Distance: 30    // ?u?i t?i ?a 30m
```

### 3.3 Attack Hitbox
```
[Attack Hitbox]
?? Attack Point: (kéo child object vào)
?? Attack Radius: 1.5
?? Player Layer: Player      // Ch?n layer c?a Player
```

### 3.4 References
```
[References]
?? Target: (kéo Player GameObject vào)
   
?? N?u ?? tr?ng, boss s? t? tìm GameObject có tag "Player"
```

### 3.5 Visual Feedback
```
[Visual Feedback]
?? Damage Color: Red (255, 0, 0, 255)
?? Damage Flash Duration: 0.1
```

---

## ?? B??C 4: SETUP ANIMATION (OPTIONAL)

### 4.1 Thêm Animator Component
```
1. Select Boss GameObject
2. Add Component > Animator
3. Assign Animator Controller vào field "Controller"
```

### 4.2 T?o Animator Parameters
```
M? Animator window (Ctrl+6), t?o Parameters:

[Parameters Tab]
?? Speed    (Float)    // T?c ?? di chuy?n
?? Attack   (Trigger)  // Trigger t?n công
?? Hit      (Trigger)  // Trigger nh?n ?òn
?? Death    (Trigger)  // Trigger ch?t
```

### 4.3 T?o Animation States
```
[States c?n có]
?? Idle
?? Walk/Run
?? Attack (1 ho?c nhi?u)
?? Hit/Hurt
?? Death
```

### 4.4 Setup Transitions
```
Idle ? Walk:
- Condition: Speed > 0.1
- Has Exit Time: ?
- Transition Duration: 0.1s

Walk ? Idle:
- Condition: Speed < 0.1
- Has Exit Time: ?
- Transition Duration: 0.1s

Any State ? Attack:
- Condition: Attack trigger
- Has Exit Time: ?

Attack ? Idle:
- Condition: None
- Has Exit Time: ?
- Exit Time: 0.9 (90% animation)

Any State ? Hit:
- Condition: Hit trigger
- Has Exit Time: ?

Any State ? Death:
- Condition: Death trigger
- Has Exit Time: ?
```

---

## ?? B??C 5: T?O ATTACK POINT

### 5.1 T?o Attack Point GameObject
```
1. Right click Boss GameObject > Create Empty
2. ??t tên "AttackPoint"
3. Position AttackPoint phía tr??c Boss:
   - Position: (0, 1, 1.5)  // Tùy theo model
   
4. Kéo AttackPoint vào field "Attack Point" c?a BossController
```

### 5.2 Visualize Attack Range (Optional)
```
1. Select AttackPoint
2. Add Component > Draw Gizmo (t? t?o script)
3. Ho?c dùng Gizmos c?a BossController (ch? hi?n khi select)
```

---

## ??? B??C 6: SETUP LAYER VÀ COLLISION

### 6.1 T?o Layer cho Player
```
1. Edit > Project Settings > Tags and Layers
2. Thêm Layer:
   - Layer 6: Player
   
3. Select Player GameObject
4. Set Layer = "Player"
```

### 6.2 C?u hình BossController
```
1. Select Boss GameObject
2. Inspector > BossController
3. Player Layer > Ch?n "Player"
```

### 6.3 C?u hình Player Tag
```
1. Select Player GameObject
2. Inspector > Tag > Ch?n "Player"
   
?? N?u không có tag "Player":
   - Inspector > Tag > Add Tag...
   - Thêm tag m?i: "Player"
   - Quay l?i Player object và set tag
```

### 6.4 Layer Collision Matrix (Optional)
```
1. Edit > Project Settings > Physics
2. Layer Collision Matrix:
   ? Boss có th? va ch?m v?i Player
   ? Boss có th? va ch?m v?i Ground
```

---

## ?? B??C 7: TEST BOSS

### 7.1 Test Detection Range
```
1. Play scene
2. Di chuy?n Player l?i g?n Boss
3. Ki?m tra:
   ? Boss chuy?n sang Chase khi Player trong vùng phát hi?n
   ? Boss quay m?t v? Player
   ? Boss di chuy?n theo Player
```

### 7.2 Test Attack
```
1. Di chuy?n Player vào t?m ?ánh
2. Ki?m tra:
   ? Boss d?ng l?i
   ? Boss trigger animation Attack
   ? Console log "Boss hit: Player" xu?t hi?n
```

### 7.3 Test Damage
```
1. T?o script test ?? g?i boss.TakeDamage(100)
2. Ki?m tra:
   ? Boss nh?p nháy màu ??
   ? Console log hi?n damage nh?n
   ? Boss chuy?n sang Chase (n?u ?ang Idle)
```

### 7.4 Test Return to Spawn
```
1. Cho Player ch?y xa Boss (> Max Chase Distance)
2. Ki?m tra:
   ? Boss quay v? v? trí spawn
   ? Boss h?i full máu khi v? spawn
   ? Boss chuy?n v? Idle
```

---

## ?? TROUBLESHOOTING

### ? Boss không di chuy?n
```
Ki?m tra:
1. ? NavMesh ?ã ???c bake ch?a?
2. ? Boss ??ng trên vùng NavMesh xanh?
3. ? NavMeshAgent component enabled?
4. ? Ground ?ã set Navigation Static?
5. ? Target (Player) ?ã ???c assign?
```

### ? Boss không t?n công
```
Ki?m tra:
1. ? Player trong Attack Range?
2. ? Player Layer ?úng?
3. ? Attack Point ?ã ???c assign?
4. ? Attack Cooldown ?ã h?t?
5. ? Animator có parameter "Attack"?
```

### ? Boss không nh?n damage
```
Ki?m tra:
1. ? ?ã g?i boss.TakeDamage(amount)?
2. ? Boss ch?a ch?t (isDead = false)?
3. ? Renderer có material v?i _Color property?
```

### ? Animation không ch?y
```
Ki?m tra:
1. ? Animator Controller ?ã assign?
2. ? Parameters tên ?úng (Speed, Attack, Hit, Death)?
3. ? Transitions ?ã setup ?úng?
4. ? Animation clips ?ã assign vào states?
```

### ? Boss quay v? spawn liên t?c
```
Ki?m tra:
1. ? Max Chase Distance > Detection Range
2. ? Spawn position c?a Boss h?p lý
3. ? NavMesh ?? l?n cho Boss di chuy?n
```

---

## ?? GIZMOS VISUALIZATION

Khi select Boss trong Scene view, b?n s? th?y:

```
?? Vòng tròn vàng   = Detection Range (phát hi?n player)
?? Vòng tròn ??     = Attack Range (t?n công player)
?? Vòng tròn tím    = Attack Hitbox (vùng damage)
?? Vòng tròn xanh   = Max Chase Distance (?u?i t?i ?a)
```

---

## ?? CHECKLIST HOÀN CH?NH

### Boss GameObject
- [ ] BossController script ?ã thêm
- [ ] NavMeshAgent component ?ã thêm
- [ ] Collider ?ã thêm và ?i?u ch?nh size
- [ ] Animator ?ã thêm (optional)

### NavMesh Setup
- [ ] Ground objects ?ã set Navigation Static
- [ ] NavMesh ?ã ???c bake
- [ ] Boss ??ng trên vùng NavMesh xanh

### BossController Configuration
- [ ] Max Health, Attack Damage, Move Speed ?ã set
- [ ] Detection Range, Attack Range ?ã set
- [ ] Attack Point ?ã t?o và assign
- [ ] Player Layer ?ã ch?n ?úng
- [ ] Target (Player) ?ã assign ho?c Player có tag "Player"

### Animation Setup (Optional)
- [ ] Animator Controller ?ã assign
- [ ] Parameters ?ã t?o (Speed, Attack, Hit, Death)
- [ ] Animation States ?ã t?o
- [ ] Transitions ?ã setup

### Player Setup
- [ ] Player có Tag "Player"
- [ ] Player có Layer "Player"
- [ ] Player có Collider

### Testing
- [ ] Boss phát hi?n và ?u?i Player
- [ ] Boss t?n công khi Player trong t?m
- [ ] Boss nh?n damage và flash màu ??
- [ ] Boss quay v? spawn khi Player ch?y xa
- [ ] Boss ch?t khi h?t máu

---

## ?? TIPS & TRICKS

### 1. T?i ?u hi?u su?t
```csharp
// N?u có nhi?u boss, t?ng update interval c?a NavMeshAgent
navMeshAgent.updateInterval = 0.2f; // Update 5 l?n/giây thay vì m?i frame
```

### 2. Thêm debug visualization
```csharp
// Thêm vào Update() ?? debug
void Update()
{
    Debug.DrawLine(transform.position, target.position, Color.red);
}
```

### 3. T?o Boss Variants
```
1. T?o Prefab t? Boss ?ã setup
2. T?o variants v?i stats khác:
   - MiniBoss: Health 500, Speed 5
   - MegaBoss: Health 2000, Speed 2
```

### 4. K?t h?p v?i Player Attack
```csharp
// Trong Player Attack script, g?i:
BossController boss = hit.GetComponent<BossController>();
if (boss != null)
{
    boss.TakeDamage(attackDamage);
}
```

---

## ?? H? TR?

N?u g?p v?n ??, ki?m tra:
1. Console logs ?? xem l?i
2. Inspector ?? xem references ?ã ?? ch?a
3. Scene view Gizmos ?? xem ranges
4. Animator window ?? ki?m tra transitions

**Chúc b?n t?o Boss thành công! ??**
