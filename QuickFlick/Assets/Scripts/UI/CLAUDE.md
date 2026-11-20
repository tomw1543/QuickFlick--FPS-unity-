# UI

Namespace: `QuickFlick.UI`

HUD, screens, post-fx builder.

- `Reticle` — circle-with-dot. Circle reddens when a camera-forward raycast hits an enemy.
- `Hud` — 5 HP pips top-left, score top-right.
- `WaveBanner` — centre screen, visible only between waves.
- `DebugOverlay` — F3 toggles. Shows stick values, FPS, enemy count.
- `PostFxBuilder` — populates the scene's Global Volume at runtime with Bloom + Vignette + Tonemapping.

**Rule:** HUD is minimal (SPEC.md Q15 (b)). Add elements only when the user asks.

**Rule:** UI reads game state via events — never pulls from systems with `FindObjectOfType`.
