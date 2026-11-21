# ENEMY-COOKBOOK.md — adding an enemy without breaking anything

For teammates picking up Crab, Octopus, Space Drone, or any future enemy.

The rule: **your work is one new file + one FBX model.** You never edit `GameBootstrap`, `Spawner`, `WaveManager`, `EnemyPool`, `Enemy.cs`, or any other enemy's file. If you think you need to, flag it — the infra probably has the hook you want.

Read `FBX-IMPORT.md` first (the model-loading contract) and `SPEC.md` (the enemy stats table).

---

## What's shipped for you

- `Enemy` (abstract base) in `Scripts/Enemies/Enemy.cs` — HP, pool wiring, distance-despawn at 80u, `OnDeath` event, `OnBeforeDespawn` hook.
- `EnemyPool.GetByType(Type)` — spawns instances of any enemy type reflectively (used by the debug spawner).
- `FixImportedMaterials` — static on `GameBootstrap`, callable from anywhere. See `FBX-IMPORT.md`.
- `DebugSpawner` — F1–F4 keys to force-spawn one of each enemy during Play (see Testing below).

## The enemy spec (from `SPEC.md`)

| Enemy | HP | Speed (u/s) | Damage | Scale | Collider radius (local) | FBX model | Fallback colour |
|---|---|---|---|---|---|---|---|
| Crab | 3 | 2.0 | 1 | **1.0×** | **1.0** | `Enemy Large by Quaternius - UgBmRVnQ9h` *(or Robot Enemy — pick one)* | `(0.55, 0.20, 0.15)` dark red |
| Octopus | 6 | 1.2 | 2 | **1.8×** | **1.3** | `Alien by Quaternius - sUTLXji0aL` | `(0.55, 0.25, 0.60)` purple-green mix |
| Space Drone | 3 | 3.0 | 1 | **1.2×** | **1.0** | `Satellite by Poly by Google - fmYHY1leH_P` | `(0.55, 0.60, 0.65)` metallic grey |
| Asteroid (big) | 1 | 2.0 | 1 | 1.5× | 1.5 | `Asteroid by J-Toastie - YS1jpm3mNr` *(done)* | `(0.45, 0.42, 0.38)` |
| Asteroid (small) | 1 | 2.0 | 1 | 0.45× | 1.0 | same | same |

**These are the only tunable constants.** Put them as `const` at the top of your file. Do not change the scale or radius without saying so — balance depends on it.

---

## The 7-step cookbook

Example target: Crab.

### 1. Get the FBX

The `.glb` lives at `QuickFlick/Assets/Resources/SkyBox/<model name>.glb`. Run the Blender converter once to produce the `.fbx`:

```
"C:\Program Files\Blender Foundation\Blender 5.3\blender.exe" --background --python scripts/glb_to_fbx.py
```

Converted `.fbx` lands in `Resources/SkyBox/Converted/`. Don't hand-edit it in Unity.

### 2. Create one file

`QuickFlick/Assets/Scripts/Enemies/Crab.cs`:

```csharp
using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Enemies
{
    public class Crab : Enemy
    {
        // SPEC values: 3 HP, 2 u/s, 1 dmg, scale 1.0, collider 1.0
        const float Speed = 2f;
        const int Damage = 1;
        const int MaxHp = 3;
        const float Scale = 1.0f;
        const float ColliderRadius = 1.0f;
        const float ReachDistance = 1.5f;
        const string ModelPath = "SkyBox/Converted/Enemy Large by Quaternius - UgBmRVnQ9h";
        static readonly Color CrabColor = new Color(0.55f, 0.20f, 0.15f);

        Transform player;
        bool visualReady;

        // Pool entry point.
        public override void Reset(Vector3 position)
        {
            hp = MaxHp;
            player = UnityEngine.Camera.main.transform;
            transform.position = position;
            transform.localScale = Vector3.one * Scale;
            var sc = GetComponent<SphereCollider>();
            if (sc != null) sc.radius = ColliderRadius;
            AttachVisual();
            gameObject.SetActive(true);
        }

        public override void Tick(float dt)
        {
            if (player == null) return;
            var dir = (player.position - transform.position).normalized;
            transform.position += dir * (Speed * dt);
            if (Vector3.Distance(transform.position, player.position) < ReachDistance)
            {
                if (PlayerHp.Instance != null) PlayerHp.Instance.TakeDamage(Damage);
                Die();
            }
        }

        void AttachVisual()
        {
            if (visualReady) return;
            visualReady = true;
            var mr = GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false; // hide pool placeholder tetrahedron
            var prefab = Resources.Load<GameObject>(ModelPath);
            if (prefab == null) { Debug.LogWarning("Crab: FBX not found, keeping placeholder"); if (mr != null) mr.enabled = true; return; }
            var go = Instantiate(prefab, transform);
            go.transform.localPosition = Vector3.zero;
            foreach (var c in go.GetComponentsInChildren<Collider>()) Destroy(c);
            QuickFlick.Bootstrap.GameBootstrap.FixImportedMaterials(go, CrabColor);
        }
    }
}
```

That's a full working enemy. Copy, rename, swap the five constants + FBX path + colour. Done.

### 3. Play test

- Press Play.
- Press **F1** (or your key — see below). One Crab spawns at the ring.
- Shoot it. It should take 3 shots to die (3 HP × 1 damage per shot). Reaching you should take 1 HP off your pips.

### 4. Tune if it feels wrong

Only the five consts. Don't touch anything else. If visual scale is off, bump `Scale`. If hit sphere feels too generous/tight, bump `ColliderRadius`.

### 5. Commit

```bash
git add QuickFlick/Assets/Scripts/Enemies/Crab.cs QuickFlick/Assets/Scripts/Enemies/Crab.cs.meta
git commit -m "feat(enemies): Crab"
git push
```

**Commit when it works.** Project rule is commit per verified change, no batching.

### 6. (Optional) Override for special behaviour

See the "Advanced" section below if your enemy fires, splits, or has on-death VFX.

### 7. Leave the integration to someone else

Don't touch `Spawner.cs` to put your enemy into the wave rotation. The wave-table rewrite (Phase 7h) is a separate ticket.

---

## Testing: F1–F4 debug spawn

`DebugSpawner` auto-installs in Play mode. Key bindings:

- **F1** → Crab
- **F2** → Octopus
- **F3** → SpaceDrone
- **F4** → Asteroid (big)
- **P** → pause / resume the auto wave spawner

Each press spawns one enemy at a random point on the 30u ring. If your class doesn't exist yet (`Type.GetType` returns null), the Console logs `[DebugSpawn] Crab not implemented yet` — you'll see that until you drop your file in.

**Press P first, before F1-F4, to stop the wave system spawning asteroids over your test.** Pause kicks in at the next wave boundary, so any mid-wave asteroids finish arriving first. Press P again to resume waves.

**Click inside the Game view** before pressing keys — Unity only captures keyboard input in the focused panel.

---

## Advanced

### Splitting on death (asteroid pattern)

Override `Die()` and spawn children via the pool **before** calling `base.Die()`:

```csharp
protected override void Die()
{
    if (IsBig)
    {
        for (int s = -1; s <= 1; s += 2)
            EnemyPool.Instance.Get<Crab>().Init(false, transform.position, ...);
    }
    base.Die();
}
```

(See `Asteroid.cs` for the full working pattern.)

### Firing projectiles (Space Drone's blockable balls)

Space Drone fires a projectile that player bullets can intercept. For now:

1. In your enemy's `Tick`, track a cooldown; when ready, call `Instantiate` on a simple sphere with a `Projectile`-like script that:
   - Moves toward the player's current position (sampled at fire time, not per-frame).
   - Deals 1 damage on hit via `IDamageable`.
   - Has `1 HP` — a player projectile should destroy it on hit.
2. Keep this inside your enemy's file for now; don't touch `Projectile.cs` (that's the player's projectile).
3. If this feels like it needs a shared `EnemyProjectile` class, flag it — the infra should probably add one.

### On-death VFX

Override the hook in your enemy:

```csharp
protected override void OnBeforeDespawn()
{
    // Called both on combat death (Die) and on far-despawn. Keep cheap.
    // Example: emit a particle burst, scale a quick flash, play a sound.
}
```

Pool-friendly rules:
- Do NOT `Destroy` the enemy here — the pool needs to recycle it.
- Do NOT start a long coroutine on the enemy itself — it's about to deactivate. Spawn a separate short-lived GO if you need one.

---

## Rules of engagement

- **One file + one FBX.** Nothing else.
- **No `FindObjectOfType`** — use events and `Instance` singletons where they exist (`PlayerHp.Instance`, `EnemyPool.Instance`).
- **Trust the base class.** HP decrement, pool return, 80u despawn all happen for free. You override `Reset` (pool entry), `Tick` (per-frame movement), and optionally `Die` / `OnBeforeDespawn`.
- **FBX material setup:** always call `GameBootstrap.FixImportedMaterials(go, fallback)` after `Instantiate`. See `FBX-IMPORT.md`.
- **Pool placeholder:** every pooled enemy has a tetrahedron `MeshRenderer` by default. Disable it in `AttachVisual` once the FBX loads (see Crab example).
- **Scale + collider are in the table above.** Don't invent your own without discussing — it affects whether the player can hit you and whether you fit visually.
- **Don't push broken code.** Press Play, confirm it works with your F-key, then commit.
- **`Input.` namespace gotcha.** The project has a `QuickFlick.Input` namespace, which shadows `UnityEngine.Input`. If you need keyboard/mouse input in your enemy (you probably don't — enemies move by `Tick`), fully qualify as `UnityEngine.Input.GetKeyDown(...)`, never bare `Input.GetKeyDown`.

## Before you commit — checklist

- [ ] One new file under `Scripts/Enemies/` and optionally one new `.glb` under `Resources/SkyBox/`.
- [ ] Converted FBX exists under `Resources/SkyBox/Converted/`.
- [ ] F-key spawn works in Play. Enemy takes correct hits, deals correct damage.
- [ ] No other file in `git status` (apart from `.meta` files Unity generated).
- [ ] Console shows no red errors or `NullReferenceException` when your enemy spawns/dies/despawns.
- [ ] Committed with `feat(enemies): <name>` and pushed.

---

## When the infra isn't enough — raise your hand

If the base class or pool genuinely can't do what you need, don't bend the rules — tell someone. The main-session Claude will extend the infra once (behind a stable API) and update this doc. Adding bespoke hacks in your file to get around infra limits is how we end up with today's cascade bugs.
