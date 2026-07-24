# 13 — Save System

## Purpose
This document defines what data StarSower persists between sessions, where it lives, and the rules governing save integrity. It is the design/technical contract for `09_Progression.md`'s meta-progression model.

## Goals
- Enumerate exactly what must be saved and why.
- Define a save architecture appropriate for a mobile, offline-first, single-player game.
- Guarantee the "progress never regresses" promise from `00_Vision.md` and `09_Progression.md` at the data layer.
- Keep the save system simple enough to avoid becoming a maintenance burden over a long production cycle.

## Principles
- **Local-first, offline-safe.** StarSower must be fully playable and save correctly with no network connection.
- **Never lose player progress.** Save writes must be resilient to app kill/crash at any point (see Section 4).
- **Small, human-readable data.** The save format should be simple enough to debug by hand during development.
- **No hidden state.** Every persisted value must trace back to a system defined elsewhere in this bible (`09_Progression.md`, `10_Collectibles.md`, `23_Achievements.md`).

## Detailed Design

### 1. What Is Saved
Derived directly from `09_Progression.md`'s data model:
- `LifetimeStarlightEarned` (drives the Sky Restoration Meter)
- `CurrentStarlightBalance` (spendable)
- `UnlockedBeacons` (set of Region identifiers, see `12_Regions.md`)
- `UnlockedCosmetics` (set of cosmetic identifiers, see `10_Collectibles.md`)
- `BestHeightReached` (overall, and optionally per Region — display-only)
- `UnlockedAchievements` (see `23_Achievements.md`)
- `SettingsState` (music/SFX volume, haptics toggle, language — see `06_UIUX.md`, `24_Localization.md`)

Explicitly **not** saved: in-run state (current Star Fragment tally, current platform layout) — a killed app during a run simply loses that single run's progress up to the last completed conversion point, which is an acceptable, clearly-communicated trade-off consistent with the short session design in `01_Gameplay.md`.

### 2. Storage Model
- **Primary store:** a single structured local file (JSON) written to the platform's persistent application data path, containing all fields from Section 1.
- **Settings-only fast path:** trivial settings toggles (volume, haptics) may additionally use lightweight platform preference storage for instant read on cold boot, but the JSON file remains the source of truth reconciled on load.
- **No cloud save in the base release** — see Future Expansion. This keeps the initial architecture simple and avoids account/login friction, consistent with `00_Vision.md` and `06_UIUX.md`'s no-account-system stance.

### 3. Write Triggers
Saves are written at clearly defined, infrequent moments rather than continuously, to minimize I/O and corruption risk:
- Immediately after a run ends and Star Fragments convert to Starlight (`09_Progression.md`).
- Immediately after any cosmetic purchase or Beacon unlock.
- Immediately after a settings change.
- On application pause/quit, as a final safety-net write of current in-memory state.

### 4. Integrity & Corruption Safety
- Writes use an atomic write pattern (write to a temporary file, then replace the previous save file) to avoid partial/corrupted saves from an app kill mid-write.
- A single backup of the last-known-good save is retained; if the primary save fails to parse on load, the system falls back to the backup before ever defaulting to a fresh/empty save.
- A fresh/empty save is only ever created on a genuinely first launch (no prior save or backup found) — the system must never silently wipe a corrupted save without first attempting recovery.

### 5. Versioning
- The save file includes a schema version number from the first release onward. Any future field addition must be backward-compatible (new fields default sensibly for old saves); any structural change requires an explicit migration step, documented at the time it is introduced.

## Future Expansion
- **Cloud save / cross-device sync** via a platform account system (Google Play Games / Game Center), scoped as a clearly separable future milestone — must not block or complicate the local-first save path when added; local save remains authoritative offline.
- **Save export/import for support purposes** (a simple copyable code or file), useful for player support without requiring a full account system.

## Notes
- This document defines *what* is saved and the integrity guarantees; the exact serialization implementation (JSON structure, field names) is owned by `14_TechnicalArchitecture.md` and must stay in sync with any change to `09_Progression.md`'s data model.
- Any new persisted field must be added to Section 1 before being implemented — untracked save fields are not permitted, to keep this document authoritative.
