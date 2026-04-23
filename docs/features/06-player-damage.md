# Feature 06 — Player Damage

## Overview

One-hit kill system in SUPERHOT style. When any bullet hits the player, they die immediately — movement is disabled and a static `OnPlayerDeath` event fires for GameManager to listen to. No health bar, no damage numbers, just instant death.

## Scripts

| Script | Path | Purpose |
|--------|------|---------|
| PlayerDamage | Assets/Scripts/Player/PlayerDamage.cs | Detects bullet hits, triggers death, disables movement |
| Bullet (modified) | Assets/Scripts/Enemy/Bullet.cs | Removed player-hit log — self-destructs silently on Player tag |

## Inspector Fields

None.

## Public API

| Member | Type | Description |
|--------|------|-------------|
| OnPlayerDeath | static event System.Action | Fires once on first bullet hit — GameManager subscribes to trigger game over |

## How It Works

1. `PlayerDamage` is attached to the Player GameObject alongside `PlayerMovementController`.
2. `OnTriggerEnter()` checks if the collider's layer is "Bullet" (using `LayerMask.NameToLayer`).
3. On first hit: sets `_isDead = true`, invokes `OnPlayerDeath`, disables `PlayerMovementController`, and logs "Player killed!".
4. Subsequent hits are ignored via the `_isDead` guard.
5. Meanwhile, `Bullet.OnTriggerEnter()` still detects the "Player" tag and self-destructs — but no longer logs, since death logic now lives in PlayerDamage.

## Dependencies

- Spec 02 (PlayerMovement) — PlayerDamage disables PlayerMovementController on death.
- Spec 05 (Bullets) — Bullets must be on the "Bullet" layer for detection.

## Unity Setup & Test

1. Select the **Player** GameObject in the hierarchy
2. Add Component → **PlayerDamage**
3. Ensure the Player has a Collider (CapsuleCollider should already exist from the Capsule shape)
4. Ensure the **"Bullet" layer** exists (should already be set up from Spec 05)
5. Enter Play mode
6. Move to trigger enemy fire, then stop — bullets freeze mid-air
7. Walk into a frozen bullet — Console should show **"Player killed!"**
8. WASD should stop working after death (PlayerMovementController disabled)
9. Try walking into more bullets after death — no additional death logs should appear

## Known Limitations

- No death animation or visual feedback for MVP.
- No respawn mechanic — requires GameManager (Spec 10) to restart the game.
- Mouse look still works after death (only movement is disabled).
