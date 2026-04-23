# Spec 11 — HUD / UI

**Status:** To build
**Scripts:** `Assets/Scripts/UI/GameHUD.cs`, `Assets/Scripts/UI/GameOverScreen.cs`
**Dependencies:** Spec 10 (GameManager)

## Summary

Minimal UI overlay showing game state. A HUD during gameplay (enemy count, score, timer) and a game-over screen (win/lose, final score, restart prompt).

## HUD Elements (during Playing state)

| Element | Position | Content | Update Frequency |
|---------|----------|---------|-----------------|
| Enemy Counter | Top-right | "Enemies: 3/6" | On enemy death |
| Score | Top-left | "Score: 3" | On enemy death |
| Timer | Top-center | "00:42" | Every frame (unscaled) |

## Game Over Screen

| Element | Content |
|---------|---------|
| Title | "SUPER HOT" (win) or "ELIMINATED" (lose) |
| Score | "Score: 6" |
| Time | "Time: 01:23" |
| Restart Prompt | "Press R to restart" |

## Script Details

### GameHUD.cs

**File:** `Assets/Scripts/UI/GameHUD.cs`

**Inspector fields:**

| Field | Type | Default |
|-------|------|---------|
| `_enemyCountText` | TextMeshProUGUI | reference |
| `_scoreText` | TextMeshProUGUI | reference |
| `_timerText` | TextMeshProUGUI | reference |
| `_hudPanel` | GameObject | reference to HUD panel |

**Behavior:**
- On `Start()`: subscribe to `GameManager.Instance.OnStateChanged`
- Show `_hudPanel` when state is Playing, hide otherwise
- In `Update()`: if Playing state:
  - `_timerText.text` = format `GameManager.Instance.ElapsedTime` as "MM:SS"
  - `_scoreText.text` = `"Score: " + GameManager.Instance.Score`
  - `_enemyCountText.text` = format from EnemySpawner alive/total counts
- Uses `Time.unscaledDeltaTime` awareness — Update runs even during freeze because UI text assignment doesn't depend on deltaTime

### GameOverScreen.cs

**File:** `Assets/Scripts/UI/GameOverScreen.cs`

**Inspector fields:**

| Field | Type | Default |
|-------|------|---------|
| `_titleText` | TextMeshProUGUI | reference |
| `_scoreText` | TextMeshProUGUI | reference |
| `_timeText` | TextMeshProUGUI | reference |
| `_restartText` | TextMeshProUGUI | reference |
| `_gameOverPanel` | GameObject | reference |

**Behavior:**
- On `Start()`: subscribe to `GameManager.Instance.OnStateChanged`
- Hide `_gameOverPanel` initially
- When state changes to GameOver:
  - Show `_gameOverPanel`
  - Set `_titleText` to "SUPER HOT" if `GameManager.Instance.DidWin`, else "ELIMINATED"
  - Set `_scoreText` to final score
  - Set `_timeText` to formatted elapsed time
  - Set `_restartText` to "Press R to restart"
- When state changes away from GameOver: hide `_gameOverPanel`

## Canvas Setup

```
GameCanvas (Canvas, Screen Space - Overlay, Sort Order 10)
├── HUDPanel
│   ├── ScoreText      — TextMeshPro, top-left anchor, "Score: 0"
│   ├── TimerText      — TextMeshPro, top-center anchor, "00:00"
│   └── EnemyCountText — TextMeshPro, top-right anchor, "Enemies: 0/0"
├── GameOverPanel (initially inactive)
│   ├── TitleText      — TextMeshPro, center, large font (48pt), "SUPER HOT"
│   ├── FinalScoreText — TextMeshPro, below title, "Score: 0"
│   ├── FinalTimeText  — TextMeshPro, below score, "Time: 00:00"
│   └── RestartText    — TextMeshPro, bottom, "Press R to restart"
```

## Font & Style

- Use TextMeshPro (already in project via Packages/manifest.json)
- HUD text: white, 24pt, with slight shadow for readability
- Game over title: white, 48pt, bold
- Game over details: white, 28pt

## Acceptance Criteria

1. HUD displays during Playing state with correct enemy count, score, and timer
2. Score updates immediately when enemies die
3. Timer counts up accurately during gameplay (wall-clock time)
4. HUD text is visible and readable against the game scene
5. HUD updates even during time freeze (text doesn't depend on deltaTime)
6. Game over screen appears on win with "SUPER HOT" title
7. Game over screen appears on lose with "ELIMINATED" title
8. Game over screen shows correct final score and time
9. Game over screen hidden during Calibration and Playing states
10. R key triggers restart from game over screen

## Unity Setup & Test

1. Create a Canvas in the scene: right-click Hierarchy > UI > Canvas
2. Set Canvas to Screen Space - Overlay, Sort Order = 10
3. Name it "GameCanvas"
4. **HUD Panel:**
   a. Right-click GameCanvas > Create Empty, name "HUDPanel"
   b. Set RectTransform to stretch-stretch (full screen)
   c. Right-click HUDPanel > UI > TextMeshPro Text, name "ScoreText"
   d. Anchor ScoreText to top-left, set text "Score: 0", font size 24, color white
   e. Repeat for "TimerText" (top-center) and "EnemyCountText" (top-right)
5. **GameOver Panel:**
   a. Right-click GameCanvas > Create Empty, name "GameOverPanel"
   b. Uncheck the active checkbox (initially hidden)
   c. Add a dark semi-transparent Image component as background (optional, RGBA 0,0,0,180)
   d. Create "TitleText" — center anchor, font size 48, bold, "SUPER HOT"
   e. Create "FinalScoreText" — below title, font size 28
   f. Create "FinalTimeText" — below score, font size 28
   g. Create "RestartText" — near bottom, font size 24, "Press R to restart"
6. **Wire up scripts:**
   a. Add `GameHUD` component to GameCanvas — drag text references and HUDPanel
   b. Add `GameOverScreen` component to GameCanvas — drag text references and GameOverPanel
7. **Test:**
   a. Enter Play mode — HUD should show score, timer, enemy count
   b. Timer should count up as you play
   c. Let all enemies die — "SUPER HOT" game over screen should appear
   d. Press R — scene reloads
   e. Get hit by a bullet — "ELIMINATED" screen should appear
