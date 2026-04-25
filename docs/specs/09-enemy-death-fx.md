# Spec 09 — Enemy Death FX

**Status:** To build
**Script:** `Assets/Scripts/Enemy/EnemyDeathHandler.cs`
**Prefab:** `Assets/Prefabs/EnemyDeathParticle.prefab`
**Dependencies:** Spec 04 (EnemyController), Spec 08 (BCITargetingSystem fires bullets at enemies)

## Summary

When an enemy is hit by a player bullet (fired by BCI targeting), it dies with a particle burst effect. The enemy is destroyed and the spawner/targeting system are notified.

## Behavior

**Detecting the hit:**
- Enemy has a CapsuleCollider (non-trigger, from EnemyController prefab)
- Player bullets use the Bullet script with "Bullet" layer and isTrigger=true on their SphereCollider
- When a trigger collider (bullet) enters a non-trigger collider (enemy), Unity fires `OnTriggerEnter` on both objects
- EnemyDeathHandler uses `OnTriggerEnter`: if the collider's GameObject is on the "Bullet" layer, trigger death

**Death sequence:**
1. Instantiate particle system prefab at enemy position
2. Invoke `OnDeath` event (EnemySpawner subscribes to decrement count)
3. Destroy the bullet that hit the enemy
4. Destroy the enemy GameObject

## Script Details

**File:** `Assets/Scripts/Enemy/EnemyDeathHandler.cs`

**Inspector fields:**

| Field | Type | Default |
|-------|------|---------|
| `_deathParticlePrefab` | GameObject | EnemyDeathParticle prefab |

**Public:**
- `event System.Action<EnemyDeathHandler> OnDeath` — EnemySpawner subscribes to this per enemy

**OnTriggerEnter(Collider other):**
- If `other.gameObject.layer != LayerMask.NameToLayer("Bullet")`, return
- Instantiate `_deathParticlePrefab` at `transform.position` with default rotation
- Invoke `OnDeath(this)`
- `Destroy(other.gameObject)` (destroy the bullet)
- `Destroy(gameObject)` (destroy the enemy)

## Particle Prefab (`Assets/Prefabs/EnemyDeathParticle.prefab`)

| Property | Value |
|----------|-------|
| Duration | 0.5s |
| Looping | false |
| Start Lifetime | Random Between Two Constants: 0.3–0.5s |
| Start Speed | Random Between Two Constants: 5–10 |
| Start Size | Random Between Two Constants: 0.1–0.3 |
| Start Color | Red (255, 50, 50) — matches enemy material |
| Simulation Space | World |
| Stop Action | Destroy |
| Play On Awake | true |
| **Shape module** | Sphere, Radius = 0.5 |
| **Emission module** | Bursts: 1 burst at time 0, count 25 |
| **Renderer module** | Billboard, Default-Particle material |

## Integration with Other Systems

- **EnemySpawner (Spec 07):** Subscribes to `OnDeath` to decrement alive count
- **BCITargetingSystem (Spec 08):** Needs to call `UnregisterEnemy(classId)` when enemy dies. The spawner or death handler can trigger this.
- **Chain:** EnemyDeathHandler.OnDeath → EnemySpawner.OnEnemyDied (decrements count) + BCITargetingSystem.UnregisterEnemy (removes from targeting)

## Acceptance Criteria

1. Enemy hit by a player bullet — red particle burst appears at enemy position
2. Enemy is destroyed immediately after the hit
3. The bullet that hit the enemy is also destroyed
4. Particle effect plays for ~0.5s then auto-destroys (Stop Action = Destroy)
5. EnemySpawner alive count decrements correctly
6. BCITargetingSystem stops targeting the dead enemy
7. Particles use World simulation space so they don't disappear with the destroyed enemy

## Unity Setup & Test

1. Create a Particle System in the scene (GameObject > Effects > Particle System)
2. Configure all properties per the table above
3. Test the effect by clicking Play on the Particle System component — verify red burst
4. Drag the configured Particle System from Hierarchy into Assets/Prefabs/ to create EnemyDeathParticle.prefab
5. Delete the test particle from the scene
6. Open the Enemy prefab (double-click Assets/Prefabs/Enemy.prefab)
7. Add Component → `EnemyDeathHandler`
8. Drag EnemyDeathParticle.prefab into the `_deathParticlePrefab` field
9. Apply prefab changes
10. Enter Play mode with enemies and BCI debug simulator active
11. Wait for BCI auto-fire to hit an enemy
12. Verify: red particle burst appears, enemy disappears, bullet disappears
13. Check Console for any errors
14. Verify the particle effect self-destructs after 0.5s (no orphaned particles in Hierarchy)
