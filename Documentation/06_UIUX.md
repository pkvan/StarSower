# 06 — UI / UX

## Purpose
This document defines every user-facing interface element in StarSower, the interaction rules governing them, and the philosophy that keeps the UI minimal enough to never compete with the world itself. It is authoritative for anything the player reads, taps, or looks at outside of direct character control.

## Goals
- Enumerate every screen and HUD element the game will ever need for its core loop.
- Lock a "diegetic-first, minimal-chrome" UI philosophy consistent with `00_Vision.md`.
- Define concrete interaction and layout rules usable directly by UI implementation (see `14_TechnicalArchitecture.md`, `06_UIUX.md`'s relationship to the existing `IInputProvider`/on-screen control implementation).
- Ensure UI never obstructs the vertical sightline needed for platforming.

## Principles
- **Minimal UI.** If information can be conveyed through the world (light, motion, color) instead of a UI widget, it must be.
- **Two-zone screen.** The bottom third of the screen is reserved for controls; the rest of the screen is reserved for the game world — no HUD element may cross this boundary during active play.
- **Calm typography.** Text, when it must appear, is soft, rounded, and appears sparingly — StarSower is not a stats-heavy game.
- **No modal interruptions during a run.** Once a run starts, nothing may pause it to show a popup, ad, or notification except the run-end summary.

## Detailed Design

### 1. Screen Inventory
StarSower has exactly four screens/states in its base structure:

1. **Hub (Main Menu)** — see `02_World.md`. Shows Sky Restoration Meter, Play button, access to Beacons and Cosmetics.
2. **In-Run HUD** — minimal overlay during gameplay.
3. **Run Summary** — shown immediately after a run ends.
4. **Cosmetics / Beacon Selection** — a lightweight panel accessed from the Hub, never from mid-run.

No settings menu is treated as a "screen" in the traditional sense — see Section 5.

### 2. In-Run HUD
The in-run HUD is deliberately sparse:
- **Left joystick and Jump button** (bottom zone, see `01_Gameplay.md` for the canonical control scheme; implemented via the project's on-screen `OnScreenJoystick` and `TouchButton` UI widgets).
- **A small, unobtrusive Star Fragment counter** (top corner, small icon + number, low-opacity until a fragment is collected, at which point it briefly brightens) — this is the only persistent numeric readout during play.
- **No health bar** (there is no health system — see `01_Gameplay.md`).
- **No visible altitude/score counter during play** — height is only shown at Run Summary, keeping the in-run screen calm per `00_Vision.md`.
- **No minimap, no pause button during free-fall-to-death sequences** (the run-end transition is automatic and calm, not player-triggered mid-fall).

A **Pause button** (small, top corner, low-opacity) is the only other permitted HUD element, opening a simple overlay with Resume / Restart / Return to Hub — no settings changes are exposed mid-pause beyond audio mute (see `07_Audio.md`).

### 3. Run Summary Screen
Shown immediately after a fail state (see `01_Gameplay.md`):
- Height reached (this run + best-ever, shown gently, never as a harsh "high score" callout).
- Star Fragments collected this run, animated converting into Starlight (visual tie to `09_Progression.md`).
- A single primary action: "Climb Again." A secondary, smaller action: "Return to Hub."
- No forced rating prompts, no forced ad, no forced social share button (optional share may exist but must never be the primary or default-focused button).

### 4. Hub Screen
- Sky Restoration Meter is the dominant visual element, rendered as part of the world (a brightening horizon), not a traditional progress bar with a percentage number (a small numeric readout may exist but is secondary to the visual).
- A single, large, unambiguous "Play" interaction — no nested menus required to start a run.
- Beacon selection (see `12_Regions.md`) and Cosmetics are accessed via small, clearly-iconed side buttons, not front-and-center, since starting a run is always the primary action.

### 5. Settings
Settings are treated as a lightweight overlay accessible from the Hub only (never mid-run), containing exactly: Music volume, SFX volume, Haptics toggle, Language (see `24_Localization.md`), and Credits. No account system, no complex preference matrix.

### 6. Interaction & Layout Rules
- All interactive elements must have a minimum touch target of 48x48dp (standard mobile accessibility minimum) regardless of visual size.
- The joystick and jump button positions are fixed to screen-relative anchors (bottom-left / bottom-right) and must remain reachable one-handed on all supported aspect ratios (see `14_TechnicalArchitecture.md` for the Canvas scaling contract already established in the project — Scale With Screen Size, 1080x1920 reference resolution).
- No UI animation may exceed 300ms for functional transitions (menu opens, button feedback) to keep the game feeling responsive, reserving longer, slower animation timing exclusively for celebratory/ambient moments (Sky Meter fill, Region transition) per `08_GameFeel.md`.
- Text uses a single rounded, friendly typeface family across the entire game — no mixing of multiple display fonts.

### 7. Accessibility Baseline
- Colorblind-safe contrast between the joystick/button controls and the background at all times (controls use a translucent neutral pastel that is deliberately distinct from any Region's dominant hue).
- All numeric information (height, fragment count) must also be readable via icon + number, never color alone.
- Full guidelines tracked in `25_RiskAnalysis.md` and `24_Localization.md`.

## Future Expansion
- **Haptic feedback layer** tied to jump/landing/collection events — flagged for `08_GameFeel.md` cross-reference once mobile haptics implementation is scheduled.
- **Colorblind mode palette variant** as a settings toggle, once the core Region palettes (see `05_ArtDirection.md`) are finalized.
- **Optional altitude ghost-line** (a subtle, toggleable in-run marker showing personal best height) — must remain off by default to preserve the calm in-run screen.

## Notes
- Any new UI element proposal must specify which of the four screens it belongs to and must be checked against the "two-zone screen" rule before being approved.
- The in-run HUD element count is intentionally capped (joystick, jump button, fragment counter, pause button) — a new persistent in-run HUD element requires explicit revision of this document, not an incremental addition.
