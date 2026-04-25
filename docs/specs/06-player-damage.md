# Spec 06 — Player Damage

**Status:** To build
**Script:** `Assets/Scripts/Player/PlayerDamage.cs`
**Dependencies:** Spec 02 (PlayerMovement), Spec 05 (Bullets that hit player)

## Summary

One-hit kill system (SUPERHOT-style). When any bullet collides with the player, the player dies immediately. Death triggers a game-over event that GameManager listens to. No health bar needed.

## Behavior

- Attach to the Player GameObject
- PlayerDamage owns the death logic via its own `OnTriggerEnter` checking for "Bullet" layer
- Bullet still destroys itself on contact with "Player" tag (existing behavior)

## Script Details

**File:** `Assets/Scripts/Player/PlayerDamage.cs`

**Private fields:**
- `bool _isDead` — prevents multiple death triggers

**Public:**
- `static event System.Action OnPlayerDeath` — GameManager subscribes to this

**OnTriggerEnter(Collider other):**
- If `_isDead`, return early
- If `other.gameObject.layer` matches the "Bullet" layer:
  - Set `_isDead = true`
  - Invoke `OnPlayerDeath`
  - Disable `PlayerMovementController` via `GetComponent<PlayerMovementController>().enabled = false`
  - `Debug.Log("Player killed!")`

## Bullet.cs Update

Remove the player-specific hit detection from Bullet's `OnTriggerEnter`. The bullet should still destroy itself when it hits the player (check for "Player" tag), but the death logic is now in PlayerDamage. This means:

- Bullet `OnTriggerEnter`: if `other.CompareTag("Player")` → `Destroy(gameObject)` (just self-destruct, no log)
- PlayerDamage `OnTriggerEnter`: handles death logic when bullet layer detected

## Inspector Fields

None.

## Acceptance Criteria

1. Player hit by a bullet — "Player killed!" appears in Console, movement stops
2. Only the first hit counts (no duplicate death events from multiple bullets)
3. Bullets still despawn on contact with player
4. `OnPlayerDeath` event fires (verifiable by subscribing in a test script or checking GameManager later)

## Unity Setup & Test

1. Select the Player GameObject in the hierarchy
2. Add Component → `PlayerDamage`
3. Ensure the Player has a Collider (CapsuleCollider should already exist from the Capsule shape)
4. Ensure the "Bullet" layer exists (should already be set up from Spec 05)
5. Enter Play mode
6. Move to trigger enemy fire, then stop — bullets freeze mid-air
7. Walk into a frozen bullet — Console should show "Player killed!"
8. WASD should stop working after death (PlayerMovementController disabled)
9. Try walking into more bullets after death — no additional death logs should appear
