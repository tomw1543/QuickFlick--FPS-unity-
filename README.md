# QuickFlick

Unity 6 (URP, C#) stationary FPS space shooter. Dual analog sticks over Arduino USB serial (hardware tomorrow); mouse tonight.

## Run

1. Unity Hub → Open → `QuickFlick/`
2. Open `Assets/Scenes/Main.unity`
3. Press Play. Move the mouse to look. LMB / RMB fire. Both together within 150 ms = MegaShot.

## Where things are

| Path | What |
|---|---|
| `SPEC.md` | Scope, phases, enemy/scoring/wave data |
| `CONTEXT.md` | Glossary |
| `CLAUDE.md` | Agent workflow and project rules |
| `HANDOFF.md` | Session-pause record (frozen) |
| `QuickFlick/Assets/Scripts/<area>/` | Code, one `CLAUDE.md` per area |
| `.claude/agents/` | `sonnet-builder`, `opus-reviewer` |
| `docs/adr/` | Decisions (none yet) |

## Hardware

Arduino + two analog sticks arrive 2026-11-21. Mouse is the always-available fallback.
