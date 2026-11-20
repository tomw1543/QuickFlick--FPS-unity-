# Enemies

Namespace: `QuickFlick.Enemies`

Enemy base + concrete types.

- `Enemy` — abstract. `int Hp`, `event Action<Enemy> OnDeath`, `abstract void Tick(float dt)`. `TakeDamage(int)` decrements HP, raises `OnDeath` at ≤0, returns to pool.
- `Drone` (tonight) — 1 HP, 8 u/s, flies straight at player.
- `Weaver`, `Brute` (later) — see `SPEC.md` enemy table.

**Rule:** every new enemy type is a subclass of `Enemy`. No `if (type == …)` branches in gameplay code.

**Rule:** enemies are pooled.
