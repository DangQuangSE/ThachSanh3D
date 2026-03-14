# ? QUICK BOSS SETUP - 5 MINUTES

## ?? QUICK SETUP (No Animation Required)

### 1?? CREATE BOSS (30 seconds)
```
1. Drag Boss model into Scene
2. Add Component:
   - BossController ?
   - Nav Mesh Agent ?
   - Capsule Collider ?
```

### 2?? BAKE NAVMESH (1 minute)
```
1. Window > AI > Navigation
2. Select Ground objects
3. Tick "Navigation Static"
4. Tab "Bake" > Click "Bake"
```

### 3?? CONFIGURE BOSS (2 minutes)
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
?? Player Layer: [Select "Player"]

References:
?? Target: [Leave empty - auto find]
```

### 4?? SETUP PLAYER (1 minute)
```
1. Select Player GameObject
2. Inspector > Tag: "Player"
3. Inspector > Layer: "Player"
```

### 5?? CREATE ATTACK POINT (30 seconds)
```
1. Right click Boss > Create Empty
2. Name: "AttackPoint"
3. Position: (0, 1, 1.5)
4. Drag into Boss > Attack Point field
```

---

## ? DONE! TEST NOW

### Run Game and Test:
1. **Player far away** ? Boss Idle ?
2. **Player approaches** ? Boss chases ?
3. **Player in range** ? Boss attacks ?
4. **Player runs away** ? Boss returns to spawn ?

---

## ?? ADD ANIMATION (OPTIONAL)

### Setup Animation in 2 minutes:
```
1. Add Component > Animator
2. Assign Animator Controller
3. Create Parameters:
   - Speed (Float)
   - Attack (Trigger)
   - Hit (Trigger)
   - Death (Trigger)
4. Setup States: Idle, Walk, Attack, Hit, Death
5. Create Transitions with Exit Time
```

---

## ?? COMMON ERRORS

| Error | Fix |
|-------|-----|
| Boss not moving | Bake NavMesh, check Boss on blue area |
| Boss not attacking | Check Player Layer, Attack Point |
| Boss not taking damage | Call `boss.TakeDamage(amount)` |
| Animation not playing | Check Parameters names are correct |

---

## ?? RECOMMENDED SETTINGS

### Normal Boss:
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

## ?? SAMPLE CODE FOR TESTING

### Test Boss taking damage from Player:
```csharp
// Add to Player Attack script
void OnAttackHit()
{
    Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius);
    foreach (Collider hit in hits)
    {
        BossController boss = hit.GetComponent<BossController>();
        if (boss != null)
        {
            boss.TakeDamage(30); // Deal 30 damage
        }
    }
}
```

### Test Boss with button:
```csharp
// Create script TestBoss.cs
public class TestBoss : MonoBehaviour
{
    public BossController boss;
    
    void Update()
    {
        // Press K to deal 100 damage
        if (Input.GetKeyDown(KeyCode.K))
        {
            boss.TakeDamage(100);
        }
        
        // Press L to kill boss
        if (Input.GetKeyDown(KeyCode.L))
        {
            boss.TakeDamage(9999);
        }
    }
}
```

---

## ?? 1-MINUTE CHECKLIST

- [ ] Boss has BossController + NavMeshAgent + Collider
- [ ] NavMesh is baked
- [ ] Boss Stats are filled
- [ ] Player has Tag "Player"
- [ ] Player Layer selected in BossController
- [ ] AttackPoint created and assigned
- [ ] Test: Boss chases Player ?

**DONE! Boss is ready for battle! ????**
