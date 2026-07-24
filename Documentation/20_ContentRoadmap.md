# 20 — Content Roadmap

## Purpose
This document sequences StarSower's content production across development phases — what ships in the initial vertical slice, what ships at launch, and what follows post-launch. It translates the full design scope in this bible into an ordered, buildable plan.

## Goals
- Define a realistic phase structure that avoids building breadth before depth is validated.
- Ensure every phase produces a playable, coherent experience, not a partial/broken one.
- Give `26_Backlog.md` a phase structure to organize concrete tasks against.

## Principles
- **Validate the core loop before adding content breadth.** One Region, fully polished, proves the game before five Regions are built.
- **Every phase must be playable end-to-end.** No phase ships a broken or incomplete core loop.
- **Content follows systems, not the reverse.** Platform behaviors and Region config systems (`11_Platforms.md`, `18_ScriptableObjects.md`) are built before the content that depends on them is authored.

## Detailed Design

### Phase 0 — Foundation (Current State)
Already implemented, per `14_TechnicalArchitecture.md`: core movement (`PlayerController`/`PlayerMotor`/`GroundChecker`), procedural static-platform spawning with reachability guarantees, upward-only smoothed camera with Dead Zone and prepared Shake/Zoom APIs, fail-state detection, and the mobile-first `IInputProvider` abstraction (joystick + jump button). This phase establishes the technical spine every later phase builds on.

### Phase 1 — Vertical Slice: The Fading Ground
- Full art/audio pass on Region 1 only (`12_Regions.md` §2.1), validating the pastel visual identity (`05_ArtDirection.md`) and adaptive music base layer (`07_Audio.md`) end-to-end.
- Implement Star Fragments (`10_Collectibles.md`) and the Star Fragment → Starlight conversion loop (`09_Progression.md`).
- Implement the Hub screen (`02_World.md`, `06_UIUX.md`) with the Sky Restoration Meter, even if visually simple at this stage.
- Implement local save (`13_SaveSystem.md`) covering Starlight and best-height tracking.
- **Exit criteria:** a complete, polished loop — Hub → run in Region 1 → fail → Starlight earned → Hub reflects growth — playable start to finish with no placeholder art or text.

### Phase 2 — Systemic Expansion
- Introduce the first ScriptableObject config assets (`18_ScriptableObjects.md`: `PlayerMovementConfig`, `RegionConfig`) so subsequent Region work is data-driven from the start.
- Implement Moving and Breakable platform behaviors (`11_Platforms.md` §3.1–3.2) and validate them against the Fairness Checklist.
- Build Regions 2 and 3 (`The Grey Cloudbelt`, `The Twilight Reach`) using the new config-driven pipeline.
- Implement Beacons (`09_Progression.md` §3) for Regions 1–3.

### Phase 3 — Full Vertical Slice Completion
- Implement Vanishing and Bounce platform behaviors (`11_Platforms.md` §3.3–3.4).
- Build Regions 4 and 5 (`The Fallen Star Expanse`, `The Zenith`), completing the base five-Region climb.
- Implement the full Cosmetics catalog (`10_Collectibles.md` §4) and Cosmetics UI panel.
- Implement Achievements (`23_Achievements.md`).
- Begin real Object Pooling (`14_TechnicalArchitecture.md` §5) and Addressables migration (`19_ContentPipeline.md`) as performance/memory work, targeting mid-tier Android device benchmarks.

### Phase 4 — Launch Readiness
- Localization pass (`24_Localization.md`) for initial target languages.
- Monetization integration (`22_Monetization.md`): cosmetic IAP, optional rewarded-ad revive.
- Full QA/balancing pass across all five Regions (`21_Balancing.md`, `25_RiskAnalysis.md`).
- Store listing, analytics/telemetry hookup via the event channel system (`17_EventSystem.md`).

### Phase 5 — Post-Launch
- Additional Regions beyond the Zenith (see `12_Regions.md` Future Expansion, `27_FutureIdeas.md`).
- Remote Addressables content delivery for post-launch content without app updates (`19_ContentPipeline.md`).
- Ongoing balancing based on live player data (`21_Balancing.md`).

## Future Expansion
- A living, date-stamped release-planning addendum should track actual phase timelines once production begins — this document defines *order and scope*, not calendar dates, to remain stable as a reference regardless of schedule changes.

## Notes
- No phase may be reordered to introduce content breadth (more Regions) before the systemic work it depends on (platform behaviors, config assets) is complete — this is the primary risk this roadmap is designed to prevent, per `25_RiskAnalysis.md`.
- This roadmap must be re-validated against `00_Vision.md`'s filter question whenever a phase's scope is revised.
