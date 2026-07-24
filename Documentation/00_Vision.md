# 00 — Vision

## Purpose
This document defines why StarSower exists, what experience it must deliver, and the single sentence every future design, art, audio, or engineering decision must be tested against. It is the root document of the Design Bible — every other file inherits its authority from this one, and no other document may contradict it.

## Goals
- Establish the emotional and mechanical promise of StarSower in unambiguous terms.
- Give every discipline (design, art, audio, engineering, monetization) one shared north star.
- Define what StarSower is **not**, to prevent scope creep over a long development cycle.
- Provide a filter question the team can apply to any new feature proposal.

## Principles
- **One sentence rules everything.** *"Every jump brings light."* If a feature cannot be traced back to this sentence, it does not belong in StarSower.
- **Feeling over content volume.** A small game that feels extraordinary to play beats a large game that feels average.
- **Respect the player's time and trust.** No manipulation, no anxiety-driven monetization, no artificial friction.
- **Design for one thumb.** The entire core loop must be playable one-handed, on a phone, in short sessions.

## Detailed Design

### 1. The One-Line Pitch
StarSower is a 2D vertical platformer about a lone figure climbing through a sky that has gone dark, replanting stars with every leap, and slowly bringing light back to a world that forgot how to hope.

### 2. The Design Thesis
The central thesis of StarSower is that **movement is meaning**. In most platformers, jumping is a mechanical obstacle to overcome. In StarSower, every jump is the verb of the game's theme — the player is not climbing *away* from something, they are climbing *toward* restoring something. This reframing changes how every system must be evaluated:

| Traditional platformer question | StarSower question |
|---|---|
| "Is this jump hard enough?" | "Does this jump feel like an act of hope?" |
| "How do we punish failure?" | "How do we make failure gentle, so the player wants to try again?" |
| "How do we reward skill?" | "How does skill visibly restore light to the world?" |

### 3. Experience Pillars
These five pillars are permanent. They are referenced by every other document in this bible.

1. **Gentle Momentum** — Controls must feel light, floaty-but-precise, and forgiving at the edges (coyote time, generous hitboxes). The player should feel like they are *rising*, not fighting gravity.
2. **Visible Restoration** — Every meaningful action (collecting a Star Fragment, reaching a new Region, completing a run) must produce an immediate, visible lightening of the world. Progress must be seen, not just read as a number.
3. **Quiet Failure** — Falling is not punished with harsh feedback (no red flashes, no failure stingers, no shaming text). Failure is framed as "the light dims, but it does not go out" — the player always keeps what they earned in Starlight.
4. **Uncluttered Screen** — The player's eyes stay on the character and the sky. UI is minimal, diegetic where possible, and never blocks the vertical sightline the player needs to plan jumps.
5. **A World That Remembers You** — Meta-progression (Starlight, unlocked Beacons, a persistently brightening sky) ensures that no run is "wasted" — every session leaves a permanent, visible mark on the game's world state.

### 4. What StarSower Is Not
Explicitly excluding scope is as important as defining it:
- **Not a difficulty-punishing "rage platformer".** Precision is rewarded, but failure is never humiliating.
- **Not a live-service game.** No energy timers, no daily-login guilt loops, no FOMO events.
- **Not a narrative-heavy game.** Story is told environmentally and through world-state, not through dialogue trees or cutscenes (see `03_Lore.md`, `04_Characters.md`).
- **Not pay-to-win.** No purchasable power. See `22_Monetization.md`.
- **Not a content-bloated game.** Every Region, platform type, and system must justify its existence against the thesis before being greenlit.

### 5. The Filter Question
Before any feature enters production, it must pass this question, asked in this literal order:

1. Does it make a jump feel more meaningful?
2. Does it make light (literal or metaphorical) more visible in the world?
3. Does it respect a short mobile session (under 3 minutes to feel rewarded)?
4. Does it avoid punishing the player emotionally for failing?

If the answer to any of these is "no," the feature must be redesigned or rejected. This filter is authoritative and referenced explicitly in `26_Backlog.md` and `27_FutureIdeas.md`.

## Future Expansion
- As the game matures past its first vertical slice, this document should gain a **"Vision Validation"** appendix — a short retrospective confirming shipped features still serve the thesis. This is a review process, not a rewrite; the core vision itself is intended to remain stable for the life of the project.
- If a spin-off or sequel is ever considered, it must start from a new Vision document — the thesis of StarSower is specific to this game and should not be diluted to accommodate a different product.

## Notes
- This document takes precedence over any other design document in case of conflict. Any contradiction discovered between documents must be resolved by amending the *other* document, not this one, unless the team explicitly convenes to revise the vision itself.
- "Every jump brings light" must appear, in spirit or in literal wording, in the Purpose or Principles section of every subsequent document in this bible.
