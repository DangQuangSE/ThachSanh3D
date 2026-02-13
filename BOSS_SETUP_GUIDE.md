# ?? BOSS CONTROLLER SETUP GUIDE

## ?? TABLE OF CONTENTS
1. [System Requirements](#system-requirements)
2. [Prepare Boss GameObject](#prepare-boss-gameobject)
3. [Setup NavMesh](#setup-navmesh)
4. [Configure BossController](#configure-bosscontroller)
5. [Setup Animation (Optional)](#setup-animation-optional)
6. [Create Attack Point](#create-attack-point)
7. [Setup Layer and Collision](#setup-layer-and-collision)
8. [Test Boss](#test-boss)
9. [Troubleshooting](#troubleshooting)

---

## ? SYSTEM REQUIREMENTS

### Required Packages:
- ? Unity NavMesh (AI Navigation)
- ?? Animator (Optional - for animation)

### Boss Components Required:
- ? **BossController** script
- ? **NavMeshAgent** component
- ? **Collider** (Box/Capsule/Sphere)
- ?? **Animator** (Optional)
- ?? **Rigidbody** (Optional - if physics needed)

---

## ?? STEP 1: PREPARE BOSS GAMEOBJECT

### 1.1 Create Boss GameObject
```
1. In Scene, create Empty GameObject
   - Right click > Create Empty > Name it "Boss"
   
2. Or use existing 3D model
   - Drag Boss model into Scene
```

### 1.2 Add Collider
```
1. Select Boss GameObject
2. Add Component > Box Collider (or Capsule Collider)
3. Adjust size to fit model:
   - Center: (0, 1, 0)  // Depends on model
   - Size: (1, 2, 1)    // Depends on model
```

### 1.3 Add NavMeshAgent
```
1. Select Boss GameObject
2. Add Component > Nav Mesh Agent
3. Leave default settings (will configure later)
```

### 1.4 Add BossController Script
```
1. Select Boss GameObject
2. Add Component > BossController
3. Or drag script into GameObject
```

---

## ??? STEP 2: SETUP NAVMESH

### 2.0 Install AI Navigation Package (REQUIRED for Unity 6)

```
Unity 6 does NOT have built-in NavMesh anymore!
You MUST install the package first.

1. Open Package Manager:
   Window > Package Manager

2. Change dropdown (top-left):
   "Packages: In Project" ? "Unity Registry"

3. Search for: "AI Navigation"
   (Or full name: "com.unity.ai.navigation")

4. Click "Install" button

5. Wait for Unity to install (may take a few seconds)

6. Verify installation:
   ? GameObject > AI > NavMesh Surface (new menu appears)
   ? Component search shows "NavMesh Surface"
   ? Component search shows "NavMesh Agent"
```

---

### 2.1 Add NavMesh Surface to Ground

```
Unity 6 uses NavMesh Surface component instead of Bake window!

1. Select "Environment" GameObject (or Ground/Floor parent)

2. Click "Add Component" at bottom of Inspector

3. Search: "NavMesh Surface"

4. Add the component

5. Configure NavMesh Surface:
   ???????????????????????????????????
   ? NavMesh Surface                 ?
   ???????????????????????????????????
   ? Agent Type: Humanoid            ?
   ? Default Area: Walkable          ?
   ? Generate Links: ? (unchecked)  ?
   ?                                 ?
   ? Collect Objects: All            ? ? Important!
   ? Include Layers: Everything      ?
   ?                                 ?
   ? Use Geometry: Render Meshes     ?
   ?                                 ?
   ? ???????????????????????????     ?
   ? ?     ?? Bake             ?     ? ? Click here
   ? ???????????????????????????     ?
   ???????????????????????????????????

6. Click "Bake" button

7. Wait for baking to complete (progress bar appears)

8. Check result:
   ? Blue overlay appears on ground in Scene view
   ? Walkable areas show in light blue color
   ? Boss must stand on blue area
```

**Important Notes:**
- If "Collect Objects" = `All`: No need to mark objects as Static
- If "Collect Objects" = `Volume` or `Children`: Must mark objects as Navigation Static
- **Recommended:** Use `All` for simplicity

---

### 2.2 Configure Agent Type (Optional)

```
If you want custom agent size:

1. Window > AI > Navigation (legacy window)

2. Click "Agents" tab

3. Select "Humanoid" agent type

4. Adjust settings:
   - Agent Radius: 0.5
   - Agent Height: 2.0
   - Step Height: 0.4
   - Max Slope: 45

5. Go back to NavMesh Surface component

6. Click "Bake" again to apply changes
```

---

### 2.3 Verify NavMesh Setup

```
1. Select Boss GameObject in Hierarchy

2. Check Scene view:
   ? Boss should be standing on blue NavMesh area
   ? If not, move Boss to stand on blue area

3. Check Environment object:
   ? Should have "NavMesh Surface" component
   ? Component should show "NavMesh Data Asset" field filled

4. Test in Play mode:
   - Boss should be able to move on blue areas
   - Boss cannot move on areas without blue overlay
```

---

### 2.4 Troubleshooting NavMesh

**Problem: No blue overlay appears after Bake**
```
Solutions:
1. Check "Collect Objects" is set to "All"
2. Verify ground has Mesh Renderer component
3. Try marking ground as "Navigation Static":
   - Select Ground
   - Inspector > Static dropdown > Navigation Static
   - Bake again
```

**Problem: NavMesh Surface component not found**
```
Solution:
- AI Navigation package not installed
- Go back to Step 2.0 and install the package
```

**Problem: Boss falls through ground**
```
Solutions:
1. Ground must have Mesh Collider or Box Collider
2. Check ground Layer is not in "Ignore Raycast"
3. Verify Boss has NavMesh Agent component
```

---

## ?? STEP 3: CONFIGURE BOSSCONTROLLER

### 3.1 Boss Stats
```
Select Boss > Inspector > BossController:

[Boss Stats]
?? Max Health: 1000          // Boss health
?? Attack Damage: 30         // Damage per hit
?? Move Speed: 3.5           // Movement speed
```

### 3.2 Combat Settings
```
[Combat Settings]
?? Detection Range: 15       // Detect player within 15m
?? Attack Range: 2.5         // Attack player within 2.5m
?? Attack Cooldown: 2        // Cooldown 2 seconds
?? Max Chase Distance: 30    // Chase up to 30m
```

### 3.3 Attack Hitbox
```
[Attack Hitbox]
?? Attack Point: (drag child object here)
?? Attack Radius: 1.5
?? Player Layer: Player      // Select Player layer
```

### 3.4 References
```
[References]
?? Target: (drag Player GameObject here)
   
?? If left empty, boss will auto-find GameObject with tag "Player"
```

### 3.5 Visual Feedback
```
[Visual Feedback]
?? Damage Color: Red (255, 0, 0, 255)
?? Damage Flash Duration: 0.1
```

---

## ?? STEP 4: SETUP ANIMATION (OPTIONAL)

### 4.1 Add Animator Component
```
1. Select Boss GameObject
2. Add Component > Animator
3. Assign Animator Controller to "Controller" field
```

### 4.2 Create Animator Parameters
```
Open Animator window (Ctrl+6), create Parameters:

[Parameters Tab]
?? Speed    (Float)    // Movement speed
?? Attack   (Trigger)  // Attack trigger
?? Hit      (Trigger)  // Hit trigger
?? Death    (Trigger)  // Death trigger
```

### 4.3 Create Animation States
```
[Required States]
?? Idle
?? Walk/Run
?? Attack (1 or more)
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

## ?? STEP 5: CREATE ATTACK POINT

### 5.1 Create Attack Point GameObject
```
1. Right click Boss GameObject > Create Empty
2. Name it "AttackPoint"
3. Position AttackPoint in front of Boss:
   - Position: (0, 1, 1.5)  // Depends on model
   
4. Drag AttackPoint into "Attack Point" field of BossController
```

### 5.2 Visualize Attack Range

**Good news!** BossController already has **built-in Gizmos visualization**. No need to add any component!

#### How to see Attack Range visualization:

```
1. Select Boss GameObject in Hierarchy

2. Look at Scene view (NOT Game view)

3. You will see colored circles:
   ?? Yellow circle   = Detection Range (15m)
   ?? Red circle      = Attack Range (2.5m)
   ?? Purple circle   = Attack Hitbox (at AttackPoint position)
   ?? Blue circle     = Max Chase Distance (30m)

4. These circles are ONLY visible when Boss is selected
```

#### To see Gizmos in Scene view:

```
1. Make sure "Gizmos" button is ON in Scene view toolbar
   (Button at top-right of Scene view)

2. If circles are too small/big, click Gizmos dropdown:
   - Adjust "3D Icons" slider
   - Or enable/disable specific Gizmos
```

#### Code responsible for visualization:

Already included in BossController.cs:
```csharp
void OnDrawGizmosSelected()
{
    // Yellow circle: Detection Range
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, detectionRange);
    
    // Red circle: Attack Range
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, attackRange);
    
    // Purple circle: Attack Hitbox
    if (attackPoint != null)
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
    
    // Blue circle: Max Chase Distance
    Gizmos.color = Color.cyan;
    Gizmos.DrawWireSphere(transform.position, maxChaseDistance);
}
```

#### Optional: Always Show Gizmos (even when Boss is not selected)

If you want to see Gizmos even when Boss is NOT selected, change method name:

```csharp
// In BossController.cs
// Change from:
void OnDrawGizmosSelected()

// To:
void OnDrawGizmos()  // ? Remove "Selected" suffix
```

?? **Warning:** This will draw Gizmos for ALL bosses in scene, which may clutter the view.

---

## ??? STEP 6: SETUP LAYER AND COLLISION

### 6.1 Create Layer for Player
```
1. Edit > Project Settings > Tags and Layers
2. Add Layer:
   - Layer 6: Player
   
3. Select Player GameObject
4. Set Layer = "Player"
```

### 6.2 Configure BossController
```
1. Select Boss GameObject
2. Inspector > BossController
3. Player Layer > Select "Player"
```

### 6.3 Configure Player Tag
```
1. Select Player GameObject
2. Inspector > Tag > Select "Player"
   
?? If "Player" tag doesn't exist:
   - Inspector > Tag > Add Tag...
   - Add new tag: "Player"
   - Go back to Player object and set tag
```

### 6.4 Layer Collision Matrix (Optional)
```
1. Edit > Project Settings > Physics
2. Layer Collision Matrix:
   ? Boss can collide with Player
   ? Boss can collide with Ground
```

---

## ?? STEP 7: TEST BOSS

### 7.1 Test Detection Range
```
1. Play scene
2. Move Player close to Boss
3. Check:
   ? Boss changes to Chase when Player in detection range
   ? Boss faces Player
   ? Boss moves toward Player
```

### 7.2 Test Attack
```
1. Move Player into attack range
2. Check:
   ? Boss stops
   ? Boss triggers Attack animation
   ? Console log "Boss hit: Player" appears
```

### 7.3 Test Damage
```
1. Create test script to call boss.TakeDamage(100)
2. Check:
   ? Boss flashes red color
   ? Console log shows damage received
   ? Boss changes to Chase (if was Idle)
```

### 7.4 Test Return to Spawn
```
1. Make Player run far from Boss (> Max Chase Distance)
2. Check:
   ? Boss returns to spawn position
   ? Boss restores full health at spawn
   ? Boss changes to Idle
```

---

## ?? TROUBLESHOOTING

### ? Boss not moving
```
Check:
1. ? Is NavMesh baked?
2. ? Is Boss standing on blue NavMesh area?
3. ? Is NavMeshAgent component enabled?
4. ? Is Ground set to Navigation Static?
5. ? Is Target (Player) assigned?
```

### ? Boss not attacking
```
Check:
1. ? Is Player in Attack Range?
2. ? Is Player Layer correct?
3. ? Is Attack Point assigned?
4. ? Has Attack Cooldown finished?
5. ? Does Animator have "Attack" parameter?
```

### ? Boss not taking damage
```
Check:
1. ? Did you call boss.TakeDamage(amount)?
2. ? Is Boss not dead (isDead = false)?
3. ? Does Renderer have material with _Color property?
```

### ? Animation not playing
```
Check:
1. ? Is Animator Controller assigned?
2. ? Are Parameters named correctly (Speed, Attack, Hit, Death)?
3. ? Are Transitions setup correctly?
4. ? Are Animation clips assigned to states?
```

### ? Boss keeps returning to spawn
```
Check:
1. ? Is Max Chase Distance > Detection Range?
2. ? Is Boss spawn position reasonable?
3. ? Is NavMesh large enough for Boss movement?
```

---

## ?? GIZMOS VISUALIZATION

When Boss is selected in Scene view, you will see:

```
?? Yellow circle   = Detection Range (detect player)
?? Red circle      = Attack Range (attack player)
?? Purple circle   = Attack Hitbox (damage area)
?? Blue circle     = Max Chase Distance (max chase)
```

---

## ?? COMPLETE CHECKLIST

### Boss GameObject
- [ ] BossController script added
- [ ] NavMeshAgent component added
- [ ] Collider added and size adjusted
- [ ] Animator added (optional)

### NavMesh Setup
- [ ] Ground objects set to Navigation Static
- [ ] NavMesh baked
- [ ] Boss standing on blue NavMesh area

### BossController Configuration
- [ ] Max Health, Attack Damage, Move Speed set
- [ ] Detection Range, Attack Range set
- [ ] Attack Point created and assigned
- [ ] Player Layer selected correctly
- [ ] Target (Player) assigned or Player has tag "Player"

### Animation Setup (Optional)
- [ ] Animator Controller assigned
- [ ] Parameters created (Speed, Attack, Hit, Death)
- [ ] Animation States created
- [ ] Transitions setup

### Player Setup
- [ ] Player has Tag "Player"
- [ ] Player has Layer "Player"
- [ ] Player has Collider

### Testing
- [ ] Boss detects and chases Player
- [ ] Boss attacks when Player in range
- [ ] Boss takes damage and flashes red
- [ ] Boss returns to spawn when Player runs away
- [ ] Boss dies when health reaches zero

---

## ?? TIPS & TRICKS

### 1. Optimize Performance
```csharp
// If you have many bosses, increase NavMeshAgent update interval
navMeshAgent.updateInterval = 0.2f; // Update 5 times/second instead of every frame
```

### 2. Add Debug Visualization
```csharp
// Add to Update() for debugging
void Update()
{
    Debug.DrawLine(transform.position, target.position, Color.red);
}
```

### 3. Create Boss Variants
```
1. Create Prefab from setup Boss
2. Create variants with different stats:
   - MiniBoss: Health 500, Speed 5
   - MegaBoss: Health 2000, Speed 2
```

### 4. Integrate with Player Attack
```csharp
// In Player Attack script, call:
BossController boss = hit.GetComponent<BossController>();
if (boss != null)
{
    boss.TakeDamage(attackDamage);
}
```

---

## ?? SUPPORT

If you encounter issues, check:
1. Console logs to see errors
2. Inspector to verify all references are assigned
3. Scene view Gizmos to see ranges
4. Animator window to check transitions

**Good luck creating your Boss! ??**
