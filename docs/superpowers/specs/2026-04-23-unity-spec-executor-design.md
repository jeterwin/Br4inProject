# Unity Spec Executor — Design Spec

## Goal

Create a Claude Code skill and supporting infrastructure that drives spec-to-implementation workflow for the SUPERHOT BCI game. The skill reads a spec file, implements it, verifies acceptance criteria, and automatically generates two feature documents (technical reference + judge-friendly summary card). CLAUDE.md rules enforce that documentation is a mandatory output of every implementation.

## Decisions

| Decision | Choice |
|----------|--------|
| Skill relationship to existing skills | Orchestrator — invokes `unity-bci-game` and `gtec-integration` as needed |
| Documentation per feature | Two docs: technical reference + summary card |
| Doc audience | Both devs (us) and external (judges) |
| Session model | One spec per session |
| Rules location | CLAUDE.md for enforcement, skill for detailed workflow |
| Documentation trigger | Automatic — built into skill workflow, not a hook |

## Deliverables

1. **Skill file:** `.claude/skills/unity-spec-executor.md`
2. **CLAUDE.md update:** New "Implementation Workflow" section with enforcement rules
3. **Directory structure:** `docs/features/` and `docs/features/summaries/`
4. **Index file:** `docs/features/README.md`

---

## 1. Skill: `unity-spec-executor`

**Location:** `.claude/skills/unity-spec-executor.md`

**Trigger:** Use when implementing a spec file — when the user says "implement spec X", "build spec X", or references a numbered spec from `docs/specs/`. Also triggers on "implement feature", "next spec", or any request to turn a spec into working code.

### Workflow Phases

The skill enforces this exact sequence. No phase may be skipped.

#### Phase 1 — Load Spec
- Read the spec file from `docs/specs/XX-name.md`
- Parse: summary, scripts to create/modify, inspector fields, behavior rules, dependencies, acceptance criteria, Unity setup instructions
- Check dependencies: verify that prerequisite specs are already implemented (check `docs/features/README.md` index)
- If dependencies are missing, warn the user and ask whether to proceed

#### Phase 2 — Plan
- Break the spec into implementation tasks using TaskCreate
- Each task corresponds to a script to create/modify or a scene/prefab change
- Tasks ordered by internal dependencies (e.g., create script before configuring prefab)
- If the spec touches BCI code: note that `gtec-integration` skill rules apply (thread safety, EventHandler.Instance.Enqueue, etc.)
- If the spec touches game scripts: note that `unity-bci-game` skill rules apply (time-freeze conventions, unscaledDeltaTime for BCI/UI, etc.)

#### Phase 3 — Implement
- Work through tasks sequentially
- Follow project coding conventions from CLAUDE.md:
  - PascalCase public, `_camelCase` private
  - One MonoBehaviour per file, filename = class name
  - `[SerializeField]` for inspector-exposed private fields
  - `[RequireComponent]` where dependencies exist
  - No comments unless WHY is non-obvious
- Place scripts in the correct folder per CLAUDE.md organization (`Core/`, `Player/`, `Enemy/`, `BCI/`, `UI/`)
- Mark each task completed as it's done

#### Phase 4 — Verify
- Walk through every acceptance criterion listed in the spec
- For code-verifiable criteria: check the implementation matches
- For editor-verifiable criteria: produce the Unity Setup & Test instructions (these go into the technical doc)
- If any criterion is not met: fix before proceeding
- Do NOT claim completion without checking every criterion

#### Phase 5 — Document
Generate two documents:

**Technical Reference** → `docs/features/XX-name.md`

```markdown
# Feature XX — {Name}

## Overview
{2-3 sentences: what this feature does and why it exists in the game}

## Scripts

| Script | Path | Purpose |
|--------|------|---------|
| {ClassName} | {Assets/Scripts/...} | {One-line purpose} |

## Inspector Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| {_fieldName} | {type} | {default} | {What it controls} |

## How It Works
{Runtime behavior: what happens when, key methods, event flow between systems.
Written so a developer can understand the feature without reading the source.}

## Dependencies
{Which specs/systems must be in place for this to work}

## Unity Setup & Test
{Step-by-step Unity Editor instructions — copied/refined from the spec's acceptance criteria.
Numbered checklist: GameObjects to create, components to add, fields to configure,
how to enter Play mode and verify, expected behavior, common problems.}

## Known Limitations
{What's intentionally out of scope for MVP, if anything}
```

**Summary Card** → `docs/features/summaries/XX-name-summary.md`

```markdown
# {Feature Name}

> {One-sentence description of what users/players experience}

**Category:** {Core Mechanic / Player / Enemy AI / BCI / UI}
**Status:** Implemented
**Depends on:** {List of prerequisite feature names}
**Scripts:** {Count} ({new/modified}) — {list of class names}
```

#### Phase 6 — Update Index
Update `docs/features/README.md`:

```markdown
# Implemented Features

| # | Feature | Status | Technical Doc | Summary |
|---|---------|--------|---------------|---------|
| 01 | Time Manager | Implemented | [Reference](01-time-manager.md) | [Summary](summaries/01-time-manager-summary.md) |
| 02 | Player Movement | Implemented | [Reference](02-player-movement.md) | [Summary](summaries/02-player-movement-summary.md) |
| 03 | Arena | Not started | — | — |
| ... | ... | ... | ... | ... |
```

#### Phase 7 — Commit
- Stage all new/modified files (scripts, docs, CLAUDE.md if updated)
- Commit message format:
  - New implementation: `implement spec XX: {feature name}`
  - Documentation-only (already-built specs): `document spec XX: {feature name}`
- Do NOT push unless the user asks

### Domain Skill Integration

The skill does not contain game or BCI domain knowledge. Instead:

- When the spec involves scripts in `Assets/Scripts/BCI/` or references g.tec SDK types → the existing `gtec-integration` skill provides the SDK patterns, thread safety rules, and BCI-specific conventions
- When the spec involves scripts in `Assets/Scripts/Core/`, `Player/`, `Enemy/`, or `UI/` → the existing `unity-bci-game` skill provides the time-freeze conventions, architecture rules, and game-specific patterns
- The spec executor skill references these by name in its instructions so Claude knows to load them when relevant

### Error Handling

- If a dependency spec is not implemented: warn and ask user
- If a spec file doesn't exist at the expected path: ask user for the correct path
- If acceptance criteria are ambiguous: ask user to clarify before marking verified
- If implementation requires scene changes that can't be done via code: produce detailed Unity Editor instructions and note what the user must do manually

---

## 2. CLAUDE.md Rules

Add this section to the existing CLAUDE.md:

```markdown
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
```

---

## 3. Directory Structure

```
docs/
├── specs/                  # Source of truth (already exists, 11 spec files)
│   ├── 01-time-manager.md
│   ├── ...
│   └── 11-hud-ui.md
├── features/               # Generated after implementation
│   ├── README.md           # Index of all features with status
│   ├── 01-time-manager.md  # Technical reference
│   ├── 02-player-movement.md
│   ├── ...
│   └── summaries/
│       ├── 01-time-manager-summary.md  # Judge-friendly card
│       ├── 02-player-movement-summary.md
│       └── ...
├── superhot-test-setup.md          # Existing
├── enemy-shooting-test-setup.md    # Existing
└── superpowers/specs/              # Existing brainstorming specs
```

---

## 4. Full Session Flow Example

```
User: "implement spec 04"

Phase 1 — Load:
  Read docs/specs/04-enemy-controller.md
  Check index: Spec 01 ✓, Spec 03 ✓ → dependencies met

Phase 2 — Plan:
  Task 1: Create Assets/Scripts/Enemy/EnemyController.cs
  Task 2: Create Enemy prefab with NavMeshAgent + EnemyController
  Task 3: Verify acceptance criteria
  Task 4: Write technical doc
  Task 5: Write summary card
  Task 6: Update index

Phase 3 — Implement:
  Write EnemyController.cs following conventions
  (unity-bci-game skill rules apply: Update() auto-freezes, etc.)

Phase 4 — Verify:
  ✓ Enemy navigates around obstacles
  ✓ Enemy stops at stopping distance
  ✓ Enemy freezes when player stops
  ✓ Enemy resumes on player move
  ✓ Multiple enemies don't overlap

Phase 5 — Document:
  Write docs/features/04-enemy-controller.md
  Write docs/features/summaries/04-enemy-controller-summary.md

Phase 6 — Index:
  Update docs/features/README.md: Spec 04 → Implemented

Phase 7 — Commit:
  git commit: "implement spec 04: enemy controller"
```

---

## Verification

To verify this system works end-to-end:
1. Create the skill file and CLAUDE.md rules
2. Create the directory structure and initial README.md index
3. Test by implementing Spec 01 (TimeManager — already built, so this is a documentation-only pass)
4. Verify both docs are generated correctly
5. Verify the index is updated
6. Then proceed with Spec 03 (Arena) as the first real implementation
