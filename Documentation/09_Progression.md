# 09 — Progression

## Purpose
This document defines StarSower's meta-progression system: how a player's cumulative effort across many runs produces permanent, visible change in the game world. It is the mechanical backbone of "every jump brings light" beyond a single session.

## Goals
- Define the full currency and unlock loop connecting runs to persistent world state.
- Ensure progression is generous, ungrindy, and never gates core gameplay behind repetitive friction.
- Give `13_SaveSystem.md` and `18_ScriptableObjects.md` a precise data model to implement against.

## Principles
- **No Heavy Grinding.** Every run contributes meaningfully; the player should never feel they must repeat runs mechanically to "unlock the game."
- **Progress is never lost.** Meta-progression only ever increases — see `02_World.md`'s Persistent Layer concept.
- **Visible over numerical.** Progression should be felt through world brightness and Hub changes, not primarily through spreadsheet-style stat screens.
- **No power creep.** Progression unlocks cosmetics and convenience (alternate start points), never gameplay power — see `01_Gameplay.md` (movement/jump stats are fixed, not upgradable) and `22_Monetization.md`.

## Detailed Design

### 1. The Two Currencies
- **Star Fragments (in-run, ephemeral):** collected during a run (see `10_Collectibles.md`). Exist only within the context of the current run's tally.
- **Starlight (persistent, meta):** the converted, permanent form of Star Fragments. At the end of every run — regardless of how the run ended — all Star Fragments collected are converted 1:1 (or via a simple documented multiplier, tuned in `21_Balancing.md`) into Starlight, which is saved permanently (see `13_SaveSystem.md`).

This two-currency split exists specifically so that "failure" never erases earned progress — a direct mechanical expression of `00_Vision.md`'s "the light dims, but does not go out."

### 2. The Sky Restoration Meter
The primary, always-visible expression of meta-progression:
- A persistent, monotonically-increasing meter tied to lifetime Starlight earned (not current Starlight balance — spending Starlight on cosmetics never reduces this meter, since it represents *total light restored*, not a spendable resource itself).
- Visually represented in the Hub as a brightening sky/horizon (see `02_World.md`, `05_ArtDirection.md`), not a numeric progress bar as the primary presentation (a small numeric readout may exist secondarily).
- Divided into milestone thresholds that unlock: new Beacon checkpoints (Section 3), cosmetic reveals (`10_Collectibles.md`), and Hub ambient richness (`07_Audio.md`, `05_ArtDirection.md`).
- Has no final "100%" end-state planned for initial release — it is designed as a long-horizon, near-endless meta-goal, consistent with the endless-climb core loop (see `01_Gameplay.md`). A soft narrative "Zenith reached" milestone exists (see `12_Regions.md`) but does not close off further meter growth.

### 3. Beacons (Alternate Start Points)
- Beacons are checkpoints unlocked permanently once a player has reached a given Region a documented number of times, or via a Sky Restoration Meter milestone (final rule owned by `21_Balancing.md`).
- Once unlocked, a Beacon allows a run to start at that Region's height instead of the very bottom, letting experienced players reach higher Regions faster without replaying identical low-Region content — this is the primary anti-grind mechanism for a game with an endless vertical structure.
- Beacons are purely a starting-position convenience — they do not alter difficulty, platform density, or fragment value, preserving fairness and avoiding pay-to-win-adjacent design (see `22_Monetization.md`).

### 4. Cosmetic Unlocks
- Starlight (the spendable balance, distinct from the lifetime meter in Section 2) can be spent in the Hub's Cosmetics panel on trims, cloak colors, and particle trail variants for the Star Sower (see `04_Characters.md`, `10_Collectibles.md`).
- Cosmetic unlocks are additive and never expire, never rotate out on a timer (no FOMO shop design — see `22_Monetization.md`).

### 5. Progression Data Model (Conceptual)
For implementation reference (finalized structure owned by `13_SaveSystem.md` and `18_ScriptableObjects.md`):
- `LifetimeStarlightEarned` (drives the Sky Restoration Meter; monotonic, never decreases)
- `CurrentStarlightBalance` (spendable; decreases on cosmetic purchase)
- `UnlockedBeacons` (set of Region identifiers)
- `UnlockedCosmetics` (set of cosmetic identifiers)
- `BestHeightReached` (per Region and overall; display-only, does not gate anything)

### 6. Anti-Grind Safeguards
- No stamina, energy, or timer-gated run limits — the player may always immediately start another run.
- No mandatory daily quotas or login streak mechanics (see `00_Vision.md`, `22_Monetization.md`).
- Starlight earn rate is tuned (see `21_Balancing.md`) so that meaningful cosmetic/Beacon unlocks are reachable within a small number of typical play sessions, not weeks of dedicated grinding.

## Future Expansion
- **Seasonal/limited cosmetic drops** are explicitly *not* planned as FOMO-timed content — if pursued, they must be additive and remain permanently obtainable, per `22_Monetization.md`'s no-FOMO stance.
- **Secondary meta-currency for a specific system** (e.g., a distinct resource for a future "world decoration" feature) should only be introduced if it does not duplicate Starlight's role — flagged for review in `27_FutureIdeas.md`.

## Notes
- Any new progression system proposal must specify: (a) which currency it uses, (b) whether it can regress, and (c) how it is visually represented in the Hub — proposals missing any of these three are incomplete per this document's standard.
- The Sky Restoration Meter's monotonic, un-spendable nature is a deliberate and permanent design decision — it must never be refactored into a spendable resource, as that would break the "light never fades once restored" promise.
