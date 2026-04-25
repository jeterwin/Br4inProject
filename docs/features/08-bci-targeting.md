# Feature 08 — BCI Targeting System

## Overview

Bridges brain-computer interface (BCI) detection to in-game shooting. The system tracks P300 detections per enemy in a rolling confidence window and auto-fires a bullet when the threshold is reached. A debug simulator allows testing without BCI hardware by cycling through enemies automatically.

## Scripts

| Script | Path | Purpose |
|--------|------|---------|
| BCITargetingSystem | Assets/Scripts/BCI/BCITargetingSystem.cs | Confidence tracking, enemy registration, auto-fire |
| BCIDebugSimulator | Assets/Scripts/BCI/BCIDebugSimulator.cs | Simulates BCI detections by cycling through enemies |

## Inspector Fields

### BCITargetingSystem

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `_requiredDetections` | int | 4 | Detections needed within the window to fire |
| `_windowDuration` | float | 5f | Rolling time window in seconds (unscaled) |
| `_bulletPrefab` | GameObject | — | Bullet prefab to instantiate on fire |
| `_bulletSpeed` | float | 20f | Speed of fired bullets |
| `_fireOrigin` | Transform | — | Where bullets spawn (player camera) |
| `_debugMode` | bool | true | If true, skip real BCI subscription |

### BCIDebugSimulator

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `_cycleInterval` | float | 1.5f | Seconds between simulated detections (unscaled) |
| `_targetingSystem` | BCITargetingSystem | — | Reference to the targeting system |

## How It Works

1. **EnemySpawner** calls `RegisterEnemy(classId, enemy)` for each spawned enemy, populating the targeting system's dictionaries.
2. **Detection events** arrive via either:
   - **Debug mode:** `BCIDebugSimulator` cycles through active class IDs every 1.5s (unscaled time) and calls `OnClassSelected(classId)`.
   - **Real BCI mode:** `BCIManager.Instance.ClassSelectionAvailable` fires from the g.tec ERP pipeline and calls `OnClassSelected(classId)`.
3. **OnClassSelected** records a timestamp (`Time.unscaledTime`), prunes old entries outside the rolling window, and checks if the count meets the threshold.
4. **FireAtEnemy** instantiates a bullet **1.5 units forward** from the player camera (to prevent self-collision with the player's collider) aimed at the targeted enemy.
5. **On enemy death**, `EnemySpawner.OnEnemyDied` calls `UnregisterEnemy(classId)` to remove the enemy from targeting rotation.

All detection logic uses `Time.unscaledTime`/`Time.unscaledDeltaTime` so it works during time freeze.

## Dependencies

- **Spec 04 (Enemy Controller):** Enemies with `ClassId` property
- **Spec 05 (Bullet):** Reuses `Bullet` prefab and `Bullet.Initialize()`
- **Spec 07 (Enemy Spawner):** Registers/unregisters enemies with the targeting system
- **Spec 09 (Enemy Death FX):** Will trigger `UnregisterEnemy` via the death chain

## Unity Setup & Test

### Scene Setup
1. Create empty GameObject **"BCITargetingSystem"**
2. Add Component → `BCITargetingSystem`
3. Set: Required Detections = 4, Window Duration = 5, Bullet Speed = 20, Debug Mode = checked
4. Drag `Assets/Prefabs/Bullet.prefab` into **Bullet Prefab** field
5. Drag the Player's **Camera** (child of Player) into **Fire Origin** field
6. Create empty GameObject **"BCIDebugSimulator"**
7. Add Component → `BCIDebugSimulator`
8. Set Cycle Interval = 1.5
9. Drag the BCITargetingSystem GameObject into **Targeting System** field
10. Select the **EnemySpawner** GameObject → in the Inspector, drag the BCITargetingSystem GameObject into the new **Targeting System** field

### Testing WITHOUT BCI Hardware
1. Enter Play mode — enemies should spawn (via EnemySpawner)
2. Watch the Console — the debug simulator cycles detections every 1.5s
3. To see firing faster: reduce **Enemy Count** to 2 on the EnemySpawner, so detections accumulate faster per enemy
4. After enough detections within the window, a bullet should fire from the camera toward the targeted enemy
5. Stand still to verify detections still accumulate during time freeze (unscaled time)
6. Move to unfreeze and watch bullets fly

### Testing WITH BCI Hardware
1. Add `BCI Visual ERP 3D` prefab from `Assets/g.tec/Unity Interface/Prefabs/BCI/`
2. Use `DeviceDialog_UI` to connect Unicorn Hybrid Black
3. Complete training calibration
4. Uncheck **Debug Mode** on BCITargetingSystem
5. Start application mode — P300 detections feed into the same confidence pipeline

### Troubleshooting
- Bullets not firing → Check Required Detections vs cycle timing. With 6 enemies at 1.5s cycle, each enemy gets 1 detection per 9s. Reduce enemy count or increase window duration for testing.
- Bullets spawn but don't move → They use `Time.deltaTime`, so they only move when time flows (player is moving). This is correct SUPERHOT behavior.
- No detections logged → Ensure EnemySpawner has the BCITargetingSystem reference wired up.

## Known Limitations

- Real BCI integration (`BCIManager.Instance.ClassSelectionAvailable`) is stubbed — the `_debugMode` flag gates it but actual SDK subscription code is deferred until hardware testing
- No muzzle flash or fire sound effects for MVP
- With 6 enemies and default 1.5s cycle, auto-fire takes a while in debug mode — tune inspector values for faster testing
