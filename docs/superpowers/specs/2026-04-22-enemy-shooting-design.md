# Enemy Shooting — Design Spec

## Goal

Add shooting behavior to existing DriftingObject and SpinningObject enemies. Bullets are small yellow spheres that fly toward the player and freeze in mid-air when the player stops moving (SUPERHOT mechanic). Bullets despawn on contact with the player (console log, no damage system yet).

## New Scripts

### 1. Bullet (`Assets/Scripts/Enemy/Bullet.cs`)

MonoBehaviour on the bullet prefab.

**Inspector fields:**
- None exposed — configured via `Initialize()` at spawn time

**Private fields:**
- `float _speed` — set by Initialize
- `float _lifetime` — default 10f, accumulated via Time.deltaTime
- `float _age` — current accumulated time

**Public API:**
- `void Initialize(Vector3 direction, float speed)` — sets `transform.rotation = Quaternion.LookRotation(direction)` and stores speed

**Update():**
- `transform.Translate(Vector3.forward * _speed * Time.deltaTime)` — auto-freezes when timeScale = 0
- Accumulate `_age += Time.deltaTime`, destroy self when `_age >= _lifetime`

**OnTriggerEnter(Collider other):**
- If `other.CompareTag("Player")`: `Debug.Log("Player hit by bullet!")`, `Destroy(gameObject)`

### 2. EnemyShooter (`Assets/Scripts/Enemy/EnemyShooter.cs`)

MonoBehaviour attached alongside DriftingObject or SpinningObject.

**Inspector fields:**
- `GameObject _bulletPrefab` — reference to bullet prefab
- `float _fireInterval` — seconds between shots (default 2f)
- `float _bulletSpeed` — speed passed to bullet (default 8f)

**Private fields:**
- `Transform _playerTransform` — cached in Start()
- `float _timer` — accumulates Time.deltaTime

**Start():**
- `_playerTransform = GameObject.FindWithTag("Player").transform`

**Update():**
- `_timer += Time.deltaTime` (freezes with time)
- When `_timer >= _fireInterval`:
  - Calculate `direction = (_playerTransform.position - transform.position).normalized`
  - `Instantiate` bullet prefab at `transform.position`
  - Call `bullet.GetComponent<Bullet>().Initialize(direction, _bulletSpeed)`
  - Reset `_timer = 0f`

## Bullet Prefab (`Assets/Prefabs/Bullet.prefab`)

| Property | Value |
|----------|-------|
| Mesh | Sphere |
| Scale | (0.15, 0.15, 0.15) |
| Material | YellowMat (Assets/Materials/YellowMat) — bright yellow (255, 255, 50) |
| SphereCollider | isTrigger = true |
| Rigidbody | isKinematic = true, useGravity = false |
| Script | Bullet |
| Layer | Bullet |

## Scene Changes

1. Tag the Player capsule as "Player" (built-in Unity tag)
2. Create a "Bullet" physics layer
3. In Edit → Project Settings → Physics, disable Bullet-Bullet collisions
4. Add `EnemyShooter` component to all 6 existing enemy objects:

| Object | Fire Interval | Bullet Speed |
|--------|--------------|-------------|
| DriftCube1 | 1.5 | 8 |
| DriftCube2 | 2.0 | 8 |
| DriftCube3 | 1.5 | 8 |
| SpinCube1 | 2.0 | 8 |
| SpinSphere1 | 2.5 | 8 |
| SpinCube2 | 2.0 | 8 |

5. Drag Bullet prefab into the `_bulletPrefab` field on each EnemyShooter

## File Organization

```
Assets/Scripts/
├── Enemy/
│   ├── Bullet.cs
│   └── EnemyShooter.cs
Assets/Prefabs/
│   └── Bullet.prefab  (created in Unity Editor, not code)
Assets/Materials/
│   └── YellowMat       (created in Unity Editor, not code)
```

## Verification

1. Enter Play mode — stand still. No bullets should be fired (timer uses Time.deltaTime which is 0)
2. Press WASD — bullets start flying toward you from all red objects
3. Release keys — all bullets freeze mid-air instantly
4. Move again — frozen bullets resume their path
5. Walk into a bullet — console shows "Player hit by bullet!", bullet disappears
6. Bullets that miss should despawn after ~10 seconds of game-time
7. Bullets should not collide with each other
