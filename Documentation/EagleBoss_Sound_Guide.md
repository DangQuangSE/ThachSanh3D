# Boss Dai Bang — Sound Guide (HuuAnh Branch)

## 1. Inspector SFX Slots (BossDaiBangController)

| Slot | Sound Type | When It Plays |
|------|-----------|---------------|
| Sfx Roar | Monster roar | Mutant Roaring — immediate on trigger |
| Sfx Jump Attack | Ground slam | JumpAttack — early in animation (5–20%) |
| Sfx Magic Attack | Cast sound | MagicAttack — before fireball spawns (10–40%) |
| Sfx Hit | Punch impact | Punch/Uppercut/Roar — on confirmed hit only |
| Sfx Hurt | Boss grunt | When boss takes damage (0.5s cooldown) |
| Sfx Death | Death cry | On boss death |
| Sfx Volume | 0–1 slider | Master volume for all above (default 0.7) |

## 2. Fireball SFX (DaiBangFireballProjectile — on prefab, NOT boss)

| Slot | When It Plays |
|------|---------------|
| Sfx Fly Loop | Loops while fireball is flying |
| Sfx Hit Explosion | On collision with player |

## 3. How to Add Sound

1. Import `.wav`/`.ogg` into `Assets/ThachSanhGeneral/HuuAnh/Audio/SFX/`
2. Select **Boss GameObject** (`finalv5`) in Hierarchy
3. Find `BossDaiBangController` → **SFX** section
4. Drag clips into matching slots
5. For fireball: select **FX_Fireball prefab** → `DaiBangFireballProjectile` → assign `Sfx Fly Loop` + `Sfx Hit Explosion`
6. Play test — done

## 4. How Sound Works in Code

All sounds are **code-driven** (no Animation Events needed).


| Sound | Method | Why |
|-------|--------|-----|
| Hit, Hurt, JumpAttack, MagicAttack | `PlaySFX` | Boss alive, 3D positioned |
| Roar, Death | `PlayClipAtPoint` | Must survive after destroy / AudioSource busy |
| Fireball fly | Own `AudioSource.Play` (loop) | Independent from boss |
| Fireball explosion | `PlayClipAtPoint` | Fireball destroyed on hit |

## 5. Anti-Spam

| Sound | Guard |
|-------|-------|
| sfxHurt | 0.5s time cooldown |
| sfxJumpAttack | Once per animation (`_jumpAttackSoundPlayedThisCast`) |
| sfxMagicAttack | Once per animation (`_magicAttackSoundPlayedThisCast`) |
| sfxHit (melee) | Once per state hash (`_lastBossAttackHitDone`) |
| sfxRoar | Once per `PerformAttack()` call |
| sfxDeath | Boss dies once (`isDead = true`) |

## 6. AudioSource Auto-Setup

No manual setup needed — `Start()` auto-creates AudioSource:
spatialBlend = 1 (3D) | minDistance = 2m | maxDistance = 25m | Linear rolloff

## 7. Add New Sound (Example: wing flap on chase)

**Step 1** — Add field in SFX section:
[Tooltip("Wing flap on chase")] public AudioClip sfxWingFlap;

**Step 2** — Call in `ChangeState()`:
case BossState.Chase: if (agent != null) agent.isStopped = false; PlaySFX(sfxWingFlap); break;

**Step 3** — Drag clip into Inspector → test.

> All SFX fields are null-safe. Leave empty = skip sound, no errors.