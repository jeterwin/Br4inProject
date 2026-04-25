---
name: gtec-integration
description: Use when working with BCI code, P300 targeting, g.tec SDK, Unicorn Hybrid Black device connection, ERP pipeline, or brain-computer interface integration
---

# g.tec Unity Interface SDK — Integration Guide

## SDK Location

`Assets/g.tec/Unity Interface/` contains:
- `Plugins/` — Compiled DLLs (Gtec.Chain.Common, UnicornDotNet, Gtec.Licensing, etc.)
- `Prefabs/` — Ready-to-use prefabs for device, pipelines, and UI
- `SDK/UI/` — MonoBehaviour UI scripts (source code)
- `Resources/` — Icons, flash stimulus images

## Device Connection

1. Add `Device.prefab` to scene
2. Subscribe to events in `Start()`:

```csharp
Device device = GetComponentInParent<Device>();
device.OnDevicesAvailable.AddListener(OnDevicesAvailable);    // List<string> serial numbers
device.OnDeviceStateChanged.AddListener(OnStateChanged);       // States.Connected / Disconnected
```

3. Use `DeviceDialog_UI.prefab` for a ready-made connection dropdown UI
4. Connection fires `OnConnect(string serialNumber)` / `OnDisconnect()` UnityEvents

## ERP Pipeline Setup

1. Add `BCI Visual ERP 3D.prefab` to scene — this includes:
   - `ERPPipeline` — processing pipeline
   - `ERPParadigm` — controls training/application modes
   - Flash objects — visual stimuli that flash to trigger P300 responses

2. Each flash object maps to a target (enemy). The BCI detects which flash the user attends to.

## Training Workflow

```csharp
ERPParadigm paradigm = GetComponentInParent<ERPParadigm>();
ERPPipeline pipeline = GetComponentInParent<ERPPipeline>();

// 1. Start training
paradigm.StartParadigm(ParadigmMode.Training);
// → paradigm.OnParadigmStarted fires

// 2. User attends to flashing targets while EEG is recorded

// 3. Stop training
paradigm.StopParadigm();
// → paradigm.OnParadigmStopped fires

// 4. Listen for calibration result
pipeline.OnCalibrationResult.AddListener((ERPParadigm p, CalibrationResult result) => {
    // result.CalibrationQuality: Good | Ok | Bad
    // result.Crossvalidation: Dictionary<uint, double> (per-class accuracy)
    // result.TrialsSelected: uint (trials used for model)
});
```

## Application Mode (Continuous P300 Detection)

After successful calibration:

```csharp
paradigm.StartParadigm(ParadigmMode.Application);
// P300 detections now stream continuously
// Subscribe to selection events from the pipeline to know which target was detected
```

Flash objects keep flickering. The pipeline classifies which target the user is attending to in real-time. Map flash object indices to enemy GameObjects to know which enemy is being targeted.

## Thread Safety — Critical

BCI events fire from background hardware threads. ALWAYS dispatch to main thread:

```csharp
EventHandler.Instance.Enqueue(() => {
    // Safe to access Unity objects here
    enemy.TakeDamage();
});
```

Never access GameObjects, Transforms, or any Unity API directly from BCI callbacks.

## Signal Monitoring

```csharp
SignalQualityPipeline sqp = GetComponentInParent<SignalQualityPipeline>();
sqp.OnSignalQualityAvailable.AddListener((List<ChannelStates> quality) => {
    // Per-channel: ChannelStates.Good or ChannelStates.Bad
});

BatteryLevelPipeline blp = GetComponentInParent<BatteryLevelPipeline>();
blp.OnBatteryLevelAvailable.AddListener((float level) => {
    // 0.0 to 1.0 battery percentage
});
```

## Available UI Prefabs

| Prefab | Purpose |
|--------|---------|
| `BCI_UI` | Main BCI status panel |
| `BCIBarDocker_UI` | Compact dockable BCI status bar |
| `DeviceDialog_UI` | Device selection dropdown + connect/disconnect |
| `DeviceConnectionState_UI` | Connection status icon |
| `ERPPipeline_UI` | Calibration quality display |
| `ERPParadigm_UI` | Training/application mode buttons |
| `SignalQualityPipeline_UI` | Channel quality bars |
| `BatteryLevelPipeline_UI` | Battery level icon |

## Key Enums

- `States` — `Connected`, `Disconnected`
- `PipelineState` — `NotReady`, `Ready`
- `ParadigmMode` — `Training`, `Application`
- `CalibrationQuality` — `Good`, `Ok`, `Bad`
- `ChannelStates` — `Good`, `Bad`

## Integration Pattern for This Game

The `BCITargetingSystem` script should:
1. Hold references to `ERPPipeline` and `ERPParadigm` (via `GetComponentInParent`)
2. Subscribe to P300 detection/selection events in `Start()`
3. Map each detection to an enemy via flash object → enemy lookup table
4. Track detections in a rolling window per enemy (use `Time.unscaledTime`)
5. When confidence threshold is met → fire at that enemy
6. Use `EventHandler.Instance.Enqueue()` for all callbacks that touch game state

## Unity Setup & Test Instructions — REQUIRED

**Every time you write or modify a BCI-related script, you MUST end your response with a `## Unity Setup & Test` section** containing step-by-step Unity Editor instructions. BCI integration has hardware dependencies — always specify what can be tested without the headset and what requires it.

### Template for BCI features:

```
## Unity Setup & Test

### Scene Setup
1. Open `Assets/Scenes/SampleScene.unity`
2. [Drag specific g.tec prefabs from `Assets/g.tec/Unity Interface/Prefabs/` into hierarchy...]
3. [Parent prefabs correctly — e.g., "make ERPPipeline a child of BCI Base"]
4. [Create GameObjects for custom scripts, add components...]
5. [Wire inspector references — specify exact fields and what to drag into them...]
6. [Configure flash objects — how to map them to enemies...]

### Testing WITHOUT BCI Hardware
1. [How to simulate BCI events — e.g., add a debug key that calls the targeting method directly]
2. [What mock data to use — e.g., "press 1-5 to simulate P300 detection on enemy 1-5"]
3. [What to verify in Console — e.g., "confidence scores should print each simulated detection"]

### Testing WITH BCI Hardware
1. Connect Unicorn Hybrid Black via Bluetooth
2. [Use DeviceDialog_UI to select device from dropdown, click Connect]
3. [Verify connection status icon turns green]
4. [Start training — what the BCI person should do during training]
5. [Check calibration quality — what Good/Ok/Bad means]
6. [Start application mode — what to observe during live targeting]

### Troubleshooting
- Device not showing in dropdown → Check Bluetooth pairing, restart Unity
- Signal quality bars all red → Adjust headset position, check electrode contact
- No P300 detections → Recalibrate, ensure user is focused on flash targets
- Unity crash on BCI callback → Missing EventHandler.Instance.Enqueue() wrapper
```

### Critical: Always Include Debug/Mock Mode

Since BCI hardware isn't always available, every BCI script MUST include a debug mode that simulates detections via keyboard input (e.g., number keys 1-6 to simulate P300 detection on enemy 1-6). Document this in the Unity Setup & Test section so the team can test without the headset.
