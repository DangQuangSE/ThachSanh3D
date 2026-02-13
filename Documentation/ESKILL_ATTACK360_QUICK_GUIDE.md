# ?? E SKILL (ATTACK360) - QUICK SETUP GUIDE

> **Feature:** 360-degree spinning slash attack with VFX effects  
> **Keybind:** E Key  
> **Cooldown:** 8 seconds (configurable)  
> **Setup Time:** 15-20 minutes

---

## ?? WHAT IS E SKILL?

**E Skill (Attack360)** is a special combat ability that:
- ? Performs a 360-degree spinning slash attack
- ? Has an 8-second cooldown to prevent spam
- ? Spawns circular VFX effects during the attack
- ? Can only be used while grounded
- ? Blocks movement during execution
- ? Works independently from the 3-hit combo system

---

## ?? QUICK SETUP (5 STEPS)

### STEP 1: Input Configuration ??

1. Open `StarterAssets.inputactions` file
2. Add new action:
   - Name: `ESkill`
   - Type: Button
   - Binding: `<Keyboard>/e`
3. Save file

**? Expected Result:** Pressing E key will be detected by the system

---

### STEP 2: Animator Setup ??

1. Open Player's **Animator Controller**
2. Create **Parameter**:
   - Type: Trigger
   - Name: `ESkill`
3. Create **Animation State**:
   - Name: `Attack360`
   - Motion: Your 360 attack animation
4. Create **Transitions**:
   ```
   Any State ? Attack360
   - Condition: ESkill
   - Has Exit Time: NO
   - Transition Duration: 0.1
   
   Attack360 ? Idle
   - Has Exit Time: YES
   - Exit Time: 0.95
   - Transition Duration: 0.2
   ```

**? Expected Result:** E key triggers Attack360 animation

---

### STEP 3: VFX Assignment ??

1. Select **Player** GameObject
2. Find **AttackVFXManager** component
3. Locate **"VFX Prefabs - E Skill Effects"** section
4. Assign VFX:
   - **E Skill VFX:** Drag `Slash_07` or circular slash prefab
   - **E Skill Spawn Time:** `0.3` (30% into animation)
   - **E Skill Rotation Offset:** `(0, 0, 0)` (no rotation for circular VFX)

**Recommended VFX Prefabs:**
- `Slash_07` - Large circular slash
- `Slash_08` - Explosive 360° effect
- Any circular/wide slash VFX

**? Expected Result:** VFX spawns during Attack360 animation

---

### STEP 4: Controller Settings ??

In **ThirdPersonController** component:

| Setting | Value | Purpose |
|---------|-------|---------|
| **E Skill Cooldown** | `8.0` | Seconds between uses |
| **E Skill Enabled** | ? | Turn feature on/off |

**? Expected Result:** 8-second cooldown prevents spamming E Skill

---

### STEP 5: Test & Adjust ??

1. **Enter Play Mode**
2. **Press E** ? Attack360 animation plays
3. **Check:**
   - VFX spawns at weapon?
   - VFX size appropriate? (should be larger than normal attacks)
   - VFX timing matches animation peak?
   - Cooldown prevents immediate re-use?

**Adjust if needed:**
- **VFX too small?** ? Increase `VFX Scale` to 2.5-4.0
- **VFX timing off?** ? Adjust `E Skill Spawn Time`
- **VFX wrong direction?** ? Adjust `E Skill Rotation Offset`

**? Expected Result:** Smooth, visually impressive 360° attack

---

## ?? VISUAL CONFIGURATION

### VFX Scale Recommendations

```
Normal Attacks:  1.5 - 2.0
E Skill:         2.5 - 4.0  ? Much larger!
Ultimate:        3.0 - 5.0
```

**Why larger?**  
E Skill covers 360° area, needs bigger VFX for visual impact.

---

### Spawn Timing Guide

```
Attack360 Animation Timeline:

0%      20%     40%     60%     80%     100%
?       ?       ?       ?       ?       ?
Start   Wind    SLASH!  Spin    Finish  End
        up      ?
                Spawn VFX here (30-40%)
```

**Recommended:** `0.3` - `0.4` (30-40% into animation)

---

## ?? ADVANCED CUSTOMIZATION

### Multiple VFX Effects

Want multiple slashes? Create array of spawn points:

```
Weapon (Axe_Attack)
?? VFX_SpawnPoint_Front    // 0°
?? VFX_SpawnPoint_Right    // 90°
?? VFX_SpawnPoint_Back     // 180°
?? VFX_SpawnPoint_Left     // 270°
```

Then spawn VFX at all points simultaneously (requires code modification).

---

### Cooldown Adjustment

Change cooldown in `ThirdPersonController`:

```csharp
Combat feel preference:

Fast-paced:  4.0 - 6.0 seconds
Balanced:    8.0 seconds (default)
Strategic:   10.0 - 15.0 seconds
```

---

### VFX Color Theming

Choose VFX that matches your character theme:

| Theme | Recommended VFX Color |
|-------|----------------------|
| **Ice/Frost** | Blue/Cyan slashes |
| **Fire** | Orange/Red slashes |
| **Magic** | Purple/Violet slashes |
| **Nature** | Green slashes |
| **Holy** | White/Gold slashes |

---

## ?? TROUBLESHOOTING

### ? E Key Does Nothing

**Check:**
1. Input Actions saved? ? Re-save `StarterAssets.inputactions`
2. ESkill action exists? ? Open Input Actions and verify
3. E key bound? ? Check binding shows `<Keyboard>/e`

**Fix:** Re-create input binding and save

---

### ? Animation Plays But No VFX

**Check:**
1. VFX prefab assigned? ? Drag prefab to E Skill VFX slot
2. VFX too small? ? Increase VFX Scale to 3.0
3. Spawn time wrong? ? Try values: 0.2, 0.3, 0.4, 0.5

**Fix:** Assign prefab and adjust timing

---

### ? Can Spam E Skill Without Cooldown

**Check:**
1. Cooldown = 0? ? Set to 8.0 seconds
2. E Skill Enabled = false? ? Enable checkbox

**Fix:** Set cooldown to 8.0 in ThirdPersonController

---

### ? "E Skill on cooldown!" Message Spam

**This is normal!** Message appears when:
- You press E while cooldown is active
- Shows remaining time: "E Skill on cooldown! 5.2s remaining"

**Not an error** - just feedback that skill isn't ready yet.

---

## ?? E SKILL VS OTHER ATTACKS

| Feature | Normal Attack | E Skill | Ultimate |
|---------|--------------|---------|----------|
| **Trigger** | Left Click | E Key | R Key |
| **Cooldown** | 0.5s | 8s | 15s |
| **Type** | 3-hit combo | Single 360° | Special move |
| **VFX Size** | Medium | Large | Huge |
| **Use Case** | Primary combat | AOE clear | Boss finisher |

---

## ? COMPLETION CHECKLIST

- [ ] Input Actions configured (E key)
- [ ] Animator parameter added (ESkill trigger)
- [ ] Animation state created (Attack360)
- [ ] Transitions configured (Any State ? Attack360 ? Idle)
- [ ] VFX prefab assigned
- [ ] VFX timing adjusted (0.3-0.4)
- [ ] VFX scale appropriate (2.5-4.0)
- [ ] Cooldown works (8 seconds)
- [ ] Tested in Play Mode
- [ ] No console errors

---

## ?? CONTROLS SUMMARY

After setup, your controls are:

| Key | Action | Cooldown |
|-----|--------|----------|
| **Left Click** | 3-hit combo attack | 0.5s |
| **E** | 360° spin attack (E Skill) | 8s |
| **R** | Ultimate attack | 15s |
| **Q** | Shield/Protect | None |

---

## ?? RELATED GUIDES

- **Main VFX Guide:** `HUONG_DAN_THEM_VFX_SLASH_EFFECTS.md`
- **Ultimate Skill Guide:** `ULTIMATE_SKILL_IMPLEMENTATION.md`
- **Combat System:** See `ThirdPersonController.cs` comments

---

## ?? STILL NEED HELP?

1. **Enable Debug:**
   - Add `AttackSystemDebugger` component
   - Enable "Show On Screen Debug"
   - Watch for E Skill status in top-left

2. **Check Console:**
   - Look for: "E Skill (Attack360) activated!"
   - Errors? Read message carefully

3. **Verify Files:**
   - `StarterAssetsInputs.cs` - Has `eskill` field?
   - `ThirdPersonController.cs` - Has `HandleESkill()` method?
   - `AttackVFXManager.cs` - Has `eskillVFX` field?

4. **Re-build Project:**
   - Sometimes Unity needs a refresh
   - Edit ? Project Settings ? Player
   - Click any setting to force recompile

---

**Created:** December 2024  
**Version:** 1.0  
**Maintained By:** ThachSanh3D Development Team

?? **Enjoy your 360° spin attack!** ??
