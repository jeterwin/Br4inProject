# SUPERHOT BCI Game — Spec Decomposition

## Overview

Complete feature decomposition for a co-op SUPERHOT-style first-person 3D shooter with g.tec BCI brain-computer interface targeting. Two players share one entity: Person A (BCI headset) targets enemies via P300 Visual ERP with confidence-based auto-fire; Person B (WASD + mouse) controls movement, which triggers the time-freeze mechanic.

This document defines 11 fine-grained spec files in dependency order. Each spec is self-contained and can be implemented independently once its dependencies are satisfied.

## Decisions

| Decision | Choice |
|----------|--------|
| Player damage model | One-hit kill (SUPERHOT-style) |
| Enemy death visual | Particle burst + destroy |
| Enemy AI complexity | NavMesh pathfinding + shoot |
| BCI debug mode | Auto-cycle simulation (no headset needed) |
| Wave structure | Single wave — spawn N enemies, kill all to win |
| Arena design | Flat arena with static obstacles (walls, pillars) for NavMesh |
| Spec granularity | Fine-grained (~11 files), dependency-ordered |

## Dependency Graph

```
01-time-manager ─────────────────────────────────────┐
02-player-movement ──(depends on 01)                 │
03-arena ────────────────────────────────────────┐   │
04-enemy-controller ──(depends on 01, 03)        │   │
05-enemy-shooter-bullet ──(depends on 04)        │   │
06-player-damage ──(depends on 02, 05)           │   │
07-enemy-spawner ──(depends on 03, 04)           │   │
08-bci-targeting ──(depends on 04)               │   │
09-enemy-death-fx ──(depends on 04, 08)          │   │
10-game-manager ──(depends on 06, 07, 08)        │   │
11-hud-ui ──(depends on 10)                      │   │
```

---

## Spec 01 — Time Manager

**Status:** Already implemented  
**File:** `docs/specs/01-time-manager.md`  
**Script:** `Assets/Scripts/Core/TimeManager.cs`  
**Dependencies:** None

### Summary

Singleton that controls `Time.timeScale`. When Person B stops moving, `timeScale = 0` freezes all standard `Update()` calls. When they move, `timeScale = 1` resumes everything. This is the core SUPERHOT mechanic.

### Behavior

- `SetMoving(bool isMoving)` — sets `Time.timeScale` to 1 or 0
- `IsTimeFrozen` property — returns true when timeScale == 0
- Singleton pattern: first instance persists, duplicates destroyed
- Debug logging on every state change

### Inspector Fields

None.

### Acceptance Criteria

1. Stand still in Play mode — Console shows "Time frozen", all Update-based motion stops
2. Press WASD — Console shows "Time resumed", motion resumes
3. Mouse look works during freeze (uses raw input, not deltaTime)

---

## Spec 02 — Player Movement

**Status:** Already implemented  
**File:** `docs/specs/02-player-movement.md`  
**Script:** `Assets/Scripts/Player/PlayerMovementController.cs`  
**Dependencies:** Spec 01 (TimeManager)

### Summary

Rigidbody-based first-person controller. WASD movement + mouse look on a Capsule with child Camera. Reports movement state to TimeManager.

### Behavior

- Read `Input.GetAxisRaw` for horizontal/vertical
- Determine `hasInput` from input magnitude > 0.01
- Call `TimeManager.Instance.SetMoving(hasInput)` before applying velocity
- Calculate world-space direction relative to player facing
- Set rigidbody velocity directly (not AddForce)
- Mouse look: rotate body Y, rotate camera X (clamped +/-90 degrees)
- Mouse uses raw input (no deltaTime scaling) — works during freeze
- Cursor locked and hidden on Start

### Inspector Fields

| Field | Type | Default |
|-------|------|---------|
| `_moveSpeed` | float | 5f |
| `_mouseSensitivity` | float | 2f |

### Acceptance Criteria

1. WASD moves the player in the direction they're facing
2. Mouse rotates view smoothly, camera pitch clamped
3. Standing still freezes time, moving resumes it
4. Cursor is locked to center of screen

---

## Spec 03 — Arena

**Status:** To build  
**File:** `docs/specs/03-arena.md`  
**Dependencies:** None

### Summary

Replace the current 10x10 flat ground plane with a larger arena containing static obstacles. The obstacles serve two purposes: (1) give NavMesh pathfinding something meaningful to navigate around, (2) provide cover for the player.

### Layout

- Ground plane scaled to 30x30 units (position 0,0,0)
- 4-6 concrete pillar obstacles (cubes, scale ~2x3x2) placed around the arena
- 2 low wall segments (cubes, scale ~6x2x1) providing partial cover
- All obstacles use a neutral grey material (different shade from ground)
- Arena boundary: invisible walls (box colliders, no mesh renderer) at edges to prevent falling off
- All obstacle objects are marked as **Navigation Static** for NavMesh baking

### NavMesh Setup

- Bake NavMesh in Navigation window (Window > AI > Navigation)
- Agent Radius: 0.5 (matches enemy capsule radius)
- Agent Height: 2.0
- Step Height: 0.4
- Verify baked mesh covers walkable ground and carves around obstacles

### Scene Hierarchy

```
Arena/
  Ground        — Plane, scale (30,1,30), grey material
  Pillar1       — Cube, (5, 1.5, 5), scale (2,3,2), Navigation Static
  Pillar2       — Cube, (-5, 1.5, -5), scale (2,3,2), Navigation Static
  Pillar3       — Cube, (8, 1.5, -3), scale (2,3,2), Navigation Static
  Pillar4       — Cube, (-7, 1.5, 6), scale (2,3,2), Navigation Static
  Wall1         — Cube, (0, 1, 8), scale (6,2,1), Navigation Static
  Wall2         — Cube, (-3, 1, -8), scale (6,2,1), Navigation Static
  BoundaryN     — Empty with BoxCollider, (0,1.5,15), scale (30,3,1)
  BoundaryS     — Empty with BoxCollider, (0,1.5,-15), scale (30,3,1)
  BoundaryE     — Empty with BoxCollider, (15,1.5,0), scale (1,3,30)
  BoundaryW     — Empty with BoxCollider, (-15,1.5,0), scale (1,3,30)
```

### Materials

| Material | Color (RGB) | Used On |
|----------|-------------|---------|
| GroundMat | (180, 180, 180) | Ground plane |
| ObstacleMat | (100, 100, 110) | Pillars and walls |

### Acceptance Criteria

1. Player can walk around the arena and collide with obstacles
2. Player cannot fall off the edges (invisible walls)
3. NavMesh is baked and visible in Scene view (blue overlay on walkable areas)
4. NavMesh correctly excludes obstacle footprints

---

## Spec 04 — Enemy Controller

**Status:** To build  
**File:** `docs/specs/04-enemy-controller.md`  
**Script:** `Assets/Scripts/Enemy/EnemyController.cs`  
**Dependencies:** Spec 01 (TimeManager), Spec 03 (Arena with NavMesh)

### Summary

Enemy AI using NavMeshAgent. Enemies navigate around obstacles toward the player. Movement auto-freezes because NavMeshAgent respects `Time.timeScale`. Replaces the current DriftingObject/SpinningObject test enemies.

### Behavior

- `NavMeshAgent` component handles pathfinding
- In `Update()`: set `agent.destination = playerTransform.position`
- NavMeshAgent speed, acceleration, and stopping distance configurable via inspector
- Stopping distance defines how close the enemy gets before halting (shooting range)
- Face the player when within stopping distance
- Auto-freezes via `Time.timeScale = 0` (NavMeshAgent pauses automatically)

### Inspector Fields

| Field | Type | Default |
|-------|------|---------|
| `_moveSpeed` | float | 3.5f |
| `_acceleration` | float | 8f |
| `_stoppingDistance` | float | 8f |

### Enemy Prefab (`Assets/Prefabs/Enemy.prefab`)

| Property | Value |
|----------|-------|
| Mesh | Capsule |
| Scale | (1, 1, 1) |
| Material | RedMat (existing) |
| NavMeshAgent | speed=3.5, acceleration=8, stoppingDistance=8, angularSpeed=120 |
| CapsuleCollider | default |
| EnemyController | script attached |
| EnemyShooter | script attached (from Spec 05) |
| Tag | "Enemy" |

### Script

```
Assets/Scripts/Enemy/EnemyController.cs
```

**Start():**
- Cache player transform via `FindWithTag("Player")`
- Get NavMeshAgent component
- Set agent properties from inspector fields

**Public API:**
- `int ClassId` — assigned by EnemySpawner at spawn time, used by BCITargetingSystem

**Update():**
- Set `agent.destination` to player position
- If within stopping distance: face the player (`transform.LookAt` on Y axis only)

### Acceptance Criteria

1. Enemy navigates around pillars/walls to reach the player
2. Enemy stops at stopping distance and faces the player
3. Enemy freezes in place when player stops moving (time frozen)
4. Enemy resumes pathfinding when player moves
5. Multiple enemies navigate simultaneously without overlapping

---

## Spec 05 — Enemy Shooter + Bullet

**Status:** Already implemented (minor update needed)  
**File:** `docs/specs/05-enemy-shooter-bullet.md`  
**Scripts:** `Assets/Scripts/Enemy/EnemyShooter.cs`, `Assets/Scripts/Enemy/Bullet.cs`  
**Dependencies:** Spec 04 (EnemyController)

### Summary

Already implemented. Enemies fire yellow bullet spheres toward the player at configurable intervals. Bullets freeze mid-air when time stops. Update needed: EnemyShooter should be moved from the test DriftingObject/SpinningObject enemies to the new Enemy prefab.

### Existing Behavior (No Changes)

**Bullet.cs:**
- `Initialize(Vector3 direction, float speed)` — sets rotation and speed
- `Update()` — moves forward via `Time.deltaTime` (auto-freezes), tracks lifetime, auto-destroys
- `OnTriggerEnter` — detects player hit, logs, destroys self

**EnemyShooter.cs:**
- Finds player via tag, fires at intervals using `Time.deltaTime` timer
- Instantiates bullet prefab, calls Initialize with direction to player

### Update Required

- Remove EnemyShooter from the 6 test objects (DriftCube/SpinCube/SpinSphere)
- Add EnemyShooter to the new Enemy prefab alongside EnemyController
- Keep same inspector fields and defaults

### Acceptance Criteria

1. New Enemy prefab fires bullets toward player at regular intervals
2. Bullets freeze mid-air when player stops
3. Bullets resume when player moves
4. Bullets that hit player trigger console log and destroy themselves
5. Bullets auto-despawn after 10 seconds of game-time

---

## Spec 06 — Player Damage

**Status:** To build  
**File:** `docs/specs/06-player-damage.md`  
**Script:** `Assets/Scripts/Player/PlayerDamage.cs`  
**Dependencies:** Spec 02 (PlayerMovement), Spec 05 (Bullets that hit player)

### Summary

One-hit kill system. When any bullet collides with the player, the player dies immediately. Death triggers a game-over event that GameManager listens to. No health bar needed.

### Behavior

- Attach to the Player GameObject
- Listen for bullet collision (the Bullet already has `OnTriggerEnter` that detects "Player" tag)
- Two approaches — either:
  - (A) Bullet calls into PlayerDamage via `GetComponent`, or
  - (B) PlayerDamage has its own `OnTriggerEnter` checking for "Bullet" layer
- **Use approach B** — PlayerDamage owns the death logic, Bullet just moves and despawns on contact

### Script

```
Assets/Scripts/Player/PlayerDamage.cs
```

**Private fields:**
- `bool _isDead` — prevents multiple death triggers
- `event System.Action OnPlayerDeath` — static event for GameManager to subscribe

**OnTriggerEnter(Collider other):**
- If `_isDead`, return
- If `other.gameObject.layer == bulletLayer`:
  - Set `_isDead = true`
  - Invoke `OnPlayerDeath`
  - Disable PlayerMovementController
  - Log "Player killed!"

### Bullet.cs Update

- Remove the player-hit detection from Bullet.OnTriggerEnter (PlayerDamage now handles it)
- Bullet still destroys itself on any trigger collision with "Player" tag

### Inspector Fields

None.

### Acceptance Criteria

1. Player hit by a bullet — "Player killed!" in console, movement stops
2. Only first hit counts (no duplicate death events)
3. Bullets still despawn on contact with player
4. Death event fires (testable by subscribing in a test script)

---

## Spec 07 — Enemy Spawner

**Status:** To build  
**File:** `docs/specs/07-enemy-spawner.md`  
**Script:** `Assets/Scripts/Enemy/EnemySpawner.cs`  
**Dependencies:** Spec 03 (Arena), Spec 04 (Enemy prefab with EnemyController)

### Summary

Single-wave spawner. Spawns N enemies at random positions along the arena edges. All enemies must be killed to win. Exposes events for GameManager.

### Behavior

- Spawn all enemies at wave start (called by GameManager)
- Spawn positions: random points along the 4 arena boundaries (offset inward by 2 units so enemies are on the NavMesh)
- Track alive enemy count
- When alive count reaches 0, fire `OnAllEnemiesDefeated` event

### Script

```
Assets/Scripts/Enemy/EnemySpawner.cs
```

**Inspector fields:**

| Field | Type | Default |
|-------|------|---------|
| `_enemyPrefab` | GameObject | Enemy prefab |
| `_enemyCount` | int | 6 |
| `_spawnYOffset` | float | 1f |

**Public API:**
- `void SpawnWave()` — instantiates `_enemyCount` enemies at random edge positions
- `void OnEnemyDestroyed()` — called when any enemy dies, decrements count
- `event System.Action OnAllEnemiesDefeated` — fires when count reaches 0

**SpawnWave():**
1. Pick random edge (N/S/E/W) for each enemy
2. Pick random position along that edge (inward by 2 units)
3. Instantiate enemy prefab at position
4. Assign sequential class ID (1, 2, 3...) to each enemy — stored on EnemyController, used by BCITargetingSystem to map BCI detections to enemies
5. Subscribe to each enemy's death event

**Spawn Positions (arena is 30x30, centered at origin):**
- North edge: x in [-13, 13], z = 13
- South edge: x in [-13, 13], z = -13
- East edge: x = 13, z in [-13, 13]
- West edge: x = -13, z in [-13, 13]

### Acceptance Criteria

1. Calling SpawnWave creates 6 enemies at the arena edges
2. All enemies are on the NavMesh and begin navigating toward player
3. Enemies don't spawn inside obstacles
4. When all enemies die, OnAllEnemiesDefeated fires
5. Spawner works correctly with time freeze (spawning itself is instant, not time-dependent)

---

## Spec 08 — BCI Targeting System

**Status:** To build  
**File:** `docs/specs/08-bci-targeting.md`  
**Scripts:** `Assets/Scripts/BCI/BCITargetingSystem.cs`, `Assets/Scripts/BCI/BCIDebugSimulator.cs`  
**Dependencies:** Spec 04 (Enemies to target)

### Summary

Bridges the g.tec ERP pipeline to game logic. In real mode, subscribes to `BCIManager.Instance.ClassSelectionAvailable` to receive P300 detection events — each event identifies which enemy (by class ID) the BCI user is focusing on. Maintains a rolling confidence window per enemy. When confidence threshold is reached, auto-fires a projectile at that enemy.

In debug mode (no headset), an auto-cycle simulator generates fake detection events to test the full pipeline.

### g.tec SDK Integration Pattern (from reference project)

The reference project (BCI-Hackaton-2023) shows the proven integration pattern:

1. **BCIManager** (singleton, non-MonoBehaviour) subscribes to `ERPBCIManager.Instance.ScoreValueAvailable`
2. It processes the score matrix to find the class with the highest score above a probability threshold (0.99)
3. It fires `ClassSelectionAvailable` event with the selected class ID
4. **BCIManager3D** (MonoBehaviour) orchestrates the full lifecycle:
   - Device connection via `ERPBCIManager.Instance.Initialize(serial)`
   - Training mode: `_flashController.StartFlashing(Mode.Training)` → `ERPBCIManager.Instance.Train()`
   - Classifier evaluation with auto-selected number of averages
   - Application mode: `ERPBCIManager.Instance.Configure(Mode.Application)` → continuous detection
5. **ERPFlashController3D** handles the visual flashing stimulus — flash objects map to game objects by class ID
6. **ERPBCIManager.Instance.SetTrigger(isTarget, id, trial, isLastOfTrial)** is called from flash trigger events

### Architecture

```
BCITargetingSystem (MonoBehaviour on BCI Visual ERP 3D prefab)
├── Subscribes to BCIManager.Instance.ClassSelectionAvailable
├── Maps class ID → Enemy GameObject
├── Maintains rolling confidence window per enemy
├── Auto-fires when confidence threshold reached
└── Uses Time.unscaledDeltaTime (works during freeze)

BCIDebugSimulator (MonoBehaviour, separate GameObject)
├── Auto-cycles through enemies on a timer
├── Fires fake class selection events into BCITargetingSystem
└── Uses Time.unscaledDeltaTime
```

### BCITargetingSystem Script

```
Assets/Scripts/BCI/BCITargetingSystem.cs
```

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
- If not `_debugMode`: subscribe to `BCIManager.Instance.ClassSelectionAvailable`
- Build `_classToEnemy` mapping from scene enemies (each enemy gets a class ID assigned by EnemySpawner)

**OnClassSelected(uint classId):** (called by BCIManager event or debug simulator)
- Record timestamp (`Time.unscaledTime`) in `_detectionTimestamps[classId]`
- Prune timestamps older than `_windowDuration`
- If count >= `_requiredDetections`: fire at that enemy, clear that enemy's timestamps

**FireAtEnemy(GameObject enemy):**
- Instantiate bullet at `_fireOrigin.position`
- Direction = `(enemy.transform.position - _fireOrigin.position).normalized`
- Call `bullet.Initialize(direction, _bulletSpeed)`
- No muzzle FX for MVP — just the bullet instantiation

**UnregisterEnemy(int classId):**
- Remove enemy from tracking when it dies
- Called by enemy death event

### BCIDebugSimulator Script

```
Assets/Scripts/BCI/BCIDebugSimulator.cs
```

**Inspector fields:**

| Field | Type | Default |
|-------|------|---------|
| `_cycleInterval` | float | 1.5f |
| `_targetingSystem` | BCITargetingSystem | reference |

**Behavior:**
- Uses `Time.unscaledDeltaTime` to tick even during freeze
- Cycles through registered enemies sequentially
- Every `_cycleInterval` seconds, calls `_targetingSystem.OnClassSelected(currentClassId)`
- Advances to next enemy, wraps around
- Skips dead enemies

### BCI Prefab Setup

The `BCI Visual ERP 3D` prefab from `Assets/g.tec/Unity Interface/Prefabs/BCI/` contains:
- ERPFlashController3D (manages flash stimulus objects)
- Canvas children for training/connection dialogs

For real BCI mode, each enemy needs an ERPFlashTag3D component that maps it to a class ID for the P300 stimulus.

### Acceptance Criteria

1. **Debug mode:** Auto-cycle simulator targets enemies sequentially
2. After enough detections in the window, a bullet fires from the player toward the targeted enemy
3. Confidence window rolls correctly (old detections expire)
4. Detection and firing work during time freeze (unscaledDeltaTime)
5. Dead enemies are removed from the targeting rotation
6. **Real BCI mode (with headset):** Class selection events from BCIManager trigger the same confidence pipeline

---

## Spec 09 — Enemy Death FX

**Status:** To build  
**File:** `docs/specs/09-enemy-death-fx.md`  
**Script:** `Assets/Scripts/Enemy/EnemyDeathHandler.cs`  
**Dependencies:** Spec 04 (EnemyController), Spec 08 (BCITargetingSystem fires at enemies)

### Summary

When an enemy is hit by a player bullet (fired by BCI targeting), it dies with a particle burst effect. The enemy is destroyed and the spawner is notified.

### Behavior

**Detecting the hit:**
- Enemy has a Collider (CapsuleCollider from EnemyController)
- Player bullets (from BCI targeting) use the same Bullet script and "Bullet" layer
- EnemyDeathHandler has `OnTriggerEnter`: if collider is on "Bullet" layer, trigger death

**Death sequence:**
1. Instantiate particle system at enemy position
2. Notify EnemySpawner (via event or direct call)
3. Notify BCITargetingSystem to unregister this enemy
4. Destroy the enemy GameObject

### Script

```
Assets/Scripts/Enemy/EnemyDeathHandler.cs
```

**Inspector fields:**

| Field | Type | Default |
|-------|------|---------|
| `_deathParticlePrefab` | GameObject | Particle system prefab |

**Public:**
- `event System.Action<EnemyDeathHandler> OnDeath` — EnemySpawner subscribes to this

**OnTriggerEnter(Collider other):**
- If `other.gameObject.layer == bulletLayer`:
  - Instantiate `_deathParticlePrefab` at `transform.position`
  - Invoke `OnDeath(this)`
  - Destroy `other.gameObject` (the bullet)
  - Destroy `gameObject` (the enemy)

### Particle Prefab (`Assets/Prefabs/EnemyDeathParticle.prefab`)

| Property | Value |
|----------|-------|
| Duration | 0.5s |
| Start Lifetime | 0.3–0.5s |
| Start Speed | 5–10 |
| Start Size | 0.1–0.3 |
| Start Color | Red (match enemy color) |
| Shape | Sphere, radius 0.5 |
| Emission | Burst of 20–30 particles |
| Renderer | Billboard, default particle material |
| Stop Action | Destroy |
| Play On Awake | true |

### Acceptance Criteria

1. Enemy hit by player bullet — particle burst appears at enemy position
2. Enemy is destroyed immediately
3. Particle effect plays for ~0.5s then auto-cleans
4. EnemySpawner count decrements (verified via console log or inspector)
5. BCITargetingSystem stops targeting the dead enemy
6. Particle effect plays correctly during time freeze (if bullet hits during freeze transition)

---

## Spec 10 — Game Manager

**Status:** To build  
**File:** `docs/specs/10-game-manager.md`  
**Script:** `Assets/Scripts/Core/GameManager.cs`  
**Dependencies:** Spec 06 (PlayerDamage), Spec 07 (EnemySpawner), Spec 08 (BCITargetingSystem)

### Summary

Singleton managing the game state machine. Three states: Calibration (BCI setup), Playing (active gameplay), and GameOver (win or lose). Controls flow between systems.

### State Machine

```
Calibration ──(BCI ready or debug skip)──> Playing
Playing ──(all enemies dead)──> GameOver (Win)
Playing ──(player hit)──> GameOver (Lose)
GameOver ──(restart input)──> Calibration
```

### Behavior

**Calibration state:**
- In debug mode: skip immediately to Playing
- In real BCI mode: wait for BCI connection and training to complete
- Display calibration UI (handled by HUD spec)
- Time is frozen during calibration

**Playing state:**
- Call `EnemySpawner.SpawnWave()`
- Enable PlayerMovementController
- Enable BCITargetingSystem
- Subscribe to `PlayerDamage.OnPlayerDeath` → GameOver (lose)
- Subscribe to `EnemySpawner.OnAllEnemiesDefeated` → GameOver (win)
- Track elapsed time (using `Time.unscaledTime` for accurate wall-clock)
- Track score (enemies killed)

**GameOver state:**
- Freeze time (`timeScale = 0`)
- Disable player movement
- Display win/lose screen with score and time
- Listen for restart input (R key) → reload scene

### Script

```
Assets/Scripts/Core/GameManager.cs
```

**Inspector fields:**

| Field | Type | Default |
|-------|------|---------|
| `_enemySpawner` | EnemySpawner | reference |
| `_bciTargeting` | BCITargetingSystem | reference |
| `_playerDamage` | PlayerDamage | reference |
| `_playerMovement` | PlayerMovementController | reference |
| `_debugMode` | bool | true |

**Public API:**
- `GameState CurrentState` — enum: Calibration, Playing, GameOver
- `int Score` — enemies killed
- `float ElapsedTime` — wall-clock seconds since Playing started
- `bool DidWin` — true if all enemies defeated, false if player died
- `event System.Action<GameState> OnStateChanged` — for HUD to subscribe

### Acceptance Criteria

1. Game starts in Calibration state (or skips to Playing in debug mode)
2. Enemies spawn when Playing begins
3. Killing all enemies shows win screen
4. Getting hit shows lose screen
5. Pressing R restarts the game
6. Score increments correctly
7. Timer displays accurate elapsed time

---

## Spec 11 — HUD / UI

**Status:** To build  
**File:** `docs/specs/11-hud-ui.md`  
**Scripts:** `Assets/Scripts/UI/GameHUD.cs`, `Assets/Scripts/UI/GameOverScreen.cs`  
**Dependencies:** Spec 10 (GameManager)

### Summary

Minimal UI overlay showing game state. A HUD during gameplay (enemy count, score, BCI target indicator) and a game-over screen (win/lose, score, restart prompt).

### HUD Elements (during Playing state)

| Element | Position | Content |
|---------|----------|---------|
| Enemy Counter | Top-right | "Enemies: 3/6" |
| Score | Top-left | "Score: 3" |
| BCI Target Indicator | Center | Crosshair or highlight on currently targeted enemy |
| Timer | Top-center | "00:42" elapsed time |

### Game Over Screen

| Element | Content |
|---------|---------|
| Title | "SUPER HOT" (win) or "ELIMINATED" (lose) |
| Score | Final score |
| Time | Total elapsed time |
| Restart Prompt | "Press R to restart" |

### Scripts

**GameHUD.cs** (`Assets/Scripts/UI/GameHUD.cs`):
- References TextMeshPro elements for each HUD field
- Subscribes to GameManager.OnStateChanged
- Updates enemy count from EnemySpawner
- Updates score from GameManager
- Updates timer from GameManager.ElapsedTime
- Uses `Time.unscaledDeltaTime` for UI updates during freeze

**GameOverScreen.cs** (`Assets/Scripts/UI/GameOverScreen.cs`):
- Hidden during Calibration and Playing
- Shown on GameOver state
- Displays win/lose title, score, time
- Listens for R key to restart

### Canvas Setup

- Canvas with Screen Space - Overlay
- Use TextMeshPro for all text (already in project dependencies)
- Separate panels for HUD and GameOver, toggled via SetActive

### Inspector Fields (GameHUD)

| Field | Type | Default |
|-------|------|---------|
| `_enemyCountText` | TextMeshProUGUI | reference |
| `_scoreText` | TextMeshProUGUI | reference |
| `_timerText` | TextMeshProUGUI | reference |
| `_gameOverPanel` | GameObject | reference |

### Acceptance Criteria

1. HUD displays during gameplay with correct enemy count
2. Score updates when enemies die
3. Timer counts up during gameplay
4. HUD updates even during time freeze
5. Game over screen appears on win/lose with correct info
6. Game over screen hidden during gameplay
7. R key restarts from game over screen

---

## Build Order (Implementation Roadmap)

| Phase | Specs | What You Get |
|-------|-------|-------------|
| 1 | 03 (Arena) | Playable arena with obstacles and baked NavMesh |
| 2 | 04 (EnemyController) | Enemies that pathfind toward player |
| 3 | 05 update (Shooter/Bullet migration) | Enemies that navigate AND shoot |
| 4 | 06 (PlayerDamage) | Player can die from bullet hits |
| 5 | 07 (EnemySpawner) | Enemies spawn at arena edges |
| 6 | 08 (BCITargeting) | BCI auto-fire system (with debug simulator) |
| 7 | 09 (EnemyDeathFX) | Enemies die with particle effects |
| 8 | 10 (GameManager) | Full game loop: start → play → win/lose |
| 9 | 11 (HUD/UI) | Score, timer, game over screen |

Specs 01 (TimeManager) and 02 (PlayerMovement) are already implemented — their spec files document existing behavior for reference.

## File Structure

```
docs/specs/
├── 01-time-manager.md
├── 02-player-movement.md
├── 03-arena.md
├── 04-enemy-controller.md
├── 05-enemy-shooter-bullet.md
├── 06-player-damage.md
├── 07-enemy-spawner.md
├── 08-bci-targeting.md
├── 09-enemy-death-fx.md
├── 10-game-manager.md
└── 11-hud-ui.md
```
