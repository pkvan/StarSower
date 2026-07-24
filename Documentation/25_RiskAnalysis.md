# 25 — Risk Analysis

## Purpose
This document identifies the design, technical, and business risks most likely to threaten StarSower's success, and the mitigation strategy owned by this bible for each. It exists to make risk management a living, referenced practice rather than an afterthought.

## Goals
- Enumerate concrete, specific risks rather than generic project-management boilerplate.
- Tie each risk to the specific document(s) responsible for its mitigation.
- Flag risks that require ongoing monitoring versus risks that are mitigated by a single design decision already made.

## Principles
- **Name risks specifically.** "The game might not be fun" is not actionable; "the endless-climb loop may feel repetitive after Region 3 without difficulty-curve variety" is.
- **Every risk has an owner document.** A risk without a clear mitigation owner is treated as unmitigated, regardless of how small it seems.
- **Mitigate through design, not through hope.** Wherever possible, risks are addressed by a structural design decision (already reflected in this bible), not a vague intention to "keep an eye on it."

## Detailed Design

### 1. Design Risks

**Risk: Endless vertical climbing becomes repetitive.**
- *Mitigation:* One new platform behavior per Region (`11_Platforms.md`, `12_Regions.md`) provides a structured novelty curve; procedural spawning (`14_TechnicalArchitecture.md`) ensures no two runs are identical even within one Region.
- *Ongoing monitoring:* Playtest feedback specifically after Region 3, where fatigue risk is highest per typical vertical-platformer pacing.

**Risk: Failure feels punishing despite design intent.**
- *Mitigation:* The two-currency system (`09_Progression.md`) guarantees no progress loss on fail; `08_GameFeel.md`'s explicit "nothing sudden" failure-feedback rules prevent harsh sensory punishment.
- *Ongoing monitoring:* First-time-player sentiment testing specifically around the fail moment.

**Risk: One-thumb mobile controls feel imprecise compared to two-thumb alternatives.**
- *Mitigation:* Generous coyote time and jump buffering (`08_GameFeel.md`), full air control, and a reachability-guaranteed platform generation system (`11_Platforms.md`) compensate structurally for single-thumb precision limits.
- *Ongoing monitoring:* Device-diversity testing (touch latency varies meaningfully across low-end Android hardware).

**Risk: Minimal-cast, no-dialogue storytelling fails to land emotionally.**
- *Mitigation:* Heavy reliance on visual/audio storytelling craft (`05_ArtDirection.md`, `07_Audio.md`) rather than text — this is a higher-craft-bar approach and carries genuine execution risk that must be validated with real playtesting, not assumed to work from documentation alone.
- *Ongoing monitoring:* Early playtests should specifically probe whether players understand the "restoring light" throughline without being told.

### 2. Technical Risks

**Risk: Mobile performance degradation as content grows (more Regions, more particle effects, more platforms on screen).**
- *Mitigation:* Object pooling plan (`14_TechnicalArchitecture.md` §5), Addressables-based memory management per Region (`19_ContentPipeline.md`), and art constraints favoring sprite-based, low-overhead effects (`05_ArtDirection.md` §6).
- *Ongoing monitoring:* Regular profiling against defined low/mid-tier Android device benchmarks, not just high-end test devices.

**Risk: Architecture drift as the team/timeline grows.**
- *Mitigation:* `16_CodingGuidelines.md`'s binding process discipline (explain-before-code, no unprompted refactors) and `14_TechnicalArchitecture.md`'s Dependency Direction diagram, treated as enforced review criteria.
- *Ongoing monitoring:* Periodic architecture review against the Dependency Direction diagram as new systems are added.

**Risk: Save corruption or data loss erodes trust in the meta-progression promise.**
- *Mitigation:* Atomic writes and backup-recovery strategy (`13_SaveSystem.md` §4) directly address this; the "progress never regresses" promise (`00_Vision.md`, `09_Progression.md`) is only as credible as this system's reliability.
- *Ongoing monitoring:* Explicit QA test cases for app-kill-during-save scenarios (see proposed `30_QATesting.md`).

### 3. Business/Monetization Risks

**Risk: Cosmetic-only monetization underperforms revenue expectations.**
- *Mitigation:* This is an accepted, deliberate trade-off per `22_Monetization.md`'s permanent no-pay-to-win constraint — the mitigation is realistic revenue expectation-setting, not a design compromise. Any future revenue-shortfall discussion must propose solutions that still satisfy `22_Monetization.md`'s exclusion list, not quietly relax it.

**Risk: Rewarded-ad revive feature reduces perceived challenge/integrity of runs.**
- *Mitigation:* Limited to once per run, framed as optional and low-pressure (`22_Monetization.md` §1.2); Starlight/achievement systems are explicitly never affected by whether a revive was used, preserving the integrity of meta-progression regardless of ad usage.

### 4. Scope Risks

**Risk: Feature creep erodes the "Minimal UI" / "No Feature Bloat" pillars over a long dev cycle.**
- *Mitigation:* `00_Vision.md`'s explicit filter question, required for every feature proposal; `26_Backlog.md`'s structure separates committed scope from speculative ideas (`27_FutureIdeas.md`) to prevent silent scope absorption.

## Future Expansion
- A formal risk-review cadence (e.g., revisited at each `20_ContentRoadmap.md` phase boundary) should be adopted once production is underway, logging whether each risk's mitigation held or required adjustment.

## Notes
- This document should be revisited whenever a new system is added to `14_TechnicalArchitecture.md` or a new content category is added to `20_ContentRoadmap.md` — new systems should be checked for new risks they introduce, not just evaluated on their own merits.
- Risks in this document are not exhaustive; it is a living document expected to grow as production surfaces new, concrete concerns.
