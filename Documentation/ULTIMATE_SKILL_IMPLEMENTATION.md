# Ultimate Skill Implementation Guide

## 📋 Table of Contents
1. [Overview](#overview)
2. [Features](#features)
3. [File Structure](#file-structure)
4. [Implementation Steps](#implementation-steps)
5. [Code Breakdown](#code-breakdown)
6. [Unity Editor Setup](#unity-editor-setup)
7. [Testing & Debugging](#testing--debugging)
8. [Troubleshooting](#troubleshooting)

---

## 🎯 Overview

This document details the complete implementation of the **Ultimate Skill System** for the Third Person Controller. The system allows players to execute a powerful special attack with a cooldown mechanism, integrated seamlessly with the existing combo attack system.

**Key Features:**
- ⌨️ **Input:** Press `R` key to activate
- ⏱️ **Cooldown:** 15 seconds (configurable)
- 🎬 **Animation:** Mixamo animation support
- 🚫 **Restrictions:** Can only be used while grounded
- 🔄 **Integration:** Works alongside combo attack system
- 📊 **Debug:** Real-time on-screen monitoring

---

## ✨ Features

### Core Functionality
- **Cooldown System:** Prevents spam and adds strategic gameplay
- **State Management:** Tracks ultimate readiness and execution
- **Animation Integration:** Smooth transition to/from ultimate animation
- **Movement Lock:** Player cannot move during ultimate execution
- **Root Motion Support:** Animation can move the character
- **Attack Reset:** Cancels ongoing combo when ultimate is triggered
- **Jump Boost:** Player jumps into the air when ultimate activates
- **Air Boost:** Maintains elevation during ultimate animation

### Debug Features
- On-screen debug display showing:
  - Ultimate input status
  - Ultimate ready/cooldown state
  - Remaining cooldown time
  - Current animation state
- Console logging for events
- Animator settings validation tool

---

## 📁 File Structure

```
Assets/StarterAssets/
├── InputSystem/
│   └── StarterAssetsInputs.cs           [Modified] ✓
├── ThirdPersonController/
│   └── Scripts/
│       ├── ThirdPersonController.cs     [Modified] ✓
│       ├── AttackSystemDebugger.cs      [Modified] ✓
│       └── AnimatorSettingsChecker.cs   [Modified] ✓
└── Documentation/
    └── ULTIMATE_SKILL_IMPLEMENTATION.md [New] ✓
```

---

## 🛠️ Implementation Steps

### Step 1: Update Input System ✅

**File:** `StarterAssetsInputs.cs`

#### Added Components:
```csharp
// Public field for ultimate input
public bool ultimate;

// Input callback method
public void OnUltimate(InputValue value)
{
    UltimateInput(value.isPressed);
}

// Input handler
public void UltimateInput(bool newUltimateState)
{
    ultimate = newUltimateState;
}
```

**Purpose:** Capture player input for ultimate skill activation.

---

### Step 2: Extend Third Person Controller ✅

**File:** `ThirdPersonController.cs`

#### A. Added Inspector Fields:
```csharp
[Header("Ultimate Skill")]
[Tooltip("Cooldown time for ultimate skill in seconds")]
public float UltimateCooldown = 15.0f;

[Tooltip("Duration of ultimate animation")]
public float UltimateDuration = 3.0f;

[Tooltip("Enable/disable ultimate skill")]
public bool UltimateEnabled = true;

[Tooltip("Upward force applied when ultimate is activated")]
public float UltimateJumpForce = 5.0f;

[Tooltip("Additional upward velocity during ultimate animation")]
public float UltimateAirBoost = 2.0f;
```

#### B. Added Private Variables:
```csharp
// Ultimate state tracking
private float _ultimateCooldownTimer = 0f;
private bool _isUltimateReady = true;
private bool _isPerformingUltimate = false;

// Animation ID
private int _animIDUltimate;
```

#### C. Updated Methods:

##### 1. AssignAnimationIDs()
```csharp
_animIDUltimate = Animator.StringToHash("Ultimate");
```

##### 2. Update()
```csharp
private void Update()
{
    _hasAnimator = TryGetComponent(out _animator);
    JumpAndGravity();
    GroundedCheck();
    Move();
    HandleAttack();
    HandleUltimate();  // ← New call
}
```

##### 3. HandleUltimate() - New Method
```csharp
private void HandleUltimate()
{
    // Cooldown countdown
    if (!_isUltimateReady && _ultimateCooldownTimer > 0)
    {
        _ultimateCooldownTimer -= Time.deltaTime;
        if (_ultimateCooldownTimer <= 0)
        {
            _isUltimateReady = true;
            Debug.Log("Ultimate skill is ready!");
        }
    }

    // Check if currently in ultimate animation
    bool isInUltimateState = false;
    if (_hasAnimator)
    {
        AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
        isInUltimateState = currentState.IsName("UntimateAttack");
        
        if (isInUltimateState)
            _isPerformingUltimate = true;
        else if (_isPerformingUltimate)
            _isPerformingUltimate = false;
    }

    // Handle input
    if (_input.ultimate)
    {
        _input.ultimate = false;

        // Validation checks
        if (!UltimateEnabled)
        {
            Debug.LogWarning("Ultimate skill is disabled!");
            return;
        }

        if (!_isUltimateReady)
        {
            Debug.Log($"Ultimate on cooldown! {_ultimateCooldownTimer:F1}s remaining");
            return;
        }

        if (!Grounded)
        {
            Debug.Log("Cannot use ultimate in air!");
            return;
        }

        if (_isPerformingUltimate || isInUltimateState)
        {
            Debug.Log("Already performing ultimate!");
            return;
        }

        // Execute ultimate
        if (_hasAnimator)
        {
            // Reset attack combo state
            _attackCount = 0;
            _attackQueued = false;
            _lastProcessedAttackCount = 0;
            _attackCooldownTimer = 0f;

            // Clear all triggers
            _animator.ResetTrigger(_animIDAttack1);
            _animator.ResetTrigger(_animIDAttack2);
            _animator.ResetTrigger(_animIDAttack3);
            _animator.ResetTrigger(_animIDUltimate);

            // Trigger ultimate animation
            _animator.SetTrigger(_animIDUltimate);
            
            // Set cooldown
            _isUltimateReady = false;
            _ultimateCooldownTimer = UltimateCooldown;
            _isPerformingUltimate = true;

            Debug.Log("Ultimate skill activated!");
        }
    }
}
```

##### 4. Move() - Updated
```csharp
// Block movement during attack or ultimate
bool isInUltimateState = false;
if (_hasAnimator)
{
    AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
    isInUltimateState = currentState.IsName("UntimateAttack");
}

if (isInAttackState || isInUltimateState)
{
    _speed = 0f;
    _animationBlend = Mathf.Lerp(_animationBlend, 0f, Time.deltaTime * SpeedChangeRate);
    
    if (_hasAnimator)
    {
        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, 0f);
    }
    return;
}
```

##### 5. OnAnimatorMove() - Updated
```csharp
bool isInUltimateState = currentState.IsName("UntimateAttack");

if (isInAttackState || isInUltimateState)
{
    Vector3 rootMotionDelta = _animator.deltaPosition;
    _controller.Move(rootMotionDelta);
}
```

##### 6. Public API Methods - New
```csharp
// Get cooldown progress (0.0 to 1.0)
public float GetUltimateCooldownProgress()
{
    if (_isUltimateReady) return 1f;
    return 1f - (_ultimateCooldownTimer / UltimateCooldown);
}

// Check if ultimate is ready
public bool IsUltimateReady()
{
    return _isUltimateReady;
}

// Get remaining cooldown in seconds
public float GetUltimateRemainingCooldown()
{
    return _ultimateCooldownTimer;
}
```

---

### Step 3: Update Debug System ✅

**File:** `AttackSystemDebugger.cs`

#### Added Fields:
```csharp
[SerializeField] private bool isUltimatePressed;
[SerializeField] private bool isUltimateReady;
[SerializeField] private float ultimateCooldown;
```

#### Updated Update():
```csharp
if (_input != null)
{
    isUltimatePressed = _input.ultimate;
    
    if (_input.ultimate && logToConsole)
    {
        Debug.Log($"[Ultimate Input] Ultimate button pressed at {Time.time:F2}s");
    }
}

if (_controller != null)
{
    isUltimateReady = _controller.IsUltimateReady();
    ultimateCooldown = _controller.GetUltimateRemainingCooldown();
}
```

#### Updated UpdateAnimatorInfo():
```csharp
else if (_animator.GetCurrentAnimatorStateInfo(0).IsName("UntimateAttack"))
{
    currentAnimationState = "UntimateAttack";
    animationNormalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
}
```

#### Updated OnGUI():
```csharp
// Ultimate Input Status
style.normal.textColor = isUltimatePressed ? Color.magenta : Color.white;
GUI.Label(new Rect(20, yPos, 330, 20), 
    $"Ultimate Input: {(isUltimatePressed ? "PRESSED" : "Released")}", style);
yPos += lineHeight;

// Ultimate Ready Status
style.normal.textColor = isUltimateReady ? Color.green : Color.red;
GUI.Label(new Rect(20, yPos, 330, 20), $"Ultimate Ready: {isUltimateReady}", style);
yPos += lineHeight;

// Ultimate Cooldown
if (!isUltimateReady)
{
    style.normal.textColor = Color.yellow;
    GUI.Label(new Rect(20, yPos, 330, 20), 
        $"Ultimate Cooldown: {ultimateCooldown:F1}s", style);
    yPos += lineHeight;
}
```

#### Added Animation Event Handler:
```csharp
public void OnUltimateHit()
{
    if (logToConsole)
    {
        Debug.Log($"[Ultimate Event] Hit frame at {Time.time:F2}s");
    }
}
```

---

### Step 4: Update Animator Checker ✅

**File:** `AnimatorSettingsChecker.cs`

#### Updated CheckParameters():
```csharp
string[] requiredParams = { "Attack1", "Attack2", "Attack3", "Ultimate" };
```

#### Updated CheckStates():
```csharp
string[] requiredStates = { "Attack_1", "Attack_2", "Attack_3", "UntimateAttack" };
```

#### Updated CheckTransitions():
```csharp
var ultimate = System.Array.Find(stateMachine.states, 
    s => s.state.name == "UntimateAttack");

// Check Ultimate transition
if (ultimate.state != null)
{
    Debug.Log("\n▶▶ Checking Ultimate Transition:");
    Debug.Log("  ⚠ Ultimate should transition from Any State");
    Debug.Log("  ⚠ Check manually: Any State → UntimateAttack");
    CheckReturnToIdle(ultimate.state, "UntimateAttack", entryState);
}
```

---

## 🎮 Unity Editor Setup

### 1. Input Actions Configuration

Open `StarterAssets.inputactions` and add:

```
Action Name: Ultimate
Action Type: Button
Binding Path: <Keyboard>/r
Processors: none
Interactions: Press
```

**Steps:**
1. Double-click `StarterAssets.inputactions` in Project
2. Click `+` to add new Action
3. Name it `Ultimate`
4. Change Type to `Button`
5. Click `+` under Bindings
6. Select `Keyboard` > `R`
7. Save Asset

---

### 2. Animator Controller Setup

#### A. Import Mixamo Animation

**Recommended Animations:**
- Spin Attack
- Standing Melee Attack Horizontal
- Power Up
- Sword Slash
- Roundhouse Kick

**Import Steps:**
1. Download from Mixamo:
   - Format: **FBX for Unity**
   - Skin: **Without Skin** ✓

2. Import to Unity:
   - Drag `.fbx` into `Assets/Animations/`

3. Configure Animation File:
   - Select animation in Project
   - **Rig Tab:**
     - Animation Type: `Humanoid`
     - Avatar Definition: `Copy From Other Avatar`
     - Source: Select your Player's Avatar
     - Click `Apply`
   
   - **Animation Tab:**
     - Loop Time: ❌ (off for ultimate)
     - Root Transform Rotation:
       - Bake Into Pose: ✓ (Y axis)
       - Based Upon: Original
     - Root Transform Position (Y):
       - Bake Into Pose: ✓
       - Based Upon (at Start): Original
     - Root Transform Position (XZ):
       - Bake Into Pose: ❌
       - Based Upon: Center of Mass
     - Click `Apply`

#### B. Create Animator Parameter

1. Open Player's **Animator Controller**
2. Go to **Parameters** tab
3. Click `+` > `Trigger`
4. Name: `Ultimate`

#### C. Create Animation State

1. In **Animator** window
2. Right-click > **Create State** > **Empty**
3. Name: `UntimateAttack`
4. Select state
5. In Inspector:
   - **Motion:** Drag ultimate animation clip
   - **Speed:** 1
   - **Mirror:** ❌
   - **Foot IK:** ❌

#### D. Create Transitions

##### Transition 1: Any State → UntimateAttack

1. Right-click **Any State** > **Make Transition**
2. Click `UntimateAttack` state
3. Configure transition:
   ```
   Has Exit Time: ❌ (unchecked)
   Exit Time: 0
   Fixed Duration: ✓
   Transition Duration: 0.1
   Interruption Source: None
   Can Transition To Self: ❌
   
   Conditions:
   + Ultimate
   ```

##### Transition 2: UntimateAttack → Idle (or Entry)

1. Right-click `UntimateAttack` > **Make Transition**
2. Click **Entry** or your Idle state
3. Configure transition:
   ```
   Has Exit Time: ✓ (checked)
   Exit Time: 0.95
   Fixed Duration: ✓
   Transition Duration: 0.2
   
   Conditions: (none)
   ```

---

### 3. Inspector Settings

Select **Player GameObject** and configure:

#### ThirdPersonController Component:
```
Ultimate Skill:
  Ultimate Cooldown: 15.0
  Ultimate Duration: 3.0
  Ultimate Enabled: ✓
  Ultimate Jump Force: 5.0
  Ultimate Air Boost: 2.0
```

#### AttackSystemDebugger Component:
```
Debug Settings:
  Show On Screen Debug: ✓
  Log To Console: ✓
```

---

## 🧪 Testing & Debugging

### Test Procedure

1. **Enter Play Mode**
2. **Test Basic Activation:**
   - Press `R` key
   - Verify ultimate animation plays
   - Check console: "Ultimate skill activated!"

3. **Test Cooldown:**
   - Wait for animation to finish
   - Press `R` immediately
   - Check console: "Ultimate on cooldown! X.Xs remaining"
   - Observe on-screen debug showing cooldown timer

4. **Test Ready State:**
   - Wait 15 seconds
   - Check console: "Ultimate skill is ready!"
   - On-screen debug should show "Ultimate Ready: True"

5. **Test Restrictions:**
   - Jump in air
   - Press `R`
   - Check console: "Cannot use ultimate in air!"

6. **Test Combo Integration:**
   - Start combo attack (3 hits)
   - During combo, press `R`
   - Ultimate should interrupt and reset combo

### Debug Display

Top-left corner shows:
```
=== COMBAT SYSTEM DEBUG ===
Attack Input: Released
Ultimate Input: Released
Ultimate Ready: True
Has Animator: True
Current State: Idle
Animation Time: 0.00
Combo Window: 1.00s
Attack Cooldown: 0.50s
Ultimate Cooldown: 15.00s
Left Click = Attack | R = Ultimate
```

### Animator Validation

Run from menu: `Tools > Check Animator Settings`

Expected output:
```
=== ▶ STARTING ANIMATOR SETTINGS CHECK ===

▶▶ CHECKING PARAMETERS:
════════════════════════════════════════
✓ Attack1: Trigger (CORRECT)
✓ Attack2: Trigger (CORRECT)
✓ Attack3: Trigger (CORRECT)
✓ Ultimate: Trigger (CORRECT)
✓ ALL PARAMETERS CORRECT!

▶▶ CHECKING STATES:
════════════════════════════════════════
✓ Attack_1: Has animation 'Attack1'
✓ Attack_2: Has animation 'Attack2'
✓ Attack_3: Has animation 'Attack3'
✓ UntimateAttack: Has animation 'ultimate_animation'
✓ ALL STATES EXIST!

...

=== ✓ CHECK COMPLETED ===
```

---

## 🐛 Troubleshooting

### Issue 1: Ultimate Not Triggering

**Symptoms:**
- Pressing `R` does nothing
- No console messages

**Solutions:**
1. Check Input Actions:
   ```
   - Is StarterAssets.inputactions saved?
   - Is Ultimate action bound to R key?
   - Is OnUltimate() method in StarterAssetsInputs.cs?
   ```

2. Check Inspector:
   ```
   - Is UltimateEnabled = true?
   - Is Player tagged as "Player"?
   ```

3. Check Console for errors

---

### Issue 2: Animation Not Playing

**Symptoms:**
- Ultimate triggers but no animation
- Character freezes

**Solutions:**
1. Check Animator:
   ```
   - Does "UntimateAttack_1" state exist?
   - Is animation clip assigned to state?
   - Is "Ultimate" parameter a Trigger?
   ```

2. Run Animator Checker:
   ```
   Tools > Check Animator Settings
   ```

3. Check animation import settings:
   ```
   - Rig Type: Humanoid
   - Avatar: Copied from Player
   ```

---

### Issue 3: Animation Has "Missing" Bones

**Symptoms:**
- Yellow "Missing" warnings in Animation window
- Animation doesn't play correctly

**Solutions:**
1. Re-import animation:
   ```
   Select animation file > Inspector > Rig Tab
   - Animation Type: Humanoid
   - Avatar Definition: Copy From Other Avatar
   - Source: [Player Avatar]
   - Apply
   ```

2. Verify Avatar is Humanoid:
   ```
   Select Player model > Rig Tab
   - Animation Type: Humanoid
   - Configure > Check all bones are green
   ```

---

### Issue 4: Cooldown Not Working

**Symptoms:**
- Can spam ultimate infinitely
- Cooldown timer doesn't decrease

**Solutions:**
1. Check code:
   ```csharp
   // In HandleUltimate()
   _isUltimateReady = false;  // Must be set
   _ultimateCooldownTimer = UltimateCooldown;  // Must be set
   ```

2. Verify Update() calls HandleUltimate():
   ```csharp
   private void Update()
   {
       // ...
       HandleUltimate();  // Must be called
   }
   ```

---

### Issue 5: Character Doesn't Move Forward During Animation

**Symptoms:**
- Character plays animation in place
- No forward movement like in Mixamo preview
- Character "runs in place"

**Root Cause:**
- Animator "Apply Root Motion" setting is incorrect
- Or root motion handling code is missing

**Solutions:**
#### Option 1: Using "Handled by Script" (Recommended)
This is the approach used in Unity Starter Assets.

1. **Animator Settings:**
   ```
   Animator Component:
   - Apply Root Motion: FALSE (or "Handled by Script")
   ```

2. **Add ApplyRootMotionManually() method:**
   ```csharp
   private void Update()
   {
       _hasAnimator = TryGetComponent(out _animator);
       JumpAndGravity();
       GroundedCheck();
       Move();
       HandleAttack();
       HandleUltimate();
       
       // Handle root motion manually
       if (_hasAnimator && !_animator.applyRootMotion)
       {
           ApplyRootMotionManually();
       }
   }

   private void ApplyRootMotionManually()
   {
       if (!_animator || !_hasAnimator || !_controller) return;

       AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
       bool isInAttackState = currentState.IsName("Attack_1") || 
                            currentState.IsName("Attack_2") || 
                            currentState.IsName("Attack_3");
       bool isInUltimateState = currentState.IsName("UntimateAttack_1");
       
       if (isInAttackState)
       {
           // For attacks: Only apply horizontal movement (XZ)
           Vector3 rootMotionDelta = _animator.deltaPosition;
           Vector3 horizontalMotion = new Vector3(rootMotionDelta.x, 0f, rootMotionDelta.z);
           _controller.Move(horizontalMotion);
       }
       else if (isInUltimateState)
       {
           // For ultimate: Apply FULL root motion (XYZ)
           Vector3 rootMotionDelta = _animator.deltaPosition;
           _controller.Move(rootMotionDelta);
           
           // Disable gravity during ultimate to let animation control everything
           _verticalVelocity = 0f;
       }
   }
   ```

3. **Update HandleUltimate():**
   ```csharp
   if (isInUltimateState)
   {
       _isPerformingUltimate = true;
       // Don't modify _verticalVelocity here
       // Let ApplyRootMotionManually() handle it
   }
   else if (_isPerformingUltimate)
   {
       _isPerformingUltimate = false;
   }
   ```

4. **Update ultimate activation:**
   ```csharp
   // When activating ultimate
   _verticalVelocity = 0f; // Reset to let root motion take over
   _animator.SetTrigger(_animIDUltimate);
   ```

#### Option 2: Using OnAnimatorMove() (Alternative)
If you prefer to use Unity's callback:

1. **Animator Settings:**
   ```
   Animator Component:
   - Apply Root Motion: TRUE
   ```

2. **Update OnAnimatorMove():**
   ```csharp
   private void OnAnimatorMove()
   {
       if (_hasAnimator && _controller != null)
       {
           AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
           bool isInAttackState = currentState.IsName("Attack_1") || 
                                currentState.IsName("Attack_2") || 
                                currentState.IsName("Attack_3");
           bool isInUltimateState = currentState.IsName("UntimateAttack_1");
           
           if (isInAttackState)
           {
               Vector3 rootMotionDelta = _animator.deltaPosition;
               Vector3 horizontalMotion = new Vector3(rootMotionDelta.x, 0f, rootMotionDelta.z);
               _controller.Move(horizontalMotion);
           }
           else if (isInUltimateState)
           {
               Vector3 rootMotionDelta = _animator.deltaPosition;
               _controller.Move(rootMotionDelta);
           }
       }
   }
   ```

**Important Notes:**
- ⚠️ **State Name Must Match:** Make sure your animation state is named exactly `UntimateAttack_1`
- ⚠️ **Animation Import Settings:** Root Transform Position (XZ) must have "Bake Into Pose" = FALSE
- ⚠️ **Test Both Axes:** Check that both XZ (forward/sideways) and Y (jump/fall) motion work

---

### Issue 6: Character Falls Too Slowly After Ultimate

**Symptoms:**
- Character floats in air after animation ends
- Gravity seems reduced
- Takes too long to land

**Root Cause:**
- Vertical velocity is being reset to 0 during ultimate
- Gravity not reapplying properly after animation

**Solutions:**
1. **Update ApplyRootMotionManually():**
   ```csharp
   else if (isInUltimateState)
   {
       Vector3 rootMotionDelta = _animator.deltaPosition;
       _controller.Move(rootMotionDelta);
       
       // FULL ROOT MOTION: Let animation control Y axis
       _verticalVelocity = 0f;
   }
   ```

2. **Ensure gravity re-enables after ultimate:**
   ```csharp
   // In HandleUltimate()
   else if (_isPerformingUltimate)
   {
       // Animation finished, gravity will automatically resume in JumpAndGravity()
       _isPerformingUltimate = false;
   }
   ```

3. **JumpAndGravity() already handles this automatically:**
   ```csharp
   // When not grounded, gravity applies
   if (_verticalVelocity < _terminalVelocity)
   {
       _verticalVelocity += Gravity * Time.deltaTime; // -15.0 default
   }
   ```

**Verification:**
- After ultimate animation ends, `_verticalVelocity` should decrease by `Gravity * Time.deltaTime` each frame
- Default gravity is -15.0, which is realistic
- Character should land within 0.5-1 second after animation peak

---

### Issue 7: Animation State Name Mismatch

**Symptoms:**
- Debug log shows: `isInUltimateState=False` even though animation is playing
- Root motion not applied
- Character stuck in animation pose

**Root Cause:**
- State name in Animator doesn't match name in code
- Common mismatches:
  - `"UntimateAttack"` vs `"UntimateAttack_1"`
  - `"UltimateAttack"` vs `"UntimateAttack"` (typo in first letter)

**Solutions:**
1. **Check Animator state name:**
   ```
   - Open Animator window
   - Select the ultimate state
   - Check exact name in Inspector
   - Note: Unity may add "_1" suffix automatically
   ```

2. **Update all state checks in code:**
   ```csharp
   // In Move()
   isInUltimateState = currentState.IsName("UntimateAttack_1");
   
   // In HandleUltimate()
   isInUltimateState = currentState.IsName("UntimateAttack_1");
   
   // In ApplyRootMotionManually()
   bool isInUltimateState = currentState.IsName("UntimateAttack_1");
   ```

3. **Enable debug logging temporarily:**
   ```csharp
   // In ApplyRootMotionManually()
   if (_isPerformingUltimate)
   {
       Debug.Log($"State Check: isInUltimateState={isInUltimateState} | " +
                 $"StateHash={currentState.fullPathHash}");
   }
   ```

4. **Get exact state name from log:**
   - Play ultimate animation
   - Check console for hash value
   - Or check Animator window state name

**Prevention:**
- Use consistent naming: Always `UntimateAttack_1` everywhere
- Or use hash comparison instead:
  ```csharp
  private int _ultimateStateHash;
  
  void Start()
  {
      _ultimateStateHash = Animator.StringToHash("UntimateAttack_1");
  }
  
  // Then use:
  bool isInUltimateState = currentState.fullPathHash == _ultimateStateHash;
  ```

---

