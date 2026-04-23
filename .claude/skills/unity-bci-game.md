---
name: unity-bci-game
description: Use when writing game scripts — movement, enemies, time management, spawning, game state, shooting, or any gameplay logic for the SUPERHOT-style BCI game
---

# Unity BCI Game — Architecture & Patterns

## Time-Freeze Mechanic

Use `Time.timeScale` for the SUPERHOT effect:
- `Time.timeScale = 0f` when Person B (movement player) is stationary
- `Time.timeScale = 1f` when Person B moves
- All standard `Update()` code auto-freezes — enemies, projectiles, animations all stop
- BCI systems and UI MUST use `Time.unscaledDeltaTime` and `Time.unscaledTime` to keep running during freeze
- Physics also freezes (`FixedUpdate` stops), so rigidbody movement requires `Time.timeScale > 0`

### TimeManager Pattern

```csharp
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    
    public void SetMoving(bool isMoving)
    {
        Time.timeScale = isMoving ? 1f : 0f;
    }
}
```

`PlayerMovementController` calls `TimeManager.Instance.SetMoving()` each frame based on input.

## Game Loop

1. **Calibration phase**: Game starts here. BCI person trains P300 classifier via g.tec SDK. Movement player waits.
2. **Arena phase**: After successful calibration, enemies begin spawning.
3. **Gameplay cycle**:
   - Person B moves → time unfreezes → enemies move toward player and shoot
   - Person B stops → time freezes → player can assess the battlefield
   - P300 flashing runs continuously (on unscaled time) — BCI detects which enemy Person A attends to
   - Confidence accumulates in rolling window → auto-fire when threshold reached → enemy destroyed
4. **Wave progression**: All enemies in wave killed → next wave spawns with more/faster enemies
5. **Game over**: Player HP reaches 0

## Confidence-Based Auto-Fire

The `BCITargetingSystem` tracks detections per enemy in a rolling time window:
- Window duration: configurable (default ~5 seconds), measured in **unscaled time**
- Threshold: configurable (e.g., 4 out of 5 detections)
- On threshold reached: fire at that enemy (one-shot kill), reset confidence for that enemy
- Old detections outside the window are discarded each tick
- Visual feedback: highlight the enemy with highest current confidence

## Component Communication

- `TimeManager` ← called by `PlayerMovementController`
- `BCITargetingSystem` → fires events when enemy is targeted/killed
- `EnemyController` ← spawned by `EnemySpawner`, reports death to `GameManager`
- `GameManager` ← tracks score, health, wave state; listens to enemy deaths and player damage

Use C# events or UnityEvents for loose coupling between systems.

## Enemy Design (MVP)

- Simple capsule/cube geometry with a distinct color (red recommended for SUPERHOT feel)
- Move toward player position at configurable speed
- Shoot projectiles toward player at configurable interval
- One-shot killed when BCI auto-fire targets them
- Each enemy has an associated P300 flash stimulus (flash object from g.tec SDK)

## Player Health

- Player has HP (e.g., 3-5 hits)
- Enemy projectiles deal damage on collision
- Display HP on HUD (use `unscaledDeltaTime` for HUD updates)
- HP reaches 0 → game over screen

## Script Organization

All game scripts go in `Assets/Scripts/` with subfolders:
- `Core/` — TimeManager.cs, GameManager.cs
- `Player/` — PlayerMovementController.cs
- `Enemy/` — EnemyController.cs, EnemySpawner.cs, EnemyProjectile.cs
- `BCI/` — BCITargetingSystem.cs
- `UI/` — HUDController.cs, GameOverUI.cs, CalibrationUI.cs

## Unity Setup & Test Instructions — REQUIRED

**Every time you write or modify a game script, you MUST end your response with a `## Unity Setup & Test` section** containing step-by-step Unity Editor instructions. The users are not experienced with Unity — be explicit about every click. Never say "add the script to a GameObject" without specifying which GameObject and how.

### Template for each feature:

```
## Unity Setup & Test

### Scene Setup
1. Open `Assets/Scenes/SampleScene.unity`
2. [Create/select specific GameObjects...]
3. [Add specific components via Add Component...]
4. [Configure specific inspector fields with specific values...]
5. [Drag specific prefabs and parent them...]

### Testing
1. Press Play in the Unity Editor
2. [Specific inputs to give — e.g., "press W to move forward"]
3. [What to observe — e.g., "enemies should start moving when you walk"]
4. [How to verify success — e.g., "check Console for 'TimeScale: 1' logs"]

### Troubleshooting
- [Common issue] → [Fix]
```

### Per-feature examples of what to include:

**TimeManager**: Create empty GameObject named "TimeManager", add TimeManager.cs, verify in Console that timeScale toggles between 0 and 1 when pressing/releasing WASD.

**PlayerMovementController**: Create a Capsule named "Player", add Rigidbody (freeze rotation X/Z, constraints), add PlayerMovementController.cs, set Move Speed and Mouse Sensitivity in inspector, add a Camera as child, verify WASD moves and mouse rotates.

**EnemyController**: Create a Cube named "Enemy", set material to red, add EnemyController.cs, set Speed and Player reference in inspector, press Play, verify enemy walks toward player and freezes when player stops.

**EnemySpawner**: Create empty GameObject, add EnemySpawner.cs, assign enemy prefab and spawn points (empty GameObjects at arena edges), press Play, verify enemies spawn in waves.

**GameManager**: Create empty "GameManager", add GameManager.cs, wire references in inspector, verify game state transitions show in Console or UI.

**HUD/UI**: Create Canvas, add HUD elements (Text for HP, score), assign references in HUDController, verify values update during Play.

Always include Layer and Tag setup when collision detection is involved (e.g., "Enemy" tag on enemies, "Projectile" layer for bullets, Physics collision matrix settings).
