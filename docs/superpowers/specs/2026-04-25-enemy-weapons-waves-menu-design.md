# Design: Enemy Weapon Variety, Endless Waves, Main Menu & Settings

**Date:** 2026-04-25
**Status:** Approved
**Scope:** Three features — enemy weapon variety (ScriptableObject configs), endless wave system with scaling, and main menu scene with BCI training gate and audio settings.

---

## 1. Enemy Weapon Variety

### 1.1 WeaponConfig ScriptableObject

**File:** `Assets/Scripts/Enemy/WeaponConfig.cs`

A `[CreateAssetMenu]` ScriptableObject that defines all parameters for a weapon type.

| Field | Type | Description |
|-------|------|-------------|
| `WeaponName` | string | Display name |
| `FireInterval` | float | Seconds between shots (or bursts) |
| `BulletSpeed` | float | Bullet travel speed |
| `BulletScale` | float | Multiplier on bullet prefab local scale |
| `BulletColor` | Color | Applied to bullet renderer material |
| `EnemyColor` | Color | Applied to enemy renderer material |
| `BurstCount` | int | Bullets per burst (1 = single shot) |
| `BurstDelay` | float | Seconds between bullets within a burst |

### 1.2 Weapon Type Definitions

Three ScriptableObject assets created at `Assets/Data/Weapons/`:

| Field | Pistol.asset | Burst.asset | Heavy.asset |
|-------|-------------|-------------|-------------|
| `WeaponName` | "Pistol" | "Burst" | "Heavy" |
| `FireInterval` | 2.0 | 3.5 | 4.0 |
| `BulletSpeed` | 8.0 | 10.0 | 5.0 |
| `BulletScale` | 1.0 | 0.8 | 2.5 |
| `BulletColor` | Yellow (255,255,50) | Orange (255,165,0) | Red (255,50,50) |
| `EnemyColor` | Red (current RedMat) | Orange (255,165,0) | Dark Red (139,0,0) |
| `BurstCount` | 1 | 3 | 1 |
| `BurstDelay` | 0 | 0.12 | 0 |

### 1.3 EnemyShooter Changes

**File:** `Assets/Scripts/Enemy/EnemyShooter.cs`

Replace individual `_fireInterval` and `_bulletSpeed` fields with a single `[SerializeField] private WeaponConfig _weaponConfig` field.

Fire logic:
- If `_weaponConfig.BurstCount > 1`: fire a coroutine that instantiates `BurstCount` bullets with `BurstDelay` between each, then waits `FireInterval` before the next burst
- If `_weaponConfig.BurstCount == 1`: current single-shot behavior using `FireInterval`

On bullet instantiation:
- Set `bullet.transform.localScale *= _weaponConfig.BulletScale`
- Set bullet renderer material color to `_weaponConfig.BulletColor`

New public method:
- `public void ApplyScaling(float fireIntervalMultiplier, float bulletSpeedMultiplier)` — stores multipliers, applied on top of `WeaponConfig` base values

### 1.4 Visual Differentiation

At spawn time, the enemy's mesh renderer material color is set to `_weaponConfig.EnemyColor`. This lets the player read the threat type at a glance.

| Weapon | Enemy Color | Bullet Color |
|--------|------------|--------------|
| Pistol | Red | Yellow |
| Burst | Orange | Orange |
| Heavy | Dark Red | Red |

### 1.5 Spawner Integration

`EnemySpawner` holds a `[SerializeField] private WeaponConfig[] _weaponConfigs` array. At spawn time, picks a random config from the array and assigns it to the enemy's `EnemyShooter` component.

---

## 2. Endless Wave System

### 2.1 EnemySpawner Modifications

The existing `EnemySpawner` becomes wave-aware. Replaces the current single-wave behavior.

**New inspector fields:**

| Field | Type | Default | Purpose |
|-------|------|---------|---------|
| `_baseEnemyCount` | int | 6 | Enemies in wave 1 |
| `_enemiesPerWaveIncrease` | int | 2 | Extra enemies added each wave |
| `_fireRateScaling` | float | 0.12 | 12% faster fire each wave |
| `_bulletSpeedScaling` | float | 0.10 | 10% faster bullets each wave |
| `_moveSpeedScaling` | float | 0.08 | 8% faster movement each wave |

**Removed fields:**
- `_enemyCount` — replaced by `_baseEnemyCount` + wave calculation

### 2.2 Wave Flow

1. `GameManager` calls `SpawnWave()` to start wave 1
2. All enemies in current wave die → `OnAllEnemiesDefeated` fires
3. `EnemySpawner` increments `_currentWave`, immediately calls `SpawnWave()` again
4. Loops forever — pure arcade, no cap

### 2.3 Scaling Formulas

Per wave (wave number is 0-indexed internally, displayed as 1-indexed):

- **Enemy count:** `_baseEnemyCount + (_currentWave * _enemiesPerWaveIncrease)`
  - Wave 1: 6, Wave 2: 8, Wave 3: 10, Wave 4: 12...
- **Fire interval multiplier:** `(1 - _fireRateScaling) ^ _currentWave`
  - Decreases each wave → enemies shoot faster
- **Bullet speed multiplier:** `(1 + _bulletSpeedScaling) ^ _currentWave`
  - Increases each wave → bullets harder to dodge
- **Move speed multiplier:** `(1 + _moveSpeedScaling) ^ _currentWave`
  - Increases each wave → enemies close in faster

No caps — scaling increases forever. Pure arcade difficulty curve.

### 2.4 Enemy API for Scaling

**EnemyShooter:**
- `public void ApplyScaling(float fireIntervalMultiplier, float bulletSpeedMultiplier)` — stores multipliers applied on top of `WeaponConfig` base values

**EnemyController:**
- `public void ApplySpeedMultiplier(float multiplier)` — adjusts `NavMeshAgent.speed` by multiplying base `_moveSpeed`

### 2.5 Public API Additions on EnemySpawner

- `int CurrentWave` — read-only, current wave number (1-indexed for display)
- `event Action<int> OnWaveStarted` — fires with wave number when a new wave spawns

### 2.6 Self-Chaining

When `OnAllEnemiesDefeated` fires, the spawner itself triggers the next wave immediately (no delay). The `OnWaveStarted` event fires so the HUD and GameManager can react.

### 2.7 HUD Impact

Spec 11 (HUD) should be updated to:
- Display current wave number (e.g., "Wave 3")
- Optionally flash "WAVE 3" briefly when a new wave starts

---

## 3. Main Menu & Audio Settings

### 3.1 Scene Structure

New scene: `Assets/Scenes/MainMenu.unity`

Build Settings order:
- Index 0: `MainMenu`
- Index 1: `SampleScene`

### 3.2 GameSettings Static Class

**File:** `Assets/Scripts/Core/GameSettings.cs`

```
public static class GameSettings
{
    public static bool IsCalibrated = false;
    public static bool SkipCalibration = true;
}
```

- `IsCalibrated` starts `false` on app launch. Set to `true` by `GameManager` when BCI calibration completes successfully.
- `SkipCalibration` set by `MainMenu` before loading the game scene. `true` = Play (skip to Playing), `false` = Train BCI (enter Calibration).
- Both persist across scene loads (static), reset on app restart (intentional — BCI calibration is per-session).

### 3.3 MainMenu Scene Contents

```
MainMenuCanvas (Canvas, Screen Space - Overlay)
├── TitleText           — "BRAINHOT", large, centered upper area
├── ButtonPanel         — centered vertically
│   ├── PlayButton          — loads SampleScene, skips calibration
│   ├── TrainBCIButton      — loads SampleScene in calibration mode
│   ├── SettingsButton      — shows SettingsPanel
│   └── QuitButton          — quits application
├── SettingsPanel       — initially hidden
│   ├── SettingsTitleText   — "Settings"
│   ├── MasterVolumeSlider  — slider 0-1
│   ├── VolumeLabel         — "Master Volume" + percentage
│   └── BackButton          — returns to ButtonPanel
└── PlayDisabledLabel   — "Complete BCI training first" (visible when Play disabled)
```

Visual design (colors, fonts, spacing) deferred to designer. Spec defines layout and functionality only.

### 3.4 MainMenu.cs

**File:** `Assets/Scripts/UI/MainMenu.cs`

**Inspector fields:**

| Field | Type |
|-------|------|
| `_playButton` | Button |
| `_trainBCIButton` | Button |
| `_settingsButton` | Button |
| `_quitButton` | Button |
| `_buttonPanel` | GameObject |
| `_settingsPanel` | GameObject |
| `_playDisabledLabel` | GameObject |

**Behavior:**

- `Start()`:
  - Hide `_settingsPanel`, show `_buttonPanel`
  - Unlock cursor: `Cursor.lockState = CursorLockMode.None`, `Cursor.visible = true`
  - Set `_playButton.interactable = GameSettings.IsCalibrated`
  - Show/hide `_playDisabledLabel` based on `!GameSettings.IsCalibrated`
- **Play** → `GameSettings.SkipCalibration = true`, `SceneManager.LoadScene(1)`
- **Train BCI** → `GameSettings.SkipCalibration = false`, `SceneManager.LoadScene(1)`
- **Settings** → hide `_buttonPanel`, show `_settingsPanel`
- **Quit** → `Application.Quit()` (+ `#if UNITY_EDITOR UnityEditor.EditorApplication.isPlaying = false`)

### 3.5 SettingsMenu.cs

**File:** `Assets/Scripts/UI/SettingsMenu.cs`

**Inspector fields:**

| Field | Type |
|-------|------|
| `_masterVolumeSlider` | Slider |
| `_volumeLabel` | TextMeshProUGUI |
| `_backButton` | Button |
| `_buttonPanel` | GameObject |
| `_settingsPanel` | GameObject |

**Behavior:**

- `Start()`: load volume from `PlayerPrefs.GetFloat("MasterVolume", 1f)`, set slider value and `AudioListener.volume`
- Slider `onValueChanged` → set `AudioListener.volume`, save to `PlayerPrefs.SetFloat("MasterVolume", value)`, update label to show percentage (e.g., "75%")
- **Back** → hide `_settingsPanel`, show `_buttonPanel`

### 3.6 GameManager Changes

- `Start()` reads `GameSettings.SkipCalibration` instead of `_debugMode` to decide whether to enter Calibration or Playing state
- When calibration completes: set `GameSettings.IsCalibrated = true`
- GameOver → pressing R loads MainMenu scene (build index 0) instead of reloading game scene

---

## 4. CLAUDE.md Updates Required

After implementation, update CLAUDE.md to reflect:
- New `Assets/Scripts/UI/` scripts: `MainMenu.cs`, `SettingsMenu.cs`
- New `Assets/Scripts/Core/GameSettings.cs`
- New `Assets/Scripts/Enemy/WeaponConfig.cs`
- New `Assets/Data/Weapons/` directory for ScriptableObject assets
- Updated architecture table: add `MainMenu`, `SettingsMenu`, `GameSettings`, `WeaponConfig`
- Updated scene list: `MainMenu` + `SampleScene`
- Updated `EnemySpawner` description: wave-based → endless wave system

---

## 5. New Spec Files Required

These features should be written as formal specs in `docs/specs/`:

| Spec | File | Depends On |
|------|------|-----------|
| Spec 12 — Enemy Weapon Variety | `docs/specs/12-enemy-weapon-variety.md` | Spec 04, 05 |
| Spec 13 — Endless Wave System | `docs/specs/13-endless-wave-system.md` | Spec 07, 12 |
| Spec 14 — Main Menu & Settings | `docs/specs/14-main-menu-settings.md` | Spec 10 |
