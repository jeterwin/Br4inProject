# BCI Pipeline Integration — Unity Setup & Test Guide

## Part A: CalibrationScene Setup

1. **Create the scene**: Go to **File → New Scene → Basic (Built-in)**. Save as `Assets/Scenes/CalibrationScene.unity`.

2. **Add to Build Settings**: Go to **File → Build Settings**, click **Add Open Scenes**. Ensure both `CalibrationScene` and `SampleScene` are listed. CalibrationScene should be at index 0 (first to load).

3. **Create BCI Root**: In the Hierarchy, create an empty GameObject named `BCI Root`. This persists across scenes via `DontDestroyOnLoad`.

4. **Add Device prefab**: Drag `Assets/g.tec/Unity Interface/Prefabs/Device.prefab` into the hierarchy. **Parent it under BCI Root**.

5. **Add BCI Visual ERP 3D prefab**: Drag `Assets/g.tec/Unity Interface/Prefabs/BCI Visual ERP 3D.prefab` into the hierarchy. **Parent it under BCI Root**. This contains `ERPPipeline` and `ERPParadigm` components.

6. **Add pipeline prefabs under BCI Root**:
   - Drag `Assets/g.tec/Unity Interface/Prefabs/SignalQualityPipeline.prefab` → parent under `BCI Root`
   - Drag `Assets/g.tec/Unity Interface/Prefabs/BatteryLevelPipeline.prefab` → parent under `BCI Root`
   - Drag `Assets/g.tec/Unity Interface/Prefabs/DataLostPipeline.prefab` → parent under `BCI Root`

7. **Add BCIBarDocker_UI**: Drag `Assets/g.tec/Unity Interface/Prefabs/BCIBarDocker_UI.prefab` into the hierarchy. **Parent it under BCI Root**.

8. **Wire sub-UI prefabs inside BCIBarDocker_UI**: The `BCIUI` script on BCIBarDocker_UI expects references to child UI prefabs. Check the Inspector — if the fields are empty, drag in these prefabs as children of BCIBarDocker_UI:
   - `Assets/g.tec/Unity Interface/Prefabs/DeviceDialog_UI.prefab`
   - `Assets/g.tec/Unity Interface/Prefabs/SignalQualityBar_UI.prefab`
   - `Assets/g.tec/Unity Interface/Prefabs/ERPPipeline_UI.prefab`
   - `Assets/g.tec/Unity Interface/Prefabs/ERPParadigm_UI.prefab`
   - `Assets/g.tec/Unity Interface/Prefabs/DeviceConnectionState_UI.prefab`
   - `Assets/g.tec/Unity Interface/Prefabs/BatteryLevelPipeline_UI.prefab`
   - `Assets/g.tec/Unity Interface/Prefabs/DataLostPipeline_UI.prefab`

   Then in the `BCIUI` Inspector on BCIBarDocker_UI, drag each into its corresponding field (`DeviceDialogUI`, `SignalQualityUI`, `ERPPipelineUI`, `ERPParadigmUI`, `DeviceConnectionStateUI`, `BatteryLevelPipelineUI`, `DataLostPipelineUI`).

9. **Add BCIPipelineOrchestrator**: Select `BCI Root` → **Add Component → BCIPipelineOrchestrator**. Wire the Inspector fields:

   | Field | What to drag |
   |-------|-------------|
   | `Device` | The `Device` child object from BCI Root |
   | `ERP Pipeline` | The `ERPPipeline` component (inside `BCI Visual ERP 3D`) |
   | `ERP Paradigm` | The `ERPParadigm` component (inside `BCI Visual ERP 3D`) |
   | `Paradigm UI` | The `ERPParadigm_UI` object (from BCIBarDocker_UI children) |
   | `Gameplay Scene Name` | Type `SampleScene` |
   | `Calibration Targets Root` | The `CalibrationTargets` object (created in step 10) |

10. **Create Calibration Targets**: Create an empty GameObject named `CalibrationTargets`. Create 6 Cube children inside it (right-click CalibrationTargets → 3D Object → Cube). Name them `Target1` through `Target6`. Set each cube's scale to **(0.5, 0.5, 0.5)**. Positions:

    | Target | Position (X, Y, Z) |
    |--------|-------------------|
    | Target1 | (-1.5, 2.1, 3) |
    | Target2 | (0, 2.1, 3) |
    | Target3 | (1.5, 2.1, 3) |
    | Target4 | (-1.5, 0.9, 3) |
    | Target5 | (0, 0.9, 3) |
    | Target6 | (1.5, 0.9, 3) |

11. **Add ERPFlashTag3D to each target**: For each Target1–6, drag `Assets/g.tec/Unity Interface/Prefabs/ERPFlashTag3D.prefab` as a child, or add the `ERPFlashTag3D` component directly. Set the `ClassId` in the Inspector to match the target number (1–6). Configure dark/flash materials if the component has those fields.

12. **Position the Main Camera** at **(0, 1.5, 0)** facing forward (rotation 0, 0, 0) so it looks at the calibration grid.

13. **Save the scene** (Ctrl+S / Cmd+S).

### Expected Hierarchy

```
BCI Root
├── Device
├── BCI Visual ERP 3D
│   ├── ERPPipeline
│   └── ERPParadigm
├── SignalQualityPipeline
├── BatteryLevelPipeline
├── DataLostPipeline
├── BCIBarDocker_UI
│   ├── DeviceDialog_UI
│   ├── SignalQualityBar_UI
│   ├── ERPPipeline_UI
│   ├── ERPParadigm_UI
│   ├── DeviceConnectionState_UI
│   ├── BatteryLevelPipeline_UI
│   └── DataLostPipeline_UI
└── BCIPipelineOrchestrator (component on BCI Root)

CalibrationTargets
├── Target1 (Cube + ERPFlashTag3D, ClassId=1)
├── Target2 (Cube + ERPFlashTag3D, ClassId=2)
├── Target3 (Cube + ERPFlashTag3D, ClassId=3)
├── Target4 (Cube + ERPFlashTag3D, ClassId=4)
├── Target5 (Cube + ERPFlashTag3D, ClassId=5)
└── Target6 (Cube + ERPFlashTag3D, ClassId=6)

Main Camera
Directional Light
```

---

## Part B: SampleScene Verification

No prefab changes needed — the BCI Root arrives via `DontDestroyOnLoad`. Verify:

1. Open `SampleScene`
2. Confirm `BCITargetingSystem` object exists and has the singleton (`Instance` property added in `Awake()`)
3. Confirm `EnemySpawner` still has its `_targetingSystem` reference wired in the Inspector
4. The `BCIPipelineOrchestrator.Instance` null-check in EnemySpawner means it works with or without the orchestrator present — no changes needed

---

## Part C: Testing WITHOUT BCI Hardware

1. **Open CalibrationScene** and enter Play mode
2. **Console check**: You should see:
   ```
   [BCIOrchestrator] Initialized — waiting for device connection
   ```
3. **UI check**: The BCIBarDocker_UI should be visible. The device dropdown will be empty (no headset), the Connect button visible, and the Start Training / Continue buttons hidden (pipeline not ready)
4. **State verification**: The orchestrator stays in `Disconnected` state without hardware — this is correct

### Debug Mode (No Calibration Needed)

5. **Open `SampleScene` directly** (not CalibrationScene)
6. Ensure `BCITargetingSystem._debugMode` = **true** in the Inspector
7. Ensure `BCIDebugSimulator` is active in the scene with its `_targetingSystem` reference wired
8. Enter Play mode — enemies spawn and get targeted by the auto-cycling simulator exactly as before
9. `BCIPipelineOrchestrator.Instance` is null, so the EnemySpawner flash-target hooks are skipped automatically

---

## Part D: Testing WITH BCI Hardware

### Connection

1. **Pair** the Unicorn Hybrid Black headset via Bluetooth on your computer
2. **Open CalibrationScene**, enter Play mode
3. **Device dropdown**: Should show the Unicorn serial number (e.g., `UN-2023.xx.xx`). Select it and click **Connect**
4. **Console**: Should show:
   ```
   [BCIOrchestrator] Device connected — ready for training
   ```

### Signal Quality

5. The `SignalQualityBar_UI` should show 8 channel bars
6. **Adjust the headset** until all bars are green (good electrode contact)
7. Red bars mean floating electrodes — press them firmly against the scalp or apply more gel

### Training (Calibration)

8. Click **Start Training**
9. The 6 calibration cubes should start flashing in sequence
10. The BCI user should **focus on the designated target cube** — the one highlighted or indicated by the training paradigm
11. Keep still, minimize blinking, maintain focus for the full training duration
12. Click **Stop Training**
13. Console should show:
    ```
    [BCIOrchestrator] Training stopped — waiting for calibration result
    ```

### Calibration Evaluation

14. Wait for the calibration result. Console shows:
    ```
    [BCIOrchestrator] Calibration complete — Quality: Good
    [BCIOrchestrator] Trials selected: 3, Est. selection time: 900ms
    [BCIOrchestrator] Class 1 accuracy: 95.0%
    [BCIOrchestrator] Class 2 accuracy: 92.0%
    ...
    ```
15. The UI shows a green/yellow/red circle indicating quality:
    - **Green (Good)**: ≥90% accuracy — reliable for gameplay
    - **Yellow (Ok)**: 80–90% — usable but some errors expected
    - **Red (Bad)**: <80% — retrain recommended

16. If **Bad**: click **Retrain** and repeat steps 8–15
17. If **Good/Ok**: click **Continue**

### Scene Transition to Gameplay

18. Console shows:
    ```
    [BCIOrchestrator] User clicked Continue — transitioning to gameplay
    [BCIOrchestrator] Gameplay scene loaded — wiring BCI to enemies
    ```
19. `SampleScene` loads. The BCI Root persists (visible in Hierarchy under `DontDestroyOnLoad`).
20. Enemies spawn. Console shows flash target registration:
    ```
    [BCIOrchestrator] Registering flash target classId=1 on Enemy(Clone)
    [BCIOrchestrator] Registering flash target classId=2 on Enemy(Clone)
    ...
    ```

### Live P300 Detection

21. The paradigm starts in Application mode — enemies should flash (once ERPFlashTag3D registration is confirmed with hardware)
22. The BCI user **focuses on an enemy** they want to kill
23. P300 detections flow through the orchestrator → `BCITargetingSystem.OnClassSelected()`
24. After **4 detections within 5 seconds** for the same enemy, a bullet fires at that enemy
25. Watch the Console for detection events and confidence buildup

---

## Part E: Troubleshooting

| Issue | Fix |
|-------|-----|
| Device not in dropdown | Check Bluetooth pairing. Restart Unity. Ensure Unicorn is powered on. |
| Signal quality bars all red | Adjust headset position. Check electrode gel/contact. Focus on parietal electrodes. |
| Calibration quality Bad | Retrain. Ensure user is focused during flashing. Minimize movement and blinking. |
| No P300 detections in Application mode | Check Console for errors. Verify flash tags are configured. Check calibration quality was Good. |
| Scene transition fails | Ensure both scenes are in Build Settings (**File → Build Settings → Add Open Scenes**). |
| `BCIOrchestrator` not found after scene load | Verify `DontDestroyOnLoad` is on the BCI Root. Check that `transform.root` points to BCI Root. |
| Unity crash on BCI callback | Missing `EventHandler.Instance.Enqueue()` wrapper — all BCI callbacks must dispatch to main thread. |
| Debug mode broken after changes | Ensure `BCIDebugSimulator` is active in SampleScene, `BCITargetingSystem._debugMode` = true in Inspector. |
| Enemies don't flash during Application mode | The `RegisterFlashTarget` / `UnregisterFlashTarget` methods are stubbed — the exact ERPFlashTag3D registration API needs to be confirmed with the SDK at hardware integration time. |
