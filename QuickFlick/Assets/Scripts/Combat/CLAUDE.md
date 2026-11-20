# Combat

Namespace: `QuickFlick.Combat`

Firing, projectiles, MegaShot detection.

- `Cannon` — one per side (Left, Right). Cooldown 0.15 s. Fires on rising edge of its Fire bool.
- `Projectile` — pooled. Speed, lifetime, damage. On-hit → `Enemy.TakeDamage`.
- `MegaShotDetector` — watches both cannons. If both fire within 0.15 s, next shot carries 3× damage and raises the `MegaShot` event (for SFX / visual).

**Rule:** projectiles are pooled. Never `Instantiate`/`Destroy` in a hot path.
