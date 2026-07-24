# 16 — Coding Guidelines

## Purpose
This document defines the binding coding standards for StarSower's C# codebase. It formalizes the collaboration rules already established for this project into a permanent, reviewable reference, so code quality remains consistent regardless of who is writing it or how long the project runs.

## Goals
- Lock naming, structure, and style conventions so the codebase reads as one voice.
- Formalize the standing engineering rules (no God Classes, SerializeField over hardcoding, composition over inheritance) as enforceable review criteria.
- Define the process discipline for how changes are proposed and made in this project.

## Principles
- **Never break existing, working code.** Changes are additive or precisely scoped; incidental breakage is a defect, not an acceptable side effect.
- **Never refactor unprompted.** Refactors happen only when explicitly requested or when directly required to complete the requested change — not as a drive-by "improvement."
- **Never rename classes, namespaces, or prefabs without necessity.** Renames ripple through scenes, prefabs, and serialized references; they are treated as high-cost changes requiring explicit justification.
- **One class, one responsibility.** If a class's description requires the word "and," it is a candidate for splitting.
- **Composition over inheritance.** New behavior is added via new components attached alongside existing ones, not via deepening a class hierarchy.
- **No hardcoded gameplay values.** Every tunable number is a `[SerializeField]`, never a literal embedded in logic.
- **Explain architecture before implementing.** For any new feature, a short architecture explanation (what classes/interfaces, what folder, how it composes with existing systems) is presented and confirmed before code is written.

## Detailed Design

### 1. Process Discipline (Binding)
1. Before writing code for a new feature, state the intended architecture briefly — affected classes/interfaces, target folder (see `15_ProjectStructure.md`), and how it connects to existing systems (see `14_TechnicalArchitecture.md`'s dependency diagram). Wait for confirmation before implementing.
2. Scope every change to exactly what was requested. If an unrelated improvement is noticed during the work, it is noted for later rather than folded into the current change.
3. Never use a destructive or renaming operation as a shortcut to solve a naming/structure disagreement — raise it explicitly instead.

### 2. Naming Conventions
- **Namespaces:** root `StarSower`, sub-namespace per system folder (`StarSower.Core`, `StarSower.Player`, `StarSower.CameraSystem`, `StarSower.Platform`, `StarSower.Managers`, `StarSower.UI`) — matches `15_ProjectStructure.md` exactly.
- **Interfaces:** prefixed with `I` and named for the capability they expose, not the implementation (`IInputProvider`, not `IKeyboardInput`).
- **Classes:** PascalCase, named for their single responsibility (`GroundChecker`, not `PlayerHelper` or `PlayerUtils`).
- **Private fields:** camelCase, no underscore prefix, always `[SerializeField]` when a value needs Inspector tuning; plain `private` (no serialization) only for pure runtime state.
- **Methods:** PascalCase, verb-led (`Move`, `Jump`, `Recycle`), each doing one clear thing.

### 3. Class Design Rules
- **No God Classes.** A class that reads input, applies physics, and manages UI is three responsibilities in one file and must be split, following the pattern already established between `PlayerController` (orchestration), `PlayerMotor` (physics), and the `IInputProvider` implementations (input).
- **Interfaces at system boundaries.** Any class consumed by another *system* (not just another class in the same folder) is consumed through an interface — see `14_TechnicalArchitecture.md`'s Dependency Direction diagram, which is binding.
- **MonoBehaviours vs. plain classes:** logic that does not need Unity lifecycle callbacks or Inspector exposure should be a plain C# class, not a MonoBehaviour, to keep scene/prefab wiring minimal and testable.
- **RequireComponent for hard dependencies:** if a class cannot function without a sibling component (e.g., a platform behavior requiring `Collider2D`), declare it via `[RequireComponent]` rather than defensive null-checks scattered through the class.

### 4. Inspector & Data Rules
- Every gameplay-tunable value (speed, force, distance, duration, threshold) must be `[SerializeField] private`, with a `[Tooltip]` when its purpose isn't self-evident from the name.
- Values shared across many instances or iterated on frequently by design (Region parameters, platform gap tuning) are migrated to ScriptableObject config assets per `18_ScriptableObjects.md` as that system comes online — new systems should default to ScriptableObject-driven config where the value is design-owned rather than engineering-owned.
- No magic numbers in conditional logic (e.g., comparing against a bare `0.5f` inline) — name the value as a field or constant, even if its default is only used once currently.

### 5. Comments
- Class-level comments are short (1–3 lines) and explain *why* the class exists and what it deliberately does *not* do (its responsibility boundary), not a restatement of its name.
- Inline comments are reserved for non-obvious constraints (a subtle math invariant, a Unity-specific gotcha being avoided) — not for narrating straightforward code.
- No comments referencing a specific past task, ticket, or conversation ("added for X request") — comments describe the code's current reason for being, not its history.

### 6. Error Handling & Validation
- Gameplay code trusts its own internal contracts (a `[RequireComponent]`-guaranteed dependency is never null-checked defensively) — validation is reserved for genuine external boundaries (save data parsing, platform APIs).
- Fail loudly in the Editor (asserts, clear `Debug.LogError`) rather than silently swallowing an invalid state during development; production builds must never crash from a missing optional reference that has a sensible fallback (e.g., `PlayerController` continuing gracefully if a shake source is unassigned).

### 7. Testing Expectations
- Pure logic (reachability math, currency conversion, save serialization) should be structured so it is unit-testable independent of the Unity scene — favor plain C# classes with injected dependencies (via interfaces) for these cases.
- Full test strategy and coverage expectations are owned by a dedicated QA document — see `25_RiskAnalysis.md` and the proposed `30_QATesting.md` (see `28_Glossary.md`/appendix list).

## Future Expansion
- Formal static analysis / lint ruleset once the team grows beyond a size where convention alone is sufficient.
- `asmdef` enforcement of the Dependency Direction diagram (see `14_TechnicalArchitecture.md`), turning an architectural convention into a compiler-enforced boundary.

## Notes
- This document is the formalized, permanent version of the project's standing collaboration rules — any conflict between informal guidance and this document should be resolved by updating this document, which is the document of record going forward.
- Rule violations found in code review are treated as defects to fix before merge, not style nitpicks to defer.
