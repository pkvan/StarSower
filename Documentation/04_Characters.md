# 04 — Characters

## Purpose
This document defines the complete cast of StarSower and the design rules governing character creation. StarSower is intentionally a minimal-cast game — this document exists as much to constrain future character creep as it does to define who currently exists.

## Goals
- Fully specify the player character and the one companion entity.
- Establish firm rules for why StarSower does not and should not have a large cast, dialogue-driven NPCs, or a personified antagonist.
- Give art and animation a clear, complete brief for each character.

## Principles
- **Minimal cast, maximum meaning.** Every character that exists must be irreplaceable to the theme; StarSower does not use characters as content-padding.
- **No dialogue, ever.** Characters communicate only through motion, light, and simple reactive animation.
- **Silhouette-first design.** Every character must be identifiable by silhouette alone, per `05_ArtDirection.md`.

## Detailed Design

### 1. The Player Character — "Người Gieo Sao" (The Star Sower)
- **Role:** Protagonist and sole playable character.
- **Identity:** Deliberately ambiguous in age, gender, and origin — a universal, mythic figure rather than a specific individual, so any player can project themselves onto the role. No backstory is given beyond what is described in `03_Lore.md`.
- **Silhouette:** A small, cloaked figure with a subtly glowing trim (color intensifies as Starlight/meta-progression increases — see `09_Progression.md`, `10_Collectibles.md`), designed to read clearly against both dark lower-Region backgrounds and bright upper-Region backgrounds.
- **Animation set (see `05_ArtDirection.md`, `08_GameFeel.md` for feel targets):** idle (gentle sway, cloak drift), run, jump start, air rise, air fall, land (soft squash), fall-out (a slow, weightless drift, never a violent "death" animation).
- **Cosmetic variation:** Trims, cloak colors, and particle trail effects are the only customizable elements (see `10_Collectibles.md`, `22_Monetization.md`). The silhouette itself never changes, to preserve instant recognizability.
- **No voice, no face detail.** The character's face is never shown in close detail; emotional expression is conveyed entirely through posture and motion (a slumped idle after repeated falls softens into a determined lean after a good run — a subtle, optional animation-layer idea, see Future Expansion).

### 2. The Companion — "Đóm Sao" (Star Mote)
- **Role:** A small, silent orb of light that drifts near the player during runs.
- **Function:** Purely atmospheric and feedback-oriented — it brightens briefly when a Star Fragment is collected and drifts slightly ahead of the player when approaching a Region transition, providing a soft, diegetic directional cue without any UI arrow or marker (ties to `06_UIUX.md`'s "Minimal UI" rule).
- **Design constraint:** Đóm Sao must never block gameplay visibility, never require player interaction, and never gate progress. It is a mood and feedback layer only, not a mechanic.
- **Narrative function:** As described in `03_Lore.md`, it implicitly represents hope itself. It is never explained via text.

### 3. The Antagonistic Force — "The Fading"
- **Role:** Not a character. This entry exists specifically to document that no antagonist character should ever be created for StarSower.
- **Representation:** Purely environmental — desaturated palettes, drifting ash particles, and the fail state itself (see `03_Lore.md`, `02_World.md`). It has no face, form, name spoken in-game, or personality.
- **Design rule:** Any future feature proposal that introduces a "boss," "enemy," or antagonist character must be rejected at the concept stage unless it passes an explicit, documented revision of both this file and `00_Vision.md` — this is a deliberate, high-friction gate, not an oversight.

### 4. Rules for Any Future Character Proposal
Before any new character (companion, cameo, seasonal cosmetic figure) is approved:
1. It must have no spoken or written dialogue.
2. It must be explainable in one sentence that references `03_Lore.md`.
3. It must not introduce a new required interaction (e.g., no NPC shops requiring dialogue trees).
4. It must be visually distinguishable from Đóm Sao and the Star Sower at a glance.

## Future Expansion
- **Subtle player-state animation layer:** a long-term idea where the Star Sower's idle posture subtly reflects recent performance (see Character 1 note above) — must remain purely cosmetic and never communicate mechanical information that isn't already visible elsewhere.
- **Cosmetic "other Sowers" cameos:** faint, non-interactive silhouettes of other Star Sowers glimpsed in the far background of upper Regions, implying the player is not alone across time — a lore-reinforcing idea, not a new character system. Flagged in `27_FutureIdeas.md`.

## Notes
- This document intentionally locks the cast size at two entities (player + companion) plus one non-character narrative force. Any deviation must be treated as a major design decision requiring sign-off against `00_Vision.md`, not a routine content addition.
