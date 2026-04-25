# BCI Pipeline Integration — Full End-to-End g.tec P300 Pipeline

**Date:** 2026-04-25
**Status:** Approved
**Depends on:** Spec 08 (BCITargetingSystem), g.tec Unity Interface SDK
**Reference:** docs/reference/bci-hackathon-2023-scripts/ (2023 Spring School project)

## Overview

Wires the full g.tec P300 ERP pipeline into BrainHot so the game is playable with the Unicorn Hybrid Black headset. Uses the new prefab-based SDK API (`Device`, `ERPPipeline`, `ERPParadigm` components) with the g.tec UI prefabs (`BCIBarDocker_UI`) for device connection, signal quality, and calibration controls.

Two scenes: a dedicated CalibrationScene for BCI setup/training, and the existing SampleScene for gameplay. A persistent BCI hierarchy survives the scene transition via `DontDestroyOnLoad`.

## Architecture

```
CalibrationScene                          SampleScene (Gameplay)
─────────────────                         ─────────────────────
BCI Root (DontDestroyOnLoad)              BCI Root (persisted)
├── Device                                ├── Device (still connected)
├── BCI Visual ERP 3D                     ├── BCI Visual ERP 3D
│   ├── ERPPipeline                       │   ├── ERPPipeline (Application mode)
│   └── ERPParadigm                       │   └── ERPParadigm (running)
├── BCIBarDocker_UI (Canvas)              ├── BCIBarDocker_UI (minimized/hidden)
├── Calibration Targets (6 cubes)         └── BCIPipelineOrchestrator
└── BCIPipelineOrchestrator
                                          EnemySpawner
                                          ├── spawns enemies with ClassId
                                          └── registers as flash objects

                                          BCITargetingSystem
                                          └── receives OnClassSelected
```

### SDK API Used (New Prefab-Based)

| Component | Source | How we use it |
|-----------|--------|---------------|
| `Device` | `Device.prefab` | `Device.Connect(serial)`, `OnDeviceStateChanged`, `OnDevicesAvailable` |
| `ERPPipeline` | `BCI Visual ERP 3D.prefab` | `OnCalibrationResult`, `OnPipelineStateChanged` |
| `ERPParadigm` | `BCI Visual ERP 3D.prefab` | `StartParadigm(mode)`, `StopParadigm()`, `OnParadigmStarted`, `OnParadigmStopped` |
| `ERPFlashTag3D` | `ERPFlashTag3D.prefab` | Added to each enemy for P300 stimulus |
| `SignalQualityPipeline` | `SignalQualityPipeline.prefab` | `OnSignalQualityAvailable` (displayed by SDK UI) |
| `BatteryLevelPipeline` | `BatteryLevelPipeline.prefab` | `OnBatteryLevelAvailable` (displayed by SDK UI) |
| `EventHandler` | SDK singleton | `EventHandler.Instance.Enqueue()` for thread safety |

### Threading Model

All BCI callbacks fire from background threads. Every callback that touches Unity objects MUST use `EventHandler.Instance.Enqueue(() => { ... })`. This is the g.tec-recommended pattern and is already used by all SDK UI scripts.

---

## BCIPipelineOrchestrator

**File:** `Assets/Scripts/BCI/BCIPipelineOrchestrator.cs`
**Attached to:** BCI Root GameObject (in CalibrationScene, persists via DontDestroyOnLoad)

### Singleton

```csharp
public static BCIPipelineOrchestrator Instance { get; private set; }
```

Set in `Awake()` with standard duplicate-destroy guard. `DontDestroyOnLoad(gameObject.transform.root.gameObject)` on the BCI Root.

### State Machine

```
Disconnected → Connected → Training → Trained → Application
```

| State | Entry condition | SDK events subscribed | Visible UI | Exit condition |
|-------|----------------|----------------------|-----------|----------------|
| **Disconnected** | Initial state | `Device.OnDevicesAvailable`, `Device.OnDeviceStateChanged` | Device dropdown, Connect button | `OnDeviceStateChanged(Connected)` |
| **Connected** | Device connected | `SignalQualityPipeline.OnSignalQualityAvailable` (via SDK UI) | Signal quality bars, battery, "Start Training" button | User clicks "Start Training" → `ERPParadigm.OnParadigmStarted` |
| **Training** | Paradigm started in Training mode | `ERPParadigm.OnParadigmStopped` | Flashing targets, "Stop Training" button | User clicks "Stop Training" → paradigm stops → calibration runs |
| **Trained** | Calibration complete | `ERPPipeline.OnCalibrationResult` | Calibration quality indicator (green/yellow/red), "Continue" and "Retrain" buttons | User clicks "Continue" → `ERPParadigmUI.OnStartParadigm(Application)` |
| **Application** | Application mode started | P300 classification events from pipeline | Gameplay scene, minimized BCI status bar | Game over or disconnect |

### Inspector Fields

| Field | Type | Default |
|-------|------|---------|
| `_device` | Device | reference (from BCI Root hierarchy) |
| `_erpPipeline` | ERPPipeline | reference |
| `_erpParadigm` | ERPParadigm | reference |
| `_paradigmUI` | ERPParadigmUI | reference |
| `_gameplaySceneName` | string | "SampleScene" |
| `_calibrationTargetsRoot` | GameObject | parent of the 6 calibration target cubes |

### Start() — Event Subscriptions

```csharp
_device.OnDeviceStateChanged.AddListener(OnDeviceStateChanged);
_erpPipeline.OnCalibrationResult.AddListener(OnCalibrationResult);
_erpPipeline.OnPipelineStateChanged.AddListener(OnPipelineStateChanged);
_erpParadigm.OnParadigmStarted.AddListener(OnParadigmStarted);
_erpParadigm.OnParadigmStopped.AddListener(OnParadigmStopped);
_paradigmUI.OnStartParadigm.AddListener(OnUIStartParadigm);
```

### Key Methods

**OnDeviceStateChanged(States state):**
- If `Connected`: transition to Connected state, log device serial
- If `Disconnected`: transition to Disconnected state, clean up

**OnCalibrationResult(ERPParadigm paradigm, CalibrationResult result):**
- Log calibration quality (`result.CalibrationQuality`: Good/Ok/Bad)
- Log cross-validation accuracy per class (`result.Crossvalidation`)
- Log estimated selection time: `(paradigm.OnTimeMs + paradigm.OffTimeMs) * result.TrialsSelected`
- Transition to Trained state
- SDK UI automatically shows the quality indicator and Retrain/Continue buttons

**OnUIStartParadigm(ParadigmMode mode):**
- If `mode == ParadigmMode.Application`: this is the "Continue" click
  - Disable calibration targets
  - Start scene transition to gameplay
- If `mode == ParadigmMode.Training`: this is a retrain, transition to Training state

**TransitionToGameplay():**
- Disable/destroy `_calibrationTargetsRoot`
- Call `SceneManager.LoadSceneAsync(_gameplaySceneName)` (async to avoid freeze during load)
- Register a `SceneManager.sceneLoaded` callback for post-load setup
- After scene load: find `BCITargetingSystem.Instance` and `EnemySpawner` via `FindObjectOfType`
- Subscribe to `EnemySpawner` events for enemy registration
- Wait for enemies to spawn, then register them as flash objects
- Start the paradigm in Application mode: `_erpParadigm.StartParadigm(ParadigmMode.Application)`

### P300 Detection Bridge

The new SDK API fires P300 detection events from `ERPPipeline` (the exact event name is inside the DLL — likely `OnClassSelected` or similar on `ERPParadigm`). The orchestrator subscribes to this event and forwards to the game:

```csharp
EventHandler.Instance.Enqueue(() =>
{
    if (BCITargetingSystem.Instance != null)
        BCITargetingSystem.Instance.OnClassSelected(classId);
});
```

**Important:** The exact event name for P300 class selection in the new API needs to be discovered at integration time. The SDK UI scripts (`ERPParadigmUI`) don't show it because they only handle training/application mode switching. The classification result likely comes from the `ERPPipeline` component. If the new API uses a different event pattern than `ScoreValueAvailable`, we'll adapt the subscription.

**Fallback:** If the new SDK doesn't expose a direct class-selection event, we can replicate the 2023 `BCIManager.cs` pattern: subscribe to score events, extract the max-probability class, apply the 0.99 threshold, and fire our own selection.

---

## Calibration Scene

**File:** `Assets/Scenes/CalibrationScene.unity`

### Hierarchy

```
BCI Root (DontDestroyOnLoad)
├── Device (from Device.prefab)
├── BCI Visual ERP 3D (from prefab)
│   ├── ERPPipeline
│   └── ERPParadigm
├── SignalQualityPipeline (from prefab)
├── BatteryLevelPipeline (from prefab)
├── DataLostPipeline (from prefab)
├── BCIBarDocker_UI (from prefab, Canvas)
│   ├── DeviceDialog_UI (from prefab)
│   ├── SignalQualityBar_UI (from prefab)
│   ├── ERPPipeline_UI (from prefab)
│   ├── ERPParadigm_UI (from prefab)
│   ├── DeviceConnectionState_UI (from prefab)
│   ├── BatteryLevelPipeline_UI (from prefab)
│   └── DataLostPipeline_UI (from prefab)
└── BCIPipelineOrchestrator (script)

CalibrationTargets
├── Target1 (Cube, ERPFlashTag3D, ClassId=1)
├── Target2 (Cube, ERPFlashTag3D, ClassId=2)
├── Target3 (Cube, ERPFlashTag3D, ClassId=3)
├── Target4 (Cube, ERPFlashTag3D, ClassId=4)
├── Target5 (Cube, ERPFlashTag3D, ClassId=5)
└── Target6 (Cube, ERPFlashTag3D, ClassId=6)

Main Camera
Directional Light
```

### Calibration Targets

6 cubes arranged in a 3x2 grid (3 columns, 2 rows), centered at (0, 1.5, 3) in front of the camera. Spacing: 1.5m horizontal, 1.2m vertical. Cube scale: (0.5, 0.5, 0.5). Each cube has:
- An `ERPFlashTag3D` component (from the SDK prefab) that makes it part of the P300 flashing paradigm
- A unique `ClassId` (1-6)
- A default material (dark state) and a flash material (bright state) configured on the flash tag

The grid layout matches the 2023 project's 6-target calibration setup. The positions should be distinct enough that the P300 response clearly differentiates targets.

### User Flow in CalibrationScene

1. **Connect**: User sees device dropdown, selects Unicorn serial, clicks Connect
2. **Signal check**: Signal quality bars appear — user adjusts headset until channels are green
3. **Train**: User clicks "Start Training". Targets flash. User focuses on the highlighted target.
4. **Stop**: User clicks "Stop Training". Classifier trains automatically.
5. **Evaluate**: Calibration quality appears (green/yellow/red circle).
   - If bad: click "Retrain" → back to step 3
   - If good: click "Continue" → transitions to SampleScene

---

## Enemy Flash Object Registration

When enemies spawn in SampleScene during Application mode, they need to become P300 flash targets.

### Registration Flow

1. `EnemySpawner.SpawnWave()` spawns enemies and assigns `ClassId` (already implemented)
2. If BCI mode is active (`BCIPipelineOrchestrator.Instance != null`):
   - For each spawned enemy, call `BCIPipelineOrchestrator.Instance.RegisterFlashTarget(classId, enemy)`
3. The orchestrator:
   - Adds/configures an `ERPFlashTag3D` component on the enemy (or its child)
   - Sets the flash tag's `ClassId` to match the enemy's `ClassId`
   - Configures dark/flash materials (default enemy material vs a bright highlight material)
   - Registers the flash tag with the `ERPParadigm` so the P300 stimulus includes this enemy

### Deregistration Flow

1. `EnemyDeathHandler.OnDeath` fires
2. `EnemySpawner.OnEnemyDied` calls `BCITargetingSystem.UnregisterEnemy(classId)` (already implemented)
3. Additionally: `BCIPipelineOrchestrator.Instance.UnregisterFlashTarget(classId)` removes the flash tag from the paradigm's active targets

### Enemy Count Constraint

The P300 paradigm works best with 4-6 simultaneous targets. The `EnemySpawner._enemyCount` defaults to 6, which is within the optimal range. If waves spawn more enemies, the flash paradigm's timing stretches (more targets = more flashes per cycle = slower detection). Recommend keeping at most 6 active flash targets at a time — if more enemies are alive, only the closest 6 get flash tags.

---

## BCITargetingSystem Changes

**File:** `Assets/Scripts/BCI/BCITargetingSystem.cs`

### Add Singleton

```csharp
public static BCITargetingSystem Instance { get; private set; }
```

Set in `Awake()`:
```csharp
private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
}
```

### No Other Changes

The `OnClassSelected(uint classId)` method, rolling window, confidence tracking, and `FireAtEnemy()` all work identically regardless of whether the input comes from:
- `BCIDebugSimulator` (debug mode)
- `BCIPipelineOrchestrator` (real BCI mode)

The `_debugMode` field controls which input source is active. When `_debugMode = false`, the debug simulator should be disabled and the orchestrator feeds detections.

---

## EnemySpawner Changes

**File:** `Assets/Scripts/Enemy/EnemySpawner.cs`

### Flash Target Registration Hook

In `SpawnWave()`, after the existing `_targetingSystem.RegisterEnemy(classId, enemy)` call, add:

```csharp
if (BCIPipelineOrchestrator.Instance != null)
    BCIPipelineOrchestrator.Instance.RegisterFlashTarget(classId, enemy);
```

In `OnEnemyDied()`, after the existing `_targetingSystem.UnregisterEnemy(classId)` call, add:

```csharp
if (BCIPipelineOrchestrator.Instance != null)
    BCIPipelineOrchestrator.Instance.UnregisterFlashTarget(classId);
```

---

## Debug Mode Compatibility

The entire real-BCI pipeline is orthogonal to debug mode:

- **Debug mode ON** (`_debugMode = true`): No `BCIPipelineOrchestrator` in the scene. `BCIDebugSimulator` feeds fake detections. Game starts directly in SampleScene. No CalibrationScene needed.
- **Debug mode OFF** (`_debugMode = false`): Game starts in CalibrationScene. `BCIPipelineOrchestrator` manages the pipeline. Real P300 detections feed `BCITargetingSystem`.

Both modes use the same `BCITargetingSystem` confidence pipeline — the only difference is who calls `OnClassSelected()`.

---

## Files Summary

### New Files

| File | Purpose |
|------|---------|
| `Assets/Scripts/BCI/BCIPipelineOrchestrator.cs` | Full BCI lifecycle manager: connect → train → evaluate → play |
| `Assets/Scenes/CalibrationScene.unity` | BCI setup + training scene with 6 calibration targets |

### Modified Files

| File | Change |
|------|--------|
| `Assets/Scripts/BCI/BCITargetingSystem.cs` | Add singleton `Instance` property |
| `Assets/Scripts/Enemy/EnemySpawner.cs` | Add flash target registration/deregistration hooks |

### Prefab Changes

| Prefab | Change |
|--------|--------|
| Enemy prefab | May need a flash material variant for the `ERPFlashTag3D` bright state |

### Scene Setup

| Scene | Contents |
|-------|----------|
| `CalibrationScene` | BCI Root (Device + BCI Visual ERP 3D + BCIBarDocker_UI + BCIPipelineOrchestrator) + 6 calibration cubes + Camera + Light |
| `SampleScene` | No BCI prefab changes — the BCI Root arrives via DontDestroyOnLoad |

---

## Acceptance Criteria

1. **Device connection**: User can select and connect to a Unicorn Hybrid Black from the CalibrationScene dropdown
2. **Signal quality**: Per-channel signal quality is visible after connection (green/red indicators)
3. **Training**: User can start/stop P300 calibration training with visual flash stimuli on 6 targets
4. **Calibration result**: Quality indicator (Good/Ok/Bad) is shown after training. User can retrain or continue.
5. **Scene transition**: Clicking "Continue" loads SampleScene with the BCI pipeline still connected and running
6. **Flash object registration**: Spawned enemies in SampleScene become P300 flash targets (they flash during Application mode)
7. **P300 → kill pipeline**: Real P300 detections flow through BCITargetingSystem's rolling window and fire bullets at enemies when confidence threshold is reached
8. **Debug mode still works**: Setting `_debugMode = true` on BCITargetingSystem bypasses all BCI pipeline setup and uses the debug simulator as before
9. **Thread safety**: All BCI callbacks use `EventHandler.Instance.Enqueue()` — no direct Unity API calls from background threads
10. **BCI status persists**: Battery, signal quality, and connection state remain visible (minimized) during gameplay

## Open Questions / Integration-Time Decisions

1. **Exact P300 classification event name** on the new SDK's `ERPPipeline` component — needs to be discovered when the DLL is available. The 2023 project used `ScoreValueAvailable` on `ERPBCIManager`. The new API may expose a higher-level class selection event directly, or we may need to process scores ourselves.

2. **ERPFlashTag3D dynamic registration API** — the exact method to add/remove flash targets at runtime on `ERPParadigm` needs to be confirmed. The 2023 project used `_flashController.ApplicationObjects.Add()`. The new prefab may manage targets via child objects or a similar list.

3. **Flash material for enemies** — what material/color enemies should flash with during the P300 stimulus. The 2023 project used bright sprite swaps. For 3D, we'll likely swap to a bright emissive material during flash and back to the default material.
