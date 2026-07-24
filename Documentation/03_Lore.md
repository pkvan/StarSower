# 03 — Lore

## Purpose
This document defines the narrative backstory of StarSower — why the sky went dark, who the Star Sower is, and what restoring light means within the fiction. It exists to give art, audio, and world design a consistent well of meaning to draw from, without introducing an explicit narrative delivery system (no dialogue, no cutscenes, no text logs).

## Goals
- Provide a complete, internally consistent backstory that never needs to be fully shown to the player, only felt.
- Give every Region, character, and collectible a grounded narrative reason to exist.
- Keep the lore simple enough to be conveyed entirely through visual storytelling (see `05_ArtDirection.md`, `02_World.md`).

## Principles
- **Show, never tell.** No dialogue boxes, no journals, no narrator. If a piece of lore cannot be expressed visually or through sound, it stays in this document only, as internal context.
- **Myth, not exposition.** The tone is that of a bedtime story or folk myth — simple, universal, emotionally resonant, not a dense fantasy encyclopedia.
- **Hope is earned, not given.** The story must never resolve neatly with a "the world is saved" ending screen — restoration is depicted as ongoing and cumulative (see `09_Progression.md`).

## Detailed Design

### 1. The Myth (Core Backstory)
Long ago, the sky was full of stars, and every star was a small piece of hope belonging to someone in the world below. As long as people kept hoping, the stars stayed lit.

Slowly, hope faded — not from any single catastrophe, but from the ordinary erosion of a world that stopped looking up. As hope thinned, the stars began to fall, one by one, until the sky above the world was empty and grey, and the world below grew quiet and still.

The Star Sower is the last person who still remembers how to hope in a way strong enough to climb. They do not know why they were the one left to do this — only that somewhere above, the fallen stars are still waiting to be replanted.

### 2. What "Replanting a Star" Means
Mechanically, this is a Star Fragment (see `10_Collectibles.md`). Narratively, each fragment is a small, dormant piece of someone else's forgotten hope. The Star Sower does not create new light — they gather what was lost and carry it back upward, restoring it to where it belongs. This is why collecting fragments visually brightens the world (see `02_World.md`) rather than simply increasing a score: the fiction and the mechanic are the same action.

### 3. Why the World Is Vertical
The world's fall was not physical destruction — it was a fall in the sense of "falling out of the sky," a slow sinking of hope and light toward the ground. The Star Sower's climb is a literal reversal of that fall. This is the narrative justification for the single-axis vertical world model in `02_World.md`.

### 4. The Antagonistic Force: The Fading
StarSower does not have a villain character. The opposing force is **the Fading** — an ambient, impersonal entropy, not a sentient enemy. The Fading is represented only through environmental decay (dead, grey platforms; drifting ash-like particles in lower Regions) and through the fail state itself (falling below the light the player has climbed toward). It is never personified with a face, a voice, or dialogue — keeping with `00_Vision.md`'s "No Feature Bloat" and the no-dialogue policy in `04_Characters.md`.

### 5. Regional Narrative Throughline
Each Region in `12_Regions.md` represents a stage of the world's memory of light, from most forgotten to nearly restored:
1. **The Fading Ground** — where hope is thinnest; the world here barely remembers it once had stars.
2. **The Grey Cloudbelt** — a hazy, uncertain space; neither fully dark nor fully lit.
3. **The Twilight Reach** — the boundary where old light and new light meet; the first Region where the player's presence visibly changes the color of the sky.
4. **The Fallen Star Expanse** — where the most stars fell and scattered; the densest concentration of fragments.
5. **The Zenith** — the highest point yet reached collectively by all Star Sowers who came before; each run pushes this frontier a little further (tied to the persistent Sky Restoration Meter, see `09_Progression.md`).

### 6. The Silent Companion
A small drifting light, **Đóm Sao** (see `04_Characters.md`), accompanies the player without speaking. It is not explained in-game — it is simply present, reacting subtly to fragment collection and Region transitions. Internally, it represents hope itself, given just enough form to keep the player company without becoming a talking-mascot character.

### 7. Tone Guidelines for Any Future Narrative Content
- Never use the words "save," "defeat," "boss," or "villain" in any user-facing copy — these frame the story as a battle rather than a restoration.
- Never explain the myth directly to the player in text form. If future marketing or store-page copy needs the backstory, it may summarize this document, but in-game the myth must remain implicit.
- Any new lore element must answer: "What did hope look like before it faded here?"

## Future Expansion
- A future "Archive" unlockable in the Hub could let curious players opt into reading a condensed version of this lore, entirely optional and separate from the core experience — flagged in `27_FutureIdeas.md`.
- Additional Regions beyond the Zenith (a "beyond the sky" concept) may be introduced in future content updates once the core five Regions are validated — see `20_ContentRoadmap.md`.

## Notes
- This document is the narrative constitution for `04_Characters.md`, `05_ArtDirection.md`, `07_Audio.md`, and `12_Regions.md`. Any visual or audio asset that implies a lore detail not present here must be reconciled with this document before production, not after.
