# 21 — Balancing

## Purpose
This document owns the numeric tuning process for StarSower — the ranges, methodology, and ownership of every gameplay constant referenced qualitatively elsewhere in this bible (`08_GameFeel.md`, `11_Platforms.md`, `12_Regions.md`, `09_Progression.md`). It does not fix final numbers permanently; it defines how numbers are derived, tested, and revised.

## Goals
- Provide a single authoritative home for numeric tuning so values don't drift inconsistently across documents or components.
- Define a repeatable tuning methodology appropriate for a small team iterating on feel.
- Define the economy math connecting Star Fragments, Starlight, and unlock costs.

## Principles
- **Feel first, numbers second.** Numeric ranges exist to serve the qualitative targets in `08_GameFeel.md`, not the other way around.
- **Reachability is a hard constraint, not a tuning variable.** Jump envelope and platform gap ranges must always satisfy `11_Platforms.md`'s reachability guarantee — this is validated, not just tuned.
- **Generosity by default.** When in doubt between a stricter and a more forgiving value, the more forgiving value is chosen, consistent with `00_Vision.md`'s "no punishing difficulty" stance.
- **Currency curves favor early accessibility.** Early unlocks (first cosmetics, first Beacon) must be reachable within a small number of sessions, per `09_Progression.md`'s anti-grind principle.

## Detailed Design

### 1. Movement & Jump Tuning (Reference Ranges)
Exact final values are set in `PlayerMotor`/`GroundChecker`/`PlayerMovementConfig` (see `14_TechnicalArchitecture.md`, `18_ScriptableObjects.md`) via playtesting, but must fall within these qualitative reference ranges:
- **Move speed:** fast enough to cross a full screen width in under 2 seconds, never so fast that fine platform-edge positioning becomes twitchy on a touch joystick.
- **Jump height:** must comfortably clear the largest vertical gap defined for the easiest Region (`The Fading Ground`) with visible margin, so players never feel a jump "barely" succeeded on the tutorial Region.
- **Coyote time / jump buffer window:** short enough to be imperceptible as "cheating," long enough to eliminate frustration from frame-perfect inputs (target: a few tenths of a second, validated by playtesting, not fixed here as dogma).

### 2. Platform Gap Tuning
- Vertical and horizontal gap ranges (see `11_Platforms.md` §2) must always keep the *maximum* possible gap achievable with clear margin below the *minimum* achievable jump distance/height, at the tuned movement values from Section 1 — never tuned so tightly that the two ranges are equal.
- Gap ranges widen slightly (more horizontal variance) in later Regions to increase difficulty through variety and pacing, never by shrinking the safety margin below the reachability guarantee.

### 3. Region Difficulty Curve
Difficulty across the five Regions (`12_Regions.md`) increases via:
1. Introduction of one new platform behavior per Region (complexity), not tighter gap tolerances (per Section 2).
2. Gradually reduced average "float" time between required inputs (more frequent decision points), still within the same jump envelope.
3. Increased platform behavior density (more Moving/Breakable/Vanishing platforms per screen) in later Regions, always maintaining at least one stable path per `11_Platforms.md`'s fairness rule.

### 4. Economy Tuning
- **Star Fragment → Starlight conversion rate:** tuned so an average run (`01_Gameplay.md`'s 60–180 second target) yields a meaningful, visible Starlight gain — never a rounding-to-zero amount that makes short runs feel pointless.
- **First cosmetic item cost:** tuned to be reachable within roughly 3–5 typical runs for a new player, reinforcing early momentum per `09_Progression.md`'s anti-grind principle.
- **Beacon unlock requirement:** tuned as a "reach this Region N times" or Sky Meter milestone (owned by this document, referencing `09_Progression.md` §3) — set generously enough that a reasonably engaged player unlocks their first Beacon within their first several sessions, not after extensive repetition.

### 5. Tuning Methodology
1. Set an initial value within the qualitative range/target defined above or in `08_GameFeel.md`.
2. Playtest against the specific feel/fairness question the value serves (e.g., "does this coyote time eliminate frustration without feeling like cheating?").
3. Adjust in small increments; document the final chosen value directly on the relevant `[SerializeField]` field's Tooltip or the relevant ScriptableObject config asset (`18_ScriptableObjects.md`) — not duplicated as a hardcoded number in this document, to avoid drift between documentation and implementation.
4. Re-validate reachability (Section 2) after any movement or gap-range change — this is a mandatory regression check, not optional.

### 6. Difficulty Validation Process
Before any Region ships (`20_ContentRoadmap.md`), it must pass:
- A "cold" playtest (a tester with no prior knowledge of that Region's new platform behavior) to validate `00_Vision.md`'s "Easy to Learn" pillar holds for first exposure.
- A reachability audit confirming every generated platform sequence in that Region's config is completable within the tuned jump envelope, including behavior-specific reachable windows (see `11_Platforms.md` §3.1–3.4).

## Future Expansion
- **Live-data-informed balancing** post-launch (fail-location heatmaps, average run length by Region) once telemetry (`17_EventSystem.md`, `20_ContentRoadmap.md` Phase 4) is in place.
- **Difficulty-assist tuning variant** (see `08_GameFeel.md` Future Expansion, `25_RiskAnalysis.md`) as a separate, opt-in tuning profile rather than a change to the base curve.

## Notes
- This document is the only authorized place to describe *why* a numeric range was chosen; individual component Tooltips and ScriptableObject assets hold the *current implemented value*, which may be revised without updating this document unless the underlying range/methodology itself changes.
- Any proposed value outside the ranges defined here requires this document to be explicitly revised first, not silently overridden in a component.
