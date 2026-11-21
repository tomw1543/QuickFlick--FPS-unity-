# QuickFlick

Unity 6 (URP, C#) stationary FPS space shooter. You are a turret: you don't move, you look and shoot. Survive waves of enemies.

Input devices: **mouse** (works now), **Arduino controller** (planned) and **phone controller** (planned). One device is active at a time; the mouse is always the no-hardware fallback.

## Run

1. Unity Hub → Add → `QuickFlick/`. Use editor **6000.0.27f1**. Don't let Hub upgrade the project to a newer Unity: newer editors bump the package versions, and 6000.0.27f1 then fails with NUnit / TestTools errors.
2. Open `Assets/Scenes/Main.unity`.
3. Press Play.

## Controls (current build)

| Input | Action |
|---|---|
| Mouse | Look |
| LMB / RMB | Fire left / right cannon. Both within 150 ms = MegaShot. |
| Space | Restart after game over |
| P | Pause / resume the wave spawner (debug) |
| F1–F5 | Debug-spawn Crab / Octopus / Space Drone / Asteroid / Mario |

Click the Game view first so it receives key presses.

## What's in the game

- Pooled projectiles, enemy waves, HP pips, score, wave banner, off-screen enemy arrows, reticle.
- Enemies: Asteroid (splits), Crab, Octopus (Barbara the Bee, spirals in), Mario (`LevelApproachEnemy`: walks in level with the camera, faces you). Space Drone is not built yet.
- Music crossfades to a second track when wave 3 starts.
- See `HANDOFF.md` for the latest session notes and known issues.

## Plan changes (not built yet)

These supersede parts of `SPEC.md`, which still describes the older plan.

### One cannon

The player has **one cannon**, not two. The two-cannon design and MegaShot (both cannons within 150 ms) are dropped. `SPEC.md`, `CONTEXT.md` and the combat code still assume two cannons until this lands.

### One input device at a time

Only one device drives the game at once: mouse, Arduino controller or phone. They are not merged. Every device implements the same `IControllerInput`, so gameplay code never knows which one is active.

Switch the active device with a key (number row):

| Key | Device |
|---|---|
| 7 | Mouse (default) |
| 8 | Arduino controller |
| 9 | Phone |

### Shared processing in Unity

Devices send **raw readings**. Unity does all the processing (deadzones, turn-speed mapping, fire thresholds) in the same place for every device, with tunables in `Settings`. The phone page and the Arduino firmware stay simple sensors and senders.

```
device ──raw readings──▶ background thread ──latest value──▶ main thread: processing ──▶ IControllerInput
(Arduino via USB serial, phone via websocket)                 (deadzone, velocity, fire threshold)
```

### Aiming: velocity control

Aim is **rate-based**, like a joystick, on **both axes**:

- **Left / right:** point the device a few degrees (e.g. 3°) left of neutral and the camera keeps turning left until you return to neutral. Same for right.
- **Up / down:** point the device a few degrees up and the camera keeps looking further up until you return to neutral. Same for down.

A bigger angle turns faster, up to a cap. A small deadzone around neutral stops drift. A recenter action resets neutral to however the player is holding the device.

### Firing: push motion (phone)

A **push** of the phone (a short forward thrust, measured by the accelerometer) fires the cannon when the acceleration crosses a threshold. A short cooldown stops one push firing twice. Turning is briefly ignored during a push so the shot doesn't throw your aim.

### Phone controller

Full spec: **`PHONE-CONTROLLER-PLAN.md`**.

- Players scan a QR code (a PNG the game writes to `QuickFlick/Tools/join-qr.png`, not shown in-game) with their own iPhone, tap Start and allow motion access. No app install. The phone fires the **right cannon** only.
- The page streams raw orientation (gyroscope) and acceleration readings over a **websocket**, continuously (60 Hz, 2 samples per message).
- iPhone Safari only gives motion data to **HTTPS** pages, and an HTTPS page can only open a secure websocket (`wss://`). **Decision: Cloudflare Worker relay** (free plan, fixed `*.workers.dev` URL, Tom's account). The Worker serves the page and relays websockets. The phone and the game both connect **out** to it, so the laptop runs no server, needs no tunnel, and the phone just needs any internet. Each laptop has its own **room** (`PHONE_ROOM` in the gitignored `.env`), so laptops never cross-talk. Rejected: a self-signed certificate (warning on every phone, same-Wi-Fi only) and a Cloudflare quick tunnel (URL changes every run).
- Network reading happens on a background thread only and never calls Unity APIs (see `CLAUDE.md`). All processing (deadzone, velocity, push threshold) is in Unity.

### Arduino controller

Out of scope for the phone work: the Arduino firmware and its serial format are not changed by this plan.

### Open questions

- Exact tuning: deadzone, full-speed angle, push threshold and cooldown.

## Where things are

| Path | What |
|---|---|
| `SPEC.md` | Original scope, phases, enemy/scoring/wave data (see Plan changes above) |
| `CONTEXT.md` | Glossary |
| `CLAUDE.md` | Agent workflow and project rules |
| `HANDOFF.md` | Session notes, newest first |
| `PHONE-CONTROLLER-PLAN.md` | Phone controller spec + relay contract |
| `relay/` | Cloudflare Worker relay (planned, Tom) |
| `ENEMY-COOKBOOK.md` | How to add an enemy (one file + one model) |
| `FBX-IMPORT.md` | Loading converted models without the white-material bug |
| `QuickFlick/Assets/Scripts/<area>/` | Code, one `CLAUDE.md` per area |
| `scripts/glb_to_fbx.py` | Blender .glb → .fbx converter |
| `.claude/agents/` | `sonnet-builder`, `opus-reviewer` |
