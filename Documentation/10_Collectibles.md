# 10 — Collectibles

## Purpose
This document defines Star Fragments — StarSower's sole in-run collectible — and the rules governing their placement, value, and visual/audio identity. It also defines the cosmetic unlock catalog they ultimately fund.

## Goals
- Fully specify the one collectible type in the base game and explain why StarSower intentionally has only one.
- Define fair, readable placement rules that keep collection feeling like an extension of platforming, not a separate systemic layer.
- Connect collectible value directly to `09_Progression.md`'s currency model.

## Principles
- **One collectible, deeply integrated.** StarSower avoids collectible bloat (multiple currencies, gacha-style rarity tiers) in favor of one meaningful, well-realized collectible tied directly to the theme.
- **Collection should never require detours.** Fragments are placed to reward good platforming lines, not to force the player off the natural path (see `11_Platforms.md`, `19_ContentPipeline.md`).
- **Value is visible, not hidden.** The player should always be able to tell, at a glance, roughly how many fragments they've collected without opening a menu (see `06_UIUX.md`).

## Detailed Design

### 1. Star Fragments
- **Narrative identity:** dormant pieces of forgotten hope, as defined in `03_Lore.md`. Collecting one is the literal, mechanical embodiment of "every jump brings light."
- **Visual identity:** a small, warm, softly-pulsing point of light (see `05_ArtDirection.md` — always rendered at full palette warmth regardless of Region). Never uses gem, coin, or currency iconography — it must read as "light," not as "money."
- **Collection method:** automatic on overlap/contact with the player — no separate "grab" input, consistent with the one-thumb control philosophy (see `01_Gameplay.md`).
- **Feedback on collection:** per the feedback table in `08_GameFeel.md` — bright flash, chime, brief character glow pulse, and a subtle brightening of Đóm Sao (`04_Characters.md`).

### 2. Placement Rules
- Fragments are placed in relation to the platform layout generated for a run (see `11_Platforms.md`, `19_ContentPipeline.md`), always along a reachable line consistent with the same jump-arc guarantees used for platform spawn placement — a fragment must never require a jump the base moveset cannot achieve.
- Fragment density increases gradually by Region (see `12_Regions.md`), consistent with the narrative idea that upper Regions ("The Fallen Star Expanse" especially) contain a denser concentration of fallen light.
- Occasional fragments are placed slightly off the safest/most direct line (but always within safe jump range) to reward players who explore small variations in their platforming line, without ever requiring risky detours to achieve full collection.
- No fragment is ever placed in a position that requires backtracking — consistent with the one-directional vertical world model (see `02_World.md`).

### 3. Value & Conversion
- All Star Fragments collected in a run convert to Starlight at run end, per the model defined in `09_Progression.md`. Exact conversion rate is a balancing value, not a design-fixed constant (see `21_Balancing.md`).
- There is no in-run "spending" of fragments — they exist purely as a running tally until conversion, keeping the in-run mental model simple (see `08_GameFeel.md`, `06_UIUX.md`).

### 4. Cosmetic Catalog (Spent via Starlight, see `09_Progression.md`)
Cosmetics are the sole spend-target for Starlight and are organized into three slots, each independently unlockable:
1. **Cloak Trim Color** — recolors the Star Sower's glowing trim (see `04_Characters.md`).
2. **Trail Effect** — a particle trail following the player during jumps, always in the game's pastel palette family (see `05_ArtDirection.md`) regardless of chosen color.
3. **Landing Flourish** — a small cosmetic visual flourish on landing (e.g., a brief ring of light), purely decorative and never affecting the landing feel/timing defined in `08_GameFeel.md`.

All cosmetics are permanent once unlocked, non-expiring, and never gameplay-affecting — see `22_Monetization.md`.

### 5. Explicit Non-Goals
To prevent scope creep, this document explicitly states StarSower's base design does **not** include:
- Multiple collectible currencies or rarity tiers.
- Randomized loot-box-style cosmetic unlocks.
- Consumable power-up collectibles (speed boosts, extra jumps as a pickup) — any future power-up concept must be evaluated separately against `01_Gameplay.md` and `25_RiskAnalysis.md`, not folded into the fragment system.

## Future Expansion
- **Rare "Fallen Star" variant fragments** (visually distinct, worth more Starlight, placed sparingly in harder-to-reach-but-still-fair positions) — a possible future addition for the Fallen Star Expanse Region specifically, flagged in `20_ContentRoadmap.md`, but must not introduce a new currency, only a higher-value instance of the existing one.
- **Cosmetic slot expansion** (e.g., a fourth slot for companion Đóm Sao color) once the base three-slot catalog is validated with players.

## Notes
- Any new collectible type proposal must justify why it cannot simply be a variant of the existing Star Fragment before being considered, per the "one collectible, deeply integrated" principle.
- Placement rules in Section 2 must be re-validated whenever `11_Platforms.md`'s reachability guarantee logic changes, since fragment placement is derived from platform spawn logic, not independent of it.
