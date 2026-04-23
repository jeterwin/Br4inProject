---
name: unity-spec-executor
description: Drives spec-to-implementation workflow — reads a spec file from docs/specs/, implements it, verifies acceptance criteria, generates feature docs (technical reference + summary card), and updates the feature index. Use when implementing a spec file.
---

# Unity Spec Executor

Use when implementing a spec file — when the user says "implement spec X", "build spec X", or references a numbered spec from `docs/specs/`. Also triggers on "implement feature", "next spec", or any request to turn a spec into working code.

## Hard Rules

- Follow ALL phases in order. No phase may be skipped.
- Do NOT claim a spec is complete without both feature docs written.
- Do NOT commit without verifying every acceptance criterion from the spec.
- Scene/prefab changes cannot be made via code — produce Unity Editor instructions for the user.

## Phase 1 — Load Spec

1. Read the spec file from `docs/specs/XX-name.md`
2. Parse out: summary, scripts to create/modify, inspector fields, behavior rules, dependencies, acceptance criteria
3. Read `docs/features/README.md` to check which prerequisite specs are already implemented
4. If dependencies are missing: warn the user, list what's missing, ask whether to proceed

## Phase 2 — Plan

1. Create tasks (TaskCreate) — one per script to create/modify, plus tasks for verify, document, and index
2. Order tasks by internal dependencies (create script before configuring prefab that uses it)
3. Identify which domain skills apply:
   - Scripts in `Assets/Scripts/BCI/` or using g.tec SDK types → follow `gtec-integration` skill (thread safety, EventHandler.Instance.Enqueue, BCI patterns)
   - Scripts in `Assets/Scripts/Core/`, `Player/`, `Enemy/`, `UI/` → follow `unity-bci-game` skill (time-freeze conventions, unscaledDeltaTime for BCI/UI)

## Phase 3 — Implement

Work through tasks sequentially. For each script:

1. Follow coding conventions from CLAUDE.md:
   - PascalCase public, `_camelCase` private
   - One MonoBehaviour per file, filename = class name
   - `[SerializeField]` for inspector-exposed private fields
   - `[RequireComponent]` where dependencies exist
   - No comments unless WHY is non-obvious
2. Place scripts in the correct folder: `Core/`, `Player/`, `Enemy/`, `BCI/`, `UI/`
3. Match the spec exactly — inspector fields, types, defaults, behavior rules
4. Mark each task completed as it's done

## Phase 4 — Verify

Walk through EVERY acceptance criterion listed in the spec:

- For code-verifiable criteria: confirm the implementation matches
- For editor-verifiable criteria: include them in the Unity Setup & Test section of the technical doc
- If any criterion is not met: fix it before proceeding
- Produce a checklist showing each criterion and its status

## Phase 5 — Document

Generate two files:

### Technical Reference → `docs/features/XX-name.md`

```markdown
# Feature XX — {Name}

## Overview
{2-3 sentences: what this feature does and why it exists in the game}

## Scripts

| Script | Path | Purpose |
|--------|------|---------|
| {ClassName} | {Assets/Scripts/Folder/ClassName.cs} | {One-line purpose} |

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
{Step-by-step Unity Editor instructions.
Numbered checklist: GameObjects to create, components to add, fields to configure,
how to enter Play mode and verify, expected behavior, common problems.}

## Known Limitations
{What's intentionally out of scope for MVP, if anything}
```

### Summary Card → `docs/features/summaries/XX-name-summary.md`

```markdown
# {Feature Name}

> {One-sentence description of what users/players experience}

**Category:** {Core Mechanic / Player / Enemy AI / BCI / UI}
**Status:** Implemented
**Depends on:** {List of prerequisite feature names}
**Scripts:** {Count} ({new/modified}) — {list of class names}
```

## Phase 6 — Update Index

Edit `docs/features/README.md` — set the spec's row status to "Implemented" and add links to both docs.

## Phase 7 — Commit

- Stage all new/modified files (scripts + docs)
- Commit message format:
  - New implementation: `implement spec XX: {feature name}`
  - Documentation-only (already-built specs): `document spec XX: {feature name}`
- Do NOT push unless the user explicitly asks

## Error Handling

- Dependency spec not implemented → warn user, list what's missing, ask whether to proceed
- Spec file not found at expected path → ask user for the correct path
- Acceptance criteria ambiguous → ask user to clarify before marking verified
- Scene/prefab changes needed → produce detailed Unity Editor instructions, note what user must do manually
