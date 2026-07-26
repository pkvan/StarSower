using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Platform
{
    // Moon Platform (S1-018): đá mặt trăng phản ứng với ánh trăng, 4 trạng thái.
    //
    //   Hidden    — người chơi ở xa: tối gần như hoà vào nền trời, chỉ còn chút ánh trăng mờ.
    //   Activated — người chơi lại gần: ánh trăng rọi tới, rune sáng lên, hiện rõ hoàn toàn.
    //   Vanishing — người chơi RỜI ĐI: vết nứt sáng bừng rồi tan dần, collider tắt.
    //   Restore   — vài giây sau: kết tinh lại, dùng được tiếp.
    //
    // Collider LUÔN BẬT ở Hidden và Activated. Đây là điểm mấu chốt về công bằng: platform tối
    // nhưng vẫn đứng được, và nó sáng lên TRƯỚC khi người chơi tới nơi (bán kính kích hoạt lớn hơn
    // nhiều so với một cú nhảy), nên không bao giờ có chuyện đạp phải khoảng không vô hình.
    //
    // Nghe PlatformStandDetector giống FallingPlatform — không lặp lại logic phát hiện Player.
    // Không đụng Rigidbody2D: platform này không rơi, chỉ tắt/bật.
    [RequireComponent(typeof(PlatformStandDetector))]
    public class MoonPlatform : MonoBehaviour
    {
        private enum State { Hidden, Activated, Vanishing, Restoring }

        [Header("Ánh trăng (Hidden ↔ Activated)")]
        [Tooltip("Người chơi vào trong bán kính này thì platform hiện rõ. Phải LỚN HƠN một cú nhảy " +
                 "(tầm nhảy tối đa ~7.3 unit) để platform luôn sáng xong TRƯỚC khi người chơi tới nơi.")]
        [SerializeField] private float activationRadius = 9f;

        [Tooltip("Độ mờ lúc ở xa. 0.55 = mờ đi khoảng một nửa, KHÔNG phải tàng hình: platform mờ tới " +
                 "mức không thấy gì thì người chơi đọc ra là 'chỗ này trống', không nhận ra đó là " +
                 "một cơ chế — và cũng không lên kế hoạch đường đi được.")]
        [Range(0.2f, 0.9f)]
        [SerializeField] private float hiddenAlpha = 0.55f;

        [Tooltip("Màu lúc ẩn — xanh bạc dịu, tối hơn nhưng vẫn đọc được hình dáng platform.")]
        [SerializeField] private Color hiddenTint = new Color(0.62f, 0.68f, 0.82f, 1f);

        [Tooltip("Màu lúc được ánh trăng rọi — bạc sáng.")]
        [SerializeField] private Color activatedTint = new Color(1f, 1f, 1f, 1f);

        [Tooltip("Tốc độ chuyển giữa ẩn và hiện (đơn vị/giây). Thấp = mượt, cao = dứt khoát.")]
        [SerializeField] private float revealSpeed = 3.5f;

        [Tooltip("Trần độ sáng khi người chơi đã lại gần. 1 = sáng rõ hoàn toàn (Moon Platform thường).\n\n" +
                 "Nhỏ hơn 1 = PLATFORM ẨN: lại gần cũng chỉ sáng lên một phần, người chơi phải nhìn kỹ " +
                 "mới nhận ra. Dùng cho đoạn cuối màn — thử thách quan sát, không phải thử thách phản xạ.\n\n" +
                 "Đừng để dưới 0.35: thấp hơn nữa thì trên nền trời sáng platform gần như biến mất, " +
                 "thành đoán mò chứ không còn là quan sát.")]
        [Range(0.35f, 1f)]
        [SerializeField] private float maxReveal = 1f;

        [Header("Vanishing (sau khi người chơi rời đi)")]
        [Tooltip("Vết nứt sáng bừng bao lâu trước khi tan. Người chơi đã rời platform rồi nên đây " +
                 "là tín hiệu THÔNG BÁO, không phải hạn chót — vì vậy ngắn hơn cảnh báo thường.")]
        [Range(0.2f, 3f)]
        [SerializeField] private float warningDuration = 0.8f;

        [Tooltip("Thời gian tan biến trước khi kết tinh lại.")]
        [Range(1f, 10f)]
        [SerializeField] private float vanishDuration = 4f;

        [Tooltip("Màu vết nứt lúc sắp tan.")]
        [SerializeField] private Color crackGlowColor = new Color(0.68f, 0.85f, 1f, 1f);

        [Header("An toàn")]
        [Tooltip("Layer Player. Dùng cho cả dò khoảng cách lẫn kiểm tra 'còn đứng chồng lên không' " +
                 "trước khi bật lại collider.")]
        [SerializeField] private LayerMask playerLayer;

        [Tooltip("Giãn cách giữa hai lần dò khoảng cách (giây). Không cần dò mỗi khung hình — " +
                 "0.1s là đủ nhạy mà giảm 6 lần số truy vấn vật lý trên mobile.")]
        [SerializeField] private float proximityCheckInterval = 0.1f;

        [Header("Âm thanh (tuỳ chọn)")]
        [Tooltip("Để trống thì im lặng — dự án chưa có asset SFX nào.")]
        [SerializeField] private AudioClip vanishSound;

        private PlatformStandDetector standDetector;
        private Collider2D platformCollider;
        private readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        private readonly List<Color> baseColors = new List<Color>();

        private State state = State.Hidden;
        private float reveal;          // 0 = ẩn hẳn, 1 = sáng hẳn
        private float phaseTimer;
        private float proximityTimer;
        private bool playerNear;

        private void Awake()
        {
            standDetector = GetComponent<PlatformStandDetector>();
            platformCollider = GetComponent<Collider2D>();

            // CHỈ lấy renderer đang BẬT. GetComponentsInChildren trả về cả SpriteRenderer nằm trên
            // chính GameObject này, mà từ S1-016 nó đã bị tắt có chủ ý (còn giữ sprite Square xám,
            // phần hiển thị thật nằm ở object con "Visual"). Ôm nó vào là pha hiện lại sẽ bật nó
            // dậy và để lộ một ô xám to bằng collider.
            var found = new List<SpriteRenderer>();
            GetComponentsInChildren<SpriteRenderer>(true, found);
            foreach (SpriteRenderer r in found)
            {
                if (!r.enabled)
                    continue;

                renderers.Add(r);
                baseColors.Add(r.color);
            }

            Paint(hiddenTint, hiddenAlpha);
        }

        private void OnEnable() => standDetector.OnPlayerLeave += HandlePlayerLeave;
        private void OnDisable() => standDetector.OnPlayerLeave -= HandlePlayerLeave;

        // Tan biến kích hoạt lúc RỜI ĐI, không phải lúc đứng lên: người chơi được đứng bao lâu tuỳ ý
        // để quan sát và tính đường. Áp lực nằm ở chỗ KHÔNG quay đầu lại được — trong một chuỗi
        // Moon Platform, bước vừa rời đã tan, nên phải đi tiếp chứ không lùi.
        private void HandlePlayerLeave()
        {
            if (state == State.Vanishing || state == State.Restoring)
                return;

            state = State.Vanishing;
            phaseTimer = 0f;

            if (vanishSound != null)
                AudioSource.PlayClipAtPoint(vanishSound, transform.position);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            proximityTimer -= dt;
            if (proximityTimer <= 0f)
            {
                proximityTimer = proximityCheckInterval;
                playerNear = Physics2D.OverlapCircle(transform.position, activationRadius, playerLayer) != null;
            }

            switch (state)
            {
                case State.Hidden:
                case State.Activated:
                    state = playerNear ? State.Activated : State.Hidden;

                    // maxReveal chặn trần khi mới lại gần: platform ẩn không bao giờ tự sáng hết.
                    // NHƯNG đứng được lên rồi thì sáng rõ hoàn toàn — xác nhận "tìm đúng chỗ rồi".
                    // Không làm giảm thử thách quan sát (đã tìm ra mới đứng được), mà bỏ đi cảm giác
                    // chông chênh không biết mình đang đứng trên cái gì.
                    float target = standDetector.IsPlayerStanding ? 1f
                                 : playerNear ? maxReveal
                                 : 0f;
                    reveal = Mathf.MoveTowards(reveal, target, revealSpeed * dt);
                    Paint(Color.Lerp(hiddenTint, activatedTint, reveal),
                          Mathf.Lerp(hiddenAlpha, 1f, reveal));
                    break;

                case State.Vanishing:
                    phaseTimer += dt;
                    if (phaseTimer < warningDuration)
                    {
                        // Vết nứt sáng bừng rồi tắt dần cùng độ mờ — "năng lượng ma thuật tan đi".
                        float t = phaseTimer / warningDuration;
                        float pulse = Mathf.Sin(t * Mathf.PI);              // sáng lên rồi dịu xuống
                        Paint(Color.Lerp(activatedTint, crackGlowColor, pulse), 1f - t * 0.85f);
                    }
                    else
                    {
                        platformCollider.enabled = false;
                        SetRenderersEnabled(false);
                        state = State.Restoring;
                        phaseTimer = 0f;
                    }
                    break;

                case State.Restoring:
                    phaseTimer += dt;
                    if (phaseTimer < vanishDuration)
                        break;

                    // Không bật collider ngay dưới chân Player — sẽ đẩy văng hoặc kẹt.
                    if (IsPlayerOverlapping())
                        break;

                    SetRenderersEnabled(true);
                    platformCollider.enabled = true;
                    reveal = 0f;
                    state = State.Hidden;
                    Paint(hiddenTint, hiddenAlpha);
                    break;
            }
        }

        private bool IsPlayerOverlapping()
        {
            Bounds b = platformCollider.bounds;
            return Physics2D.OverlapBox(b.center, b.size, 0f, playerLayer) != null;
        }

        // Nhân với màu gốc của TỪNG renderer thay vì gán đè, để không xoá mất tint riêng của platform.
        private void Paint(Color tint, float alpha)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                Color c = baseColors[i];
                renderers[i].color = new Color(c.r * tint.r, c.g * tint.g, c.b * tint.b, c.a * alpha);
            }
        }

        private void SetRenderersEnabled(bool value)
        {
            foreach (SpriteRenderer r in renderers)
                r.enabled = value;
        }

#if UNITY_EDITOR
        // Vẽ bán kính kích hoạt trong Scene view để chỉnh cho khớp với khoảng cách nhảy thực tế.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.68f, 0.85f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, activationRadius);
        }
#endif
    }
}
