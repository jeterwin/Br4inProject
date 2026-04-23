# Spec 10 — Game Manager

**Status:** To build
**Script:** `Assets/Scripts/Core/GameManager.cs`
**Dependencies:** Spec 06 (PlayerDamage), Spec 07 (EnemySpawner), Spec 08 (BCITargetingSystem)

## Summary

Singleton managing the game state machine. Three states: Calibration (BCI setup), Playing (active gameplay), and GameOver (win or lose). Controls the flow between all game systems.

## State Machine

```
Calibration ──(BCI ready or debug skip)──> Playing
Playing ──(all enemies dead)──> GameOver (Win)
Playing ──(player hit)──> GameOver (Lose)
GameOver ──(R key pressed)──> Reload scene (back to Calibration)
```

## Behavior

**Calibration state:**
- In debug mode (`_debugMode = true`): skip immediately to Playing state on Start
- In real BCI mode: wait for BCI device connection and training to complete before transitioning
- Time is frozen during calibration (`Time.timeScale = 0`)

**Playing state:**
- Call `_enemySpawner.SpawnWave()` to spawn enemies
- Enable `PlayerMovementController`
- Enable `BCITargetingSystem` (or its debug simulator)
- Subscribe to `PlayerDamage.OnPlayerDeath` → transition to GameOver (lose)
- Subscribe to `EnemySpawner.OnAllEnemiesDefeated` → transition to GameOver (win)
- Track elapsed time using `Time.unscaledTime` for accurate wall-clock measurement
- Track score (incremented each time an enemy dies)

**GameOver state:**
- Set `Time.timeScale = 0` to freeze everything
- Disable player movement
- Set `DidWin` based on whether all enemies are dead or player died
- Listen for R key (`Input.GetKeyDown(KeyCode.R)`) to restart
- Restart = `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)`

## Script Details

**File:** `Assets/Scripts/Core/GameManager.cs`

**Singleton:** `public static GameManager Instance`

**Inspector fields:**

| Field | Type | Default |
|-------|------|---------|
| `_enemySpawner` | EnemySpawner | reference |
| `_bciTargeting` | BCITargetingSystem | reference |
| `_playerDamage` | PlayerDamage | reference |
| `_playerMovement` | PlayerMovementController | reference |
| `_debugMode` | bool | true |

**Public API:**
- `GameState CurrentState` — enum { Calibration, Playing, GameOver }
- `int Score` — read-only, enemies killed count
- `float ElapsedTime` — read-only, wall-clock seconds since Playing started
- `bool DidWin` — read-only, true if all enemies defeated
- `event System.Action<GameState> OnStateChanged` — HUD subscribes to this

**GameState enum:** defined in same file or separate file `Assets/Scripts/Core/GameState.cs`

**Key methods:**
- `TransitionTo(GameState state)` — handles enter/exit logic for each state, fires OnStateChanged
- `Update()` — in Playing state: track elapsed time. In GameOver state: listen for R key.

## Acceptance Criteria

1. Game starts in Calibration state (or auto-transitions to Playing in debug mode)
2. Enemies spawn when Playing state begins
3. Killing all enemies → transitions to GameOver with `DidWin = true`
4. Getting hit by a bullet → transitions to GameOver with `DidWin = false`
5. Pressing R during GameOver → scene reloads, fresh start
6. Score increments each time an enemy is killed
7. ElapsedTime tracks accurate wall-clock time during Playing state
8. OnStateChanged fires on every transition (HUD can react)

## Unity Setup & Test

1. Create an empty GameObject named "GameManager" in the scene
2. Add Component → `GameManager`
3. Check `_debugMode` = true
4. Drag the following into the inspector fields:
   - EnemySpawner object → `_enemySpawner`
   - BCITargetingSystem object → `_bciTargeting`
   - Player object (with PlayerDamage) → `_playerDamage`
   - Player object (with PlayerMovementController) → `_playerMovement`
5. Enter Play mode
6. In debug mode, game should immediately start Playing (enemies spawn)
7. Let BCI debug simulator kill all enemies — verify GameOver (win) transition
8. Press R — scene should reload
9. Test lose path: move into enemy bullets — verify GameOver (lose) transition
10. Check Console for state transition logs
11. Verify Score and ElapsedTime in the Inspector (expand GameManager component)
