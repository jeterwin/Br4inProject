# Spec 08 — BCI Targeting System

**Status:** To build
**Scripts:** `Assets/Scripts/BCI/BCITargetingSystem.cs`, `Assets/Scripts/BCI/BCIDebugSimulator.cs`
**Dependencies:** Spec 04 (Enemies to target)

## Summary

Bridges the g.tec ERP pipeline to game logic. In real mode, subscribes to `BCIManager.Instance.ClassSelectionAvailable` to receive P300 detection events — each event identifies which enemy (by class ID) the BCI user is focusing on. Maintains a rolling confidence window per enemy. When confidence threshold is reached, auto-fires a projectile at that enemy.

In debug mode (no headset), an auto-cycle simulator generates fake detection events to test the full pipeline.

## g.tec SDK Integration Pattern

From the reference project (BCI-Hackaton-2023-Spring-School), the proven integration flow is:

1. **BCIManager** (singleton, non-MonoBehaviour in `Gtec.UnityInterface` namespace) subscribes to `ERPBCIManager.Instance.ScoreValueAvailable`
2. It processes the score matrix to find the class with the highest score above a probability threshold (0.99)
3. It fires `ClassSelectionAvailable` event with the selected class ID (uint)
4. **BCIManager3D** (MonoBehaviour) orchestrates the lifecycle:
   - Device connection: `ERPBCIManager.Instance.Initialize(serial)` (on a background thread)
   - Training mode: `_flashController.StartFlashing(Mode.Training)` → when stopped → `ERPBCIManager.Instance.Train()`
   - Classifier evaluation: auto-selects number of averages based on accuracy
   - Application mode: `ERPBCIManager.Instance.Configure(Mode.Application)` → continuous P300 detection
5. **ERPFlashController3D** handles visual flashing stimulus — flash objects map to game objects by class ID
6. Trigger forwarding: `ERPBCIManager.Instance.SetTrigger(isTarget, id, trial, isLastOfTrial)` called from flash events

## Architecture

```
BCITargetingSystem (MonoBehaviour)
├── Subscribes to BCIManager.Instance.ClassSelectionAvailable
├── Maps class ID → Enemy GameObject (via EnemyController.ClassId)
├── Maintains rolling confidence window per enemy
├── Auto-fires when confidence threshold reached
└── Uses Time.unscaledTime (works during freeze)

BCIDebugSimulator (MonoBehaviour, separate GameObject)
├── Auto-cycles through registered enemies on a timer
├── Calls BCITargetingSystem.OnClassSelected() with fake detections
└── Uses Time.unscaledDeltaTime (works during freeze)
```

## BCITargetingSystem Script

**File:** `Assets/Scripts/BCI/BCITargetingSystem.cs`

**Inspector fields:**

| Field | Type | Default |
|-------|------|---------|
| `_requiredDetections` | int | 4 |
| `_windowDuration` | float | 5f |
| `_bulletPrefab` | GameObject | Bullet prefab (reuse existing) |
| `_bulletSpeed` | float | 20f |
| `_fireOrigin` | Transform | Player camera transform |
| `_debugMode` | bool | true |

**Private fields:**
- `Dictionary<int, List<float>> _detectionTimestamps` — rolling window per class ID
- `Dictionary<int, GameObject> _classToEnemy` — maps class ID to enemy GameObject

**Start():**
- If not `_debugMode`: subscribe to `BCIManager.Instance.ClassSelectionAvailable += OnBCIClassSelected`
- Initialize dictionaries

**RegisterEnemy(int classId, GameObject enemy):**
- Add to `_classToEnemy` mapping
- Initialize empty timestamp list in `_detectionTimestamps`
- Called by EnemySpawner after spawning each enemy

**OnClassSelected(uint classId):** (called by BCIManager event or debug simulator)
- If `classId` not in `_classToEnemy` or enemy is null, return
- Record timestamp (`Time.unscaledTime`) in `_detectionTimestamps[classId]`
- Prune timestamps older than `_windowDuration` from current unscaled time
- If remaining count >= `_requiredDetections`: call `FireAtEnemy`, clear that enemy's timestamps

**FireAtEnemy(GameObject enemy):**
- Instantiate bullet at `_fireOrigin.position + direction * 1.5f` (offset prevents self-collision with player)
- Direction = `(enemy.transform.position - _fireOrigin.position).normalized`
- Call `bullet.GetComponent<Bullet>().Initialize(direction, _bulletSpeed)`
- No muzzle FX for MVP — just the bullet instantiation

**UnregisterEnemy(int classId):**
- Remove from `_classToEnemy` and `_detectionTimestamps`
- Called when enemy dies (from EnemyDeathHandler.OnDeath event chain)

## BCIDebugSimulator Script

**File:** `Assets/Scripts/BCI/BCIDebugSimulator.cs`

**Inspector fields:**

| Field | Type | Default |
|-------|------|---------|
| `_cycleInterval` | float | 1.5f |
| `_targetingSystem` | BCITargetingSystem | reference |

**Private fields:**
- `float _timer` — accumulated unscaled time
- `int _currentIndex` — index into the enemy list
- `List<int> _activeClassIds` — class IDs of alive enemies

**Behavior:**
- Uses `Time.unscaledDeltaTime` to tick even during time freeze
- Accumulates `_timer += Time.unscaledDeltaTime`
- When `_timer >= _cycleInterval`:
  - Get current active class IDs from `_targetingSystem`
  - If list is empty, return
  - Call `_targetingSystem.OnClassSelected((uint)_activeClassIds[_currentIndex])`
  - Advance `_currentIndex`, wrap around
  - Reset timer

## BCI Prefab Notes (Real Mode)

For real BCI mode (not debug), the setup requires:
- `BCI Visual ERP 3D` prefab from `Assets/g.tec/Unity Interface/Prefabs/BCI/` added to scene
- Each enemy needs an ERPFlashTag3D mapping it to a class ID for the P300 stimulus
- BCIManager.Instance must be initialized with `_flashController.NumberOfClasses`
- Connection, training, and application mode are handled by a BCIManager3D-style orchestrator

This real-mode integration can be layered on after the debug simulator proves the confidence/fire pipeline works.

## Acceptance Criteria

1. **Debug mode:** Auto-cycle simulator targets enemies sequentially every 1.5s
2. After 4 detections within 5 seconds for one enemy, a bullet fires from the player camera toward that enemy
3. Confidence window rolls correctly — detections older than 5s are pruned and don't count
4. Detection and firing work during time freeze (`Time.unscaledTime` / `Time.unscaledDeltaTime`)
5. Dead enemies are removed from the targeting rotation and dictionary
6. **Real BCI mode (with headset):** ClassSelectionAvailable events from BCIManager trigger the same confidence pipeline

## Unity Setup & Test

1. Create empty GameObject "BCITargetingSystem"
2. Add Component → `BCITargetingSystem`
3. Set Required Detections = 4, Window Duration = 5, Bullet Speed = 20, Debug Mode = checked
4. Drag Bullet prefab into `_bulletPrefab` field
5. Drag the Player's Camera (child of Player) into `_fireOrigin` field
6. Create empty GameObject "BCIDebugSimulator"
7. Add Component → `BCIDebugSimulator`
8. Set Cycle Interval = 1.5
9. Drag BCITargetingSystem object into `_targetingSystem` field
10. Ensure enemies are spawned (via EnemySpawner or manually placed)
11. Enter Play mode
12. Watch the Console — you should see detection events being fired cyclically
13. After ~6 seconds (4 detections at 1.5s intervals), a bullet should fire toward one of the enemies
14. If time is frozen (standing still), detections should still accumulate (unscaled time)
15. Verify that after an enemy is killed, it's removed from the cycle
