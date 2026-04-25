# Feature 02 — Player Movement

## Overview

Rigidbody-based first-person controller for Person B (the movement player). Uses WASD for movement and mouse for look, then reports movement state to TimeManager to drive the core SUPERHOT time-freeze mechanic. Mouse look uses raw input so it keeps working during freeze.

## Scripts

| Script | Path | Purpose |
|--------|------|---------|
| PlayerMovementController | Assets/Scripts/Player/PlayerMovementController.cs | FPS movement + mouse look, reports movement state to TimeManager |

## Inspector Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| _moveSpeed | float | 5f | Movement speed in units/second |
| _mouseSensitivity | float | 2f | Mouse look sensitivity multiplier |

## How It Works

1. `Start()` caches the Rigidbody (required component), finds the child Camera, freezes rigidbody rotation, and locks + hides the cursor.
2. `Update()` runs two methods each frame:
   - **HandleMouseLook()** — reads `Input.GetAxisRaw("Mouse X/Y")` multiplied by sensitivity (no deltaTime, so it works during freeze). Rotates the player body on Y axis, rotates the camera on X axis with pitch clamped to +/-90 degrees.
   - **HandleMovement()** — reads `Input.GetAxisRaw("Horizontal/Vertical")`, determines `hasInput` from magnitude, calls `TimeManager.Instance.SetMoving(hasInput)`. If moving, sets rigidbody velocity to world-space direction * speed. If stationary, zeros horizontal velocity (preserves Y for gravity).

## Dependencies

- Spec 01 (TimeManager) — must exist in scene for `SetMoving()` calls.

## Unity Setup & Test

1. Create a **Capsule** GameObject named **Player** at position `(0, 1, 0)`
2. Tag it as **"Player"** (built-in tag)
3. Add Component → **Rigidbody**: check Freeze Rotation X and Z (leave Y unfrozen)
4. Add Component → **PlayerMovementController**
5. Create a **Camera** as a child of Player at local position `(0, 0.5, 0)`
6. Delete the default Main Camera from the scene
7. Ensure **TimeManager** exists in the scene (Spec 01)
8. Enter Play mode
9. WASD should move the player; mouse should rotate the view
10. Release all keys — time freezes (check Console log)
11. Camera pitch should clamp and not flip past 90 degrees

## Known Limitations

- No jump or crouch for MVP.
- No sprint/speed variation.
- Mouse sensitivity not adjustable at runtime.
