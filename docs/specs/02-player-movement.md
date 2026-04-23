# Spec 02 — Player Movement

**Status:** Already implemented
**Script:** `Assets/Scripts/Player/PlayerMovementController.cs`
**Dependencies:** Spec 01 (TimeManager)

## Summary

Rigidbody-based first-person controller. WASD movement + mouse look on a Capsule with child Camera. Reports movement state to TimeManager.

## Behavior

- Read `Input.GetAxisRaw` for horizontal/vertical
- Determine `hasInput` from input magnitude > 0.01
- Call `TimeManager.Instance.SetMoving(hasInput)` before applying velocity
- Calculate world-space direction relative to player facing
- Set rigidbody velocity directly (not AddForce)
- Mouse look: rotate body Y, rotate camera X (clamped +/-90 degrees)
- Mouse uses raw input (no deltaTime scaling) — works during freeze
- Cursor locked and hidden on Start

## Inspector Fields

| Field | Type | Default |
|-------|------|---------|
| `_moveSpeed` | float | 5f |
| `_mouseSensitivity` | float | 2f |

## Acceptance Criteria

1. WASD moves the player in the direction they're facing
2. Mouse rotates view smoothly, camera pitch clamped at +/-90 degrees
3. Standing still freezes time, moving resumes it
4. Cursor is locked to center of screen

## Unity Setup & Test

1. Create a Capsule GameObject named "Player" at position (0, 1, 0)
2. Tag it as "Player" (built-in tag)
3. Add Component → Rigidbody: set Freeze Rotation X and Z (leave Y unfrozen)
4. Add Component → `PlayerMovementController`
5. Create a Camera as a child of Player at local position (0, 0.5, 0)
6. Delete the default Main Camera from the scene
7. Ensure TimeManager exists in the scene (Spec 01)
8. Enter Play mode
9. WASD should move the player; mouse should rotate the view
10. Release all keys — time freezes (check Console log)
11. Camera pitch should clamp and not flip past 90 degrees
