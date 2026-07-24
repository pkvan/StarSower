# 30 — QA & Testing (Proposed Addition)

## Purpose
This document defines StarSower's quality assurance strategy — what must be tested, at what level (automated vs. manual), and the specific test cases derived from the fairness, feel, and trust guarantees made throughout this bible. It was proposed as an addition to the base 30-file list because several documents (`16_CodingGuidelines.md`, `25_RiskAnalysis.md`, `13_SaveSystem.md`) reference dedicated QA ownership that needed a concrete home.

## Goals
- Translate the bible's qualitative guarantees (reachability, gentleness, save integrity) into concrete, repeatable test cases.
- Define which logic should be automated (unit-testable) versus which requires human playtesting.
- Give `20_ContentRoadmap.md`'s phase exit criteria a testing checklist to execute against.

## Principles
- **Test the guarantees, not just the features.** A platform behavior isn't "done" when it spawns correctly — it's done when it passes the reachability and fairness guarantees defined in `11_Platforms.md`.
- **Automate what is pure logic; playtest what is feel.** Currency math and reachability geometry are unit-testable; whether a jump "feels gentle" is not — see `16_CodingGuidelines.md` §7.
- **Regression safety over one-time verification.** A guarantee validated once and never re-checked is not actually guaranteed over a long production cycle.

## Detailed Design

### 1. Automated Test Coverage
Per `16_CodingGuidelines.md` §7's testability guidance (plain C# classes behind interfaces), the following are prioritized for unit testing:
- **Reachability math** (`11_Platforms.md` §2, `21_Balancing.md` §2): given a movement/jump config and a gap range, assert every possible generated gap is completable.
- **Currency conversion** (`09_Progression.md` §1, `10_Collectibles.md` §3): Star Fragment → Starlight conversion produces expected, non-negative, non-lossy results.
- **Save serialization round-trip** (`13_SaveSystem.md`): saved data, when written and reloaded, is bit-for-bit equivalent; corrupted-file fallback to backup behaves as specified.
- **Event channel dispatch** (`17_EventSystem.md`): a raised event reaches all registered listeners exactly once, and unregistering stops delivery.

### 2. Manual/Playtest Coverage
- **First-time-player comprehension tests** — validating `00_Vision.md`'s "Easy to Learn" pillar and each new Region's "cold" playtest requirement (`21_Balancing.md` §6).
- **Feel validation** against `08_GameFeel.md`'s qualitative targets (movement weight, jump forgiveness, failure calmness) — inherently subjective and requires structured human feedback, not automation.
- **Emotional tone validation** for the no-dialogue storytelling approach (`03_Lore.md`, `25_RiskAnalysis.md`) — do players understand the restoration theme without being told?
- **Device-diversity testing** across low/mid/high-tier Android and iOS devices for both performance (see `31_PerformanceBudget.md`) and touch-input latency/precision (`08_GameFeel.md`, `25_RiskAnalysis.md`).

### 3. Critical Test Scenarios (Non-Negotiable Before Any Release)
1. App killed mid-save write does not corrupt or lose prior save data (`13_SaveSystem.md` §4).
2. Every shipped Region's platform generation never produces an unreachable gap across an extended (thousands-of-spawns) simulated run.
3. A run ending in fail state always correctly converts and persists Star Fragments to Starlight, even if the app is closed immediately after the Run Summary screen appears.
4. No ad placement (`22_Monetization.md` §1.2) can be triggered more than once per run, and declining it never blocks progression.
5. All achievement triggers (`23_Achievements.md`) fire exactly once per qualifying condition, with no duplicate-unlock or missed-unlock edge cases around app restarts mid-run.

### 4. Regression Process
- Each new platform behavior, Region, or economy change (`26_Backlog.md`) must re-run the reachability and currency-conversion automated suite before merge.
- A lightweight regression checklist (derived from Section 3) is executed before every build submitted for wider playtesting or release.

## Future Expansion
- **Automated device farm integration** for performance regression testing across a matrix of real hardware, once `31_PerformanceBudget.md`'s benchmarks are established.
- **Analytics-informed test prioritization** post-launch, focusing manual testing effort on Regions/features showing the highest fail-rate or drop-off in live data (`32_AnalyticsAndTelemetry.md`).

## Notes
- This document's Section 3 list is the minimum bar for any release candidate — a build failing any of these five scenarios is not release-eligible regardless of how much other content is complete.
- Test cases here must be updated whenever the systems they test (`09_Progression.md`, `11_Platforms.md`, `13_SaveSystem.md`, `22_Monetization.md`, `23_Achievements.md`) are revised.
