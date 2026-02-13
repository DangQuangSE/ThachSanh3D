# ?? VFX SLASH EFFECTS SETUP GUIDE FOR AXE

> **Project:** ThachSanh3D  
> **Purpose:** Add VFX effects when attacking with axe using Slash Effects FREE  
> **Setup Time:** 15-30 minutes  
> **Updated:** Added individual Rotation Offset for each attack

---

## ?? TABLE OF CONTENTS

1. [Preparation](#1-preparation)
2. [Create VFX Spawn Point](#2-create-vfx-spawn-point)
3. [Add VFX Manager Component](#3-add-vfx-manager-component)
4. [Setup VFX Prefabs](#4-setup-vfx-prefabs)
5. [Enable and Use Gizmos](#5-enable-and-use-gizmos)
6. [Adjust Rotation for Diagonal Slash](#6-adjust-rotation-for-diagonal-slash) ? **NEW UPDATE**
7. [Adjust Timing](#7-adjust-timing)
8. [Post-Processing (Optional)](#8-post-processing-optional)
9. [Troubleshooting](#9-troubleshooting)

---

## 1. PREPARATION

### ? Check Required Files:

Make sure you have the following files in your project:

```
Assets/
??? StarterAssets/ThirdPersonController/Scripts/
?   ??? AttackVFXManager.cs          ? (Main script)
?   ??? AdvancedAttackVFXManager.cs  ? (Advanced version)
?   ??? VFXSetupGuide.cs            ? (Setup guide)
?
??? ThachSanhGeneral/Quang/Matthew Guz/Slash Effects FREE/
    ??? Prefabs/                      ? (VFX prefabs)
    ??? Documentation/
        ??? Readme IMPORTANT.txt      ?
```

### ?? Available Slash Effects FREE Prefabs:

Navigate to: `Assets/ThachSanhGeneral/Quang/Matthew Guz/Slash Effects FREE/Prefabs`

You'll find VFX prefabs like:
- `Slash_01` - Horizontal slash
- `Slash_02` - Diagonal slash
- `Slash_03` - Heavy slash
- `Slash_04` - Spin slash
- `Slash_05` - Ultimate slash
- ... and many more

---

## 2. CREATE VFX SPAWN POINT

VFX Spawn Point is where the effect will appear (usually at the axe blade tip).

### Step 1: Find Weapon in Hierarchy

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
                                  ?? Weapon (Axe) ? FIND THIS OBJECT
```

### Step 2: Create Empty GameObject

1. **Right-click** on `Weapon` (or axe holder object)
2. Select **Create Empty**
3. Name it: `VFX_SpawnPoint`

### Step 3: Position the Spawn Point

1. **Select** `VFX_SpawnPoint` in Hierarchy
2. In **Scene view**, move it to:
   - **Axe blade tip** (for best slash effect)
   - Or **middle of blade** (for wider VFX)

3. **Recommended Transform:**
   ```
   Position: (0.5, 0, 0)  // Offset from weapon pivot
   Rotation: (0, 0, 0)    // Keep original or rotate to slash direction
   Scale: (1, 1, 1)       // Keep original
   ```

> ?? **Tip:** Enable **Gizmos** to easily see spawn point position (guide in step 5)

---

## 3. ADD VFX MANAGER COMPONENT

### Step 1: Select Player GameObject

1. In **Hierarchy**, find and select **Player** GameObject (or PlayerArmature)
2. Make sure this object has **Animator** component

### Step 2: Add Component

1. In **Inspector**, click **Add Component**
2. Search: `AttackVFXManager`
3. Click to add component

> ?? **Note:** For multiple VFX per attack or advanced features, use `AdvancedAttackVFXManager` instead

---

## 4. SETUP VFX PREFABS

After adding `AttackVFXManager` component, you'll see these fields in Inspector:

### ?? VFX Prefabs - Slash Effects

Drag VFX prefabs into corresponding slots:

| Slot | Recommended VFX | Description |
|------|----------------|-------------|
| **Attack 1 VFX** | `Slash_01` or `Slash_02` | First attack (usually horizontal) |
| **Attack 2 VFX** | `Slash_03` or `Slash_04` | Second attack (diagonal slash) |
| **Attack 3 VFX** | `Slash_05` or `Slash_06` | Combo finisher (strongest) |
| **Ultimate VFX** | `Slash_05` or largest VFX | Ultimate skill |

**How to drag prefab:**
1. Open folder: `Assets/ThachSanhGeneral/Quang/Matthew Guz/Slash Effects FREE/Prefabs`
2. **Drag** prefab into corresponding slot in Inspector

### ?? VFX Spawn Settings

| Field | Recommended Value | Description |
|-------|------------------|-------------|
| **VFX Spawn Point** | ? Drag `VFX_SpawnPoint` GameObject | VFX spawn position |
| **Spawn Offset** | `(0, 0, 0)` | Additional offset from spawn point |
| **VFX Scale** | `1.0` - `3.0` | VFX size (increase if too small) |
| **VFX Lifetime** | `2.0` | VFX duration (seconds) |

### ?? VFX Rotation Per Attack ? **NEW**

| Field | Recommended Value | Description |
|-------|------------------|-------------|
| **Attack 1 Rotation Offset** | `(0, 0, 0)` | Horizontal slash |
| **Attack 2 Rotation Offset** | `(0, 0, 45)` or `(0, 90, 0)` | **Diagonal slash** - Try different values! ? |
| **Attack 3 Rotation Offset** | `(0, 0, -45)` or `(0, 0, 0)` | Reverse diagonal or horizontal |
| **Ultimate Rotation Offset** | `(0, 0, 0)` | Customize based on animation |

### ?? Timing Settings

| Field | Recommended Value | Description |
|-------|------------------|-------------|
| **Attack 1 Spawn Time** | `0.4` | VFX spawns at 40% of animation |
| **Attack 2 Spawn Time** | `0.4` | VFX spawns at 40% of animation |
| **Attack 3 Spawn Time** | `0.4` | VFX spawns at 40% of animation |
| **Ultimate Spawn Time** | `0.5` | VFX spawns at 50% of animation |

> ?? **Explanation:** `0.4` = 40% of animation, meaning VFX spawns when animation is 40% complete

### ??? VFX Playback Settings

| Field | Recommended Value | Description |
|-------|------------------|-------------|
| **Auto Play Particle Systems** | ? Enabled | Auto-play VFX on spawn |
| **Use Weapon Rotation** | ? Enabled | VFX rotates with weapon (or disable for world rotation) |

### ?? Debug

| Field | Recommended Value | Description |
|-------|------------------|-------------|
| **Show Debug Logs** | ? Enable (when testing) | Show logs in Console |
| **Show Detailed Debug** | ? Disable (after testing) | Detailed debugging |
| **Show Rotation Gizmos** | ? Enable | **Show direction arrows in Scene** ? |

---

## 5. ENABLE AND USE GIZMOS

### ? Why can't I see Gizmos?

Possible reasons:
1. ? Haven't selected Player GameObject
2. ? Gizmos disabled in Scene view
3. ? VFX prefabs not assigned

### ? How to enable Gizmos:

#### Step 1: Enable Gizmos in Scene View

Look at top-right corner of **Scene view**, you'll see toolbar:

```
[2D] [Shaded] [Gizmos ?] [??] [??] ...
                  ?
            CLICK HERE
```

1. Click **Gizmos** button
2. Make sure it's **ENABLED** (bright, not grayed out)

#### Step 2: Select Player GameObject

1. In **Hierarchy**, click select **Player** (object with `AttackVFXManager` component)
2. Scene view will auto-focus on Player

#### Step 3: Check Show Rotation Gizmos

1. In **Inspector** of `AttackVFXManager`
2. Under **Debug** ? **Show Rotation Gizmos** must be ? **ENABLED**

### ?? What will Gizmos show?

When setup correctly, you'll see in **Scene view**:

```
                    VFX_SpawnPoint
                         ?
    ???????????????????????????
    ?      ?? Red sphere      ? ? Spawn position
    ?                         ?
    ?  ?? ??????             ? ? BLUE arrow = original weapon direction
    ?  ?? ????               ? ? GREEN arrow = Attack 1
    ?  ?? ????               ? ? YELLOW arrow = Attack 2 (diagonal) ?
    ?  ?? ????               ? ? CYAN arrow = Attack 3
    ?  ?? ????               ? ? PURPLE arrow = Ultimate
    ?                         ?
    ???????????????????????????
```

### ?? Color meanings:

| Color | Meaning |
|-------|---------|
| ?? **Red** | VFX spawn position (sphere + line) |
| ?? **Blue** | Weapon's original forward direction (no rotation offset) |
| ?? **Green** | VFX direction for **Attack 1** |
| ?? **Yellow** | VFX direction for **Attack 2** (important for diagonal slash!) |
| ?? **Cyan** | VFX direction for **Attack 3** |
| ?? **Purple** | VFX direction for **Ultimate** |

> ?? **Tip:** If you still don't see Gizmos:
> 1. Zoom closer to Player in Scene view
> 2. Check if `VFX_SpawnPoint` is assigned to `VFX Spawn Point` slot
> 3. Check if at least 1 VFX prefab is assigned (Attack 1, 2, 3, or Ultimate)

---

## 6. ADJUST ROTATION FOR DIAGONAL SLASH ? **NEW UPDATE**

### ?? Problem: VFX direction is wrong for diagonal slash

When animation does diagonal slash, VFX needs rotation to match slash direction. **AttackVFXManager** has been updated to support **individual Rotation Offset for each attack**.

### ?? Understanding Rotation Offset (X, Y, Z):

```
        Y (Yaw)
         ?
         ?      Z (Roll)
         ?     ?
         ?   ?
         ? ?
         ???????????? X (Pitch)
```

| Axis | Name | Description | Example Use |
|------|------|-------------|-------------|
| **X** | Pitch | Rotate up/down (tilt forward/back) | Slash from top to bottom |
| **Y** | Yaw | Rotate left/right (turn horizontally) | **Change VFX direction sideways** ? |
| **Z** | Roll | Rotate tilt (roll) | Diagonal slash angle |

### ?? Common Rotation Values:

#### 1. HORIZONTAL Slash (left ? right or right ? left)
```
Rotation Offset: (0, 0, 0)
```

#### 2. VFX flying from outside in ? Rotate 90° on Y
```
Rotation Offset: (0, 90, 0)   ? USE THIS FOR YOUR PROBLEM! ?
or
Rotation Offset: (0, -90, 0)  ? Try if opposite direction
```

#### 3. DIAGONAL Slash from top-left ? bottom-right ?
```
Rotation Offset: (0, 0, 45)
or
Rotation Offset: (0, 0, 60)   ? Steeper angle
```

#### 4. DIAGONAL Slash from top-right ? bottom-left ?
```
Rotation Offset: (0, 0, -45)
or
Rotation Offset: (0, 0, -60)
```

#### 5. VERTICAL Slash (top ? down) ?
```
Rotation Offset: (0, 0, 90)
or
Rotation Offset: (90, 0, 0)   ? Try both, see which looks better
```

#### 6. VFX reversed direction ? Rotate 180°
```
Rotation Offset: (0, 180, 0)  ? Completely reverse
```

### ?? Step-by-step adjustment:

#### Step 1: Identify the problem

Based on your screenshot: **VFX flying diagonally from outside in** instead of **horizontal slash**

? **Solution:** Rotate VFX 90° on Y axis

#### Step 2: Enter values in Inspector

1. **Select Player** in Hierarchy
2. In `AttackVFXManager` ? **VFX Rotation Per Attack**
3. Adjust **Attack 3 Rotation Offset** (or whichever attack has the issue):

```
Try values in this order:

Try 1:
X: 0
Y: 90    ? START WITH THIS VALUE!
Z: 0

If wrong direction, try:
X: 0
Y: -90   ? Or this value
Z: 0

If still wrong, try:
X: 0
Y: 180  ? Reverse
Z: 0

Or combine:
X: 0
Y: 45   ? Diagonal angle
Z: 45
```

#### Step 3: Check Gizmos

1. In **Scene view**, observe **colored arrow**:
   - ?? Yellow = Attack 2
   - ?? Cyan = Attack 3
   - etc.

2. Arrow must **point in slash direction** of animation

3. Adjust X, Y, Z values until arrow matches slash direction

#### Step 4: Test in Play mode

1. **Play** game
2. **Attack** to see VFX
3. **Check:**
   - ? VFX points in correct slash direction?
   - ? VFX looks good and natural?
   - ? If wrong, go back to Step 2 and try different values

### ?? Quick Reference for Common Issues:

| Issue | Recommended Rotation Offset |
|-------|---------------------------|
| VFX flying from outside in | `(0, 90, 0)` or `(0, -90, 0)` |
| VFX reversed direction | `(0, 180, 0)` |
| VFX diagonal slash 45° | `(0, 0, 45)` |
| VFX vertical slash | `(90, 0, 0)` or `(0, 0, 90)` |

### ?? Example Setup for 3-hit Combo:

```
Attack 1: Horizontal slash left ? right
  ? Rotation Offset: (0, 0, 0)

Attack 2: Diagonal slash top-right ? bottom-left
  ? Rotation Offset: (0, 0, -45)
  
Attack 3: Horizontal slash right ? left (YOUR VFX flying wrong)
  ? Rotation Offset: (0, 90, 0)   ? FIX FOR YOUR PROBLEM!

Ultimate: Jump up + thrust down
  ? Rotation Offset: (90, 0, 0)
```

### ?? If still wrong direction:

#### Try 1: Disable "Use Weapon Rotation"
```
AttackVFXManager:
?? VFX Playback Settings:
?   ?? Use Weapon Rotation: ? DISABLE
?
?? VFX Rotation Per Attack:
    ?? (Now uses world rotation, easier to control)
```

#### Try 2: Adjust VFX_SpawnPoint rotation
```
1. Select VFX_SpawnPoint in Hierarchy
2. Rotate this GameObject's Rotation in Inspector
3. Combine with Rotation Offset in script
```

#### Try 3: Try all axes
```
Try in sequence:
(90, 0, 0)
(0, 90, 0)
(0, 0, 90)
(-90, 0, 0)
(0, -90, 0)
(0, 0, -90)
(180, 0, 0)
(0, 180, 0)
(0, 0, 180)
```

---

## 7. ADJUST TIMING

### ?? What is Spawn Time?

**Spawn Time** = % of animation when VFX appears
- `0.0` = VFX spawns at animation start
- `0.5` = VFX spawns at middle of animation
- `1.0` = VFX spawns at animation end

### ?? Goal: VFX spawns exactly when axe strikes

#### Step 1: Determine exact timing

1. **Open** Window ? Animation
2. **Select** animation clip (Attack_1, Attack_2, Attack_3)
3. **Play** animation and observe:
   - When does axe start swing?
   - When does axe hit target (strike point)?
4. **Remember** that moment (e.g., 0.4 seconds in 1-second animation = 40%)

#### Step 2: Adjust in Inspector

1. Select **Player** in Hierarchy
2. In `AttackVFXManager` ? **Timing Settings**
3. Adjust values:

| Animation | Recommended Spawn Time | Description |
|-----------|----------------------|-------------|
| Attack 1 | `0.3` - `0.5` | Usually spawns mid-swing |
| Attack 2 | `0.3` - `0.5` | Depends on animation |
| Attack 3 | `0.4` - `0.6` | Heavy attacks usually swing slower |
| Ultimate | `0.5` - `0.7` | Spawn when falling or on impact |

#### Step 3: Test and fine-tune

1. **Play** game
2. **Attack** and observe:
   - VFX spawns **TOO EARLY** ? Increase Spawn Time (e.g., `0.4` ? `0.5`)
   - VFX spawns **TOO LATE** ? Decrease Spawn Time (e.g., `0.4` ? `0.3`)
3. **Repeat** until perfect

### ?? Specific Example:

```
Animation Attack_1 (horizontal slash):
- Total duration: 1 second
- Axe starts swing: 0.2 seconds ? 20%
- Axe strikes (peak): 0.4 seconds ? 40% ?
- Animation ends: 1.0 seconds ? 100%

? Spawn Time should be: 0.4 or 0.3-0.5
```

> ?? **Tip:** Enable `Show Debug Logs` to see in Console when VFX spawns, helps debug timing easier.

---

## 8. POST-PROCESSING (OPTIONAL)

Make VFX look better with Bloom and Ambient Occlusion effects.

### ?? Step 1: Install Post Processing

1. Open **Window** ? **Package Manager**
2. Search: `Post Processing`
3. Click **Install**

### ?? Step 2: Create Layer

1. Top-right of Unity ? **Layers** ? **Edit Layers**
2. Add new layer: `PostProcessing`

### ?? Step 3: Setup Main Camera

1. Select **Main Camera** in Hierarchy
2. **Add Component** ? `Post Processing Layer`
3. In component:
   - **Layer**: Select `PostProcessing`
   - **Trigger**: Select `Main Camera` (or leave None)

### ?? Step 4: Create Post Processing Volume

1. **Hierarchy** ? Right-click ? **Create Empty**
2. Name it: `PP Volume`
3. **Add Component** ? `Post Processing Volume`
4. In component:
   - ? **Is Global**: ENABLE
   - **Profile**: Click **New**

### ? Step 5: Add Effects

1. In `Post Processing Volume` ? Click **Add effect...**
2. Add **Ambient Occlusion**:
   - ? **Intensity**: ENABLE, set value = `1`
3. Click **Add effect...** again
4. Add **Bloom**:
   - ? **Intensity**: ENABLE, set value = `5`
   - ? **Soft Knee**: ENABLE
   - ? **Clamp**: ENABLE
   - ? **Diffusion**: ENABLE

### ? Result:

VFX will have glow effect (Bloom) and better shadows (AO).

> ?? **Note:** If Post Processing doesn't work in build, try deleting and setting up from scratch (according to Slash Effects Readme file).

---

## 9. TROUBLESHOOTING

### ? VFX doesn't spawn

#### Causes & Solutions:

1. **VFX prefabs not assigned**
   - ? Check in Inspector ? VFX Prefabs
   - ? Drag prefab into slots (Attack 1, 2, 3, Ultimate)

2. **VFX Spawn Point not set**
   - ? Check `VFX Spawn Point` has GameObject `VFX_SpawnPoint`
   - ? If not, drag `VFX_SpawnPoint` from Hierarchy into this slot

3. **Animation state name mismatch**
   - ? Open Animator Controller
   - ? Check state names: `Attack_1`, `Attack_2`, `Attack_3`, `UntimateAttack`
   - ? If different, modify in `AttackVFXManager.cs` script

4. **Component not added**
   - ? Check Player has `AttackVFXManager` component

5. **Animator not set**
   - ? Player must have `Animator` component

### ? VFX spawns but not visible

1. **VFX too small**
   - ? Increase `VFX Scale` (e.g., from 1 ? 2 or 3)

2. **VFX spawns far from camera**
   - ? Check position of `VFX_SpawnPoint`
   - ? Adjust `Spawn Offset`

3. **VFX destroyed too quickly**
   - ? Increase `VFX Lifetime` (e.g., from 2 ? 5 seconds)

4. **Particle System not playing**
   - ? Check `Auto Play Particle Systems` is ENABLED
   - ? See Console for "Playing ParticleSystem" log

### ? VFX wrong direction (important!) ? **NEW UPDATE**

#### Solution 1: Adjust Rotation Offset
```
For "VFX flying from outside in" problem:
Attack Rotation Offset:
- X: 0
- Y: 90    ? TRY THIS VALUE FIRST!
- Z: 0

If still wrong, try in sequence:
(0, -90, 0)
(0, 180, 0)
(0, 45, 0)
(45, 0, 0)
(0, 0, 45)
