# SUPERHOT Time-Freeze Test Scene — Setup & Test

## Scene Setup

1. **Open** `Assets/Scenes/SampleScene.unity`

2. **Delete the default Main Camera** — select "Main Camera" in the Hierarchy, right-click → Delete

3. **Create the Ground:**
   - Hierarchy → right-click → 3D Object → **Plane**
   - Name it `Ground`
   - Set Transform Position to `(0, 0, 0)`
   - Set Transform Scale to `(10, 1, 10)`

4. **Create the Player:**
   - Hierarchy → right-click → 3D Object → **Capsule**
   - Name it `Player`
   - Set Transform Position to `(0, 1, 0)`
   - Select `Player` → Inspector → **Add Component** → type `PlayerMovementController` → select it (this auto-adds a Rigidbody)
   - On the **Rigidbody** component: make sure **Use Gravity** is checked
   - Leave PlayerMovementController defaults (Move Speed = 5, Mouse Sensitivity = 2)

5. **Create the Camera as a child of Player:**
   - Select `Player` in Hierarchy → right-click on it → **Camera**
   - Name it `PlayerCamera`
   - Set its **local** Transform Position to `(0, 0.5, 0)`
   - Set its local Rotation to `(0, 0, 0)`

6. **Create the TimeManager:**
   - Hierarchy → right-click → **Create Empty**
   - Name it `TimeManager`
   - Add Component → type `TimeManager` → select it

7. **Create a Red Material:**
   - In the Project window, navigate to `Assets/Materials/`
   - Right-click → Create → **Material**
   - Name it `RedMat`
   - Click the white box next to **Albedo** → set color to red `(255, 50, 50)`

8. **Create Drifting Cubes (3 total):**

   | Name | Position | Direction | Speed |
   |------|----------|-----------|-------|
   | DriftCube1 | (-5, 0.5, 3) | (1, 0, 0) | 2 |
   | DriftCube2 | (3, 0.5, -5) | (0, 0, 1) | 3 |
   | DriftCube3 | (0, 0.5, 5) | (-1, 0, 0.5) | 1.5 |

   For each one:
   - Hierarchy → 3D Object → **Cube**
   - Set the name and position from the table
   - Drag `RedMat` onto it in the Scene view
   - Add Component → `DriftingObject`
   - Set **Direction** and **Speed** from the table

9. **Create Spinning Objects (3 total):**

   | Name | Type | Position | Rotation Speed |
   |------|------|----------|---------------|
   | SpinCube1 | Cube | (4, 0.5, 4) | (0, 90, 0) |
   | SpinSphere1 | Sphere | (-3, 1, -3) | (45, 0, 90) |
   | SpinCube2 | Cube | (-4, 0.5, 2) | (0, 0, 120) |

   For each one:
   - Hierarchy → 3D Object → **Cube** or **Sphere** per the table
   - Set the name and position
   - Drag `RedMat` onto it
   - Add Component → `SpinningObject`
   - Set **Rotation Speed** from the table

10. **Save the scene** — Ctrl+S (Windows) / Cmd+S (Mac)

## Testing

1. Press **Play**
2. **Don't touch any keys** — all red objects should be completely still. Console shows `"Time frozen"`
3. **Press W** — everything starts moving/spinning. Console shows `"Time resumed"`
4. **Release W** — everything freezes instantly. Console shows `"Time frozen"`
5. **Move the mouse while standing still** — you should look around freely even though objects are frozen
6. **Walk around with WASD + mouse** — smooth FPS movement, all objects moving
7. **Rapidly tap W** — objects should stutter-start and stutter-stop in sync with your input

## Troubleshooting

- **Camera doesn't rotate / cursor visible** → Make sure Camera is a **child** of Player (not at scene root). Click the Game window to give it focus
- **Player falls through ground** → Confirm Rigidbody has Use Gravity checked and the Plane exists
- **Objects don't freeze** → Check that the TimeManager GameObject exists with the TimeManager script attached
- **NullReferenceException on play** → Either TimeManager is missing from the scene, or the Camera isn't a child of Player
- **Press Escape** to unlock the cursor if you need to click in the Editor during Play mode
