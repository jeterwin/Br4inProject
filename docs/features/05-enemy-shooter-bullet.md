# Feature 05 — Enemy Shooter + Bullet

## Overview

Enemies fire yellow bullet spheres toward the player at configurable intervals. Bullets travel in a straight line using `Time.deltaTime`, so they freeze mid-air when time stops and resume when the player moves. On contact with the player, bullets log a hit and self-destruct. They also auto-despawn after 10 seconds of game-time to prevent accumulation.

## Scripts

| Script | Path | Purpose |
|--------|------|---------|
| EnemyShooter | Assets/Scripts/Enemy/EnemyShooter.cs | Timer-based firing system that instantiates bullets aimed at the player |
| Bullet | Assets/Scripts/Enemy/Bullet.cs | Projectile that travels forward, auto-freezes with time, detects player hits |

## Inspector Fields

**EnemyShooter:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| _bulletPrefab | GameObject | — | Reference to Bullet prefab |
| _fireInterval | float | 2f | Seconds between shots (game-time) |
| _bulletSpeed | float | 8f | Bullet travel speed in units/second |

**Bullet:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| _lifetime | float | 10f | Seconds before auto-despawn (game-time) |

## How It Works

### EnemyShooter
1. `Start()` caches the player transform via `FindWithTag("Player")`.
2. `Update()` increments a timer by `Time.deltaTime` (auto-pauses during freeze).
3. When timer exceeds `_fireInterval`, calls `Fire()` and resets.
4. `Fire()` calculates direction to player, instantiates the bullet prefab **1.5 units forward** from the enemy (to prevent self-collision with the enemy's own collider), and calls `Bullet.Initialize()` with direction and speed.

### Bullet
1. `Initialize(direction, speed)` sets the bullet's rotation to face the direction and stores the speed.
2. `Update()` translates forward by `speed * Time.deltaTime` — freezes automatically when `timeScale = 0`.
3. `Update()` also tracks age via `Time.deltaTime` and self-destructs at `_lifetime`.
4. `OnTriggerEnter()` checks for "Player" tag — logs "Player hit by bullet!" and destroys the bullet.

## Bullet Prefab (`Assets/Prefabs/Bullet.prefab`)

| Property | Value |
|----------|-------|
| Mesh | Sphere, scale (0.15, 0.15, 0.15) |
| Material | YellowMat (255, 255, 50) |
| SphereCollider | isTrigger = true |
| Rigidbody | isKinematic = true, useGravity = false |
| Script | Bullet |
| Layer | "Bullet" |

## Dependencies

- Spec 04 (EnemyController) — EnemyShooter is added to the Enemy prefab alongside EnemyController.
- Player must be tagged "Player" in the scene.

## Unity Setup & Test

1. Select the **Enemy prefab** in `Assets/Prefabs/Enemy.prefab` (or a test enemy in the scene)
2. Add Component → **EnemyShooter**
3. Drag `Assets/Prefabs/Bullet.prefab` into the **Bullet Prefab** field
4. Set Fire Interval = 2, Bullet Speed = 8
5. Apply changes to prefab
6. **Migration:** Remove EnemyShooter from old test objects (DriftCube1-3, SpinCube1-2, SpinSphere1) if still present
7. Enter Play mode — enemies should fire bullets toward you
8. Stand still — bullets freeze mid-air
9. Move — bullets resume flight
10. Walk into a bullet — Console shows "Player hit by bullet!", bullet disappears
11. Wait 10+ game-time seconds — old bullets auto-despawn

## Known Limitations

- Bullets fire from the enemy's center (no muzzle offset) for MVP.
- Bullets spawn 1.5 units forward from the enemy to avoid self-collision — at very close range this could miss the player.
- No bullet spread or accuracy variation.
- Bullets pass through other enemies and obstacles (trigger collider, no physics collision with environment).
