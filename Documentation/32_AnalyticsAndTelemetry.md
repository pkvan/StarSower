# 32 — Analytics & Telemetry (Proposed Addition)

## Purpose
This document defines what StarSower measures about live play, why, and the privacy/trust constraints governing that measurement. It was proposed as an addition because `17_EventSystem.md`, `20_ContentRoadmap.md`, and `21_Balancing.md` all reference a telemetry system that needed its own scope and rules defined explicitly, especially given `00_Vision.md`'s trust-first stance.

## Goals
- Define a minimal, purpose-driven telemetry scope that informs design decisions without becoming invasive data collection.
- Tie every tracked event to a specific design or balancing question it answers.
- Ensure telemetry integration never compromises the trust principles established in `22_Monetization.md` and `00_Vision.md`.

## Principles
- **Measure to improve the game, not to monetize attention.** Telemetry exists to answer design questions (`21_Balancing.md`), not to build player profiles for advertising.
- **Minimal, purposeful collection.** Every tracked event must answer a specific, named question — no speculative "track everything just in case" data collection.
- **Fully decoupled from gameplay code.** Per `17_EventSystem.md` §5, telemetry listens to event channels; gameplay code never calls an analytics SDK directly.
- **Transparent to the player.** Data collection practices are disclosed clearly in a privacy policy, consistent with platform requirements and `00_Vision.md`'s trust-first stance.

## Detailed Design

### 1. What Is Tracked and Why
| Event | Question It Answers | Owning Document |
|---|---|---|
| Run started/ended (with Region reached, height, duration) | Where do players struggle or disengage? Is average session length matching the 60–180s target? | `01_Gameplay.md` §7, `21_Balancing.md` |
| Fail location (Region + approximate platform-sequence position) | Are specific platform behaviors or gap configurations causing disproportionate fails? | `11_Platforms.md`, `21_Balancing.md` §6 |
| Star Fragment collection rate per run | Is fragment density (`10_Collectibles.md` §2) tuned appropriately per Region? | `21_Balancing.md` §4 |
| Beacon/cosmetic unlock timing | Is the economy curve (`21_Balancing.md` §4) actually reachable within the intended session count? | `09_Progression.md`, `21_Balancing.md` |
| Second Wind (rewarded-ad revive) opt-in rate | Is the optional revive framed appropriately — neither ignored nor over-relied-upon? | `22_Monetization.md` §1.2 |
| Achievement unlock rate | Are achievements (`23_Achievements.md`) appropriately paced, not trivially easy or frustratingly rare? | `23_Achievements.md` |

### 2. What Is Explicitly Not Tracked
- No personally identifiable information beyond what is strictly required for platform store compliance (purchase receipts, crash reports).
- No cross-app or third-party ad-network behavioral tracking beyond what the single rewarded-ad SDK (`22_Monetization.md` §1.2) minimally requires to function.
- No granular per-input tracking (e.g., raw joystick coordinates) — only meaningful, aggregated gameplay events per Section 1.

### 3. Technical Integration
Per `17_EventSystem.md` §5, telemetry subscribes to the same ScriptableObject event channels used by gameplay/UI/audio systems (fail state, fragment collected, Region entered, unlocks) rather than requiring bespoke tracking calls scattered through gameplay code. This keeps telemetry fully removable/mockable for QA builds without touching gameplay logic, and ensures new trackable moments are added by wiring a new listener to an existing or new channel, not by modifying `PlayerController`, `PlatformSpawner`, or other core systems.

### 4. Use of Data
- Aggregated data informs `21_Balancing.md` tuning decisions and `20_ContentRoadmap.md` Phase 5 post-launch prioritization (`25_RiskAnalysis.md` §"Ongoing monitoring" items across multiple risks reference this data source).
- Data is never used to implement dynamic, individualized difficulty manipulation designed to pressure spending (a common predatory mobile-game pattern) — this is a hard exclusion consistent with `22_Monetization.md`'s permanent constraints.

## Future Expansion
- **Cohort-based balancing analysis** (comparing fail rates across player skill segments) once sufficient post-launch data volume exists.
- **Opt-out control** in Settings (`06_UIUX.md` §5) for players who prefer not to share aggregated gameplay analytics, respecting platform privacy requirements and the trust-first principle.

## Notes
- Any new tracked event must be added to Section 1's table with an explicit design question it answers before implementation — untracked-purpose data collection is not permitted under this document's principles.
- This document must be reviewed against current platform (App Store/Play Store) privacy requirements before each major release, as those requirements evolve independently of this bible.
