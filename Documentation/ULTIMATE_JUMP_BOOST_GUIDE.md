# Ultimate Skill Jump Boost Feature

## ?? Overview

This feature adds a **jump boost** effect when the Ultimate skill is activated, making the player launch into the air for a more dramatic and impactful ultimate animation.

---

## ? Features

### Initial Jump Boost
- **Trigger:** When Ultimate is activated (press R)
- **Effect:** Player launches upward with configurable force
- **Default Value:** `UltimateJumpForce = 5.0f`

### Air Boost During Animation
- **Trigger:** During first half of ultimate animation
- **Effect:** Continuous upward boost to maintain elevation
- **Default Value:** `UltimateAirBoost = 2.0f`

---

## ?? How It Works

### 1. Initial Launch (Frame 0)
```csharp
// When R is pressed and ultimate activates
_verticalVelocity = UltimateJumpForce;  // Launch into air
```

**Result:** Player immediately jumps upward with force of 5.0 (same as falling from ~1.27m high)

### 2. Sustained Elevation (Animation frames 0-50%)
```csharp
// During first half of animation
if (currentState.normalizedTime < 0.5f)
{
    _verticalVelocity += UltimateAirBoost * Time.deltaTime;
    _verticalVelocity = Mathf.Min(_verticalVelocity, UltimateJumpForce);
}
```

**Result:** Player maintains height during animation instead of immediately falling

### 3. Natural Fall (Animation frames 50-100%)
- After 50% of animation, air boost stops
- Gravity takes over
- Player falls naturally with animation

---

## ?? Configuration

### In Unity Inspector

Select **Player GameObject** ? **ThirdPersonController**:

```
Ultimate Skill:
  Ultimate Cooldown: 15.0
  Ultimate Duration: 3.0
  Ultimate Enabled: ?
  
  Ultimate Jump Force: 5.0    ? Adjust this for jump height
  Ultimate Air Boost: 2.0     ? Adjust this for air time
```

### Recommended Values

| Effect | Jump Force | Air Boost | Description |
|--------|------------|-----------|-------------|
| **Subtle** | 3.0 | 1.0 | Small hop, quick return |
| **Normal** | 5.0 | 2.0 | Moderate jump, feels powerful |
| **Dramatic** | 8.0 | 4.0 | High jump, anime-style |
| **Extreme** | 12.0 | 6.0 | Super jump, almost flying |

### Fine-Tuning Guide

**Want player to jump higher?**
? Increase `Ultimate Jump Force` (3.0 ? 8.0)

**Want player to stay in air longer?**
? Increase `Ultimate Air Boost` (2.0 ? 4.0)

**Want faster fall after animation?**
? Decrease `Ultimate Air Boost` (2.0 ? 0.5)

**Want ground-based ultimate (no jump)?**
? Set both to `0.0`

---

## ?? Testing

### Test Scenarios

1. **Normal Ultimate:**
   ```
   - Stand on ground
   - Press R
   - Should jump ~1.5-2 meters high
   - Float during animation
   - Land smoothly
   ```

2. **Compare with Normal Jump:**
   ```
   - Press Space (normal jump)
   - Note the height
   - Press R (ultimate)
   - Ultimate should jump higher
   ```

3. **Edge Cases:**
   ```
   - Near ceiling: Should hit ceiling
   - On slope: Should jump straight up
   - In combat: Should interrupt attacks
   ```

---

## ?? Visual Customization

### Combine with Effects

For best results, add these to the ultimate:

1. **Particle Effects:**
   - Spawn particles at feet on launch
   - Trail particles during air time
   - Impact particles on landing

2. **Camera Shake:**
   - Shake on launch
   - Stabilize during air
   - Shake on landing

3. **Slow Motion:**
   - Slow down time at peak height
   - Resume normal speed on descent

---

## ?? Advanced Customization

### Custom Jump Curve

Replace linear boost with animation curve:

```csharp
[Header("Ultimate Skill")]
public AnimationCurve ultimateJumpCurve;

// In HandleUltimate()
float curveValue = ultimateJumpCurve.Evaluate(currentState.normalizedTime);
_verticalVelocity += curveValue * UltimateAirBoost * Time.deltaTime;
```

**Setup in Inspector:**
1. Click on `Ultimate Jump Curve`
2. Create custom curve (e.g., starts high, gradually decreases)

### Direction Control

Add horizontal boost:

```csharp
[Header("Ultimate Skill")]
public float UltimateHorizontalBoost = 2.0f;

// In HandleUltimate()
Vector3 launchDirection = transform.forward * UltimateHorizontalBoost;
_controller.Move(launchDirection * Time.deltaTime);
```

---

## ?? Troubleshooting

### Issue 1: Player Jumps Too High

**Solution:**
```
Decrease Ultimate Jump Force:
  Current: 5.0
  New: 3.0 or 2.0
```

### Issue 2: Player Falls Too Fast

**Solution:**
```
Increase Ultimate Air Boost:
  Current: 2.0
  New: 3.0 or 4.0
```

### Issue 3: Player Floats Forever

**Symptoms:** Never comes down

**Solution:**
```csharp
// Check this code exists:
if (currentState.normalizedTime < 0.5f)  // ? This line is important!
{
    _verticalVelocity += UltimateAirBoost * Time.deltaTime;
}
```

### Issue 4: Jump Not Triggering

**Solutions:**
1. Check `Ultimate Jump Force` is not 0
2. Verify `HandleUltimate()` is called in `Update()`
3. Check Console for "Ultimate skill activated!"

---

## ?? Physics Breakdown

### Gravity Integration

```
Normal Gravity: -15.0 (pulls down)
Jump Force: +5.0 (pushes up)
Air Boost: +2.0 per frame (maintains height)

Frame 0: velocity = +5.0 (launch)
Frame 1: velocity = +5.0 + 2.0 - 15.0*dt = ~4.5
Frame 2: velocity = +4.5 + 2.0 - 15.0*dt = ~4.0
...
Frame 30: velocity = capped at 5.0 (max)
Frame 31-60: Air boost stops, gravity takes over
```

### Jump Height Calculation

```
Height = (Jump Force)² / (2 * Gravity)
      = (5.0)² / (2 * 15.0)
      = 25 / 30
      = 0.83 meters (base jump)

With Air Boost:
  Sustained for 0.5 seconds
  Additional height ? 0.5-1.0 meters
  
Total jump ? 1.5-2.0 meters
```

---

## ?? Best Practices

1. **Match Animation:**
   - If animation has big wind-up, use higher jump
   - If animation is quick, use smaller jump

2. **Game Feel:**
   - Action game: Higher, more dramatic
   - Realistic game: Lower, more grounded

3. **Level Design:**
   - Ensure ceilings are high enough
   - Consider low ceiling areas
   - Test in all environments

4. **Balance:**
   - Ultimate should feel powerful but not overpowered
   - Consider adding landing stun if jump is very high

---

## ?? Code Reference

### Complete Jump Logic

```csharp
private void HandleUltimate()
{
    // ... (cooldown and state checks)
    
    if (isInUltimateState)
    {
        _isPerformingUltimate = true;
        
        // Sustain air time
        if (currentState.normalizedTime < 0.5f)
        {
            _verticalVelocity += UltimateAirBoost * Time.deltaTime;
            _verticalVelocity = Mathf.Min(_verticalVelocity, UltimateJumpForce);
        }
    }
    
    // ... (input handling)
    
    if (ultimate activated)
    {
        // Initial launch
        _verticalVelocity = UltimateJumpForce;
        
        // ... (trigger animation)
    }
}
```

---

## ? Checklist

Before release, verify:

- [ ] Jump height feels good in all areas
- [ ] Player doesn't clip through ceilings
- [ ] Landing is smooth
- [ ] Works with all animations
- [ ] No bugs when spamming R
- [ ] Cooldown prevents abuse
- [ ] Visual effects match jump arc

---

## ?? Example Configurations

### Configuration 1: Ground Slam
```
Jump Force: 8.0
Air Boost: 3.0
Animation: High jump ? Slam down
Effect: Player leaps high, crashes down
```

### Configuration 2: Spinning Attack
```
Jump Force: 5.0
Air Boost: 4.0
Animation: Mid-air spin
Effect: Floats while spinning
```

### Configuration 3: Quick Strike
```
Jump Force: 3.0
Air Boost: 1.0
Animation: Fast slash
Effect: Small hop for mobility
```

---

**Last Updated:** 2024
**Version:** 1.0
**Status:** ? Implemented

