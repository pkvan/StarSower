# 12 — Regions

## Purpose
This document defines the five vertical Regions that make up StarSower's climb, their mechanical parameters, and their narrative role. It is the concrete level-design breakdown of the world model established in `02_World.md` and the lore established in `03_Lore.md`.

## Goals
- Fully specify each Region's visual identity, platform behavior mix, fragment density, and audio identity in one authoritative table and set of entries.
- Ensure Region-to-Region progression is a smooth, well-paced difficulty and mood curve.
- Provide the data shape that `18_ScriptableObjects.md`'s planned `RegionConfig` asset must implement.

## Principles
- **Five Regions, one climb.** Regions are chapters in a single continuous ascent, not separate levels selected from a menu (see `02_World.md`).
- **One new idea per Region.** Each Region after the first introduces exactly one new platform behavior (see `11_Platforms.md`) to keep the learning curve gentle and legible.
- **Difficulty rises through complexity, not unfairness.** Harder Regions have tighter timing and more platform variety, never smaller reachability margins than the tuned jump envelope guarantees (see `08_GameFeel.md`).
- **Every Region must visually justify its place in the light-restoration gradient** defined in `05_ArtDirection.md`.

## Detailed Design

### 1. Region Overview Table

| # | Region (VN) | Region (EN) | New Platform Behavior | Fragment Density | Palette Anchor |
|---|---|---|---|---|---|
| 1 | Vùng Đất Tàn | The Fading Ground | — (Static only; tutorial band) | Low | Muted sage / dust grey |
| 2 | Vùng Mây Xám | The Grey Cloudbelt | Moving Platform | Low–Medium | Blue-grey / pale lavender |
| 3 | Vùng Hoàng Hôn | The Twilight Reach | Breakable Platform | Medium | Dusty orange / soft rose |
| 4 | Vùng Sao Rơi | The Fallen Star Expanse | Vanishing Platform | High | Deep pastel violet / gold flecks |
| 5 | Vùng Thiên Đỉnh | The Zenith | Bounce Platform | Medium (quality over quantity) | Warm white / pale gold |

### 2. Region Entries

#### 2.1 The Fading Ground
- **Role:** The onboarding Region. Every run begins here unless a Beacon (see `09_Progression.md`) has been unlocked for a higher Region.
- **Mechanical focus:** Teaches core movement and jump timing with Static platforms only, generous spacing at the low end of the tuned gap range (see `21_Balancing.md`).
- **Narrative role:** The world at its most forgotten — quiet, still, minimal color (see `03_Lore.md`).
- **Exit condition:** A calm, clearly-lit vertical threshold marks the transition into The Grey Cloudbelt (see `08_GameFeel.md`'s Region-transition feedback rules).

#### 2.2 The Grey Cloudbelt
- **Role:** First complexity increase. Introduces Moving Platforms (`11_Platforms.md` §3.1) at low frequency, mixed generously with Static platforms.
- **Mechanical focus:** Timing a jump onto/off a slowly moving surface; still very forgiving gap tuning.
- **Narrative role:** A hazy, uncertain space — the world beginning to remember, but not yet sure of itself.

#### 2.3 The Twilight Reach
- **Role:** Introduces Breakable Platforms (`11_Platforms.md` §3.2). This is the first Region where the player must actively read platform visuals before committing to a landing.
- **Mechanical focus:** Sequencing jumps so a breakable platform is used as a step, not a resting point; spawn logic guarantees a stable alternative is always within reach (see `11_Platforms.md` §3.2's fairness rule).
- **Narrative role:** The boundary Region — old light and new light meeting; the first Region where the sky's color visibly warms in response to the player's presence.

#### 2.4 The Fallen Star Expanse
- **Role:** Introduces Vanishing Platforms (`11_Platforms.md` §3.3) and the highest fragment density of the base five Regions.
- **Mechanical focus:** Reading timed visibility cycles; the densest, most rewarding collection Region, matching its narrative role.
- **Narrative role:** Where the most stars fell — visually the richest Region prior to the Zenith, dense with drifting fragment light.

#### 2.5 The Zenith
- **Role:** The culmination Region for the base game's vertical slice. Introduces Bounce Platforms (`11_Platforms.md` §3.4) as a joyful, momentum-driven capstone mechanic.
- **Mechanical focus:** Rhythmic, flowing traversal using bounce platforms to chain rises quickly — designed to feel like a reward lap after four Regions of careful platforming.
- **Narrative role:** The highest point yet collectively reached (tied to the persistent Sky Restoration Meter, see `09_Progression.md`); reaching it for the first time in a save file triggers a distinct, one-time celebratory beat (see `08_GameFeel.md`), but does not end or cap further climbing — the Zenith continues proceduraly upward beyond this point using its established rules, consistent with the endless core loop (`01_Gameplay.md`).

### 3. Region Transition Rules
- Transitions occur at fixed height thresholds per Region, tuned in `21_Balancing.md`, not by discrete "level complete" gates — the player never stops moving to trigger a transition.
- Each transition triggers exactly the feedback defined in `08_GameFeel.md`'s Region-transition row (visual wash, musical swell, subtle camera zoom) and nothing more.
- Music and ambience crossfade per `07_Audio.md`'s adaptive layering rules — never a hard audio cut.

### 4. Beacon Placement
Per `09_Progression.md`, one Beacon (alternate run-start point) is associated with the entry threshold of each Region except The Fading Ground (which is always the default start). Beacon unlock requirements are owned by `21_Balancing.md`.

## Future Expansion
- **Region 6 and beyond** past the Zenith are intentionally left undesigned in this document — see `20_ContentRoadmap.md` for the post-launch content cadence and `27_FutureIdeas.md` for early concept notes. Any new Region must follow the "one new platform behavior" pacing rule established here.
- **Region-specific micro-events** (rare, non-hazardous visual moments unique to a Region, e.g., a slow-drifting flock of light motes in The Grey Cloudbelt) as low-risk atmospheric variety additions.

## Notes
- The Region order in Section 1 is fixed and canonical — it must not be reordered without a full revision pass across this document, `02_World.md`, `03_Lore.md`, and `05_ArtDirection.md`, since all four are cross-referenced against this ordering.
- Any new platform behavior introduced in `11_Platforms.md` must be assigned to a specific Region debut here before being considered ready for implementation.
