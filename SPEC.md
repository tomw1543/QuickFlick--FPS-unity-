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
| MegaShot | AOE: kills all enemies within camera cone (FOV) out to 15 u. Nothing behind player. |

### Enemies

| Name | HP | Speed | Damage | Behaviour | Tonight? |
|---|---|---|---|---|---|
| Asteroid (big) | 1 | 2 u/s | 1 | Straight at player. Splits into 2 smaller asteroids on death. | Phase 7 |
| Asteroid (small) | 1 | 2 u/s | 1 | Straight at player. Dies to 1 shot, no split. | Phase 7 |
| Crab | 3 | 2 u/s | 1 | 3D low-poly crab, straight at player. | Phase 7 |
| Octopus | 6 | 1.2 u/s | 2 | Larger, slower, straight at player. Purple + green. | Phase 7 |
| Space Drone (saucer) | 3 | 3 u/s | 1 | 3D movement. Hovers, fires blockable balls, repositions after dwell, drifts closer each cycle. | Phase 7 |
| Alien Lord (boss) | 70 | — | — | Wave 10. Spawns Crabs/Octopuses/Drones. Has health bar. | Tomorrow |

`Enemy` is abstract. Each type is a subclass.

**Space Drone ball**: projectile fired by Space Drone. 1 HP (one player shot blocks it). 4 u/s speed. 1 damage on hit.

### Scoring

| Event | Points |
|---|---|
| Drone kill | 100 |
| MegaShot kill | 3× base |
| Wave clear bonus | 500 × waveNumber |

### Waves

| Wave | Enemies | Spawn rate |
|---|---|---|
| 1 | 5 Asteroids (big) | 1 / 1.5 s |
| 2 | 8 Asteroids | 1 / 1.2 s |
| 3 | 10 Asteroids + 2 Crabs | 1 / 1.0 s |
| 4 | 8 Asteroids + 4 Crabs | 1 / 1.0 s |
| 5 | 6 Crabs + 2 Octopuses | 1 / 1.0 s |
| 6 | 8 Crabs + 3 Octopuses + 1 Space Drone | 1 / 0.9 s |
| 7 | 4 Octopuses + 3 Space Drones | 1 / 0.9 s |
| 8–9 | heavier mixes | 1 / 0.8 s |
| 10 | **Boss: Alien Lord** (tomorrow) | — |

5-second banner between waves.

### Spawning

Spherical, not ring. Enemies pick a random yaw (0–360°) and random pitch (-30° to +60° to avoid under-foot and directly-overhead), then spawn at `SpawnRingRadius` on that unit direction from player.

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

### Phase 2.5 — Starfield skybox

- `QuickFlick.UI.StarfieldBuilder` — builds a starfield cubemap at runtime, assigns to `RenderSettings.skybox`. Black background, ~800 white pixels scattered across the 6 faces.
- `GameBootstrap` instantiates it on Start.

Editor test: Play. Background is black with stars instead of grey gradient. Camera look still works.

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
- **UI**: Reticle, HP pips, score, wave banner, F3 debug overlay, **off-screen enemy arrows** (chevron at screen edge pointing at each off-camera Drone).
- **PostFx**: `PostFxBuilder` fills Global Volume with Bloom + Vignette + Tonemapping.
- **Audio**: `MusicPlayer` loops the default track, `Sfx` plays cannon-fire / hit / megashot / death.

Editor test: Play. HUD shows. Bloom on cannon shots. Music loops. SFX on fire/hit.

### Phase 7 — Polish + scope expansion

**7a** — Bigger lasers, sphere spawn, MegaShot AOE
**7b** — Nebulae (background haze)
**7c** — Planet (one big orb)
**7d** — Asteroid enemy (replaces Drone, splits on big-variant death)
**7e** — Crab enemy
**7f** — Octopus enemy
**7g** — Space Drone enemy (with blockable balls)
**7h** — Wave composition rewrite per table above
**7i** — Title screen

Each visual (Nebulae, Planet, Asteroid, Crab, Octopus, Space Drone) gets a design-approval gate before building.

## Post-phase work (after Phase 7)

### Additional scenes

Three more scenes planned: **Space Station**, **Mars**, **Asteroid Field**. Each is a new `.unity` under `Assets/Scenes/`, following the same 5-object bootstrap pattern. Scene-specific content (skybox, environment props, lighting) belongs to the scene; gameplay systems (Bootstrap, Input, Combat, Enemies, UI, Audio) are reused unchanged. `GameBootstrap` must be scene-agnostic — no hard-coded references to Main.unity content.

### Graphics approval gate

For any phase that designs a visual object (projectile mesh, enemy mesh, environment prop, post-fx look), the builder stops after *each* object, posts a screenshot or description of the design, and waits for human approval before moving to the next. No batching.

## Serial protocol

TBD when hardware arrives. Contract will live in `Assets/Scripts/Input/CLAUDE.md` and `ControllerProtocolTests.cs`.

## Build target

Editor Play Mode only tonight. Standalone .exe tomorrow.
