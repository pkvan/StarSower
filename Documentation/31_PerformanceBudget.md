# 31 — Performance Budget (Proposed Addition)

## Purpose
This document defines StarSower's concrete mobile performance targets and the budget allocation across rendering, physics, audio, and memory that keeps the game smooth on low/mid-tier Android devices, not just flagship hardware. It was proposed as an addition because `05_ArtDirection.md`, `14_TechnicalArchitecture.md`, and `19_ContentPipeline.md` all reference performance constraints that needed one authoritative numeric home.

## Goals
- Define frame rate, memory, and load-time targets appropriate for a mobile-first, one-thumb game.
- Allocate performance budget across systems so no single system (particles, audio, procedural spawning) can silently consume the whole budget.
- Give `30_QATesting.md`'s device-diversity testing concrete numbers to test against.

## Principles
- **Design for the median device, not the best one.** StarSower's target audience skews toward broad accessibility, not high-end hardware exclusivity.
- **Smoothness over visual maximalism.** A consistently smooth 60fps experience always wins over a higher-fidelity but stuttering one, consistent with `08_GameFeel.md`'s precision-dependent feel.
- **Budget is enforced per system, not just measured globally.** Each major system (Section 2) has its own ceiling so regressions are traceable to their source.

## Detailed Design

### 1. Top-Line Targets
- **Frame rate:** stable 60fps on target-tier devices (defined in Section 4); no dropped frames during active platforming, since even brief stutter directly damages jump-timing precision (`08_GameFeel.md` §2).
- **Cold start to control:** under 5 seconds (already specified in `01_Gameplay.md` §7) — this document owns the technical budget that makes that number achievable.
- **Run transition (Hub → gameplay, gameplay → Run Summary):** under 1 second, no visible loading screen for these transitions given the game's small per-Region asset footprint (`19_ContentPipeline.md`).

### 2. System Budget Allocation
| System | Budget Concern | Constraint |
|---|---|---|
| Rendering (sprites, `Light2D`) | Draw calls, overdraw from layered parallax backgrounds | Batched sprite rendering; glow/bloom limited to diegetically-lit objects only (`05_ArtDirection.md` §3) |
| Particles | GC pressure, overdraw from fragment/collection/ash effects | Small, pooled particle counts (`05_ArtDirection.md` §6); no particle system exceeds a defined max active-particle ceiling |
| Physics | `Rigidbody2D`/`Collider2D` overlap checks, especially `GroundChecker` and platform recycling | Continuous collision limited to the player only; static platforms use simple box colliders, no complex mesh colliders |
| Platform Spawning/Recycling | Instantiate/Destroy GC churn | Addressed structurally by the planned real object pool (`14_TechnicalArchitecture.md` §5) — this is the single highest-priority performance item on the roadmap |
| Audio | Memory footprint of simultaneously loaded Region music/ambience/SFX | Streaming music, decompress-on-load short SFX, Addressables-managed per-Region audio banks (`07_Audio.md` §6, `19_ContentPipeline.md`) |
| Memory (overall) | Total resident asset memory, especially on 2–3GB RAM low-tier devices | Only current + adjacent Region content resident at once (`19_ContentPipeline.md` §2) |

### 3. Object Pooling as the Primary Performance Lever
Because StarSower's core loop continuously spawns and recycles platforms (`11_Platforms.md` §2, `14_TechnicalArchitecture.md`), uncontrolled Instantiate/Destroy churn is the single most likely source of GC-spike frame drops. The `IPlatformPool` abstraction (`14_TechnicalArchitecture.md`) exists specifically so this can be fixed with a pooled implementation without touching spawner/recycler logic — this document flags real pooling as the top-priority performance milestone in `20_ContentRoadmap.md` Phase 3.

### 4. Target Device Tiers
- **Minimum supported tier:** representative low-end Android devices from recent years (entry-level chipsets, 2–3GB RAM) — the game must remain playable and reasonably smooth here, even if not a flawless 60fps in every scenario.
- **Target tier:** mid-range Android/iOS devices from the last 2–3 years — stable 60fps is required here.
- **Aspirational tier:** flagship devices — used to validate headroom for future visual richness, not as the baseline design target.

### 5. Load & Memory Budgets
- Initial app install size and cold-start memory footprint are kept low by shipping only local (non-remote) Addressables content for the base five Regions at launch (`19_ContentPipeline.md` §5), deferring remote content delivery to post-launch.
- Per-Region resident memory (art + audio, per Section 2's Addressables table in `19_ContentPipeline.md`) must stay within a budget validated against the minimum supported device tier's available RAM after OS and Unity engine overhead.

## Future Expansion
- **Automated performance regression testing** on a device farm (see `30_QATesting.md` Future Expansion) once budgets in this document are validated on real hardware.
- **Dynamic quality scaling** (e.g., reduced particle counts on detected low-tier devices) if profiling reveals the minimum tier needs further headroom beyond content-level optimization.

## Notes
- Any new visual or audio feature proposal must specify which budget row in Section 2 it draws from and confirm it does not exceed that row's constraint before being approved for production.
- This document's numeric targets should be revisited once the team has real profiling data from Phase 1 (`20_ContentRoadmap.md`) rather than treated as final without validation.
