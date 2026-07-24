# 19 — Content Pipeline

## Purpose
This document defines how content — art, audio, prefabs, and data — moves from creation to in-game delivery, including the planned Addressables-based asset management strategy. It exists to keep content production scalable across a long-running project with recurring Region and cosmetic content.

## Goals
- Define the asset delivery strategy appropriate for a mobile game with memory and download-size constraints.
- Establish naming and organization conventions for content assets consistent with `15_ProjectStructure.md`.
- Define how new Regions, platform variants, and cosmetics move from design/art production into a shippable build.

## Principles
- **Memory-conscious by default.** Mobile devices, especially low/mid-tier Android, cannot hold all Region content in memory simultaneously — only the current and adjacent Region's assets should be resident at once.
- **Content updates should not require a full app resubmission** wherever feasible — Addressables' remote-content capability is treated as a long-term goal, not a day-one requirement.
- **Consistent naming enables automation.** Predictable asset naming allows tooling (build scripts, Addressables group rules) to categorize content without manual per-asset configuration.

## Detailed Design

### 1. Current State
Platform prefabs (`Assets/Prefabs/Platform_Basic.prefab`, `Platform_Wide.prefab`) are currently referenced directly by `PlatformSpawner` (see `14_TechnicalArchitecture.md`). This is appropriate at the project's current single-Region scale and is the correct, simple starting point — no premature Addressables setup has been introduced.

### 2. Planned Addressables Migration
As Region content (`12_Regions.md`) and cosmetic content (`10_Collectibles.md`) grow, assets migrate to Addressables-managed groups organized as follows:

| Addressables Group | Contents | Load Trigger |
|---|---|---|
| `Core_Always` | Player character, UI widgets, always-needed shared VFX/SFX | App launch |
| `Region_<Name>` (one per Region) | Platform variant prefabs, background art, ambience/music tracks, Region-specific particle sets | Approaching that Region's height threshold; unloaded once sufficiently far below it |
| `Cosmetics_<Category>` | Trim/trail/flourish visual assets | On-demand when opening the Cosmetics panel, or on equip |
| `Audio_Shared` | UI sounds, universal SFX (jump, land, collect — see `07_Audio.md`) | App launch |

Region groups are loaded slightly ahead of the camera reaching that Region's threshold (a small predictive buffer) and released once the player and camera are sufficiently far past a Region to never return to it in the same run — consistent with the one-directional vertical world model in `02_World.md`.

### 3. Asset Naming Conventions
- Platform prefabs: `Platform_<Region>_<Variant>` once Region-specific variants exist (e.g., `Platform_TwilightReach_Breakable`), extending the existing `Platform_Basic`/`Platform_Wide` naming pattern.
- ScriptableObject config assets: match the type names defined in `18_ScriptableObjects.md` (e.g., `RegionConfig_FadingGround`).
- Audio assets: `SFX_<Event>` / `Music_<Region>_<Layer>` / `Ambience_<Region>`, matching the layered music model in `07_Audio.md`.
- Cosmetic assets: `Cosmetic_<Slot>_<Name>`, matching the slot categories in `10_Collectibles.md`.

### 4. Content Production Flow
1. **Design spec:** a new Region, platform variant, or cosmetic is fully specified in its owning design document (`12_Regions.md`, `11_Platforms.md`, `10_Collectibles.md`) before art/audio production begins.
2. **Art/audio production:** assets are produced against the palette/shape/sound rules in `05_ArtDirection.md` and `07_Audio.md`.
3. **Config authoring:** a designer creates or updates the relevant ScriptableObject config asset (`18_ScriptableObjects.md`) referencing the new assets.
4. **Addressables grouping:** the asset is placed in its correct group per Section 2's table.
5. **Validation:** the new content is checked against `11_Platforms.md`'s Fairness Validation Checklist (for platforms) or the relevant document's acceptance criteria before being merged into the content roadmap (`20_ContentRoadmap.md`).

### 5. Build Size & Download Considerations
- Initial release ships all five base Regions (`12_Regions.md`) as local (non-remote) Addressables content, keeping the base install simple; remote-content delivery is a Future Expansion item once post-launch Region content (`20_ContentRoadmap.md`) makes app-size growth a concern.
- Audio assets use appropriate mobile compression settings per `07_Audio.md`'s technical constraints, coordinated through this pipeline's Addressables group settings rather than per-asset ad hoc configuration.

## Future Expansion
- **Remote Addressables content delivery** for post-launch Regions/cosmetics without requiring a full app store update — the primary long-term payoff of adopting Addressables early.
- **Automated Addressables group validation** (a build-time check that no asset is missing its expected group) once content volume makes manual review error-prone.

## Notes
- This document's Addressables group table must be kept in sync with `12_Regions.md`'s Region list and `10_Collectibles.md`'s cosmetic catalog — a new Region or cosmetic category is not considered fully specified until it has an entry here.
- Migrating an asset from direct reference to Addressables must never change its runtime behavior from a design perspective — this is purely a delivery/memory optimization and must be validated as behaviorally invisible to the player.
