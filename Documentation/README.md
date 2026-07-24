# StarSower — Design Bible

**"Every jump brings light."**

This is the official internal design and technical documentation set for **StarSower**, a 2D vertical platformer for mobile (Unity 6 / URP). Every document below is written as production-ready reference material — not brainstorming, not a placeholder. `00_Vision.md` is the root authority; every other document must remain consistent with it, and with each other.

## How to Use This Bible
- Start with `00_Vision.md` if you are new to the project — it defines the one sentence every decision is tested against.
- Each document follows the same structure: **Purpose, Goals, Principles, Detailed Design, Future Expansion, Notes.**
- Cross-references between documents (e.g., "see `11_Platforms.md`") are load-bearing — they indicate a real dependency, not a casual pointer.
- `29_DesignPhilosophy.md` has final interpretive authority for resolving ambiguity between documents, subordinate only to `00_Vision.md`.

## Document Index

### Foundation
- [00_Vision.md](00_Vision.md) — Why StarSower exists; the filter question for every feature.
- [01_Gameplay.md](01_Gameplay.md) — Core loop, controls, fail condition.
- [02_World.md](02_World.md) — Spatial/world model, the Hub, persistent vs. run-layer state.
- [03_Lore.md](03_Lore.md) — The myth of the fallen stars; narrative rules (no dialogue).
- [04_Characters.md](04_Characters.md) — The Star Sower, Đóm Sao, and why the cast stays minimal.

### Craft
- [05_ArtDirection.md](05_ArtDirection.md) — Pastel palette philosophy, shape language, lighting.
- [06_UIUX.md](06_UIUX.md) — Screens, HUD, minimal-UI interaction rules.
- [07_Audio.md](07_Audio.md) — Adaptive music, sound design, mix hierarchy.
- [08_GameFeel.md](08_GameFeel.md) — Movement/jump/camera feel targets, feedback table.

### Systems & Content
- [09_Progression.md](09_Progression.md) — Starlight, Sky Restoration Meter, Beacons.
- [10_Collectibles.md](10_Collectibles.md) — Star Fragments, cosmetic catalog.
- [11_Platforms.md](11_Platforms.md) — Static/Moving/Breakable/Vanishing/Bounce platform design.
- [12_Regions.md](12_Regions.md) — The five Regions of the climb, full breakdown.

### Technical
- [13_SaveSystem.md](13_SaveSystem.md) — What's saved, integrity guarantees.
- [14_TechnicalArchitecture.md](14_TechnicalArchitecture.md) — System map, dependency rules, current + planned architecture.
- [15_ProjectStructure.md](15_ProjectStructure.md) — Folder structure and ownership.
- [16_CodingGuidelines.md](16_CodingGuidelines.md) — Binding engineering standards and process discipline.
- [17_EventSystem.md](17_EventSystem.md) — Decoupled communication, event channel architecture.
- [18_ScriptableObjects.md](18_ScriptableObjects.md) — Data-driven config asset catalog.
- [19_ContentPipeline.md](19_ContentPipeline.md) — Addressables strategy, asset naming, production flow.

### Planning & Balance
- [20_ContentRoadmap.md](20_ContentRoadmap.md) — Phased production plan.
- [21_Balancing.md](21_Balancing.md) — Tuning methodology and economy math.
- [26_Backlog.md](26_Backlog.md) — Concrete, phase-organized task backlog.
- [27_FutureIdeas.md](27_FutureIdeas.md) — Unscoped, speculative ideas.

### Business & Reach
- [22_Monetization.md](22_Monetization.md) — Cosmetic IAP, Second Wind revive, permanent no-pay-to-win constraints.
- [23_Achievements.md](23_Achievements.md) — Achievement catalog and presentation rules.
- [24_Localization.md](24_Localization.md) — Language support and accessibility baseline.

### Reference
- [25_RiskAnalysis.md](25_RiskAnalysis.md) — Named risks and their mitigations.
- [28_Glossary.md](28_Glossary.md) — Canonical terminology.
- [29_DesignPhilosophy.md](29_DesignPhilosophy.md) — The reasoning pattern behind every decision in this bible.

### Proposed Additions (beyond the original 30-file brief)
- [30_QATesting.md](30_QATesting.md) — QA strategy and critical release-gate test scenarios.
- [31_PerformanceBudget.md](31_PerformanceBudget.md) — Mobile performance targets and per-system budget.
- [32_AnalyticsAndTelemetry.md](32_AnalyticsAndTelemetry.md) — Minimal, purpose-driven telemetry scope.
- [33_BuildAndRelease.md](33_BuildAndRelease.md) — Build configuration and release checklist.

## Governing Rule
Every document must serve, and never contradict, the thesis established in `00_Vision.md`:

> **Every jump brings light.**
