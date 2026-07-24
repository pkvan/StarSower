# 33 — Build & Release (Proposed Addition)

## Purpose
This document defines StarSower's build configuration and release process — how a change in the project becomes a build, and how a build becomes a release. It was proposed as an addition to close the gap between production documents (`20_ContentRoadmap.md`, `26_Backlog.md`) and the concrete mechanics of shipping.

## Goals
- Define build configuration ownership (platform targets, Addressables profiles, versioning).
- Define the release checklist that gates any build from reaching players.
- Keep release process lightweight and appropriate for a small indie team.

## Principles
- **A release candidate must pass `30_QATesting.md`'s critical scenarios, no exceptions.** Section 3 of that document is a hard gate.
- **Versioning is meaningful, not arbitrary.** Version numbers communicate scope of change to the team and, where relevant, to players via store changelogs.
- **Release process is repeatable, not heroic.** No release should depend on undocumented tribal knowledge of build steps.

## Detailed Design

### 1. Platform Targets
- **Primary:** Android (Google Play), given the project's mobile-first, broad-accessibility design intent (`00_Vision.md`, `31_PerformanceBudget.md`'s device-tier considerations).
- **Secondary:** iOS (App Store), targeted once the Android build is stable, sharing the same Unity 6/URP project with platform-specific build settings only where required (e.g., platform store SDK integration for `22_Monetization.md`'s IAP/rewarded-ad features).

### 2. Build Configuration
- **Engine:** Unity 6, URP 2D Renderer (`14_TechnicalArchitecture.md` §1).
- **Content delivery:** local (non-remote) Addressables groups for the base five Regions at launch, per `19_ContentPipeline.md` §5; remote content delivery configuration is a Phase 5 (`20_ContentRoadmap.md`) addition, not part of the initial build pipeline.
- **Versioning scheme:** semantic-style `MAJOR.MINOR.PATCH` — `MAJOR` for structural changes (e.g., a new Region set shipping), `MINOR` for content additions within existing structure (new cosmetics, a single new platform behavior), `PATCH` for bug fixes and balance tuning only.

### 3. Release Checklist (Gate Before Any Public Build)
1. All five `30_QATesting.md` §3 critical scenarios pass on the current build.
2. Performance validated against `31_PerformanceBudget.md`'s minimum supported device tier — stable frame rate during active platforming, no memory-related crashes during extended play sessions.
3. Full playthrough of all currently-shipped Regions (`12_Regions.md`) with no placeholder art, audio, or text remaining (per `20_ContentRoadmap.md`'s phase exit criteria).
4. Localization completeness check for all currently-supported languages (`24_Localization.md`) — no missing-key fallback text visible anywhere in the build.
5. Monetization flows (`22_Monetization.md`) tested end-to-end in sandbox/test-purchase mode: cosmetic purchase, Second Wind revive opt-in and decline, all with correct save-state outcomes.
6. Save/load integrity re-verified per `30_QATesting.md` §3 item 1, specifically on the target release build configuration (not just editor testing).
7. Store listing assets and copy reviewed for tone consistency with `03_Lore.md`/`28_Glossary.md`'s established voice.

### 4. Release Cadence
- Major content releases follow `20_ContentRoadmap.md`'s phase structure — a release is not scheduled by calendar date alone but by phase-completion readiness (Section 3 checklist passing).
- Patch releases (bug fixes, balance-only tuning changes per `21_Balancing.md`) may ship independently of the phase cadence as needed, without requiring the full Section 3 checklist's content-completeness items (1, 3, 4) if those are unaffected by the patch's scope — but must still pass the QA and save-integrity items (1, 2, 6).

### 5. Rollback Policy
- Any release found post-launch to violate a `13_SaveSystem.md` integrity guarantee (data loss or corruption) is treated as a critical-severity issue warranting an expedited patch release, bypassing the standard cadence — consistent with `00_Vision.md`'s trust-first principle applied to release operations.

## Future Expansion
- **Automated CI build pipeline** (build-on-commit, automated QA suite execution per `30_QATesting.md` §1) once team size/production velocity justifies the tooling investment.
- **Staged rollout** (releasing to a small player percentage before full rollout) once the player base is large enough to make staged rollout data meaningful.

## Notes
- This document's Release Checklist (Section 3) is binding — no build may be published to a public store listing without every applicable item confirmed complete.
- Any deviation from the versioning scheme (Section 2) requires a documented reason at release time, to keep the version history meaningful for the team long-term.
