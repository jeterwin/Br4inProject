# Spec 03 — Arena

**Status:** To build
**Dependencies:** None

## Summary

Replace the current 10x10 flat ground plane with a larger arena containing static obstacles. The obstacles serve two purposes: (1) give NavMesh pathfinding something meaningful to navigate around, (2) provide cover for the player.

## Layout

- Ground plane scaled to 30x30 units (position 0,0,0)
- 4 concrete pillar obstacles (cubes, scale ~2x3x2) placed around the arena
- 2 low wall segments (cubes, scale ~6x2x1) providing partial cover
- All obstacles use a neutral grey material (different shade from ground)
- Arena boundary: invisible walls (empty GameObjects with BoxCollider, no MeshRenderer) at edges to prevent falling off
- All obstacle objects are marked as **Navigation Static** for NavMesh baking

## NavMesh Setup

- Bake NavMesh in Navigation window (Window > AI > Navigation)
- Agent Radius: 0.5 (matches enemy capsule radius)
- Agent Height: 2.0
- Step Height: 0.4
- Verify baked mesh covers walkable ground and carves around obstacles

## Scene Hierarchy

```
Arena/
  Ground        — Plane, scale (30,1,30), GroundMat
  Pillar1       — Cube, position (5, 1.5, 5), scale (2,3,2), Navigation Static, ObstacleMat
  Pillar2       — Cube, position (-5, 1.5, -5), scale (2,3,2), Navigation Static, ObstacleMat
  Pillar3       — Cube, position (8, 1.5, -3), scale (2,3,2), Navigation Static, ObstacleMat
  Pillar4       — Cube, position (-7, 1.5, 6), scale (2,3,2), Navigation Static, ObstacleMat
  Wall1         — Cube, position (0, 1, 8), scale (6,2,1), Navigation Static, ObstacleMat
  Wall2         — Cube, position (-3, 1, -8), scale (6,2,1), Navigation Static, ObstacleMat
  BoundaryN     — Empty with BoxCollider, position (0,1.5,15), size (30,3,1)
  BoundaryS     — Empty with BoxCollider, position (0,1.5,-15), size (30,3,1)
  BoundaryE     — Empty with BoxCollider, position (15,1.5,0), size (1,3,30)
  BoundaryW     — Empty with BoxCollider, position (-15,1.5,0), size (1,3,30)
```

## Materials

| Material | Color (RGB) | Used On |
|----------|-------------|---------|
| GroundMat | (180, 180, 180) light grey | Ground plane |
| ObstacleMat | (100, 100, 110) dark grey-blue | Pillars and walls |

## Acceptance Criteria

1. Player can walk around the arena and collide with obstacles
2. Player cannot fall off the edges (invisible walls)
3. NavMesh is baked and visible in Scene view (blue overlay on walkable areas)
4. NavMesh correctly excludes obstacle footprints

## Unity Setup & Test

1. Delete the existing Ground plane from the scene
2. Create empty GameObject "Arena" at origin
3. Create a Plane as child of Arena named "Ground" — set scale to (30, 1, 30)
4. Create material "GroundMat" in Assets/Materials/ — set Albedo to (180,180,180), apply to Ground
5. Create material "ObstacleMat" in Assets/Materials/ — set Albedo to (100,100,110)
6. Create 4 Cube children of Arena for pillars — set positions, scales, and material per the table above
7. Create 2 Cube children of Arena for walls — set positions, scales, and material per the table above
8. Select all 6 obstacles → in the Inspector, check **Navigation Static** (dropdown next to Static checkbox)
9. Also mark the Ground plane as Navigation Static
10. Create 4 empty GameObjects for boundaries (BoundaryN/S/E/W) as children of Arena
11. Add BoxCollider to each boundary — set position and size per the table above
12. Open Window > AI > Navigation
13. Set Agent Radius=0.5, Agent Height=2.0, Step Height=0.4
14. Click "Bake" — verify blue NavMesh overlay appears on walkable ground, excluding obstacles
15. Enter Play mode — walk around, confirm collision with pillars and walls, confirm boundaries block you
16. Common issue: if NavMesh doesn't appear, ensure objects are marked Navigation Static and re-bake
