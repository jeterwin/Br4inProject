# SUPERHOT Time-Freeze Test Scene — Design Spec

## Goal

Create a minimal playable scene that demonstrates the core SUPERHOT mechanic: time freezes when the player stands still and resumes when they move. This validates the TimeManager + PlayerMovementController foundation before layering on enemies, BCI targeting, and game state.

## What's In the Scene

- Open flat plane arena (no walls)
- First-person player with WASD movement and mouse look
- 3–4 red cubes drifting across the arena at constant speed
- 2–3 cubes/spheres spinning in place
- All moving objects freeze when the player stops, resume when the player moves

## Scripts

### 1. TimeManager (`Assets/Scripts/Core/TimeManager.cs`)

Singleton MonoBehaviour. Controls `Time.timeScale`.

**Public API:**
- `static TimeManager Instance` — singleton accessor
- `void SetMoving(bool isMoving)` — sets `Time.timeScale` to `1f` or `0f`
- `bool IsTimeFrozen` — read-only property, `true` when `timeScale == 0`

**Behavior:**
- `Awake()`: singleton setup (destroy duplicate instances)
- Logs to Console on every freeze/unfreeze toggle for debugging

### 2. PlayerMovementController (`Assets/Scripts/Player/PlayerMovementController.cs`)

Rigidbody-based first-person controller on a Capsule.

**Inspector fields:**
- `float _moveSpeed` (default 5f)
- `float _mouseSensitivity` (default 2f)

**Movement (Update):**
1. Read `Input.GetAxisRaw("Horizontal")` and `Input.GetAxisRaw("Vertical")`
2. Determine `bool hasInput = moveInput.sqrMagnitude > 0.01f`
3. Call `TimeManager.Instance.SetMoving(hasInput)` — this sets `timeScale` before movement calc
4. Calculate world-space direction relative to player facing
5. Set `Rigidbody.velocity = moveDir * _moveSpeed` when moving, zero when not
6. Rigidbody with `isKinematic = false` handles collisions; gravity via Rigidbody settings

**Mouse Look (Update):**
- Read `Input.GetAxisRaw("Mouse X")` and `Input.GetAxisRaw("Mouse Y")`
- Multiply by `_mouseSensitivity` (no `deltaTime` scaling — raw mouse input is frame-independent)
- Rotate player body on Y axis, rotate child camera on X axis (clamped ±90°)
- Mouse look works even when frozen because it uses raw rotation, not physics

**Startup:**
- Lock and hide cursor: `Cursor.lockState = CursorLockMode.Locked`

**IsMoving detection:**
- Based on raw WASD input magnitude, not rigidbody velocity — pressing a key = "moving"

### 3. DriftingObject (`Assets/Scripts/Core/DriftingObject.cs`)

Translates in a direction at constant speed.

**Inspector fields:**
- `Vector3 _direction` (normalized movement direction)
- `float _speed` (default 2f)

**Update():**
- `transform.Translate(_direction.normalized * _speed * Time.deltaTime, Space.World)`
- Auto-freezes when `timeScale = 0` because `Time.deltaTime` becomes 0

### 4. SpinningObject (`Assets/Scripts/Core/SpinningObject.cs`)

Rotates around configurable axes.

**Inspector fields:**
- `Vector3 _rotationSpeed` (degrees/second per axis, default (0, 90, 0))

**Update():**
- `transform.Rotate(_rotationSpeed * Time.deltaTime)`
- Auto-freezes when `timeScale = 0`

## Scene Layout

| Object | Type | Position | Components | Notes |
|--------|------|----------|------------|-------|
| Ground | Plane | (0, 0, 0), scale (10, 1, 10) | Default | Grey/white |
| Player | Capsule | (0, 1, 0) | Rigidbody (freeze rot X/Z), PlayerMovementController | |
| Main Camera | Camera | (0, 0.5, 0) child of Player | Camera, AudioListener | Remove default Main Camera |
| TimeManager | Empty GO | origin | TimeManager | |
| DriftCube1 | Cube | (-5, 0.5, 3) | DriftingObject (dir: 1,0,0 speed: 2) | Red material |
| DriftCube2 | Cube | (3, 0.5, -5) | DriftingObject (dir: 0,0,1 speed: 3) | Red material |
| DriftCube3 | Cube | (0, 0.5, 5) | DriftingObject (dir: -1,0,0.5 speed: 1.5) | Red material |
| SpinCube1 | Cube | (4, 0.5, 4) | SpinningObject (rotSpeed: 0,90,0) | Red material |
| SpinSphere1 | Sphere | (-3, 1, -3) | SpinningObject (rotSpeed: 45,0,90) | Red material |
| SpinCube2 | Cube | (-4, 0.5, 2) | SpinningObject (rotSpeed: 0,0,120) | Red material |

## Verification

1. Enter Play mode
2. Stand still — all cubes/spheres should be frozen (not moving or rotating)
3. Press WASD — everything starts moving/spinning
4. Release keys — everything freezes again
5. Mouse look should work at all times (frozen or not)
6. Console should show "Time frozen" / "Time resumed" logs on each toggle
7. Camera should follow player smoothly, no jitter
