# PROJECT_CONTEXT.md — Starsower

> Tài liệu context chính thức của dự án. Cập nhật đến hết **S1-014C (xong)**; **S1-015 đang làm**.
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
- Gameplay tập trung vào đúng 4 thứ: **Platforming · Khám phá · Leo cao · Khôi phục bầu trời**.
- **Star Fragment không phải điểm số.** Mỗi mảnh là một mảnh ánh sáng giúp bầu trời sống lại. Người chơi phải cảm thấy mình đang *"gieo lại các vì sao"*.
- Người chơi phải cảm thấy **bầu trời đang dần sống lại nhờ hành trình của mình**.
- **Đây là hành trình xúc cảm, không phải mechanic phức tạp.** Bản sắc mỗi Region đến từ 5 thứ: **không khí riêng · nhạc riêng · bản sắc hình ảnh · tên chòm sao · mạch cảm xúc đáng nhớ**.
- **Mechanic mới là thứ yếu** so với việc dựng một thế giới liền mạch và đáng nhớ.
- **Gameplay đọc được quan trọng hơn hiệu ứng hình ảnh.** Không khí phải làm gameplay rõ hơn, tuyệt đối không được che nó.

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
  ↓  ├─ chạm mốc Chapter → KHÔI PHỤC CHÒM SAO (giữa gameplay, dừng 1s + trình diễn) → leo tiếp
  ↓
Goal
  ↓
Transition (tự động, không popup)
  ↓
Region mới (nền + trời đổi mượt, tên khu vực ~2s, tên chòm sao Region ~2.5s, nhạc/ambient/hạt riêng)
  ↓
Tiếp tục leo
```

**Trình tự khôi phục chòm sao** (tên và hình chạy song song, không phải nối đuôi):

```
t=0.0   khoá điều khiển, dừng 1 giây
t=1.0   ┌ vẽ chòm sao (trời tối → sao sáng dần → nối nét), chừa 18% màn hình phía trên
        └ fade in TÊN + description ở dải trên cùng — cùng khung hình, không chồng lên hình
        giữ nguyên cả hai 1 giây
        ┌ chòm sao tan
        └ tên tan — cùng khung hình, cùng thời lượng
        trả quyền điều khiển
```

Tổng thời gian khoá: Lyra 6.3s · Cassiopeia 7.8s · Orion 9.8s.

---

## 4. ARCHITECTURE

### 4.1 Nguyên tắc nền

- **Hướng phụ thuộc:** `Core ← Systems ← Managers/Level ← UI`. Tầng dưới không biết tầng trên.
- **Single-Writer Principle** — mỗi thứ chỉ có đúng MỘT class được ghi:

  | Đối tượng | Chỉ được ghi bởi |
  |---|---|
  | `Rigidbody2D` Player | `PlayerMotor` |
  | `transform.position` Main Camera | `CameraFollow2D` (trừ lúc `LevelFlowManager` chủ động tắt component để tự lái) |
  | File save | `ProgressManager` |
  | `SpriteRenderer` nền | `BackgroundManager` |
  | Sky Plane + `Camera.backgroundColor` | `SkyManager` |
  | `AudioSource` nhạc/ambient | `AudioManager` |
  | Hiệu ứng hạt của Region | `ParticleController` |
  | Vị trí hạt parallax | `ParallaxLayer` (đọc `Camera.main`, ghi transform của chính nó — không parent vào Camera) |
  | Vị trí vật trang trí world | `WorldAmbientField` (ghi transform con của chính nó, chỉ khi có trôi ngang) |

- **Event hub:** `GameEvents` (static) để các hệ thống không tham chiếu trực tiếp lẫn nhau.
- **Interface để thay thế:** `ITransitionEffect`, `IConstellationRestoreSequence`, `IInputProvider`, `IGroundDetector`, `ILaunchable`, `ICameraTarget/Shake/Zoom`, `IPlatformPool`.
- **Không Singleton.** Mọi phụ thuộc gán qua `[SerializeField]` trong Inspector, kể cả khi component được spawn động (đọc `Camera.main`/tự tạo con lúc `Awake()` thay vì kéo-thả).

### 4.2 Danh sách script theo namespace

| Namespace | Script chính | Vai trò |
|---|---|---|
| `StarSower.Core` | `GameEvents`, `IInputProvider`, `IGroundDetector`, `ILaunchable`, `ICameraTarget/Shake/Zoom`, `ITransitionEffect`, `IPlatformPool`, `PlayerMovementState` | Hợp đồng dùng chung, không phụ thuộc tầng trên |
| `StarSower.Player` | `PlayerController`, `PlayerMotor`, `PlayerJumpController`, `GroundChecker`, `PlayerMovementStateMachine`, `InputManager` + 2 provider | Di chuyển, nhảy, input |
| `StarSower.CameraSystem` | `CameraFollow2D` (đang dùng) · `CameraFollowY` (tắt) · `CameraShake`/`CameraZoom` (có sẵn, chưa dùng trong content) | Camera bám Player |
| `StarSower.Platform` | `Platform`, `MovingPlatform`, `FallingPlatform`, `SpringPlatform`, `OneWayPlatform`, `PlatformStandDetector` (đang dùng) · `BreakablePlatform`, `PlatformSpawner`/`Recycler`/`SimplePlatformPool` (tắt, level là bố cục thủ công) | Nền |
| `StarSower.Collectibles` | `StarFragment`, `CollectibleManager` | Thu thập sao, đếm tự động |
| `StarSower.Constellations` *(S1-012, S1-013.2)* | `ChapterData`/`ChapterDatabase`/`ConstellationData` (SO), `ChapterProgressManager`, `ConstellationManager` (giữ nhịp trình diễn), `ConstellationUI`, `ConstellationNameCard`, `IConstellationRestoreSequence`, `ConstellationRestoreSequence` | Hệ khôi phục chòm sao |
| `StarSower.Biome` *(S1-013, mở rộng S1-014/S1-014C)* | `RegionData` (SO), `BiomeManager` (nền+trời), `BackgroundManager`, `SkyManager`, `BiomeSession`, `RegionAtmosphereManager` (điều phối audio+particle), `ParticleController`, `AmbientParticleField`, `ParallaxLayer`, `WorldAmbientField` | Bản sắc hình ảnh + không khí từng Region |
| `StarSower.Audio` *(S1-014, S1-014C)* | `AudioManager` (crossfade nhạc+ambient 2 kênh), `AmbientLayerData`/`AmbientProfile` (SO), `LayeredAmbientPlayer` | Nhạc nền + ambient nhiều lớp |
| `StarSower.Level` | `GoalController`, `LevelFlowManager`, `LevelManager`, `LevelDefinition`, `LevelDatabase` (SO), `ProgressManager`, `LevelTimer` (đang dùng) · `LevelCompleteUI` (nghỉ hưu, trái vision) | Điều phối level/region |
| `StarSower.Transition` | `SceneTransitionController`, `TransitionEffectBase`, `ColorFadeEffect`/`CloudFadeEffect`/`LightFadeEffect` | Che/mở màn hình |
| `StarSower.Persistence` | `SaveData`, `SaveManager` | JSON I/O |
| `StarSower.UI` *(+ S1-014C)* | `CollectibleHUD`, `RegionIntroUI`, `OnScreenJoystick`/`TouchButton`, `RegionTitleUI`, `RegionTitleSession` (đang dùng) · `LevelTitleView` (tắt) · `LevelSelectController`/`EntryView` (cô lập, không lối vào) | HUD, intro, title chòm sao Region |
| `StarSower.Managers` | `GameOverManager` (tắt — không chết khi rơi) · `LevelIntroSequence` (tắt, thay bằng `RegionIntroUI`) | — |
| `StarSower.Effects` | `GroundImpactVFX`, `SpringLaunchVFX` | Placeholder |

### 4.3 Scenes

| Scene | Region | Build Index |
|---|---|---|
| `SampleScene.unity` | Forgotten Forest | 0 |
| `Level_02.unity` | Cloud Garden | 1 |
| `Level_03.unity` | Sky Ruins | 2 |
| `Level_04.unity` | Aurora Cliffs | 3 |
| `Level_05.unity` | Moon Gate | 4 |

Mỗi scene chứa cùng một bộ hệ thống: `ConstellationSystem`, `BiomeSystem`, `Canvas_ConstellationName`, `AtmosphereSystem`, `Canvas_RegionTitle`. **Chỉ `SampleScene` (Forgotten Forest) và `Level_02` (Cloud Garden) có nội dung audio/particle/title thật** — 3 scene còn lại có đủ hệ thống nhưng field dữ liệu còn trống (xem mục 9.1).

`AudioManager` và các hạt `AmbientParticleField` đều **tự tạo GameObject con lúc `Awake()`**, không wire `AudioSource`/particle sẵn trong scene — giảm rủi ro sửa YAML tay.

### 4.4 Data Objects chính

| Asset | Nội dung |
|---|---|
| `LevelDatabase.asset` / `ChapterDatabase.asset` / `Chapter_01.asset` | 5 level cùng `chapter_01`, tổng 53 fragment, 3 chòm sao |
| `Constellation_Lyra` / `Cassiopeia` / `Orion` | Mốc 12/30/53, hình dạng + thời lượng + description riêng |
| `Region_ForgottenForest.asset` | **Đầy đủ nhất** — sky gradient bình minh ấm, nhạc, `AmbientProfile` (chim+lá), 3 particle prefab parallax, `constellationTitle: The Verdant Crown` |
| `Region_CloudGarden/SkyRuins/AuroraCliffs/MoonGate.asset` | Có sky gradient + màu nền; **chưa có** nhạc/ambient/particle/title |
| `Ambient_Forest.asset` | 3 layer `RandomOneShot`: Birds, Morning Bird, Leaves — **không có Wind** (bỏ có chủ đích, xem 5. S1-014C) |
| `Particle_SunDust` / `Particle_BackgroundLeaves` / `Particle_ForegroundLeaves` | 3 prefab hạt parallax của Forgotten Forest (xem 5. S1-014C) |
| `CloudField_SkyMotes` / `CloudField_Background` / `CloudField_Foreground` | 3 prefab mây **cố định trong world** của Cloud Garden, dùng `WorldAmbientField` (xem 5. S1-015) |
| `StarFragment.prefab`, `Platform_Basic/Wide.prefab` | Prefab gameplay cơ bản |

### 4.5 Save System

- JSON qua `JsonUtility`, tại `~/Library/Application Support/DefaultCompany/StarSower/starsower_save.json`.
- `ProgressManager` là **lớp diễn giải + nơi ghi đĩa duy nhất**. `SaveManager` chỉ biết đọc/ghi, không hiểu ý nghĩa dữ liệu.
- **Hai chiều ghi tách bạch, không được gộp** *(bài học S1-013.1)*: `WriteChapterProgress()` chỉ đi lên (dùng lúc chơi bình thường); `ResetChapterProgress()` chỉ đi xuống (dùng khi bắt đầu lại chapter).
- `BiomeSession`, `RegionTitleSession` cố tình **không** nằm trong save — là trạng thái một phiên chơi, không phải tiến trình người chơi.

### 4.6 Bộ số tham chiếu nhanh

| Component | Giá trị |
|---|---|
| `PlayerMotor` | moveSpeed 5 · jumpForce 12 · fallMultiplier 2.5 · airControl 0.8 |
| `PlayerJumpController` | jumpBufferTime 0.15 · coyoteTime 0.15 |
| `CameraFollow2D` | offset (0,1) · deadZoneWidth 2 (ngang) · smoothTimeY 0.12, **không deadzone dọc** |
| `LevelFlowManager` | cameraDelay 0.4 · driftDuration 0.6 · transitionHold 0.3 |
| `ConstellationManager` | pauseBeforeRestore 1.0 · holdAfterReveal 1.0 · fadeOutDuration 0.8 |
| `ConstellationRestoreSequence` | tỉ lệ vẽ 0.2/0.3/0.2 (sky/stars/lines) · **shapeTopMargin 0.18** (chừa chỗ cho thẻ tên) |
| `ConstellationNameCard` | anchor top, tên tại -60, mô tả tại -175 |
| `AudioManager` | masterVolume/musicVolume/ambientVolume mặc định 1 (mixing không live-reactive) |
| `LayeredAmbientPlayer` (Forest) | Birds/Morning Bird/Leaves volume 0.15/0.15/0.05 = % nghe được cuối cùng trực tiếp |
| `AmbientParticleField` × 3 (Forest) | SunDust 16 hạt · BackgroundLeaves 5 hạt · ForegroundLeaves 2 hạt |
| `ParallaxLayer` × 3 (Forest) | **`parallaxFactor` là `Vector2`**, X thấp (0.2/0.3/0.9) cho chiều sâu ngang, Y sát 1 (0.94/0.95/0.98) để nhảy không làm hạt trôi. `executionOrder: 100` (chạy sau `CameraFollow2D`) |
| `RegionTitleUI` (Forest) | fadeIn 1s · hold 2.5s · fadeOut 1.2s · `sortingOrder 26` |

---

## 5. LỊCH SỬ SPRINT *(đã xong + đang làm)*

### S1-001 → S1-011 — Nền tảng gameplay & trình bày
Kiến trúc SOLID (S1-001) → di chuyển + nhảy có coyote/buffer (S1-002/003) → camera bám (S1-004) → 5 loại platform (S1-005) → mobile input (S1-006) → level chơi được đầu tiên + Goal flow chuẩn hoá (S1-007/008.1) → Star Fragment + Save/Progress (S1-008/009) → 5 region dạy mechanic (S1-010) → chuyển region liền mạch không UI cắt ngang (S1-011, `LevelFlowManager`/`SceneTransitionController`/`RegionIntroUI` ra đời, `LevelCompleteUI` nghỉ hưu).
**Không nên đổi:** Single-Writer, không Singleton, không hardcode số level/sao, Goal chỉ phát event.

### S1-012 — Constellation Restoration System
Star Fragment thành mảnh ánh sáng, không phải điểm. `ChapterData`/`ConstellationData` (data, không hardcode Chapter 1), `ChapterProgressManager` cộng dồn fragment + phát hiện mốc 12/30/53, `ConstellationManager` chạy sự kiện khôi phục giữa gameplay (không chuyển scene/menu/popup).
**Không nên đổi:** Fragment đếm lúc nhặt; không reset khi qua level; Không sửa Player/Camera/Platform/Transition/Goal.

### S1-013 — Biome Presentation System
Mỗi Region có bản sắc hình ảnh: `RegionData` gom toàn bộ diện mạo 1 asset; `BiomeManager` áp nền+trời trong `Awake()` (không cần sửa Transition); `SkyManager` nướng `Gradient` thành `Texture2D` runtime; `SkyPlane` làm con Camera (không sửa Camera). `BiomeSession` nhớ region trước để bầu trời đổi màu mượt qua ranh giới scene, không ghi save.
**S1-013.1 (regression fix):** Bug "tiến trình reset" **không phải do S1-013** (xác nhận bằng `git diff`) — lỗi tiềm ẩn của S1-012: `restartChapterOnFirstLevel` dùng hàm ghi save chỉ-biết-đi-lên nên xoá fragment nhưng không xoá được cờ `restored`. Fix: tách `WriteChapterProgress` (lên) / `ResetChapterProgress` (xuống) — **không được gộp lại**.
**S1-013.2:** Tên chòm sao hiện đồng thời với nét vẽ, tan cùng lúc — `Reveal()`/`Dismiss()` tách khỏi `Play()` gộp, `ConstellationManager` giữ nhịp chung.

### S1-014 — Atmosphere & Audio Foundation
Nền tảng kỹ thuật cho không khí Region: `AudioManager` (crossfade 2 kênh, không Singleton/DontDestroyOnLoad), `RegionAtmosphereManager` (đọc Region qua `BiomeManager.Region`, không tự có field riêng), `ParticleController`. **Giới hạn có chủ đích:** crossfade thật xuyên ranh giới scene là bất khả thi nếu giữ luật No Singleton (mỗi Region là 1 scene, `LoadScene` phá huỷ mọi thứ) — giải pháp là scene cũ fade nhạc về 0 trước khi bị huỷ, scene mới fade in từ im lặng.

**S1-014B — Forgotten Forest BGM.** Gán `BGM_ForgottenForest.mp3` vào `RegionData.defaultMusic`. Không sửa script/scene — toàn bộ pipeline (auto-play, loop, fade, mixing) đã có sẵn từ S1-014.

### S1-014C — Forgotten Forest Atmosphere Complete
Region đầu tiên có **bản sắc trọn vẹn**. Từ đây trở đi, "không khí một Region" = 5 thành phần cố định: **BGM · Ambient Audio · Particles · Sky & Lighting · Constellation Title**. Toàn bộ hệ thống bên dưới đã tổng quát hoá, Region sau chỉ cần asset + gán field.

**Ambient Audio** — chim + lá xào xạc phát ngẫu nhiên, **không có gió** (bỏ có chủ đích: "yên tĩnh, huyền ảo" thay vì "hiệu ứng rừng chung chung").
- `AmbientLayerData`/`AmbientProfile` (SO) — layer `Loop` hoặc `RandomOneShot`, gom trong 1 asset per-Region.
- `LayeredAmbientPlayer` — layer Loop có `AudioSource` riêng; mọi layer `RandomOneShot` dùng CHUNG 1 `AudioSource.PlayOneShot` (overlap tự nhiên).
- `Ambient_Forest.asset`: Birds (delay 10–30s) · Morning Bird (delay 90–180s — **độ hiếm đến từ delay dài hơn**, không dùng trọng số ngẫu nhiên) · Leaves (delay 20–45s). Volume 0.15/0.15/0.05 = **trực tiếp là % nghe được cuối cùng**.
- `wind_soft.mp3` còn trên đĩa nhưng không còn tham chiếu nào — không xoá tài nguyên người dùng đã import.

**Particles** — lá rơi + bụi nắng, **parallax 3 lớp**: "thế giới di chuyển quanh người chơi", không phải "cả khu rừng bám theo người chơi".
- `AmbientParticleField` — hạt 2D **tự pool bằng code, KHÔNG dùng Unity ParticleSystem (Shuriken)**: dự án không có Editor để dựng/kiểm tra asset Shuriken bằng tay. 1 vòng lặp dịch chuyển N `SpriteRenderer` tạo sẵn, không `Instantiate`/`Destroy` lúc chạy.
- `ParallaxLayer` — đọc `Camera.main`, tự ghi vị trí mình bằng `parallaxFactor` kiểu `Vector2` (tách X/Y), tự neo lại khi lệch xa.
- `Particle_SunDust` (−60) · `Particle_BackgroundLeaves` (−10) · `Particle_ForegroundLeaves` (**+1**, cố ý trước Player — an toàn nhờ chỉ 2 hạt).

**Bốn bài học kỹ thuật, rút ra qua nhiều vòng vá lỗi thật:**
1. **`Instantiate(prefab, position, rotation, parent)` ép world position tuyệt đối**, xoá offset prefab tự khai báo — từng khiến hạt sinh đúng tại vị trí Camera (gần hơn Near Clip Plane, bị cắt sạch). Dùng `Instantiate(prefab)` → `SetParent(..., false)` → copy tường minh `localPosition`.
2. **`spritePixelsToUnits` phải khớp độ phân giải ảnh nguồn** — để mặc định 100 với ảnh 1024px khiến hạt to gần bằng màn hình.
3. **Ngẫu nhiên hoá vị trí phải áp dụng ở MỌI lần tái sinh, không chỉ lúc khởi động** — chỉ random Y một lần lúc `Awake()` rồi ép về đỉnh mãi khiến hạt dồn hết lên 1/3 trên màn hình.
4. **Parallax mạnh (factor thấp) chỉ an toàn trên trục camera đi chậm.** Camera leo dọc bám Y gần 1:1 mỗi cú nhảy — dùng chung 1 factor cho X/Y khiến hạt quét ngược 25–28% màn hình mỗi lần nhảy. Fix: `Vector2`, giữ Y sát 1 (0.94–0.98), chỉ hạ X. Kèm `executionOrder: 100` cho `ParallaxLayer` — hai `LateUpdate` phụ thuộc nhau mà cùng để 0 gây trễ 1 khung hình không xác định.

**Sky & Lighting** — bầu trời bình minh dịu, Global Light 2D từ tint xanh lá sang trắng ấm, intensity 0.9→1.0. **"Softer" đạt bằng tông ấm, không phải giảm sáng.** Lighting để riêng trong từng scene, **không đưa vào `RegionData`** — đưa lẻ 1 Region sẽ tạo 2 nguồn sự thật (migrate cả 5 scene là việc backlog).

**Constellation Title** — hệ tái sử dụng cho mọi Region:
- `RegionData.constellationTitle`. Forgotten Forest = `The Verdant Crown` → hiện `✦ The Verdant Crown ✦`. Region để trống thì không hiện, không lỗi.
- `RegionTitleUI` — fade in → giữ 2.5s → fade out, đúng **1 lần mỗi Region mỗi phiên** (`RegionTitleSession`, static, không ghi save — giống `BiomeSession`). `ShowOnce()` trả `void` chứ không `IEnumerator`, để không ai lỡ chặn gameplay.
- `LevelFlowManager` gọi **sau** khi trả quyền điều khiển, không yield.
- **Rủi ro chưa xác nhận: glyph ✦ (U+2726) nhiều khả năng ra ô vuông rỗng** vì Arial builtin thiếu. Chữa bằng 1 field Inspector (`{0}` hoặc `★ {0} ★`).
- Bugfix kèm theo: thẻ tên chòm sao lúc khôi phục từng đè lên chính hình chòm sao (cả hai neo giữa màn hình). Fix hai phía: label lên đỉnh + `shapeTopMargin` 0.18 đẩy hình xuống — áp cho cả 5 scene.

**Không nên đổi:** Không parent hạt trực tiếp vào Camera để tạo chiều sâu (triệt tiêu parallax). Không hạ `parallaxFactor.y` dưới ~0.9. Không hardcode tên chòm sao. Không cho `ShowOnce()` trả `IEnumerator`. Không gộp `LayeredAmbientPlayer` vào `AudioManager`.

### S1-015 — Cloud Garden Atmosphere *(ĐANG LÀM)*
Bản sắc thứ hai, cố tình **tương phản hoàn toàn** với Forgotten Forest: ấm/bám đất/rậm → sáng/thoáng/trôi nổi. Chứng minh khuôn mẫu Region tổng quát hoá thật sự — **không thêm manager nào**, `AudioManager`/`RegionTitleUI`/`RegionAtmosphereManager`/`ParticleController` không sửa một dòng.

**Đã làm xong:**
- **BGM** `BGM_CloudGarden.mp3` gán vào `RegionData.defaultMusic`. Luồng chuyển dùng nguyên hạ tầng S1-014: rời Region fade out 1s trước khi scene bị huỷ, vào Region fade in 2s. Chống phát trùng sẵn trong `FadeChannel.Play()`.
- **Constellation Title** `The Cloud Veil` → `✦ The Cloud Veil ✦`. `Canvas_RegionTitle` port sang `Level_02`, nối `LevelFlowManager.regionTitleUI`.
- **Sprite mây tự sinh** (`Assets/Particles/CloudGarden/`): **hợp (union) các đĩa tròn** + méo toạ độ bằng nhiễu mềm → đáy phẳng, đỉnh nhiều múi. **Đã thử tổng gaussian trước, ra vòm nhẵn không thành mây** — ghi lại để khỏi thử lại.
- **Bảng màu đã cân theo tương phản đo được** *(xem 8.4)* — thủ phạm chính của tình trạng "trắng xoá không đọc được" là **Global Light 2D `@1.15`**, không phải bảng màu: nó nhân lên mọi sprite và đẩy trời/mây/platform động vào vùng cắt trắng (luminance 0.99–1.00, tương phản 1.00:1). Hạ về `@1.00` rồi tách 3 tầng độ sáng.
  Kết quả: platform vs mây `1.17 → 2.76`, platform tĩnh vs động `1.18 → 1.33`, Player vs trời `1.08 → 1.57`, mây vs trời `1.01 → 1.41`, điểm sáng nhất màn hình `1.00 → 0.68`.
- **Sprite platform có shading** (`Assets/Sprites/CloudGarden/platform_cloudgarden.png`) — gradient dọc, đáy chìm bóng lệch lam. Ba ràng buộc bắt buộc: **256×256 + PPU 256** trùng `Square.png` builtin nên thay vào không đổi kích thước; **tràn viền alpha = 255** vì `BoxCollider2D` không đổi theo sprite; **chỉ gradient dọc, không bo góc** vì platform kéo giãn ngang tới scale 3. Màu tint chia cho 0.843 (nhân sáng trung bình của ảnh) để màu *hiển thị* đúng bằng con số đã duyệt.
- **Mây nằm trong WORLD, không bám camera** — `WorldAmbientField` (component mới): rải sprite có vị trí cố định, người chơi leo xuyên qua. **Không** làm con Camera, **không** `ParallaxLayer`, **không** bám Player.
  - **Hệ thứ HAI, cố ý không gộp với `AmbientParticleField`** — hai bài toán ngược nhau: `AmbientParticleField` = hạt sống *quanh người chơi*, có vòng đời, tái sinh liên tục (đúng cho lá Forest); `WorldAmbientField` = vật thể *đứng yên trong thế giới*, không vòng đời, ra khỏi khung hình là hết — đúng như platform.
  - **Không recycle**: hành trình chỉ ~45 unit nên rải thẳng một lần lúc `Awake()`. 60 `SpriteRenderer` tĩnh, Unity tự frustum-cull. **Tự bật/tắt `SetActive` mỗi khung hình CHẬM HƠN culling có sẵn** — cái bỏ qua theo khoảng cách là *phép tính trôi*, không phải việc render.
  - **Rải phân tầng** (chia dải Y thành `count` khoảng, mỗi khoảng 1 vật thể + xê dịch), **seed cố định + trả lại `Random.state`**, **trôi ngang là dao động SIN** (trôi đều thì sau vài phút lệch khỏi cột chơi).
  - `CloudField_SkyMotes` (−60, 40 vật thể) · `CloudField_Background` (−10, 14) · `CloudField_Foreground` (+1, 6), dải Y −8..40.

**Còn lại:**
- **Chưa có Ambient Audio** — Cloud Garden mới chỉ có BGM, chưa có `AmbientProfile` (chưa có asset âm thanh).
- **Chưa playtest.** Toàn bộ mới qua audit tĩnh.
- 3 prefab `Particle_SkyMotes`/`Particle_BackgroundClouds`/`Particle_ForegroundClouds` (bản bám camera cũ) **giờ không ai trỏ tới — asset chết**, chưa dọn.

**Cần biết:** mây cố định nghĩa là mỗi cú nhảy 3.5 unit làm bầu trời quét xuống ~35% màn hình — *nhiều hơn* trường hợp từng gây bug "nhức đầu" ở S1-014C. Vẫn đúng, vì mây giờ trượt **đúng nhịp** với platform (mắt đọc ra "tôi vừa nhảy") thay vì **lệch nhịp** ("cái nền bị trôi"). Nếu playtest thấy chóng mặt thì chữa bằng **deadzone dọc cho `CameraFollow2D`** (hiện `followY: 1`, không deadzone), **KHÔNG** quay lại parallax cao.

**Không nên đổi:** Không gắn `ParallaxLayer` trở lại `CloudField_*`. Không gộp `WorldAmbientField` vào `AmbientParticleField`. Không nâng Global Light quá 1.0 ở Cloud Garden. Không tô sáng riêng Player ở một region. Không thêm mép trong suốt vào sprite platform. Mây không được xoay.

---

## 6. CURRENT FEATURES

### Player & Camera
Di chuyển có gia tốc, nhảy với coyote time (0.15s) + jump buffer (0.15s), khoá di chuyển không tắt animation. Camera bám Player gần 1:1 theo Y (không deadzone dọc), deadzone ngang 2 unit.

### Platform, Collectibles, Save
5 loại platform (tĩnh/di chuyển/rơi/lò xo/một chiều). Star Fragment đếm tự động từ scene. Save JSON lưu ngay khi chạm Goal / nhặt sao.

### Constellation Restoration
Fragment cộng dồn toàn chapter (`★☆☆ 12/53` trên HUD) → chạm mốc → dừng ~1s → chòm sao + tên hiện đồng thời, chừa 18% màn hình trên cho tên → giữ 1s → cả hai tan cùng lúc → chơi tiếp. Mốc sau hoành tráng hơn (thời lượng + scale).

### Biome Presentation
Mỗi Region có nền + sky gradient + màu camera riêng trong `RegionData`. Bầu trời đổi màu mượt 1.5s qua ranh giới scene. Gradient nướng thành texture runtime — không cần asset ảnh.

### Atmosphere & Audio
Nhạc + ambient 2 kênh độc lập, fade in/out, chống phát trùng. **Chỉ Forgotten Forest có nội dung thật**: BGM + ambient (chim, lá — không gió) + 3 lớp particle parallax (bụi nắng, lá nền, lá tiền cảnh) + title chòm sao "The Verdant Crown". 4 Region còn lại có đủ hệ thống, field dữ liệu còn trống.

### Chapter Progress
Chapter suy ra từ `chapterId` của level. `Restart Chapter On First Level` (mặc định bật): vào region đầu chapter thì fragment về 0, chòm sao khôi phục lại từ đầu — chơi lại luôn trải nghiệm trọn vẹn.

---

## 7. CURRENT CONTENT

| # | Region | Scene | Star Fragment | Mốc chòm sao | Nội dung Atmosphere |
|---|---|---|---|---|---|
| 1 | **Forgotten Forest** | `SampleScene` | 10 | Lyra @ 12 | **Đầy đủ** — sky/light polish, BGM, ambient chim+lá, 3 particle parallax, title "The Verdant Crown" |
| 2 | **Cloud Garden** | `Level_02` | 10 | — | **Đang làm** — BGM, mây cố định trong world, bảng màu đã cân theo tương phản đo, sprite platform riêng, title "The Cloud Veil". **Chưa có ambient audio** |
| 3 | **Sky Ruins** | `Level_03` | 10 | Cassiopeia @ 30 | Sky gradient + màu nền có sẵn. Chưa có audio/particle/title |
| 4 | **Aurora Cliffs** | `Level_04` | 11 | — | Sky gradient + màu nền có sẵn. Chưa có audio/particle/title |
| 5 | **Moon Gate** | `Level_05` | 12 | Orion @ 53 | Sky gradient + màu nền có sẵn. Chưa có audio/particle/title |

Fragment cộng dồn: 10 → 20 → 30 → 41 → 53. Mốc chòm sao giả định nhặt đủ sao; bỏ sót thì mốc dời về sau.

**Bảng màu Sky đã khai cho 3 Region còn lại** (chưa polish như Forest/Cloud Garden): Sky Ruins xám lam→xanh đêm · Aurora Cliffs tím→tím đen · Moon Gate xanh đêm→đen.

---

## 8. DESIGN DECISIONS

**Không được tự ý đảo ngược bất kỳ mục nào dưới đây.**

### 8.1 Vision & thể loại
1. Không Combat/Enemy/Boss/Skill Tree/Shop. 2. Không tự thêm mechanic ngoài roadmap. 3. Trải nghiệm quan trọng hơn số lượng mechanic. 4. Gameplay chỉ: Platforming, Khám phá, Leo cao, Khôi phục bầu trời.

### 8.2 Cấu trúc hành trình
5. Region thay cho Level. 6. Leo liên tục, không cắt ngang. 7. Goal không phải mục tiêu cuối. 8. Transition tự động, điện ảnh. 9. Không Next Level/Retry trong luồng chính. 10. Không màn hình Level Complete. 11. Tên khu vực hiện tự động.

### 8.3 Star Fragment & Constellation
12. Star Fragment là ánh sáng, không phải điểm. 13. Không bắt buộc thu hết sao để qua region. 14. Constellation là meta progression dài hạn. 15. Fragment cộng dồn toàn chapter, đếm lúc nhặt (không phải lúc chạm Goal). 16. Khôi phục không chuyển scene/menu/popup. 17. Mốc sau hoành tráng hơn mốc trước. 18. UI tiến trình nhỏ gọn, không che gameplay. 19. Chơi lại từ đầu chapter thì xem lại được toàn bộ khoảnh khắc khôi phục. 20. **Tên chòm sao hiện đồng thời với nét vẽ, tan cùng lúc** — người chơi phải cảm thấy đang chứng kiến, không phải xem xong mới biết tên. 21. Tên + description lấy từ `ConstellationData`, không hardcode.

### 8.4 Biome, Atmosphere & Presentation *(S1-013 → S1-015)*
22. Mỗi Region phải có bản sắc hình ảnh + không khí + ambient + chòm sao riêng — không chỉ khác gameplay. 23. Chuyển Region: bầu trời đổi màu mượt, không nhảy màu. 24. Diện mạo Region nằm trong `RegionData`, không rải rác scene. 25. Biome/Atmosphere không được ảnh hưởng ngược lên gameplay. 26. Trạng thái phiên chơi (`BiomeSession`, `RegionTitleSession`) không ghi save. 27. Nhạc + ambient là 2 kênh độc lập, không cắt đột ngột, chống phát trùng. 28. **Không dùng Singleton/DontDestroyOnLoad để giải quyết crossfade xuyên scene** — giới hạn "fade out rồi fade in" là đánh đổi có chủ đích. 29. `RegionAtmosphereManager`/`ParallaxLayer` đọc Region/Camera qua tham chiếu gián tiếp (`BiomeManager.Region`, `Camera.main`) — không tự có field riêng dễ lệch. 30. **Parallax mạnh chỉ an toàn trên trục camera di chuyển chậm** — trục giật nhanh (Y trong game leo dọc) phải giữ factor sát 1.

### 8.5 Gameplay
31. Không chết khi rơi. 32. Không cơ chế kéo-thả kiểu ná. 33. Nhảy tha thứ lỗi bấm (coyote + buffer). 34. Khoá di chuyển không tắt animation.

### 8.6 Kiến trúc
35. Không Singleton. 36. Không hardcode số lượng/tên bất kỳ nội dung nào. 37. Single-Writer cho mọi tài nguyên chia sẻ (xem bảng 4.1). 38. Goal chỉ phát event. 39. Một class một trách nhiệm. 40. Mở rộng qua kế thừa/composition, không sửa lõi. 41. `[SerializeField]`, không `public` field. 42. Interface khi có nhiều cách hiện thực. 43. ScriptableObject cho dữ liệu designer, SaveData cho tiến trình — không trộn lẫn. 44. Không refactor/rename chủ động. 45. **Phép ghi save "đi lên" và "đi xuống" phải là hai hàm riêng.** 46. Class nào giữ nhịp thì sở hữu con số thời gian dùng chung, không để mỗi bên tự tính rồi lệch. 47. **`Instantiate(prefab, position, rotation, parent)` không được dùng khi parent không đứng ở gốc toạ độ có ý nghĩa** — ép world position tuyệt đối, xoá offset prefab tự khai. 48. **Hai `LateUpdate`/`Update` phụ thuộc thứ tự lẫn nhau bắt buộc phải đặt `executionOrder` tường minh.**

---

## 9. KNOWN ISSUES

### 9.1 Nội dung còn thiếu (kiến trúc đã xong, chỉ thiếu asset/data)

| Hạng mục | Tình trạng |
|---|---|
| **Ambient Audio cho Cloud Garden** | Chưa có asset âm thanh. Hạ tầng (`AmbientProfile` + `LayeredAmbientPlayer`) đã sẵn, chỉ cần file + 1 asset |
| **Audio/Ambient/Particle/Title cho Sky Ruins, Aurora Cliffs, Moon Gate** | **Chưa có gì.** Toàn bộ hệ thống (`AudioManager`, `LayeredAmbientPlayer`, `AmbientParticleField`, `ParallaxLayer`, `RegionTitleUI`) đã tổng quát hoá xong ở Forgotten Forest, chỉ cần tạo asset + gán field cho 4 Region còn lại, không cần sửa code. Cloud Garden (S1-015) đã chứng minh chỉ cần asset + gán field, không cần sửa code. Đây là khoảng cách lớn nhất hiện tại. |
| **Art chòm sao / Icon / particle khôi phục / âm thanh khôi phục** | Placeholder hình học đơn giản. Ô Inspector đã có, chưa có asset thật |
| **Camera nhìn lên bầu trời lúc khôi phục** | TODO — hiện thay bằng lớp phủ tối toàn màn hình |
| **`ConstellationReward`** | Đã gỡ khỏi data, chưa có hệ thống phần thưởng |
| **Cloud Density / `parallaxFactor` của `BackgroundLayerData`** | Có field, chưa component nào đọc (khác `ParallaxLayer` của hạt, đã dùng) |
| **Glyph ✦ (U+2726) trong `RegionTitleUI`** | Rủi ro cao ra ô vuông rỗng — Arial builtin không có. Chưa xác nhận trong Editor |
| **VFX tiếp đất/lò xo, Animation Player, Object pooling thật** | Placeholder / chưa tối ưu |
| **`backgroundLayers[0].sprite` của cả 5 Region** | GUID `311925a002…` **không tồn tại trong project** — tham chiếu treo có từ S1-013. Unity đọc thành null → `BackgroundManager` giữ nguyên sprite sẵn trong scene và chỉ áp màu, nên không crash, nhưng ô Inspector đang trỏ vào hư không |
| **3 prefab `Particle_*Clouds`/`Particle_SkyMotes`** | Bản mây bám camera cũ, giờ không asset nào trỏ tới — asset chết, chưa dọn |
| **`Assets/Particles/Forest/leaf_01.png`** | `spritePixelsToUnits` = 100 trong khi leaf_02/03 = 1024 → lá này to gấp ~10 lần hai lá kia. Do Unity tự sinh lại meta, xoá mất giá trị đặt tay ở S1-014C |

### 9.2 Feature chưa hoàn thiện
Chapter Complete chưa có màn kết chương (chỉ lưu `completed: true`). Moon Gate là ngõ cụt. Level Select cô lập, chưa có Main Menu. `LevelCompleteUI` nghỉ hưu nhưng chưa xoá hẳn.

### 9.3 Rủi ro thiết kế cần theo dõi
- **Mốc 3 = 53/53 = 100% tổng fragment** — bỏ sót 1 sao là Orion không bao giờ khôi phục. Hệ quả trực tiếp của spec, không phải lỗi.
- **Chưa playtest cân bằng độ khó** cho cả 5 region.
- **Không có "tiến trình vĩnh viễn" cho Constellation** — chơi lại từ đầu chapter xoá sạch tiến trình cũ, mâu thuẫn với tầm nhìn "gieo lại các vì sao" nếu sau này làm sky gallery.
- **Crossfade audio không thật sự chồng lớp qua ranh giới scene** — giới hạn có chủ đích (mục 8.4 #28), không phải bug.
- **S1-014C và S1-015 chưa playtest trong Unity** — chỉ qua audit tĩnh (fileID, tham chiếu, GUID font, tên field khớp C#↔scene). S1-012 và bản vá S1-013.1 đã xác nhận qua save file thật; S1-013 → S1-015 thì chưa.
- **Field thiếu trong prefab YAML dựa vào giá trị khởi tạo trong C#** — `horizontalSpeed*` và `randomizeInitialRotation` không ghi trong 3 prefab Forest, tin rằng Unity lấy default của class (0 và `true`). Đúng theo hiểu biết hiện tại nhưng **chưa xác nhận trong Editor**; Unity sẽ ghi đủ field khi prefab được lưu lại lần tới.

### 9.4 Bài học kỹ thuật đã ghim (áp dụng khi viết code mới)
- Font builtin dùng GUID `0000000000000000e000000000000000`, fileID `10102`. Text legacy cần `CanvasRenderer`.
- Phân biệt rõ fileID GameObject vs Component khi viết YAML tay; `m_Children`/`m_Father` phải đối ứng.
- Đổi API phải grep toàn bộ call site, kể cả code đã nghỉ hưu.
- **Trước khi nhận một sprint có regression, phải `git diff` để xác định phạm vi thật** — trùng thời điểm phát hiện ≠ trùng nguyên nhân.
- **Mọi thay đổi chạm tiến trình phải test hai lượt chơi liên tiếp**, đối chiếu trực tiếp file save.
- `Instantiate(prefab, position, rotation, parent)` ép world position tuyệt đối — chỉ an toàn khi parent ở gốc toạ độ có ý nghĩa.
- Hai `LateUpdate` phụ thuộc nhau (vd Camera → Parallax) phải đặt `executionOrder` tường minh, không để mặc định 0 cả hai.
- Parallax factor phải tách theo trục — trục camera giật nhanh giữ sát 1.
- `spritePixelsToUnits` phải khớp độ phân giải ảnh nguồn, đừng để mặc định rồi bù bằng scale cực nhỏ.
- Sinh sprite hữu cơ (mây, khói): dùng **hợp các hình cơ bản**, đừng dùng tổng gaussian — tổng sẽ tan thành một khối nhẵn, mất hết đường viền.
- Thêm khả năng mới vào hệ dùng chung thì **giá trị mặc định phải bằng hành vi cũ**, để Region đã xong không đổi một chút nào.
- Vật trang trí thuộc về THẾ GIỚI (mây, tàn tích nền) phải đặt cố định trong world; chỉ hạt sống *quanh người chơi* mới được bám camera. Bám camera = người chơi vác bầu trời đi theo, leo mãi không thấy tiến.
- Đổi màu vì lý do **đọc được** thì phải đo luminance + tỉ lệ tương phản, tính cả `Light2D` và alpha blend — đừng chỉnh bằng cảm giác.
- Sprite thay cho `Square.png` builtin phải **256×256, PPU 256, alpha tràn viền** — lệch cái nào cũng làm hình lệch khỏi `BoxCollider2D`.
- Khi người dùng chỉ mô tả chung chung ("tên chòm sao", "hạt kỳ kỳ"), **phải xác định đúng component trước khi sửa** nếu project có nhiều thứ cùng tên gọi — hỏi lại hoặc xin ảnh/video thay vì đoán.

---

## 10. ROADMAP

### 10.1 Đã hoàn thành
`S1-001` → … → `S1-014` → `S1-014B` (Forgotten Forest BGM) → **`S1-014C`** (Forgotten Forest Atmosphere Complete)

### 10.2 Đang làm
**`S1-015` — Cloud Garden Atmosphere.** Xong: BGM, mây world-space, bảng màu readability, sprite platform, constellation title. Còn: **ambient audio**, và **playtest toàn bộ**.

> Lịch sử đặt tên đã đổi số nhiều lần do trùng tên giữa các lượt yêu cầu — **mục 10.1 này luôn là nguồn sự thật duy nhất**, không tin vào số sprint nhắc tới trong hội thoại cũ.

### 10.3 Tiếp theo (đã lên kế hoạch)

| Sprint | Nội dung |
|---|---|
| **S1-016 – Sky Ruins Atmosphere** | Tương tự, bản sắc "tàn tích cổ, cô độc". Có Cassiopeia @ mốc 30. |
| **S1-017 – Aurora Cliffs Atmosphere** | Tương tự, bản sắc "huyền bí, ánh sáng tím". |
| **S1-018 – Moon Gate Atmosphere** | Tương tự, bản sắc "tĩnh lặng, không gian". Có Orion @ mốc 53 — chòm sao cuối chapter. |
| **S1-018.5 – Region Transition Polish** | Sau khi cả 5 Region có atmosphere đầy đủ: rà lại cảm giác chuyển tiếp giữa các Region liên tiếp (âm thanh, ánh sáng, particle) có mượt không, có "va" nhau không. |
| **S1-019 – Journey Cinematic & World Feeling** | Nhìn lại tổng thể hành trình sau khi có đủ 5 bản sắc — có thể là camera nhìn lên trời lúc khôi phục (đã treo từ 9.1), hoặc cinematic nhỏ khi hoàn thành chapter. |

### 10.4 Backlog (chưa xếp lịch)
Chapter Complete & Chapter 2, Main Menu, hệ thống phần thưởng chòm sao, tiến trình Constellation vĩnh viễn (sky gallery), playtest cân bằng, animation Player.

### 10.5 Ý tưởng chưa duyệt *(không tự làm)*
`LoadSceneAsync`/additive scene (cũng mở khoá crossfade audio thật xuyên scene), object pooling thật, TextMeshPro (giải quyết luôn glyph ✦/✨ thiếu), dọn `CS0618`, đưa Global Light 2D vào `RegionData` (cần migrate cả 5 scene cùng lúc).

---

## 11. CODING GUIDELINES

### 11.1 Quy tắc bắt buộc
Không phá code đang chạy. Không refactor/rename chủ động. Ưu tiên khả năng mở rộng hơn viết nhanh. Một class một trách nhiệm. Không hardcode — luôn `[SerializeField]`. Chỉ đụng đúng phần được hỏi. Chọn cách dễ bảo trì nhất cho mobile, không phải nhanh nhất. `git diff` trước khi nhận sprint có regression. Test tiến trình hai lượt chơi liên tiếp.

### 11.2 Quy ước code
Namespace theo thư mục `StarSower.<Module>`. Comment tiếng Việt giải thích **tại sao**, không phải **cái gì**. Coroutine cho trình tự có thời gian. `[Header]`/`[Tooltip]` cho field designer cần chỉnh. Event dùng `System.Action`, tên `On...`. ScriptableObject cho dữ liệu designer.

### 11.3 Định dạng trả lời cho mỗi Story *(bắt buộc)*
1. Thiết kế — class nào, trách nhiệm gì. 2. Đánh giá rủi ro. 3. Triển khai — code thật. 4. Kiểm thử — bước test thủ công (project chưa có test tự động). 5. Đề xuất Story tiếp theo.
Không dừng chờ duyệt giữa chừng. Chỉ hỏi lại khi yêu cầu mơ hồ tới mức nguy cơ chọn sai kiến trúc.

### 11.4 Sửa scene bằng YAML tay (không có Editor)
`fileID` duy nhất; script GUID `aaaa…NN`, asset/prefab `bbbb…NN`. Phân biệt fileID GameObject vs Component. `m_Children`/`m_Father` đối ứng. Font builtin GUID `0000000000000000e000000000000000`, fileID `10102`. Đăng ký root mới vào `SceneRoots.m_Roots`. Sau mỗi lần sửa: audit trùng fileID, tham chiếu treo, đối ứng cha-con, khớp kiểu component, tên field C#↔YAML.

---

## 12. PROJECT STATUS

### Đánh giá: **VERTICAL SLICE, đang tiến vào giai đoạn Content Pass**

**Đã đạt:**
- Vòng lặp gameplay cốt lõi hoàn chỉnh end-to-end.
- Kiến trúc ổn định qua 17+ sprint, ranh giới trách nhiệm rõ.
- Save đã xác nhận qua file thật (S1-012, S1-013.1).
- **Forgotten Forest là bản mẫu hoàn chỉnh** cho "1 Region đầy đủ bản sắc" — BGM, ambient, particle parallax, sky & lighting, constellation title (S1-014C).
- **Cloud Garden (S1-015) đã xác nhận khuôn mẫu tái sử dụng được**: dựng gần trọn một Region thứ hai mà **không thêm manager nào**. Hai component mới sinh ra là để giải bài toán MỚI (`WorldAmbientField` cho vật trang trí world-space), không phải để vá hệ cũ.

**Chưa đạt:**
- **S1-015 chưa xong**: Cloud Garden còn thiếu ambient audio.
- 3/5 Region (Sky Ruins, Aurora Cliffs, Moon Gate) vẫn chỉ có sky gradient — công việc của S1-016 → S1-018.
- **Chưa có sprint nào từ S1-013 trở đi được playtest trong Unity** — mới chỉ qua audit tĩnh.
- Chưa playtest cân bằng độ khó.
- Chưa có Main Menu, Chapter 2, animation Player.

---

## 13. TÓM TẮT NHANH CHO PHIÊN LÀM VIỆC MỚI

1. Starsower là **một hành trình xúc cảm leo lên bầu trời**, không phải game nhiều mechanic. Đọc mục 2 và 8 trước khi đề xuất thay đổi.
2. Không Combat/Enemy/Boss. Không tự thêm mechanic ngoài roadmap.
3. **`S1-014C` đã xong, `S1-015` ĐANG LÀM.** Forgotten Forest là bản mẫu đầy đủ 5 thành phần. Cloud Garden đã có gần đủ nhưng **còn thiếu ambient audio** — chưa được tính là xong. **3 Region còn lại trống trơn** — việc của S1-016 → S1-018 (mục 10.3).
4. Hệ thống ổn định, không sửa khi không được yêu cầu: Player, Camera, Platform, Transition, Goal, Biome, Atmosphere.
5. `ProgressManager` là nơi duy nhất ghi save. `AudioManager` là nơi duy nhất ghi `AudioSource`. `ParallaxLayer` là nơi duy nhất ghi vị trí hạt parallax.
6. Trả lời mỗi story theo 5 phần ở mục 11.3.
7. Khi nghi có regression: `git diff` với commit S1-012 (`1add22d`) trước, đừng quy lỗi cho sprint mới nhất chỉ vì trùng thời điểm.
8. Test tiến trình phải chạy hai lượt chơi liên tiếp.
9. **Chưa playtest trong Unity**: toàn bộ chuỗi S1-013 → S1-015. Rủi ro cụ thể: glyph ✦ (U+2726) trong title nhiều khả năng ra ô vuông rỗng (giờ ảnh hưởng 2 Region).
10. Bài học kỹ thuật ghim lại (mục 9.4): `Instantiate` với parent tuỳ ý phải copy tường minh local transform; `executionOrder` tường minh cho các `LateUpdate` phụ thuộc nhau; `parallaxFactor` phải tách trục theo tốc độ camera; `spritePixelsToUnits` phải khớp ảnh nguồn; mở rộng hệ dùng chung thì default phải bằng hành vi cũ.
11. Khi người dùng mô tả lỗi chung chung mà project có nhiều thứ trùng tên gọi (vd "tên chòm sao" = `RegionTitleUI` hay `ConstellationNameCard`?), phải hỏi lại hoặc xin ảnh/video trước khi sửa.
12. Không dùng Singleton/DontDestroyOnLoad để "sửa" giới hạn crossfade audio xuyên scene — đánh đổi kiến trúc có chủ đích.
13. Tên sprint từng đổi số nhiều lần trong lịch sử tài liệu — **luôn tin mục 10.1 làm nguồn sự thật**.
