# Enemy Shooting — Setup & Test

## A. Tag the Player

1. Select the **Player** capsule in the Hierarchy
2. In the Inspector, click the **Tag** dropdown (top-left, probably says "Untagged")
3. Select **Player** (it's a built-in Unity tag)

## B. Create the Bullet Physics Layer

1. Go to **Edit → Project Settings → Tags and Layers**
2. Under **Layers**, find the first empty User Layer (e.g., Layer 6)
3. Type `Bullet`
4. Go to **Edit → Project Settings → Physics**
5. In the **Layer Collision Matrix** at the bottom, **uncheck** the Bullet-Bullet intersection (find the Bullet row and Bullet column, uncheck that box)

## C. Create the Yellow Material

1. In the Project window, go to `Assets/Materials/`
2. Right-click → Create → **Material**
3. Name it `YellowMat`
4. Click the white box next to **Albedo** → set color to bright yellow `(255, 255, 50)`

## D. Create the Bullet Prefab

1. In the Hierarchy, right-click → 3D Object → **Sphere**
2. Name it `Bullet`
3. Set Transform Scale to `(0.15, 0.15, 0.15)`
4. Drag `YellowMat` onto it
5. Select it → Inspector:
   - **Sphere Collider**: check **Is Trigger** ✓
   - **Add Component** → `Rigidbody`: check **Is Kinematic** ✓, uncheck **Use Gravity**
   - **Add Component** → `Bullet` (the script we just created)
   - Set **Layer** dropdown (top-right of Inspector) to `Bullet`
6. In the Project window, create a folder: `Assets/Prefabs/` (right-click Assets → Create → Folder → name it `Prefabs`)
7. **Drag the Bullet from Hierarchy into `Assets/Prefabs/`** to create the prefab
8. **Delete the Bullet from the Hierarchy** (it's now saved as a prefab)

## E. Add EnemyShooter to All Enemy Objects

For **each** of the 6 objects (DriftCube1, DriftCube2, DriftCube3, SpinCube1, SpinSphere1, SpinCube2):

1. Select the object in the Hierarchy
2. Inspector → **Add Component** → type `EnemyShooter` → select it
3. Drag the `Bullet` prefab from `Assets/Prefabs/` into the **Bullet Prefab** field
4. Set the fire intervals:

| Object | Fire Interval | Bullet Speed |
|--------|--------------|-------------|
| DriftCube1 | 1.5 | 8 |
| DriftCube2 | 2.0 | 8 |
| DriftCube3 | 1.5 | 8 |
| SpinCube1 | 2.0 | 8 |
| SpinSphere1 | 2.5 | 8 |
| SpinCube2 | 2.0 | 8 |

5. **Save the scene** — Ctrl+S / Cmd+S

## F. Testing

1. Press **Play**
2. **Stand still** — no bullets should appear (timer freezes with `Time.deltaTime = 0`)
3. **Press WASD** — yellow bullets should start flying toward you from all red objects
4. **Release keys** — all bullets freeze mid-air instantly
5. **Move again** — frozen bullets resume their path toward where you were
6. **Walk into a bullet** — Console shows `"Player hit by bullet!"`, bullet disappears
7. **Watch bullets that miss** — they should despawn after ~10 seconds of game-time (not real-time, since the timer also freezes)
8. **Verify bullets don't collide with each other** — they should pass through one another

## G. Troubleshooting

- **No bullets appear** → Check that the Bullet Prefab field is assigned on each EnemyShooter (not `None`)
- **Bullets fire while standing still** → The timer uses `Time.deltaTime`; verify TimeManager is in the scene and working
- **Bullets don't hit player / pass through** → Confirm the Player capsule has a Collider, is tagged "Player", and the Bullet's Sphere Collider has `Is Trigger` checked
- **`NullReferenceException` on Play** → The Player object isn't tagged "Player", so `FindWithTag` returns null
- **Bullets collide with each other and bounce** → Make sure Bullet layer is set on the prefab AND Bullet-Bullet is unchecked in Physics layer matrix
