# Spec 05 — Enemy Shooter + Bullet

**Status:** Already implemented (minor migration needed)
**Scripts:** `Assets/Scripts/Enemy/EnemyShooter.cs`, `Assets/Scripts/Enemy/Bullet.cs`
**Prefab:** `Assets/Prefabs/Bullet.prefab`
**Dependencies:** Spec 04 (EnemyController — new Enemy prefab)

## Summary

Already implemented. Enemies fire yellow bullet spheres toward the player at configurable intervals. Bullets freeze mid-air when time stops. The only update needed is migrating EnemyShooter from the old test objects (DriftCube/SpinCube/SpinSphere) to the new Enemy prefab.

## Existing Behavior (No Code Changes)

**Bullet.cs:**
- `Initialize(Vector3 direction, float speed)` — sets rotation and speed
- `Update()` — moves forward via `Time.deltaTime` (auto-freezes), tracks lifetime, auto-destroys after 10s
- `OnTriggerEnter` — detects player hit, logs, destroys self

**EnemyShooter.cs:**
- Finds player via `FindWithTag("Player")` in Start
- Fires at intervals using `Time.deltaTime` timer
- Instantiates bullet prefab, calls `Initialize` with direction to player

## Inspector Fields

**EnemyShooter:**

| Field | Type | Default |
|-------|------|---------|
| `_bulletPrefab` | GameObject | Bullet prefab |
| `_fireInterval` | float | 2f |
| `_bulletSpeed` | float | 8f |

**Bullet prefab:**

| Property | Value |
|----------|-------|
| Mesh | Sphere, scale (0.15, 0.15, 0.15) |
| Material | YellowMat (255, 255, 50) |
| SphereCollider | isTrigger = true |
| Rigidbody | isKinematic = true, useGravity = false |
| Script | Bullet |
| Layer | "Bullet" |

## Migration Required

1. Remove EnemyShooter component from all 6 test objects (DriftCube1-3, SpinCube1-2, SpinSphere1)
2. Add EnemyShooter component to the Enemy prefab (alongside EnemyController from Spec 04)
3. Assign the Bullet prefab to the `_bulletPrefab` field on the Enemy prefab
4. Keep default fire interval (2f) and bullet speed (8f)

## Acceptance Criteria

1. New Enemy prefab fires bullets toward player at regular intervals
2. Bullets freeze mid-air when player stops
3. Bullets resume when player moves
4. Bullets that hit player trigger console log and destroy themselves
5. Bullets auto-despawn after 10 seconds of game-time
6. Old test objects no longer have EnemyShooter (cleaned up)

## Unity Setup & Test

1. Select the Enemy prefab in Assets/Prefabs/Enemy.prefab (or the test enemy from Spec 04)
2. Add Component → `EnemyShooter`
3. Drag Assets/Prefabs/Bullet.prefab into the `_bulletPrefab` field
4. Set Fire Interval = 2, Bullet Speed = 8
5. Apply changes to prefab
6. Remove EnemyShooter from DriftCube1, DriftCube2, DriftCube3, SpinCube1, SpinSphere1, SpinCube2
7. Enter Play mode — enemies should fire bullets toward you
8. Stand still — bullets freeze mid-air
9. Move — bullets resume flight
10. Walk into a bullet — Console shows "Player hit by bullet!", bullet disappears
