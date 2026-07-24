# 17 — Event System

## Purpose
This document defines StarSower's approach to decoupled communication between systems — how a system announces something happened without needing to know who, if anyone, is listening. It covers the current minimal implementation and the target architecture as the number of cross-system reactions grows.

## Goals
- Keep systems decoupled per `14_TechnicalArchitecture.md`'s Dependency Direction rule, avoiding direct references between unrelated systems (e.g., `PlayerController` must never directly reference a future analytics or achievement system).
- Define a consistent, low-overhead pattern for global event broadcasting appropriate for a small mobile game team.
- Provide a clear migration path from the current static event hub to a more scalable, Inspector-friendly event-channel architecture as content volume grows.

## Principles
- **Publishers don't know their subscribers.** A system that raises an event (e.g., `GameOverManager`) has zero knowledge of what reacts to it.
- **Events are for state transitions, not chatter.** The event system is reserved for meaningful, infrequent state changes (game over, fragment collected, Region entered), not per-frame data streams — those remain direct method calls within a system's own boundary.
- **No event should carry hidden gameplay logic.** Event handlers react to and reflect state; they must never be the *only* place a critical gameplay rule is enforced (critical rules live in the owning system, e.g., fail-state detection lives in `GameOverManager`, not scattered across `OnGameOver` subscribers).

## Detailed Design

### 1. Current Implementation
`StarSower.Core.GameEvents` is a minimal static C# event hub (`OnGameOver` currently, per `14_TechnicalArchitecture.md`). Any system may raise or subscribe to these events without a direct reference to the other side. This is intentionally lightweight and sufficient for the project's current scale (one meaningful cross-system event).

### 2. Target Architecture: Event Channels
As StarSower's event surface grows (fragment collected, Region entered, Beacon unlocked, cosmetic equipped, Sky Meter milestone reached — all anticipated from `09_Progression.md`, `10_Collectibles.md`, `12_Regions.md`), the static-event-hub approach does not scale well: it requires code changes to add new event types and offers no Inspector visibility into who's listening to what.

The target architecture is a **ScriptableObject-based event channel** system (see `18_ScriptableObjects.md` for the asset-creation pattern):
- Each distinct event type is represented by a ScriptableObject asset (e.g., a `StarFragmentCollectedEvent` channel), rather than a hardcoded C# event.
- Publishers hold a `[SerializeField]` reference to the channel asset and call a `Raise()`-style method — no code reference to subscribers required.
- Subscribers hold a `[SerializeField]` reference to the same channel asset and register a listener, visible and wireable entirely in the Inspector.
- This makes event wiring inspectable and designer-adjustable (e.g., a designer can point a UI feedback component at a different event channel without an engineer touching code), directly serving the project's "Inspector Friendly" tech pillar.

### 3. Migration Rules
- The existing `GameEvents.OnGameOver` static event remains valid and is not removed casually — per `16_CodingGuidelines.md`'s no-unprompted-refactor rule, migration to the channel architecture happens deliberately, one event at a time, as new event-driven features are built, not as a wholesale rewrite.
- New event types introduced from this point forward should default to the ScriptableObject channel pattern rather than adding further static events to `GameEvents`, to avoid growing the class that this document explicitly plans to phase out as the primary pattern.

### 4. What Belongs on the Event Bus (and What Doesn't)
**Belongs:**
- Game Over (existing)
- Star Fragment Collected (feeds HUD counter, audio, Đóm Sao reaction — see `06_UIUX.md`, `07_Audio.md`, `04_Characters.md`)
- Region Entered (feeds camera zoom moment, music swell, background transition — see `08_GameFeel.md`, `07_Audio.md`)
- Sky Meter Milestone Reached (feeds Hub visual/audio richness — see `09_Progression.md`)
- Beacon Unlocked / Cosmetic Unlocked (feeds Hub UI update, save trigger — see `13_SaveSystem.md`)

**Does not belong (stays as direct method calls within a system):**
- Per-frame input reads (`IInputProvider` polling)
- Physics/movement updates (`PlayerMotor`)
- Camera position updates (`CameraFollowY`)

### 5. Analytics/Telemetry Hook Point
The event channel architecture is also the designated integration point for any future analytics/telemetry system (see `20_ContentRoadmap.md`) — telemetry listens to the same channels as gameplay/UI systems, never requiring gameplay code to call an analytics SDK directly. This keeps telemetry fully removable/mockable without touching gameplay logic.

## Future Expansion
- **Typed payload channels** (e.g., an event carrying "which Region" or "how many fragments") once the base no-payload channel pattern is validated — payload channels follow the same ScriptableObject pattern with a generic or typed variant.
- **Editor debug view** listing all active event channels and their current subscriber counts, to aid debugging as the event surface grows.

## Notes
- This document governs *global, cross-system* communication only. Communication between tightly coupled components within one system (e.g., `PlayerController` calling `PlayerMotor.Move()`) is a direct method call, not an event, and should remain so — see `14_TechnicalArchitecture.md`'s Dependency Direction diagram for the distinction between "system boundary" and "internal composition."
