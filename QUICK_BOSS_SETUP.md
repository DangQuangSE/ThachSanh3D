# ? QUICK BOSS SETUP - 5 PHÚT

## ?? SETUP NHANH (Không c?n Animation)

### 1?? T?O BOSS (30 giây)
```
1. Kéo model Boss vào Scene
2. Add Component:
   - BossController ?
   - Nav Mesh Agent ?
   - Capsule Collider ?
```

### 2?? BAKE NAVMESH (1 phút)
```
1. Window > AI > Navigation
2. Select Ground objects
3. Tick "Navigation Static"
4. Tab "Bake" > Click "Bake"
```

### 3?? C?U HÌNH BOSS (2 phút)
```
Select Boss > Inspector:

Boss Stats:
?? Max Health: 1000
?? Attack Damage: 30
?? Move Speed: 3.5

Combat Settings:
?? Detection Range: 15
?? Attack Range: 2.5
?? Attack Cooldown: 2
?? Max Chase Distance: 30

Attack Hitbox:
?? Attack Radius: 1.5
?? Player Layer: [Ch?n "Player"]

References:
?? Target: [?? tr?ng - auto tìm]
```

### 4?? SETUP PLAYER (1 phút)
```
1. Select Player GameObject
2. Inspector > Tag: "Player"
3. Inspector > Layer: "Player"
```

### 5?? T?O ATTACK POINT (30 giây)
```
1. Right click Boss > Create Empty
2. Tên: "AttackPoint"
3. Position: (0, 1, 1.5)
4. Kéo vào Boss > Attack Point field
```

---

## ? XONG! TEST NGAY

### Ch?y Game và Test:
1. **Player ??ng xa** ? Boss Idle ?
2. **Player l?i g?n** ? Boss ?u?i theo ?
3. **Player trong t?m** ? Boss t?n công ?
4. **Player ch?y xa** ? Boss v? spawn ?

---

## ?? THÊM ANIMATION (OPTIONAL)

### Setup Animation trong 2 phút:
```
1. Add Component > Animator
2. Assign Animator Controller
3. T?o Parameters:
   - Speed (Float)
   - Attack (Trigger)
   - Hit (Trigger)
   - Death (Trigger)
4. Setup States: Idle, Walk, Attack, Hit, Death
5. T?o Transitions v?i Exit Time
```

---

## ?? L?I TH??NG G?P

| L?i | Fix |
|-----|-----|
| Boss không di chuy?n | Bake NavMesh, ki?m tra Boss trên vùng xanh |
| Boss không t?n công | Ki?m tra Player Layer, Attack Point |
| Boss không nh?n damage | G?i `boss.TakeDamage(amount)` |
| Animation không ch?y | Ki?m tra Parameters tên ?úng |

---

## ?? SETTINGS KHUY?N NGH?

### Boss Th??ng:
```
Health: 500-1000
Damage: 20-30
Speed: 3-4
Detection: 10-15
Attack Range: 2-3
```

### Mini Boss:
```
Health: 300-500
Damage: 15-20
Speed: 4-6
Detection: 8-12
Attack Range: 2
```

### Mega Boss:
```
Health: 2000-5000
Damage: 50-100
Speed: 2-3
Detection: 20-30
Attack Range: 3-5
```

---

## ?? CODE M?U ?? TEST

### Test Boss nh?n damage t? Player:
```csharp
// Thêm vào Player Attack script
void OnAttackHit()
{
    Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius);
    foreach (Collider hit in hits)
    {
        BossController boss = hit.GetComponent<BossController>();
        if (boss != null)
        {
            boss.TakeDamage(30); // Gây 30 damage
        }
    }
}
```

### Test Boss b?ng button:
```csharp
// T?o script TestBoss.cs
public class TestBoss : MonoBehaviour
{
    public BossController boss;
    
    void Update()
    {
        // Press K ?? gây 100 damage
        if (Input.GetKeyDown(KeyCode.K))
        {
            boss.TakeDamage(100);
        }
        
        // Press L ?? kill boss
        if (Input.GetKeyDown(KeyCode.L))
        {
            boss.TakeDamage(9999);
        }
    }
}
```

---

## ?? CHECKLIST 1 PHÚT

- [ ] Boss có BossController + NavMeshAgent + Collider
- [ ] NavMesh ?ã bake
- [ ] Boss Stats ?ã ?i?n
- [ ] Player có Tag "Player"
- [ ] Player Layer ?ã ch?n trong BossController
- [ ] AttackPoint ?ã t?o và assign
- [ ] Test: Boss ?u?i theo Player ?

**DONE! Boss s?n sàng chi?n ??u! ????**
