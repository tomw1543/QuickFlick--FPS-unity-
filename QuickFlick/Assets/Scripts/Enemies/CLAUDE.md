# Enemies

Namespace: `QuickFlick.Enemies`

Enemy base + concrete types.

- `Enemy` — abstract. `int Hp`, `event Action<Enemy> OnDeath`, `abstract void Tick(float dt)`. `TakeDamage(int)` decrements HP, raises `OnDeath` at ≤0, returns to pool.
- `Drone` (tonight) — 1 HP, 8 u/s, flies straight at player.
- `Weaver`, `Brute` (later) — see `SPEC.md` enemy table.

**Rule:** every new enemy type is a subclass of `Enemy`. No `if (type == …)` branches in gameplay code.

**Rule:** enemies are pooled.

**For new enemies, see `ENEMY-COOKBOOK.md` at the repo root.** That doc has the stats table (HP/speed/damage/scale/collider/FBX/colour per enemy), a copy-paste `Crab.cs` template, and the F1–F4 debug-spawn flow. Teammates must only add their own enemy file + FBX — no edits to `Enemy.cs`, `EnemyPool.cs`, `Spawner.cs`, or other enemies.

Current hooks on `Enemy` base:
- `Reset(Vector3 position)` — pool entry. Override to set hp, position, scale, collider radius, attach FBX visual.
- `Tick(float dt)` — per-frame movement / behaviour.
- `Die()` — combat death; overrideable (asteroid splits here).
- `Despawn()` — far-distance despawn (>80u, see `Settings.EnemyDespawnDistance`); no event, no split.
- `OnBeforeDespawn()` — called before BOTH `Die` and `Despawn`. Override for on-death VFX / SFX / loot.
