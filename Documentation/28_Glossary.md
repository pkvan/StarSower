# 28 — Glossary

## Purpose
This document defines every proper noun, system name, and term of art used across the StarSower Design Bible, ensuring consistent usage in every document, in code naming (`16_CodingGuidelines.md`), and in player-facing copy (`24_Localization.md`).

## Goals
- Provide one canonical definition per term, eliminating ambiguity between documents.
- Distinguish narrative/player-facing terms from internal/technical terms.
- Serve as the reference for translators and new team members alike.

## Principles
- **One term, one meaning.** No word in this glossary may be reused elsewhere in the bible with a different meaning.
- **Player-facing terms are tone-checked.** Any term intended for player-facing UI or copy must match the calm, mythic tone established in `03_Lore.md`.
- **Internal terms are clearly marked.** Terms used only in design/engineering discussion are distinguished from terms the player will actually see.

## Detailed Design

### 1. Narrative & World Terms (Player-Facing)
- **Người Gieo Sao / The Star Sower** — the player character (`04_Characters.md`).
- **Đóm Sao / Star Mote** — the silent companion light (`04_Characters.md`).
- **The Fading** — the ambient, non-personified force representing entropy/lost hope (`03_Lore.md`, `04_Characters.md`). Never referred to as an "enemy" or "villain" in any document or copy.
- **Star Fragment (Mảnh Sao)** — the in-run collectible (`10_Collectibles.md`).
- **Starlight** — the persistent meta-currency (`09_Progression.md`).
- **Sky Restoration Meter** — the persistent, monotonic progress indicator of world-light restoration (`09_Progression.md`).
- **Beacon** — an unlockable alternate run-start point tied to a Region (`09_Progression.md` §3, `12_Regions.md` §4).
- **The Hub** — the between-runs space, functioning as the game's main menu (`02_World.md` §3).
- **Region** — one of the five (initially) major vertical zones of the climb (`12_Regions.md`). Named individually: The Fading Ground, The Grey Cloudbelt, The Twilight Reach, The Fallen Star Expanse, The Zenith.

### 2. Gameplay Terms (Mostly Internal, Some Player-Facing via UI)
- **Run** — a single attempt at the climb, from Hub departure to fail state (`01_Gameplay.md`).
- **Fail State** — the moment a run ends due to falling below the camera threshold (`01_Gameplay.md` §5). Never referred to as "death" or "game over" in player-facing copy — StarSower avoids failure-shaming language (`03_Lore.md` §7).
- **Dead Zone** — the camera buffer zone within which player movement does not trigger camera motion (`01_Gameplay.md` §4, `14_TechnicalArchitecture.md`).
- **Coyote Time** — the short grace window after leaving a platform edge during which a jump is still valid (`08_GameFeel.md` §2). Internal term; never shown to players.
- **Jump Buffering** — a queued jump input that executes immediately upon landing (`08_GameFeel.md` §2). Internal term.
- **Reachability Guarantee** — the design/technical rule that every generated platform gap is completable within the tuned jump envelope (`11_Platforms.md` §2, `21_Balancing.md`). Internal term.

### 3. Technical Terms (Internal Only)
- **`IInputProvider`** — the interface abstracting player input source (`14_TechnicalArchitecture.md`).
- **`IGroundDetector`** — the interface abstracting ground-contact detection.
- **`IPlatformPool`** — the interface abstracting platform instance allocation/recycling.
- **`ICameraShake` / `ICameraZoom`** — interfaces exposing camera juice APIs.
- **Event Channel** — a ScriptableObject-based decoupled event type, the target architecture for cross-system communication (`17_EventSystem.md`).
- **Config Asset** — a ScriptableObject holding design-owned tunable data (`18_ScriptableObjects.md`).
- **Single-Writer Principle** — the rule that any shared Transform has exactly one owning class responsible for writing its position (`14_TechnicalArchitecture.md` §4).

### 4. Business Terms (Internal Only)
- **Cosmetic** — a purely visual, non-power-affecting unlockable item (`10_Collectibles.md` §4, `22_Monetization.md`).
- **Second Wind** — the optional, once-per-run rewarded-ad revive offer (`22_Monetization.md` §1.2).

## Future Expansion
- As new Regions, platform behaviors, or systems are introduced, their canonical names must be added here in the same change that introduces them elsewhere in the bible.

## Notes
- If any document uses a term not listed here, that is a defect in this glossary and must be corrected — this document must stay exhaustive relative to the rest of the bible.
- Player-facing terms (Section 1) must never appear inconsistently capitalized or translated differently across documents — this glossary's spelling is authoritative for `24_Localization.md`'s source strings.
