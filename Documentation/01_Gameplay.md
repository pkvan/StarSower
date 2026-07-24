# 01 — Gameplay

## Purpose
This document defines the core gameplay loop, control scheme, win/fail conditions, and moment-to-moment player experience of StarSower. It is the single source of truth for "what does the player actually do." Every mechanic described here must serve the thesis: *every jump brings light.*

## Goals
- Define the core loop precisely enough that design, engineering, and art can build against it without ambiguity.
- Lock the control scheme so all downstream documents (`08_GameFeel.md`, `11_Platforms.md`, `17_EventSystem.md`) share one canonical input model.
- Define session structure appropriate for mobile (short, replayable, low-friction).
- Establish the fail condition and its emotional framing.

## Principles
- **Easy to Learn, Hard to Master.** A first-time player must understand the entire control scheme within 10 seconds, with zero tutorial text if possible.
- **One-handed, thumb-only.** All controls live in the bottom half of the screen and require no more than one thumb.
- **Momentum tells the story.** The player's upward progress *is* the game's narrative progress — there is no separate "cutscene mode."
- **No dead time.** From app open to "in control of the character," the player should wait as little as possible.

## Detailed Design

### 1. Core Loop
```
Launch Run → Climb (Move + Jump across Platforms) → Collect Star Fragments
   → Camera climbs with player → Player falls behind Camera threshold
   → Run Ends → Star Fragments convert to Starlight → Return to Hub
```
This loop is designed to complete in 60–180 seconds for an average run, matching mobile "one more try" session expectations (see `20_ContentRoadmap.md` for pacing targets).

### 2. Control Scheme (Canonical)
StarSower ships with exactly two physical inputs, matching the project's implemented `IInputProvider` abstraction:

- **Left Joystick (on-screen, bottom-left):** controls horizontal movement only. Fully analog; magnitude controls movement speed proportionally, not just direction.
- **Jump Button (on-screen, bottom-right):** a single discrete action. Pressing it while grounded triggers a jump of fixed height/force (see `08_GameFeel.md` for tuning philosophy). Pressing it while airborne does nothing (no double-jump in the base kit — see Future Expansion).

There is no drag-to-aim, no slingshot, and no swipe gesture. This is a deliberate, final decision: earlier prototyping explored a slingshot-style drag-aim control, but it was rejected because it required two-handed precision and did not scale well to one-thumb mobile play. Joystick + Jump is the canonical and only supported control scheme going forward.

A keyboard-equivalent mapping exists for editor testing only (arrow keys / A-D for horizontal, Space for jump) and must never diverge in feel from the mobile mapping — see `16_CodingGuidelines.md` for the input abstraction that guarantees this.

### 3. Movement Rules
- Gravity is always active. The character is never "floating" outside of a jump arc.
- Horizontal movement and vertical (jump) movement are independent — the joystick does not need to be held to remain in a jump arc; releasing it decelerates the character back to a stop.
- Landing on a platform is defined by a grounded state (see `11_Platforms.md`), which is required before another jump can be triggered.
- There is no fall damage, no stamina, no "run" toggle. Movement has exactly one speed curve, tuned for readability over realism.

### 4. Camera as a Gameplay System
The camera is not passive — it is an active difficulty and pacing tool:
- The camera follows the player upward only and never retreats downward, permanently "closing the door" behind the player. This creates gentle forward pressure without a countdown timer.
- A **Dead Zone** buffer means small movements near the current camera position do not cause camera motion, keeping the frame calm during fine platforming.
- The camera's refusal to descend is the mechanical embodiment of the fail condition: falling out of frame is fatal not because of an arbitrary rule, but because light, once climbed toward, is not something the world gives back for free.

### 5. Fail Condition
The run ends when the player's vertical position falls a configurable distance below the camera's current position (see `14_TechnicalArchitecture.md` for the implementation contract). On fail:
- No harsh visual/audio punishment (see `08_GameFeel.md`, `07_Audio.md`).
- All Star Fragments collected during the run are preserved and converted to Starlight (see `10_Collectibles.md`, `09_Progression.md`) — the player is never asked to repeat exact platform sequences to "recover" lost currency.
- The player is returned to the Hub with a clear, calm summary: height reached, fragments collected, Starlight earned.

### 6. Win Condition
StarSower's core mode has no traditional "win" — it is a score-attack / endless climb structure, where the measure of success is height reached and light restored. Region completion (see `12_Regions.md`) provides mid-run milestones, and the long-term "win" is the completion of the Sky Restoration Meter (see `09_Progression.md`), which is a meta-progression goal, not a single-run goal.

### 7. Session Structure
- **Cold start to control:** under 5 seconds (no forced splash sequences beyond a brief, skippable logo).
- **Average run length:** 60–180 seconds.
- **Between-run friction:** a single tap returns the player to a new run from the Hub. No forced ads, no mandatory screens (see `22_Monetization.md`).

## Future Expansion
- **Double Jump / Air Dash as unlockable Beacon rewards:** must be designed as optional meta-progression unlocks, never required to complete a Region, to preserve the "Easy to Learn" pillar for new players.
- **Wall interactions** (wall-slide, wall-jump) were prototyped early in development but are not part of the canonical control scheme; if revisited, they must be re-validated against the one-thumb principle before being reconsidered.
- **Assist Mode:** a future accessibility option (larger dead zones, slower fall speed) — see `24_Localization.md` and `25_RiskAnalysis.md` for accessibility considerations.

## Notes
- Any future mechanic that requires a second on-screen control beyond the joystick and jump button requires explicit sign-off against the Vision filter in `00_Vision.md`, as it directly threatens the one-handed design pillar.
- The slingshot/drag-aim control scheme is explicitly deprecated and must not be reintroduced without a full revision of this document and `08_GameFeel.md`.
