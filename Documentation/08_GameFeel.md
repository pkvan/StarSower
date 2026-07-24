# 08 — Game Feel

## Purpose
This document defines the tactile, moment-to-moment sensation of controlling the Star Sower — the tuning philosophy behind movement, jump arcs, camera response, and feedback timing. Where `01_Gameplay.md` defines *what* the controls do, this document defines *how they should feel* doing it.

## Goals
- Translate `00_Vision.md`'s "Gentle Momentum" pillar into concrete, testable feel targets.
- Give engineering tuning guidance (ranges and qualitative targets, not exact final constants — see `21_Balancing.md` for numeric tuning process) for movement, jump, camera, and feedback systems.
- Define the celebration/impact feedback rules for collection, landing, and Region transitions.
- Define the deliberately gentle failure feedback rules.

## Principles
- **Weight without heaviness.** The character should feel like it has presence and momentum, but never feel sluggish or hard to control.
- **Forgiving edges.** Precision failures at the edge of a platform should be softened by generous, well-tuned tolerances, not punished by pixel-perfect requirements.
- **Feedback proportional to meaning.** Small actions get small feedback; meaningful milestones (Region transitions, Sky Meter growth) get proportionally larger, but always calm, feedback.
- **Nothing sudden.** No screen-shake, sound, or animation in StarSower should ever feel like a jump-scare — abruptness is the one sensation explicitly banned from the feel palette.

## Detailed Design

### 1. Movement Feel
- Horizontal acceleration/deceleration should be fast enough to feel responsive (near-immediate direction changes) but with a small amount of easing so the character never feels like it's snapping between fixed velocities — a subtle acceleration curve, not instant max-speed.
- Air control (horizontal movement while airborne) is fully available and matches grounded movement responsiveness — StarSower does not restrict air control, since punishing mid-air correction contradicts the "forgiving edges" principle.
- There is no "ice" or slippery deceleration anywhere in the base game — stopping is quick and predictable, keeping platforming precise and readable.

### 2. Jump Feel
- Jump initiation must feel instantaneous from input (minimal input-to-liftoff delay) — this is a hard technical requirement, not just a feel preference, since input lag directly damages the "Easy to Learn" pillar.
- Jump arc is tuned to be slightly "floaty" at the apex (a brief hang-time) to give players a comfortable window to make horizontal adjustments and to reinforce the emotional sensation of "rising," per `00_Vision.md`.
- **Coyote time:** a short grace window after leaving a platform's edge during which a jump input still registers as valid, softening accidental late jumps.
- **Jump buffering:** a short grace window before landing during which a jump input is queued and executes immediately on landing, so players who tap jump slightly early are not punished.
- Fall speed (post-apex) is slightly faster than rise speed, giving a satisfying, readable arc without making descents feel endless or floaty in a way that hurts platforming precision.

### 3. Camera Feel
- Camera follow uses smoothed interpolation (not an instant hard-lock to the player), tuned so the camera feels like it is gently "keeping pace" rather than mechanically clamped to the character — see `14_TechnicalArchitecture.md` for the SmoothDamp-based follow system already implemented.
- The Dead Zone buffer (see `01_Gameplay.md`, `14_TechnicalArchitecture.md`) is tuned so minor vertical bobbing from normal jump arcs does not cause visible camera jitter — the camera should feel calm even during fast, repeated jumping.
- Camera Shake and Zoom capabilities exist as a general-purpose feel toolkit (see `14_TechnicalArchitecture.md`) reserved for rare, meaningful moments only — e.g., a soft, low-magnitude shake on a major Region-transition milestone, never on routine jumps or landings, to avoid feel fatigue over long sessions.

### 4. Feedback Timing & Celebration Rules
| Event | Visual | Audio | Camera | Haptic (future) |
|---|---|---|---|---|
| Jump | Small squash/stretch anticipation | Soft whoosh/chime | None | Very light tick |
| Landing | Soft squash on contact | Gentle thud (scaled softly by fall distance) | None | Light tick, scaled by fall distance |
| Star Fragment collected | Bright flash on fragment + brief character glow pulse | Bright short chime | None | Light tick |
| Region transition | Full-screen soft light wash | Musical swell (`07_Audio.md`) | Very subtle, slow zoom-out/in via `CameraZoom` API | Soft double-tick |
| Fail state | Slow fade-to-soft-light, no harsh flash | Soft descending tone, then silence | None (camera holds still, does not shake or snap) | None |
| Sky Meter milestone (Hub) | Ambient brightening animation | Instrument-layer swell (`07_Audio.md`) | N/A (Hub, not in-run) | Soft double-tick |

This table is the canonical feedback contract — any new event added to the game must be slotted into this table before implementation.

### 5. Failure Feel (Critical Rule)
Failure in StarSower must never spike in intensity relative to normal play. Concretely:
- No screen shake on fail.
- No red/harsh color flash on fail.
- No abrupt hard-cut to the Run Summary screen — always a soft, brief transition (under 1 second) that lets the "falling" motion read as gentle drifting rather than a violent stop.
- Run Summary screen audio/visuals open at the same calm intensity as the rest of the game — never a "sad trombone" tonal shift.

### 6. Tuning Process Ownership
This document defines *qualitative* feel targets. Exact numeric values (jump force, move speed, coyote time duration, camera smooth time, dead zone size) are owned and iterated in `21_Balancing.md` and implemented via `[SerializeField]`-exposed values on the relevant components (see `16_CodingGuidelines.md` — no gameplay-feel constant may be hardcoded in code). Any tuning pass must be validated against the qualitative targets in this document before being considered final.

## Future Expansion
- **Haptic feedback implementation** on supported devices, following the table in Section 4 — flagged as a near-term technical task once core feel is validated on real hardware.
- **Adaptive difficulty-assist feel** (slightly more generous coyote time/dead zone after repeated fast fails) — must be carefully scoped to avoid feeling patronizing; flagged in `25_RiskAnalysis.md` for design review before implementation.

## Notes
- Any new mechanic (moving platforms, breakable platforms, bounce platforms — see `11_Platforms.md`) must have its feel target explicitly authored as an addition to this document before being greenlit for production, following the same table format as Section 4.
- "Nothing sudden" (Principles) is the single most important rule in this document and overrides any individual feel decision that conflicts with it.
