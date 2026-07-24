# 11 — Platforms

## Purpose
This document defines every platform type in StarSower, their mechanical behavior, and the extensibility model that allows new platform behaviors to be added without modifying core traversal or spawning systems. It is the design-side companion to the technical platform architecture described in `14_TechnicalArchitecture.md`.

## Goals
- Define the base Static Platform and the full roadmap of planned platform behaviors.
- Guarantee every platform configuration remains fair and reachable, per `01_Gameplay.md` and `08_GameFeel.md`.
- Establish a composition-based extensibility model so new platform behaviors (moving, breakable, vanishing, bounce) can be added as independent, combinable components rather than new monolithic classes.

## Principles
- **Reachability is non-negotiable.** No platform configuration may require a jump outside the tuned, documented jump envelope (see `08_GameFeel.md`, `21_Balancing.md`).
- **Composition over inheritance.** Platform behaviors (movement, breaking, vanishing, bouncing) are additive components layered onto a common base, not a growing hierarchy of platform subclasses — consistent with the project's core technical principle (see `16_CodingGuidelines.md`).
- **New behaviors introduce, never replace, calm.** Any new platform type must still fit `00_Vision.md`'s gentle, non-punishing tone — even "hazard-like" platforms (breakable, vanishing) must fail gently.
- **Visual identity precedes mechanical identity.** A player must be able to tell a platform's behavior by looking at it, before touching it.

## Detailed Design

### 1. Base Platform (Static) — *Implemented*
- The foundational platform type: a stationary, solid surface the player can stand on.
- Currently realized as two visual/size variants (a standard platform and a wider "safer" variant used more often near the start of a run), matching the project's existing `Platform_Basic` and `Platform_Wide` prefab variants.
- All other platform types in this document are designed as additive behaviors layered on top of this base, never a replacement for it.

### 2. Spawning & Reachability Model — *Implemented*
- Platforms are generated procedurally above the camera as the player climbs and recycled once sufficiently far below the camera (see `14_TechnicalArchitecture.md` for the `PlatformSpawner`/`PlatformRecycler` implementation contract).
- Vertical and horizontal gaps between consecutively spawned platforms are bounded by configurable, designer-tunable ranges, guaranteeing every next platform is within the tuned jump envelope from the previous one — this is the mechanical guarantee behind "Spawn always reachable" and must hold for every future platform type added to the spawn pool.
- Multiple platform prefabs can be registered with the spawner and are chosen at random per spawn, which is the extension point for adding new visual/behavioral variants without touching spawner logic.

### 3. Planned Platform Behaviors (Designed, Not Yet Implemented)
Each behavior below is designed as an independent, composable component that attaches alongside the base platform, per the composition-over-inheritance principle. None of these are implemented yet — see `20_ContentRoadmap.md` for sequencing.

#### 3.1 Moving Platform
- Moves along a short, fixed path (horizontal or vertical short-throw) at a slow, predictable, constant speed.
- The player's velocity inherits the platform's motion while standing on it (no relative-motion surprises).
- Visual tell: a subtle drifting particle trail behind the platform, distinguishing it at a glance from static platforms.
- Reachability rule: spawn logic must account for the platform's motion range when computing jump-gap validity — a moving platform's *reachable range*, not just its spawn-time position, must satisfy the reachability guarantee in Section 2.

#### 3.2 Breakable Platform
- Begins to crumble a short, fixed delay after the player lands on it, then disappears — but never instantly, preserving fairness (the player always has enough time to jump away after landing).
- Visual tell: a distinct, slightly more "fractured"-looking pastel texture, readable before landing.
- Failure framing: crumbling is depicted as soft light dispersing (small particle burst, no jagged debris), consistent with `05_ArtDirection.md`'s no-sharp-shapes rule and `08_GameFeel.md`'s no-sudden-failure principle.
- Design constraint: breakable platforms must never be the *only* viable next platform in a jump sequence — spawn logic must guarantee at least one stable (non-breakable) alternative within reach at all times, preserving the "gentle" pillar even under this harder mechanic.

#### 3.3 Vanishing Platform
- Distinct from Breakable: cycles between visible/solid and invisible/intangible on a slow, readable timer, rather than reacting to player contact.
- Visual tell: a slow pulsing fade, telegraphed at least one full cycle in advance so a player approaching can time their jump without punishment for a first-time encounter.
- Reachability rule: spawn logic must guarantee the platform is scheduled to be solid during the earliest window a player could plausibly arrive, avoiding "impossible timing" edge cases.

#### 3.4 Bounce Platform
- Launches the player upward with a fixed, higher-than-normal jump force on contact, without requiring a jump input — a traversal accelerant, not a hazard.
- Visual tell: a distinct springy, rounded silhouette and a soft "boing"-adjacent (but still gentle, per `07_Audio.md`) sound cue.
- Design intent: used sparingly to create rhythm variation and moments of delight ("every jump brings light" made literal by a platform that helps the player rise faster), never as a mandatory precision-timing gate.

### 4. Platform Selection & Region Theming
- Which platform behaviors are eligible to spawn, and at what frequency, is controlled per-Region (see `12_Regions.md`) via data-driven configuration (see `18_ScriptableObjects.md`'s planned `RegionConfig`/`PlatformConfig` assets) — never hardcoded per-scene.
- Lower Regions use Static platforms almost exclusively (teaching the core loop); each subsequent Region introduces exactly one new behavior at a controlled frequency, per the Region breakdown in `12_Regions.md`.

### 5. Fairness Validation Checklist
Before any new platform behavior ships, it must pass:
1. Does the reachability guarantee in Section 2 still hold with this behavior in the spawn pool?
2. Is the behavior visually telegraphed before the player commits to landing on it?
3. Does a failure/edge-case interaction with this platform stay within `08_GameFeel.md`'s "nothing sudden" rule?
4. Can the behavior be implemented as an additive component without modifying the base platform or spawner contracts?

## Future Expansion
- **Combined behaviors** (e.g., a moving + breakable platform) are supported naturally by the composition model in Section 3, but must be introduced gradually and individually validated against the Fairness Checklist before being combined.
- **Region-exclusive "signature" platform variant** for the Zenith (see `12_Regions.md`) as a capstone visual/behavioral treat, to be scoped once the base four behaviors are validated.

## Notes
- This document is the design authority for platform behavior; `14_TechnicalArchitecture.md` and `16_CodingGuidelines.md` own how these behaviors are implemented as components, but must not introduce behaviors not first defined here.
- The reachability guarantee (Section 2) is the single most important constraint in this document — no platform feature may ship if it cannot be proven to preserve it.
