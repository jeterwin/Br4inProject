# Spec 01 — Time Manager

**Status:** Already implemented
**Script:** `Assets/Scripts/Core/TimeManager.cs`
**Dependencies:** None

## Summary

Singleton that controls `Time.timeScale`. When Person B stops moving, `timeScale = 0` freezes all standard `Update()` calls. When they move, `timeScale = 1` resumes everything. This is the core SUPERHOT mechanic.

## Behavior

- `SetMoving(bool isMoving)` — sets `Time.timeScale` to 1 or 0
- `IsTimeFrozen` property — returns true when timeScale == 0
- Singleton pattern: first instance persists, duplicates destroyed
- Debug logging on every state change

## Inspector Fields

None.

## Acceptance Criteria

1. Stand still in Play mode — Console shows "Time frozen", all Update-based motion stops
2. Press WASD — Console shows "Time resumed", motion resumes
3. Mouse look works during freeze (uses raw input, not deltaTime)

## Unity Setup & Test

1. Create an empty GameObject named "TimeManager" in the scene hierarchy
2. Add Component → `TimeManager`
3. Enter Play mode
4. Stand still — check Console for "Time frozen" log
5. Press W — check Console for "Time resumed" log
6. Confirm: any objects using `Time.deltaTime` in Update freeze when you stop
