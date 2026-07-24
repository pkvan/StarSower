# 24 — Localization

## Purpose
This document defines StarSower's approach to localization and accessibility — how the game supports multiple languages and diverse player needs without compromising the minimal, visual-storytelling-first design established in `00_Vision.md` and `03_Lore.md`.

## Goals
- Define the localization pipeline and initial language targets.
- Ensure the no-dialogue, visual-storytelling narrative approach (`03_Lore.md`) minimizes localization burden by design.
- Define baseline accessibility commitments consistent with `06_UIUX.md`.

## Principles
- **Localization by design, not by patch.** All player-facing text is externalized to a key-based system from the start, never hardcoded in code or scenes.
- **The story needs almost no words.** Because StarSower tells its story visually (`03_Lore.md`, `05_ArtDirection.md`), localization scope is deliberately small — UI labels and short copy, not narrative prose.
- **Accessibility is baseline, not an add-on.** Core accessibility considerations (contrast, touch target size, colorblind-safe cues) are part of initial design, not a post-launch patch.

## Detailed Design

### 1. Localization Scope
Because of the no-dialogue design (`03_Lore.md`, `04_Characters.md`), the entire localizable text surface of StarSower consists of:
- UI labels and buttons (`06_UIUX.md`: Hub, Run Summary, Settings, Cosmetics/Beacon panels).
- Achievement names and short descriptions (`23_Achievements.md`).
- Cosmetic item names (`10_Collectibles.md`).
- Settings and onboarding micro-copy (if any onboarding text exists at all — StarSower favors teaching through play per `01_Gameplay.md`, minimizing this further).
- Store listing copy (owned separately from in-game text, but sourced from the same lore voice established in `03_Lore.md`).

This is a deliberately small text surface, which keeps localization cost and maintenance low across a long production cycle.

### 2. Initial Language Targets
Vietnamese (primary development language and default) and English are the baseline supported languages at launch, given the project's origin and target audience considerations. Additional languages are added post-launch based on market data (see `20_ContentRoadmap.md` Phase 5), prioritized toward markets with strong mobile casual/indie platformer engagement.

### 3. Technical Approach
- All text is referenced via localization keys, never literal strings, in both UI components and any `ScriptableObject` definitions that carry display text (`AchievementDefinition`, `CosmeticDefinition` — see `18_ScriptableObjects.md`).
- Font selection (`06_UIUX.md`) must support the full character set required for all target languages (including Vietnamese diacritics) without a font-switching hack — this is validated before any new language is added.
- Number, currency, and date formatting (Starlight totals, height display) follow locale-appropriate formatting conventions.

### 4. Tone Consistency Across Languages
Any translation must preserve the myth-like, gentle tone established in `03_Lore.md` and `06_UIUX.md`'s "calm typography" principle — translations are reviewed for tone, not just literal accuracy, particularly for the small amount of narrative-adjacent copy (achievement names, store listing).

### 5. Accessibility Baseline
- Minimum 48x48dp touch targets for all interactive elements (see `06_UIUX.md`).
- Colorblind-safe distinction between interactive controls and background at all times; no information conveyed by color alone (numeric/icon backup always present).
- Full audio-off playability: no gameplay-critical information is audio-only (see `07_Audio.md`).
- Text scaling: UI text respects a minimum readable size on small mobile screens and does not rely on dense paragraph text anywhere in the core experience.

## Future Expansion
- **Right-to-left language support** if a future target market requires it — flagged as a UI layout consideration requiring `06_UIUX.md` review before commitment.
- **Full accessibility audit** (screen reader support for menus, extended colorblind palette modes) as a dedicated pre-launch milestone once the core five-Region content (`20_ContentRoadmap.md` Phase 3) is complete.
- **Assist Mode** (see `01_Gameplay.md` Future Expansion, `25_RiskAnalysis.md`) as a motor-accessibility feature, distinct from but complementary to the visual/audio accessibility items in this document.

## Notes
- Any new UI text introduced by a feature must be added as a localization key at implementation time, never as a hardcoded string "to be localized later" — this is a binding rule, not a best-effort suggestion, per `16_CodingGuidelines.md`'s no-hardcoding principle applied to text.
- This document's small localization scope (Section 1) is a direct benefit of the narrative decisions in `03_Lore.md` and must be considered a design constraint protecting that benefit — any future feature proposing significant new narrative text should be evaluated partly on its localization cost impact.
