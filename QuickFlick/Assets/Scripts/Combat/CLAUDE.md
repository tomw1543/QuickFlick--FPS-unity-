# Combat

Namespace: `QuickFlick.Combat`

Firing, projectiles, MegaShot detection.

- `Cannon` — one per side (Left, Right). Cooldown 0.15 s. Fires on rising edge of its Fire bool.
- `Projectile` — pooled. Speed, lifetime, damage. On-hit → `Enemy.TakeDamage`.
- `MegaShotDetector` — watches both cannons. If both fire within 0.15 s, suppresses the individual cannon shots and fires an AOE that instant-kills every `Enemy` in the camera cone within `Settings.MegaShotRange` (15 u). Raises `ShotFired(true)` for SFX and triggers `HudRoot.Flash` for a brief screen flash.

**Rule:** projectiles are pooled. Never `Instantiate`/`Destroy` in a hot path.
