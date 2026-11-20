# Systems

Namespace: `QuickFlick.Systems`

Game-level state that isn't one entity: waves, scoring, HP, game state transitions, settings, spawning.

- `Settings` — static constants (HP, cooldowns, speeds, wave table).
- `Spawner` — spawns enemies at a ring around player.
- `WaveManager` — drives Spawner per the wave table. Raises `WaveStart`/`WaveClear`.
- `Score` — adds points on kill, applies 3× on MegaShot kill, adds wave-clear bonus.
- `PlayerHp` — 5 HP. Decrements on enemy reach. At 0 → GameState.GameOver.
- `GameState` — Boot → Playing → GameOver. Space restarts.

**Rule:** systems talk via events, not direct refs. GameBootstrap wires subscriptions.
