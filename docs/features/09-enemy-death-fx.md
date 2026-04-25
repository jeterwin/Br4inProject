# Feature 09 — Enemy Death FX

## Overview

When an enemy is hit by a player bullet (fired by the BCI targeting system), it dies with a red particle burst effect. The enemy and bullet are destroyed, and the spawner/targeting systems are notified via the OnDeath event chain.

## Scripts

| Script | Path | Purpose |
|--------|------|---------|
| EnemyDeathHandler | Assets/Scripts/Enemy/EnemyDeathHandler.cs | Detects bullet hits, spawns death particles, fires OnDeath event, destroys enemy |

## Inspector Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `_deathParticlePrefab` | GameObject | — | Particle system prefab instantiated at enemy position on death |

## How It Works

1. **OnTriggerEnter** checks if the colliding object is on the "Bullet" layer.
2. If so, the bullet is destroyed immediately.
3. **Die()** is called, which:
   - Instantiates the death particle prefab at the enemy's position
   - Invokes the `OnDeath` event (EnemySpawner subscribes to decrement alive count and unregister from BCITargetingSystem)
   - Destroys the enemy GameObject
4. The particle system plays its burst (25 red particles), then auto-destroys via Stop Action = Destroy.

**Event chain:** `EnemyDeathHandler.OnDeath` → `EnemySpawner.OnEnemyDied` (decrements count + calls `BCITargetingSystem.UnregisterEnemy`)

## Dependencies

- **Spec 04 (EnemyController):** Enemy prefab with collider
- **Spec 05 (Bullet):** Bullets on "Bullet" layer with trigger collider
- **Spec 07 (EnemySpawner):** Subscribes to OnDeath for alive count tracking
- **Spec 08 (BCITargetingSystem):** Unregisters dead enemies from targeting

## Unity Setup & Test

### Create the Particle Prefab

1. In the scene, create **GameObject → Effects → Particle System**
2. Configure the Particle System with these settings:

| Property | Value |
|----------|-------|
| Duration | 0.5s |
| Looping | false |
| Start Lifetime | Random Between Two Constants: 0.3–0.5s |
| Start Speed | Random Between Two Constants: 5–10 |
| Start Size | Random Between Two Constants: 0.1–0.3 |
| Start Color | Red (255, 50, 50) |
| Simulation Space | **World** |
| Stop Action | **Destroy** |
| Play On Awake | true |

3. **Shape module:** Sphere, Radius = 0.5
4. **Emission module:** Bursts → Add burst at time 0, count = 25
5. **Renderer module:** Billboard, Default-Particle material
6. Test by clicking Play on the Particle System component — verify red burst
7. Drag the Particle System from Hierarchy into `Assets/Prefabs/` to create **EnemyDeathParticle.prefab**
8. Delete the test particle from the scene

### Wire Up the Prefab

9. Open the Enemy prefab (double-click `Assets/Prefabs/TestEnemy.prefab`)
10. Select the EnemyDeathHandler component
11. Drag `Assets/Prefabs/EnemyDeathParticle.prefab` into the **Death Particle Prefab** field
12. Apply prefab changes

### Test

13. Enter Play mode — wait for BCI debug simulator to fire at an enemy
14. When bullet hits: red particle burst should appear, enemy disappears, bullet disappears
15. Check Console for any errors
16. Verify the particle effect self-destructs after ~0.5s (no orphaned particles in Hierarchy)
17. Check EnemySpawner alive count decrements in Debug Inspector

### Troubleshooting
- No particles visible → Check that `_deathParticlePrefab` is assigned on the prefab, not just the scene instance
- Particles disappear instantly → Simulation Space must be **World**, not Local (otherwise they're destroyed with the enemy)
- Particles linger forever → Stop Action must be **Destroy**, Looping must be **false**

## Known Limitations

- No death animation — enemy simply disappears with particle burst
- No death sound effect for MVP
- Particle color is hardcoded red — doesn't adapt to enemy material color
