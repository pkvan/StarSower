# 15 — Project Structure

## Purpose
This document defines the canonical folder structure of the StarSower Unity project — where every type of asset and script belongs. It exists to keep a long-running project navigable as its content volume grows.

## Goals
- Lock a folder structure that matches the architecture described in `14_TechnicalArchitecture.md`.
- Give every future contributor an unambiguous answer to "where does this file go?"
- Keep Unity-specific organizational concerns (Addressables groups, Resources folders) intentional rather than accidental.

## Principles
- **Folder structure mirrors architecture, not asset type alone.** Scripts are organized by system responsibility (`Player`, `Camera`, `Platform`...), not dumped into one flat `Scripts` folder.
- **One canonical location per concept.** There is exactly one place a given kind of asset belongs; ambiguity is a structure defect to be fixed, not tolerated.
- **Documentation lives outside `Assets`.** Design documents are project knowledge, not game content, and must never be imported into Unity's asset database.

## Detailed Design

### 1. Current Structure (Implemented)
```
StarSower/
├── Assets/
│   ├── Scenes/
│   │   └── SampleScene.unity            (current primary gameplay scene)
│   ├── Scripts/
│   │   ├── Core/                        (interfaces + cross-cutting contracts: IInputProvider, IGroundDetector, IPlatformPool, ICameraShake, ICameraZoom, GameEvents)
│   │   ├── Player/                      (PlayerController, PlayerMotor, GroundChecker, KeyboardInputProvider, MobileInputProvider)
│   │   ├── Camera/                      (CameraFollowY, CameraShake, CameraZoom)
│   │   ├── Platform/                    (Platform, PlatformSpawner, PlatformRecycler, SimplePlatformPool)
│   │   ├── Managers/                    (GameOverManager)
│   │   └── UI/                          (OnScreenJoystick, TouchButton)
│   ├── Prefabs/                         (Platform_Basic.prefab, Platform_Wide.prefab)
│   └── Settings/                        (URP render pipeline assets, project template scenes)
├── Documentation/                       (this Design Bible — outside Assets, never imported by Unity)
├── ProjectSettings/
└── Packages/
```

### 2. Folder Responsibilities

| Folder | Contains | Does Not Contain |
|---|---|---|
| `Assets/Scripts/Core` | Interfaces, static cross-cutting utilities (e.g. `GameEvents`) | Any `MonoBehaviour` with gameplay logic |
| `Assets/Scripts/Player` | Everything owning player movement, input abstraction, ground detection | Camera, platform, or UI-specific logic |
| `Assets/Scripts/Camera` | Camera follow, shake, zoom | Gameplay rules (e.g., fail-state logic belongs in `Managers`) |
| `Assets/Scripts/Platform` | Platform behavior, spawning, recycling, pooling | Player-specific logic |
| `Assets/Scripts/Managers` | Cross-system game-state orchestration (fail state, future run/session managers) | Reusable, reference-only interfaces (those belong in `Core`) |
| `Assets/Scripts/UI` | Raw, gameplay-agnostic UI widgets | Anything that reads `PlayerController` or gameplay state directly |
| `Assets/Prefabs` | All prefab assets, sub-organized by system as volume grows (see Future Expansion) | Scene-only, non-reusable setup (that stays in the scene) |
| `Documentation/` | This Design Bible | Any Unity-importable asset |

### 3. Naming Conventions (Cross-Reference)
Script, class, and asset naming rules are owned by `16_CodingGuidelines.md` to avoid duplication; this document governs *placement*, that document governs *naming*.

### 4. Scene Strategy
- StarSower currently uses a single primary scene for gameplay. As Regions (`12_Regions.md`) and the Hub (`02_World.md`) are built out, the project will move to a small, fixed set of scenes (e.g., `Hub.unity`, `Gameplay.unity`) rather than one scene per Region — Region content is data/prefab-driven within the single gameplay scene, consistent with the procedural spawning architecture in `14_TechnicalArchitecture.md`.
- No scene should ever be created as a one-off testing scratch space inside `Assets/Scenes` — temporary test scenes belong in a dedicated `Assets/_Sandbox` folder (see Future Expansion) that is never included in release builds.

## Future Expansion
- **`Assets/Prefabs` sub-folders** (`Prefabs/Platforms`, `Prefabs/UI`, `Prefabs/Characters`) once prefab count grows past current low volume.
- **`Assets/ScriptableObjects`** folder, sub-organized per config type (`RegionConfig`, `PlatformConfig`, cosmetic definitions), introduced alongside the work described in `18_ScriptableObjects.md`.
- **`Assets/Addressables`** group organization, introduced alongside the migration described in `19_ContentPipeline.md`.
- **`Assets/_Sandbox`** folder for engineer scratch scenes/prefabs, explicitly excluded from build settings, to prevent experimental content from leaking into shipped builds.

## Notes
- Any new top-level folder under `Assets/Scripts` requires a corresponding entry in `14_TechnicalArchitecture.md`'s System Map — folder structure and architecture documentation must never drift apart.
- This document must be updated whenever a new folder category is introduced, in the same change that creates it.
