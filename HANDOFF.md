# HANDOFF — QuickFlick

## Session 2026-11-21 afternoon (Georgia): phone controller plan — Tom, action needed

**Read `PHONE-CONTROLLER-PLAN.md`** (v3, decisions final). Summary for Tom:

**What:** any iPhone becomes a wireless controller. Scan a QR → Safari controller page → tilt to aim (velocity, both axes), push the phone towards the screen to fire the **right cannon**. Keys **7 / 8 / 9** switch the game between mouse / Arduino / phone (one device at a time; failed switch = stay + brief message). Arduino firmware and serial format are **not** changed.

**Why Cloudflare:** iPhone Safari only gives motion data to HTTPS pages. A Cloudflare Worker gives a free, fixed HTTPS URL, serves the page, and relays websockets. Both the phone and the game connect **out** to it, so no laptop server, no tunnel, no firewall prompt, no same-Wi-Fi requirement. Free plan is enough (~18 h of play per day; resets 11:00 Sydney).

**Tom's part (you have the Cloudflare account):**
1. Build `relay/` in this repo: `wrangler.jsonc`, `package.json`, `src/index.ts` (Worker + `Room` Durable Object, ~100 lines). Spec: plan **section 4 (contract)** and **section 6**. The relay only forwards text; it never parses.
2. Free plan needs the SQLite Durable Object type: migration `new_sqlite_classes: ["Room"]`.
3. Add `relay/node_modules/` and `relay/.wrangler/` to `.gitignore` (outside the env/secrets section).
4. Deploy: `cd relay` → `npm install` → `npx wrangler login` → `npx wrangler deploy`. Share the `https://….workers.dev` URL with Georgia.
5. Pick a room per laptop (name + random suffix, e.g. `tom-7f3kq`) and add to your repo-root `.env`: `PHONE_RELAY_URL=…` and `PHONE_ROOM=…`.

**Georgia's part (in progress on `georgia/new-feature`):** all Unity code (RelayClient on a background thread, PhoneInput processing, InputSelector 7/8/9, F6 phone debug panel, QR PNG writer) **and** `relay/public/controller.html` (the page your Worker serves as a static asset). It lands as a **PR** because it touches `GameBootstrap.cs` (one line), `Settings.cs` (appended constants) and `.gitignore`.

**Contract rule:** section 4 (URLs, roles, `kicked`, `#phone-connected` / `#phone-left`, the 2-samples-per-frame CSV) is shared by your relay, the page and Unity. Change it only together.

**Heads-up (existing, not fixed):** F3 toggles `DebugOverlay` **and** spawns a Space Drone in `DebugSpawner`.

---

## Session 2026-11-21 midday (enemy variety pass)

Shipped: Octopus, Mario polish, Crab pivot fix. All on `main`, mouse-only flow still green.

- **Crab** (`57f6fcc`): bounds-centroid re-center so the 3-axis tumble pivots on the mesh centre, not the FBX origin.
- **Octopus** = Barbara the Bee (`65f9b33`): Y-billboard facing player, spirals inward on the player's Y plane. r=25 start, 2.0 u/s radial (~12s close), 6.0 u/s tangential. Scale 0.7, collider 2.2 (lasers were clipping through at 1.3). 6-colour yellow/purple palette cycled per material slot.
- **Mario** (`6c74883`): 6-colour palette (red/blue/peach/yellow/brown/white) via new `LevelApproachEnemy.Palette` hook (default null → old solid-Colour behaviour; only Mario overrides). Loops `freesound_community-mariomp3-14599.mp3` via a local AudioSource on `OnEnable`, stops on `OnDisable` (pool deactivation covers death). Speed doubled to 3.0–8.0 u/s.
- **Debug** (earlier same session): `DebugSpawner` `DontDestroyOnLoad` so F1–F5 survive bootstrap's scene cleanup. `P` toggles the wave spawner for isolated enemy testing (takes effect at next wave boundary).
- **Audio** (Georgia, `30fd3b3`): crossfade to a new music track on wave 3.
- **Materials** (`87e725d`): `FixImportedMaterials` gained optional `emissionStrength` param (default 0.4 for enemies). Planet call passes 0.1 so it doesn't dominate the skybox.

### Risks to watch

- **Multiple Marios = overlapping mario loops.** Each one owns its own AudioSource. Cap count or mute later spawners if it gets ugly.
- **Octopus radial sign is critical.** Had a bug where `-n` pushed outward; now `+n`. If someone re-reads the code: `n` points at the player.
- **Mario palette slot order is FBX-dependent.** If hat comes out blue, re-order `MarioPalette` in `Mario.cs`.
- **Octopus at 6 u/s tangential + 2.2u collider** can streak past the reticle. Dial back if hit rate feels unfair.
- **`.meta` for the new mario mp3** was committed with the mp3. If LFS is ever added, re-check that pair.

### Open from yesterday (still open)

- Sun/Planet white-render bug — untouched today.
- Serial / Arduino integration — plan laid out in the old session below, not started.

---

Hand-off from 2026-11-21 late-night session. Read this first tomorrow.

## State of play

Game is **playable with mouse**. Phases 0–6 shipped: controllers, cannons, pooled projectiles, enemy waves, HP/score HUD, SFX, music, pause, off-screen arrows, reticle. MegaShot cone AOE in. Spawning is above-horizon only. Platform under the ship is a light-grey disc (runtime-built).

**Phase 7 is partially done.** Starfield + stars work. Sun and Planet are instantiated but render **pure white** no matter what. See the bug below — this is the blocker.

## Triumphs tonight

- Full QuickFlick rename. Namespaces `QuickFlick.*`. Repo on GitHub: `tomw1543/QuickFlick--FPS-unity-`.
- Doc set written: root `CLAUDE.md`, `SPEC.md`, `CONTEXT.md`, per-area `CLAUDE.md` in Bootstrap/Combat/Enemies/Systems/UI/Audio/Input.
- Tier 2 workflow proven: parallel Sonnet builders completed Phases 1–6 without colliding.
- Hybrid scene rule enforced: `Main.unity` holds only 5 bootstrap objects, `GameBootstrap` instantiates everything else.
- Difficulty tuning: DroneSpeed 8 → 2, projectile hit-collider 2.5×, arrow size scales with distance (bigger = closer).
- MegaShot pivoted from projectile-multiplier to short-range cone AOE (iterates `FindObjectsByType<Enemy>` in `MegaShotDetector`).
- `DontDestroyOnLoad` wired on HUD, Sfx, PostFx so scene reload keeps them alive.
- **Blender pipeline for .glb → .fbx works end-to-end.** 20 Quaternius/Poly models converted. Script at `scripts/glb_to_fbx.py`. Run with `"C:\Program Files\Blender Foundation\Blender 5.3\blender.exe" --background --python scripts/glb_to_fbx.py`.
- FBX `AssetPostprocessor` (`Assets/Editor/FbxMaterialToUrpPostprocessor.cs`) swaps Standard → URP Lit on import for everything under `Resources/SkyBox/Converted/`.
- Runtime safety net `FixImportedMaterials` in `GameBootstrap` clones each imported material, forces URP Lit, copies colour from `_BaseColor`/`_Color`, applies orange fallback for near-black or Unity-default-grey.
- Verified by logs: planet material at runtime shows `shader=Universal Render Pipeline/Lit color=RGBA(0.800, 0.370, 0.070)` — orange is actually assigned.

## Fails tonight

- **GLTFast is broken on Unity 6000.0.27f1.** Versions 6.0.0 and 6.21.0 both fail to compile (`ReadOnlySpan<byte>` API not present). Package removed; `.glb` loading dropped in favour of the Blender → FBX pipeline.
- **Nebulae abandoned.** Procedural, NASA image, additive particle — all rendered as grey balls. NebulaBuilder is a no-op now. User said "skip nebulas all together".
- **Sun and Planet still render white.** This is the current open bug. Materials are logged as correct (orange `_BaseColor` on URP Lit for Planet, warm-white texture on Unlit/Texture for Sun), but both appear white in play. Attempts that did NOT fix it:
  - Swapping URP Lit → Unlit/Texture on the Sun
  - Setting `_MainTex` and `_BaseMap` with a solid-colour 2×2 texture
  - Running an `AssetPostprocessor` over every FBX to convert Standard → URP Lit
  - Runtime `FixImportedMaterials` cloning materials and forcing URP Lit with explicit colours
  - Treating Unity's default 0.8/0.8/0.8 grey as "no colour set" and applying the orange fallback
  - Lowering bloom from threshold 0.9 / intensity 1.2 → threshold 1.5 / intensity 0.6
  - Switching tonemapping ACES → Neutral

  All changes compiled and ran. Logs show the materials are correct every time. Render is still white.

## Current theory — where to look tomorrow

Material is orange, render is white. The material isn't the issue. Something between material and pixel is overriding. Candidates in order of likelihood:

1. **Directional light still blowing it out.** Intensity 1.3, white, aimed at the planet. Even with Neutral tonemap, orange × bright white directional = near-white. Try `light.intensity = 0.6f` in `GameBootstrap.BuildSun`.
2. **HDR camera + no exposure clamp.** Main Camera likely has HDR on from URP defaults. Without an Exposure override in the PostFx profile, bright surfaces saturate. Add `profile.Add<ColorAdjustments>(true)` with `postExposure.Override(-1.5f)` and see what happens.
3. **The planet FBX ships with a transparent surface type.** Scene lighting + transparent = white sky bleed. Check `material.GetFloat("_Surface")` in the `FixImportedMaterials` log — should be 0 (Opaque). Force it: `m.SetFloat("_Surface", 0); m.SetOverrideTag("RenderType", "Opaque"); m.renderQueue = 2000;`
4. **`Lighting Data Asset is incompatible`** warning in the log is NOT harmless — URP may be sampling broken baked lightmaps. Open Window → Rendering → Lighting → Delete the Lighting Data Asset in the scene inspector, or click "Clear Baked Data". Sun/Planet may start to render with their actual surface shading.
5. **Nuclear option: disable PostFxBuilder entirely for one play test.** Comment out its `[RuntimeInitializeOnLoadMethod]` attribute. If sun/planet render correctly with NO post-fx, the culprit is 100% in the volume profile. If they still render white, it's lighting or the shader itself.

Try these in order, one per play run. Logs already print material shader + colour, so you only need to confirm pixel colour visually.

## Plan — tomorrow

### Morning (fix + finish Phase 7)
1. Work the sun/planet fix list above. Budget 30 min. If still broken after step 5, kill Sun and Planet entirely for the demo — just the starfield + platform is fine.
2. Phase 7 remaining scope (per `SPEC.md`):
   - Asteroid enemy, big → splits into 2 smalls on death, smalls die in one shot
   - Crab enemy (3 HP)
   - Octopus enemy (6 HP)
   - Space Drone enemy with blockable balls
   - Wave composition rewrite per the Phase 7 table
   - Title screen (one button: PLAY)
3. Load the converted FBX models into the enemies: use `Resources.Load<GameObject>("SkyBox/Converted/<name>")` and the same `FixImportedMaterials` helper. Model mapping:
   - Asteroid → `Asteroid by Poly by Google - enaIlQWET9a`
   - Crab → (Quaternius enemy — pick from `Enemy Large by Quaternius - UgBmRVnQ9h` or `Robot Enemy by Quaternius - 1gNo5ezvmr`)
   - Octopus → `Alien by Quaternius - sUTLXji0aL`
   - Space Drone → `Satellite by Poly by Google - fmYHY1leH_P`

### Afternoon (Arduino)
1. Serial protocol: `SerialInput` reads ASCII `LX,LY,LB,RX,RY,RB\n` at 115200 on a background thread. Parse floats with `CultureInfo.InvariantCulture`.
2. `ControllerProtocolTests.cs` — required per `CLAUDE.md`. Parse one line, parse garbage, parse half-line + newline across two reads. EditMode asmdef.
3. Firmware: Arduino sketch reads two analog sticks + two buttons, prints the same format, 100 Hz.
4. Upload with `arduino-cli` (command goes in `SPEC.md` once wired).
5. Close `SerialPort` on `OnApplicationQuit` AND on domain reload (editor).

### Model rule (tomorrow is Tier 3 territory)
- Serial + threading = Tier 3. Spawn Sonnet builders for firmware + C# serial + tests in parallel. Opus reviewer must PASS before the human tests.
- Every builder prompt: "Load and follow the `ponytail:ponytail` skill."

## Known noise in the editor log (ignore)

- `Identifier uniqueness violation: Name:barrel/gate/Group, Type:Mesh` — Blender FBX export has duplicate mesh names across the Poly pack. Harmless, Unity still imports.
- `Lighting data asset 'LightingData' is incompatible` — stale baked lightmap. Delete the asset from the scene inspector (see candidate 4 above). This one is possibly NOT harmless, revisit tomorrow.

## File map (where everything lives)

- `QuickFlick/Assets/Scripts/Bootstrap/GameBootstrap.cs` — single entry point; builds platform, sun, planet, calls `FixImportedMaterials`
- `QuickFlick/Assets/Scripts/Editor/` — nothing yet; postprocessor lives under `Assets/Editor/` (Unity's special folder)
- `QuickFlick/Assets/Editor/FbxMaterialToUrpPostprocessor.cs` — FBX import material conversion
- `QuickFlick/Assets/Resources/SkyBox/*.glb` — source models (do not delete)
- `QuickFlick/Assets/Resources/SkyBox/Converted/*.fbx` — Blender-exported, Unity-ready models
- `scripts/glb_to_fbx.py` — Blender conversion script
- `QuickFlick/Assets/Scripts/UI/PostFxBuilder.cs` — bloom/vignette/tonemap volume
- `QuickFlick/Assets/Resources/Audio/Music/*.ogg` — music loop
- `QuickFlick/Assets/Resources/Audio/Sfx/*.ogg` — shot/hit/mega/wave sfx

## Repo state

- Branch: whatever's current (not committed tonight)
- GitHub remote: `https://github.com/tomw1543/QuickFlick--FPS-unity-.git`
- Nothing pushed since the rename. Morning task: commit the Blender pipeline + postprocessor + runtime material fix as one feature commit before the Phase 7 enemies land.

## Quick test on wake

1. Open Unity. Let it compile. Press Play.
2. Expected: mouse-look fires cannons, drones spawn above horizon, you shoot them, HP bar + score update, waves escalate, music plays.
3. Actual bug: planet + sun are white. Attack the fix list above, step by step.
