# ?? Eagle Boss (Dai Bang) - HuuAnh Branch Documentation

> **Branch:** `test-bossdaibang`  
> **Folder:** `Assets/ThachSanhGeneral/HuuAnh/`  
> **Unity Version:** 2022.3.x (URP)  
> **Last Updated:** 2025

---

## ?? Folder Structure

```
Assets/ThachSanhGeneral/HuuAnh/
??? Scripts/
?   ??? BossDaiBangController.cs      # Main boss AI & combat logic
?   ??? BossDaiBangDebugger.cs        # Runtime debug tools & hotkeys
?   ??? BossDaiBangHealthBarWorld.cs  # World-space health bar (above boss head)
?   ??? DaiBangFireballProjectile.cs  # Fireball projectile (MagicAttack)
?   ??? HealthBarUIDaiBang.cs         # Screen-space health bar UI
?   ??? PlayerHealthBarSync.cs        # Player health bar sync helper
?   ??? VFXGroundSnap.cs             # Auto-snap VFX to ground level
??? VFX/
    ??? Free Game VFX/                # VFX prefabs (particles)
    ??? Resource/Shader/              # Custom shaders (AdditiveFlow, AlphaBlendFlow)
```

---

## ?? Script Overview

### 1. `BossDaiBangController.cs` — Main Boss Script

**Attach to:** Boss root GameObject (must have `Animator` + `NavMeshAgent`)

#### State Machine
| State | Behavior |
|-------|----------|
| **Idle** | Waits until player enters `detectionRange` |
| **Chase** | NavMeshAgent moves toward player. Returns to spawn if `maxChaseDistance` exceeded |
| **Attack** | Stops moving, rotates toward player, executes attack rotation |
| **Death** | Disables agent & collider, plays Die animation, destroys after 5s |

#### Attack Rotation (6-hit cycle, then loops)
| Index | Attack | Trigger | Notes |
|-------|--------|---------|-------|
| 0 | Punch | `Punch` | Melee, spawns Green Hit VFX |
| 1 | Punch | `Punch` | Same as above |
| 2 | Uppercut | `Uppercut` | Melee, spawns Green Hit VFX |
| 3 | Jump Attack | `JumpAttack` | Spawns FX_Weapon Effect at ground |
| 4 | Magic Attack | `MagicAttack` | Spawns FX_Fireball projectile |
| 5 | Mutant Roaring | `MutantRoaring` | AoE damage, no fire VFX, boss stands still for `roarDuration` |

#### Inspector Fields

| Section | Field | Description |
|---------|-------|-------------|
| **Boss Stats** | `maxHealth` | Boss max HP (default: 1000) |
| | `attackDamage` | Damage per hit (default: 30) |
| | `moveSpeed` | NavMeshAgent speed (default: 3.5) |
| **Combat** | `detectionRange` | Range to start chasing (default: 15) |
| | `attackRange` | Range to stop and attack (default: 2.5) |
| | `attackCooldown` | Seconds between attacks (default: 2) |
| | `maxChaseDistance` | Max chase distance before returning (default: 30) |
| **Attack Hitbox** | `attackPoint` | Transform for melee hit detection |
| | `attackRadius` | Hit sphere radius (default: 1.5) |
| | `playerLayer` | LayerMask for player detection |
| **References** | `target` | Player Transform (auto-finds by "Player" tag if empty) |
| **VFX** | `magicSpawnPoint` | Fireball spawn position (hand/staff) |
| | `fireBreathSpawnPoint` | Roar damage origin (mouth). Falls back to magicSpawnPoint |
| | `jumpAttackSpawnPoint` | Jump Attack VFX spawn (feet/ground). Falls back to transform |
| | `fxGreenHit` | Green hit VFX prefab (Punch/Uppercut) |
| | `fxFireball` | Fireball VFX prefab (MagicAttack) |
| | `fxWeaponEffect` | Ground impact VFX prefab (JumpAttack) |
| **Roar** | `roarDuration` | How long boss stands still during Roar (default: 2.2s) |
| | `magicToRoarDelay` | Delay between MagicAttack and Roar (default: 1.5s) |
| **Player Damage** | `playerHitRange` | Range for player melee to register (default: 3) |
| | `fallbackPlayerDamage` | Damage if `PlayerAttack` not found (default: 25) |

#### Damage System
- **Boss ? Player:** Animation-driven detection (no Animation Events required for testing). Checks melee hit window at normalized time 0.2–0.7.
- **Player ? Boss:** Boss auto-detects player attack animations (`Attack_1`, `Attack_2`, `Attack_3`, `UntimateAttack`, `Attack360`) within `playerHitRange`.
- **Visual Feedback:** Player flashes red on hit, uses cached original colors for correct restoration.

---

### 2. `DaiBangFireballProjectile.cs` — Fireball Projectile

**Attach to:** FX_Fireball prefab

| Field | Description |
|-------|-------------|
| `speed` | Flight speed (default: 10) |
| `maxDistance` | Auto-destroy distance (default: 30) |
| `damage` | Damage on hit (default: 20) |
| `playerLayer` | Player LayerMask for collision filtering |
| `hitVfx` | Explosion VFX prefab on impact (optional) |

**Behavior:** Flies forward, damages player on `OnTriggerEnter`, then self-destructs. Requires a **Collider** set to **Is Trigger** on the prefab.

---

### 3. `BossDaiBangDebugger.cs` — Debug Tools

**Attach to:** Same Boss GameObject (or child)

#### Hotkeys (Runtime)
| Key | Action |
|-----|--------|
| `K` | Deal test damage to boss |
| `L` | Kill boss instantly |
| `H` | Heal boss to full |

#### On-Screen Debug UI
- Boss state (color-coded)
- Health percentage
- Distance to target
- Hotkey reference

---

### 4. `BossDaiBangHealthBarWorld.cs` — World-Space Health Bar

**Attach to:** `BossHealthBarCanvas` (child of Boss GameObject)

| Field | Description |
|-------|-------------|
| `boss` | Auto-finds `BossDaiBangController` in parent if empty |
| `healthSlider` | Auto-finds Slider in children if empty |
| `bossDisplayName` | Name shown above health bar |
| `offset` | Position offset above boss (default: `0, 3, 0`) |
| `billboardToCamera` | Always faces camera |

**Requirements:**
- Canvas **Render Mode = World Space** (mandatory)
- Canvas **Scale = (0.01, 0.01, 0.01)**
- Slider with Fill image as child

---

### 5. `HealthBarUIDaiBang.cs` — Screen-Space Health Bar

**Attach to:** UI Canvas (Screen Space)

Supports both `BossDaiBangController` and `PlayerHealth` as targets. Assign via `healthTarget` field. Automatically updates color gradient from green (full) to red (low).

---

### 6. `PlayerHealthBarSync.cs` — Player Health Sync

**Attach to:** Canvas/object containing player health Slider

Auto-finds player by `"Player"` tag. Updates Slider value and optional health text every frame.

---

## ?? Setup Guide

### Step 1: Boss GameObject Setup

```
1. Select Boss root object in Hierarchy (e.g. "finalv5")
2. Required components on this object:
   ? Animator (Controller = BossDaiBang)
   ? NavMeshAgent
   ? Collider (BoxCollider or CapsuleCollider)
   ? BossDaiBangController (Add Component)
```

### Step 2: Create Spawn Points (Empty GameObjects as children of Boss)

```
Boss (finalv5)
??? MagicSpawnPoint      ? Position at hand/staff (for Fireball)
??? FireBreathSpawn      ? Position at mouth (for Roar damage origin)
??? JumpAttackSpawnPoint ? Position at feet, near ground (for weapon VFX)
??? AttackPoint          ? Position at fist/weapon tip (for melee hitbox)
```

### Step 3: Inspector Assignment

```
BossDaiBangController:
??? Target            ? Drag Player from Hierarchy (or leave empty for auto-find)
??? Attack Point      ? MagicSpawnPoint or AttackPoint
??? Magic Spawn Point ? MagicSpawnPoint
??? Fire Breath Spawn ? FireBreathSpawn (optional)
??? Jump Attack Spawn ? JumpAttackSpawnPoint (optional, prevents VFX floating)
??? Player Layer      ? Select "Player" layer
??? Fx Green Hit      ? Drag prefab from HuuAnh/VFX/Free Game VFX/Prefab
??? Fx Fireball       ? Drag prefab from HuuAnh/VFX/Free Game VFX/Prefab
??? Fx Weapon Effect  ? Drag prefab from HuuAnh/VFX/Free Game VFX/Prefab
```

### Step 4: NavMesh Bake

```
1. Window ? AI ? Navigation
2. Select ground objects ? Mark as "Navigation Static"
3. Bake tab ? Click "Bake"
```

### Step 5: Animation Events (Optional but Recommended)

Animation Events make damage timing precise. Without them, the script uses animation-driven detection as fallback.

| Animation Clip | Frame | Function |
|----------------|-------|----------|
| Punch | Hit frame (~50%) | `DealDamageToPlayer` |
| Uppercut | Hit frame (~50%) | `DealDamageToPlayer` |
| Jump Attack | Landing frame | `DealDamageToPlayer` |
| Magic Attack | Cast frame (~30%) | `SpawnMagicAttack` |

### Step 6: Health Bar Setup

#### World-Space (Above Boss Head)
```
1. Create child Canvas under Boss ? Name: "BossHealthBarCanvas"
2. Canvas ? Render Mode = World Space
3. RectTransform ? Scale = (0.01, 0.01, 0.01)
4. Add Slider as child ? Remove Handle, set Fill color = Red
5. Add Component ? BossDaiBangHealthBarWorld
6. Position offset Y = 3 (above boss head)
```

#### Screen-Space (Player HP)
```
1. Create Canvas ? Render Mode = Screen Space - Overlay
2. Add Slider + Text as children
3. Add Component ? PlayerHealthBarSync
4. Script auto-finds Player by tag
```

---

## ?? Troubleshooting

| Problem | Solution |
|---------|----------|
| Boss doesn't move | Check NavMesh is baked. Check `NavMeshAgent` is on same object |
| Boss doesn't attack | Check `target` is assigned or Player has `"Player"` tag |
| Boss attacks but no damage | Add Animation Events or check `playerLayer` matches Player's layer |
| VFX spawns too high | Assign `jumpAttackSpawnPoint` at boss feet. Or add `VFXGroundSnap` to VFX prefab |
| Fireball doesn't move | Check `DaiBangFireballProjectile` is on the FX_Fireball prefab |
| Fireball doesn't damage | Check prefab has Collider with `Is Trigger = true`. Check `playerLayer` |
| Health bar invisible | Canvas must be **World Space**. Check Scale = (0.01, 0.01, 0.01) |
| Health bar doesn't update | Check `BossDaiBangHealthBarWorld` has boss reference (auto-finds in parent) |
| Player health bar stays full | Add `PlayerHealthBarSync` to the canvas and ensure Player has `PlayerHealth` |
| Boss Roar does no damage | Player must be within `attackRadius * 3`. Check `fireBreathSpawnPoint` position |
| Red flash stays permanently | Ensure only one boss instance. Check `_playerFlashInProgress` resets correctly |

---

## ?? Important Notes

1. **Do NOT modify files outside `HuuAnh/` folder** — other team members' code (Bach, Quang) should remain untouched.
2. **Player must have:**
   - `PlayerHealth` component
   - `PlayerAttack` component (for `attackDamage` value)
   - `"Player"` tag
   - Assigned to a `Player` layer
3. **Animator Controller** must have these triggers: `Punch`, `Uppercut`, `JumpAttack`, `MagicAttack`, `MutantRoaring`, `Die` and bool `isWalking`.
4. **VFX Prefabs** are located in `HuuAnh/VFX/Free Game VFX/Prefab/`. Render pipeline = **URP**.
5. **Scaling:** Boss hitbox radius auto-scales with `transform.lossyScale`. No manual adjustment needed for different model sizes.

---

## ?? Quick Test Checklist

- [ ] Boss chases player when in range
- [ ] Boss stops and attacks when close enough
- [ ] Punch/Uppercut spawn Green Hit VFX on player
- [ ] Jump Attack spawns Weapon Effect VFX at ground level
- [ ] Magic Attack spawns Fireball that flies toward player
- [ ] Fireball damages player on collision
- [ ] Mutant Roaring deals AoE damage (no fire VFX)
- [ ] Boss returns to spawn when player runs too far
- [ ] Boss health bar updates above head
- [ ] Player health bar updates on screen
- [ ] Player attacks damage boss (check Console logs)
- [ ] Boss dies at 0 HP, plays Die animation
- [ ] Debug hotkeys work (K = damage, L = kill, H = heal)

---

## ? VFX System — Complete Guide

### VFX Asset Info

- **Source:** `HuuAnh/VFX/Free Game VFX/` (10 beam/particle prefabs)
- **Render Pipeline:** URP only
- **Texture:** 1024×1024 (PNG, TGA)
- **Custom Shaders:** `HuuAnh/VFX/Resource/Shader/` — `AdditiveFlow`, `AlphaBlendFlow`

---

### Boss VFX (BossDaiBangController)

Boss VFX are spawned by code inside `BossDaiBangController.cs`. Each attack spawns a different prefab:

| Attack | VFX Field | Spawn Method | Spawn Point | Description |
|--------|-----------|--------------|-------------|-------------|
| Punch | `fxGreenHit` | `SpawnGreenHitOnPlayer()` | Player position + 1m up | Green impact flash on player |
| Uppercut | `fxGreenHit` | `SpawnGreenHitOnPlayer()` | Player position + 1m up | Same as Punch |
| Jump Attack | `fxWeaponEffect` | `SpawnJumpAttackMagic()` | `jumpAttackSpawnPoint` (boss feet) | Ground impact shockwave |
| Magic Attack | `fxFireball` | `SpawnMagicAttack()` | `magicSpawnPoint` (hand/staff) | Projectile flying toward player |
| Mutant Roaring | *(none)* | — | — | AoE damage only, no VFX |

#### How to Assign Boss VFX Prefabs

```
1. In Project window, navigate to: HuuAnh/VFX/Free Game VFX/Prefab/
2. Select Boss in Hierarchy
3. In Inspector ? BossDaiBangController:
   - Fx Green Hit     ? Drag a green/impact particle prefab
   - Fx Fireball      ? Drag a fireball/projectile particle prefab
   - Fx Weapon Effect ? Drag a ground slam/shockwave particle prefab
```

#### VFX Spawn Points Explained

```
Boss (finalv5)
?
??? MagicSpawnPoint         ? magicSpawnPoint (Fireball spawns here)
?   Position: at hand/staff tip
?
??? FireBreathSpawn         ? fireBreathSpawnPoint (Roar damage origin)
?   Position: near mouth
?
??? JumpAttackSpawnPoint    ? jumpAttackSpawnPoint (Weapon Effect spawns here)
?   Position: at feet, Y near ground level
?
??? AttackPoint             ? attackPoint (melee hitbox center)
    Position: at fist/weapon tip
```

**Important:** If `jumpAttackSpawnPoint` is not assigned, `fxWeaponEffect` falls back to `transform.position` (boss root). If the root is above ground, VFX will float — assign the spawn point at feet level to fix this.

---

### Player VFX (AttackVFXManager)

Player attack VFX are managed by `AttackVFXManager.cs` (located in `StarterAssets/ThirdPersonController/Scripts/`). This script auto-detects animation states and spawns VFX at the right timing — no Animation Events needed.

**Attach to:** Player root (same object with `Animator`)

#### Player VFX Mapping

| Animation State | VFX Field | Spawn Time | Description |
|-----------------|-----------|------------|-------------|
| `Attack_1` | `attack1VFX` | 0.4 | First swing slash |
| `Attack_2` | `attack2VFX` | 0.4 | Second swing slash |
| `Attack_3` | `attack3VFX` | 0.4 | Third swing slash |
| `UntimateAttack` | `ultimateVFX` | 0.5 | Ultimate attack effect |
| `ProtectAxe` | `protectVFX` | 0.1 | Shield effect (stays active) |
| `Attack360` | `eskillVFX` | 0.3 | 360° spin slash |

#### How to Setup Player VFX

```
1. Select Player in Hierarchy
2. Add Component ? AttackVFXManager
3. Assign fields:
   - VFX Spawn Point   ? Weapon tip Transform (child of player)
   - Spawn Offset      ? Fine-tune position (default: 0,0,0)
   - VFX Scale         ? Scale multiplier (default: 1)
   - VFX Lifetime      ? Seconds before auto-destroy (default: 2)
4. Assign VFX prefabs:
   - Attack 1/2/3 VFX  ? Drag slash effect prefabs
   - Ultimate VFX      ? Drag ultimate effect prefab
   - Protect VFX       ? Drag shield effect prefab
   - E Skill VFX       ? Drag 360° effect prefab
5. Adjust rotation offsets per attack if VFX faces wrong direction
6. Enable "Show Debug Logs" to verify spawn positions in Console
```

#### Adjusting VFX Position & Rotation

| Setting | What it does |
|---------|--------------|
| `spawnOffset` | Moves VFX relative to spawn point (local space) |
| `attackNRotationOffset` | Rotates VFX for each attack (X=pitch, Y=yaw, Z=roll) |
| `useWeaponRotation` | `true` = follow weapon rotation, `false` = world rotation |
| `vfxScale` | Uniform scale multiplier |

---

### VFXGroundSnap (Auto-Snap VFX to Ground)

**Script:** `HuuAnh/Scripts/VFXGroundSnap.cs`  
**Attach to:** Any VFX prefab that needs to spawn at ground level

This is a standalone component — no changes needed to `AttackVFXManager` or `BossDaiBangController`.

#### How it Works
1. When VFX is spawned (by any script), `Start()` fires
2. Raycasts downward to find ground surface
3. Snaps VFX Y position to ground + `groundOffset`

#### Setup
```
1. Open VFX prefab (e.g. FX_Weapon Effect)
2. Add Component ? VFXGroundSnap
3. Configure:
   - Ground Layer   ? Select ground/terrain layer (default: Everything)
   - Ground Offset  ? Height above ground (default: 0.05, set 0 for flat on ground)
   - Snap Only Y    ? ? Keep X/Z position, only adjust height
   - Show Debug Log ? ? See snap results in Console
4. Save prefab (Ctrl+S)
```

#### When to Use
| Scenario | Solution |
|----------|----------|
| VFX floats above ground | Add `VFXGroundSnap`, set `groundOffset = 0` |
| VFX clips into ground | Increase `groundOffset` (e.g. `0.1`) |
| VFX moves horizontally when snapping | Enable `snapOnlyY = true` |
| VFX doesn't snap at all | Check `groundLayer` matches your terrain's layer |

---

### Fireball Prefab Setup (DaiBangFireballProjectile)

The fireball is a special VFX that also acts as a projectile with damage.

#### Prefab Requirements
```
FX_Fireball (prefab root)
??? ParticleSystem(s)          ? Visual effect
??? Collider (Sphere/Box)      ? Is Trigger = ?
??? DaiBangFireballProjectile  ? Script component
```

#### Setup Steps
```
1. Open/Create FX_Fireball prefab
2. Add ParticleSystem for visual (fire/energy particles)
3. Add SphereCollider:
   - Is Trigger = ?
   - Radius = 0.5 (adjust to fireball size)
4. Add Rigidbody:
   - Use Gravity = ?
   - Is Kinematic = ?
5. Add Component ? DaiBangFireballProjectile
6. Configure:
   - Speed        ? 10 (flight speed)
   - Max Distance ? 30 (auto-destroy range)
   - Damage       ? 20 (damage to player)
   - Player Layer ? Select "Player" layer
   - Hit VFX      ? Optional explosion prefab on impact
7. Save prefab
```

---

### VFX Troubleshooting

| Problem | Cause | Fix |
|---------|-------|-----|
| VFX doesn't appear | `PlayOnAwake = false` and not manually played | Enable `autoPlayParticleSystems` on AttackVFXManager, or set `PlayOnAwake = true` on prefab |
| VFX appears then vanishes instantly | `Duration` too short or `Looping = false` with short lifetime | Increase ParticleSystem `Duration` or `Start Lifetime` |
| VFX is pink/magenta | Shader incompatible with URP | Re-assign material to use URP particle shader (`Particles/Unlit`) |
| VFX spawns at wrong position | Wrong spawn point assigned | Check which Transform is assigned. Enable `showDebugLogs` to see exact spawn position |
| VFX floats above ground | Spawn point is on hand/staff (high up) | Assign `jumpAttackSpawnPoint` at feet, or add `VFXGroundSnap` to prefab |
| VFX faces wrong direction | Rotation offset not configured | Adjust `attackNRotationOffset` values. Enable `showRotationGizmos` to visualize in Scene view |
| VFX too big / too small | Scale mismatch | Adjust `vfxScale` on AttackVFXManager, or `groundOffset` scale on VFXGroundSnap |
| Fireball doesn't collide | Missing Collider or wrong layer | Add Collider with `Is Trigger = true`. Set `playerLayer` to match Player's layer |
| Fireball goes through walls | No collision with environment | Add Rigidbody (Kinematic) + check Physics collision matrix |
| Multiple VFX spawn per attack | Animation looping or spawn flag not resetting | Check `normalizedTime` thresholds. Ensure animation doesn't loop unintentionally |
