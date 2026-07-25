# PROJECT_CONTEXT.md — Starsower

> Tài liệu context chính thức của dự án. Cập nhật đến hết **S1-014 – Atmosphere & Audio Foundation**
> (kế thừa S1-013 Biome Presentation, bản vá regression S1-013.1, cải tiến trình diễn S1-013.2).
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
  - Chỉ `AudioManager` được ghi các `AudioSource` nhạc/ambient *(S1-014)*.
  - Chỉ `ParticleController` được ghi các hiệu ứng hạt của Region *(S1-014)*.
- **Event hub:** `GameEvents` (static) để các hệ thống không tham chiếu trực tiếp lẫn nhau.
- **Interface để thay thế:** `ITransitionEffect`, `IConstellationRestoreSequence`, `IInputProvider`, `IGroundDetector`, `ILaunchable`, `ICameraTarget/Shake/Zoom`, `IPlatformPool`.
- **Không Singleton.** Mọi phụ thuộc gán qua `[SerializeField]` trong Inspector.

### 4.2 Danh sách script (79 file)

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

#### Biome — `StarSower.Biome` *(S1-013, mở rộng ở S1-014)*
| File | Vai trò |
|---|---|
| `RegionData.cs` (SO) | Diện mạo + không khí 1 khu vực: tên, sky gradient, màu nền camera, danh sách lớp background, cloud density, music/ambient clip + volume *(S1-014)*, danh sách particle prefab *(S1-014)* |
| `BiomeManager.cs` | Điều phối **HÌNH ẢNH**: áp nền + trời trong `Awake()`, ghi nhớ region cho lần chuyển kế. Từ S1-014 **không còn phát audio** — đã tách sang `RegionAtmosphereManager` |
| `BackgroundManager.cs` | **Nơi duy nhất** ghi các `SpriteRenderer` nền. Hỗ trợ n lớp |
| `SkyManager.cs` | **Nơi duy nhất** ghi Sky Plane + `Camera.backgroundColor`. Nướng `Gradient` thành `Texture2D` lúc chạy |
| `BiomeSession.cs` | Static, nhớ region vừa rời để blend màu trời qua ranh giới scene. **Không ghi vào save** |
| `RegionAtmosphereManager.cs` *(S1-014)* | Điều phối **KHÔNG KHÍ**: đọc Region từ `BiomeManager.Region` (không tự có field riêng), gọi `AudioManager` phát nhạc/ambient và `ParticleController` đổi hạt trong `Awake()`. Có `FadeOutForDeparture()` gọi từ `LevelFlowManager` |
| `ParticleController.cs` *(S1-014)* | **Nơi duy nhất** ghi các hiệu ứng hạt của Region. `Switch(prefabs)` / `Clear()`, không object pool |

#### Audio — `StarSower.Audio` *(S1-014)*
| File | Vai trò |
|---|---|
| `AudioManager.cs` | Trình phát crossfade 2 kênh (nhạc + ambient) dùng chung, không biết Region/Save gì cả. Mỗi kênh giữ 2 `AudioSource` (A/B) để fade-in chồng lên fade-out. **Không Singleton, không DontDestroyOnLoad** |

#### Level & Flow — `StarSower.Level`
| File | Vai trò | Trạng thái |
|---|---|---|
| `GoalController.cs` | **Chỉ** phát `GameEvents.RaiseLevelCompleted()`. Không load scene, không lưu, không điều khiển UI | Đang dùng |
| `LevelFlowManager.cs` | Điều phối toàn bộ trình tự đến/đi của region. Lấy tên khu vực từ `BiomeManager` nếu có gán, nếu không thì dùng `regionDisplayName` cũ. Từ S1-014: gọi `RegionAtmosphereManager.FadeOutForDeparture()` lúc màn hình bắt đầu che | Đang dùng |
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
| `AtmosphereSystem` *(S1-014)* | `872000001` | `AudioManager` · `ParticleController` · `RegionAtmosphereManager` |

`BackgroundPlane` (`867000001`) có sẵn từ trước giờ được `BackgroundManager` quản làm **lớp nền 0**.

`AudioManager` **không có `AudioSource` nào wire sẵn trong scene** — 4 field (`musicSourceA/B`, `ambientSourceA/B`) để trống, tự tạo GameObject con lúc `Awake()` giống cách `ConstellationManager` từng tự tạo `AudioSource` ở S1-012. Giảm rủi ro sửa YAML, không cần thêm component nào vào scene cho việc này.

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

### S1-014 — Atmosphere & Audio Foundation
- **Objective:** Nền tảng kỹ thuật cho không khí từng Region — nhạc, ambient, hiệu ứng hạt — data-driven và dễ mở rộng. **Thuần trình bày, không gameplay.**
- **Result:**
  - `AudioManager` (`StarSower.Audio`, mới) — trình phát crossfade 2 kênh (nhạc + ambient) dùng chung, mỗi kênh 2 `AudioSource` (A/B) để fade-in chồng lên fade-out. Chống phát trùng (gọi lại đúng clip đang phát thì bỏ qua).
  - `RegionAtmosphereManager` (`StarSower.Biome`, mới) — điều phối không khí: đọc Region từ `BiomeManager.Region` (không tự có field riêng, tránh 2 nơi khai báo lệch nhau), gọi `AudioManager`/`ParticleController` trong `Awake()`.
  - `ParticleController` (`StarSower.Biome`, mới) — nơi duy nhất ghi hiệu ứng hạt của Region. `Switch()`/`Clear()`, không object pool.
  - `RegionData` mở rộng: `musicVolume`, `ambientVolume`, `particlePrefabs` (list).
  - `BiomeManager` **bỏ** `PlayAudio()`/`musicSource`/`ambientSource` — audio đã tách hẳn sang `RegionAtmosphereManager`, giữ đúng một class một trách nhiệm.
  - `LevelFlowManager` **+1 field tuỳ chọn** `regionAtmosphereManager`, gọi `FadeOutForDeparture()` ngay khi màn hình bắt đầu che (trước `PlayIn()`).
- **Quyết định đáng nhớ — giới hạn kiến trúc thành thật:** True crossfade **xuyên ranh giới scene** là bất khả thi nếu giữ đúng luật "Không Singleton / Không DontDestroyOnLoad" của dự án, vì mỗi Region là 1 scene và `SceneManager.LoadScene` phá huỷ mọi GameObject ngay lập tức. Giải pháp đã chọn: scene cũ tự fade nhạc về 0 **trước khi bị huỷ** (gọi từ `LevelFlowManager` lúc màn hình che kín, có đủ thời gian vì transition + hold ≈ 1.4s > `fadeOutDuration` 1s mặc định), scene mới fade in từ im lặng trong `Awake()`. Đạt đúng *cảm giác* "fade out rồi fade in", không cần phá luật No Singleton.
- **`AudioManager` không wire `AudioSource` sẵn trong scene** — tự tạo lúc chạy, giống `ConstellationManager` ở S1-012. Giảm rủi ro sửa YAML.
- **Không nên đổi:** Không đưa `AudioManager`/`ParticleController` thành Singleton/DontDestroyOnLoad để "sửa" giới hạn crossfade — đó là đánh đổi có chủ đích, không phải thiếu sót. Không để `RegionAtmosphereManager` tự có field `RegionData` riêng — luôn đọc qua `BiomeManager.Region`. Không đưa logic atmosphere vào `PlayerController`.
- **Chưa playtest** — chỉ qua audit tĩnh (xem mục 9.3).

### S1-014B — Audio Content Integration
- **Objective:** Nối nội dung thật vào hệ thống atmosphere đã có từ S1-014. **Không viết lại kiến trúc.**
- **Result:**
  - `AudioManager` **+3 field mixing**: `masterVolume`, `musicVolume`, `ambientVolume` (kênh, độc lập với Volume riêng từng Region). `PlayMusic`/`PlayAmbient` nhân 3 tầng: Region Volume × Channel Volume × Master Volume. Có `SetMasterVolume/SetMusicVolume/SetAmbientVolume` cắm sẵn cho màn hình Settings sau này.
  - `RegionAtmosphereManager` **+`WarnIfMissing()`**: thiếu Music/Ambient Clip thì in `Debug.LogWarning` nêu rõ tên Region + tên field, **không throw, không chặn gameplay**.
  - `RegionData`: chỉ đổi `[Header]`/`[Tooltip]` cho rõ ràng hơn ("chỉ cần kéo Clip vào, Volume mặc định 1 là đủ") — **không đổi tên field nào**, không phá serialization.
  - **Không sửa scene YAML** — 3 field mixing mới dùng default C# (`1f`) khi vắng mặt trong `.unity`, đúng chuẩn tương thích ngược đã áp dụng từ S1-013.
- **Quyết định đáng nhớ:** Mixing **không phản ứng sống (live)** — `SetMasterVolume()` chỉ có tác dụng ở lần `Play` kế tiếp, không cập nhật âm lượng đang phát ngay lập tức. Lý do: dự án chưa có màn hình Settings nào tiêu thụ giá trị này; xây khả năng phản ứng sống bây giờ là tối ưu sớm cho một tính năng chưa tồn tại.
- **Giới hạn không giải quyết được (đã biết trước, đúng scope):** **Không có audio asset thật nào trong project** (`find Assets -iname "*.mp3/*.wav/*.ogg"` ra 0 kết quả) — không thể tự tạo ra âm nhạc/âm thanh. Cả 10 clip (5 Region × Music + Ambient) vẫn trống. Hệ thống đã sẵn sàng chạy đúng ngay khi designer kéo file vào — đây là ranh giới giữa "kiến trúc" (đã xong) và "nội dung" (chưa có gì để gắn).
- **Không nên đổi:** Không tự sinh AudioClip giả (silence) để lấp chỗ trống — sẽ tạo cảm giác sai là đã có nội dung trong khi chưa có gì. Không biến mixing thành live-reactive khi chưa có UI nào cần nó.

### S1-014C — Integrate First Region BGM
> **Cảnh báo trùng tên:** yêu cầu sprint này lại đặt tên "S1-014B" lần thứ hai — trùng với sprint ngay phía trên (mixing + warning). Đổi số thành **S1-014C** để giữ dòng lịch sử không đè lên nhau. Nếu tương lai có yêu cầu ghi "S1-014B" lần nữa, hãy hỏi lại người dùng ý đang chỉ tới sprint nào.
- **Objective:** Nạp file nhạc thật đầu tiên (`Assets/Audio/Music/BGM_ForgottenForest.mp3`) vào hệ thống atmosphere đã có. **Không viết lại kiến trúc.**
- **Result:** Gán `defaultMusic` của asset `Region_ForgottenForest.asset` vào clip đã import (`guid: 6c835f07405074d4aab9aabace15a91f`). Không sửa script, không sửa scene — toàn bộ pipeline (auto-play trong `RegionAtmosphereManager.Awake()`, loop, fade in/out, dedupe, mixing) đã tồn tại nguyên vẹn từ S1-014/S1-014B.
- **Đã xác nhận qua đọc trực tiếp scene YAML** (không phải đoán): `SampleScene` (level_01, level đầu Chapter 1) → `BiomeManager.region` trỏ đúng `Region_ForgottenForest` → `RegionAtmosphereManager` trỏ đúng `BiomeManager` đó → `LevelFlowManager.regionAtmosphereManager` đã nối sẵn từ S1-014, gọi `FadeOutForDeparture()` lúc rời Region.
- **1/10 clip đã gắn** (Forgotten Forest Music). 9 ô còn lại (4 Music khác + 5 Ambient) vẫn trống, có cảnh báo Console nhắc từ `WarnIfMissing()`.
- **Không nên đổi:** Không hardcode đường dẫn asset trong script — nguồn sự thật duy nhất của "Region nào phát nhạc gì" vẫn là field `defaultMusic` trên `RegionData`.

### S1-014C-001 — Forgotten Forest Ambient Audio Manager
- **Objective:** Biến Forgotten Forest từ "chỉ có BGM" thành không khí rừng sống động — gió lặp liên tục + chim/lá phát ngẫu nhiên, không lặp mẫu cố định. **Chưa làm particle.**
- **Result:**
  - `AmbientLayerData` (`StarSower.Audio`, mới) — 1 lớp âm thanh: `Loop` (lặp liên tục) hoặc `RandomOneShot` (phát rời rạc theo chu kỳ ngẫu nhiên). Plain `[Serializable]`, sống trong `AmbientProfile` giống `BackgroundLayerData` sống trong `RegionData`.
  - `AmbientProfile` (`StarSower.Audio`, mới, SO) — danh sách layer của MỘT khu vực, tách khỏi `RegionData.Ambient` (1 clip lặp đơn, S1-014) vì đây là NHIỀU lớp chồng nhau. Region nào chưa cần lớp phức tạp vẫn dùng Ambient đơn giản, không bị ép nâng cấp.
  - `LayeredAmbientPlayer` (`StarSower.Audio`, mới) — phát 1 `AmbientProfile`: mỗi layer Loop có 1 `AudioSource` lặp riêng (tạo động lúc `Play()`), mọi layer RandomOneShot dùng CHUNG 1 `AudioSource.PlayOneShot` (overlap được, không cần 1 nguồn/layer). Route ngẫu nhiên delay + chọn clip mỗi vòng lặp → không bao giờ ra nhịp cố định.
  - `RegionData` **+1 field tuỳ chọn** `ambientProfile` (kiểu `AmbientProfile`).
  - `RegionAtmosphereManager` **+1 field tuỳ chọn** `layeredAmbientPlayer` — gọi `Play(region.AmbientProfile)` trong `Awake()`, `Stop()` trong `FadeOutForDeparture()`. Tái dùng ĐÚNG lifecycle đã có từ S1-014, không tạo entry/exit hook mới.
  - Asset `Ambient_Forest.asset` — ban đầu 4 layer gồm cả Wind; đã bỏ Wind ở **S1-014C-002** ngay bên dưới. Xem bảng hiện hành ở đó.
  - Import + tạo `.meta` cho 5 file audio + 5 thư mục dưới `Assets/Audio/Ambient/Forest/` (chưa có `.meta` nào khi nhận task — dấu hiệu file được thả vào ổ đĩa chứ chưa qua Editor).
- **Quyết định đáng nhớ:**
  - **"Chim buổi sáng hiếm hơn" không dùng trọng số ngẫu nhiên.** Cân nhắc giữa (a) gộp `morning_bird` vào chung pool với `bird_1`/`bird_2` kèm trọng số thấp, và (b) tách hẳn thành layer riêng với delay dài hơn — chọn (b) vì spec liệt kê "Morning bird" thành mục riêng biệt, và (b) không cần thêm khái niệm "trọng số" vào `AmbientLayerData`, chỉ tái dùng đúng cơ chế delay đã có.
  - **Master Volume của `LayeredAmbientPlayer` KHÔNG nối vào `AudioManager.AmbientVolume`** dù cả hai đều là "âm lượng ambient chung". Cố tình tách để không tạo phụ thuộc chéo giữa 2 hệ thống độc lập (đúng yêu cầu "avoid unnecessary dependencies") — muốn 1 slider Settings điều khiển cả hai thì gọi cả 2 API từ màn hình Settings sau này, không phải việc của sprint này.
  - **Fade khi rời Region chỉ áp cho layer Loop** (gió) — layer RandomOneShot chỉ cần dừng coroutine, không có "âm thanh đang kêu liên tục" nào cần fade.
- **Giới hạn không giải quyết được:** Mục tiêu mix "BGM 70–80% / Ambient 20–30%" chỉ áp được qua các con số volume (Wind 0.9 × Master 0.28 ≈ 25%) — **cân bằng thật sự phụ thuộc độ loudness gốc của từng file mp3**, thứ tôi không kiểm chứng được vì không nghe được audio.
- **Không nên đổi:** Không gộp `LayeredAmbientPlayer` vào `AudioManager` — 2 khái niệm khác nhau (2 nguồn cố định crossfade được vs N nguồn động phát chồng lớp). Không thêm particle ở sprint này — đúng scope task yêu cầu.
- **Chưa playtest** — chỉ qua audit tĩnh + kiểm tra chéo guid.

### S1-014C-002 — Remove Wind, Rebalance Mix
- **Objective:** Bỏ hẳn layer Wind khỏi Forgotten Forest. Cảm giác mới: yên tĩnh, huyền ảo, sống động — chỉ còn Birds + Leaves. Đổi mục tiêu mix từ "70–80/20–30" sang **BGM 80% / Birds 15% / Leaves 5%**.
- **Result:**
  - `Ambient_Forest.asset` — xoá hẳn entry Wind. Còn lại 3 layer, tất cả `RandomOneShot`, **không còn layer Loop nào**:

    | Layer | Clip | Volume | Delay |
    |---|---|---|---|
    | Birds | `bird_1`, `bird_2` | 0.15 | 10–30s |
    | Morning Bird | `morning_bird` | 0.15 (bằng Birds — độ hiếm đến từ delay, không phải volume) | 90–180s |
    | Leaves | `leaves_1` | 0.05 | 20–45s |

  - `LayeredAmbientPlayer.masterVolume` mặc định đổi từ `0.28` → **`1`**. Lý do: trước đây phải nhân 2 tầng (layer volume × master) mới ra số phần trăm cuối cùng khó đoán; giờ **volume của mỗi layer chính là % nghe được cuối cùng** (0.15 = 15%) — số trong Inspector khớp thẳng với con số spec, không cần quy đổi.
  - `SampleScene.unity` — cập nhật `masterVolume: 0.28` → `1` trên component đã wire từ S1-014C-001.
- **Đã xác nhận bằng grep:** guid của `wind_soft.mp3` (`53631af8181f408f8f2a53fe178c2470`) **không còn xuất hiện** ở bất kỳ `.asset`/`.unity`/`.cs` nào trong project.
- **Quyết định đáng nhớ — KHÔNG xoá file `wind_soft.mp3` khỏi đĩa.** Yêu cầu là "xoá khỏi không khí Forgotten Forest", không phải "xoá khỏi project". File + `.meta` vẫn còn nguyên tại `Assets/Audio/Ambient/Forest/Wind/` — có thể cần lại cho Region khác sau này (Sky Ruins "Strong Wind", Aurora Cliffs "Magical Wind" từng nhắc ở S1-014B). Xoá file người dùng đã import mà không được yêu cầu rõ là hành động không cần thiết.
- **Không đổi kiến trúc động cơ:** `AmbientLayerType.Loop` vẫn còn trong enum, code xử lý Loop trong `LayeredAmbientPlayer` vẫn còn — đây là hạ tầng dùng chung (`StarSower.Audio`), không phải code riêng cho gió. Xác nhận bằng `grep -rni "wind" Assets/Scripts/`: chỉ còn 2 chỗ nhắc "Wind" trong COMMENT ví dụ minh hoạ (không phải logic), không có identifier/hardcode nào. Đúng yêu cầu "keep the ambient manager reusable" + "do not create a generic nature ambience system" — bộ máy vẫn generic, nội dung Forest vẫn riêng.
- **Không nên đổi:** Không thêm lại Wind vào `Ambient_Forest.asset` trừ khi có yêu cầu mới. Không xoá `AmbientLayerType.Loop`/logic Loop khỏi `LayeredAmbientPlayer` — vẫn cần cho region khác.

### S1-014C-003 — Forgotten Forest Particle Atmosphere
> **Cảnh báo trùng tên:** yêu cầu sprint này lại đặt tên "S1-014C-002" — trùng với sprint ngay phía trên (bỏ Wind). Đổi số thành **S1-014C-003**.
- **Objective:** Thêm hạt môi trường tinh tế cho Forgotten Forest: lá rơi + bụi nắng xuyên tán cây. **Thuần hình ảnh, không gameplay.**
- **Result:**
  - `AmbientParticleField` (`StarSower.Biome`, mới) — hạt 2D nhẹ, **tự pool hoàn toàn bằng code, KHÔNG dùng Unity ParticleSystem (Shuriken)**. 1 vòng lặp trung tâm dịch chuyển N `SpriteRenderer` đã tạo sẵn lúc `Awake()`; hết vòng đời hoặc rơi khỏi vùng thì respawn tại chỗ (đổi vị trí/sprite/tốc độ), **không `Instantiate`/`Destroy` lúc chạy** → không allocation mỗi khung hình.
  - `ParticleController` **+1 field tuỳ chọn** `followTarget` — có thể parent hạt vào Camera thay vì chính nó. Để trống thì hành vi y hệt cũ (S1-014).
  - 2 prefab: `Particle_FallingLeaves.prefab` (3 sprite lá, ngẫu nhiên), `Particle_SunDust.prefab` (1 sprite bụi).
  - Import + tạo `.meta` cho 4 file `.png` dưới `Assets/Particles/` (chưa có `.meta` nào khi nhận task, chưa có subfolder `Forest/` như mô tả — file nằm thẳng trong `Assets/Particles/`), cấu hình Texture Type = Sprite (2D and UI).
  - `Region_ForgottenForest.asset.particlePrefabs` = [`Particle_SunDust`, `Particle_FallingLeaves`].
  - `SampleScene.unity` — `ParticleController.followTarget` → Main Camera Transform (fileID `519420032`). **Không thêm/sửa component nào TRÊN GameObject Main Camera** — chỉ tham chiếu Transform có sẵn, y hệt cách `SkyPlane` làm con Camera ở S1-013.
- **Quyết định đáng nhớ — vì sao không dùng Shuriken ParticleSystem:** Dự án không có Unity Editor để dựng/kiểm tra asset ParticleSystem bằng tay — YAML Shuriken rất dễ ra asset hỏng (sai field, thiếu module) mà không cách nào phát hiện trước khi mở Editor. Toàn bộ hiệu ứng runtime trước giờ của dự án (`ConstellationRestoreSequence`, `SkyManager`) đều dựng bằng code, không phải asset Editor — `AmbientParticleField` theo đúng tiền lệ đó, đổi lấy rủi ro-bằng-0 lấy chi phí phải tự viết vòng lặp thay vì cấu hình Inspector quen thuộc.
- **Quyết định đáng nhớ — "không sửa Camera" hiểu như thế nào:** Không đụng `CameraFollow2D.cs` hay component nào có sẵn trên GameObject Main Camera. `ParticleController.followTarget` chỉ ĐỌC Transform của Camera để làm cha cho hạt spawn ra — đúng nguyên tắc đã áp dụng cho `SkyPlane` ở S1-013 (parent làm con Camera trong hierarchy ≠ sửa logic Camera).
- **Sorting Order — đánh đổi an toàn hơn đúng-tuyệt-đối-theo-thứ-tự đề xuất:** Layer sorting trong project chỉ có 1 Sorting Layer ("Default") với `TransparencySortMode: Default` (tie-break theo khoảng cách camera, không đoán trước được với 2 sprite cùng Z). Player và Environment đều đang ở `sortingOrder 0`. Đặt Falling Leaves ở `sortingOrder -1` (chắc chắn sau Player, thay vì đúng-y-nguyên "trước Environment" như sơ đồ gợi ý) để **đảm bảo tuyệt đối "particles do not hide the player"** — ưu tiên yêu cầu cứng (test case tường minh) hơn gợi ý mềm ("recommended order"). Sun Dust ở `-60` (giữa Background `-100` và Environment `0`), đúng sơ đồ.
- **Không kiểm chứng được:** Tôi không nghe/nhìn được kết quả thật — mọi con số (`maxParticles`, tốc độ, opacity, sortingOrder) là ước lượng theo mô tả chữ, cần chỉnh bằng mắt trong Editor.
- **Không nên đổi:** Không thêm particle cho Cloud Garden ở sprint này — đúng scope. Không tạo thêm asset môi trường mới ngoài 4 sprite đã cho. Không đổi Lighting.

### S1-014C-004 — Bugfix: Falling Leaves Không Hiện
- **Triệu chứng:** Vào Forgotten Forest không thấy lá bay, dù logic C# (đếm, di chuyển, fade) chạy đúng và mọi guid/reference đã kiểm tra khớp.
- **Nguyên nhân:** `ParticleController.Switch()` gọi `Instantiate(prefab, parent.position, Quaternion.identity, parent)` — ép **vị trí spawn trùng hệt `parent.position`**, xoá mất offset mà bản thân prefab tự khai báo. Khi `parent` = Main Camera Transform (world z = **-10**), hạt sinh ra đúng tại vị trí camera — **gần hơn Near Clip Plane (0.3)** — bị camera cắt bỏ hoàn toàn, không bao giờ render dù mọi thứ khác đều đúng.
- **Fix:**
  - `ParticleController.Switch()` — bỏ overload `Instantiate(prefab, position, rotation, parent)` (ngữ nghĩa mập mờ, đã chính là nguồn bug), thay bằng `Instantiate(prefab)` → `SetParent(parent, worldPositionStays: false)` → copy tường minh `localPosition`/`localRotation`/`localScale` từ CHÍNH prefab. Prefab tự quyết định nó cách `parent` bao xa, không bị ai ép về (0,0,0).
  - `Particle_FallingLeaves.prefab` và `Particle_SunDust.prefab` — `m_LocalPosition` đổi từ `{0,0,0}` → **`{0, 0, 10}`**. Camera ở world z=-10, cộng offset local z=10 → world z=0, đúng độ sâu nơi Player/Platform/StarFragment đang render (đã xác nhận bằng cách đọc trực tiếp `m_LocalPosition` của các object đó trong scene).
- **Tương thích ngược:** Trường hợp `followTarget` để trống (hạt đứng yên trong world, hành vi S1-014 gốc) không đổi — mọi prefab cũ có `localPosition (0,0,0)` vẫn spawn đúng y hệt vị trí `transform` của `ParticleController`, y hệt trước khi sửa.
- **Bài học:** `Instantiate(prefab, position, rotation, parent)` **ép world position tuyệt đối**, xoá sạch offset prefab tự khai báo — chỉ an toàn khi parent đứng ở gốc toạ độ có ý nghĩa (`AtmosphereSystem` tại (0,0,0) thì vô hại), **nguy hiểm ngay khi đổi sang parent có vị trí riêng** (Camera). Bất kỳ lần nào đổi `followTarget`/parent sau này đều phải rà lại toàn bộ prefab con xem `localPosition` có còn hợp lý ở world space mới hay không.
- **Không nên đổi:** Không quay lại dùng overload `Instantiate(prefab, position, rotation, parent)` cho `ParticleController` — đã có bằng chứng cụ thể nó không an toàn với parent tuỳ ý.

### S1-014C-005 — Bugfix: Hình Particle Quá To
- **Bối cảnh:** Sau S1-014C-004 (lá đã render), người dùng tự chuyển 4 file `.png` từ `Assets/Particles/` vào `Assets/Particles/Forest/` qua Unity Editor (khớp đúng cấu trúc thư mục task gốc mô tả) — Editor tự reimport, guid giữ nguyên nên không ảnh hưởng reference nào.
- **Triệu chứng:** Lá/bụi hiện ra nhưng **to bất thường**, gần như che kín màn hình.
- **Nguyên nhân:** Ảnh nguồn có độ phân giải rất cao — `leaf_01` 1254×1254px, `leaf_02`/`leaf_03`/`dust_01` 1024×1024px — trong khi `spritePixelsToUnits` để mặc định **100**. Native size của 1 chiếc lá ở scale=1 vì vậy là **~10.24–12.54 world unit**, gần bằng cả chiều cao khung nhìn camera (ortho size 5 → cao ~10 unit). `scaleMin/scaleMax` (0.5–1) chỉ nhân thêm vào cái nền đã quá lớn đó.
- **Fix:**
  - Chuẩn hoá `spritePixelsToUnits` **theo từng ảnh** để native size (scale=1) = đúng 1 world unit cho cả 4 sprite: `leaf_01` → 1254, `leaf_02`/`leaf_03`/`dust_01` → 1024. Từ giờ giá trị `scaleMin`/`scaleMax` trong prefab **chính là kích thước cuối cùng tính bằng world unit** — dễ suy luận, không còn phải nhẩm qua PPU.
  - `Particle_FallingLeaves.prefab`: `scaleMin/scaleMax` 0.5–1 → **0.25–0.45** (nhỏ hơn Player 0.75, gần cỡ StarFragment 0.4).
  - `Particle_SunDust.prefab`: `scaleMin/scaleMax` 0.15–0.35 → **0.05–0.12** (hạt bụi li ti).
- **Không nên đổi:** Nếu sau này thêm sprite particle mới, luôn set `spritePixelsToUnits` = đúng số pixel cạnh ảnh (ảnh vuông) hoặc quy đổi tương ứng — đừng để mặc định 100 cho ảnh nguồn độ phân giải cao rồi cố gắng bù bằng scale rất nhỏ (0.03–0.05), dễ nhầm và khó đoán.

### S1-014C-006 — Bugfix: Lá Chỉ Rơi Ở 1/3 Trên Màn Hình
- **Triệu chứng:** Sau khi kích thước đã đúng (S1-014C-005), lá chỉ xuất hiện ở khoảng 1/3 phía trên màn hình, không rơi lan xuống hết chiều cao.
- **Nguyên nhân:** `Respawn()` chỉ ngẫu nhiên hoá toạ độ Y trên TOÀN vùng phát đúng **1 lần duy nhất lúc `Awake()`**. Mọi lần tái sinh sau đó (hết vòng đời) đều bị ép cứng về đỉnh vùng (`areaSize.y * 0.5f`). Vòng đời (6–12s) × tốc độ rơi (0.2–0.5 unit/s) chỉ đủ đi được ~1.2–6 unit, trong khi cả vùng cao 12 unit (cần ~30s+ mới rơi hết) — lá luôn "dạt ngược lên đỉnh" trước khi kịp rơi xuống được nửa dưới màn hình.
- **Fix:** `AmbientParticleField.Respawn()` tách 1 cờ `randomizeElapsed` thành **2 cờ độc lập** `randomizeElapsed` + `randomizeY`:
  - **Hết vòng đời** → `randomizeY: true` — coi như một chiếc lá MỚI xuất hiện ở độ cao bất kỳ trong vùng, không phải chiếc cũ dạt lên đỉnh. Đây là fix chính.
  - **Rơi chạm đáy vùng** (hiếm khi xảy ra, vì lifetime thường hết trước) → `randomizeY: false`, giữ nguyên hành vi reset về đỉnh để cảm giác "rơi liền mạch từ trên xuống" không bị phá.
  - `randomizeElapsed` giữ nguyên ý nghĩa cũ, chỉ dùng lúc `Awake()`.
- **Bài học:** Gộp 2 khái niệm khác nhau ("hạt đã trôi được bao lâu" và "hạt xuất hiện ở đâu") vào chung 1 tham số boolean là nguồn bug — chỉ đúng ở đúng 1 trường hợp gọi (Awake) mà sai ở các trường hợp gọi còn lại (respawn giữa lúc chơi). Tham số nào có ý nghĩa khác nhau giữa các call site thì phải tách riêng, không dùng chung 1 cờ cho "tiện".
- **Không nên đổi:** Không gộp lại `randomizeElapsed`/`randomizeY` thành 1 cờ dù có vẻ "gọn hơn" — đã có bằng chứng cụ thể đó chính là nguyên nhân bug.

### S1-014C-007 — Forgotten Forest Parallax Atmosphere (thiết kế lại)
- **Objective:** Nâng cấp hệ hạt Forgotten Forest sang **kiến trúc parallax nhiều lớp** để tạo chiều sâu — "thế giới di chuyển quanh người chơi", không phải "cả khu rừng bám theo người chơi". Thay thế hoàn toàn cách tiếp cận "làm con trực tiếp của Camera" ở S1-014C-003 (bám 100%, không có độ sâu).
- **Result:**
  - `ParallaxLayer` (`StarSower.Biome`, mới) — component reusable: mỗi khung hình đọc `Camera.main.transform.position`, tự ghi vị trí CHÍNH MÌNH bằng một PHẦN TRĂM chuyển động của camera (`parallaxFactor`). Không parent vào Camera, không sửa Camera/Player — chỉ đọc.
    - Tự tìm `Camera.main` lúc `Awake()` thay vì field kéo-thả: component nằm trong prefab hạt, spawn ĐỘNG lúc chạy qua `ParticleController.Switch()` — không cách nào kéo-thả tham chiếu Camera của 1 scene cụ thể vào file prefab dùng chung.
    - **Tự "neo lại" (`Rebase()`)** khi độ lệch tích luỹ vượt `maxOffsetBeforeRebase` (mặc định 6 unit) — Forgotten Forest cao ~26 unit trong khi factor nền chỉ 0.2–0.3, nếu không neo lại lớp nền sẽ trôi ra khỏi khung hình vĩnh viễn sau vài màn hình leo. Neo lại xảy ra đúng tại vị trí đã tính nên không có bước nhảy hình ảnh.
  - **3 prefab hạt**, mỗi cái = `AmbientParticleField` (đã có từ S1-014C-003) + `ParallaxLayer` mới:

    | Prefab | Sprite | Số hạt | Kích thước | `sortingOrder` | `parallaxFactor` |
    |---|---|---|---|---|---|
    | `Particle_SunDust` | dust_01 | 16 | 0.05–0.12 | -60 (sau Environment) | 0.2 |
    | `Particle_BackgroundLeaves` *(mới, thay `Particle_FallingLeaves`)* | leaf_01/02/03 | 5 | 0.15–0.3 (nhỏ hơn = xa hơn) | -10 (sau Player) | 0.3 |
    | `Particle_ForegroundLeaves` *(mới, optional theo spec)* | leaf_01/02/03 | 2 | 0.35–0.55 (to hơn = gần hơn) | **1 (trước Player)** | 0.9 |

  - `Region_ForgottenForest.asset.particlePrefabs` = [SunDust, BackgroundLeaves, ForegroundLeaves] — bỏ `Particle_FallingLeaves` ra khỏi danh sách dùng.
  - `SampleScene.unity` — `ParticleController.followTarget` trả về `{fileID: 0}` (bỏ parent-vào-Camera). Parallax giờ do `ParallaxLayer` tự tính, không cần parenting nữa.
  - Cả 3 prefab `m_LocalPosition.z` = **0** (không còn cần offset z=10 kiểu S1-014C-004 — offset đó chỉ để né Near Clip Plane khi làm con Camera ở z=-10; giờ hạt không còn là con Camera, chỉ cần khớp độ sâu world z=0 nơi Player/Platform đang render).
- **Quyết định đáng nhớ — vì sao Foreground Leaves phá lệ "particles luôn sau Player":** S1-014C-003 từng cố định mọi hạt ở `sortingOrder ≤ -1` để tuyệt đối không che Player. Layer tiền cảnh MỚI này **cố ý** đặt `sortingOrder: 1` (trước Player) vì bản chất chức năng của nó là "một chiếc lá lướt qua gần ống kính" — đúng yêu cầu spec. An toàn nhờ số lượng cực thấp (tối đa 2 hạt cùng lúc, so với 12 của layer cũ) nên rủi ro che khuất Player trên thực tế rất nhỏ, và đã ghi rõ ràng trong Inspector để dễ tắt layer này nếu chơi thử thấy khó chịu.
- **Đã xoá orphan:** Không xoá file `Particle_FallingLeaves.prefab` khỏi đĩa (asset tôi tự tạo, nhưng giữ lại phòng khi cần đối chiếu) — chỉ gỡ khỏi `RegionData.particlePrefabs`, không còn được dùng.
- **Không nên đổi:** Không dùng lại cách parent-vào-Camera (`ParticleController.followTarget`) cho hạt môi trường có ý định tạo độ sâu — làm vậy triệt tiêu hoàn toàn hiệu ứng parallax (factor luôn = 1 tuyệt đối). `followTarget` vẫn là field hợp lệ cho các nhu cầu khác (hiệu ứng bám camera 100% có chủ đích), chỉ là không dùng cho trường hợp này.
- **Chưa playtest** — chỉ qua audit tĩnh + đối chiếu guid + đối chiếu tên field.

### S1-014C-008 — Forgotten Forest Sky & Lighting Polish + Constellation Title
> **Cảnh báo trùng tên:** yêu cầu sprint này lại đặt tên "S1-014C-003" (đã dùng cho Particle Atmosphere). Đổi số thành **S1-014C-008**. Xem mục 13 #13 — luôn tin mục 10.1 làm nguồn sự thật về số sprint.
- **Objective:** Hoàn thiện không khí hình ảnh Forgotten Forest: bầu trời bình minh dịu, ánh sáng ấm, và giới thiệu tên chòm sao của khu vực. **Presentation only.**

**Feature 1 — Sky.** `Region_ForgottenForest.asset`, giảm bão hoà + ấm hơn:

| | Cũ | Mới |
|---|---|---|
| Chân trời | (0.98, 0.82, 0.62) | (0.99, 0.88, 0.76) kem đào dịu |
| Giữa trời | (0.72, 0.85, 0.75) ngả xanh lá | (0.87, 0.85, 0.82) gần trung tính ấm |
| Đỉnh trời | (0.45, 0.72, 0.82) | (0.62, 0.74, 0.84) xanh nhạt hơn |
| Camera BG | (0.45, 0.72, 0.82) | (0.62, 0.74, 0.84) khớp đỉnh trời |
| Lớp nền rừng | (0.10, 0.20, 0.10, α 0.55) | (0.13, 0.20, 0.14, α 0.50) ấm hơn, lộ trời hơn |

**Feature 2 — Lighting.** `SampleScene` Global Light 2D: màu (0.75, 0.95, 0.7) ngả xanh lá, intensity 0.9 → **(1.0, 0.95, 0.87) trắng ấm, intensity 1.0**.
- **Quyết định — "softer" đạt bằng tông ấm, KHÔNG bằng giảm sáng:** yêu cầu vừa "softer lighting" vừa "do not reduce gameplay visibility" là mâu thuẫn nếu hiểu "softer = tối hơn". Ánh sáng mới có mọi kênh ≥ 0.87 (cũ: R 0.675, G 0.855, B 0.63) nên **sáng hơn và dễ nhìn hơn**, phần "dịu" đến từ việc bỏ tint xanh lá gắt.
- **Tác dụng phụ có lợi:** Light 2D nhân vào cả SkyPlane/BackgroundPlane (dùng material sprite-lit). Trước đây tint xanh lá làm bầu trời hiện ra KHÔNG giống gradient đã khai trong asset; giờ ánh sáng gần trắng nên bầu trời hiển thị sát giá trị authored.
- **Quyết định — lighting để trong scene, KHÔNG đưa vào `RegionData`:** cả 4 Region còn lại đã lưu màu Light 2D riêng trong scene của chúng. Đưa riêng Forgotten Forest sang `RegionData` sẽ tạo **hai nguồn sự thật** cho cùng một thứ. Muốn data-driven thì phải migrate cả 5 scene — nằm ngoài phạm vi sprint chỉ đụng 1 region. Đã ghi vào mục 10.4 làm việc dọn dẹp tương lai.

**Feature 3 — Constellation Title.**
- `RegionTitleUI` (`StarSower.UI`, mới) — fade in → giữ 2.5s → fade out ở đầu màn hình. `ShowOnce()` trả về `void` (KHÔNG phải `IEnumerator`) **có chủ đích**: bên gọi không thể vô tình `yield` và chặn gameplay.
- `RegionTitleSession` (`StarSower.UI`, mới) — static, nhớ Region nào đã hiện title trong phiên chơi. Mirror `BiomeSession` (S1-013), có `ResetAll()` cho yêu cầu "unless intentionally reset".
- `RegionData` **+1 field** `constellationTitle`. Forgotten Forest = `The Verdant Crown`. 4 Region khác để trống → không hiện title, không lỗi.
- `LevelFlowManager` **+1 field tuỳ chọn** `regionTitleUI`, gọi `ShowOnce()` **SAU** `SetMovementLocked(false)` và **không yield** → chữ chạy chồng lên gameplay, người chơi di chuyển bình thường.
- `SampleScene` — `Canvas_RegionTitle` (sortingOrder **26**: trên Region Intro 25, dưới Transition 30 nên bị che đúng lúc rời khu vực). Có component uGUI `Shadow` (offset 2,-2, alpha 0.45) làm "subtle shadow". Không có panel nền, `blocksRaycasts: false`.
- **Bố cục title (đã sửa 1 lần — xem ghi chú bên dưới):** `pivot (0.5, 1)` + `anchoredPosition (0, -205)` + `m_Alignment: 1` (UpperCenter). Với pivot ở đỉnh và căn chữ lên trên, con số `-205` mang nghĩa TRỰC TIẾP là "chữ bắt đầu cách mép trên 205px" — chỉnh trong Inspector không cần nhẩm thêm. Nằm ngay dưới `ConstellationProgressLabel` của HUD (kết thúc ở -200), không chồng chữ.

**Ghi chú sửa lỗi bố cục (người dùng báo "tên chòm sao chưa đặt lên đầu"):** bản đầu dùng `anchoredPosition (0, -300)` + `pivot (0.5, 0.5)` + `alignment MiddleCenter`. Hai lỗi cộng dồn:
  1. **Offset pixel cố định không ổn định theo tỉ lệ khung hình.** Canvas Scaler dùng `MatchWidthOrHeight 0.5` với ref 1080×1920, nên chiều cao canvas quy đổi thay đổi theo aspect: portrait 1920 ref-units nhưng **Game view ngang 16:9 chỉ 1080**. Cùng offset -300 cho ra 15.6% (portrait) so với **27.8% (ngang)** — ở màn hình ngang nó rơi xuống gần một phần ba màn hình.
  2. **Căn giữa theo chiều dọc** đẩy chữ xuống thêm nửa chiều cao hộp (70px) so với mép trên hộp.
  Sau khi sửa, chữ bắt đầu ở **10.7% (portrait) / 19.0% (ngang) / 12.3% (iPad 4:3)** — luôn nằm trong dải trên cùng.
- **Ràng buộc còn lại cần biết:** không thể đẩy title cao hơn -205 mà không đè lên HUD `ConstellationProgressLabel` (★☆☆ 12/53, đang ở -170) và `StarsLabel` (⭐ x/y, ở -80). Cả hai HUD này dùng offset pixel cố định. Muốn title lên sát đỉnh hơn nữa thì phải dời hai nhãn HUD đó — một thay đổi HUD, không nằm trong phạm vi sprint trình bày.
- **Quyết định — one-time theo PHIÊN CHƠI, không ghi save:** giống `BiomeSession`. Lý do: sprint "presentation only" không được đụng Save System; và ghi xuống đĩa nghĩa là người chơi vĩnh viễn không xem lại được — mâu thuẫn quyết định #26. Hệ quả: mở lại game thì title hiện lại. Muốn nhớ vĩnh viễn phải thêm field vào `SaveData` + `ProgressManager`.
- **CHƯA KIỂM CHỨNG ĐƯỢC — ký tự ✦ (U+2726):** nằm trong khối **Dingbats**, giống hệt ✨ (U+2728) đã từng phải bỏ ở S1-013.2 vì font builtin Arial không có glyph. Tôi **vẫn ship đúng như yêu cầu** ("Display exactly: ✦ The Verdant Crown ✦") nhưng **rủi ro cao hiện ra ô vuông rỗng**. Nếu bị: sửa `titleFormat` trên `Canvas_RegionTitle` thành `{0}` trơn hoặc `★ {0} ★` (U+2605, khối Miscellaneous Symbols — Arial CÓ, và project đã dùng ★ trong `ConstellationUI`). Là 1 field Inspector, đổi mất 1 giây.
- **Không có font fantasy:** project chỉ có Arial builtin, không có asset font nào. "Elegant" đạt bằng cỡ chữ 64 + **weight thường (không bold)** + màu trắng ấm + shadow, thay vì bold to như `RegionNameLabel`.
- **Không nên đổi:** Không cho `ShowOnce()` trả `IEnumerator`. Không hardcode tên chòm sao trong code — luôn qua `RegionData.ConstellationTitle`.
- **Chưa playtest.**

### S1-014C-009 — Bugfix: Thẻ Tên Chòm Sao Đè Lên Hình Chòm Sao
- **Triệu chứng (ảnh chụp màn hình từ người dùng, `Level_02`):** trong lúc khôi phục chòm sao, chữ "Lyra" / "The Harp" hiện **ngay giữa lòng hình chòm sao**, chồng lên các ngôi sao và nét nối.
- **Ghi nhận sai sót của tôi:** ở lượt trước người dùng báo *"tên chòm sao chưa đặt lên đầu"*, tôi hiểu nhầm là `RegionTitleUI` ("The Verdant Crown") và đi sửa component đó. Thực tế họ nói về **`ConstellationNameCard`** (thẻ "Lyra"/"The Harp" của S1-013.2) — một component hoàn toàn khác. Bản sửa `RegionTitleUI` vẫn giữ lại vì nó độc lập và cũng đúng, nhưng nó **không phải** thứ được báo lỗi.
- **Nguyên nhân:** `ConstellationNameLabel` (0, 40) và `ConstellationDescLabel` (0, -60) đều neo `(0.5, 0.5)` = **giữa màn hình**. Trong khi hình chòm sao vẽ theo `starPoints` chuẩn hoá phủ gần hết chiều cao — Lyra trải từ `y 0.4` đến `0.8` (tức 20%–60% từ đỉnh), bao trọn tâm màn hình. Hai thứ chiếm đúng một chỗ.
- **Fix (hai phía, vì chỉ sửa một phía là chưa đủ khoảng cách):**
  1. **Chữ lên đỉnh:** cả 2 label đổi sang `anchor/pivot (0.5, 1)` + `m_Alignment: UpperCenter`. Tên ở `-60` (cao 110), mô tả ở `-175` (cao 80).
  2. **Hình xuống dưới:** `ConstellationRestoreSequence` **+field `shapeTopMargin` (mặc định 0.18)** và hàm `MapPoint()` nhân toạ độ `y` chuẩn hoá với `(1 - shapeTopMargin)` — đẩy toàn bộ ngôi sao lẫn nét nối xuống, chừa 18% phía trên làm dải tiêu đề. Áp cho **cả sao và nét nối** (nếu chỉ áp cho sao thì nét nối sẽ lệch khỏi sao).
- **Vì sao đặt margin ở lớp trình diễn, không sửa toạ độ trong từng `ConstellationData`:** dữ liệu hình dạng do designer vẽ theo hệ 0..1 toàn màn hình — giữ nguyên ý nghĩa đó. "Chừa chỗ cho chữ" là quyết định **bố cục**, thuộc lớp trình diễn. Nhờ vậy thêm chòm sao mới không phải tự nhớ trừ hao phần đầu màn hình.
- **Kết quả tách bạch** (HUD bị lớp phủ tối của restore sequence che nên dải trên hoàn toàn trống):

  | | Chữ kết thúc | Chòm sao bắt đầu |
  |---|---|---|
  | Portrait 1080×1920 | 13.3% | 34.4% |
  | Ngang 16:9 | 23.6% | 34.4% |

- **Áp cho cả 5 scene**, không riêng Forgotten Forest — vì `ConstellationNameCard` và `ConstellationRestoreSequence` dùng chung ở mọi Region.
- **Bài học:** khi người dùng nói "tên chòm sao", trong project này có **hai** thứ mang nghĩa đó — `RegionTitleUI` (tên chòm sao của Region, S1-014C-008) và `ConstellationNameCard` (tên chòm sao vừa khôi phục, S1-013.2). Phải hỏi lại hoặc xin ảnh trước khi sửa, thay vì đoán.
- **Chưa playtest lại sau khi sửa.**

### S1-014C-010 — Bugfix: Lá Parallax Gây Nhức Mắt Khi Nhảy
- **Triệu chứng:** người dùng quay video báo lá trông "kì kì, hơi nhức đầu" mỗi lúc nhảy. *(Tôi không xem được file `.mov` — công cụ chỉ đọc ảnh/PDF. Chẩn đoán dựa trên code + tính toán biên độ chuyển động, không dựa trên video.)*
- **Nguyên nhân 1 — biên độ quét quá lớn khi camera giật dọc.** Camera bám Player gần 1:1 theo trục Y (`followY: 1`, `smoothTimeY: 0.12`, **không có deadzone dọc**). Với `parallaxFactor` cũ áp CHUNG cho cả X lẫn Y, mỗi cú nhảy ~3.5 unit làm hạt trượt ngược trên màn hình:

  | Lớp | factor cũ | Quét mỗi cú nhảy |
  |---|---|---|
  | Sun Dust | 0.2 | **28% chiều cao màn hình** |
  | Background Leaves | 0.3 | **25%** |
  | Foreground Leaves | 0.9 | 4% |

  Trong platformer nhảy liên tục, nền quét xuống 1/4 màn hình rồi vọt ngược lên mỗi giây — đúng cảm giác nhức mắt.
- **Nguyên nhân 2 — trễ một khung hình.** `CameraFollow2D.LateUpdate()` và `ParallaxLayer.LateUpdate()` đều có `executionOrder: 0`. Thứ tự giữa hai `LateUpdate` cùng mức ưu tiên là **không xác định** — nếu ParallaxLayer chạy trước, nó dùng vị trí camera của khung hình TRƯỚC, gây rung/nhoè rõ nhất đúng lúc camera di chuyển nhanh.
- **Fix:**
  1. `ParallaxLayer.parallaxFactor` đổi từ `float` → **`Vector2`**, tách riêng X và Y. Y đặt sát 1 (0.94–0.98) nên nhảy gần như không làm hạt trôi; X giữ nguyên thấp nên **chiều sâu ngang không mất**.

     | Lớp | X (giữ nguyên) | Y (mới) | Quét mỗi cú nhảy |
     |---|---|---|---|
     | Sun Dust | 0.2 | 0.94 | 28% → **2.1%** |
     | Background Leaves | 0.3 | 0.95 | 25% → **1.8%** |
     | Foreground Leaves | 0.9 | 0.98 | 4% → **0.7%** |

  2. `ParallaxLayer.cs.meta` đặt **`executionOrder: 100`** — đảm bảo chạy SAU `CameraFollow2D` (order 0), hết trễ khung hình.
- **Lợi ích phụ đáng kể:** factor Y cũ 0.3 còn có nghĩa là sau khi leo hết Forgotten Forest (~26 unit), lớp hạt chỉ đi lên 7.8 unit — **tụt lại 18 unit, toàn bộ lá ra khỏi khung hình**. Factor Y 0.95 khiến lớp hạt bám gần camera nên lá luôn còn trên màn hình suốt hành trình leo. Bug này chưa ai báo nhưng chắc chắn sẽ xảy ra.
- **Nguyên tắc rút ra cho game leo dọc:** parallax mạnh (factor thấp) chỉ an toàn trên trục mà camera di chuyển **chậm và mượt**. Trục nào camera giật nhanh thì factor phải sát 1, nếu không hạt thưa sẽ đọc thành nhiễu chứ không thành chiều sâu. Trong Starsower, trục đó là **Y**.
- **Không nên đổi:** Không hạ `parallaxFactor.y` xuống dưới ~0.9 cho bất kỳ lớp hạt nào. Không trả `executionOrder` của `ParallaxLayer` về 0.
- **Chưa playtest lại sau khi sửa.**

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

### Atmosphere & Audio *(S1-014)*
- Mỗi Region có nhạc nền, âm thanh môi trường (2 kênh riêng), hiệu ứng hạt riêng — cùng nằm trong `RegionData`.
- Nhạc + ambient **fade in 2s** khi vào Region, **fade out 1s** khi rời — không cắt cụt.
- Chống phát trùng: gọi lại đúng track đang phát thì không restart từ đầu.
- Hạt đặc trưng (lá bay, sương, sao lấp lánh...) đổi theo Region qua danh sách prefab, không hardcode.
- **Giới hạn đã biết:** crossfade thật (2 bài chồng lên nhau) chỉ xảy ra được trong cùng 1 scene. Qua ranh giới Region (đổi scene), hiệu ứng là "fade out rồi fade in" nối tiếp, không phải chồng lớp — đánh đổi có chủ đích để giữ đúng luật Không Singleton.

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

### Không khí từng Region *(S1-014, ý định thiết kế — chưa có clip/prefab thật)*

| Region | Ambient | Nhạc | Hạt |
|---|---|---|---|
| **Forgotten Forest** | Rừng, chim, gió nhẹ | *(placeholder)* | Lá bay |
| **Cloud Garden** | Gió mềm, mây | *(placeholder)* | Mây trôi |
| **Sky Ruins** | Gió mạnh hơn, tàn tích cổ | *(placeholder)* | Bụi |
| **Aurora Cliffs** | Huyền bí | *(placeholder)* | Hạt sáng, lấp lánh |
| **Moon Gate** | Gần tĩnh lặng, âm trầm sâu | *(placeholder)* | Sao lấp lánh |

> Hệ thống đã sẵn sàng phát đúng những gì designer gắn vào `RegionData.defaultMusic` / `.ambient` / `.particlePrefabs`. Hiện tại cả 5 asset Region đều **chưa gắn clip/prefab nào** — kiến trúc xong trước, nội dung sau, đúng scope sprint yêu cầu.

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

### 8.5 Atmosphere & Audio *(S1-014)*
35. **Nhạc + ambient là 2 kênh độc lập**, không trộn chung 1 AudioSource.
36. **Không cắt nhạc đột ngột** — mọi chuyển đổi đều fade, kể cả lúc rời Region (fade out trước khi scene bị huỷ).
37. **Chống phát trùng** — gọi lại đúng track/clip đang phát không được restart từ đầu.
38. Hiệu ứng hạt của Region lấy từ danh sách prefab trong `RegionData`, **không hardcode**.
39. **Không dùng Singleton / DontDestroyOnLoad để giải quyết crossfade xuyên scene.** Giới hạn "fade out rồi fade in" (thay vì crossfade chồng lớp thật) là đánh đổi kiến trúc có chủ đích, không phải lỗi cần vá bằng cách phá luật No Singleton.
40. `RegionAtmosphereManager` đọc Region qua `BiomeManager.Region` — không tự có field `RegionData` riêng.

### 8.6 Gameplay
41. **Không chết khi rơi** — `GameOverManager` bị tắt có chủ đích.
42. Không dùng cơ chế kéo-thả kiểu ná (đã bác từ session đầu).
43. Nhảy phải tha thứ lỗi bấm: coyote time + jump buffer.
44. Khoá di chuyển **không được tắt animation** của Player.

### 8.7 Kiến trúc
45. **Không Singleton.** Phụ thuộc gán qua Inspector.
46. **Không hardcode** số level, số sao, số chapter, số chòm sao, tên level đầu chapter, tên Region.
47. **Single-Writer**: Rigidbody2D ← chỉ `PlayerMotor`; camera position ← chỉ `CameraFollow2D`; file save ← chỉ `ProgressManager`; nền ← chỉ `BackgroundManager`; trời ← chỉ `SkyManager`; audio ← chỉ `AudioManager`; hạt Region ← chỉ `ParticleController`.
48. **Goal chỉ phát event** — không load scene, không lưu, không điều khiển UI.
49. Một class một trách nhiệm, không God Class.
50. Mọi tính năng mới phải mở rộng được qua kế thừa hoặc composition.
51. Giá trị cấu hình luôn `[SerializeField]`, không `public` field.
52. Dùng interface khi có nhiều cách hiện thực (`ITransitionEffect`, `IConstellationRestoreSequence`).
53. Dữ liệu tĩnh của designer nằm trong ScriptableObject; tiến trình người chơi nằm trong SaveData — **không trộn lẫn**, để nhặt sao không ghi đè asset và reset save không mất cấu hình.
54. Không refactor / rename chủ động khi không được yêu cầu.
55. **Phép ghi save "đi lên" và "đi xuống" phải là hai hàm riêng.** Gộp lại là tái tạo bug S1-013.1.
56. Class nào **giữ nhịp** thì class đó sở hữu các con số thời gian dùng chung, không để mỗi bên tự tính rồi lệch nhau.

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
| **Nhạc / ambient theo Region** | **Hệ thống đã thật, có mixing 3 tầng** (`AudioManager` + `RegionAtmosphereManager`, fade in/out, chống trùng, cảnh báo Console khi thiếu clip). **1/10 clip đơn giản đã gắn** — Forgotten Forest Music. 9 ô còn lại (4 Music khác + 5 Ambient đơn) vẫn trống |
| **Ambient nhiều lớp (Birds/Leaves)** | **Chỉ Forgotten Forest có** (`Ambient_Forest.asset`). Từ S1-014C-002: **không còn Wind** — chỉ Birds + Morning Bird + Leaves, mix 15%/15%/5%. 4 Region còn lại (Cloud Garden, Sky Ruins, Aurora Cliffs, Moon Gate) **chưa có `AmbientProfile` nào** — hệ thống `LayeredAmbientPlayer` đã sẵn sàng dùng lại (kể cả layer Loop cho gió của region khác), chỉ thiếu asset + component trong scene |
| **Hiệu ứng hạt theo Region** | **Forgotten Forest đã có** (lá rơi + bụi nắng, `AmbientParticleField`, S1-014C-003). 4 Region còn lại **chưa gắn prefab nào** → không có hạt |
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
- **S1-013, S1-013.2, S1-014 CHƯA được playtest.** Tất cả mới qua audit toàn vẹn tĩnh (trùng fileID, tham chiếu treo, đối ứng cha-con, kiểu component, GUID font, `CanvasRenderer` anh em) nhưng **chưa ai bấm Play** để xác nhận bầu trời, thẻ tên, nhịp trình diễn, hay audio fade in/out.
- **Không có "tiến trình vĩnh viễn" cho Constellation.** Game chưa phân biệt "đã từng khôi phục" (nên giữ mãi) với "đã khôi phục trong lượt này" (reset được). Vào Level 1 là 53 fragment biến mất khỏi save. Đúng thiết kế đã chốt, nhưng mâu thuẫn với tầm nhìn "gieo lại các vì sao" — cần giải quyết nếu làm sky gallery.
- **Crossfade audio KHÔNG thật sự chồng lớp qua ranh giới scene** — chỉ fade-out-rồi-fade-in nối tiếp (xem S1-014 và mục 8.5 #39). Đây là giới hạn đã biết và chấp nhận, không phải bug; chỉ trở thành vấn đề nếu sau này có yêu cầu crossfade liền mạch thật sự (khi đó buộc phải cân nhắc lại luật No Singleton).

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
| Falling Leaves không hiện dù logic đúng *(S1-014C-004)* | `Instantiate(prefab, position, rotation, parent)` ép world position = `parent.position`. Parent = Camera (world z=-10) → hạt sinh ra đúng vị trí camera, gần hơn Near Clip Plane (0.3) → bị cắt | **Đổi `parent`/`followTarget` của bất kỳ hệ thống spawn nào phải rà lại `localPosition` mọi prefab con** — offset an toàn với parent cũ (gốc toạ độ) có thể gây bug im lặng với parent mới (Camera, hoặc bất kỳ đâu không phải gốc) |
| Lá parallax nhức mắt khi nhảy *(S1-014C-010)* | Một `parallaxFactor` dùng chung cho cả X lẫn Y. Camera giật dọc gần 1:1 mỗi cú nhảy → hạt quét ngược **25–28% màn hình**; cộng thêm trễ 1 khung hình do 2 `LateUpdate` cùng `executionOrder: 0` | **Parallax mạnh chỉ an toàn trên trục camera đi chậm/mượt.** Trục camera giật nhanh (Y trong game leo dọc) phải để factor sát 1. Và **hai `LateUpdate` phụ thuộc nhau bắt buộc phải đặt `executionOrder` tường minh** |

> **Lưu ý dùng `git diff`:** S1-012 đã commit (`1add22d`), nên mọi thay đổi từ S1-013 trở đi vẫn còn ở dạng chưa commit và diff được so với mốc đó.

---

## 10. ROADMAP

### 10.1 Sprint đã xong
`S1-001` → `S1-002` → `S1-003` → `S1-004` → `S1-005` → `S1-006` → `S1-007` → `S1-008` → `S1-008.1` → `S1-009` → `S1-010` → `S1-011` → `S1-012` → `S1-013` → `S1-013.1` → `S1-013.2` → `S1-014` → `S1-014B` → `S1-014C` → `S1-014C-001` → `S1-014C-002` → `S1-014C-003` → `S1-014C-004` → `S1-014C-005` → `S1-014C-006` → `S1-014C-007` → `S1-014C-008` → `S1-014C-009` → **`S1-014C-010`**

> **Cảnh báo trùng tên (lịch sử):** một bản tài liệu trước từng đặt "S1-013" cho một sprint *Sky Look & Constellation Art* chưa từng được duyệt, và một bản khác từng đề xuất "S1-014 – Constellation Persistence & Sky Gallery" làm sprint tiếp theo. Cả hai tên đó **đã đổi số** — nội dung Sky Look chuyển thành `S1-015c` bên dưới, nội dung Persistence chuyển thành `S1-015` bên dưới. Sprint S1-014 **thật sự đã làm** là **Atmosphere & Audio Foundation**.

### 10.2 Sprint tiếp theo (đề xuất, **chưa được duyệt**)

**S1-015 – Constellation Persistence & Sky Gallery** *(khuyến nghị)*
- Tách "đã từng khôi phục" (vĩnh viễn) khỏi "đã khôi phục trong lượt này" (reset được) trong `SaveData`.
- Màn hình xem lại các chòm sao đã khôi phục — dùng trường `Icon` hiện chưa ai đọc.
- Lý do ưu tiên: sửa đúng mâu thuẫn với tầm nhìn cốt lõi, không chỉ làm đẹp thêm.

**S1-015b – Parallax & Background Layers**
- Thêm 2–3 lớp nền mỗi Region, cho trôi theo camera với tốc độ khác nhau.
- Khai thác `parallaxFactor` đã cắm sẵn ở S1-013; bật lớp mây để `cloudDensity` có tác dụng thật.
- Không cần asset thật, vẫn placeholder được.

**S1-015c – Sky Look & Constellation Art**
- Camera ngẩng lên bầu trời khi khôi phục, thay lớp phủ tối. Nay khả thi hơn vì đã có Sky Plane thật từ S1-013.
- Thay hình placeholder bằng sprite sao thật + particle.
- Dùng Icon chòm sao trong `ConstellationUI` và trên name card.

**S1-015d – Atmosphere Content Pass**
- Gắn clip nhạc/ambient + prefab hạt thật vào 5 `RegionData` — kiến trúc đã xong từ S1-014, sprint này chỉ là nội dung.
- Cân nhắc thứ tự ưu tiên Region nào cần asset trước (gợi ý: Moon Gate — "gần tĩnh lặng" dễ làm placeholder tốt nhất vì ít cần asset).

### 10.3 Backlog
- **Chapter Complete & Chapter 2** — xử lý ngõ cụt Moon Gate, nối tiếp sang chương mới.
- **Main Menu** — Continue dùng `LastPlayedLevelId`, khôi phục lối vào Level Select.
- **Hệ thống phần thưởng** cho chòm sao đã khôi phục.
- Playtest & cân bằng độ khó 5 region.
- Animation cho Player.

### 10.4 Ý tưởng chưa triển khai *(Suggestions — không tự làm khi chưa được duyệt)*
- `LoadSceneAsync` / additive scene để khử hẳn khựng load — cũng sẽ mở khoá crossfade audio thật xuyên scene (xem mục 9.3 giới hạn hiện tại).
- Đóng gói platform mechanic thành prefab.
- Import asset particle / audio thật.
- Object pooling thật sự.
- Chuyển sang TextMeshPro *(sẽ giải quyết luôn chuyện thiếu glyph ✨)*.
- Dọn cảnh báo `CS0618`.
- Dọn YAML thừa: `LevelFlowManager` trong 5 scene còn 2 trường `constellationManager` / `restoreSequenceSource` sót từ bản S1-012 đầu. Unity bỏ qua, vô hại.
- **Đưa màu/cường độ Global Light 2D vào `RegionData`** — hiện mỗi scene tự lưu riêng (xem S1-014C-008). Muốn data-driven thì phải migrate cả 5 scene một lượt để tránh hai nguồn sự thật, không làm lẻ tẻ từng region.

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
- Kiến trúc đã ổn định qua 14 sprint, ranh giới trách nhiệm rõ, có điểm mở rộng bằng interface.
- Hệ save đầy đủ, hoạt động xuyên phiên chơi, **đã được xác nhận bằng save file thật**.
- Meta progression (Constellation) đã chạy end-to-end.
- Vòng lặp dài nhất của game — **khôi phục bầu trời** — đã hiện diện thật, không còn là ý tưởng.
- **Mỗi Region đã có bản sắc hình ảnh riêng** *(S1-013)* — người chơi cảm nhận được mình đang bước sang một tầng trời khác.
- **Nền tảng không khí (nhạc, ambient, hạt) đã sẵn sàng về mặt kỹ thuật** *(S1-014)* — chỉ còn thiếu nội dung thật, không thiếu hệ thống.

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
3. Đang ở cuối **S1-014C-010**. Content: 1 chapter, 5 region, 53 Star Fragment, 3 chòm sao (mốc 12 / 30 / 53), 5 `RegionData`. **Forgotten Forest đã polish gần hoàn chỉnh**: BGM + ambient Birds/Morning Bird/Leaves (không có Wind) + particle parallax 3 lớp + trời bình minh dịu + ánh sáng ấm + title chòm sao "The Verdant Crown". **4 Region còn lại vẫn trống trơn** về audio/particle/title — đây là khoảng cách lớn nhất hiện tại.
4. Hệ thống được coi là **ổn định, không sửa khi không được yêu cầu**: Player, Camera, Platform, Collectible, Transition, Goal, **Biome, Atmosphere**.
5. `ProgressManager` là nơi **duy nhất** ghi file save. **Hai hàm ghi — lên và xuống — không được gộp lại** (bug S1-013.1). `AudioManager` là nơi **duy nhất** ghi `AudioSource` nhạc/ambient.
6. Trả lời mỗi story theo **5 phần** ở mục 11.3.
7. Việc lớn còn treo: art/audio thật, Chapter Complete, Main Menu, playtest cân bằng, tiến trình Constellation vĩnh viễn.
8. **Đã xác nhận qua save file:** S1-012 chạy đúng end-to-end; bản vá S1-013.1 chạy đúng.
9. **CHƯA playtest:** toàn bộ S1-013 → S1-014C-008 (bầu trời, biome, thẻ tên chòm sao, audio fade/mixing, ambient chim-lá, particle parallax 3 lớp, sky/lighting polish, title "The Verdant Crown"). Mới chỉ qua audit tĩnh — **chưa ai bấm Play để nhìn/nghe**. Rủi ro cụ thể đang treo: ký tự **✦ (U+2726)** trong `titleFormat` nhiều khả năng ra ô vuông rỗng vì Arial builtin thiếu glyph (xem S1-014C-008).
10b. **Bài học ghim lại:** `Instantiate(prefab, position, rotation, parent)` ép world position, xoá offset prefab tự khai — chỉ dùng khi parent chắc chắn ở gốc toạ độ có ý nghĩa. Xem mục 9.4 + S1-014C-004.
10. Khi nghi có regression: **`git diff` với commit `1add22d` (S1-012) trước**, đừng quy lỗi cho sprint mới nhất chỉ vì nó trùng thời điểm.
11. Test tiến trình phải chạy **hai lượt chơi liên tiếp** — bug S1-013.1 vô hình ở lượt đầu.
12. **Không dùng Singleton/DontDestroyOnLoad để "sửa" giới hạn crossfade audio xuyên scene** — đó là đánh đổi kiến trúc có chủ đích (mục 8.5 #39), không phải lỗi.
13. Tên sprint đã đổi số 2 lần trong lịch sử tài liệu — **luôn tin vào mục 10.1 (Sprint đã xong) làm nguồn sự thật**, không tin vào tên sprint nhắc tới trong hội thoại cũ.
