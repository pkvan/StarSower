# 29 — Design Philosophy

## Purpose
This document is the closing synthesis of the Design Bible — it does not introduce new systems, but articulates the underlying decision-making philosophy that produced every other document, so future contributors internalize *how to think like StarSower's design team*, not just what has already been decided.

## Goals
- Distill the recurring reasoning patterns visible across all prior documents into explicit, teachable principles.
- Give the team a way to make *new* decisions consistently, not just follow existing ones.
- Serve as the final authority for resolving ambiguity when no other document directly answers a question.

## Principles
- **Every principle here has already been demonstrated somewhere else in this bible.** This document names patterns, it does not invent new rules.
- **When documents conflict, philosophy resolves it.** If two documents seem to disagree, the resolution is whichever reading better serves the philosophy in this document and the thesis in `00_Vision.md`.

## Detailed Design

### 1. The Core Reasoning Pattern
Across every document in this bible, decisions follow the same three-step reasoning:
1. **What does this serve thematically?** (Does it make a jump feel meaningful, does it make light visible — `00_Vision.md`'s filter question.)
2. **What does this cost the player?** (Time, precision demand, cognitive load, money, trust — costs are minimized by default.)
3. **What does this cost the system?** (Complexity, maintenance burden, performance — see `14_TechnicalArchitecture.md`, `16_CodingGuidelines.md`.)

A feature only proceeds when it scores well on (1) while keeping (2) and (3) low. This is why StarSower repeatedly chooses fewer, deeper systems (one collectible, two currencies, five Regions, four platform behaviors) over broad, shallow content.

### 2. Subtraction as a Design Tool
StarSower's documents consistently define what a system is *not* alongside what it is (see `00_Vision.md` §4, `04_Characters.md` §3–4, `10_Collectibles.md` §5, `22_Monetization.md` §1.3). This is deliberate: in a long-running project, the temptation to add is constant, but the temptation to remove or refuse is rare and must be actively protected. Every future design document should include an explicit "what this is not" section, following this established pattern.

### 3. Gentleness Is a System, Not a Mood
"Gentle" in StarSower is not vibes — it is implemented as concrete mechanical rules: the two-currency no-progress-loss system (`09_Progression.md`), the coyote-time/jump-buffer forgiveness (`08_GameFeel.md`), the reachability guarantee (`11_Platforms.md`), the no-harsh-failure-feedback rule (`08_GameFeel.md` §5), and the no-FOMO monetization constraint (`22_Monetization.md`). Any future feature that claims to be "gentle" must point to a specific mechanical rule that makes it so, not just a tonal intention.

### 4. Composition as Both a Code and Design Value
`14_TechnicalArchitecture.md`'s composition-over-inheritance rule is not only an engineering preference — it mirrors the design philosophy across the whole bible: platform behaviors compose onto a base (`11_Platforms.md`), Region identity composes palette + platform mix + fragment density + audio (`12_Regions.md`), and even the narrative composes from a small, reusable cast rather than growing a large character roster (`04_Characters.md`). StarSower is, structurally, a game about small, well-defined pieces recombining — this should inform how *every* future system, in code or design, is approached.

### 5. Trust Is the Product
StarSower's monetization (`22_Monetization.md`), save system (`13_SaveSystem.md`), and failure framing (`08_GameFeel.md`, `03_Lore.md`) all optimize for the same underlying goal: the player should never feel tricked, punished unfairly, or afraid of losing something they earned. This is treated as a competitive and creative advantage, not a limitation — StarSower is designed to be a game people trust, in a mobile market where trust is often the scarcest resource.

### 6. How to Resolve a New, Undocumented Question
When facing a design or technical question not covered by an existing document:
1. Apply `00_Vision.md`'s filter question.
2. Apply the three-step reasoning pattern in Section 1.
3. Check whether "doing less" (Section 2) is a valid answer before designing something new.
4. Verify the answer can be described as "gentle" via a specific mechanical rule (Section 3), not just tone.
5. Document the decision in the appropriate existing file rather than creating an orphaned one-off decision — if no file fits, propose a new document explicitly, following the same rigor used to create this bible.

## Future Expansion
- As StarSower ships and gathers real player data, this document should be revisited to confirm the reasoning patterns described here actually produced the intended player experience — and to document any philosophy refinements learned from production reality.

## Notes
- This document has final interpretive authority over ambiguity between other documents, subordinate only to `00_Vision.md` itself.
- Every document in this bible, when read in full, should feel like it was written by the same designer with the same values — that consistency is the actual deliverable of this document.
