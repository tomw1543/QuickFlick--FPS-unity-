# SPEC.md — QuickFlick

Stationary FPS-style space shooter. Camera is a turret: you don't move, you look. Dual inputs merge into one view through `StickCombiner`. Two cannons, both firing within 150 ms = MegaShot (3× damage). Survive waves of enemies.

Tonight: mouse input + 1 enemy type (Drone). Arduino sticks + more enemy types tomorrow.

## Data

### Player

| Field | Value |
|---|---|
| HP | 5 |
| Cannon cooldown | 0.15 s per cannon |
| MegaShot window | both cannons fire within 0.15 s |
| MegaShot damage | 3× single-cannon |

### Enemies

| Name | HP | Speed | Damage on hit | Behaviour | Tonight? |
|---|---|---|---|---|---|
| Drone | 1 | 8 u/s | 1 | Straight line toward player | Yes |
| Weaver | 2 | 6 u/s | 1 | Sine-weave approach | No |
| Brute | 4 | 4 u/s | 2 | Slow, returns fire | No |

`Enemy` is an abstract base class. Weaver and Brute are subclasses added later.

### Scoring

| Event | Points |
|---|---|
| Drone kill | 100 |
| MegaShot kill | 3× base |
| Wave clear bonus | 500 × waveNumber |

### Waves (tonight)

| Wave | Spawn | Spawn rate | Clear condition |
|---|---|---|---|
| 1 | 5 Drones | 1 / 1.5 s | All dead |
| 2 | 8 Drones | 1 / 1.2 s | All dead |
| 3 | 12 Drones | 1 / 1.0 s | All dead |
| 4+ | +4 Drones, rate × 0.9 | | |

5-second banner between waves.

### HUD (minimal — Q15 (b))

- Reticle: circle-with-dot, circle reddens on enemy raycast
- 5 HP pips top-left
- Score top-right
- Wave banner centre, between waves only
- F3 toggles debug overlay (stick values, FPS, enemy count)

## Phases

Each phase ends with an editor-test list. Do not start the next phase until the human confirms the test list passes.

### Phase 0 — Scaffold

Docs (this file, CLAUDE.md, CONTEXT.md, README.md), area CLAUDE.md stubs, `.gitignore`, `.asmdef`, git init + first push. SampleScene deleted. **Done.**

### Phase 1 — Settings + Bootstrap stub

- `QuickFlick.Bootstrap.GameBootstrap` MonoBehaviour on the Bootstrap GO.
- `QuickFlick.Systems.Settings` ScriptableObject or static class holding constants from Data above.
- Logs "Boot OK" on Start.

Editor test: press Play, console shows "Boot OK".

### Phase 2 — Input + Camera

- `QuickFlick.Input.IControllerInput` — `float2 LeftStick, RightStick; bool LeftFire, RightFire;`
- `QuickFlick.Input.MouseInput : IControllerInput` — mouse offset from screen centre → both sticks. LMB → LeftFire, RMB → RightFire. Cursor locked + hidden.
- `QuickFlick.Input.StickCombiner` (pure) — merges both sticks into (lookYaw, lookPitch, speedScale). Both sticks parallel = 1.0 speed; opposing = 0.
- `QuickFlick.Camera.TurretCamera` — rotates main camera from combiner output. Pitch clamped ±70°.
- `GameBootstrap` instantiates MouseInput + combiner + wires camera.

Editor test: Play. Mouse moves camera. Cursor hidden. F3 (if implemented) shows stick values.

### Phase 3 — Combat

- `QuickFlick.Combat.Cannon` — ray or projectile from a muzzle offset, cooldown 0.15 s, fires on input bool rising edge.
- `QuickFlick.Combat.Projectile` — pooled, speed, lifetime, damage, on-hit → enemy.TakeDamage.
- `QuickFlick.Combat.MegaShotDetector` — watches both cannons, if both fire within 0.15 s swap next shot's damage to 3× and emit event.

Editor test: Play. LMB and RMB fire. Both together → megashot (visible: fatter projectile or colour swap).

### Phase 4 — Enemies

- `QuickFlick.Enemies.Enemy` (abstract) — HP, OnDeath event, abstract Tick.
- `QuickFlick.Enemies.Drone : Enemy` — flies straight at player at 8 u/s.
- `QuickFlick.Systems.Spawner` — spawns Drones at a ring radius 30 u around player at random yaw.
- Pooling: `ObjectPool<Enemy>` and `ObjectPool<Projectile>`.

Editor test: Play. Drones spawn around player, fly in, die to cannon fire.

### Phase 5 — Waves + Score + HP + GameState

- `QuickFlick.Systems.WaveManager` — drives Spawner per Waves table. Emits WaveClear / WaveStart.
- `QuickFlick.Systems.Score` — adds on kill, 3× on megashot, 500×wave on clear.
- `QuickFlick.Systems.PlayerHp` — 5 HP, reduces on enemy-reach, GameOver at 0.
- `QuickFlick.Systems.GameState` — Boot → Playing → GameOver, Space to restart.

Editor test: Play. Wave 1 spawns 5 drones, clearing starts wave 2. HP drops when a drone reaches you. 0 HP → game over screen.

### Phase 6 — HUD + PostFx + Audio (TIER 2 PARALLEL)

Three independent streams, one Sonnet builder each:
- **UI**: Reticle, HP pips, score, wave banner, F3 debug overlay.
- **PostFx**: `PostFxBuilder` fills Global Volume with Bloom + Vignette + Tonemapping.
- **Audio**: `MusicPlayer` loops the default track, `Sfx` plays cannon-fire / hit / megashot / death.

Editor test: Play. HUD shows. Bloom on cannon shots. Music loops. SFX on fire/hit.

### Phase 7 — Polish

Juice from HANDOFF Q9 in order, stopping when time runs out. Off-screen arrows stay in regardless.

## Serial protocol

TBD when hardware arrives. Contract will live in `Assets/Scripts/Input/CLAUDE.md` and `ControllerProtocolTests.cs`.

## Build target

Editor Play Mode only tonight. Standalone .exe tomorrow.
