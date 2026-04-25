# Feature 01 — Time Manager

## Overview

Singleton that controls `Time.timeScale` to implement the core SUPERHOT mechanic: time freezes when Person B (movement player) is stationary and resumes when they move. All standard `Update()` logic auto-freezes because `Time.deltaTime` goes to zero, while systems that need to run during freeze (BCI, mouse look, UI) use `Time.unscaledDeltaTime` or raw input.

## Scripts

| Script | Path | Purpose |
|--------|------|---------|
| TimeManager | Assets/Scripts/Core/TimeManager.cs | Singleton that toggles `Time.timeScale` between 0 and 1 based on player movement |

## Inspector Fields

None — TimeManager has no configurable fields.

## How It Works

1. `Awake()` enforces singleton pattern: first instance assigns itself to `Instance`, duplicates self-destruct.
2. `PlayerMovementController` calls `TimeManager.Instance.SetMoving(bool)` every frame with the current movement state.
3. `SetMoving()` short-circuits if the state hasn't changed (via `_wasMoving` tracking), then sets `Time.timeScale` to 1 (moving) or 0 (stationary) and logs the state change.
4. `IsTimeFrozen` property returns `true` when `Time.timeScale == 0`, letting other systems query freeze state without coupling to the input system.

## Dependencies

None — this is the foundational system that other specs build on.

## Unity Setup & Test

1. Create an empty GameObject named **TimeManager** in the scene hierarchy
2. Add Component → `TimeManager`
3. Enter Play mode
4. Stand still — check Console for **"Time frozen"** log
5. Press W — check Console for **"Time resumed"** log
6. Confirm: any objects using `Time.deltaTime` in `Update()` freeze when you stop
7. Confirm: mouse look still works during freeze (it uses raw input, not deltaTime)

## Known Limitations

- Binary freeze only (0 or 1) — no slow-motion ramp for MVP.
- No `DontDestroyOnLoad` — singleton lifetime is per-scene.
