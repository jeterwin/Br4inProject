# Feature 03 — Arena

## Overview

A 30x30 arena with static obstacles (pillars and walls) that provide cover and give NavMesh pathfinding meaningful geometry. Invisible boundary walls prevent the player from falling off. The baked NavMesh is the foundation for enemy AI navigation in Spec 04.

## Scripts

No scripts — this feature is entirely scene-based.

## Inspector Fields

N/A

## How It Works

The arena is a parent GameObject containing:
- A scaled Plane for the ground
- 4 pillar Cubes and 2 wall Cubes as obstacles
- 4 invisible boundary colliders at the edges
- A NavMeshSurface component on the Arena parent that bakes walkable ground and carves around obstacles

All obstacles have colliders so the player physically bumps into them. The NavMesh ensures enemies pathfind around them rather than walking through.

## Materials

| Material | Color (RGB) | Used On |
|----------|-------------|---------|
| GroundMat | (180, 180, 180) light grey | Ground plane |
| ObstacleMat | (100, 100, 110) dark grey-blue | Pillars and walls |

## Dependencies

None.

## Unity Setup & Test

1. Delete the existing Ground plane from the scene
2. Create an empty GameObject named **Arena** at origin `(0, 0, 0)`
3. Create a **Plane** as child of Arena named **Ground** — set scale to `(30, 1, 30)`
4. Create material **GroundMat** in `Assets/Materials/` — set Albedo to `(180, 180, 180)`, apply to Ground
5. Create material **ObstacleMat** in `Assets/Materials/` — set Albedo to `(100, 100, 110)`
6. Create **4 Cube** children of Arena for pillars:

   | Name | Position | Scale |
   |------|----------|-------|
   | Pillar1 | (5, 1.5, 5) | (2, 3, 2) |
   | Pillar2 | (-5, 1.5, -5) | (2, 3, 2) |
   | Pillar3 | (8, 1.5, -3) | (2, 3, 2) |
   | Pillar4 | (-7, 1.5, 6) | (2, 3, 2) |

7. Create **2 Cube** children of Arena for walls:

   | Name | Position | Scale |
   |------|----------|-------|
   | Wall1 | (0, 1, 8) | (6, 2, 1) |
   | Wall2 | (-3, 1, -8) | (6, 2, 1) |

8. Apply **ObstacleMat** to all 6 obstacle cubes
9. Create **4 empty GameObjects** for boundaries as children of Arena, each with a **BoxCollider**:

    | Name | Position | BoxCollider Size |
    |------|----------|-----------------|
    | BoundaryN | (0, 1.5, 15) | (30, 3, 1) |
    | BoundaryS | (0, 1.5, -15) | (30, 3, 1) |
    | BoundaryE | (15, 1.5, 0) | (1, 3, 30) |
    | BoundaryW | (-15, 1.5, 0) | (1, 3, 30) |

10. Select the **Arena** parent GameObject → Add Component → **NavMeshSurface** (requires `com.unity.ai.navigation` package — install via Package Manager > Add by name if missing)
11. Leave defaults (Collect Objects = All, Include Layers = Everything)
12. Click **Bake** in the NavMeshSurface inspector — verify blue NavMesh overlay appears on walkable ground, excluding obstacle footprints
13. Enter Play mode — walk around, confirm collision with pillars and walls, confirm boundaries block you
14. **Common issue:** If NavMesh doesn't appear, check that obstacles have colliders and re-bake

## Known Limitations

- Simple box geometry only — no complex level design for MVP.
- No destructible cover.
- Fixed layout, not procedurally generated.
