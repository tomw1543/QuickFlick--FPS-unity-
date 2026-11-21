# PHONE-CONTROLLER-PLAN.md — phone as a wireless controller

**Tier 3** (network threading). Status: **v3.1 — decisions final (grilled 2026-11-21), aligned with Tom's `SerialInput` (`10eb082`), awaiting build**. Branch: `georgia/new-feature`.

**Ownership:** **Tom** builds + deploys the Cloudflare relay (he has the Cloudflare account). **Georgia** builds the Unity side + the phone page. Section 4 is the contract between them — change it only by agreement.

---

## 1. What the player does

1. Laptop: start the game. It connects out to the Cloudflare relay automatically and writes `QuickFlick/Tools/join-qr.png` (opens it). The URL is fixed per laptop, so the QR can be printed once.
2. Player scans the QR with their iPhone camera → Safari opens the controller page → taps **Start** → allows Motion access.
3. Laptop: press **9** to switch the game to the phone.
4. Hold the phone **like a TV remote**: screen up, top edge pointing at the laptop screen.
5. **Aim (velocity):** point a few degrees left/right/up/down of neutral → camera keeps turning that way until you return to neutral. Bigger angle = faster (linear), capped. Up = look up (not inverted).
6. **Fire:** a sharp **push** of the phone towards the screen fires the **right cannon** only. No MegaShot from the phone.
7. **Recenter:** button on the phone page. Switching to 9 does **not** auto-recenter.

**Input switching: 7 = mouse, 8 = Arduino controller (Tom's `SerialInput`), 9 = phone.** Startup default stays as Tom wired it in `GameBootstrap` (`10eb082`): **Arduino if its serial port opens, otherwise mouse**. If a switch fails (no phone connected, serial port won't open), stay on the current input and show a brief on-screen message (~2 s). If the phone drops while active, stay on phone (neutral) while it reconnects; keys still switch.

## 2. Architecture

```
iPhone Safari ── https://<worker>.<subdomain>.workers.dev/?room=<room>
  controller.html: Start/permissions, deviceorientation + devicemotion @ ~60 Hz,
  2 samples per message (~30 msg/s), Recenter, status, "another phone took over"
        │ wss://…/ws?room=<room>&role=phone
        ▼
CLOUDFLARE (free plan) — Tom
  Worker: GET / → page; GET /ws → room's Durable Object
  Durable Object "Room" (one per room): ≤1 phone + ≤1 game socket, forwards phone→game verbatim,
  latest phone/game takes over, sends #phone-connected / #phone-left to the game. Stores nothing.
        ▲ wss://…/ws?room=<room>&role=game   (outbound from laptop: no firewall prompt, no same-Wi-Fi)
        │
LAPTOP (Unity) — Georgia
  .env (gitignored): PHONE_RELAY_URL, PHONE_ROOM
  BACKGROUND THREAD (no Unity API): RelayClient (ClientWebSocket) → connect at startup, retry with backoff
        → bounded ConcurrentQueue<string>
  MAIN THREAD, every frame: PhoneInput : IControllerInput
        drain ALL messages → split samples → PhoneProtocol → PhoneAimMapper (velocity) + PushDetector (fire)
        0.5 s silence → neutral
  InputSelector (7/8/9) → GameBootstrap → StickCombiner/TurretCamera, right Cannon
  JoinQrWriter → Tools/join-qr.png   ·   PhoneDebugPanel (F6, hidden by default)
```

**Processing lives in Unity**, not the phone and not the relay. The relay never parses. Tuning never requires a redeploy.

**Every sample is processed**, not just the latest: a push spike lasts ~50–100 ms, so the main thread drains the whole queue and feeds every sample to `PushDetector`.

## 3. Cloudflare free plan (verified 2026-11-21)

- Workers + Durable Objects are on the free plan; Durable Objects must use the **SQLite** backend (`new_sqlite_classes`).
- 100,000 requests/day; **incoming** websocket messages count 20:1; outgoing are free. 13,000 GB-s/day duration.
- Phone at 30 msg/s ≈ 1.5 requests/s ≈ **~18 h of continuous play per day**. Over the limit → errors until 00:00 UTC (11:00 Sydney). No charges.
- Sources: developers.cloudflare.com/durable-objects/platform/pricing/ and /limits/.

## 4. Contract (relay ↔ phone page ↔ Unity)

### URLs

| URL | Who | What |
|---|---|---|
| `GET /` (and `/?room=…`) | phone | serves `relay/public/controller.html` (static asset) |
| `GET /ws?room=<room>&role=phone` | phone page | websocket |
| `GET /ws?room=<room>&role=game` | Unity | websocket |

- `room` must match `^[a-z0-9-]{3,40}$`, else HTTP 400. Room names are a name + random suffix (e.g. `tom-7f3kq`), set per laptop in `.env`.
- `role` must be `phone` or `game`, else HTTP 400.

### Relay behaviour (Room Durable Object, `idFromName(room)`)

- Holds at most **one phone** and **one game** socket.
- **New phone joins** → old phone gets text `kicked`, then close code `4000`.
- **New game joins** → old game socket closed with `4001` (e.g. Unity re-entered Play).
- **Phone → game:** forward every text frame **unchanged**. Drop frames > 1 KB. If no game is connected, drop silently.
- **Relay → game control messages** (start with `#`, data never does): `#phone-connected` when a phone joins (also sent to a newly joined game if a phone is already there), `#phone-left` when the phone socket closes.
- **Game → phone:** nothing (reserved).
- Use the websocket Hibernation API (`ctx.acceptWebSocket`) if convenient. No storage.

### Data message (phone → game)

One websocket text frame = **2 samples** separated by `;` (≈30 frames/s). Each sample is CSV with `.` decimals:

```
seq,alpha,beta,gamma,ax,ay,az,recenter;seq,alpha,beta,gamma,ax,ay,az,recenter
```

| Field | Meaning |
|---|---|
| `seq` | int, +1 per sample |
| `alpha` | yaw, degrees 0–360 (DeviceOrientationEvent) |
| `beta` | pitch, degrees −180–180 |
| `gamma` | roll, degrees −90–90 (debug only) |
| `ax,ay,az` | linear acceleration without gravity, m/s², device axes (DeviceMotionEvent.acceleration) |
| `recenter` | int counter, +1 per Recenter tap |

## 5. Unity processing (tunables in `Settings`)

| Setting | Default | Meaning |
|---|---|---|
| `PhoneTimeoutSeconds` | 0.5 | silence → neutral; also "no phone" for the 9 switch |
| `PhoneDeadzoneDeg` | 1.5 | no turning inside this angle |
| `PhoneFullSpeedDeg` | 20 | angle (beyond deadzone) for full turn speed |
| `PhonePushThreshold` | 15 | m/s² along device +Y (top edge, towards screen) |
| `PhonePushRelease` | 5 | must drop below to re-arm (hysteresis) |
| `PhonePushCooldownSeconds` | 0.25 | min time between shots |
| `PhoneAimFreezeSeconds` | 0.15 | sticks held at 0 after a push |
| `PhoneReconnectMaxSeconds` | 10 | backoff cap for RelayClient |
| `InputSwitchMessageSeconds` | 2 | switch-message display time |

- Yaw stick = mapped `wrap(alpha − alpha0)` (±180, sign flipped so left = left). Pitch stick = mapped `beta − beta0`. Both sticks get the same value.
- Mapping is linear and starts **just above `StickCombiner`'s 0.15 deadzone** so small angles (3°) still turn.
- Push → `RightFire = true` for a **0.1 s pulse** (same as `SerialInput.firePulseSeconds`; Cannon fires on the rising edge). `LeftFire` always false.
- **Mirrors the Arduino controller** (`SerialInput`, `10eb082`): background thread → `ConcurrentQueue` → main thread drains every sample → deadzone + linear shaping into both sticks → spike threshold + cooldown → timed right-cannon pulse. Differences: the phone thresholds on linear acceleration along the top edge (gravity already removed by iOS) with hysteresis; the Arduino thresholds on the change in raw X g between samples.
- Neutral = the first sample ever, the first sample after `#phone-connected`, and on every `recenter` change. **Not** after a silence: a >0.5 s gap only zeroes the sticks (a main-thread stall or alt-tab must not silently re-zero aim; a locked phone drops its socket, so it comes back via `#phone-connected` anyway).
- **Replaced (4001):** if another game takes this room, the client stops reconnecting and shows "Replaced by another game in this room"; pressing 9 restarts it. Prevents two games kicking each other every 0.5 s.

## 6. Work split and files

### Tom — Cloudflare relay (`relay/`)

| File | Content |
|---|---|
| `relay/wrangler.jsonc` | name, `main: src/index.ts`, `assets: { directory: "public" }`, DO binding `ROOM` → class `Room`, migration `new_sqlite_classes: ["Room"]` |
| `relay/package.json` | `wrangler` dev dependency |
| `relay/src/index.ts` | Worker routing + `Room` Durable Object per section 4 (~100 lines) |
| `.gitignore` | add `relay/node_modules/`, `relay/.wrangler/` (don't touch the env/secrets section) |

Deploy: `cd relay` → `npm install` → `npx wrangler login` → `npx wrangler deploy`. Then share the `https://….workers.dev` URL. Local test: `npx wrangler dev` (serves on `http://localhost:8787`).

### Georgia — Unity side + phone page

S1 (pure + tests) and S2 (network + page) in parallel, then S3 (integration). Then Opus review.

| Stream | Files |
|---|---|
| **S1** pure, no `UnityEngine` | `Input/PhoneReading.cs`, `Input/PhoneProtocol.cs` (split `;`, parse CSV invariant culture, ignore `#…` control lines → separate result), `Input/PhoneAimMapper.cs`, `Input/PushDetector.cs`; tests in `Assets/Tests/EditMode/` (+ `QuickFlick.Tests.EditMode.asmdef`) |
| **S2** network + page | `Net/RelayClient.cs` (ClientWebSocket on a background thread, connect at startup, backoff, bounded queue drop-oldest ~512, clean stop on quit + `beforeAssemblyReload`), `Net/EnvConfig.cs` (reads only `PHONE_RELAY_URL`, `PHONE_ROOM` from repo-root `.env`; never logs other keys), `Net/CLAUDE.md`, `relay/public/controller.html` |
| **S3** integration | `Input/PhoneInput.cs`, `Input/InputSelector.cs` (7/8/9; owns Mouse/Serial/Phone components; startup default = Serial if `IsOpen` else Mouse, exactly as Tom's bootstrap does today; 8 = enable `SerialInput`, check `IsOpen`, else message + stay; inactive devices disabled — `SerialInput` closes its port in `OnDisable`, reopens in `OnEnable`), `UI/InputMessage.cs`, `UI/PhoneDebugPanel.cs` (F6), `Net/JoinQrWriter.cs` + vendored `Assets/Scripts/ThirdParty/QrCodeGenerator/` (Net.Codecrete.QrCodeGenerator **v2.1.0**, MIT, 9 files + LICENSE; inside `QuickFlick.asmdef` because asmdef code can't reference the `Plugins/` firstpass assembly), **replace the serial/mouse selection block** in `Bootstrap/GameBootstrap.cs` (lines ~52–64) with `input = gameObject.AddComponent<InputSelector>();`, **append** to `Systems/Settings.cs`, `.gitignore` add `QuickFlick/Tools/`, docs |

**Nobody touches:** `MouseInput.cs`, `SerialInput.cs`, `SerialProtocol.cs`, `Firmware/`, `StickCombiner.cs`, `TurretCamera.cs`, `Cannon.cs`, `MegaShotDetector.cs`, `DebugOverlay.cs`, enemies, `Main.unity`, `.meta`/prefab/asset files.

## 7. Acceptance criteria

1. **Mouse unaffected:** no `.env` / no internet / relay down → game starts normally, one clear warning, mouse works, no frame stalls.
2. **Switching:** startup default unchanged (Arduino if connected, else mouse); 7/8/9 work; 9 with no phone → stay + "No phone connected" (or "Phone not configured (.env)" when `.env` lacks the PHONE_* keys); 8 with no Arduino → stay + "Arduino controller not available"; with the Arduino plugged in, 8 → 7 → 8 reopens the port cleanly.
3. **QR:** `QuickFlick/Tools/join-qr.png` written at every startup, opened in the image viewer only when the URL changed (first run / new room or relay); scanning opens the page for this laptop's room.
4. **Phone flow:** scan → Start → allow → page "connected" → press 9 → aim + fire per section 1.
5. **Velocity aim:** ~3° turns slowly and continuously; ~20° full speed; neutral stops; both axes.
6. **Push fire:** a deliberate push fires the right cannon exactly once; still/slow moves never fire; aim doesn't jump.
7. **Disconnect:** lock the phone → camera neutral within 0.5 s; reconnects on unlock.
8. **Takeover:** second phone in the same room takes over; first page shows "another phone took over". Different rooms never interfere.
9. **Clean lifecycle:** enter/exit Play 5× → no thread exceptions, no stuck sockets.
10. **Tests:** EditMode tests pass: garbage, comma decimals rejected, 2-sample split, `#` control lines, angle wrap 359°→1°, push hysteresis/cooldown.
11. **Builds:** headless compile clean.
12. **Opus review PASS** (threading, shutdown, bounded queue, malformed input) incl. a throwaway smoke test through `wrangler dev` (not committed).
13. **Free-plan sanity:** after 10 min of play, Cloudflare dashboard request count matches ~1.5 req/s.

## 8. Setup

**Tom (once):** Cloudflare account (free) → `relay/` deploy (section 6) → choose rooms → share URL.
**Each laptop:** add to repo-root `.env`: `PHONE_RELAY_URL=https://….workers.dev` and `PHONE_ROOM=<name>-<random>`. Unity 6000.0.27f1. Internet.
**Players:** iPhone Safari, any internet (mobile data fine).

## 9. Risks

| Risk | Mitigation |
|---|---|
| Gimbal lock when the phone is upright | Remote-style grip (hint on the page) |
| iOS `alpha` drifts slowly | Deadzone + Recenter |
| Venue internet down | Phones can use mobile data; laptop can use a hotspot; mouse always works |
| Free-plan daily cap | Batching (~18 h/day); resets 11:00 Sydney |
| Anyone with the QR can steer | Random room suffix; accepted for a demo |
| Push also rotates the phone | `PhoneAimFreezeSeconds` |
| Contract drift between relay, page and Unity | Section 4 is the single source; change only together |

## 10. Out of scope

Arduino firmware/serial format (unchanged). One-cannon gameplay change. Custom domain. Android-specific tuning. In-game QR.
