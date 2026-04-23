# Spec 04 — Enemy Controller

**Status:** To build
**Script:** `Assets/Scripts/Enemy/EnemyController.cs`
**Dependencies:** Spec 01 (TimeManager), Spec 03 (Arena with NavMesh)

## Summary

Enemy AI using NavMeshAgent. Enemies navigate around obstacles toward the player. Movement auto-freezes because NavMeshAgent respects `Time.timeScale`. Replaces the current DriftingObject/SpinningObject test enemies.

## Behavior

- `NavMeshAgent` component handles pathfinding
- In `Update()`: set `agent.destination = playerTransform.position`
- NavMeshAgent speed, acceleration, and stopping distance configurable via inspector
- Stopping distance defines how close the enemy gets before halting (shooting range)
- Face the player when within stopping distance
- Auto-freezes via `Time.timeScale = 0` (NavMeshAgent pauses automatically)

## Inspector Fields

| Field | Type | Default |
|-------|------|---------|
| `_moveSpeed` | float | 3.5f |
| `_acceleration` | float | 8f |
| `_stoppingDistance` | float | 8f |

## Public API

- `int ClassId { get; set; }` — assigned by EnemySpawner at spawn time, used by BCITargetingSystem to map BCI detections to this enemy

## Script Details

**File:** `Assets/Scripts/Enemy/EnemyController.cs`

**Requires:** `[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]`

**Start():**
- Cache player transform via `FindWithTag("Player")`
- Get NavMeshAgent component
- Set `agent.speed = _moveSpeed`, `agent.acceleration = _acceleration`, `agent.stoppingDistance = _stoppingDistance`

**Update():**
- `agent.destination = _playerTransform.position`
- If `agent.remainingDistance <= agent.stoppingDistance`: rotate toward player on Y axis only

## Enemy Prefab (`Assets/Prefabs/Enemy.prefab`)

| Property | Value |
|----------|-------|
| Mesh | Capsule |
| Scale | (1, 1, 1) |
| Material | RedMat (existing, Assets/Materials/RedMat) |
| NavMeshAgent | speed=3.5, acceleration=8, stoppingDistance=8, angularSpeed=120 |
| CapsuleCollider | default (center 0,1,0, radius 0.5, height 2) |
| EnemyController | script attached |
| Tag | "Enemy" |

## Acceptance Criteria

1. Enemy navigates around pillars/walls to reach the player
2. Enemy stops at stopping distance and faces the player
3. Enemy freezes in place when player stops moving (time frozen)
4. Enemy resumes pathfinding when player moves
5. Multiple enemies navigate simultaneously without overlapping

## Unity Setup & Test

1. Ensure the Arena (Spec 03) is set up with baked NavMesh
2. Create a Capsule in the scene at position (10, 1, 10), name it "TestEnemy"
3. Add Component → Nav Mesh Agent (set speed=3.5, acceleration=8, stoppingDistance=8, angularSpeed=120)
4. Add Component → `EnemyController`
5. Apply the existing RedMat material
6. Tag it as "Enemy" (create the tag if it doesn't exist: Edit > Project Settings > Tags and Layers)
7. Ensure the Player exists and is tagged "Player"
8. Enter Play mode — the enemy should navigate toward the player, going around obstacles
9. Stop moving (release WASD) — enemy should freeze in place
10. Move again — enemy should resume navigation
11. Verify the enemy stops at ~8 units from the player and faces them
12. Optionally duplicate the enemy 2-3 times at different positions to test multiple NavMesh agents
13. To create the prefab: drag TestEnemy from Hierarchy into Assets/Prefabs/ to create Enemy.prefab
14. Common issue: if enemy doesn't move, verify NavMesh is baked and the enemy is placed on a NavMesh surface
