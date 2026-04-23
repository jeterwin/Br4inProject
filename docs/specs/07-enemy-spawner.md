# Spec 07 — Enemy Spawner

**Status:** To build
**Script:** `Assets/Scripts/Enemy/EnemySpawner.cs`
**Dependencies:** Spec 03 (Arena), Spec 04 (Enemy prefab with EnemyController)

## Summary

Single-wave spawner. Spawns N enemies at random positions along the arena edges. All enemies must be killed to win. Exposes events for GameManager to monitor.

## Behavior

- Spawn all enemies at wave start (called by GameManager, or manually for testing)
- Spawn positions: random points along the 4 arena boundaries, offset inward by 2 units so enemies are on the NavMesh
- Track alive enemy count
- When alive count reaches 0, fire `OnAllEnemiesDefeated` event

## Script Details

**File:** `Assets/Scripts/Enemy/EnemySpawner.cs`

**Inspector fields:**

| Field | Type | Default |
|-------|------|---------|
| `_enemyPrefab` | GameObject | Enemy prefab |
| `_enemyCount` | int | 6 |
| `_spawnYOffset` | float | 1f |

**Public API:**
- `void SpawnWave()` — instantiates `_enemyCount` enemies at random edge positions
- `event System.Action OnAllEnemiesDefeated` — fires when alive count reaches 0
- `int AliveCount` — read-only, current number of alive enemies
- `int TotalCount` — read-only, total enemies spawned this wave

**SpawnWave() logic:**
1. For each enemy to spawn:
   a. Pick random edge (N/S/E/W)
   b. Pick random position along that edge
   c. Instantiate enemy prefab at that position
   d. Assign sequential class ID (1, 2, 3...) via `enemy.GetComponent<EnemyController>().ClassId = i + 1`
   e. Subscribe to the enemy's `EnemyDeathHandler.OnDeath` event
2. Set `_aliveCount = _enemyCount`

**OnEnemyDied(EnemyDeathHandler enemy):**
- Decrement `_aliveCount`
- If `_aliveCount <= 0`: invoke `OnAllEnemiesDefeated`

**Spawn positions (arena is 30x30, centered at origin):**
- North edge: x in [-13, 13], z = 13
- South edge: x in [-13, 13], z = -13
- East edge: x = 13, z in [-13, 13]
- West edge: x = -13, z in [-13, 13]
- Y = `_spawnYOffset` (default 1, keeps capsule above ground)

## Acceptance Criteria

1. Calling SpawnWave creates 6 enemies at the arena edges
2. All enemies are on the NavMesh and begin navigating toward player
3. Enemies don't spawn inside obstacles (edge positions are far from center obstacles)
4. When all enemies die, OnAllEnemiesDefeated fires
5. Spawner works correctly with time freeze (spawning itself is instant, not time-dependent)
6. Each enemy gets a unique class ID

## Unity Setup & Test

1. Create an empty GameObject named "EnemySpawner" in the scene
2. Add Component → `EnemySpawner`
3. Drag Assets/Prefabs/Enemy.prefab into the `_enemyPrefab` field
4. Set Enemy Count = 6, Spawn Y Offset = 1
5. For quick testing without GameManager: add a small test script that calls `SpawnWave()` on Start, or call it from the Inspector debug mode
6. Enter Play mode — 6 enemies should appear at the arena edges
7. Verify in Scene view that enemies are placed on the NavMesh (blue overlay)
8. Verify enemies begin navigating toward the player
9. Check Console for spawn logs
10. To test OnAllEnemiesDefeated: manually destroy enemies in the Inspector and check if the event fires when the last one is removed
