# 18 — ScriptableObjects

## Purpose
This document defines StarSower's data-driven configuration strategy using Unity ScriptableObjects — what config assets exist (or are planned), what they contain, and the rules for when a value belongs in a ScriptableObject versus a plain `[SerializeField]` on a component.

## Goals
- Establish a consistent pattern for design-owned tunable data that scales across many Regions, platform variants, and cosmetics without code changes.
- Prevent duplicated or drifting tuning values across multiple scene instances of the same conceptual thing.
- Define the boundary between "this belongs on a component" and "this belongs in a ScriptableObject."

## Principles
- **Data that designers iterate on independent of code belongs in a ScriptableObject.** If tuning a value requires no code change but currently requires opening a scene/prefab, it's a candidate.
- **Data shared by many instances belongs in a ScriptableObject.** If ten platform prefabs should share one gap-tuning value, that value should live in one asset, not ten duplicated fields.
- **ScriptableObjects hold data and configuration, not runtime mutable state**, except where a deliberate shared-state pattern (e.g., a runtime event channel, see `17_EventSystem.md`) is intentionally used.
- **Every ScriptableObject type has one clear owner document** (this file lists them; their meaning is defined by the relevant design document).

## Detailed Design

### 1. Current State (Honest Baseline)
As of this document's writing, StarSower's tunable values (jump force, move speed, platform gap ranges, camera dead zone/smooth time, fail distance) live as `[SerializeField]` fields directly on their owning components (`PlayerMotor`, `PlatformSpawner`, `CameraFollowY`, `GameOverManager` — see `14_TechnicalArchitecture.md`). This is appropriate at the project's current scale (few instances, single Region) and is not treated as technical debt requiring urgent fixing — it is the correct starting point per `16_CodingGuidelines.md`'s "no unprompted refactor" rule. The ScriptableObject migration below is planned, additive work, not a retroactive correction.

### 2. Planned Config Asset Catalog

| Asset Type | Purpose | Consumed By | Defined By |
|---|---|---|---|
| `RegionConfig` | Per-Region palette anchor reference, platform behavior mix/frequency, fragment density, music/ambience set reference | `PlatformSpawner`, Region-transition system, `07_Audio.md` audio layer selection | `12_Regions.md` |
| `PlatformBehaviorConfig` | Per-platform-type tuning (breakable delay, vanish cycle timing, bounce force, move-platform path/speed) | The relevant platform behavior component (see `11_Platforms.md` §3) | `11_Platforms.md`, `08_GameFeel.md` |
| `PlayerMovementConfig` | Canonical jump force, move speed, coyote time, jump buffer window | `PlayerMotor`, `GroundChecker` | `08_GameFeel.md`, `21_Balancing.md` |
| `CameraFeelConfig` | Dead zone size, smooth time, default shake/zoom presets for common moments | `CameraFollowY`, `CameraShake`, `CameraZoom` | `08_GameFeel.md` |
| `CosmeticDefinition` | Identifier, display data, Starlight cost, unlock visuals for a single cosmetic item | Cosmetics UI panel, `13_SaveSystem.md` unlock records | `10_Collectibles.md` |
| `AchievementDefinition` | Identifier, trigger condition reference, display data | Achievement tracking system | `23_Achievements.md` |

### 3. Design Rules for New Config Types
Before introducing a new ScriptableObject type:
1. Confirm it represents *design-owned, frequently-iterated* data — engineering-owned runtime state does not qualify.
2. Confirm it is consumed by more than one instance, or is a single canonical source of truth designers need to tune without touching a scene (a single-instance "global config" asset is still valid under this rule).
3. Name it `[Concept]Config` or `[Concept]Definition` consistently with the table above.
4. Add it to the catalog table in this document in the same change that introduces it.

### 4. Relationship to `[SerializeField]` Component Fields
Migrating a value from a component field to a ScriptableObject reference does not remove the component's `[SerializeField]` — the component instead exposes a `[SerializeField]` reference *to the config asset*. This preserves Inspector-friendliness (a designer can still see and reassign which config an instance uses) while centralizing the actual tuning values, per `16_CodingGuidelines.md`'s Inspector rules.

### 5. Relationship to Addressables
Config assets that reference art/audio (e.g., `RegionConfig`'s music/ambience set) hold Addressable references rather than direct hard references once the migration described in `19_ContentPipeline.md` is underway, so a Region's content can be loaded/unloaded independently of the config asset itself.

## Future Expansion
- **Editor tooling** (a custom inspector or editor window) for previewing/validating `RegionConfig` and `PlatformBehaviorConfig` combinations against the reachability guarantee in `11_Platforms.md`, once enough Regions exist to make manual validation error-prone.
- **Runtime config override for QA/debug builds**, allowing testers to swap in experimental tuning without rebuilding — flagged for `25_RiskAnalysis.md`/QA tooling discussion.

## Notes
- This document is the catalog and rulebook for ScriptableObject usage; the *values* inside any given config asset are owned by the relevant design document listed in the table, not by this document.
- No config asset should ever contain narrative text intended for player-facing display in a language-specific way without going through the localization key system defined in `24_Localization.md`.
