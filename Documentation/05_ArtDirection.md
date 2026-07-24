# 05 — Art Direction

## Purpose
This document defines the visual identity of StarSower: palette philosophy, shape language, lighting rules, and Region-by-Region visual progression. It ensures every asset produced — by any artist, at any point in development — reads as part of the same coherent world.

## Goals
- Lock a pastel, fantasy, emotionally soft visual identity consistent with `00_Vision.md`.
- Define concrete, actionable rules (not just mood words) for color, shape, and lighting.
- Ensure visual restoration of light is legible at a glance, reinforcing the core theme mechanically and aesthetically at once.
- Keep the art pipeline mobile-performance-appropriate (see `14_TechnicalArchitecture.md`).

## Principles
- **Pastel, never saturated.** StarSower's palette avoids high-saturation "candy" colors and pure black/white extremes — everything sits in a soft, muted-but-warm register.
- **Light is the main character.** Every visual decision should ask: "does this make light feel more precious?"
- **Silhouette clarity above detail.** Readability at small mobile screen sizes always wins over ornamentation.
- **Restraint over spectacle.** StarSower is calm, not flashy — visual effects support feeling, they do not compete for attention (see `08_GameFeel.md`).

## Detailed Design

### 1. Palette Philosophy
StarSower uses a **desaturation gradient tied to progression**, not a fixed palette:
- **Lower Regions** (`The Fading Ground`, `The Grey Cloudbelt`): heavily desaturated pastels — muted sage greens, dusty blue-greys, faded lavender. Contrast is intentionally low to convey a world that has forgotten color.
- **Middle Regions** (`The Twilight Reach`): a warming gradient begins — dusty orange, soft rose, pale gold enter the palette, always still pastel (never neon or saturated).
- **Upper Regions** (`The Fallen Star Expanse`, `The Zenith`): the fullest, brightest pastel palette in the game — soft gold, warm white, gentle violet — but "bright" in StarSower always means luminous and soft, never harsh or high-contrast.
- **The player character and Star Fragments are always the warmest, most saturated element on screen**, regardless of Region, so they never get lost against the background (a hard rule, not a suggestion).

Reference palette values are maintained in the shared art palette asset, not hardcoded per-Region — see `18_ScriptableObjects.md` for the planned `RegionConfig` data structure that will drive this systematically.

### 2. Shape Language
- **The world (platforms, background silhouettes):** soft, rounded rectangles and organic blob shapes — nothing sharp-edged or industrial. This applies even to "broken" or "fading" platform variants (see `11_Platforms.md`) — decay is shown through color and particle effects, not jagged geometry.
- **The player character and companion:** simple, rounded silhouettes built from few large shapes, prioritizing instant readability over anatomical detail.
- **UI elements:** rounded, soft-edged, low-ornamentation shapes consistent with the world's shape language — UI must never look like it was pulled from a generic mobile game template (see `06_UIUX.md`).

### 3. Lighting Rules
- Light sources in StarSower are always warm (never cold blue-white); even "moonlight" style ambient lighting in lower Regions leans toward a muted warm-grey rather than cold blue, to keep the world feeling melancholic rather than cold or hostile.
- Glow/bloom is used sparingly and only on diegetically "lit" objects: the player's trim, Star Fragments, Đóm Sao, and Region-transition thresholds. Bloom must never be applied to full-screen post-processing in a way that reduces gameplay readability.
- Light intensity is one of the primary tools for representing meta-progression: as the Sky Restoration Meter (see `09_Progression.md`) grows, the Hub and menu backgrounds gain measurably more ambient light and warmth across sessions — a slow, persistent visual reward independent of any single run.

### 4. Region Visual Identity Summary
(Full mechanical detail in `12_Regions.md`; this section governs visual treatment only.)

| Region | Palette Anchor | Mood |
|---|---|---|
| The Fading Ground | Muted sage / dust grey | Quiet, forgotten, still |
| The Grey Cloudbelt | Blue-grey / pale lavender | Uncertain, hazy, transitional |
| The Twilight Reach | Dusty orange / soft rose | Warming, hopeful tension |
| The Fallen Star Expanse | Deep pastel violet / gold flecks | Wonder, density, discovery |
| The Zenith | Warm white / pale gold | Arrival, culmination, calm triumph |

### 5. Character Rendering Rules
- The Star Sower and Đóm Sao (see `04_Characters.md`) always render at full palette warmth regardless of Region, using a subtle rim-light shader treatment to keep them separated from any background, even in the busiest upper Regions.
- No character ever casts a harsh, high-contrast shadow — shadows are soft, short, and warm-toned, consistent with the game's non-threatening tone.

### 6. Mobile Performance Constraints
- Sprite-based 2D art only; no real-time dynamic lighting beyond URP 2D Renderer's Light2D for controlled, designer-placed glow sources (consistent with the `Global Light 2D` already established in the project's rendering setup).
- Particle effects (star sparkle, ash drift, fragment collection bursts) must use small, pooled particle counts appropriate for low/mid-tier Android devices — see `14_TechnicalArchitecture.md` for the Object Pool requirement this implies.
- Background art uses layered parallax sprites, not per-pixel dynamic effects, to keep draw calls predictable across Regions.

## Future Expansion
- A formalized **shared color-ramp asset** (ScriptableObject-driven, see `18_ScriptableObjects.md`) to guarantee designers and artists never hand-pick divergent hex values per Region.
- Consideration of a subtle "dynamic weather" pass (see `02_World.md` Future Expansion) once the base five-Region palette system is fully validated.

## Notes
- Any new visual asset (platform skin, cosmetic trim, background element) must be checked against the Region palette table in this document before being approved for production.
- Saturation, once established per Region, must never spike upward for a single "flashy" effect (e.g., a rare collectible) — rarity and celebration are communicated through motion and light *intensity*, not saturation, to preserve the pastel identity (see `08_GameFeel.md` for celebration-feel guidelines).
