# 23 — Achievements

## Purpose
This document defines StarSower's achievement system — the philosophy behind what earns recognition, the initial achievement catalog, and the presentation rules that keep achievements feeling like genuine milestones rather than grind-checklist busywork.

## Goals
- Define an achievement catalog that reinforces the narrative and mechanical themes established elsewhere in this bible.
- Ensure achievements reward a mix of narrative milestones, skill expression, and gentle exploration — never repetitive grinding.
- Define presentation rules consistent with `06_UIUX.md`'s minimal-UI philosophy.

## Principles
- **Achievements celebrate meaning, not repetition.** No achievement should be "do X 500 times" — see `00_Vision.md`'s "No Heavy Grinding" pillar.
- **Achievements are never purchasable or skippable.** See `22_Monetization.md` §4.
- **Presentation is calm, not intrusive.** An achievement unlock never interrupts active platforming (see `01_Gameplay.md`'s no-mid-run-interruption rule).

## Detailed Design

### 1. Achievement Categories
1. **Narrative Milestones** — tied to reaching each Region for the first time (`12_Regions.md`), reinforcing the story's structure.
2. **Skill Expression** — tied to specific platforming feats (e.g., clearing a Region without landing on a single Breakable platform, chaining a number of Bounce-platform jumps consecutively).
3. **Restoration Milestones** — tied to Sky Restoration Meter thresholds (`09_Progression.md`), celebrating long-term meta-progression.
4. **Discovery** — tied to collecting a full run's worth of visible Star Fragments in a single Region pass, rewarding thorough, careful play without requiring it.

### 2. Initial Achievement Catalog

| Achievement | Category | Trigger |
|---|---|---|
| First Light | Narrative | Collect your first Star Fragment |
| Beyond the Fading Ground | Narrative | Reach The Grey Cloudbelt for the first time |
| Through the Haze | Narrative | Reach The Twilight Reach for the first time |
| Where the Stars Fell | Narrative | Reach The Fallen Star Expanse for the first time |
| The Long Climb | Narrative | Reach The Zenith for the first time |
| Sure Footed | Skill | Clear The Twilight Reach in a single run without landing on a Breakable platform that crumbles beneath you |
| Rhythm of Light | Skill | Chain a defined number of consecutive Bounce-platform jumps without touching a Static platform |
| A Brighter Sky | Restoration | Reach the first Sky Restoration Meter milestone |
| The Sky Remembers | Restoration | Reach the final base-game Sky Restoration Meter milestone |
| Nothing Left Behind | Discovery | Collect every visible Star Fragment in a single Region pass |

Exact skill-based thresholds (Section 2 rows marked "Skill") are tuning values owned by `21_Balancing.md`, not fixed permanently in this table.

### 3. Presentation Rules
- Achievement unlocks during a run are acknowledged with a small, non-blocking, low-opacity toast notification (bottom of screen, brief, auto-dismissing) — never a full-screen popup, never pausing gameplay.
- A dedicated Achievements panel, accessible from the Hub (see `06_UIUX.md`), lists all achievements with clear locked/unlocked states — locked achievements show their name and category but may withhold precise trigger details for Skill-category entries to preserve a light sense of discovery, without ever being deliberately cryptic or confusing.
- No achievement is ever retroactively removed or renamed once shipped, to preserve player trust in their save record.

### 4. Data Model
Each achievement is represented by an `AchievementDefinition` ScriptableObject (see `18_ScriptableObjects.md`), containing an identifier, category, display data, and a reference to its trigger condition (subscribed via the event channel system, `17_EventSystem.md`, where applicable). Unlocked state is persisted per `13_SaveSystem.md`.

## Future Expansion
- **Platform-native achievement integration** (Google Play Games / Game Center achievement services) as a straightforward launch-readiness task once the base catalog (Section 2) is finalized.
- **Post-launch achievement additions** tied to new Regions (`20_ContentRoadmap.md`) — new entries always follow the four-category structure in Section 1; a fifth category must be explicitly justified before being introduced.

## Notes
- Any new achievement proposal must be checked against the "no repetition-grinding" principle before being added to the catalog — a proposal requiring a large repeated count of an ordinary action should be redesigned around a qualitative feat instead.
- This document's catalog (Section 2) must stay in sync with `13_SaveSystem.md`'s `UnlockedAchievements` field and `18_ScriptableObjects.md`'s `AchievementDefinition` entry.
