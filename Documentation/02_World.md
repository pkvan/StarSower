# 02 — World

## Purpose
This document defines the physical and thematic structure of the StarSower game world: what the world *is*, how it is organized spatially, and how its state changes as the player progresses. It bridges `03_Lore.md` (why the world is this way) and `12_Regions.md` (the concrete level-design breakdown).

## Goals
- Establish a single, consistent spatial model for the world that all Regions must fit into.
- Define how the world visually and mechanically communicates the restoration of light.
- Ensure the world structure supports endless/procedural climb without feeling repetitive or arbitrary.

## Principles
- **The world is vertical, not open.** There is one axis of meaningful traversal: up. This is a deliberate constraint that keeps mobile sessions focused and readable.
- **The world remembers.** Persistent, cross-run world state (the brightening sky) is a core pillar — see `09_Progression.md`.
- **Environment tells story, dialogue does not.** See `03_Lore.md` and `04_Characters.md` for the no-dialogue policy.
- **Every Region is a chapter, not a reskin.** Visual and mechanical identity must change meaningfully between Regions (see `12_Regions.md`).

## Detailed Design

### 1. The World Model: The Skyless Climb
StarSower's world is a single, continuous vertical column of sky, divided into five named Regions (full breakdown in `12_Regions.md`). The player always starts at the bottom of the current run and climbs upward; there is no backtracking, no branching paths, and no horizontal world map. This single-axis model is intentional:
- It matches the one-thumb, portrait-mobile control scheme (see `01_Gameplay.md`).
- It keeps camera logic simple and legible (see `14_TechnicalArchitecture.md`).
- It reinforces the thematic idea of ascension — literally rising out of a fallen world toward the light.

### 2. The Two Layers of World State
StarSower's world exists on two layers that must never be confused in design or implementation:

1. **The Run Layer (ephemeral):** the specific sequence of platforms, hazards, and Star Fragments generated for a single run. This layer resets every run (see `11_Platforms.md`, `19_ContentPipeline.md` for procedural generation rules).
2. **The Persistent Layer (the Hub World):** the state of the world *between* runs — how bright the sky is, which Beacons are unlocked, which cosmetic elements have been restored to the Hub screen. This layer only ever grows brighter/fuller; it never regresses (see `09_Progression.md`, `13_SaveSystem.md`).

This separation ensures failure in the Run Layer never damages the Persistent Layer — a direct mechanical expression of "the light dims, but does not go out" (see `00_Vision.md`).

### 3. The Hub
Between runs, the player exists in a calm, non-interactive-menu space called **the Hub** — visually, a small starlit vista that brightens over time as Starlight accumulates. The Hub is not a separate gameplay space with its own controls; it is effectively the game's main menu, reskinned as part of the world rather than as a traditional UI screen (see `06_UIUX.md`). The Hub shows:
- The current Sky Restoration Meter (see `09_Progression.md`).
- Access to Beacons (unlocked alternate starting heights, see `12_Regions.md`).
- Access to cosmetic unlocks (see `10_Collectibles.md`, `22_Monetization.md`).

### 4. Environmental Storytelling Rules
Because StarSower has no dialogue system (see `04_Characters.md`), the world must carry narrative weight visually:
- Each Region's background silhouettes (ruins, dead trees, drifting debris) reference the state of the world described in `03_Lore.md` — e.g., lower Regions show more visual "decay," upper Regions show more restored light.
- Restored light is always depicted as warm pastel glows, never harsh bloom or saturated color — consistent with `05_ArtDirection.md`.
- The world must never contain readable text (signage, books, letters) as a storytelling device — visual composition and lighting carry all narrative weight.

### 5. Scale and Continuity
- Regions are conceptually stacked in a fixed vertical order (see `12_Regions.md`) but are procedurally assembled within each Region's band, so no two runs through the same Region play identically.
- The world has no explicit "floor" or origin point shown to the player — the game begins mid-climb, reinforcing that the player is one of many attempts, not a singular chosen event (ties to `03_Lore.md`).
- There is no explicit numeric altitude shown as diegetic world text; height is only ever communicated at the run-summary screen (see `06_UIUX.md`), keeping the in-run screen clean per `00_Vision.md`.

## Future Expansion
- **Weather/time-of-day variation per Region** to increase replay variety without new geometry — flagged for a post-launch content pass in `20_ContentRoadmap.md`.
- **A visible "world map" of Regions** in the Hub, once enough Regions exist to justify it, without turning the Hub into a traditional menu-heavy screen.
- **Seasonal Hub dressing** tied to live content updates (see `19_ContentPipeline.md`) — strictly cosmetic, never gameplay-altering, to preserve `00_Vision.md`'s "No Feature Bloat" pillar.

## Notes
- Any new Region must be defined first in this document's Region ordering before being detailed in `12_Regions.md` — Regions must not be added ad hoc without updating the world model.
- The Hub must never be allowed to accumulate interactive complexity (shops, multiple sub-menus) that competes with its role as a calm, restorative space — any Hub feature proposal must be checked against `00_Vision.md`'s "Minimal UI" pillar.
