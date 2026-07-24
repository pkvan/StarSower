# 22 — Monetization

## Purpose
This document defines StarSower's complete monetization model and the hard constraints that keep it aligned with `00_Vision.md`'s "No Pay to Win" and "No Heavy Grinding" principles. It is authoritative for any revenue-related feature proposal.

## Goals
- Define every revenue stream StarSower will use, precisely and exhaustively.
- Establish permanent, non-negotiable constraints preventing predatory or trust-eroding monetization patterns.
- Ensure monetization never contradicts the emotional tone established in `08_GameFeel.md` and `07_Audio.md`.

## Principles
- **No Pay-to-Win, absolutely.** No purchase may affect movement stats, jump ability, platform difficulty, or fragment value. This is a permanent constraint, not a launch-only policy.
- **No FOMO.** No time-limited shops, no countdown timers, no "offer expires in" pressure tactics.
- **No forced interruption.** Monetization never interrupts an active run (see `01_Gameplay.md`'s "no modal interruptions during a run" rule).
- **Respect trust as a resource.** A player's goodwill is treated as more valuable long-term than any single transaction.

## Detailed Design

### 1. Revenue Streams

#### 1.1 Cosmetic IAP
- Direct purchase of Starlight bundles (a convenience purchase, not a separate currency — see `09_Progression.md`) or direct purchase of specific cosmetic items (`10_Collectibles.md` §4) with real currency.
- Every cosmetic purchasable with real money must also be purchasable with earned Starlight — no cosmetic is ever real-money-exclusive. This guarantees free players can eventually access all cosmetic content through play alone.
- Cosmetic pricing is modest and consistent with a small indie mobile game — no artificial price anchoring via inflated "original price" strikethroughs.

#### 1.2 Optional Rewarded-Ad Revive ("Second Wind")
- Once per run, after a fail state, the player may optionally watch a rewarded ad to continue the current run from a safe recent point (implementation detail owned by `14_TechnicalArchitecture.md` once scheduled) instead of ending the run.
- This offer is presented calmly, without pressure framing ("last chance!", countdown timers) — a single clear button on the Run Summary screen (`06_UIUX.md`), easily ignorable.
- Declining the offer has zero negative consequence — Starlight conversion and progress saving proceed identically either way (see `13_SaveSystem.md`).
- This is the *only* ad placement in the base game. No banner ads, no interstitial ads between runs, no forced ad-gates on any UI flow.

#### 1.3 Explicitly Excluded Revenue Models
The following are permanently excluded from StarSower and must be rejected without further discussion if proposed:
- Loot boxes or randomized-odds purchases of any kind.
- Energy/stamina systems that limit play frequency and can be bypassed by payment.
- Power-affecting purchases (extra jump height, faster movement, wider platforms) of any kind.
- Subscription models.
- Forced interstitial or banner ads.
- Pay-to-skip-grind mechanics — since `09_Progression.md`'s anti-grind design means there is no meaningful grind to skip in the first place.

### 2. Pricing Philosophy
- Prices are set low enough to feel like a fair, guilt-free "tip the developer" gesture for a player who is enjoying the game, not a necessary unlock for content they feel entitled to.
- No purchase is ever required to experience the full five-Region climb (`12_Regions.md`), the full core loop (`01_Gameplay.md`), or the full Sky Restoration Meter progression (`09_Progression.md`).

### 3. Transparency Rules
- All prices are shown in real currency before purchase, with no obfuscating virtual-currency-only pricing that hides real-world cost.
- Any Starlight bundle purchase clearly shows exactly what cosmetic(s) that amount could buy, avoiding ambiguous "buy currency now, decide later" pressure patterns.

### 4. Relationship to Achievements & Progression
Achievements (`23_Achievements.md`) and Beacons/Sky Meter progression (`09_Progression.md`) are never purchasable — they must always be earned through play, preserving their meaning as genuine accomplishment markers.

## Future Expansion
- **A single, optional one-time "Supporter" cosmetic bundle** (purely cosmetic, clearly marketed as a way to support development) is a permissible future addition under these rules, but must still follow the "also earnable via Starlight" rule in Section 1.1 unless explicitly scoped otherwise with full design review.
- **Regional pricing localization** to keep purchases fairly priced across markets (see `24_Localization.md`).

## Notes
- Any new monetization feature proposal must be checked against Section 1.3's exclusion list and `00_Vision.md`'s filter question before being considered — this document's constraints are permanent and require full team/stakeholder sign-off to change, not a routine design decision.
- The rewarded-ad revive (Section 1.2) is the single ad placement permitted in the base game; adding a second ad placement anywhere requires an explicit revision of this document.
