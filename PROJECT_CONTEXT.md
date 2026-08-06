# PROJECT_CONTEXT.md — Starsower

> Nguồn tham chiếu duy nhất khi tiếp tục phát triển sau compact.
>
> ⚠️ **Dự án đã sang giai đoạn mới. Đọc §13 (S3 — StarSower Rebirth) TRƯỚC.** Từ §1 tới §12 mô tả
> giai đoạn S1–S2 (platformer leo dọc, màn hình đứng) — phần lớn vẫn đúng về kiến trúc và bài học,
> nhưng mọi thứ liên quan tới **hướng màn hình, bố cục màn chơi và bộ kỹ năng** đã bị thay ở S3.
>
> Cập nhật đến hết **S3-004 (xong)**.
>
> **Ngày cập nhật:** 2026-07-31

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

---

## 2. GAME VISION

**Đây là phần không được tự ý thay đổi.** Mọi quyết định thiết kế sau này phải kiểm tra lại với danh sách dưới đây.

> ⚠️ **ĐẢO HƯỚNG Ở S3-000 (2026-07-31), theo quyết định của người dùng.** Game chuyển từ **platformer leo dọc màn hình đứng** sang **platformer khám phá màn hình ngang**, lấy cảm hứng Ori / Hollow Knight / Celeste. Các gạch đầu dòng dưới đây về "leo dọc" **không còn hiệu lực**; phần khí chất (mềm, huyền ảo, mơ màng) và phần khôi phục chòm sao thì **giữ nguyên**.
>
> Đừng "sửa lại cho đúng luật cũ" — đây là đổi hướng có chủ đích, không phải sai sót.

- ~~Starsower là **một hành trình duy nhất từ mặt đất lên đỉnh bầu trời**~~ → **hành trình khám phá theo chiều ngang**, mỗi khu vực là một bản đồ có đường chính, đường phụ, khu ẩn và khu giải đố.
- **Level chỉ là các Region của cùng một hành trình** — không phải các màn chơi độc lập.
- **Không tạo cảm giác "qua màn"**. Không có màn hình "Level Complete", không bảng điểm, không nút bấm giữa các region.
- **Goal chỉ là điểm chuyển tiếp** — nghĩa là "bạn đã leo tới khu vực kế tiếp", không phải mục tiêu cuối của game.
- **Auto Transition**: chạm Goal là tự động chuyển sang region mới, liền mạch, mang tính điện ảnh.
- **Không có nút "Next Level"**. Không có nút "Retry" trong luồng chính.
- **Không Combat. Không Enemy. Không Boss.**
- Gameplay tập trung vào đúng 4 thứ: **Platforming · Khám phá · ~~Leo cao~~ → Di chuyển linh hoạt (nhảy đôi, bám tường, lướt, bay lượn) · Khôi phục bầu trời**.
- **Star Fragment không phải điểm số.** Mỗi mảnh là một mảnh ánh sáng giúp bầu trời sống lại. Người chơi phải cảm thấy mình đang *"gieo lại các vì sao"*.
- Người chơi phải cảm thấy **bầu trời đang dần sống lại nhờ hành trình của mình**.
- **Đây là hành trình xúc cảm, không phải mechanic phức tạp.** Bản sắc mỗi Region đến từ 5 thứ: **không khí riêng · nhạc riêng · bản sắc hình ảnh · tên chòm sao · mạch cảm xúc đáng nhớ**.
- **Mechanic mới là thứ yếu** so với việc dựng một thế giới liền mạch và đáng nhớ.
- **Gameplay đọc được quan trọng hơn hiệu ứng hình ảnh.** Không khí phải làm gameplay rõ hơn, tuyệt đối không được che nó.

---

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

> ⚠️ **Trình tự trên đã bị thay ở S2-006.** Cảnh khôi phục giờ chiếu sau khi hoàn thành màn, trong không gian thế giới, Hero chạy vào khung rồi tự bắn sao vẽ chòm — xem **6.9**. Nhánh UI cũ vẫn còn nguyên làm dự phòng.

---

---

## 4. KIẾN TRÚC

### 4.1 Nguyên tắc nền

- **Hướng phụ thuộc:** `Core ← Systems ← Managers/Level ← UI`.
- **Single-Writer** — mỗi tài nguyên chỉ MỘT class được ghi:

  | Đối tượng | Chỉ được ghi bởi |
  |---|---|
  | `Rigidbody2D` Player | `PlayerMotor` |
  | `transform.position` Camera | `CameraFollow2D` (trừ khi `LevelFlowManager`/`JourneyCinematic` chủ động tắt component để tự lái) |
  | File save | `ProgressManager` |
  | `SpriteRenderer` nền | `BackgroundManager` |
  | Sky Plane + `Camera.backgroundColor` | `SkyManager` |
  | `AudioSource` | `AudioManager` |
  | Hạt của Region | `ParticleController` |
  | Vị trí lớp parallax | `ParallaxLayer` |
  | Vật trang trí world | `WorldAmbientField` |
  | `localScale` sao chòm sao | `ConstellationScreen` — coroutine **hoặc** `Update()`, không đồng thời (cờ `isAnimating`) |

- **Không Singleton, không `DontDestroyOnLoad`.** Hệ quả chấp nhận: crossfade nhạc xuyên scene là không thể; chỉ crossfade thật trong cùng một scene (dùng ở `JourneyCinematic`).
- **Event hub:** `GameEvents` (static).
- **Interface để thay thế:** `IInputProvider`, `IGroundDetector`, `ISurfaceProvider`, `IGroundSurface`, `ILaunchable`, `ITransitionEffect`, `ICameraTarget/Shake/Zoom`, `IPlatformPool`.

### 4.2 Script theo namespace *(chỉ ghi thay đổi so với bản trước)*

| Namespace | Bổ sung / thay đổi |
|---|---|
| `StarSower.Core` | **+`IGroundSurface`** (ma sát + độ trôi của bề mặt), **+`ISurfaceProvider`** (tách khỏi `IGroundDetector` để mọi detector cũ không phải sửa) |
| `StarSower.Player` | `PlayerMotor` +`SetSurface(friction, drift)`; `GroundChecker` implement `ISurfaceProvider` |
| `StarSower.Platform` | **+`IcePlatform`**, **+`MoonPlatform`**, **+`MoonlightRevealPlatform`**. `FallingPlatform` không còn dùng ở Level_05 |
| `StarSower.Cinematic` *(mới)* | **`JourneyCinematic`** — cảnh kết Chapter 1 |
| `StarSower.Constellations` | **+`ConstellationScreen`** (hệ mới). `ConstellationManager`/`RestoreSequence`/`NameCard` **đã tắt ở cả 5 scene** |
| `StarSower.UI` | **+`SafeAreaFitter`**, **+`AspectEnvelopeFitter`**; `OnScreenJoystick`/`TouchButton` thêm theo dõi `pointerId` |
| `StarSower.Managers` | `GameOverManager` **đã bật lại**, dùng Kill Floor thay `CameraFollowY` |
| `Assets/Editor` *(mới, không vào build)* | `PlayModeStartSceneMenu` (chọn scene khi bấm Play), `SaveToolsMenu` (xoá/reset save) |
| `StarSower.Player` *(S2-002)* | **+`PlayerAnimationController`** (cầu nối MỘT CHIỀU trạng thái → Animator; `SetScriptedMotion()` cho cảnh diễn), **+`GroundShadowController`** |
| `StarSower.Camera` *(S2-004)* | **+`CameraAspectFitter`** — tính `orthographicSize` theo tỉ lệ máy để bề ngang sân luôn vừa khung |
| `StarSower.FX` *(S2-005, mới)* | `StarFXPool` · `PooledStarFX` · `StarFXType` · `StarCollectEffect` · `StarFlyAnimator` · `StarIdleAnimator` · `PocketFXController` · `IStarFlightListener` |
| `StarSower.Constellations` *(S2-006)* | **+`ConstellationCinematic`** (điều phối cảnh) · **+`ConstellationNode`** · **+`ConstellationLineDrawer`** · **+`ConstellationSkyBackdrop`**. 5 class cũ dời sang `Constellation/Legacy/`, GameObject `ConstellationSystem` đặt `m_IsActive: 0` ở cả 5 scene |

### 4.3 Scenes

| Scene | Region | Cao | Gap | Cú phải giữ nút |
|---|---|---|---|---|
| `SampleScene` | Forgotten Forest | 39.4 | 2.4–3.4 | 0/14 |
| `Level_02` | Cloud Garden | 54.7 | 3.0–4.0 | 6/16 |
| `Level_03` | Sky Ruins | 61.0 | 2.9–4.5 | 8/17 |
| `Level_04` | Aurora Cliffs | 86.0 | 3.4–4.9 | 13/16 |
| `Level_05` | Moon Gate | 102.4 | 3.6–5.2 | 17/18 |

**81/81 cú nhảy đã verify là tới được.** Camera đồng nhất tuyệt đối 5/5 scene (ortho 5, deadzone 2, smoothX 0.25, smoothY 0.12).

### 4.4 Bộ số tham chiếu nhanh

| Component | Giá trị |
|---|---|
| `PlayerMotor` | moveSpeed 5 · jumpForce 12 · fallMultiplier 2.5 · airControl 0.8 → **apex 7.34**, short-hop 3.67, tầm ngang ~8 |
| `SpringPlatform` | launchVelocity 18/20 → apex 16.5/20.4 |
| `IcePlatform` | friction 0.25 (dừng lâu gấp 4) · driftSpeed 1.2 (buông tay là trôi, không đứng im được) |
| `MoonPlatform` | activationRadius 9 · hiddenAlpha 0.55 · maxReveal 1.0 (0.6/0.45 cho 2 cái ẩn cuối màn) · warning 0.8s · vanish 4s |
| `MoonlightRevealPlatform` | detectionRadius 7 · hintAlpha 0.12/0.07 · colliderThreshold 0.35 |
| `GameOverManager` | **killFloorY −12** · reloadDelay 1.5 |
| `JourneyCinematic` | zoom 2.5s → giữ 4s → về 2s · ortho 5→44 · framingBias 0.75 |
| `ConstellationScreen` | fadeIn 0.8 · starPop 0.55 · lineDraw 0.5 · hold 1.6 · fadeOut 0.7 · pulse ±6% *(nhánh UI dự phòng)* |
| `CameraAspectFitter` *(S2-004)* | playableWidth 5.2 · minOrthographicSize 5 · `ortho = Max(5.2 / (2×aspect), 5)` |
| `Hero` prefab *(S2-002)* | root scale 0.75 (collider 1×1 → 0.75 world) · `Visual` scale 1.25, local `(0, −0.5)` · PPU 225 · pivot (0.5, 0.125) |
| `ConstellationCinematic` *(S2-006)* | fadeIn 0.8 · heroRun 1.4 (margin 1.2, đứng ở 0.78 nửa khung tính từ tâm xuống) · pocketGlow 0.5 · launchDelay 0.55 · flight 1.1 · lineDraw 1.4 · glow 1.2 · nameReveal 0.5 · hold 1.4 · fadeOut 1.3 · cameraRise 0.5 · finalZoom 1.35 · skipGrace 0.6 |

### 4.5 Save

`~/Library/Application Support/DefaultCompany/StarSower/starsower_save.json`

Mới ở S1-020A/B: `ConstellationSaveData.animationPlayed` (tách khỏi `restored` — một cái quyết định *vẽ*, cái kia quyết định *có diễn lại không*). `MarkConstellationUnlocked()` ghi **cộng dồn theo từng id**, không bao giờ ghi đè chòm khác.

Mới ở S2-006: **`ConstellationSaveData.nodesRestored`** — SỐ ngôi sao đã khôi phục trong chòm, để bầu trời lành **dần** qua nhiều lượt chơi. Trường cộng thêm nên save cũ đọc vào mặc định 0, **tương thích ngược**. Luật: `ceil(tổngSao × sốSaoĐạt / 3)`, kẹp trong `[0, tổng]`, **đơn điệu** — `SetConstellationNodes()` chỉ nhận giá trị lớn hơn, chơi lại kém hơn không làm tắt bớt sao. `GetConstellationNodes()` kẹp lại lúc đọc phòng khi số sao của chòm bị đổi sau này.

---

## 5. CƠ CHẾ GAMEPLAY ĐÃ CÓ

| Cơ chế | Khu vực | Ghi chú |
|---|---|---|
| Platforming chuẩn | tất cả | coyote 0.15 + jump buffer 0.15 |
| Moving Platform | CG, SR, AC, MG | quét ngang ±moveDistance, đặt `x=0` để không ra ngoài màn dọc |
| Falling Platform | SR, AC | chạm là rơi sau `fallDelay` |
| Spring Platform | AC, MG | bật cao 16.5–20.4 |
| **Ice Platform** *(S1-017)* | Aurora Cliffs | giảm **deceleration**, KHÔNG giảm acceleration → nhảy vẫn nhạy; thêm trôi chủ động nên không đứng im được. Crystal island không trơn |
| **Moon Platform** *(S1-018)* | Moon Gate | 4 trạng thái: Hidden (xa, mờ) → Activated (gần, sáng) → Vanishing (rời đi mới tan) → Restore |
| **Moonlight Reveal** *(S1-018)* | Moon Gate | vô hình khi xa, **collider tắt**; hiện + bật collider khi lại gần. 2 cái đặt lệch trục 1.6 để không chặn đường bay lò xo |
| Chết khi rơi | tất cả | Kill Floor −12 |

---

## 6. NỘI DUNG ĐÃ HOÀN THÀNH

### 6.1 Region — **5/5 xong**

Mỗi khu có: bố cục riêng · art platform riêng · 2 lớp nền parallax world-space · sky gradient · BGM · constellation title · hạt không khí.

### 6.2 Audio — **6/6 BGM đã tích hợp**

`BGM_ForgottenForest` · `BGM_CloudGarden` · `BGM_SkyRuins` · `BGM_AuroraCliffs` · `BGM_MoonGate` · `BGM_JourneyCinematic`

Import settings đã sửa cho iOS *(S1-020B)*: `loadType: CompressedInMemory` · `preloadAudioData: 1` · `loadInBackground: 1` · `3D: 0`. Và `ProjectSettings.muteOtherAudioSources: 1` để **bỏ qua công tắc gạt im lặng của iPhone**.

**Ambient: chỉ Forgotten Forest có** (`Ambient_Forest.asset`). 4 khu còn lại chưa có file.

### 6.3 Environment art — xong cho cả 5 khu

`Assets/Environment/<Region>/` gồm `Backgrounds/` (far+near) và `Platforms/` (4–5 biến thể). Toàn bộ đã crop sát viền, PPU = bề rộng ảnh, và **tách sprite sang GameObject con `Visual`** để giữ đúng tỉ lệ ảnh mà không đụng collider.

### 6.4 Constellation — dữ liệu chòm sao *(S1-020A/B)*

> Phần **trình diễn** của mục này đã được thay ở S2-006 (xem **6.9**). Phần **dữ liệu + ánh xạ** dưới đây vẫn đúng nguyên.

**Một level = một chòm sao riêng**, mở trọn vẹn, không nhỏ giọt từng ngôi.

| Màn | Chòm sao | Sao |
|---|---|---|
| Forgotten Forest | Cassiopeia | 5 |
| Cloud Garden | Orion | 7 |
| Sky Ruins | **Cygnus** *(mới)* | 6 |
| Aurora Cliffs | **Draco** *(mới)* | 7 |
| Moon Gate | Lyra | 5 |

Ánh xạ theo **chỉ số** trong `LevelDatabase` ↔ `ChapterData.constellations`, không hardcode id. Sao + nét nối dựng lúc chạy từ toạ độ chuẩn hoá 0..1 nên tự co giãn mọi màn hình. Hoạt ảnh chỉ diễn **một lần**, tên chòm sao hiện trên cùng.

### 6.5 Journey Cinematic *(S1-019)* — xong

Chạy khi `!HasNextLevel` (không hardcode tên scene). Camera zoom 5→44 kèm pan, easing SmoothStep. Phông hành trình = 5 ảnh `background_far` xếp dọc chồng mép, `sortingOrder −50`. Crossfade thật sang `BGM_JourneyCinematic`. Ẩn HUD + MobileInput trong lúc chiếu.

**Thứ tự cuối cùng** *(S1-020B)*: hoàn thành Moon Gate → lưu → **Chòm sao** → **Cảnh kết**.

### 6.6 Character — **xong** *(S2-002)*

Ô vuông hồng placeholder **đã biến mất khỏi cả 5 scene**. Player giờ là instance của `Assets/Prefabs/Player/Hero.prefab`.

**Nguồn art là ảnh trình bày, không phải sprite sheet.** 4/5 file không có kênh alpha — nền trong suốt bị nướng thành hoa văn ca-rô ngay trong RGB. Cứu bằng flood-fill từ biên (giữ răng cưa, không premultiply). **7 file gốc giữ nguyên từng byte**, bản dùng được nằm ở `Sheets/`.

Frame không nằm trên lưới đều: Jump xếp theo vòng cung, Fall lệch chân tới 140 px, tỉ lệ nhân vật khác nhau giữa các sheet. Chuẩn hoá bằng **diện tích bóng phần thân** (đã loại áo choàng), kiểm chéo bằng khoảng cách hông→mặt đất — hai phép đo độc lập lệch nhau **0.2–0.6%**.

| Sheet | Frame |
|---|---|
| `Hero_Idle` | 6 |
| `Hero_Run` | 8 |
| `Hero_Jump` | 6 |
| `Hero_Fall` | 4 |
| `Hero_Landing` | 5 |

**29 sprite**, PPU 225, ô 384, **một pivot duy nhất (0.5, 0.125)**, đường chân y=336, trôi ngang ≤ 0.004 unit giữa các frame.

`Hero_Animator.controller` 5 state. `PlayerAnimationController` chỉ **đọc** `PlayerMotor` + `IGroundDetector` rồi ghi tham số Animator — xoá đi thì gameplay chạy y nguyên, chỉ mất phần hình. Lật hướng bằng `SpriteRenderer.flipX` trên child `Visual`, **không bao giờ lật scale root** (root mang Rigidbody2D + Collider2D). **Art gốc vẽ nhân vật nhìn TRÁI** nên `flipX = velocity.x > 0`.

**Vật lý không đổi một con số nào** — `moveSpeed 5`, `jumpForce 12`, collider 1×1. 81/81 cú nhảy đã verify vẫn nguyên.

### 6.7 Khung hình dọc — **xong** *(S2-004)*

Nội dung 5 màn nằm trong X ∈ [−2.40, +2.50]. Ở `ortho 5` cố định, **mọi iPhone đời cao cắt mất ~0.29 unit** hai bên.

`CameraAspectFitter` tính `ortho = Max(playableWidth / (2 × aspect), 5)` lúc chạy. Khoá `followX` của `CameraFollow2D`, kẹp Player trong `[−2.225, +2.225]` (đã trừ nửa bề ngang collider) ngay trong `PlayerMotor`. `ProjectSettings`: chỉ cho xoay **Portrait + PortraitUpsideDown**, tắt cả hai chiều ngang.

**Không dịch một vật thể nào trong màn.**

### 6.8 Hiệu ứng nhặt sao — **xong** *(S2-005)*

19 sprite FX (bộ này bạn xuất lại có alpha thật), **shader additive tự viết** — URP `Sprite-Unlit-Default` hardcode `Blend SrcAlpha OneMinusSrcAlpha` nên không cộng sáng được. 17 prefab FX + `StarFXPool` prewarm **137 object**: luồng nhặt sao **không `Instantiate` lần nào**.

Chuỗi: chớp → 3 lớp bung → sao chổi bay theo Bezier bậc hai (đuôi/bụi/lấp lánh) → quầng sáng túi → **rồi mới cộng điểm**. Vẫn đúng một bộ đếm cũ, chỉ đổi thời điểm gọi.

Hai chi tiết dễ sai:
- Điểm đến đọc lại **mỗi frame** từ Transform của túi, còn điểm đầu + điểm điều khiển **chốt một lần** — Hero chạy tiếp thì sao vẫn đập đúng chỗ mà đường cong không vặn theo bước chân.
- Mảnh sao **không** bị `Destroy` lúc chạm. Huỷ ngay thì callback lúc sao tới túi rơi vào hư vô và **mất luôn phần thưởng**.

Âm thanh: 3 file `SFX_StarCollect_01/02/03.mp3` bốc ngẫu nhiên, phát qua một `AudioSource` dùng chung, không cấp phát.

### 6.9 Đoạn phim chòm sao — **xong** *(S2-006)*

Dựng mới hoàn toàn trong **không gian thế giới**, ngay trong scene đang chơi, để **tái dùng nguyên si** `StarFXPool` / `PooledStarFX` / `StarFlyAnimator` / `PocketFXController`. Prefab `ConstellationRig.prefab`.

`ConstellationScreen.Show()` **giữ nguyên chữ ký** nên `LevelFlowManager` không sửa một dòng. Bên trong rẽ nhánh: có `cinematic` thì chiếu cảnh, không thì chạy nhánh UI cũ (giữ lại theo yêu cầu).

Thứ tự: mờ vào → **Hero chạy từ mép trái vào giữa** (mặt quay phải) → túi sáng → sao bắn lên **từng ngôi**, tự tay vẽ thành chòm → nét nối sáng dần → **cả chòm sáng bừng** → tên hiện → **phóng to riêng chòm + tên** trong lúc màn mờ dần → sang thẳng màn kế.

- **Chỉ phóng to chòm sao và tên, không đụng camera** — đụng camera là Hero bị cắt mất.
- Hero được đặt vào **một độ cao cố định trong khung**, không dùng vị trí lúc chạm đích — lúc đó nó có thể đang bay lơ lửng.
- Bỏ qua toàn bộ cảnh bằng một cú chạm, có **0.6s ân hạn** đầu cảnh.

---

## 7. THƯ MỤC *(thay đổi so với bản trước)*

```
Assets/
├── Animations/Player/      Hero_Animator.controller + 5 .anim
├── Art/Character/Hero/     Concept/ (7 sheet gốc) · Sheets/ (5 sheet dùng được) · Production/ · Animation/
├── Audio/Music/            6 file BGM
├── Audio/SFX/Star/         SFX_StarCollect_01/02/03.mp3
├── Constellation/          bg, star_glow, constellation_line, sparkle
├── Prefabs/Player/         Hero.prefab
├── Prefabs/FX/StarCollection/  17 prefab FX + StarFXPool + ConstellationRig + Constellation_Node
├── Editor/                 công cụ dev, KHÔNG vào build
├── Environment/<Region>/   art 5 khu — thay cho Assets/Particles cũ
├── UI/                     ui_panel_round (9-slice), ui_circle
├── Prefabs/ Scenes/ Scripts/ Settings/ Sprites/
├── Enviroment/             ⚠ THƯ MỤC RỖNG, sai chính tả — nên xoá
└── Particles/              ⚠ THƯ MỤC RỖNG sau khi dời — nên xoá
```

---

## 8. DESIGN DECISIONS

**Không được tự ý đảo ngược bất kỳ mục nào dưới đây.**

### 8.1 Vision & thể loại
1. Không Combat/Enemy/Boss/Skill Tree/Shop. 2. Không tự thêm mechanic ngoài roadmap. 3. Trải nghiệm quan trọng hơn số lượng mechanic. 4. Gameplay chỉ: Platforming, Khám phá, Leo cao, Khôi phục bầu trời.

### 8.2 Cấu trúc hành trình

> ⚠️ **Luật 6 ("Leo liên tục, không cắt ngang") đã bị đảo ở S3-000** cùng với §2. Các luật còn lại của mục này (Region thay cho Level, Goal không phải mục tiêu cuối, Transition tự động, không Next Level/Retry, không màn Level Complete) **vẫn giữ nguyên**.
5. Region thay cho Level. 6. Leo liên tục, không cắt ngang. 7. Goal không phải mục tiêu cuối. 8. Transition tự động, điện ảnh. 9. Không Next Level/Retry trong luồng chính. 10. Không màn hình Level Complete. 11. Tên khu vực hiện tự động.

### 8.3 Star Fragment & Constellation
12. Star Fragment là ánh sáng, không phải điểm. 13. ~~Không bắt buộc thu hết sao để qua region~~ → **ĐẢO NGƯỢC ở S2-009 theo quyết định của người dùng: số sao trong màn = số node của chòm sao, và Astral Gate KHOÁ tới khi nhặt đủ.** Hệ quả trực tiếp: hạng sao luôn là 3, nên `nodesRestored` (S2-006) luôn đạt tối đa ngay lần đầu và cơ chế "bầu trời lành dần" trở thành code chết — giữ lại nhưng không còn tác dụng. 14. Constellation là meta progression dài hạn. 15. Fragment cộng dồn toàn chapter, đếm lúc nhặt (không phải lúc chạm Goal). 16. Khôi phục không chuyển scene/menu/popup. 17. Mốc sau hoành tráng hơn mốc trước. 18. UI tiến trình nhỏ gọn, không che gameplay. 19. Chơi lại từ đầu chapter thì xem lại được toàn bộ khoảnh khắc khôi phục. 20. **Tên chòm sao hiện đồng thời với nét vẽ, tan cùng lúc** — người chơi phải cảm thấy đang chứng kiến, không phải xem xong mới biết tên. 21. Tên + description lấy từ `ConstellationData`, không hardcode.

### 8.4 Biome, Atmosphere & Presentation *(S1-013 → S1-015)*
22. Mỗi Region phải có bản sắc hình ảnh + không khí + ambient + chòm sao riêng — không chỉ khác gameplay. 23. Chuyển Region: bầu trời đổi màu mượt, không nhảy màu. 24. Diện mạo Region nằm trong `RegionData`, không rải rác scene. 25. Biome/Atmosphere không được ảnh hưởng ngược lên gameplay. 26. Trạng thái phiên chơi (`BiomeSession`, `RegionTitleSession`) không ghi save. 27. Nhạc + ambient là 2 kênh độc lập, không cắt đột ngột, chống phát trùng. 28. **Không dùng Singleton/DontDestroyOnLoad để giải quyết crossfade xuyên scene** — giới hạn "fade out rồi fade in" là đánh đổi có chủ đích. 29. `RegionAtmosphereManager`/`ParallaxLayer` đọc Region/Camera qua tham chiếu gián tiếp (`BiomeManager.Region`, `Camera.main`) — không tự có field riêng dễ lệch. 30. **Parallax mạnh chỉ an toàn trên trục camera di chuyển chậm** — trục giật nhanh (Y trong game leo dọc) phải giữ factor sát 1.

### 8.5 Gameplay
31. **Chết khi rơi khỏi màn** — Kill Floor `y = -12`, thấp hơn hẳn platform thấp nhất (`-1.5`) nên không thể báo nhầm *(đổi ở S1-020; luật cũ “rơi quá N unit so với đỉnh” báo nhầm vì Player nhảy cao 7.34)*. 32. Không cơ chế kéo-thả kiểu ná. 33. Nhảy tha thứ lỗi bấm (coyote + buffer). 34. Khoá di chuyển không tắt animation.

### 8.6 Kiến trúc
35. Không Singleton. 36. Không hardcode số lượng/tên bất kỳ nội dung nào. 37. Single-Writer cho mọi tài nguyên chia sẻ (xem bảng 4.1). 38. Goal chỉ phát event. 39. Một class một trách nhiệm. 40. Mở rộng qua kế thừa/composition, không sửa lõi. 41. `[SerializeField]`, không `public` field. 42. Interface khi có nhiều cách hiện thực. 43. ScriptableObject cho dữ liệu designer, SaveData cho tiến trình — không trộn lẫn. 44. Không refactor/rename chủ động. 45. **Phép ghi save "đi lên" và "đi xuống" phải là hai hàm riêng.** 46. Class nào giữ nhịp thì sở hữu con số thời gian dùng chung, không để mỗi bên tự tính rồi lệch. 47. **`Instantiate(prefab, position, rotation, parent)` không được dùng khi parent không đứng ở gốc toạ độ có ý nghĩa** — ép world position tuyệt đối, xoá offset prefab tự khai. 48. **Hai `LateUpdate`/`Update` phụ thuộc thứ tự lẫn nhau bắt buộc phải đặt `executionOrder` tường minh.**

---

### 8.7 Bài học từ S1-016 → S1-020B *(bổ sung)*
49. **Sprite phải mã hoá hành vi.** Platform trông vỡ mà không rơi, hoặc trông lành mà sẽ rơi, đều là nói dối người chơi. Đa dạng hình ảnh không bao giờ được đè lên tín hiệu gameplay.
50. **Ảnh nền dùng `AspectEnvelopeFitter` (fill), không dùng `Image.preserveAspect` (fit).** Ảnh ngang đặt trong màn dọc mà dùng *fit* chỉ phủ ~56% chiều cao.
51. **Mọi UI chạm mép màn hình phải nằm trong `SafeAreaFitter`.**
52. **Component chạm phải theo dõi `pointerId`.** Không có nó, ngón thứ hai xoá trạng thái ngón thứ nhất.
53. **`GetComponentsInChildren` trả về cả component trên chính mình** — lọc `enabled` trước khi cache, nếu không sẽ bật lại thứ đã cố tình tắt.
54. **Không để `Update()` và coroutine cùng ghi một thuộc tính.** Đây là Single-Writer áp dụng ở cấp thuộc tính, không chỉ cấp đối tượng.
55. **Sửa file khi Unity đang mở thì Unity có thể ghi đè ngược.** Asset/ProjectSettings phải Reimport hoặc khởi động lại Editor.
56. **Reset save trong lúc đang Play là vô nghĩa** — `ProgressManager` giữ bản trong bộ nhớ và ghi đè khi hoàn thành màn.

### 8.8 Bài học từ S2-002 → S2-006 *(bổ sung)*
57. **Project là URP — material mặc định của sprite là `Sprite-Lit-Default.mat`** (`a97c105638bdf8b4a8650670310a4cd3`). Ghi GUID default-resources của pipeline built-in vào là nhân vật thành **khối magenta**. Mọi script kiểm tra sau khi sửa YAML **phải kiểm cả material**, không chỉ sprite.
58. **Stub `stripped` của MonoBehaviour cần 9 trường, kể cả `m_Script`** — native type (GameObject/Transform) chỉ cần 3. Thiếu là tham chiếu prefab trong scene thành null, lỗi chỉ nổ ở màn 2 trở đi.
59. **GUID bắt buộc đúng 32 ký tự hex.** Đặt `fx01…` là Unity từ chối cả loạt meta. Và khi sửa: **sửa ở script sinh ra file, không phải sửa file đầu ra** — sửa mỗi đầu ra thì lần chạy sau hỏng lại y nguyên.
60. **Canvas screen-space luôn vẽ đè lên mọi thứ trong không gian thế giới**, bất kể `sortingOrder` của SpriteRenderer. Cảnh diễn world-space nằm dưới một `Canvas_Transition` là **không thấy gì cả**.
61. **`Input.touchCount > 0` nghĩa là "ngón đang chạm", không phải "vừa chạm".** Người chơi còn đang giữ joystick lúc qua màn là cờ bỏ-qua bật ngay frame đầu, nuốt trọn cả đoạn phim. Phải xét `TouchPhase.Began` + một khoảng ân hạn.
62. **`Mathf.LerpUnchecked` là internal** — không gọi được.
63. **Đừng mở màn che trong `Cleanup()`.** Mở sớm một frame là loé lại màn cũ trước khi màn mới nạp xong.
64. **Unity không import asset khi đang ở Play Mode**, và chỉ refresh khi cửa sổ Editor được người dùng bấm vào. Mọi sửa file từ ngoài đều phải chờ điều đó.

---

## 9. LỖI & VIỆC CÒN LẠI

### 9.1 Chưa xác minh trên máy thật
**Chưa có một giây nào của game được chơi thử bởi tôi.** Mọi con số bố cục/độ khó là tính toán tĩnh. Các hạng mục cần chạy thử:
- Ngưỡng **Kill Floor −12**: rơi từ đỉnh Moon Gate mất ~4.7s — có thể quá lâu.
- Nhịp **Moon Platform** (warning 0.8s / vanish 4s) và **Ice drift** 1.2.
- **BGM trên iPhone**: khi test, gạt công tắc im lặng sang chế độ có chuông trước.
- Safe Area trên máy có tai thỏ.
- Joystick + Jump đồng thời.

Bổ sung sau S2-002 → S2-006 *(cũng chưa chơi thử giây nào)*:
- **Nhịp đoạn phim chòm sao** — tổng ~9s khi chiếu đủ. Có thể quá dài khi chơi lại nhiều lần.
- **Hero chạy vào khung** ở cảnh chòm sao: xuất phát ngoài mép trái 1.2 unit, 1.4s. Chưa biết trên máy thật có bị hụt hay thừa đường chạy không.
- **Contrast nhân vật vs nền** ở từng khu — chưa soi bằng mắt trên máy thật.
- **Hiệu năng pool 137 object** trên máy yếu.
- **`SFX_StarCollect_02/03` chồng tiếng** khi nhặt sao liên tiếp.

### 9.2 Thiếu nội dung
| Hạng mục | Trạng thái |
|---|---|
| Sprite nhân vật | ~~chưa có~~ → **xong ở S2-002** |
| Ambient audio | 4/5 khu chưa có file (Aurora Cliffs · Moon Gate · Sky Ruins · Cloud Garden) |
| SFX | mới có **3 file tiếng nhặt sao**. Nhảy / tiếp đất / mở chòm sao vẫn trống |
| Nhạc cảnh chòm sao | thiếu 3 file: **completion chime · ambient wind · magical resonance** — field để trống có chủ ý, thiếu tiếng thì cảnh vẫn chạy im lặng, không bao giờ được đứng hình |
| Art chòm sao | `star_glow.png` + `constellation_line.png` **xanh lạnh**, còn `Star_Fly_Core` **vàng ấm** — lệch tông, đang dùng tạm chờ quyết định |
| Nút Pause | **chưa từng tồn tại** |
| Hệ thống máu | **chưa từng tồn tại** |
| Particle khi Moon Platform tan | chưa làm (đánh đổi hiệu năng mobile) |
| Vignette cảnh kết | chưa làm (project tắt post-processing) |

### 9.3 Nợ kỹ thuật
- **Nhạc không bài nào sáng tác để lặp liền mạch.** Forest tắt dần về im lặng; 4 bài kia cắt ngang giữa câu. Chơi quá 60s là nghe rõ chỗ nối. **Giải pháp đã thiết kế nhưng chưa được duyệt:** crossfade vòng lặp, tái dùng 2 `AudioSource` sẵn có của `FadeChannel`.
- `Debug.Log` chẩn đoán trong `ConstellationScreen.Show()` — log tạm, cần gỡ khi xác nhận xong.
- **`[Cine]` `Debug.Log` trong `ConstellationCinematic.Play()`** — log tinh chỉnh, gỡ khi chốt xong nhịp.
- **`devAlwaysReplay` trên `ConstellationScreen`** — cờ dev bắt cảnh diễn lại mỗi lần. Trên đĩa đang **tắt** (chưa được ghi vào scene nào nên nạp lên là `false`); nếu tick trong Inspector rồi lưu scene thì **nhớ tắt trước khi build**, không thì bầu trời không bao giờ lành dần.
- `ChapterProgressManager` đã dời sang `Constellation/Legacy/` cùng 4 class cũ; GameObject `ConstellationSystem` đặt `m_IsActive: 0` ở cả 5 scene. **Chưa xoá hẳn** theo yêu cầu giữ nhánh UI dự phòng.
- **Hoãn tới phiên refactor (quyết định của người dùng, S2-009)** — ba khoản dưới đây đã được cân nhắc và **cố ý giữ nguyên** cho tới khi vòng lặp gameplay hoàn chỉnh. Đừng "dọn" chúng trước thời điểm đó:
  - `nodesRestored` + `GetConstellationNodes` + `SetConstellationNodes` + nhánh `litBefore < litAfter` trong cinematic là **code chết** kể từ khi cổng khoá tới lúc nhặt đủ sao (hạng sao luôn là 3). **Không xoá.**
  - `ConstellationScreen.ResolveConstellationForCurrentLevel()` trùng luật ánh xạ với `ConstellationLookup.ForLevel()`. Giữ hai bản vì hệ cinematic đang chạy đúng và không được đụng vào.
  - `ConstellationData.requiredFragments` (30 / 12 / 12 / 12 / 53) đã bị `NodeCount` thay thế nhưng vẫn còn trong asset.
- `SFX_StarCollect_02/03` dài **3.05s** trong khi `_01` chỉ **1.06s** — nhặt sao liên tiếp thì hai tiếng dài có thể chồng nhau.
- `platform_cloud_broken` dùng trên 3 platform Static ở Cloud Garden — vi phạm quy tắc 49 nhưng **do người dùng quyết định**; khe hở đo được 0.209 so với Player 0.75 nên vô hại.
- `Platform_Basic/Wide.prefab` mang tint tím/xanh lá không khớp khu nào; chỉ `PlatformSpawner` (đang tắt) tham chiếu.
- 2 thư mục rỗng `Enviroment/` (sai chính tả) và `Particles/`.
- `Particle_FallingLeaves.prefab` không ai dùng.

### 9.4 Bẫy đã biết khi phát triển
- **Sửa asset/ProjectSettings khi Unity đang mở → Unity ghi đè ngược.** Phải Reimport hoặc khởi động lại Editor. *(Đã xảy ra 2 lần với `LevelDatabase`.)*
- **Reset save khi đang Play là vô nghĩa.** Stop trước. `SaveToolsMenu` giờ đã chặn.
- **Editor Play chạy scene ĐANG MỞ**, không theo Build Settings. Dùng menu `StarSower → Play Mode Start Scene`.

---

## 10. SPRINT KẾ TIẾP — ĐỀ XUẤT

**S2-007 — Playtest Pass & Tuning** *(ưu tiên cao nhất)*
Cả 4 sprint vừa rồi **chưa có một giây nào được tôi chơi thử**. Cần chạy thật toàn Chapter 1 và chỉnh: Kill Floor −12, nhịp Moon Platform, Ice drift, và toàn bộ nhịp đoạn phim chòm sao. Đây là việc **tôi không làm thay được**.

**S2-008 — Art chòm sao đồng bộ tông màu**
Quyết định giữa: đổi sao/nét sang vàng ấm cho khớp `Star_Fly_Core`, hay đổi `Star_Fly_Core` sang xanh lạnh. Đang lệch tông rõ trên màn hình.

**S2-009 — Audio Completion**
3 file cho cảnh chòm sao · ambient cho 4 khu · SFX nhảy/tiếp đất · xử lý `_02/_03` dài 3.05s chồng tiếng.

*(còn treo từ Chapter 1)*

**S1-024 — Pause & Settings**
Nút Pause chưa từng tồn tại. Kèm luôn màn Settings để `AudioManager.SetMasterVolume/SetMusicVolume` có nơi tiêu thụ.

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

---


## 12. TÓM TẮT NHANH CHO PHIÊN MỚI

0. ⚠️ **Đọc §13 trước** — dự án đã sang giai đoạn S3 (khám phá màn hình ngang). Các mục dưới đây mô tả giai đoạn S1–S2 và nhiều chỗ đã lỗi thời.
1. **Chapter 1 chơi được trọn vẹn** từ Forgotten Forest đến cảnh kết + chòm sao. 5 khu, 5 cơ chế riêng, độ khó tăng đều 0→6→8→13→17 cú phải giữ nút.
2. **Nhân vật thật đã vào game** (S2-002), **khung hình dọc đã khoá** (S2-004), **hiệu ứng nhặt sao có pool, không cấp phát** (S2-005), **đoạn phim chòm sao chạy trong scene, có tiến trình lành dần** (S2-006).
3. **Không có Editor access** — mọi sửa scene/prefab làm bằng YAML tay + script Python có assert, sau đó audit: trùng fileID, tham chiếu treo, đối ứng cha-con, khớp tên field C#↔YAML. Unity chỉ import khi người dùng bấm vào cửa sổ Editor, và **không import khi đang Play**.
4. **Chưa playtest.** Vẫn là rủi ro lớn nhất của dự án — giờ còn lớn hơn vì đã chồng thêm 4 sprint chưa ai chơi thử.
5. **Trước khi build:** gỡ `[Cine]` log, gỡ log trong `ConstellationScreen.Show()`, xác nhận `devAlwaysReplay` tắt.
6. Đọc mục **8 (Design Decisions)** trước khi đề xuất bất kỳ thay đổi thiết kế nào — đó là phần không được tự ý đảo ngược.

---

## 13. S3 — StarSower Rebirth

> Giai đoạn hiện tại. Mọi mâu thuẫn giữa mục này và §1–§12 thì **mục này thắng**.

### 13.1 Tổng quan

StarSower **không còn là platformer leo dọc màn hình đứng**. Nó đã chính thức thành **platformer khám phá màn hình ngang**, lấy cảm hứng từ **Ori**, **Hollow Knight** và **Celeste** (ảnh hưởng nhẹ).

Game phải giữ được sự thư thái, huyền ảo và giàu không khí, đồng thời khó lên một chút — nhưng **không bao giờ ức chế kiểu Jump King**.

### 13.2 Vòng lặp mới

```
Khám phá → Tìm sao → Giải đố → Khôi phục chòm sao → Mở Astral Gate → Đi tiếp
```

### 13.3 Giữ và thay

| Giữ nguyên | Thay hẳn |
|---|---|
| Không khí huyền ảo | Màn hình dọc |
| Khôi phục chòm sao | Tiến trình thuần dọc |
| Mảnh sao thu thập | Di chuyển đơn giản |
| Astral Gate | |
| Tiến trình theo chapter | |

### 13.4 Cấu trúc thế giới

Forgotten Forest · Cloud Garden · Sky Ruins · Aurora Cliffs · Moon Gate

> ⚠️ **Đính chính so với spec:** spec S3 viết *"One chapter = one level"*, nhưng dữ liệu thật của dự án là **một chapter (`Chapter_01`) chứa năm level**. `ChapterDatabase` có đúng một chapter. Đừng dựng lại dữ liệu theo câu đó — ánh xạ level ↔ chòm sao đang chạy theo **chỉ số** trong `LevelDatabase` ↔ `ChapterData.Constellations`.

### 13.5 Camera — **xong (S3-002)**

`CameraFollow2D` bám cả hai trục, có look-ahead ngang **và dọc**, dead zone hai trục, damping riêng từng trục, và biên khung hình.

| Thông số | Giá trị |
|---|---|
| Damping ngang / dọc | 0.15 / 0.25 |
| Look-ahead ngang / dọc | 2.0 / 1.2 |
| Dead zone | 1.5 × 1.0 |
| Tốc độ tối đa | 12.0 |

**Dead zone dọc là thứ quyết định** — không có nó, mỗi cú nhảy làm cả khung hình nhấp nhô, và ở màn hình ngang việc đó mệt mắt hơn hẳn màn dọc. Rơi nhanh hơn −12 thì bỏ vùng đệm và bám sát.

Biên (`useBounds`) **đang tắt** — bật khi màn còn là hành lang sẽ khoá camera vào khoảng trống.

**Parallax ba lớp** (S3-002): `Far 0.85` · `Mid 0.70` · `Near 0.50` trong quy ước của `ParallaxLayer`.

> ⚠️ **`ParallaxLayer.parallaxFactor` NGƯỢC với "speed" của spec.** Ở đây `factor = 1` là bám camera (xa vô tận), `factor = 0` là đứng yên (ngang tầm chơi). Spec "speed 0.15" ⇒ `factor 0.85`. Đổi mà hiểu sai chiều là nền chạy ngược.

`background_mid.png` của 5 khu là **ảnh dẫn xuất bằng thuật toán** (pha 55% near + 45% far, kéo về màu sương mù, làm mờ nhẹ, hạ bão hoà) — không phải tranh vẽ tay. Thay file cùng đường dẫn là xong, không phải sửa scene.

**Rung camera ba cấp** (`CameraShake`): Small `(0.14, 0.05)` cho tiếp đất + nhặt sao · Medium `(0.22, 0.10)` cho cổng mở · Large `(0.38, 0.20)` **dành sẵn, chưa ai gọi**.

### 13.6 Kỹ năng nhân vật — **xong (S3-003)**

`PlayerAbilities` + `WallDetector` trên prefab Hero. Mọi thứ **đi qua `PlayerMotor`** — không component nào khác ghi `Rigidbody2D`.

| Kỹ năng | Số |
|---|---|
| Nhảy | lực 12 → apex 7.34 |
| Nhảy đôi | lực 10 → apex tổng 12.44 |
| Bám tường | rơi tối đa 2.5 |
| Nhảy tường | (9, 12), khoá điều khiển ngang 0.16s |
| Lao | 18 trong 0.16s, hồi 0.5s, có lao trên không |
| Bay lượn | rơi tối đa 2.2, chờ 0.18s |

Chạm đất **và** bám tường đều nạp lại nhảy đôi + lao — biến tường thành chỗ nghỉ, đúng tinh thần Ori.

Input: nút **DASH** ở 5 scene (`MobileInputProvider.dashButton`), bàn phím **Shift** hoặc **K**.

> ⚠️ **Hoạt ảnh là tạm.** Hero chỉ có 5 clip (Idle, Run, Jump, Fall, Landing). Bám tường dùng Fall, lao dùng Run, bay lượn dùng Jump. Có art thật thì thay clip, **không phải sửa logic**.

### 13.7 Bố cục màn — **xong (S3-004)**

| Màn | Bệ | Rộng | Cao | Sao |
|---|---|---|---|---|
| Forgotten Forest | 29 | 59.4 | −6.4 … 15.6 | 5 |
| Cloud Garden | 34 | 77.8 | −6.4 … 15.6 | 7 |
| Sky Ruins | 39 | 96.2 | −6.4 … 15.6 | 10 |
| Aurora Cliffs | 46 | 119.2 | −6.4 … 15.6 | 14 |
| Moon Gate | 52 | 142.2 | −6.4 … 15.6 | 18 |

Mỗi màn có: **đường chính** (bệ cách 4.6, độ cao theo hai sóng sin lệch pha) · **đường phụ** cao hơn 6.2 (cần nhảy đôi) · **khu giải đố** hai tường cách 4.8, bậc 3.4 (cần nhảy tường) · **khu ẩn** ở y = −6.4 · **3 ngôi sao bí mật** · **cổng** ở bệ cuối bên phải.

Kill Floor hạ **−12 → −16** để khu ẩn không thành bẫy chết.

108 cú nhảy trên đường chính của cả 5 màn đã kiểm bằng công thức tầm với: **0 cú không tới được**.

> ⚠️ **Bố cục sinh bằng thuật toán, chưa tinh chỉnh tay.** Nhịp đều đặn hơn một bản đồ do người thiết kế. Đây là việc còn lại rõ nhất.

### 13.8 Chòm sao

```
Một level = một chòm sao.   Một node = một mảnh sao.
requiredStars = constellation.NodeCount;   // KHÔNG BAO GIỜ viết cứng
```

Đường cong sau khi cân bằng lại (xếp theo **số sao thật** của từng chòm để giữ hình nhận ra được):

| Khu vực | Chòm sao | Node |
|---|---|---|
| Forgotten Forest | Cassiopeia | 5 |
| Cloud Garden | Lyra | 7 |
| Sky Ruins | Cygnus | 10 |
| Aurora Cliffs | Draco | 14 |
| Moon Gate | Orion | 18 |

### 13.9 Roadmap S3

| Sprint | Trạng thái |
|---|---|
| S3-001 Landscape conversion | **xong** |
| S3-002 Camera system | **xong** — trừ room transitions |
| S3-003 Player controller | **xong** — hoạt ảnh tạm |
| S3-004 Level redesign | **xong** — chưa tinh chỉnh tay |
| S3-005 Exploration system | chưa |
| S3-006 Secret areas | chưa |
| S3-007 UI redesign | chưa |
| S3-008 Difficulty balancing | chưa |

### 13.10 Việc còn lại của S3

- **Room transitions + biên camera** — code biên đã có, `useBounds` đang tắt, chưa có trigger chuyển phòng.
- **Tinh chỉnh tay 5 bản đồ** — nhịp hiện đều đặn kiểu máy sinh.
- **Sprite cho wall slide / dash / glide** — 3 bộ hoạt ảnh còn thiếu.
- **`ShakeLarge()`** chưa nơi nào gọi.
- **Ba file tiếng cổng** (`auraClip`, `openClip`, `burstClip`, `transitionClip`) vẫn trống.
- **`SettingsPanel`** mới là vỏ — `AudioManager` có sẵn `SetMasterVolume`/`SetMusicVolume`/`SetAmbientVolume` chưa ai gọi.

### 13.11 Điều quan trọng nhất

**Giữ được linh hồn của StarSower trong khi thay hoàn toàn cấu trúc.** Không khí mềm, huyền ảo, mơ màng là thứ **không được đánh đổi** lấy độ khó hay độ hoành tráng. Khó hơn một chút — không bao giờ ức chế.

---

