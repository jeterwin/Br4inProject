# Feature 07 — Enemy Spawner

## Overview

The Enemy Spawner creates a wave of enemies at random positions along the arena edges and tracks how many remain alive. When all enemies are eliminated, it fires an event that GameManager (spec 10) will use to trigger win conditions.

## Scripts

| Script | Path | Purpose |
|--------|------|---------|
| EnemySpawner | Assets/Scripts/Enemy/EnemySpawner.cs | Spawns enemy waves at arena edges, tracks alive count, fires defeat event |
| EnemyDeathHandler | Assets/Scripts/Enemy/EnemyDeathHandler.cs | Stub for enemy death — provides OnDeath event for spawner subscription (completed by spec 09) |

## Inspector Fields

### EnemySpawner

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `_enemyPrefab` | GameObject | — | The enemy prefab to instantiate |
| `_enemyCount` | int | 6 | Number of enemies per wave |
| `_spawnYOffset` | float | 1f | Y position offset to keep capsules above ground |

### EnemyDeathHandler

No inspector fields (stub — spec 09 adds `_deathParticlePrefab`).

## How It Works

1. **GameManager** (or a test script) calls `SpawnWave()` on the EnemySpawner.
2. For each enemy, the spawner picks a random edge (N/S/E/W) and a random position along that edge. Positions are at ±13 units from center — 2 units inward from the 30×30 arena boundary, ensuring enemies land on the NavMesh.
3. Each enemy is instantiated and assigned a sequential `ClassId` (1, 2, 3, ...) via `EnemyController.ClassId`.
4. The spawner subscribes to each enemy's `EnemyDeathHandler.OnDeath` event.
5. When an enemy dies, `OnEnemyDied` decrements the alive count and unsubscribes from the event.
6. When the alive count reaches zero, `OnAllEnemiesDefeated` fires.

**Edge positions (arena 30×30, centered at origin):**
- North: x ∈ [-13, 13], z = 13
- South: x ∈ [-13, 13], z = -13
- East: x = 13, z ∈ [-13, 13]
- West: x = -13, z ∈ [-13, 13]

## Dependencies

- **Spec 03 (Arena):** 30×30 arena with NavMesh baked
- **Spec 04 (EnemyController):** Enemy prefab with NavMeshAgent and EnemyController
- **Spec 09 (Enemy Death FX):** Will complete EnemyDeathHandler with trigger detection and particle effects

## Unity Setup & Test

1. Create an empty GameObject in the Hierarchy named **"EnemySpawner"**
2. Add Component → `EnemySpawner`
3. Drag `Assets/Prefabs/TestEnemy.prefab` into the **Enemy Prefab** field
4. Set **Enemy Count** = 6, **Spawn Y Offset** = 1
5. Open the Enemy prefab (double-click `Assets/Prefabs/TestEnemy.prefab`)
6. Add Component → `EnemyDeathHandler` to the prefab, then Apply
7. For quick testing without GameManager: create a small test script:
   ```csharp
   public class SpawnerTest : MonoBehaviour
   {
       [SerializeField] private EnemySpawner _spawner;
       private void Start() => _spawner.SpawnWave();
   }
   ```
   Attach it to the EnemySpawner GameObject and drag the EnemySpawner component into its field.
8. Enter **Play Mode** — 6 enemies should appear near the arena edges
9. Open **Scene View** and verify enemies are on the NavMesh (blue overlay)
10. Verify enemies navigate toward the Player
11. Check the Console for any errors
12. To test death tracking: select an enemy in the Hierarchy during Play mode, delete it manually, and watch `AliveCount` in the EnemySpawner inspector (Debug mode). When all are removed, `OnAllEnemiesDefeated` should fire (add a Debug.Log subscriber to verify).

## Known Limitations

- Single-wave only — no multi-wave progression (GameManager spec 10 may extend this)
- No spawn animation or delay between individual enemy spawns
- EnemyDeathHandler is a stub until spec 09 is implemented — death tracking requires spec 09's trigger collision logic to function in gameplay
