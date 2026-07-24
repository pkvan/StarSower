# 07 — Audio

## Purpose
This document defines the audio identity of StarSower — music direction, sound design philosophy, and the mixing rules that keep audio in service of the game's calm, restorative tone. It is authoritative for any music, SFX, or ambience produced for the project.

## Goals
- Establish a soft, emotionally warm audio identity consistent with `00_Vision.md` and `05_ArtDirection.md`.
- Define concrete rules for how sound reinforces "every jump brings light" without becoming noisy or repetitive over long mobile sessions.
- Define adaptive/layered music rules tied to Region progression (`12_Regions.md`) and meta-progression (`09_Progression.md`).
- Keep audio implementation mobile-appropriate (small footprint, low battery/CPU impact).

## Principles
- **Sound is comfort, not tension.** StarSower's audio should feel like a lullaby the player controls, never a stress cue.
- **Silence is a valid choice.** Not every action needs a sound; restraint keeps meaningful sounds meaningful.
- **Music breathes with the world.** Instrumentation layers in as the player climbs and the world brightens — audio mirrors the visual restoration of light.
- **No punishing audio on failure.** Falling must never produce a harsh "failure stinger" (see `01_Gameplay.md`, `08_GameFeel.md`).

## Detailed Design

### 1. Musical Identity
- **Instrumentation:** soft, primarily acoustic and mellow synth textures — music box tones, gentle plucked strings, warm pads, occasional soft choir/vocal-like synth swells reserved for major moments (Region transitions, Sky Meter milestones). No percussion-heavy or high-tempo tracks; StarSower's music never rushes the player.
- **Tempo:** deliberately slow-to-moderate (60–90 BPM range) across all Regions — the music does not "gamify" urgency; the camera's upward-only ratchet already provides gentle pressure (see `01_Gameplay.md`) without needing tense music to reinforce it.
- **Key/mode:** major and modal (lydian/mixolydian-leaning) tonalities dominate, avoiding minor-key dread even in the lowest, most "faded" Region — melancholy is expressed through sparse instrumentation and space, not dissonance.

### 2. Adaptive Music System
Music is structured in layers tied to world state, not hard scene-cut tracks:
- **Base layer:** always present during a run — a simple, sparse melodic loop unique per Region.
- **Fragment layer:** a soft harmonic layer that fades in gradually as more Star Fragments are collected within a single run, fading back out if the player goes a long stretch without collecting one. This makes collection feel musically rewarding without a discrete "ding" dominating the mix (see Section 3 for the SFX layer, which is separate and additive).
- **Region transition swell:** a short (3–5 second), non-looping harmonic swell plays once when crossing into a new Region, then settles into that Region's base layer — never a jarring track-switch cut.
- **Hub theme:** a calmer, slower variant of the game's main musical theme, subtly enriched (more instrumentation layers unlocked) as the Sky Restoration Meter grows — directly tying audio richness to meta-progression, mirroring the visual brightening described in `05_ArtDirection.md`.

### 3. Sound Design Philosophy
- **Jump:** a soft, light "whoosh/chime" hybrid — must feel airy, never mechanical or heavy.
- **Landing:** a gentle, low-volume soft-thud, scaled subtly by fall distance but never sharp or punishing even on a "hard" landing.
- **Star Fragment collection:** a bright, short chime, pitched slightly randomly (within a musical scale, not arbitrary pitch) per collection so repeated pickups in one run don't feel mechanically identical — reinforces the "musical instrument" feel of collecting light.
- **Region transition:** paired with the musical swell above; a soft rising shimmer, not a loud stinger.
- **Fail state:** deliberately quiet — a slow, soft descending tone with no "failure" connotation (more akin to a sigh than an alarm), immediately followed by silence before the Run Summary screen's calm audio cue. This is a hard rule: no sound in StarSower is allowed to use a harsh, sudden, high-frequency "error" sound anywhere in the game.
- **UI sounds:** minimal, soft "pop"/"tap" feedback only on primary actions (Play, confirm); secondary navigation may be silent.

### 4. Ambience
Each Region carries a distinct ambient soundscape (wind, distant chimes, faint crystalline drift sounds in upper Regions) mixed well beneath the music layer — ambience must never mask musical or gameplay-critical SFX (fragment collection, landing) in the mix hierarchy.

### 5. Mix Hierarchy (highest to lowest priority)
1. Player action SFX (jump, land, collect)
2. Music (base + adaptive layers)
3. Region ambience
4. UI feedback sounds

This hierarchy must be respected in implementation via audio bus ducking — action SFX always cuts through, ambience always yields.

### 6. Technical Constraints
- All audio assets are compressed and streamed appropriately for mobile (short SFX as decompress-on-load, music as streaming) to minimize memory footprint — coordinate with `14_TechnicalArchitecture.md` and `19_ContentPipeline.md` (Addressables-managed audio banks per Region, loaded/unloaded with Region transitions to control memory on low-end devices).
- A global mute/haptics-only mode must be fully supported (see `06_UIUX.md` Settings) without breaking any gameplay feedback loop — no critical information may be audio-only.

## Future Expansion
- **Dynamic instrument unlocks** tied to long-term meta-progression milestones (e.g., a new instrument voice permanently added to the Hub theme after reaching a major Sky Restoration Meter threshold) — flagged in `27_FutureIdeas.md`.
- **Player-triggered ambient chimes** (a passive, no-cost "hum" the player can optionally trigger while idle in the Hub) as a low-priority comfort feature.

## Notes
- Any new SFX must be checked against the mix hierarchy and the "no harsh sudden sound" rule before being added.
- Music tempo and key constraints in Section 1 apply to all future Regions (see `12_Regions.md`, `20_ContentRoadmap.md`) — a future Region proposing a faster or minor-key theme requires explicit revision of this document.
