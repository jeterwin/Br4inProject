# Feature 04 — Enemy Controller

## Overview

Enemy AI using Unity's NavMeshAgent for pathfinding. Enemies navigate around arena obstacles toward the player, stop at a configurable distance, and face them. Movement auto-freezes when `Time.timeScale = 0` because NavMeshAgent respects the time system. Each enemy carries a `ClassId` used by the BCI targeting system to map P300 detections to specific enemies.

## Scripts

| Script | Path | Purpose |
|--------|------|---------|
| EnemyController | Assets/Scripts/Enemy/EnemyController.cs | NavMeshAgent-based AI that chases and faces the player |

## Inspector Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| _moveSpeed | float | 3.5f | NavMeshAgent movement speed |
| _acceleration | float | 8f | NavMeshAgent acceleration |
| _stoppingDistance | float | 8f | Distance at which the enemy stops and faces the player (shooting range) |

## Public API

| Member | Type | Description |
|--------|------|-------------|
| ClassId | int (get/set) | Assigned by EnemySpawner at spawn time, used by BCITargetingSystem to map BCI detections to this enemy |

## How It Works

1. `Start()` caches the player transform via `FindWithTag("Player")`, gets the `NavMeshAgent` component, and applies inspector field values to the agent.
2. `Update()` continuously sets `agent.destination` to the player's position, causing the agent to pathfind toward them around obstacles.
3. When `agent.remainingDistance <= agent.stoppingDistance`, the enemy rotates on the Y axis only to face the player (zeroing the Y component of the direction vector prevents tilting).
4. NavMeshAgent automatically pauses when `Time.timeScale = 0` and resumes when it returns to 1 — no extra freeze logic needed.

## Enemy Prefab (`Assets/Prefabs/Enemy.prefab`)

| Property | Value |
|----------|-------|
| Mesh | Capsule |
| Scale | (1, 1, 1) |
| Material | RedMat (existing) |
| NavMeshAgent | speed=3.5, acceleration=8, stoppingDistance=8, angularSpeed=120 |
| CapsuleCollider | default (center 0,1,0, radius 0.5, height 2) |
| EnemyController | script attached |
| Tag | "Enemy" |

## Dependencies

- Spec 01 (TimeManager) — provides the freeze mechanic that NavMeshAgent respects.
- Spec 03 (Arena) — baked NavMesh required for pathfinding.

## Unity Setup & Test

1. Ensure the **Arena** (Spec 03) is set up with a baked NavMesh
2. Create a **Capsule** in the scene at position `(10, 1, 10)`, name it **TestEnemy**
3. Add Component → **Nav Mesh Agent** (set speed=3.5, acceleration=8, stoppingDistance=8, angularSpeed=120)
4. Add Component → **EnemyController**
5. Apply the existing **RedMat** material
6. Tag it as **"Enemy"** (create the tag if it doesn't exist: Edit > Project Settings > Tags and Layers)
7. Ensure the Player exists and is tagged **"Player"**
8. Enter Play mode — the enemy should navigate toward the player, going around obstacles
9. Stop moving (release WASD) — enemy should freeze in place
10. Move again — enemy should resume navigation
11. Verify the enemy stops at ~8 units from the player and faces them
12. Optionally duplicate the enemy 2-3 times at different positions to test multiple NavMesh agents
13. To create the prefab: drag TestEnemy from Hierarchy into `Assets/Prefabs/` to create **Enemy.prefab**
14. **Common issue:** If enemy doesn't move, verify NavMesh is baked and the enemy is placed on a NavMesh surface

## Known Limitations

- No attack animation or state machine — just chase and face for MVP.
- No health system on enemies yet (see Spec 09 for death effects).
- No avoidance priority tuning between enemies.
