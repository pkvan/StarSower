# 14 — Technical Architecture

## Purpose
This document defines StarSower's engineering architecture: the engine setup, core systems, their responsibilities, and the contracts between them. It is the authoritative technical reference reconciling the current implemented state of the project with the target architecture described by the tech stack in the project brief.

## Goals
- Document the systems that exist today precisely enough that no engineer needs to re-derive them by reading code from scratch.
- Define the target architecture for systems not yet built (data-driven config, real object pooling, Addressables) so future work extends toward a known destination rather than improvising.
- Lock the architectural principles (SOLID, composition over inheritance, interface-driven decoupling) as binding, not aspirational.

## Principles
- **Interfaces at every system boundary.** Any system another system depends on is consumed through an interface, never a concrete class reference, so implementations can change without rippling edits.
- **Composition over inheritance.** New behavior is added by attaching new components, not by growing class hierarchies (see `11_Platforms.md` for the concrete design application of this rule).
- **No God Classes.** Every class has exactly one responsibility. A class that both reads input and applies physics, for example, is a violation and must be split.
- **Inspector-friendly, never hardcoded.** All tunable gameplay values are exposed via `[SerializeField]`, never embedded as magic numbers in code — see `16_CodingGuidelines.md` for the enforced rule.
- **Data-driven where it matters.** Values that designers iterate on frequently (Region parameters, platform gap tuning, jump feel) are targeted for ScriptableObject-driven configuration (see `18_ScriptableObjects.md`) rather than scattered inspector fields, as the project matures.

## Detailed Design

### 1. Engine & Rendering Setup
- **Engine:** Unity 6 (6000.x LTS line), C#.
- **Rendering:** Universal Render Pipeline (URP), 2D Renderer, using URP's `Light2D` for controlled ambient/glow lighting (see `05_ArtDirection.md`).
- **Physics:** Unity's built-in 2D physics (`Rigidbody2D`, `Collider2D`), Continuous collision detection on the player to prevent tunneling through platforms at higher speeds.

### 2. Current System Map (Implemented)
Namespace root: `StarSower`. Folder layout described fully in `15_ProjectStructure.md`; this section describes responsibilities.

**`StarSower.Core`** — interfaces and cross-cutting contracts only, no MonoBehaviours with gameplay logic:
- `IInputProvider` — exposes `Horizontal` and `JumpPressed`. The single abstraction `PlayerController` depends on for input, decoupling it from any specific input source.
- `IGroundDetector` — exposes `IsGrounded`. Decouples jump-permission logic from the specific grounding-detection method.
- `IPlatformPool` — exposes `Get`/`Release` for platform instance lifecycle. Decouples spawning/recycling logic from the specific allocation strategy (currently simple Instantiate/Destroy; see Section 5 for the planned real pooling upgrade).
- `ICameraShake` / `ICameraZoom` — expose a shake-offset/zoom API for future systems (combo feedback, event moments) to drive camera juice without depending on the concrete camera follow implementation.
- `GameEvents` — a minimal static C# event hub (currently exposes `OnGameOver`) for systems that must react to global state changes without a direct reference to the system that raised them. See `17_EventSystem.md` for the planned evolution of this into a broader event-channel architecture.

**`StarSower.Player`**
- `PlayerController` — the sole orchestrator: reads `IInputProvider`, checks `IGroundDetector`, commands `PlayerMotor`. Contains no physics math and no raw input reads itself.
- `PlayerMotor` — owns all `Rigidbody2D` velocity application (move, jump). Contains no input reading and no ground-detection logic.
- `GroundChecker` — implements `IGroundDetector` via a physics overlap check against a dedicated Ground layer, avoiding self-detection against the player's own collider.
- `KeyboardInputProvider` — implements `IInputProvider` via the legacy Input Manager axes; used for editor testing.
- `MobileInputProvider` — implements `IInputProvider` by reading the raw `OnScreenJoystick`/`TouchButton` UI widgets (see `StarSower.UI` below). This is the only class in the project permitted to bridge UI to gameplay input.

**`StarSower.CameraSystem`**
- `CameraFollowY` — owns `transform.position` for the camera exclusively (single-writer principle, see Section 4). Implements SmoothDamp-based upward-only follow with a configurable Dead Zone, and composes an optional `ICameraShake` offset at the final position-assignment step.
- `CameraShake` — implements `ICameraShake`; computes a decaying random offset over time, never writes `transform.position` directly.
- `CameraZoom` — implements `ICameraZoom`; smoothly adjusts `Camera.orthographicSize` only, fully independent of position-owning systems.

**`StarSower.Platform`**
- `Platform` — a marker component guaranteeing a `Collider2D` is present; the composition anchor future platform behaviors (`11_Platforms.md` §3) attach alongside.
- `PlatformSpawner` — decides when and where to spawn the next platform, enforcing the vertical/horizontal gap constraints that guarantee reachability (see `11_Platforms.md` §2). Delegates actual instantiation to `IPlatformPool`.
- `PlatformRecycler` — attached per spawned instance; self-monitors distance below the camera and triggers recycling via `IPlatformPool` (or `Destroy` if no pool is configured).
- `SimplePlatformPool` — the current `IPlatformPool` implementation, using plain `Instantiate`/`Destroy`. Deliberately isolated behind the interface so it can be replaced by a real pooling implementation (Section 5) without touching `PlatformSpawner` or `PlatformRecycler`.

**`StarSower.Managers`**
- `GameOverManager` — compares player position against camera position; raises `GameEvents.OnGameOver` when the configured fail distance is exceeded. Owns no other responsibility (no UI, no save writes) — those must subscribe to the event instead.

**`StarSower.UI`**
- `OnScreenJoystick` — a raw, gameplay-agnostic analog joystick widget exposing a normalized `Horizontal` value.
- `TouchButton` — a raw, gameplay-agnostic touch button exposing a single-frame "was pressed" flag.

Neither UI class references `PlayerController` or any gameplay system — the dependency only flows one direction, from `MobileInputProvider` down into these widgets.

### 3. Dependency Direction (Binding Rule)
```
PlayerController → IInputProvider ← MobileInputProvider → (OnScreenJoystick, TouchButton)
PlayerController → IGroundDetector ← GroundChecker
PlatformSpawner  → IPlatformPool  ← SimplePlatformPool
CameraFollowY    → ICameraShake   ← CameraShake
```
Gameplay-facing classes (`PlayerController`, `PlatformSpawner`, `CameraFollowY`) never reference a concrete UI, pooling, or effects class directly — always through the interface. This diagram is binding: any change that causes an arrow to point the wrong way is an architecture violation and must be corrected before merge.

### 4. Single-Writer Principle
Any Unity `Transform` that multiple systems might want to influence (most notably the camera) has exactly one owning class responsible for writing `transform.position`. Other systems contribute *data* (e.g., a shake offset) that the owner composes in, rather than writing to the transform themselves. This rule prevents feedback loops and undefined per-frame ordering bugs, and applies to any future multi-system transform (e.g., a future player "juice" system must feed `PlayerMotor`, not write to the player transform directly).

### 5. Planned: Real Object Pooling
`IPlatformPool` exists today specifically so pooling can be introduced later without touching spawner/recycler code. The planned upgrade replaces `SimplePlatformPool`'s Instantiate/Destroy with a pre-warmed, reusable instance pool (deactivate/reactivate instead of destroy/recreate), reducing GC pressure and instantiation cost on mobile. This is scoped in `20_ContentRoadmap.md` as a performance milestone, not a day-one requirement.

### 6. Planned: Addressables
Platform prefab variants, Region-specific art/audio sets, and cosmetic assets (see `10_Collectibles.md`, `19_ContentPipeline.md`) are planned to be migrated to Addressables-managed assets, loaded/unloaded per Region transition to control memory footprint on low/mid-tier Android devices. Current prefabs (`Assets/Prefabs/Platform_Basic.prefab`, `Platform_Wide.prefab`) are direct references as a starting point; the migration path is detailed in `19_ContentPipeline.md`.

### 7. Planned: Data-Driven Configuration
Gameplay tuning values (jump force, move speed, platform gap ranges, Region fragment density) currently live as `[SerializeField]` values on individual components. The target architecture (see `18_ScriptableObjects.md`) introduces ScriptableObject config assets (e.g., a per-Region `RegionConfig`) that these components reference, so designers can iterate on Region tuning without touching scenes or code. This is an additive migration — existing `[SerializeField]` fields remain functional defaults and are not removed until each config asset is validated.

## Future Expansion
- Formal `asmdef` boundaries per top-level namespace (`Core`, `Player`, `CameraSystem`, `Platform`, `Managers`, `UI`) once the codebase grows large enough that compile-time isolation becomes valuable — not required at current project size.
- A lightweight service-locator or dependency-injection convention if the number of cross-system interfaces grows beyond what Inspector-wired `[SerializeField]` references can comfortably manage.

## Notes
- This document must be updated in the same change that introduces or removes a system — it is not a historical record, it is the live map of the codebase.
- Any deviation from the Dependency Direction diagram (Section 3) found during code review is a defect, not a style preference.
