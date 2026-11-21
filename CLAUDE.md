# CLAUDE.md — QuickFlick

Unity 6 (URP, C#) stationary space shooter. Dual analog sticks over Arduino USB serial (tomorrow); mouse tonight. Read `SPEC.md` for scope, phases and acceptance. Read `CONTEXT.md` for the glossary.

## Rules

- Work phase by phase from `SPEC.md`. At the end of each phase, list exactly what the human should test in the Unity editor, then stop.
- **Hybrid scene policy.** `Main.unity` holds only the five bootstrap objects (Camera, Directional Light, Global Volume, Bootstrap GO, EventSystem — see `SPEC.md` phase 0). Everything else (projectiles, enemies, HUD, post-fx volume content) is instantiated at runtime by `GameBootstrap` or its spawners. Do not hand-edit prefabs, .asset or .meta files; `Main.unity` is the only hand-editable scene file.
- Keep pure logic (`StickCombiner`, `ControllerProtocol`) free of UnityEngine dependencies.
- Tests: required for the serial protocol (`ControllerProtocolTests.cs`, added when serial lands). Optional elsewhere — a small assert-based self-check is enough for tonight's new logic.
- Serial reading happens on a background thread only. Never call Unity APIs from that thread. Always close the `SerialPort` on quit.
- Parse floats with `CultureInfo.InvariantCulture`.
- A no-hardware input (mouse tonight, keyboard later if needed) must always work, so every feature can be tested without the Arduino.
- Use object pooling for projectiles, enemies and effects. Target 60+ FPS.
- Check compile errors in `%LOCALAPPDATA%\Unity\Editor\Editor.log`.
- **Commit after every working change.** The main session git-commits once a change is verified working in the editor (even a one-liner). Reason: we lost today's orange-planet baseline because nothing was committed and six Sonnet attempts drifted away from it. No batching commits across multiple changes.
- Arduino: compile and upload with `arduino-cli` (commands land in `SPEC.md` when wired).
- Never write API keys or secrets into any file in this repo.

## Build workflow

**Tonight (solo, mouse-only, one laptop):** Tier 2 opportunistically. Inline single-stream work; spawn parallel Sonnet builders when a phase has independent streams (e.g. Phase 6 UI + Audio + PostFx). No Opus review tonight — Phase 0 scaffold and gameplay stubs have no serial/threading risk.

**From tomorrow (hardware + serial):** full workflow below applies to every change touching `SerialInput`, firmware, or anything threaded.

### Model rule

- **All code is written by Sonnet.** Every line of code, test, shader, firmware or config comes from a Sonnet subagent (`.claude/agents/sonnet-builder.md`).
- The main session does NOT write or edit code. It plans, delegates, merges results, talks to the human.
- The only non-Sonnet agent is the Opus reviewer (`.claude/agents/opus-reviewer.md`) for Tier 3. It reviews and tests; it never edits code.
- Every builder prompt includes: **"Load and follow the `ponytail:ponytail` skill."**

### Tiers

| Tier | Use for | Setup |
|---|---|---|
| **1: Small** | Bug fixes, small refactors, tuning numbers | 1 Sonnet builder |
| **2: Medium** | Several independent workstreams on the existing game | Multiple Sonnet builders in parallel, main session manages |
| **3: Heavy** | Project init, complete refactors, several features at once, anything touching serial or threading | Multiple Sonnet builders + Opus reviewer that must approve |

State the tier and a one-line reason at the top of every reply, e.g. `TIER 2: HUD + waves + new enemy`.

### Escalate to Opus on bug-fix miss (exception, ask first)

Not a default. When a Sonnet builder fails to one-shot a bug fix (first attempt doesn't resolve it, or the bug recurs), **ask the human before escalating**. If they confirm, spawn `opus-reviewer` for root-cause analysis — read-only, no edits — then hand the diagnosis to a Sonnet builder for the fix. Never escalate without an explicit go-ahead.

### Tier 2 details

1. Split work into streams that don't touch the same files. Write each stream's files and acceptance criteria.
2. Spawn one `sonnet-builder` per stream, in parallel.
3. Main session collects reports, resolves conflicts by sending fixes back to a builder (never by editing code directly), summarises for the human with exactly what to test in the editor.

### Tier 3 details

1. Write full plan: streams, file ownership, interfaces, acceptance criteria. Show the human before building.
2. Spawn `sonnet-builder`s in parallel.
3. Spawn `opus-reviewer` with: plan, acceptance criteria, changed files. It returns **PASS** or **FAIL** with specific issues.
4. PASS → summarise for human. FAIL → do NOT silently retry. Give the human the findings and grill with `AskUserQuestion`. Send agreed fixes to builders, re-review. Repeat until PASS.

> Subagents can't spawn their own subagents. The main session launches builders and the Opus reviewer.

### Every builder prompt must include

- "Load and follow the `ponytail:ponytail` skill."
- "Read `CLAUDE.md`, the relevant section of `SPEC.md`, and the `CLAUDE.md` of the folder you're working in."
- The exact files it owns (and files it must NOT touch).
- Acceptance criteria and how to verify (tests to run, what to check in Editor.log).
- "Report back: files changed, what you did, how you verified, anything unfinished."

## Style

- One class per file. Namespaces `QuickFlick.<Folder>`.
- Tunable numbers go in `Settings` or `[SerializeField]`/const at the top of the class, not buried in code.
- Area `CLAUDE.md` files hold the one convention that matters for that area. Read the area file before editing in it.
