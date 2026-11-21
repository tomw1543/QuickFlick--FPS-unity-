# Audio

Namespace: `QuickFlick.Audio`

Music loop + sfx bus.

- `MusicPlayer` — loops the configured track from `Assets/Audio/`. Default: `serhii_kliets-spaceship-arcade-shooter-game-background-soundtrack-318508.mp3`. Track swap is a one-line config change.
- `MusicPlayer` also crossfades (1.5s, unscaled time) to `jjk-divergent-fist` when `GameEvents.WaveStart(3)` fires, and back to the default on `GameEvents.Restart`.
- `Sfx` — `Play(clipKey)`. Clips: `cannon_fire`, `megashot`, `hit`, `enemy_death`, `player_hit`.

**Rule:** no `AudioSource` lookups from gameplay code. Gameplay raises events; Audio subscribes.

**Rule:** if an mp3 loop clicks audibly, convert that track to `.ogg`. Don't fight mp3 seam artefacts.
