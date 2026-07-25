# PROJECT_CONTEXT.md — Starsower

> Tài liệu context chính thức của dự án. Cập nhật đến hết **S1-013 – Biome Presentation System**
> (bao gồm bản vá regression S1-013.1 và cải tiến trình diễn S1-013.2).
> Dùng làm nguồn tham chiếu duy nhất khi tiếp tục phát triển sau compact.
>
> **Ngày cập nhật:** 2026-07-25

---

## 1. PROJECT OVERVIEW

| Mục | Nội dung |
|---|---|
| **Tên dự án** | Starsower |
| **Thể loại** | 2D Vertical Climbing Platformer (mobile, màn hình dọc) |
| **Engine** | Unity 6 — `6000.5.4f1` |
| **Render** | URP 2D Renderer |
| **Input** | Legacy Input Manager (`activeInputHandler: 2` = Both), có joystick ảo cho mobile |
| **Ngôn ngữ** | C# |
| **Thư mục gốc** | `/Users/admin/Documents/Project/StarSower` |
| **Mục tiêu cuối cùng** | Một hành trình leo lên bầu trời liên tục, nơi người chơi khôi phục lại các chòm sao đã lụi tắt bằng những Star Fragment thu thập dọc đường. |

---

## 2. GAME VISION

**Đây là phần không được tự ý thay đổi.** Mọi quyết định thiết kế sau này phải kiểm tra lại với danh sách dưới đây.

- Starsower là **một hành trình duy nhất từ mặt đất lên đỉnh bầu trời**.
- **Level chỉ là các Region của cùng một hành trình** — không phải các màn chơi độc lập.
- **Không tạo cảm giác "qua màn"**. Không có màn hình "Level Complete", không bảng điểm, không nút bấm giữa các region.
- **Goal chỉ là điểm chuyển tiếp** — nghĩa là "bạn đã leo tới khu vực kế tiếp", không phải mục tiêu cuối của game.
- **Auto Transition**: chạm Goal là tự động chuyển sang region mới, liền mạch, mang tính điện ảnh.
- **Không có nút "Next Level"**. Không có nút "Retry" trong luồng chính.
- **Không Combat. Không Enemy. Không Boss.**
- Gameplay tập trung vào đúng 4 thứ:
  - **Platforming**
  - **Khám phá**
  - **Leo cao**
  - **Khôi phục bầu trời**
- **Star Fragment không phải điểm số.** Mỗi mảnh là một mảnh ánh sáng giúp bầu trời sống lại. Người chơi phải cảm thấy mình đang *"gieo lại các vì sao"*.
- Người chơi phải cảm thấy **bầu trời đang dần sống lại nhờ hành trình của mình**.

---

## 3. GAMEPLAY LOOP

```
Spawn
  ↓
Leo
  ↓
Platform (tĩnh / di chuyển / rơi / lò xo)
  ↓
Thu Star Fragment
  ↓  ├─ chạm mốc Chapter → KHÔI PHỤC CHÒM SAO (giữa gameplay, ~1s dừng + trình diễn) → leo tiếp
  ↓
Goal
  ↓
Transition (tự động, không popup)
  ↓
Region mới (background + màu trời đổi mượt, hiện tên khu vực ~2s)
  ↓
Tiếp tục leo
```

Điểm cần nhớ: **sự kiện khôi phục chòm sao xảy ra GIỮA màn chơi lúc nhặt sao**, không phải lúc chạm Goal. Nó không chuyển scene, không mở menu.

**Trình tự sự kiện khôi phục** *(S1-013.2 — tên và hình chạy song song)*:

```
t=0.0   khoá điều khiển, dừng 1 giây
t=1.0   ┌ vẽ chòm sao: trời tối → sao sáng dần → nối nét   (= AnimationDuration)
        └ fade in TÊN + description                        (0.6s)  ← cùng khung hình
        giữ nguyên cả hai                                   (1.0s)
        ┌ chòm sao tan                                      (0.8s)
        └ tên tan                                           (0.8s)  ← cùng khung hình
        trả quyền điều khiển
```

Tổng thời gian khoá: Lyra 6.3s · Cassiopeia 7.8s · Orion 9.8s.

---

## 4. ARCHITECTURE

### 4.1 Nguyên tắc nền

- **Hướng phụ thuộc:** `Core ← Systems ← Managers/Level ← UI`. Tầng dưới không biết tầng trên.
- **Single-Writer Principle:**
  - Chỉ `PlayerMotor` được ghi `Rigidbody2D` của Player.
  - Chỉ `CameraFollow2D` được ghi `transform.position` của Main Camera (trừ lúc `LevelFlowManager` chủ động **tắt** component đó từ bên ngoài để tự lái camera).
  - Chỉ `ProgressManager` được ghi file save.
  - Chỉ `BackgroundManager` được ghi các `SpriteRenderer` nền *(S1-013)*.
  - Chỉ `SkyManager` được ghi Sky Plane và `Camera.backgroundColor` *(S1-013)*.
- **Event hub:** `GameEvents` (static) để các hệ thống không tham chiếu trực tiếp lẫn nhau.
- **Interface để thay thế:** `ITransitionEffect`, `IConstellationRestoreSequence`, `IInputProvider`, `IGroundDetector`, `ILaunchable`, `ICameraTarget/Shake/Zoom`, `IPlatformPool`.
- **Không Singleton.** Mọi phụ thuộc gán qua `[SerializeField]` trong Inspector.

### 4.2 Danh sách script (76 file)

#### Core — `StarSower.Core`
| File | Vai trò |
|---|---|
| `GameEvents.cs` | Hub sự kiện static: `OnGameOver`, `OnLevelComplete(float)`, `OnLevelCompleted` |
| `IInputProvider.cs` | Hợp đồng nguồn input (bàn phím / cảm ứng) |
| `IGroundDetector.cs` | Hợp đồng phát hiện mặt đất |
| `ILaunchable.cs` | Hợp đồng "có thể bị bắn lên" (dùng cho Spring) |
| `ICameraTarget/Shake/Zoom.cs` | Hợp đồng camera |
| `ITransitionEffect.cs` | Hợp đồng hiệu ứng che/mở màn hình |
| `IPlatformPool.cs` | Hợp đồng object pool platform |
| `PlayerMovementState.cs` | Enum trạng thái di chuyển |
| `DebugOverlaySuppressor.cs` | Tắt overlay debug |

#### Player — `StarSower.Player`
| File | Vai trò |
|---|---|
| `PlayerController.cs` | Điều phối input → jump/motor. Có `SetMovementLocked(bool)` |
| `PlayerMotor.cs` | **Nơi duy nhất** ghi `Rigidbody2D` |
| `PlayerJumpController.cs` | Jump buffer + coyote time |
| `GroundChecker.cs` | Phát hiện mặt đất |
| `PlayerMovementStateMachine.cs` | Máy trạng thái di chuyển |
| `InputManager.cs`, `KeyboardInputProvider.cs`, `MobileInputProvider.cs` | Nguồn input |

#### Camera — `StarSower.CameraSystem`
| File | Vai trò | Trạng thái |
|---|---|---|
| `CameraFollow2D.cs` | Camera bám Player (deadzone X, smooth Y) | Đang dùng |
| `CameraFollowY.cs` | Bản chỉ bám trục Y | **Đã tắt** (`m_Enabled: 0`) |
| `CameraShake.cs`, `CameraZoom.cs`, `TransformCameraTarget.cs` | Hỗ trợ | Có sẵn, chưa dùng trong content |

#### Platform — `StarSower.Platform`
| File | Vai trò | Trạng thái |
|---|---|---|
| `Platform.cs` | Lớp nền | Đang dùng |
| `MovingPlatform.cs` | Nền di chuyển | Đang dùng |
| `FallingPlatform.cs` | Nền rơi sau khi đứng lên | Đang dùng |
| `SpringPlatform.cs` | Nền lò xo | Đang dùng |
| `OneWayPlatform.cs` | Nền đi xuyên từ dưới | Đang dùng |
| `BreakablePlatform.cs` | Nền vỡ | Có code, chưa dùng trong content |
| `PlatformStandDetector.cs` | Phát hiện Player đứng lên | Đang dùng |
| `PlatformSpawner.cs`, `PlatformRecycler.cs`, `SimplePlatformPool.cs` | Sinh nền vô hạn | **Đã tắt** (GameObject `m_IsActive: 0`) — level giờ là bố cục thủ công |

#### Collectibles — `StarSower.Collectibles`
| File | Vai trò |
|---|---|
| `StarFragment.cs` | Sao xoay + nhấp nhô, trigger thu thập, tự huỷ |
| `CollectibleManager.cs` | Đếm tổng sao **tự động** trong scene, phát `OnCollectedChanged(collected, total)` |

#### Constellation — `StarSower.Constellations` *(S1-012)*
| File | Vai trò |
|---|---|
| `ChapterData.cs` (SO) | Cấu hình 1 chapter: tên, tổng fragment, danh sách chòm sao theo mốc tăng dần |
| `ChapterDatabase.cs` (SO) | Tra `ChapterData` từ `chapterId` |
| `ConstellationData.cs` (SO) | 1 chòm sao: id, tên, icon, mốc fragment, hình dạng, animation duration, effect scale, particle, audio |
| `ChapterProgressManager.cs` | **Cộng dồn fragment toàn chapter**, phát hiện vượt mốc. Không vẽ, không ghi đĩa |
| `ConstellationManager.cs` | **Giữ nhịp chung** sự kiện khôi phục: khoá điều khiển → vẽ chòm sao *song song* với thẻ tên → giữ → cả hai cùng tan → mở khoá |
| `ConstellationUI.cs` | Dòng chữ nhỏ `★☆☆  12 / 53` trên HUD |
| `ConstellationNameCard.cs` | Thẻ tên + description giữa màn hình *(S1-013.2)*. `FadeIn(data)` / `FadeOut(duration)` |
| `IConstellationRestoreSequence.cs` | Hợp đồng trình diễn: `Reveal(data)` + `Dismiss(duration)` |
| `ConstellationRestoreSequence.cs` | Bản trình diễn **placeholder**, dựng Canvas bằng code |

#### Biome — `StarSower.Biome` *(S1-013)*
| File | Vai trò |
|---|---|
| `RegionData.cs` (SO) | Diện mạo 1 khu vực: tên, sky gradient, màu nền camera, danh sách lớp background, cloud density, music/ambient |
| `BiomeManager.cs` | Điều phối: áp nền + trời trong `Awake()`, phát audio, ghi nhớ region cho lần chuyển kế |
| `BackgroundManager.cs` | **Nơi duy nhất** ghi các `SpriteRenderer` nền. Hỗ trợ n lớp |
| `SkyManager.cs` | **Nơi duy nhất** ghi Sky Plane + `Camera.backgroundColor`. Nướng `Gradient` thành `Texture2D` lúc chạy |
| `BiomeSession.cs` | Static, nhớ region vừa rời để blend màu trời qua ranh giới scene. **Không ghi vào save** |

#### Level & Flow — `StarSower.Level`
| File | Vai trò | Trạng thái |
|---|---|---|
| `GoalController.cs` | **Chỉ** phát `GameEvents.RaiseLevelCompleted()`. Không load scene, không lưu, không điều khiển UI | Đang dùng |
| `LevelFlowManager.cs` | Điều phối toàn bộ trình tự đến/đi của region. Lấy tên khu vực từ `BiomeManager` nếu có gán, nếu không thì dùng `regionDisplayName` cũ | Đang dùng |
| `LevelManager.cs` | Điều hướng scene, biết level hiện tại | Đang dùng |
| `LevelDefinition.cs` | Dữ liệu 1 level: `levelId`, `displayName`, `sceneName`, `chapterId` | Đang dùng |
| `LevelDatabase.cs` (SO) | Danh sách level — **không hardcode số lượng** | Đang dùng |
| `ProgressManager.cs` | Diễn giải save; **nơi duy nhất ghi đĩa** | Đang dùng |
| `LevelTimer.cs` | Đếm thời gian màn | Đang dùng |
| `LevelCompleteUI.cs` | Màn hình Level Complete cũ | **Đã nghỉ hưu** — GameObject tắt, code còn để biên dịch. Trái vision |

#### Transition — `StarSower.Transition`
| File | Vai trò |
|---|---|
| `SceneTransitionController.cs` | Chọn kiểu fade (Color/Cloud/Light), `PlayIn/PlayOut/SnapCovered` |
| `TransitionEffectBase.cs` | Logic fade dùng chung |
| `ColorFadeEffect.cs` / `CloudFadeEffect.cs` / `LightFadeEffect.cs` | 3 phong cách fade |

#### Persistence — `StarSower.Persistence`
| File | Vai trò |
|---|---|
| `SaveData.cs` | Dữ liệu thuần: `levels[]`, `chapters[]`, `constellations[]`, `currentChapterId`, `lastPlayedLevelId`, `totalStarFragmentsCollected`, `totalPlayTimeSeconds` |
| `SaveManager.cs` | I/O JSON tại `Application.persistentDataPath`. **Chỉ `ProgressManager` được gọi** |

#### UI — `StarSower.UI`
| File | Vai trò | Trạng thái |
|---|---|---|
| `CollectibleHUD.cs` | HUD `⭐ x/y` của level | Đang dùng |
| `RegionIntroUI.cs` | Tên khu vực fade in → giữ 2s → fade out | Đang dùng |
| `OnScreenJoystick.cs`, `TouchButton.cs` | Input cảm ứng | Đang dùng |
| `LevelTitleView.cs` | Tiêu đề màn | GameObject `TitleRoot` đang tắt |
| `LevelSelectController.cs`, `LevelSelectEntryView.cs` | Màn chọn level | **Cô lập** — còn code nhưng không có lối vào |

#### Managers — `StarSower.Managers`
| File | Vai trò | Trạng thái |
|---|---|---|
| `GameOverManager.cs` | Xử lý thua | **Đã tắt** — thiết kế chốt là không chết khi rơi |
| `LevelIntroSequence.cs` | Intro màn cũ | **Đã tắt** — thay bằng `RegionIntroUI` |

#### Effects — `StarSower.Effects`
`GroundImpactVFX.cs` (bụi khi tiếp đất), `SpringLaunchVFX.cs` (hiệu ứng lò xo) — đều là placeholder.

### 4.3 Scenes

| Scene | Region | Build Index |
|---|---|---|
| `SampleScene.unity` | Forgotten Forest | 0 |
| `Level_02.unity` | Cloud Garden | 1 |
| `Level_03.unity` | Sky Ruins | 2 |
| `Level_04.unity` | Aurora Cliffs | 3 |
| `Level_05.unity` | Moon Gate | 4 |

Tất cả đã đăng ký trong `ProjectSettings/EditorBuildSettings.asset`.

**Mỗi scene đều chứa cùng một bộ object (fileID giống hệt nhau giữa 5 scene):**

| Object | fileID | Nội dung |
|---|---|---|
| `ConstellationSystem` | `869000001` | `ChapterProgressManager` · `ConstellationManager` · `ConstellationRestoreSequence` · `ConstellationUI` |
| `ConstellationProgressLabel` | `869000101` | Text dưới `Canvas_HUD` |
| `BiomeSystem` *(S1-013)* | `870000001` | `BackgroundManager` · `SkyManager` · `BiomeManager` |
| `SkyPlane` *(S1-013)* | `870000101` | Con của **Main Camera**, `sortingOrder -300`, local z = +20 |
| `Canvas_ConstellationName` *(S1-013.2)* | `871000001` | `CanvasGroup` + `ConstellationNameCard`, `sortingOrder 210`, 2 Text con |

`BackgroundPlane` (`867000001`) có sẵn từ trước giờ được `BackgroundManager` quản làm **lớp nền 0**.

### 4.4 Data Objects (ScriptableObject / Prefab)

| Asset | Loại | Nội dung |
|---|---|---|
| `LevelDatabase.asset` | SO | 5 level, tất cả `chapterId: chapter_01` |
| `ChapterDatabase.asset` | SO | Chứa `Chapter_01` |
| `Chapter_01.asset` | SO | `chapter_01`, tổng 53 fragment, 3 chòm sao |
| `Constellation_Lyra.asset` | SO | Mốc **12**, 5 sao, 6 nét, vẽ 3.5s, scale 1.0, *"The Harp"* |
| `Constellation_Cassiopeia.asset` | SO | Mốc **30**, 5 sao, 4 nét, vẽ 5.0s, scale 1.35, *"The Seated Queen"* |
| `Constellation_Orion.asset` | SO | Mốc **53**, 7 sao, 6 nét, vẽ 7.0s, scale 1.8, *"The Hunter"* |
| `Region_ForgottenForest.asset` | SO | Trời bình minh → xanh, nền xanh rừng (α 0.55), cloud 0.15 |
| `Region_CloudGarden.asset` | SO | Trời trắng → xanh nhạt, nền trắng xanh (α 0.45), cloud 0.85 |
| `Region_SkyRuins.asset` | SO | Trời xám lam → xanh đêm, nền tím đá (α 0.60), cloud 0.25 |
| `Region_AuroraCliffs.asset` | SO | Trời tím → tím đen, nền tím đậm (α 0.55), cloud 0.35 |
| `Region_MoonGate.asset` | SO | Trời xanh đêm → đen, nền đen xanh (α 0.65), cloud 0.10 |
| `StarFragment.prefab` | Prefab | Sao thu thập được |
| `Platform_Basic.prefab` / `Platform_Wide.prefab` | Prefab | Nền cơ bản |
| `LevelSelectEntry.prefab` | Prefab | Dòng trong Level Select (đang cô lập) |

### 4.5 Save System

- Định dạng **JSON** qua `JsonUtility`, lưu tại `~/Library/Application Support/DefaultCompany/StarSower/starsower_save.json`.
- `SaveManager` chỉ biết đọc/ghi, không hiểu ý nghĩa dữ liệu.
- `ProgressManager` là **lớp diễn giải duy nhất** và là **nơi ghi đĩa duy nhất**.
- `ChapterProgressManager` giữ *luật* cộng dồn/mốc, rồi đưa kết quả cuối cho `ProgressManager` ghi qua `WriteChapterProgress(...)`.
- Save mới thêm field vẫn tương thích ngược: file cũ thiếu field thì giữ giá trị khởi tạo.
- **Hai chiều ghi tách bạch — đừng gộp lại** *(bài học S1-013.1)*:
  - `WriteChapterProgress(...)` — **chỉ đi lên**. `completed` dùng `||`, constellation chỉ gán `restored = true`. Dùng trong lúc chơi bình thường, để tiến trình không tự tụt.
  - `ResetChapterProgress(...)` — **chỉ đi xuống**. Fragment về 0, `completed` về `false`, constellation *của riêng chapter đó* về `false`. Dùng khi bắt đầu lại chapter.
- **`BiomeSession` cố tình KHÔNG nằm trong save** — nó là trạng thái một phiên chơi, không phải tiến trình người chơi.

### 4.6 Bộ số đang dùng (tham chiếu nhanh)

| Component | Giá trị |
|---|---|
| `PlayerMotor` | moveSpeed 5 · acceleration 60 · deceleration 80 · airControl 0.8 · jumpForce 12 · gravityMultiplier 1 · lowJumpMultiplier 2 · minAscentGraceTime 0.08 · fallMultiplier 2.5 |
| `PlayerJumpController` | jumpBufferTime 0.15 · coyoteTime 0.15 |
| `GroundChecker` | groundLayer bit 256 · minGroundNormalY 0.5 |
| `CameraFollow2D` | offset (0, 1) · maxFollowSpeed 30 · deadZoneWidth 2 · smoothTimeX 0.25 · smoothTimeY 0.12 |
| `LevelFlowManager` | cameraDelay 0.4 · driftDistance 3 · driftDuration 0.6 · transitionHold 0.3 · autoLoadNextScene ✔ |
| `SceneTransitionController` | fadeType Color · fadeDuration 0.8 |
| `RegionIntroUI` | fadeIn 0.6 · hold 2 · fadeOut 0.6 |
| `ConstellationManager` | pauseBeforeRestore 1.0 · **holdAfterReveal 1.0** · **fadeOutDuration 0.8** |
| `ConstellationRestoreSequence` | tỉ lệ chặng vẽ **0.2 / 0.3 / 0.2** (sky / stars / lines) · starSize 14 · lineThickness 2 · sortingOrder 200 |
| `ConstellationNameCard` | fadeInDuration 0.6 · nameFormat `{0}` · descFormat `"{0}"` · sortingOrder 210 |
| `BiomeManager` | skyTransitionDuration 1.5 |
| `SkyManager` | skySize (30, 24) · gradientSteps 64 |
| `BackgroundManager` | 1 lớp (`BackgroundPlane`) · cloudLayerIndex **-1** (chưa có lớp mây) |

---

## 5. COMPLETED SPRINTS

### S1-001 — Foundation & Architecture
- **Objective:** Dựng nền kiến trúc SOLID, tách Core/Player/Camera/Platform.
- **Result:** Hệ interface (`IInputProvider`, `IGroundDetector`, `ICameraTarget`…), quy tắc hướng phụ thuộc, `GameEvents`.
- **Không nên đổi:** Hướng phụ thuộc; quy tắc không Singleton; mọi phụ thuộc qua `[SerializeField]`.

### S1-002 — Player Movement
- **Objective:** Di chuyển ngang mượt, có gia tốc/giảm tốc, điều khiển trên không.
- **Result:** `PlayerMotor` + `PlayerController` + `PlayerMovementStateMachine`.
- **Không nên đổi:** `PlayerMotor` là nơi duy nhất ghi `Rigidbody2D`.

### S1-003 — Jump System
- **Objective:** Nhảy đã tay, tha thứ lỗi bấm.
- **Result:** `PlayerJumpController` với jump buffer + coyote time; `GroundChecker`.
- **Không nên đổi:** Bộ số jump đã cân; coyote/buffer time.

### S1-004 — Camera System
- **Objective:** Camera bám nhân vật khi leo dọc.
- **Result:** `CameraFollow2D` (deadzone ngang, smooth dọc), `CameraShake`, `CameraZoom`.
- **Không nên đổi:** Chỉ `CameraFollow2D` ghi vị trí camera.

### S1-005 — Platform Mechanics
- **Objective:** Bộ nền đa dạng.
- **Result:** `MovingPlatform`, `FallingPlatform`, `SpringPlatform`, `OneWayPlatform`, `BreakablePlatform`, `PlatformStandDetector`.
- **Không nên đổi:** Hành vi các nền đã cân; `ILaunchable` cho Spring.

### S1-006 — Mobile Input
- **Objective:** Chơi được trên mobile.
- **Result:** `OnScreenJoystick`, `TouchButton`, `MobileInputProvider`.
- **Không nên đổi:** Nguồn input phải qua `IInputProvider`.

### S1-007 — First Playable Level
- **Objective:** Một màn chơi hoàn chỉnh 2–3 phút.
- **Result:** Bố cục màn đầu, `GoalController`, `LevelCompleteUI`, `LevelTimer`, VFX tiếp đất/lò xo.
- **Không nên đổi:** `GoalController` phải tách khỏi UI. *(`LevelCompleteUI` sau đó bị vision thay thế.)*

### S1-008 — Star Fragments & Collectibles
- **Objective:** Hệ thu thập đầu tiên.
- **Result:** `StarFragment` (xoay + nhấp nhô), `CollectibleManager` (đếm tự động), `CollectibleHUD`.
- **Không nên đổi:** **Không hardcode số sao**; Goal không quản Collectible; HUD cập nhật qua event.

### S1-008.1 — Goal Completion Flow
- **Objective:** Chuẩn hoá luồng chạm Goal.
- **Result:** `PlayerController.SetMovementLocked()` (khoá di chuyển, **không** tắt animation), camera dừng bám, fade nhẹ.
- **Không nên đổi:** **Không bắt buộc thu hết sao để hoàn thành**; Goal luôn cho qua; không reload scene tức thì.

### S1-009 — Level Flow & World Progression
- **Objective:** Tiến trình giữa các level + lưu.
- **Result:** `SaveManager`, `SaveData`, `ProgressManager`, `LevelDefinition`, `LevelDatabase`, `LevelManager`, `LevelSelectController`.
- **Không nên đổi:** Tách 4 lớp Save/Progress/Level/UI; **không hardcode số level**; `ProgressManager` là nơi ghi đĩa duy nhất.

### S1-010 — Chapter 1: Vertical Slice
- **Objective:** 5 region dạy dần mechanic.
- **Result:** 5 scene với chủ đề, màu sắc, bố cục riêng; mỗi region giới thiệu 1 mechanic mới.
- **Không nên đổi:** Không sửa Player/Camera/Platform/Collectible khi làm content; **không tự thêm boss/enemy/combat/mechanic ngoài roadmap**.

### S1-011 — Seamless Journey Transition
- **Objective:** Chuyển region liền mạch, không UI cắt ngang.
- **Result:** `LevelFlowManager`, `SceneTransitionController`, 3 hiệu ứng fade, `RegionIntroUI`. `GoalController` rút gọn còn **chỉ phát 1 event**. `LevelCompleteUI` nghỉ hưu.
- **Không nên đổi:** Goal không load scene / không lưu / không điều khiển UI. Không popup, không Retry, không Next Level.

### S1-012 — Constellation Restoration System (Chapter 1)
- **Objective:** Hệ thống khôi phục bầu trời đầu tiên; Star Fragment thành mảnh ánh sáng chứ không phải điểm.
- **Result:**
  - `ChapterData` / `ChapterDatabase` / `ConstellationData` — dữ liệu, không hardcode Chapter 1.
  - `ChapterProgressManager` — cộng dồn fragment toàn chapter, phát hiện vượt mốc **12 / 30 / 53**.
  - `ConstellationManager` — sự kiện khôi phục giữa gameplay: dừng ~1s → trình diễn → particle/audio → chơi tiếp. **Không chuyển scene, không menu.**
  - `ConstellationUI` — dòng nhỏ `★☆☆  12 / 53`.
  - `ConstellationRestoreSequence` — trình diễn placeholder dựng bằng code.
  - Save mở rộng: `currentChapterId`, `chapters[]` (fragment + completed), `constellations[]` (restored).
- **Không nên đổi:**
  - Fragment đếm **lúc nhặt**, không phải lúc chạm Goal.
  - **Không reset fragment khi qua level** — cộng dồn toàn chapter.
  - Khôi phục **không** chuyển scene, **không** mở menu, **không** popup.
  - `ChapterProgressManager` không ghi đĩa; `ProgressManager` vẫn là nơi duy nhất.
  - Không sửa Player / Camera / Platform / Transition / Goal.
- **Ghi chú lịch sử:** bản đầu của S1-012 gắn khôi phục vào lúc chạm Goal và sửa `LevelFlowManager`. Bản này đã **gỡ bỏ hoàn toàn** — `LevelFlowManager` trở về đúng trạng thái S1-011. Hai class `ConstellationDatabase` và `FragmentTracker` của bản đầu đã bị xoá, thay bằng `ChapterDatabase` và `ChapterProgressManager`.

### S1-013 — Biome Presentation System
- **Objective:** Mỗi Region có bản sắc hình ảnh riêng: background, sky gradient, màu sắc, không khí. **Không thêm mechanic nào.**
- **Result:**
  - `RegionData` (SO) — toàn bộ diện mạo 1 khu vực trong 1 asset. Thêm Region mới = tạo asset + gán vào scene, không sửa code.
  - `BiomeManager` / `BackgroundManager` / `SkyManager` — điều phối / nền / trời, mỗi class một việc.
  - `BiomeSession` — nhớ region trước để **bầu trời đổi màu mượt qua ranh giới scene**, không nhảy màu.
  - `SkyManager` nướng `Gradient` thành `Texture2D` lúc chạy → **không cần asset ảnh, không cần shader**.
  - 5 asset Region cho 5 khu vực.
  - Lớp nền cũ hạ alpha xuống 0.45–0.65 để lộ bầu trời phía sau — đây mới là thứ tạo cảm giác "tầng trời".
- **Quyết định đáng nhớ:** `BiomeManager` áp biome trong **`Awake()`**, tức trước khung hình đầu tiên. Nhờ vậy **không phải sửa Transition** mà vẫn đạt đúng yêu cầu "sau khi fade xong thì đổi background". `SkyPlane` là **con của Main Camera** ở mức scene nên không script nào ghi vào transform camera — quy tắc Single-Writer còn nguyên.
- **Không nên đổi:** Không đưa logic biome vào `PlayerController`. Không ghi `BiomeSession` xuống save. `BackgroundManager`/`SkyManager` phải "ngu" — chỉ thi hành, không tự chọn thời điểm.
- **File cũ bị sửa:** chỉ `LevelFlowManager.cs`, thuần cộng thêm 4 chỗ (`using`, field `biomeManager`, hàm `ResolveRegionName()`, 1 lời gọi). Để trống `biomeManager` thì hành vi y hệt trước sprint.

### S1-013.1 — Regression Fix: Chapter Restart
- **Objective:** Sửa lỗi tiến trình Constellation "bị reset / không còn hiển thị".
- **Chẩn đoán:** **Không phải do S1-013.** `git diff` cho thấy scene chỉ +179/−1 dòng (dòng xoá duy nhất là `m_Children: []` của camera) và S1-013 không chạm file constellation nào. Save file lúc đó vẫn nguyên `53/53` + 3 chòm sao `restored: true` — dữ liệu chưa từng mất.
- **Nguyên nhân thật (lỗi tiềm ẩn của S1-012, chỉ nổ ở lần chơi THỨ HAI):** `restartChapterOnFirstLevel` gọi `WriteChapterProgress()` — một hàm **chỉ biết đi lên**. Fragment về 0 nhưng cờ `restored` của chòm sao kẹt `true` vĩnh viễn. Sang region kế, `restoredIds` nạp lại đủ 3 chòm sao cũ → HUD nhảy `☆☆☆` → `★★★` và **không mốc nào bắn được nữa**.
- **Result:** Thêm `ProgressManager.ResetChapterProgress()` — phép ghi đi xuống tường minh, tách hẳn khỏi phép ghi đi lên. `ChapterProgressManager` gọi nó khi `restarting`. `ConstellationUI` thêm `Start()` refresh phòng hờ.
- **Đã xác nhận bằng save file:** sau bản vá, chơi lại cho ra `fragmentsCollected: 16`, `completed: false`, `lyra: true`, `cassiopeia/orion: false` — đúng như thiết kế.
- **Không nên đổi:** **Không gộp hai hàm ghi save lại làm một.** Đó chính là cái đã sinh ra bug.

### S1-013.2 — Constellation Restoration Presentation
- **Objective:** Tên chòm sao phải hiện **cùng lúc** với nét vẽ, không phải sau khi vẽ xong.
- **Result:**
  - `IConstellationRestoreSequence`: `Play()` → **`Reveal()` + `Dismiss(duration)`**. `Reveal()` vẽ xong thì giữ nguyên, không tự tan.
  - `ConstellationNameCard`: `Show()` → **`FadeIn(data)` + `FadeOut(duration)`**.
  - `ConstellationManager` trở thành **người giữ nhịp chung**: chạy hai coroutine song song bằng `StartCoroutine` (không `yield`), sở hữu `holdAfterReveal` và `fadeOutDuration`.
  - `ConstellationData` thêm `description` (placeholder: The Harp / The Seated Queen / The Hunter).
- **Quyết định đáng nhớ:** `fadeOutDuration` là **một con số dùng chung** do Manager truyền xuống cả hai. Nếu để mỗi bên tự tính theo trọng số riêng, Lyra tan trong 0.62s còn Orion 1.24s trong khi tên cố định 0.6s — sẽ lệch.
- **Hệ quả:** `animationDuration` trong `ConstellationData` giờ mang nghĩa sạch: **đúng bằng thời gian vẽ**, không lẫn phần giữ và phần tan.
- **Không nên đổi:** Không gộp `Reveal`/`Dismiss` trở lại thành một hàm trọn gói — làm vậy là mất khả năng đồng bộ tên với hình.

---

## 6. CURRENT FEATURES

### Player
- Di chuyển ngang có gia tốc / giảm tốc, điều khiển trên không (hệ số 0.8).
- Nhảy lực 12, `fallMultiplier` 2.5, `lowJumpMultiplier` 2 (nhảy ngắn khi nhả sớm).
- **Coyote Time** 0.15s — vẫn nhảy được sau khi rời mép.
- **Jump Buffer** 0.15s — bấm sớm vẫn ăn.
- Máy trạng thái di chuyển (Idle / Run / Jump / Fall).
- Khoá di chuyển qua `SetMovementLocked()` mà **không tắt animation**.

### Camera
- Bám Player: deadzone ngang 2 đơn vị, smooth dọc 0.12s, offset (0, 1), tốc độ tối đa 30.
- **Camera Fall**: bám xuống khi rơi.
- Camera lướt lên thêm 3 đơn vị trong 0.6s khi chạm Goal (do `LevelFlowManager` tạm chiếm quyền).
- `CameraShake` / `CameraZoom` sẵn sàng nhưng chưa dùng trong content.

### Platform
- Platform tĩnh (`Platform_Basic`, `Platform_Wide`).
- **Moving Platform** — dạy ở Cloud Garden.
- **Falling Platform** — dạy ở Sky Ruins.
- **Spring Platform** — dạy ở Aurora Cliffs.
- One-Way Platform.
- Breakable Platform (có code, chưa dùng).

### Collectibles
- Star Fragment xoay (90°/s) + nhấp nhô (biên độ 0.15, tốc độ 2).
- Trigger thu thập, không cản di chuyển, không thu lại được.
- Tổng số đếm **tự động** từ scene.

### Goal & Auto Transition
- **Goal** chỉ phát 1 event, không làm gì khác.
- Trình tự: khoá input → đứng yên 0.4s → camera lướt lên 0.6s → fade che 0.8s → giữ 0.3s → lưu → load region kế.
- 3 phong cách fade đổi được trong Inspector: **Color / Cloud / Light**.
- **Region Intro**: tên khu vực fade in 0.6s → giữ 2s → fade out 0.6s, không cần bấm gì.
- Không popup, không Retry, không Next Level.

### Save
- JSON tại `persistentDataPath`.
- Lưu: level đã mở, sao mỗi level, chapter hiện tại, fragment mỗi chapter, chòm sao đã khôi phục, chapter completed, tổng fragment toàn game, tổng thời gian chơi, `lastPlayedLevelId`.
- Lưu **ngay lập tức** khi chạm Goal và mỗi khi nhặt được sao.

### Constellation Restoration
- Fragment cộng dồn toàn chapter, hiển thị `★☆☆  12 / 53` trên HUD.
- Chạm mốc → dừng gameplay ~1s → chòm sao hiện dần **đồng thời với tên + description** → giữ 1s → cả hai cùng tan → chơi tiếp.
- Mốc sau hiệu ứng dài hơn và to hơn (Animation Duration + Effect Scale trong data).
- Tên và description lấy từ `ConstellationData`, **không hardcode**.
- Particle prefab + Audio clip đã có ô Inspector, để trống thì bỏ qua êm.

### Biome Presentation *(S1-013)*
- Mỗi Region có background, sky gradient, màu nền camera riêng — tất cả trong 1 asset `RegionData`.
- **Bầu trời đổi màu mượt 1.5s khi sang Region mới**, không nhảy màu đột ngột.
- Gradient nướng thành texture lúc chạy → designer chỉnh trong Inspector là thấy đổi ngay, không cần asset ảnh.
- Hệ thống nền hỗ trợ **n lớp**, sẵn sàng cho parallax (hiện mỗi scene mới dùng 1 lớp).
- Region Intro dùng lại `RegionIntroUI` sẵn có từ S1-011, nguồn tên giờ là `RegionData`.

### Chapter Progress
- Chapter suy ra từ `chapterId` của level, không hardcode.
- Ô **Restart Chapter On First Level** (mặc định **bật**): vào region đầu của chapter thì fragment về 0 và các chòm sao khôi phục lại từ đầu — để mỗi lần chơi lại đều được trải nghiệm trọn vẹn.
- `chapters[].completed` đánh dấu khi chạm mốc cuối.

### Mobile
- Joystick ảo + nút nhảy cảm ứng, cùng lúc hỗ trợ bàn phím.

---

## 7. CURRENT CONTENT

### Chapter hiện có

| Chapter | Id | Số region | Tổng Star Fragment |
|---|---|---|---|
| Chapter 1 | `chapter_01` | 5 | **53** |

### Các Level / Region

| # | Level Id | Region | Scene | Star Fragment | Mechanic giới thiệu |
|---|---|---|---|---|---|
| 1 | `level_01` | **Forgotten Forest** | `SampleScene` | 10 | Movement, Jump, thu thập sao |
| 2 | `level_02` | **Cloud Garden** | `Level_02` | 10 | Moving Platform |
| 3 | `level_03` | **Sky Ruins** | `Level_03` | 10 | Falling Platform |
| 4 | `level_04` | **Aurora Cliffs** | `Level_04` | 11 | Spring Platform + kết hợp cũ |
| 5 | `level_05` | **Moon Gate** | `Level_05` | 12 | Tổng hợp toàn bộ mechanic |

Fragment cộng dồn theo region: **10 → 20 → 30 → 41 → 53**.

### Tiến trình Chapter & Constellation

| Mốc | Chòm sao | Fragment (cộng dồn) | Thời lượng | Effect Scale | Rơi vào khoảng |
|---|---|---|---|---|---|
| 1 | **Lyra** (5 sao, 6 nét) | 12 / 53 | 3.5s | 1.0 | Đầu Cloud Garden |
| 2 | **Cassiopeia** (5 sao, 4 nét) | 30 / 53 | 5.0s | 1.35 | Cuối Sky Ruins |
| 3 | **Orion** (7 sao, 6 nét) | 53 / 53 | 7.0s | 1.8 | Cuối Moon Gate |

> Các mốc trên giả định người chơi nhặt **đủ** sao. Bỏ sót thì mốc dời về sau.

### Bản sắc hình ảnh từng Region *(S1-013)*

| Region | Bầu trời (chân → đỉnh) | Lớp nền (alpha) | Cloud Density |
|---|---|---|---|
| **Forgotten Forest** | vàng bình minh → xanh lá nhạt → xanh trời | xanh rừng đậm (0.55) | 0.15 |
| **Cloud Garden** | trắng → trắng xanh → xanh nhạt | trắng xanh (0.45) | 0.85 |
| **Sky Ruins** | xám lam → lam đậm → xanh đêm | tím đá (0.60) | 0.25 |
| **Aurora Cliffs** | tím → tím sáng → tím đen | tím đậm (0.55) | 0.35 |
| **Moon Gate** | xanh đêm → gần đen → đen | đen xanh (0.65) | 0.10 |

> Cloud Density đã điền sẵn nhưng **chưa có tác dụng nhìn thấy** — xem mục 9.1.

---

## 8. DESIGN DECISIONS

**Phần quan trọng nhất. Không được tự ý đảo ngược bất kỳ mục nào dưới đây.**

### 8.1 Vision & thể loại
1. **Không Combat.**
2. **Không Enemy.**
3. **Không Boss.**
4. **Không Skill Tree.**
5. **Không Shop.**
6. Không tự thêm mechanic ngoài roadmap. Đề xuất phải nằm ở mục "Suggestions", không tự triển khai.
7. **Trải nghiệm quan trọng hơn số lượng mechanic.**
8. Gameplay chỉ xoay quanh: Platforming, Khám phá, Leo cao, Khôi phục bầu trời.

### 8.2 Cấu trúc hành trình
9. **Region thay cho Level** — mỗi scene là một khu vực của cùng một hành trình.
10. **Người chơi luôn leo liên tục**, không bị cắt ngang.
11. **Goal không phải mục tiêu cuối** — chỉ là "đã leo tới khu vực kế".
12. **Transition tự động**, mang tính điện ảnh.
13. **Không có nút Next Level.** Không Retry trong luồng chính.
14. **Không có màn hình Level Complete.** `LevelCompleteUI` đã nghỉ hưu vì trái vision.
15. Tên khu vực hiện tự động khi vào region mới, không cần người chơi bấm.

### 8.3 Star Fragment & Constellation
16. **Star Fragment không phải điểm số** — là mảnh ánh sáng khôi phục bầu trời.
17. **Không bắt buộc thu hết sao để qua region.** Goal luôn cho qua.
18. **Constellation là meta progression** — tiến trình dài hạn xuyên suốt chapter.
19. **Khôi phục bầu trời là mục tiêu dài hạn** của cả game.
20. **Fragment cộng dồn toàn chapter, không reset khi qua level.**
21. **Fragment đếm lúc nhặt**, không phải lúc chạm Goal — để khoảnh khắc khôi phục rơi đúng giữa hành trình.
22. **Sự kiện khôi phục không chuyển scene, không mở menu, không popup lớn, không bảng điểm.** Người chơi cảm nhận bằng hình ảnh.
23. Mốc sau phải hoành tráng hơn mốc trước (thời lượng + quy mô).
24. Số sao thu được **chỉ** ảnh hưởng rating/tiến trình chòm sao, **không** ảnh hưởng việc qua region.
25. UI tiến trình phải **nhỏ gọn**, không che gameplay.
26. Chơi lại từ region đầu chapter thì được xem lại toàn bộ khoảnh khắc khôi phục (`Restart Chapter On First Level`).
27. **Tên chòm sao hiện ĐỒNG THỜI với nét vẽ**, không phải sau khi vẽ xong — người chơi phải cảm thấy đang *chứng kiến* chòm sao được khôi phục, chứ không phải xem xong mới được cho biết tên.
28. Tên và chòm sao phải **tan cùng một khoảnh khắc**. Thời lượng tan là con số dùng chung do `ConstellationManager` sở hữu.
29. Tên + description lấy từ `ConstellationData`, **không hardcode**. Mỗi Chapter cấu hình danh sách chòm sao riêng.

### 8.4 Biome & trình bày khu vực *(S1-013)*
30. **Mỗi Region phải có bản sắc hình ảnh riêng** — không chỉ khác gameplay.
31. **Chuyển Region: bầu trời đổi màu mượt**, không được nhảy màu đột ngột.
32. Diện mạo Region nằm trong **`RegionData`**, không rải rác trong scene. Thêm Region mới không phải sửa code.
33. Biome **không được ảnh hưởng ngược lên gameplay** — không class nào trong `StarSower.Biome` tham chiếu Player / Platform / Goal / Constellation / Progress.
34. Trạng thái phiên chơi của biome (`BiomeSession`) **không ghi vào save**.

### 8.5 Gameplay
35. **Không chết khi rơi** — `GameOverManager` bị tắt có chủ đích.
36. Không dùng cơ chế kéo-thả kiểu ná (đã bác từ session đầu).
37. Nhảy phải tha thứ lỗi bấm: coyote time + jump buffer.
38. Khoá di chuyển **không được tắt animation** của Player.

### 8.6 Kiến trúc
39. **Không Singleton.** Phụ thuộc gán qua Inspector.
40. **Không hardcode** số level, số sao, số chapter, số chòm sao, tên level đầu chapter, tên Region.
41. **Single-Writer**: Rigidbody2D ← chỉ `PlayerMotor`; camera position ← chỉ `CameraFollow2D`; file save ← chỉ `ProgressManager`; nền ← chỉ `BackgroundManager`; trời ← chỉ `SkyManager`.
42. **Goal chỉ phát event** — không load scene, không lưu, không điều khiển UI.
43. Một class một trách nhiệm, không God Class.
44. Mọi tính năng mới phải mở rộng được qua kế thừa hoặc composition.
45. Giá trị cấu hình luôn `[SerializeField]`, không `public` field.
46. Dùng interface khi có nhiều cách hiện thực (`ITransitionEffect`, `IConstellationRestoreSequence`).
47. Dữ liệu tĩnh của designer nằm trong ScriptableObject; tiến trình người chơi nằm trong SaveData — **không trộn lẫn**, để nhặt sao không ghi đè asset và reset save không mất cấu hình.
48. Không refactor / rename chủ động khi không được yêu cầu.
49. **Phép ghi save "đi lên" và "đi xuống" phải là hai hàm riêng.** Gộp lại là tái tạo bug S1-013.1.
50. Class nào **giữ nhịp** thì class đó sở hữu các con số thời gian dùng chung, không để mỗi bên tự tính rồi lệch nhau.

---

## 9. KNOWN ISSUES

### 9.1 Placeholder (đã biết, chấp nhận ở giai đoạn này)

| Hạng mục | Tình trạng |
|---|---|
| **Art chòm sao** | Ô vuông trắng làm sao, thanh mỏng làm nét nối. Chưa có sprite |
| **Camera nhìn lên bầu trời** | **TODO — chưa làm.** Spec cấm sửa Camera và project chưa có hệ thống này. Hiện thay bằng lớp phủ tối toàn màn hình |
| **Particle khôi phục** | Ô Inspector đã có, chưa có asset |
| **Âm thanh khôi phục** | Ô Inspector đã có, chưa có clip. `AudioSource` tự tạo lúc chạy |
| **Icon chòm sao** | Ô đã có trong `ConstellationData`, **chưa component nào đọc** (`ConstellationUI` hiện ★/☆, name card chỉ hiện chữ) |
| **Description chòm sao** | Đã có dữ liệu và đã hiện lên, nhưng nội dung là placeholder tạm (The Harp / The Seated Queen / The Hunter) |
| **`ConstellationReward`** | Đã bị gỡ khỏi `ConstellationData` ở bản S1-012 cuối. Chưa có hệ thống phần thưởng nào |
| **Background 5 region** | Có sky gradient thật + lớp nền bán trong, nhưng lớp nền vẫn là **ô màu phẳng**, chưa có art |
| **`parallaxFactor`** | **TODO — chưa component nào đọc.** Trường đã có trong `BackgroundLayerData`, cắm sẵn cho sprint parallax |
| **Cloud Density** | **Chưa có tác dụng nhìn thấy.** `cloudLayerIndex` đang `-1` ở cả 5 scene vì chưa có art mây. Gán index là chạy, không sửa code |
| **Nhạc / ambient theo Region** | Ô đã có trong `RegionData`, `musicSource`/`ambientSource` chưa gán, chưa có clip → im lặng |
| **`nameFormat` không dùng được ✨** | Font builtin (Arial) không có glyph U+2728 → sẽ ra ô vuông rỗng. Mặc định để `{0}` trơn. Nhập font riêng thì trang trí được |
| **VFX tiếp đất / lò xo** | Placeholder |
| **Animation Player** | Chưa có — nhân vật không có animation clip |
| **Object pooling** | `SimplePlatformPool` là bản đơn giản, chưa tối ưu thật |

### 9.2 Feature chưa hoàn thiện

| Hạng mục | Ghi chú |
|---|---|
| **Chapter Complete** | Đạt 53/53 chỉ lưu `completed: true`. Chưa có màn kết chương, chưa có Chapter 2 để nối tiếp |
| **Moon Gate là ngõ cụt** | Hết region cuối thì `LevelFlowManager` chỉ mở lại màn hình, không có gì tiếp theo |
| **Level Select cô lập** | `LevelSelectController` còn code nhưng **không có lối vào nào** |
| **Chưa có Main Menu** | `ProgressManager.LastPlayedLevelId` đã sẵn sàng cho nút Continue nhưng chưa có màn hình gọi tới |
| **`LevelCompleteUI` nghỉ hưu** | GameObject tắt, code vẫn biên dịch. Trái vision — cần quyết định xoá hẳn hay giữ |

### 9.3 Rủi ro thiết kế cần theo dõi

- **Mốc 3 = 53/53 = 100% tổng fragment.** Bỏ sót **một** ngôi sao là Orion không bao giờ khôi phục và chapter không bao giờ hoàn thành. Đây là hệ quả trực tiếp của con số trong spec, không phải lỗi. Muốn có biên an toàn thì hạ mốc xuống ~48.
- **Chưa playtest cân bằng.** Ước tính 2–4 phút/region chưa được kiểm chứng. Khoảng cách nhảy và lực lò xo ở region 2–5 chưa hand-tune.
- **Transition dùng `SceneManager.LoadScene` đồng bộ** — khựng lúc load đang bị *che* chứ chưa được khử.
- **Cảnh báo `CS0618`** còn tồn tại (API cũ), chưa dọn.
- **S1-012 ĐÃ được xác nhận chạy đúng.** Bằng chứng: save file đạt `53/53`, `completed: true`, cả 3 chòm sao `restored: true`. Bản vá S1-013.1 cũng đã xác nhận qua save (`16/53`, `completed: false`, chỉ Lyra `true`).
- **S1-013 và S1-013.2 CHƯA được playtest.** Scene đã qua audit toàn vẹn tĩnh (trùng fileID, tham chiếu treo, đối ứng cha-con, kiểu component, GUID font, `CanvasRenderer` anh em) nhưng **chưa ai bấm Play** để xác nhận bầu trời, thẻ tên và nhịp trình diễn.
- **Không có "tiến trình vĩnh viễn" cho Constellation.** Game chưa phân biệt "đã từng khôi phục" (nên giữ mãi) với "đã khôi phục trong lượt này" (reset được). Vào Level 1 là 53 fragment biến mất khỏi save. Đúng thiết kế đã chốt, nhưng mâu thuẫn với tầm nhìn "gieo lại các vì sao" — cần giải quyết nếu làm sky gallery.

### 9.4 Lịch sử bug & bài học

| Bug | Nguyên nhân | Bài học |
|---|---|---|
| Text không bao giờ hiện (popup, HUD, tên region) | Toàn bộ `m_Font` trỏ GUID `...f000...` (unity default resources) trong khi font Arial builtin nằm ở `...e000...` (unity_builtin_extra) | **Font builtin dùng GUID `0000000000000000e000000000000000`, fileID `10102`.** Đã sửa 70+ tham chiếu |
| Text không render dù đúng font | Thiếu `CanvasRenderer` (`!u!222`) cùng GameObject | Legacy uGUI `Text` **bắt buộc** có `CanvasRenderer` |
| "Restored Transform child parent pointer from NULL" | `m_Father` trỏ fileID của **GameObject** thay vì của **RectTransform** | **Phải phân biệt rõ fileID của GameObject và fileID của Component khi viết YAML tay** |
| "Serialized reference type mismatch" | Field kiểu `Text` / `LevelSelectEntryView` trỏ vào GameObject gốc | Như trên |
| Safe Mode `CS7036` | Đổi chữ ký `ProgressManager.CompleteLevel()` mà quên cập nhật `LevelCompleteUI` (đã tắt nhưng vẫn biên dịch) | Đổi API phải grep **toàn bộ** call site, kể cả code đã nghỉ hưu |
| Chòm sao chỉ hiện đúng 1 lần rồi thôi | Cờ hoàn thành lưu vĩnh viễn trong save | Đã giải bằng ô **Restart Chapter On First Level** |
| Chẩn đoán sai "do tỉ lệ khung hình" | Kết luận vội khi chưa có bằng chứng | **Phải lấy log/bằng chứng trước khi kết luận nguyên nhân**, không suy đoán |
| Tiến trình Constellation "reset / không hiện" sau S1-013 *(S1-013.1)* | `restartChapterOnFirstLevel` dùng `WriteChapterProgress()` — hàm **chỉ biết đi lên** — nên xoá fragment nhưng không xoá được cờ `restored` | **Một hàm ghi save không thể phục vụ hai ý định trái ngược.** Tách `WriteChapterProgress` (lên) và `ResetChapterProgress` (xuống) |
| Bug trên **vô hình ở lần chơi đầu** | Checklist test chỉ chạy một lượt | **Mọi thay đổi chạm tiến trình phải test HAI LƯỢT liên tiếp** và đối chiếu trực tiếp `starsower_save.json` |
| Quy sai lỗi cho sprint mới nhất | Sprint mới trùng thời điểm phát hiện, không phải trùng nguyên nhân | **Dùng `git diff` với commit trước để xác định phạm vi thật** trước khi nhận sprint nào có lỗi |

> **Lưu ý dùng `git diff`:** S1-012 đã commit (`1add22d`), nên mọi thay đổi từ S1-013 trở đi vẫn còn ở dạng chưa commit và diff được so với mốc đó.

---

## 10. ROADMAP

### 10.1 Sprint đã xong
`S1-001` → `S1-002` → `S1-003` → `S1-004` → `S1-005` → `S1-006` → `S1-007` → `S1-008` → `S1-008.1` → `S1-009` → `S1-010` → `S1-011` → `S1-012` → **`S1-013`** → **`S1-013.1`** → **`S1-013.2`**

> **Cảnh báo trùng tên:** bản tài liệu trước đặt "S1-013" cho một sprint *Sky Look & Constellation Art* chưa từng được duyệt. Sprint S1-013 thật sự đã làm là **Biome Presentation System**. Nội dung Sky Look được chuyển xuống mục 10.2 dưới tên mới để tránh nhầm lẫn lần nữa.

### 10.2 Sprint tiếp theo (đề xuất, **chưa được duyệt**)

**S1-014 – Constellation Persistence & Sky Gallery** *(khuyến nghị)*
- Tách "đã từng khôi phục" (vĩnh viễn) khỏi "đã khôi phục trong lượt này" (reset được) trong `SaveData`.
- Màn hình xem lại các chòm sao đã khôi phục — dùng trường `Icon` hiện chưa ai đọc.
- Lý do ưu tiên: sửa đúng mâu thuẫn với tầm nhìn cốt lõi, không chỉ làm đẹp thêm.

**S1-014b – Parallax & Background Layers**
- Thêm 2–3 lớp nền mỗi Region, cho trôi theo camera với tốc độ khác nhau.
- Khai thác `parallaxFactor` đã cắm sẵn ở S1-013; bật lớp mây để `cloudDensity` có tác dụng thật.
- Không cần asset thật, vẫn placeholder được.

**S1-014c – Sky Look & Constellation Art** *(nội dung "S1-013" cũ trong tài liệu trước)*
- Camera ngẩng lên bầu trời khi khôi phục, thay lớp phủ tối. Nay khả thi hơn vì đã có Sky Plane thật từ S1-013.
- Thay hình placeholder bằng sprite sao thật + particle.
- Dùng Icon chòm sao trong `ConstellationUI` và trên name card.

**S1-014d – Region Audio**
- Nhập nhạc + ambient thật, cross-fade khi chuyển Region. Khung đã có sẵn, chỉ thiếu asset.

### 10.3 Backlog
- **Chapter Complete & Chapter 2** — xử lý ngõ cụt Moon Gate, nối tiếp sang chương mới.
- **Main Menu** — Continue dùng `LastPlayedLevelId`, khôi phục lối vào Level Select.
- **Hệ thống phần thưởng** cho chòm sao đã khôi phục.
- Playtest & cân bằng độ khó 5 region.
- Animation cho Player.

### 10.4 Ý tưởng chưa triển khai *(Suggestions — không tự làm khi chưa được duyệt)*
- `LoadSceneAsync` / additive scene để khử hẳn khựng load.
- Đóng gói platform mechanic thành prefab.
- Import asset particle / audio thật.
- Object pooling thật sự.
- Chuyển sang TextMeshPro *(sẽ giải quyết luôn chuyện thiếu glyph ✨)*.
- Dọn cảnh báo `CS0618`.
- Dọn YAML thừa: `LevelFlowManager` trong 5 scene còn 2 trường `constellationManager` / `restoreSequenceSource` sót từ bản S1-012 đầu. Unity bỏ qua, vô hại.

---

## 11. CODING GUIDELINES

### 11.1 Quy tắc bắt buộc
- Không phá code đang chạy.
- **Không refactor chủ động** — chỉ khi được yêu cầu rõ ràng.
- **Không rename** class / namespace / prefab trừ khi bắt buộc cho thay đổi được yêu cầu.
- Ưu tiên khả năng mở rộng hơn viết nhanh.
- **Một class một trách nhiệm** — không God Class.
- Mọi tính năng mới mở rộng được qua kế thừa hoặc composition.
- **Không hardcode** — luôn `[SerializeField]`, **không** dùng `public` field.
- Chỉ đụng đúng phần được hỏi, không sửa lan sang chỗ khác.
- Khi có nhiều cách làm, chọn cách **dễ bảo trì nhất cho game mobile**, không phải cách viết nhanh nhất.
- **Trước khi nhận một sprint có regression, phải `git diff` để xác định phạm vi thật.** Trùng thời điểm phát hiện ≠ trùng nguyên nhân.
- **Mọi thay đổi chạm tiến trình phải test hai lượt chơi liên tiếp** và đối chiếu trực tiếp `starsower_save.json`. Test một lượt không bắt được lỗi loại S1-013.1.

### 11.2 Quy ước code
- Namespace theo thư mục: `StarSower.<Module>`.
- Comment tiếng Việt giải thích **tại sao**, không phải **cái gì**.
- Coroutine (`IEnumerator` + `yield return`) cho mọi trình tự có thời gian.
- `[Header]` / `[Tooltip]` cho field designer cần chỉnh.
- Event dùng `System.Action`, đặt tên `On...`.
- ScriptableObject cho dữ liệu designer; class `[Serializable]` thuần cho dữ liệu con.

### 11.3 Định dạng trả lời cho mỗi Story *(bắt buộc, từ S1-002 trở đi)*
Mỗi story phải trả lời **trong một lượt** với đủ 5 phần theo thứ tự:
1. **Thiết kế (Design)** — sẽ tạo class nào, trách nhiệm từng class.
2. **Đánh giá rủi ro (Risk Analysis)** — rủi ro về khả năng mở rộng của kiến trúc đã chọn.
3. **Triển khai (Implementation)** — code thật, ghi vào file.
4. **Kiểm thử (Testing Checklist)** — các bước test thủ công trong Unity (project chưa có test tự động).
5. **Đề xuất Story tiếp theo.**

Không dừng lại chờ duyệt giữa chừng. Chỉ hỏi lại khi yêu cầu mơ hồ tới mức có nguy cơ chọn sai kiến trúc.

### 11.4 Lưu ý khi sửa scene bằng YAML (không có Editor)
- Quản lý `fileID` duy nhất; script GUID theo lược đồ `aaaa…NN`, asset/prefab `bbbb…NN`.
- **Luôn phân biệt fileID của GameObject và của Component.**
- `m_Children` / `m_Father` phải đối ứng, và `m_Father` phải trỏ Transform/RectTransform.
- Text legacy cần `CanvasRenderer`; font builtin dùng GUID `0000000000000000e000000000000000`, fileID `10102`.
- Đăng ký root mới vào `SceneRoots.m_Roots`.
- Sau mỗi lần sửa: audit trùng fileID, tham chiếu treo, đối ứng cha-con, khớp kiểu component.
- Nếu Unity đang mở scene: đóng **không lưu** rồi mở lại, tránh dữ liệu cũ trong bộ nhớ ghi đè.

---

## 12. PROJECT STATUS

### Đánh giá: **VERTICAL SLICE**

**Đã đạt được (vượt mức Prototype):**
- Vòng lặp gameplay cốt lõi hoàn chỉnh, chạy được từ đầu tới cuối: leo → nhặt sao → khôi phục chòm sao → Goal → chuyển region → leo tiếp.
- 5 region liền mạch với đường cong dạy mechanic rõ ràng.
- Kiến trúc đã ổn định qua 13 sprint, ranh giới trách nhiệm rõ, có điểm mở rộng bằng interface.
- Hệ save đầy đủ, hoạt động xuyên phiên chơi, **đã được xác nhận bằng save file thật**.
- Meta progression (Constellation) đã chạy end-to-end.
- Vòng lặp dài nhất của game — **khôi phục bầu trời** — đã hiện diện thật, không còn là ý tưởng.
- **Mỗi Region đã có bản sắc hình ảnh riêng** *(S1-013)* — người chơi cảm nhận được mình đang bước sang một tầng trời khác.

**Chưa đạt (chưa lên được Alpha):**
- **Gần như toàn bộ art, audio, VFX vẫn là placeholder.** Sky gradient là thứ đầu tiên trông "có chủ ý", nhưng vẫn không có một asset mỹ thuật thật nào.
- **Chưa playtest cân bằng độ khó** — nhịp chơi, khoảng cách nhảy, thời lượng mỗi region đều là ước tính.
- **Chỉ có 1 chapter**, và chapter đó kết thúc ở ngõ cụt.
- **Không có Main Menu**, không có lối vào Level Select, không có màn kết chương.
- Player chưa có animation.

Nói cách khác: **cấu trúc của game đã đủ để cảm nhận trọn vẹn ý đồ thiết kế, nhưng lớp trình bày và độ đánh bóng thì chưa bắt đầu.** Đó đúng là định nghĩa của Vertical Slice.

---

## 13. TÓM TẮT NHANH CHO PHIÊN LÀM VIỆC MỚI

Nếu chỉ đọc được một phần, hãy đọc phần này:

1. Starsower là **một hành trình leo lên bầu trời liên tục**, không phải game qua màn. Đọc mục 2 và mục 8 trước khi đề xuất bất cứ thay đổi nào.
2. **Không Combat / Enemy / Boss.** Không tự thêm mechanic ngoài roadmap.
3. Đang ở cuối **S1-013.2**. Content: 1 chapter, 5 region, 53 Star Fragment, 3 chòm sao (mốc 12 / 30 / 53), 5 `RegionData`.
4. Hệ thống được coi là **ổn định, không sửa khi không được yêu cầu**: Player, Camera, Platform, Collectible, Transition, Goal, **Biome**.
5. `ProgressManager` là nơi **duy nhất** ghi file save. **Hai hàm ghi — lên và xuống — không được gộp lại** (bug S1-013.1).
6. Trả lời mỗi story theo **5 phần** ở mục 11.3.
7. Việc lớn còn treo: art thật, Chapter Complete, Main Menu, playtest cân bằng, tiến trình Constellation vĩnh viễn.
8. **Đã xác nhận qua save file:** S1-012 chạy đúng end-to-end; bản vá S1-013.1 chạy đúng.
9. **CHƯA playtest:** S1-013 (bầu trời, biome) và S1-013.2 (thẻ tên chạy song song). Mới chỉ qua audit tĩnh.
10. Khi nghi có regression: **`git diff` với commit `1add22d` (S1-012) trước**, đừng quy lỗi cho sprint mới nhất chỉ vì nó trùng thời điểm.
11. Test tiến trình phải chạy **hai lượt chơi liên tiếp** — bug S1-013.1 vô hình ở lượt đầu.
