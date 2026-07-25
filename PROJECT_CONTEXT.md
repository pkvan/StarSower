# PROJECT_CONTEXT.md — Starsower

> Tài liệu context chính thức của dự án. Dùng để tiếp tục phát triển sau khi compact.
> Cập nhật lần cuối: sau Sprint **S1-011 – Seamless Journey Transition**.

---

## 1. PROJECT OVERVIEW

| Mục | Nội dung |
|---|---|
| Tên dự án | **Starsower** |
| Thể loại | Platformer 2D leo dọc (vertical climbing platformer), mobile |
| Engine | Unity **6.5 (6000.5.4f1)** |
| Render Pipeline | URP 2D Renderer |
| Ngôn ngữ | C# |
| Input | Legacy Input Manager (`activeInputHandler: 2` = Both) |
| Target | iOS (mobile portrait) |
| Repo | `origin = https://github.com/pkvan/StarSower.git`, nhánh `main` |

**Mục tiêu cuối cùng của game:** người chơi thực hiện một hành trình leo liên tục từ mặt đất lên tận bầu trời, cảm nhận được sự chinh phục độ cao — không phải chuỗi màn chơi rời rạc.

---

## 2. GAME VISION (QUAN TRỌNG NHẤT — KHÔNG ĐƯỢC TỰ THAY ĐỔI)

- Starsower là game **platformer 2D**.
- Người chơi thực hiện **MỘT hành trình duy nhất** từ mặt đất lên đỉnh bầu trời.
- Các level chỉ là các **khu vực (regions / checkpoints)** của **cùng một hành trình**.
- Người chơi **KHÔNG được có cảm giác "qua màn"**.
- Sau mỗi level **chuyển tiếp tự động** sang khu vực mới.
- **Không có nút Next Level** trong gameplay.
- **Goal chỉ là điểm chuyển tiếp** giữa các khu vực.
- Mỗi khu vực có **chủ đề hình ảnh riêng**.
- Gameplay tập trung vào: **leo cao / platforming / khám phá**.
- **Không có combat.**
- **Không có enemy.**
- **Không có boss.**
- Trọng tâm là **trải nghiệm, nhịp độ và cảm giác chinh phục bầu trời**.

---

## 3. GAMEPLAY LOOP

```
Spawn (khu vực mới)
   ↓
Region Intro (tên khu vực fade in → giữ 2s → fade out, tự động)
   ↓
Leo lên
   ↓
Vượt platform (Static / Moving / Falling / Spring / One-Way / Breakable)
   ↓
Thu thập Star Fragment (tuỳ chọn, không bắt buộc)
   ↓
Chạm Goal
   ↓
Khoá điều khiển → đứng yên ~0.4s → camera lướt lên tiếp
   ↓
Transition (fade che màn hình)
   ↓
Lưu tiến trình + mở khoá khu vực kế
   ↓
Load khu vực mới
   ↓
Tiếp tục leo (không thao tác bấm nút nào)
```

---

## 4. ARCHITECTURE

### 4.1 Nguyên tắc phụ thuộc

```
Core  ←  Systems (Player / Camera / Platform / Collectibles / Transition)  ←  Managers / Level  ←  UI
```

- `Core` chỉ chứa interface + event hub, không phụ thuộc ai.
- **Single-Writer Principle**: mỗi `Rigidbody2D` / `Transform` chỉ có đúng 1 class được phép ghi.
  - `PlayerMotor` là nơi duy nhất ghi `Rigidbody2D` của Player.
  - `CameraFollow2D` là nơi duy nhất ghi `transform.position` của Main Camera (trừ lúc `LevelFlowManager` tắt nó để tự lái).

### 4.2 Scenes

| Scene | Region | Ghi chú |
|---|---|---|
| `SampleScene.unity` | Forgotten Forest | Level 1, scene khởi đầu |
| `Level_02.unity` | Cloud Garden | |
| `Level_03.unity` | Sky Ruins | |
| `Level_04.unity` | Aurora Cliffs | |
| `Level_05.unity` | Moon Gate | Level cuối Chapter 1 |

Cả 5 scene đã đăng ký trong `ProjectSettings/EditorBuildSettings.asset`.

### 4.3 Prefabs / Assets

| Asset | Vai trò |
|---|---|
| `Platform_Basic.prefab` | Platform tĩnh cơ bản (dùng bởi PlatformSpawner) |
| `Platform_Wide.prefab` | Platform tĩnh rộng |
| `StarFragment.prefab` | Vật phẩm thu thập |
| `LevelSelectEntry.prefab` | 1 dòng trong danh sách Level Select |
| `LevelDatabase.asset` | ScriptableObject chứa danh sách 5 level (id / tên / scene) |

### 4.4 Scripts theo namespace

#### `StarSower.Core` — interface & event hub
| Script | Vai trò |
|---|---|
| `IInputProvider` | Horizontal / JumpPressed / JumpHeld |
| `IGroundDetector` | IsGrounded |
| `IPlatformPool` | Get / Release platform |
| `ICameraShake`, `ICameraZoom`, `ICameraTarget` | API camera |
| `ILaunchable` | `Launch(Vector2)` — Spring dùng để bắn Player |
| `ITransitionEffect` | `PlayIn` / `PlayOut` — pluggable style chuyển cảnh |
| `PlayerMovementState` | Enum Idle / Running / Jumping / Falling |
| `GameEvents` | Static event hub: `OnGameOver`, `OnLevelComplete(float)` *(cũ)*, `OnLevelCompleted` *(đang dùng)* |
| `DebugOverlaySuppressor` | Tắt Development Console trên build |

#### `StarSower.Player`
| Script | Vai trò |
|---|---|
| `PlayerController` | Orchestrator: đọc input → ra lệnh Motor/JumpController. `SetInputEnabled()` (đóng băng hoàn toàn) + `SetMovementLocked()` (chỉ khoá di chuyển, giữ vật lý/animation) |
| `PlayerMotor` | Nơi duy nhất ghi Rigidbody2D. Move / Jump / Launch / gravity shaping |
| `PlayerJumpController` | Jump Buffer + Coyote Time |
| `GroundChecker` | Phát hiện đất qua `OnCollisionStay2D` + contact normal |
| `PlayerMovementStateMachine` | Suy ra state từ velocity + grounded |
| `InputManager` | Tự chọn Keyboard/Mobile theo `Application.isMobilePlatform` |
| `KeyboardInputProvider`, `MobileInputProvider` | 2 nguồn input cụ thể |

#### `StarSower.CameraSystem`
| Script | Vai trò |
|---|---|
| `CameraFollow2D` | **Camera đang dùng** — follow X (Dead Zone) + Y, không ratchet |
| `CameraFollowY` | Camera "chỉ đi lên" kiểu cũ — **disabled**, giữ cho màn có vực chết |
| `CameraShake`, `CameraZoom` | API juice |
| `TransformCameraTarget` | Adapter Transform → ICameraTarget |

#### `StarSower.Platform`
| Script | Vai trò |
|---|---|
| `Platform` | Marker |
| `PlatformStandDetector` | Phát hiện Player đứng lên / rời (dùng chung) |
| `MovingPlatform` | Kinematic ping-pong |
| `FallingPlatform` | Đứng lên → chờ → rơi → reset |
| `BreakablePlatform` | Đứng lên → chờ → vỡ → respawn |
| `SpringPlatform` | Bắn Player qua `ILaunchable` |
| `OneWayPlatform` | `PlatformEffector2D` |
| `PlatformSpawner`, `PlatformRecycler`, `SimplePlatformPool` | Spawn thủ tục — **hiện disabled** (level dựng tay) |

#### `StarSower.Collectibles`
| Script | Vai trò |
|---|---|
| `StarFragment` | Xoay + bobbing, trigger thu thập, tự huỷ |
| `CollectibleManager` | Đếm tổng (tự động) + đã thu, phát `OnCollectedChanged` |

#### `StarSower.Level`
| Script | Vai trò |
|---|---|
| `GoalController` | **Chỉ** phát `GameEvents.RaiseLevelCompleted()` — không load scene, không save, không điều khiển UI |
| `LevelFlowManager` | **Orchestrator duy nhất** của toàn bộ trình tự chuyển khu vực |
| `LevelTimer` | Đếm thời gian chặng |
| `LevelDatabase` (SO) | Danh sách level — không hardcode số lượng |
| `LevelDefinition` | Data 1 level (id / displayName / sceneName) |
| `ProgressManager` | Diễn giải SaveData, `CompleteLevel()`, `ComputeStarRating()`, nơi duy nhất quyết định khi nào lưu |
| `LevelManager` | Điều hướng scene: level hiện tại, load level kế |
| `LevelCompleteUI` | Popup cũ — **còn code, đã tắt trong scene**, không còn đường vào |

#### `StarSower.Persistence`
| Script | Vai trò |
|---|---|
| `SaveData` / `LevelSaveData` | Plain data |
| `SaveManager` | I/O thuần JSON tại `Application.persistentDataPath` |

#### `StarSower.Transition`
| Script | Vai trò |
|---|---|
| `TransitionEffectBase` | Logic fade CanvasGroup dùng chung |
| `ColorFadeEffect` / `CloudFadeEffect` / `LightFadeEffect` | 3 style (màu / mây / ánh sáng) |
| `SceneTransitionController` | Chọn style qua `fadeType`, gọi PlayIn/PlayOut |

#### `StarSower.UI`
| Script | Vai trò |
|---|---|
| `RegionIntroUI` | Hiện tên khu vực: fade in → giữ → fade out |
| `CollectibleHUD` | Hiện `⭐ collected / total` |
| `OnScreenJoystick`, `TouchButton` | Widget chạm thô |
| `LevelSelectController`, `LevelSelectEntryView` | Level Select — **hiện bị cô lập**, không có đường vào |
| `LevelTitleView` | Title card cũ (dùng bởi LevelIntroSequence, đã tắt) |

#### `StarSower.Managers`
| Script | Vai trò |
|---|---|
| `GameOverManager` | Fall detection — **disabled** (không có chết khi rơi) |
| `LevelIntroSequence` | Cinematic zoom cũ — **disabled**, bị RegionIntroUI thay thế |

#### `StarSower.Effects`
| Script | Vai trò |
|---|---|
| `GroundImpactVFX` | Particle khi tiếp đất mạnh |
| `SpringLaunchVFX` | Particle khi bị Spring bắn |

> **Không có Singleton nào trong project.** Mỗi scene có instance riêng của các Manager; dữ liệu xuyên scene đi qua file save.

---

## 5. COMPLETED SPRINTS

| Sprint | Mục tiêu | Hoàn thành | Cần giữ nguyên |
|---|---|---|---|
| **S1-001** | Project Foundation | Cấu trúc thư mục, quy ước kiến trúc, dependency direction | Dependency direction |
| **S1-002** | Input Foundation | `IInputProvider`, Keyboard/Mobile provider, joystick + nút Jump | Interface input |
| **S1-003** | Player Controller | Physics jump, PlayerMotor, tách orchestrator/motor | Single-Writer trên Rigidbody2D |
| **S1-004** | Camera System | CameraFollowY, Dead Zone, Shake, Zoom API | API Shake/Zoom |
| **S1-004.5** | Gameplay Feel | Accel/decel, variable jump height, coyote time, jump buffer | Toàn bộ giá trị tuning |
| **S1-005** | Movement + Jump audit | PlayerMovementStateMachine, fix loạt bug nhảy | `GroundChecker` dùng contact-normal |
| **S1-006** | Platform Mechanics | 5 loại platform qua composition | Không kế thừa sâu, mỗi hành vi 1 component |
| **S1-007** | First Playable Level | Level đầu hoàn chỉnh, Goal, Level Complete UI, VFX hooks | Cấu trúc level dựng tay |
| **S1-008** | Star Fragments & Collectibles | StarFragment, CollectibleManager, HUD | Tổng số sao đếm tự động |
| **S1-008.1** | Goal Completion Flow | `SetMovementLocked`, dừng camera, star rating, fade | Tách Goal khỏi UI |
| **S1-009** | Level Flow & Progression | SaveManager, ProgressManager, LevelManager, LevelDatabase, Level Select | Không hardcode số level |
| **S1-010** | Chapter 1 Vertical Slice | 5 level với chủ đề/màu/ánh sáng riêng | Thứ tự dạy mechanic |
| **S1-011** | Seamless Journey Transition | LevelFlowManager, SceneTransitionController, RegionIntroUI, bỏ popup | **Goal chỉ phát event** |

---

## 6. CURRENT FEATURES

**Player**
- Movement (accel / decel / air control)
- Jump + Variable Jump Height
- Coyote Time (0.15s)
- Jump Buffer (0.15s)
- Ground Detection (contact normal, hoạt động cả khi đứng mép)
- Movement Lock (giữ vật lý/animation) vs Input Disable (đóng băng)

**Camera**
- Follow X (Dead Zone 2 unit) + Follow Y (bám sát)
- Không chết khi rơi — camera bám xuống tự do
- Camera Drift khi hoàn thành chặng
- Shake / Zoom API (chưa dùng trong gameplay)

**Platform**
- Static
- Moving (ping-pong)
- Falling (rơi + auto reset)
- Breakable (vỡ + respawn)
- Spring (bắn cao)
- One-Way (xuyên từ dưới)

**Collectibles**
- Star Fragment (xoay + bobbing + trigger)
- HUD hiển thị `⭐ x / y`, tự cập nhật qua event
- Star Rating 1–3 sao (100% / ≥50% / còn lại)

**Flow & Progression**
- Goal → event → transition tự động
- Region Intro (tên khu vực)
- 3 style transition: Color / Cloud / Light
- Save JSON: level unlocked, sao mỗi level, tổng Star Fragment, tổng thời gian chơi, `lastPlayedLevelId`
- Auto unlock + auto save ngay khi hoàn thành chặng

**Khác**
- Debug overlay suppressor
- VFX hooks (tiếp đất / spring) — chưa gán particle thật

---

## 7. CURRENT LEVELS

| # | Scene | Region | Mechanic sử dụng | Platform | Sao |
|---|---|---|---|---|---|
| 1 | `SampleScene` | **Forgotten Forest** | Dạy Movement, Jump, thu thập Star (chỉ Static) | 14 | 10 |
| 2 | `Level_02` | **Cloud Garden** | Giới thiệu **Moving Platform** + Static | 16 | 10 |
| 3 | `Level_03` | **Sky Ruins** | Giới thiệu **Falling Platform** + Static, Moving | 17 | 10 |
| 4 | `Level_04` | **Aurora Cliffs** | Giới thiệu **Spring Platform**, kết hợp Static/Moving/Falling | 18 | 11 |
| 5 | `Level_05` | **Moon Gate** | Bài kiểm tra — kết hợp toàn bộ 4 mechanic | 20 | 12 |

Mỗi region có bảng màu platform riêng, `BackgroundPlane` riêng, màu Global Light 2D riêng, màu nền Camera riêng.

---

## 8. DESIGN DECISIONS (ĐÃ CHỐT — KHÔNG ĐƯỢC BỎ SÓT)

### Vision & phạm vi
- **Không có boss.**
- **Không có combat.**
- **Không có enemy.**
- **Goal không phải mục tiêu cuối** của game — chỉ là điểm chuyển khu vực.
- **Level chỉ là checkpoint**, không phải màn chơi độc lập.
- **Transition tự động**, không cần thao tác của người chơi.
- **Người chơi luôn phải cảm thấy đang leo liên tục.**
- **Trải nghiệm quan trọng hơn số lượng mechanic.**
- **Không được biến game thành platformer kiểu Mario.**
- **Constellation là mục tiêu dài hạn** (chưa implement).

### Gameplay
- **Không có chết khi rơi** (mặc định) — camera follow tự do 2 trục, không game over, không respawn. `GameOverManager` + `CameraFollowY` giữ code nhưng disabled, dành cho màn thiết kế riêng có vực chết.
- **Không bắt buộc thu đủ sao** để hoàn thành chặng — Goal luôn cho qua; sao chỉ ảnh hưởng rating.
- Star Rating: 100% → 3 sao, ≥50% → 2 sao, còn lại → 1 sao (hoàn thành luôn tối thiểu 1 sao).
- Input: **joystick trái + nút Jump** (cơ chế kéo-thả kiểu ná đã bị loại bỏ hoàn toàn từ sớm).
- Level dựng **thủ công**, không dùng spawn thủ tục (PlatformSpawner disabled).

### UI
- **Bỏ hoàn toàn popup Level Complete** khỏi luồng chơi (không Retry, không Next Level, không hiện thống kê giữa hành trình).
- Chỉ hiện **tên khu vực** khi vào khu vực mới.
- UI thiết kế cho **portrait mobile**, CanvasScaler reference **1080×1920**.

### Kiến trúc
- **Composition over inheritance** — không có `PlatformBase` kế thừa sâu.
- **Goal chỉ phát event**, không tự load scene / save / điều khiển UI.
- **`ProgressManager` là nơi duy nhất quyết định khi nào lưu**; chỉ nó được gọi `SaveManager`.
- **Không Singleton** — mỗi scene có instance Manager riêng.
- Số lượng level **không hardcode** — đọc từ `LevelDatabase` asset.
- Transition style **pluggable** qua `ITransitionEffect`, đổi bằng Inspector không sửa code.

---

## 9. KNOWN ISSUES

### Placeholder / chưa hoàn thiện
- **Chưa có asset thật**: toàn bộ hình là ô vuông màu; `BackgroundPlane` chỉ là mảng màu phẳng, chưa parallax.
- **Chưa có particle thật**: các field `completionParticle`, `collectParticlePrefab`, `impactParticle`, `launchParticle` đều để trống (có null-check).
- **Chưa có audio**: `collectSound`, `completionSound` để trống.
- **Cloud/Light transition chỉ là màu tint + easing khác nhau**, chưa có sprite mây/ánh sáng thật.
- **Chưa có animation**: không có Animator nào; `SetMovementLocked` đã chừa sẵn chỗ cho Idle/Celebrate.
- **Chưa có Main Menu**, chưa có màn "Hoàn thành Chapter".

### Hệ thống bị cô lập / treo
- **`LevelSelectController` không còn đường vào** (nút cũ nằm trên popup đã tắt).
- **`LevelCompleteUI` còn code nhưng đã tắt**, không còn gì gọi tới.
- **Level 5 (Moon Gate) chạm Goal sẽ chỉ mở lại màn hình** — chưa có màn kết Chapter.
- `PlatformSpawner`, `SimplePlatformPool`, `PlatformRecycler` còn code nhưng disabled; pool là placeholder Instantiate/Destroy, chưa phải object pool thật.

### Cần polish / chưa kiểm chứng
- **Chưa playtest thật độ khó** của Level 2–5; khoảng cách nhảy và Spring velocity là ước lượng.
- **Thời lượng 2–4 phút/level là ước lượng**, chưa đo thực tế.
- **Vị trí Star Fragment ở Level 2–5 là auto-generate**, cần designer chỉnh tay để khuyến khích khám phá đúng nghĩa.
- **Transition chưa thật sự seamless**: dùng `SceneManager.LoadScene` đồng bộ nên có hitch ngắn lúc load (đang che bằng `transitionHoldDuration`). Muốn mượt hẳn cần `LoadSceneAsync` + additive.
- Warning `CS0618` (`FindFirstObjectByType` / `FindObjectsSortMode` obsolete) — không chặn build, chưa xử lý.
- 5 platform mechanic **chưa đóng gói thành prefab**, hiện là instance trực tiếp trong scene.

### Bug đã sửa (ghi lại để tránh lặp)
| Bug | Nguyên nhân | Cách sửa |
|---|---|---|
| Camera rung như động đất | `if/else` tạo bước nhảy giá trị tại biên Dead Zone | Thay bằng `Mathf.Clamp` liên tục |
| Không nhảy được ở mép platform | Vùng dò đất hình cố định neo ở tâm Player | Đổi sang `OnCollisionStay2D` + contact normal |
| Player không di chuyển trên iPhone | `activeInputHandler` sai + wire nhầm KeyboardInputProvider | Đặt `= 2` (Both) + dùng `InputManager` |
| Popup/HUD/tên khu vực **không hiện chữ** | Font builtin trỏ sai bundle: dùng guid `...f000...` thay vì `...e000...` | Sửa 72 tham chiếu font sang `0000000000000000e000000000000000` |
| Text không render | Thiếu component `CanvasRenderer` trên GameObject có `Text` | Thêm CanvasRenderer cho 14 Text × 5 scene + 2 trong prefab |
| Scene lỗi khi mở | `m_Father` trỏ vào GameObject fileID thay vì RectTransform fileID | Sửa `m_Father` của LevelNameText / StarRatingText |
| Nhiều field script null | Trỏ vào GameObject fileID thay vì Component fileID | Sửa `starsText`, `entryPrefab` |
| Unity vào Safe Mode | `LevelCompleteUI` không cập nhật khi `CompleteLevel()` đổi chữ ký | Thêm tham số `elapsedTime` |

> **Bài học quan trọng:** khi viết YAML scene bằng tay, phải phân biệt rõ **fileID của GameObject** và **fileID của Component**. Đây là nguồn gốc của phần lớn bug trong dự án.

---

## 10. NEXT ROADMAP

### Sprint tiếp theo (đã thống nhất)
**S1-012 – Chapter Complete & Main Menu**
- Xử lý điểm dừng ở Moon Gate: màn "Hoàn thành Chapter 1" thay vì chỉ mở lại màn hình.
- Main Menu thật với **Continue** (dùng `ProgressManager.LastPlayedLevelId` đã có sẵn).
- Đưa `LevelSelectController` (đang bị cô lập) có đường vào lại.

### Suggestions (đề xuất — chưa được duyệt, không tự làm)
- Chuyển transition sang `LoadSceneAsync` + additive để thật sự seamless.
- Đóng gói 5 platform mechanic thành prefab tái dùng.
- Import particle + audio asset thật vào các hook đã chừa sẵn.
- Player Animation (Idle / Run / Jump / Celebrate) — `PlayerMovementStateMachine` đã sẵn state.
- Object pool thật thay `SimplePlatformPool`.
- Thay legacy `Text` bằng TextMeshPro.
- Xử lý warning CS0618.

> **Không tự thêm feature ngoài roadmap.** Đặc biệt: không thêm boss, enemy, combat, hoặc mechanic mới.

---

## 11. CODING GUIDELINES

- **Single Responsibility** — một class một trách nhiệm, không God Class.
- **Inspector configurable** — mọi giá trị gameplay expose qua `[SerializeField]`, **không dùng `public` field**.
- **Không hardcode** — số lượng level, số sao, giá trị tuning đều từ data/Inspector.
- **Event-driven khi phù hợp** — hệ thống không nên biết nhau thì giao tiếp qua `GameEvents`.
- **Dễ mở rộng** — ưu tiên composition + interface; mỗi feature mới phải mở rộng được.
- **Không phá hệ thống cũ** — không refactor / đổi tên khi không được yêu cầu.
- **Tách Manager và Controller rõ ràng**:
  - *Controller* = điều khiển 1 đối tượng cụ thể (PlayerController, GoalController).
  - *Manager* = điều phối hệ thống (LevelFlowManager, ProgressManager).
- **Chỉ đụng phần được yêu cầu** — không drive-by changes.
- **Single-Writer** cho Rigidbody2D / Transform.
- Khi có nhiều cách làm, **chọn cách dễ bảo trì cho mobile game**, không chọn cách viết nhanh nhất.
- Comment bằng tiếng Việt, giải thích **tại sao** chứ không phải **cái gì**.

### Quy trình làm việc mỗi Story (từ S1-002 trở đi)
Mỗi story trả lời theo **5 phần cố định trong 1 lượt**:
1. **Thiết kế** — class nào được tạo, trách nhiệm từng class.
2. **Đánh giá rủi ro** — rủi ro mở rộng của kiến trúc đã chọn.
3. **Triển khai** — code thật, ghi vào file.
4. **Kiểm thử** — checklist test thủ công trong Unity.
5. **Đề xuất Story tiếp theo.**

---

## 12. PROJECT STATUS

### **Vertical Slice**

**Lý do:**
- ✅ Đã có **chuỗi 5 level chơi được liên tục** từ đầu đến cuối Chapter 1.
- ✅ Đã có **đầy đủ core loop**: leo → platform mechanic → thu thập → Goal → transition → khu vực mới.
- ✅ Đã có **hệ thống lưu tiến trình** hoạt động xuyên phiên chơi.
- ✅ Vision "hành trình liên tục" đã được hiện thực đúng (không popup, không nút Next Level).
- ❌ **Chưa phải Alpha**: toàn bộ art/audio/VFX là placeholder, chưa có Main Menu, chưa có màn kết Chapter, chưa playtest cân bằng độ khó.

**Việc quan trọng nhất tiếp theo:** hoàn thiện điểm đầu (Main Menu) và điểm cuối (Chapter Complete) của hành trình để Chapter 1 thành một vòng trải nghiệm khép kín.
