# 26 — Backlog

## Purpose
This document is the structured, actionable task backlog derived from `20_ContentRoadmap.md`'s phase plan. Where the roadmap defines *order and scope*, this document breaks that scope into concrete, trackable work items. It is distinct from `27_FutureIdeas.md`, which holds unscoped, speculative concepts not yet committed to production.

## Goals
- Translate roadmap phases into discrete, assignable tasks.
- Keep committed scope clearly separated from speculative ideas, protecting against silent scope creep (see `25_RiskAnalysis.md`).
- Provide a structure that stays useful as a living document throughout production, not a one-time planning artifact.

## Principles
- **Every backlog item traces to a design document.** A task with no owning document (Vision, Gameplay, Platforms, etc.) is not ready to be worked on — it belongs in `27_FutureIdeas.md` until scoped.
- **Backlog items are outcomes, not vague intentions.** "Implement Breakable Platform behavior per `11_Platforms.md` §3.2, validated against the Fairness Checklist" is a backlog item; "make platforms more interesting" is not.
- **Committed scope only.** Nothing enters this backlog without being validated against `00_Vision.md`'s filter question first.

## Detailed Design

### 1. Backlog Structure
Each item should specify: **Phase** (per `20_ContentRoadmap.md`), **System** (per `14_TechnicalArchitecture.md`/`15_ProjectStructure.md`), **Owning Document(s)**, and **Definition of Done**.

### 2. Phase 1 Backlog (Vertical Slice: The Fading Ground)
- Full art pass for The Fading Ground per `05_ArtDirection.md`'s palette table. *Owning docs: `05_ArtDirection.md`, `12_Regions.md`.* DoD: no placeholder sprites remain in Region 1.
- Implement Star Fragment collectible (visual, audio, collection logic) per `10_Collectibles.md`. DoD: collection feedback matches `08_GameFeel.md`'s feedback table exactly.
- Implement Star Fragment → Starlight conversion at run end per `09_Progression.md` §1. DoD: conversion is visible on the Run Summary screen (`06_UIUX.md`).
- Implement Hub screen with Sky Restoration Meter per `02_World.md`, `06_UIUX.md`. DoD: meter visibly changes after at least one completed run.
- Implement local save covering Starlight and best-height per `13_SaveSystem.md` §1–4. DoD: passes an app-kill-during-save test with no data loss.
- Compose and integrate The Fading Ground's base music/ambience layer per `07_Audio.md` §1–4.

### 3. Phase 2 Backlog (Systemic Expansion)
- Author `PlayerMovementConfig` and `RegionConfig` ScriptableObjects per `18_ScriptableObjects.md` §2, migrating existing `PlayerMotor`/`PlatformSpawner` fields to reference them.
- Implement Moving Platform behavior per `11_Platforms.md` §3.1; validate against the Fairness Checklist (§5).
- Implement Breakable Platform behavior per `11_Platforms.md` §3.2; validate against the Fairness Checklist.
- Build and art-pass Region 2 (The Grey Cloudbelt) and Region 3 (The Twilight Reach) per `12_Regions.md` §2.2–2.3.
- Implement Beacon unlock and alternate-start-point logic for Regions 1–3 per `09_Progression.md` §3.

### 4. Phase 3 Backlog (Full Vertical Slice Completion)
- Implement Vanishing Platform behavior per `11_Platforms.md` §3.3; validate against the Fairness Checklist.
- Implement Bounce Platform behavior per `11_Platforms.md` §3.4; validate against the Fairness Checklist.
- Build and art-pass Region 4 (The Fallen Star Expanse) and Region 5 (The Zenith) per `12_Regions.md` §2.4–2.5.
- Implement full Cosmetics catalog and Cosmetics UI panel per `10_Collectibles.md` §4, `06_UIUX.md` §4.
- Implement Achievements system and initial catalog per `23_Achievements.md`.
- Replace `SimplePlatformPool` with a real pooled implementation per `14_TechnicalArchitecture.md` §5.
- Begin Addressables migration for Region content groups per `19_ContentPipeline.md` §2.

### 5. Phase 4 Backlog (Launch Readiness)
- Localization pass for Vietnamese/English per `24_Localization.md` §1–3.
- Integrate cosmetic IAP and rewarded-ad revive per `22_Monetization.md` §1.1–1.2.
- Full cross-Region balancing and reachability audit per `21_Balancing.md` §6.
- Wire analytics/telemetry via the event channel system per `17_EventSystem.md` §5.
- Accessibility audit per `24_Localization.md` §5.

## Future Expansion
- Phase 5 (Post-Launch) items remain intentionally unscoped in this document until Phase 4 is substantially complete, per `20_ContentRoadmap.md` — premature detailed planning of post-launch work risks distracting from launch-critical scope.

## Notes
- This backlog must be revised whenever `20_ContentRoadmap.md` is revised — the two documents are kept in lockstep, with the roadmap owning phase order/scope and this document owning task-level detail.
- Any backlog item that cannot cite a specific owning document/section is invalid and must be scoped properly (or moved to `27_FutureIdeas.md`) before being accepted into a phase.
