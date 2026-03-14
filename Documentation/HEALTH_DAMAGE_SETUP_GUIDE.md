# ?? HEALTH & DAMAGE SYSTEM SETUP GUIDE

## ?? TABLE OF CONTENTS
1. [Setup Player Health](#setup-player-health)
2. [Setup Player Attack](#setup-player-attack)
3. [Setup Boss to Damage Player](#setup-boss-to-damage-player)
4. [Create HP Bar UI](#create-hp-bar-ui)
5. [Testing](#testing)

---

## ?? STEP 1: SETUP PLAYER HEALTH

### 1.1 Add PlayerHealth Component
```
1. Select Player GameObject in Hierarchy
2. Add Component > PlayerHealth
3. Configure settings:
   ??????????????????????????????
   ? PlayerHealth               ?
   ??????????????????????????????
   ? Max Health: 100            ?
   ? Damage Color: Red          ?
   ? Damage Flash Duration: 0.1 ?
   ? Respawn Delay: 3           ?
   ??????????????????????????????
```

### 1.2 Create Player Layer
```
1. Edit > Project Settings > Tags and Layers
2. Add Layer:
   - Layer 3: Player (or any unused layer)
   
3. Select Player GameObject
4. Inspector > Layer > Select "Player"
```

---

## ?? STEP 2: SETUP PLAYER ATTACK

### 2.1 Add PlayerAttack Component
```
1. Select Player GameObject
2. Add Component > PlayerAttack
3. Configure settings:
   ??????????????????????????????
   ? PlayerAttack               ?
   ??????????????????????????????
   ? Attack Damage: 25          ?
   ? Attack Radius: 1.5         ?
   ??????????????????????????????
```

### 2.2 Create Attack Point
```
1. Right-click Player in Hierarchy > Create Empty
2. Name it "AttackPoint"
3. Set Position:
   - X: 0
   - Y: 1 (waist height)
   - Z: 1.5 (in front of player)
   
4. Drag AttackPoint to PlayerAttack > Attack Point field
```

### 2.3 Configure Damageable Layers
```
1. Select Player
2. Inspector > PlayerAttack > Damageable Layers
3. Check layers:
   ? Boss
   ? Enemy
   (Any layer you want to damage)
```

### 2.4 Add Animation Event
```
This will trigger damage during attack animation.

1. Open Animator window (Ctrl+6)
2. Select Attack_1 animation state
3. Click on the animation clip in Inspector
4. Find the Animation window (Window > Animation > Animation)
5. Scrub timeline to the "hit frame" (usually 50-70% of animation)
6. Click "Add Event" button
7. In Event Inspector:
   - Function: OnAttackHit
   - (This calls PlayerAttack.OnAttackHit())

8. Repeat for Attack_2 and Attack_3
```

---

## ?? STEP 3: SETUP BOSS TO DAMAGE PLAYER

### 3.1 Configure Boss Layer
```
1. Select Boss GameObject
2. Inspector > Layer > Select "Boss" (or create new layer)
```

### 3.2 Update Boss Player Layer
```
1. Select Boss GameObject
2. Inspector > BossController > Player Layer
3. Select "Player" layer
```

### 3.3 Add Animation Event to Boss Attack
```
1. Open Boss Animator
2. Select Boss Attack animation
3. Add Animation Event at hit frame
4. Function: DealDamageToPlayer
   (This calls BossController.DealDamageToPlayer())
```

---

## ?? STEP 4: CREATE HP BAR UI

### 4.1 Create Player HP Bar (Screen Space)

#### A. Create Canvas
```
1. Hierarchy > Right-click > UI > Canvas
2. Rename to "PlayerUI"
3. Inspector > Canvas:
   - Render Mode: Screen Space - Overlay
   - Pixel Perfect: ?
```

#### B. Create HP Bar Background
```
1. Right-click Canvas > UI > Image
2. Rename to "PlayerHealthBar_BG"
3. Inspector > Rect Transform:
   - Anchor: Bottom-Left
   - Position X: 150
   - Position Y: 50
   - Width: 200
   - Height: 20
   
4. Inspector > Image:
   - Color: Dark Gray (50, 50, 50, 255)
```

#### C. Create HP Bar Fill
```
1. Right-click PlayerHealthBar_BG > UI > Slider
2. Rename to "PlayerHealthSlider"
3. Delete "Handle Slide Area" child
4. Inspector > Slider:
   - Min Value: 0
   - Max Value: 1
   - Value: 1
   - Interactable: ? (unchecked)
   
5. Select "Fill" child object
6. Inspector > Image:
   - Color: Green (0, 255, 0, 255)
```

#### D. Add Health Text
```
1. Right-click PlayerHealthBar_BG > UI > Text
2. Rename to "HealthText"
3. Inspector > Rect Transform:
   - Anchor: Center
   - Position: (0, 0)
   
4. Inspector > Text:
   - Text: "100 / 100"
   - Font Size: 14
   - Alignment: Center
   - Color: White
```

#### E. Connect to PlayerHealth
```
1. Select Player GameObject
2. Inspector > PlayerHealth:
   - Health Bar: Drag PlayerHealthSlider here
   - Health Text: Drag HealthText here
```

---

### 4.2 Create Boss HP Bar (World Space)

#### A. Create World Space Canvas
```
1. Hierarchy > Right-click > UI > Canvas
2. Rename to "BossHealthBarCanvas"
3. Inspector > Canvas:
   - Render Mode: World Space
   
4. Inspector > Rect Transform:
   - Width: 200
   - Height: 30
   - Scale: (0.01, 0.01, 0.01)
   
5. Drag canvas as child of Boss GameObject
6. Set Position:
   - X: 0
   - Y: 3 (above boss head)
   - Z: 0
```

#### B. Create HP Bar Background
```
1. Right-click BossHealthBarCanvas > UI > Image
2. Rename to "BG"
3. Inspector > Rect Transform:
   - Anchor: Center
   - Width: 180
   - Height: 20
   
4. Inspector > Image:
   - Color: Black (0, 0, 0, 200)
```

#### C. Create HP Bar Slider
```
1. Right-click BG > UI > Slider
2. Rename to "HealthSlider"
3. Delete "Handle Slide Area"
4. Inspector > Slider:
   - Value: 1
   - Interactable: ?
   
5. Select "Fill" child
6. Inspector > Image:
   - Color: Red (255, 0, 0, 255)
```

#### D. Add Boss Name
```
1. Right-click BG > UI > Text
2. Rename to "BossName"
3. Inspector > Text:
   - Text: "Boss"
   - Font Size: 16
   - Alignment: Center
   - Color: White
```

#### E. Add BossHealthBarWorld Script
```
1. Select BossHealthBarCanvas
2. Add Component > BossHealthBarWorld
3. Inspector:
   - Boss: Drag Boss GameObject
   - Health Slider: Drag HealthSlider
   - Boss Name Text: Drag BossName
   - Offset: (0, 3, 0)
   - Billboard To Camera: ?
```

---

## ?? STEP 5: TESTING

### 5.1 Test Player Taking Damage
```
1. Play scene
2. Approach Boss
3. Let Boss attack Player
4. Check:
   ? Player health bar decreases
   ? Player flashes red
   ? Health text updates
   ? Player dies at 0 HP
   ? Player respawns after 3 seconds
```

### 5.2 Test Player Attacking Boss
```
1. Play scene
2. Attack Boss (Left Click)
3. Check Console:
   ? "Player hit Boss for 25 damage!"
   ? "Boss took 25 damage. Health: X/1000"
4. Check:
   ? Boss health bar decreases
   ? Boss flashes red
   ? Boss dies at 0 HP
```

### 5.3 Test Without Animation Events
```
If Animation Events are not set up yet:

1. Select Player
2. Inspector > PlayerAttack
3. Right-click component > Test Attack
4. This manually triggers attack damage
```

---

## ?? TROUBLESHOOTING

### ? Player not taking damage from Boss
```
Check:
1. ? Boss > Player Layer is set to "Player"
2. ? Player has PlayerHealth component
3. ? Player Layer is set to "Player"
4. ? Boss attack animation has Event: DealDamageToPlayer
5. ? Boss Attack Point is positioned correctly
```

### ? Boss not taking damage from Player
```
Check:
1. ? Player > Damageable Layers includes "Boss" layer
2. ? Boss Layer is set to "Boss"
3. ? Boss has Collider
4. ? Player attack animation has Event: OnAttackHit
5. ? Player Attack Point is positioned correctly
```

### ? Health bar not updating
```
Check:
1. ? PlayerHealth > Health Bar field is assigned
2. ? Slider Min = 0, Max = 1
3. ? Slider Fill Image is assigned
4. ? Canvas is active in scene
```

### ? Boss health bar not visible
```
Check:
1. ? BossHealthBarCanvas is child of Boss
2. ? Canvas Render Mode = World Space
3. ? Canvas Scale = (0.01, 0.01, 0.01)
4. ? BossHealthBarWorld component assigned
5. ? Camera layer can see UI layer
```

---

## ?? TIPS & TRICKS

### 1. Adjust Damage Values
```
Player Attack Damage: 25
Boss Attack Damage: 30

Boss Health: 1000
Player Health: 100

Time to kill Boss: 1000 / 25 = 40 hits
Time to kill Player: 100 / 30 = ~3-4 hits
```

### 2. Add Damage Numbers (Optional)
```csharp
// Create floating damage text when hit
public void ShowDamageNumber(float damage, Vector3 position)
{
    GameObject damageText = Instantiate(damageTextPrefab, position, Quaternion.identity);
    damageText.GetComponent<Text>().text = damage.ToString();
    Destroy(damageText, 1f);
}
```

### 3. Add Sound Effects
```csharp
// In TakeDamage method
AudioSource.PlayClipAtPoint(hitSound, transform.position);
```

### 4. Add Camera Shake
```csharp
// When taking big damage
CameraShaker.Shake(0.5f, 0.3f);
```

---

## ?? COMPLETE CHECKLIST

### Player Setup
- [ ] PlayerHealth component added
- [ ] PlayerAttack component added
- [ ] Attack Point created and assigned
- [ ] Player Layer set to "Player"
- [ ] Health Bar UI connected
- [ ] Animation Events added (OnAttackHit)

### Boss Setup
- [ ] Boss Layer set to "Boss"
- [ ] BossController > Player Layer = "Player"
- [ ] Boss Attack Point assigned
- [ ] Animation Events added (DealDamageToPlayer)
- [ ] Boss Health Bar UI created

### UI Setup
- [ ] Player HP Bar canvas created
- [ ] Boss HP Bar world canvas created
- [ ] Both health bars connected to scripts
- [ ] Health text displays correctly

### Testing
- [ ] Player can damage Boss
- [ ] Boss can damage Player
- [ ] Health bars update correctly
- [ ] Player respawns on death
- [ ] Boss dies and disappears

---

**Good luck with your combat system! ??**
