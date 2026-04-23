# Br4inProject

Co-op SUPERHOT-style first-person 3D shooter with g.tec BCI brain-computer interface targeting. Hackathon MVP — single arena with wave-based enemies.

## Players

Two people share one player entity:
- **Person A (BCI)**: Wears Unicorn Hybrid Black headset. P300 Visual ERP flashing runs continuously — the system detects which enemy they focus on. Confidence-based auto-fire: a rolling time window tracks positive detections per enemy, and when the threshold is reached (e.g., 4/5 detections in 5 seconds), the game auto-fires at that enemy.
- **Person B (Movement)**: WASD + mouse, rigidbody-based FPS movement.

## Core Mechanic

Time freezes (`Time.timeScale = 0`) when Person B is stationary, resumes (`Time.timeScale = 1`) when they move. Everything in standard `Update()` auto-freezes. BCI detection and UI must use `Time.unscaledDeltaTime` to keep working during freeze.

## Tech Stack

- Unity 2022.3.62f3 (LTS), C#
- g.tec Unity Interface SDK (Unicorn Hybrid Black, P300 Visual ERP)

## Architecture

| System | Responsibility |
|--------|---------------|
| `TimeManager` | Singleton. Sets `Time.timeScale` based on movement input. |
| `PlayerMovementController` | Rigidbody FPS (WASD + mouse). Reports `IsMoving` to TimeManager. |
| `BCITargetingSystem` | Bridges g.tec ERP pipeline to game. Maintains per-enemy confidence scores in rolling window. Auto-fires at threshold. |
| `EnemyController` | Enemy AI. Moves toward player, shoots. Auto-freezes via `Update()`. |
| `EnemySpawner` | Wave-based spawning at arena edge points. |
| `GameManager` | Game state (calibration → playing → game over), score, health. |

## g.tec SDK Integration

- **Location**: `Assets/g.tec/Unity Interface/`
- **Event subscription**: `component.OnEventName.AddListener(handler)` in `Start()`
- **Component discovery**: `GetComponentInParent<T>()`
- **Thread safety**: Always use `EventHandler.Instance.Enqueue(() => { ... })` when touching Unity objects from BCI callbacks
- **ERP workflow**: Train → CalibrationResult → Application mode (continuous P300 detection)
- **Key types**: `ERPPipeline`, `ERPParadigm`, `Device`, `SignalQualityPipeline`
- **Targeting prefab**: `BCI Visual ERP 3D` — flash objects map to enemy GameObjects

## Coding Conventions

- PascalCase for public members, `_camelCase` for private fields
- One MonoBehaviour per file, filename = class name
- `[SerializeField]` for inspector-exposed private fields
- `[RequireComponent]` where dependencies exist
- No comments unless the WHY is non-obvious

## Script Organization

```
Assets/Scripts/
├── Core/        # TimeManager, GameManager
├── Player/      # PlayerMovementController
├── Enemy/       # EnemyController, EnemySpawner
├── BCI/         # BCITargetingSystem, confidence logic
└── UI/          # HUD, health display, score
```

## Key Paths

- Scene: `Assets/Scenes/SampleScene.unity`
- g.tec SDK: `Assets/g.tec/Unity Interface/`
- g.tec Prefabs: `Assets/g.tec/Unity Interface/Prefabs/`
- g.tec UI Scripts: `Assets/g.tec/Unity Interface/SDK/UI/`

## Unity Testing Rule

**Every time you write or modify a script, you MUST provide step-by-step Unity Editor instructions** explaining how to test that feature. This includes:
1. What GameObjects to create or select in the scene hierarchy
2. What components to add (Add Component) and how to configure their inspector fields
3. What prefabs to drag in (if any) and where to parent them
4. What layers, tags, or physics settings to configure (if any)
5. How to enter Play mode and verify the feature works
6. What to look for — expected behavior and how to confirm success
7. Common problems and how to fix them

Format these as a numbered checklist under a `## Unity Setup & Test` heading at the end of your response. Never skip this — the users are not experienced with Unity and need explicit editor instructions.

## Build & Run

- Open in Unity 2022.3.62f3
- Open `Assets/Scenes/SampleScene.unity`
- Play mode to test (BCI hardware required for targeting, movement testable without it)

## Implementation Workflow

- Spec files in `docs/specs/` are the source of truth for implementation
- Never implement a feature without reading its spec file first
- Use the `unity-spec-executor` skill when implementing specs
- Every implemented spec MUST produce two documents before it's considered done:
  1. Technical reference: `docs/features/XX-name.md`
  2. Summary card: `docs/features/summaries/XX-name-summary.md`
- Update `docs/features/README.md` index after each spec is completed
- Never claim a spec is complete without:
  - All acceptance criteria from the spec verified
  - Both feature docs written
  - Changes committed
