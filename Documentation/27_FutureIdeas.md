# 27 — Future Ideas

## Purpose
This document is the holding space for speculative, unscoped concepts that are interesting but not yet validated against production readiness. It exists to protect `26_Backlog.md` from scope creep while ensuring good ideas are not lost.

## Goals
- Capture promising ideas raised across other documents' "Future Expansion" sections in one browsable location.
- Keep clear separation between "idea worth remembering" and "committed scope" (see `26_Backlog.md`).
- Provide a lightweight gate (the Vision filter) that any idea must pass before being promoted to the roadmap.

## Principles
- **Ideas here are not promises.** Nothing in this document is committed; inclusion here means "worth revisiting," not "will ship."
- **Every idea must be traceable to a system or theme already established in this bible.** Wholly new, unrelated concepts should be evaluated against `00_Vision.md` before even being added here.
- **Promotion requires the same rigor as any backlog item.** An idea only moves to `26_Backlog.md` once it is fully scoped against its owning document(s), per that document's Future Expansion process.

## Detailed Design

### 1. Gameplay Ideas
- **Double Jump / Air Dash as Beacon-unlockable rewards** (see `01_Gameplay.md` Future Expansion) — optional, never required, to preserve accessibility.
- **Combined platform behaviors** (e.g., moving + breakable) once base behaviors are individually validated (`11_Platforms.md` Future Expansion).
- **Region-exclusive signature platform** for the Zenith as a capstone treat (`12_Regions.md` Future Expansion).
- **Assist Mode** (generous dead zone / slower fall speed) as a motor-accessibility option (`01_Gameplay.md`, `24_Localization.md`).

### 2. World & Narrative Ideas
- **Regions beyond the Zenith** — a "beyond the sky" concept extending the climb past the base five Regions (`12_Regions.md` Future Expansion, `20_ContentRoadmap.md` Phase 5).
- **Optional "Archive" lore unlockable** in the Hub, letting curious players read a condensed version of `03_Lore.md`'s myth, entirely opt-in (`03_Lore.md` Future Expansion).
- **Cameo silhouettes of other Star Sowers** in far-background upper Regions, reinforcing the "not alone across time" theme without introducing a new character system (`04_Characters.md` Future Expansion).
- **Weather/time-of-day variation per Region** for replay variety without new geometry (`02_World.md` Future Expansion).

### 3. Meta-Progression & Economy Ideas
- **Rare "Fallen Star" variant fragments** worth more Starlight, placed sparingly in the Fallen Star Expanse (`10_Collectibles.md` Future Expansion) — must remain a variant of the existing single currency, not a new one.
- **Fourth cosmetic slot** for Đóm Sao's color (`10_Collectibles.md` Future Expansion).
- **Cloud save / cross-device sync** via platform account services (`13_SaveSystem.md` Future Expansion).

### 4. Audio/Visual Ideas
- **Dynamic instrument unlocks** tied to major Sky Restoration Meter milestones, permanently enriching the Hub theme (`07_Audio.md` Future Expansion).
- **Player-triggered ambient Hub chimes**, a low-cost comfort feature (`07_Audio.md` Future Expansion).
- **Colorblind mode palette variant** (`06_UIUX.md` Future Expansion).

### 5. Technical Ideas
- **Editor tooling for RegionConfig/PlatformBehaviorConfig validation** against the reachability guarantee (`18_ScriptableObjects.md` Future Expansion).
- **Runtime config override for QA/debug builds** (`18_ScriptableObjects.md` Future Expansion).
- **Formal asmdef boundaries** enforcing the Dependency Direction diagram at compile time (`14_TechnicalArchitecture.md` Future Expansion).

## Future Expansion
This document is itself a "future expansion" holding space and does not have its own further expansion section beyond continuing to absorb new ideas as they're raised across the bible.

## Notes
- When an idea from this document is promoted to `26_Backlog.md`, it should be removed from here (or marked promoted) to avoid the two documents drifting out of sync.
- New ideas should be added to the relevant category above, with a citation back to the design document section that inspired them, keeping this document's ideas traceable rather than free-floating.
