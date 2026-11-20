# TILT STRIKE — Handoff (grill-with-docs, paused mid-session)

Read `PLAN.md` first for the full build spec. This file captures what has been
**decided** in the grilling session and what is **still open** so a new Claude
session can pick up without re-grilling.

Ponytail mode is active (lazy senior dev: smallest change that fully works).

---

## Scope for tonight (solo, keyboard+mouse, on tom's laptop)

- **Full phases 0–7** of `PLAN.md` playable tonight. Hardware (Arduino + sticks)
  arrives tomorrow morning — the input layer is abstracted so the swap is
  one-line in `GameBootstrap`.
- Dropping `calibrate` screen for tonight (mouse doesn't need it). Bring it back
  tomorrow for the stick version.

## Settled decisions

| # | Topic | Decision |
|---|---|---|
| Q1 | Acceptance bar tonight | Full phases 0–7 (polish included), cut-list in Q9 if time runs out |
| Q2 | Fire bindings model | Keep the two-cannon `IControllerInput` interface; don't try to simulate dual sticks on mouse |
| Q3 | Input device tonight | **Mouse replaces WASD** (position = virtual tilt), closer mapping to sticks |
| Q4 | Code-first rule | **Dropped** — build in the Unity editor like a normal project (solo, no merge-conflict risk) |
| Q5 | EditMode unit tests | **Skip tonight**. Add `ControllerProtocolTests.cs` tomorrow before wiring serial |
| Q6 | Mouse look mapping | **Rate control via mouse position** (offset from screen centre = virtual tilt angle). Cursor locked + hidden. Same `StickCombiner` code path as the Arduino tomorrow — one-line swap |
| Q7 | Fire buttons | LMB = left cannon, RMB = right cannon. Both within 0.15 s = **MEGA SHOT** (3× damage, renamed DOUBLE SHOT from plan) |
| Q8 | Unity project setup | Tom clicks through Unity Hub create; Claude writes scripts into the created project |
| Q9 | Juice cut order (if time runs out) | nebulae → planet → music loop → asteroids → slow-mo → combo multiplier → title screen → Brute return-fire → off-screen arrows → SFX → everything else stays. **Off-screen arrows stay high** — only hint of enemies behind you when look-speed is slow |
| Q10 | Audio source | Kenney CC0 pack — superseded by user-dropped music files (see below) |
| Q11 | Time mechanic | **Normal time** (SUPERHOT reference is visual + control-style only, NOT the time gimmick). Can refactor later if time |
| Q12 | Palette / aesthetic | Original plan palette: cyan lasers, magenta enemies, deep navy space, nebulae/planet/asteroids as juice tier. SUPERHOT reference = low-poly flat-shaded geometry + FPS-style controls, nothing about color |
| Q13 | Enemies tonight | **One type only**: Drone (angular red shard, flies straight, 1 HP). `Enemy` must be an abstract base class / interface so Weaver + Brute slot in as subclasses later. Extensible is non-negotiable |

### Mouse-into-input-interface resolution (decided by inference, not grilled)

`MouseInput` writes MouseX-offset into both `LeftRoll` and `RightRoll`, same
MouseY into both `LeftPitch` and `RightPitch`. `StickCombiner` sees "both sticks
moving together" and naturally gives the 100%-speed curve. No changes to the
shared maths when the Arduino swaps in tomorrow.

## Audio files (dropped at repo root)

```
331music-video-game-music-598332.mp3
musicwallah-no-copyright-gaming-80s-sci-fi-vibe-with-analog-synths-472678.mp3
psychronic-let-the-games-begin-21858.mp3
serhii_kliets-spaceship-arcade-shooter-game-background-soundtrack-318508.mp3
```

**Default pick:** `serhii_kliets-spaceship-arcade-shooter-...mp3` (best name-match).
Move all four into `unity/TiltStrike/Assets/Audio/` once the Unity project exists.
Swapping tracks is a one-line config change.

## Open questions (frontier when session paused)

### Q14 — Main.unity GameObject list (confirm the list before coding)

Proposed scene contents:
- Main Camera (origin, rotation 0,0,0)
- Directional Light (default URP new-scene light)
- Global Volume (`PostFxBuilder` fills Bloom + Vignette + Tonemapping at runtime)
- Bootstrap (empty GameObject with `GameBootstrap.cs` — spawns everything else)
- EventSystem (auto-created on first UI element)

Recommended: **confirm as-is**.

### Q15 — HUD density

- (a) Full plan HUD (reticle, HP pips, score, combo, wave, cannon cooldowns, controller status, F3 debug)
- (b) **Minimal**: reticle, 5 HP pips top-left, score top-right, wave banner only during wave break, F3 debug overlay (toggle)
- (c) Superhot-style: reticle only in gameplay; stats on death only

Recommended: **(b)**.

### Q16 — Reticle style

Dot / thin cross / **circle-with-dot** (circle doubles as hit-highlight, turns red on enemy raycast).

Recommended: **circle-with-dot**.

### Unity setup progress

Tom was mid-way through Unity Hub create steps (see "Unity Hub setup" in the
conversation / phase 0 of `PLAN.md`). When resuming:

1. Verify `unity/TiltStrike/` exists and opens in Unity 6000.0.27f1.
2. Verify **Input System** is installed and input handling is **Both**.
3. Verify Api Compatibility Level is **.NET Framework** (or `.NET Standard 2.1`
   if Unity 6 renamed the option — we'll fix on the day we wire serial).
4. Verify `Assets/Scenes/Main.unity` exists and is in Build Settings.

## Next action when resuming

1. Ask Tom to answer Q14/Q15/Q16 (or re-confirm the recommendations above).
2. Confirm Unity project opens cleanly.
3. Move audio files into `Assets/Audio/`.
4. Start Phase 0 scaffolding: `.gitignore` (Unity template + .env + build/),
   `README.md`, `CLAUDE.md`, folder stubs under `unity/TiltStrike/Assets/Scripts/`
   following the layout in `PLAN.md` §3.
5. Then straight to Phase 2 (input + camera): `IControllerInput`,
   `MouseInput` (NOT keyboard — Q6 decided mouse), `StickCombiner`, `TurretCamera`,
   wire into `GameBootstrap`. Playable look-around on launch.

## Known risks to flag at resume

- Unity Hub create may hit a sign-in / licence wall. Do this first.
- If `.NET Framework` compat level isn't in the dropdown exactly as named, pick
  the closest Framework-compatible one. Fix on the day we wire `System.IO.Ports`.
- Mp3 loop seams can click. If audible, convert one track to .ogg tomorrow.
- SUPERHOT inspiration is **visual and control-style only** — do NOT implement
  time-scaling-with-rotation unless the user explicitly asks again.
