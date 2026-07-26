using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Platform
{
    // Platform ánh trăng soi (S1-018): vô hình trong đêm, chỉ hiện ra khi người chơi lại đủ gần —
    // như cầm đèn pin rọi vào bóng tối. Rời xa thì mờ dần rồi biến mất, quay lại thì hiện lên tiếp.
    //
    // Khác MoonPlatform:
    //   MoonPlatform            — luôn nhìn thấy (mờ hoặc rõ), tan biến MỘT LẦN sau khi người chơi rời.
    //   MoonlightRevealPlatform — vô hình hẳn khi ở xa, hiện/ẩn LẶP LẠI vô hạn theo khoảng cách,
    //                             và COLLIDER TẮT lúc ẩn (không có va chạm vô hình).
    //
    // Vì collider tắt lúc ẩn, bán kính phát hiện phải đủ rộng để platform hiện xong TRƯỚC khi người
    // chơi bay tới. Xem kiểm chứng ở phần chú thích của detectionRadius.
    public class MoonlightRevealPlatform : MonoBehaviour
    {
        [Header("Ánh trăng soi")]
        [Tooltip("Người chơi vào trong bán kính này thì platform hiện ra. Phải rộng hơn quãng người " +
                 "chơi đi được trong thời gian hiện hình, nếu không họ sẽ rơi xuyên qua chỗ đáng lẽ " +
                 "có platform — đó chính là 'blind death' mà thiết kế cấm.")]
        [SerializeField] private float detectionRadius = 6f;

        [Tooltip("Độ mờ lúc ẩn. KHÔNG để 0 hẳn: yêu cầu thiết kế là còn 'một chút bụi trăng gợi ý', " +
                 "để người chơi biết chỗ đó có thứ gì đó thay vì phải nhảy mò vào khoảng không.")]
        [Range(0f, 0.25f)]
        [SerializeField] private float hintAlpha = 0.09f;

        [Tooltip("Biên độ nhấp nháy của gợi ý lúc ẩn — làm nó 'thở' nhẹ như bụi trăng, dễ nhận ra " +
                 "hơn một vệt mờ đứng yên.")]
        [Range(0f, 0.1f)]
        [SerializeField] private float hintPulse = 0.04f;

        [Tooltip("Chu kỳ nhấp nháy của gợi ý (giây).")]
        [SerializeField] private float hintPulsePeriod = 2.6f;

        [Tooltip("Tốc độ hiện ra / mờ đi (đơn vị/giây). 2.5 = hiện đầy trong ~0.4s, đủ mượt để thấy " +
                 "'ma thuật' mà vẫn kịp trước khi người chơi bay tới.")]
        [SerializeField] private float revealSpeed = 2.5f;

        [Tooltip("Màu lúc hiện — bạc ánh trăng.")]
        [SerializeField] private Color revealTint = new Color(1f, 1f, 1f, 1f);

        [Header("Va chạm")]
        [Tooltip("Mức hiện tối thiểu để BẬT collider. Đặt thấp (0.35) có chủ ý: platform chịu lực " +
                 "được từ lúc mới lờ mờ, nên người chơi không bao giờ rơi xuyên qua một platform mà " +
                 "mắt họ đã nhìn thấy.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float colliderThreshold = 0.35f;

        [SerializeField] private LayerMask playerLayer;

        [Tooltip("Giãn cách dò khoảng cách (giây) — không cần dò mỗi khung hình.")]
        [SerializeField] private float proximityCheckInterval = 0.05f;

        [Header("Âm thanh (tuỳ chọn)")]
        [SerializeField] private AudioClip revealSound;

        private Collider2D platformCollider;
        private readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        private readonly List<Color> baseColors = new List<Color>();

        private float reveal;
        private float proximityTimer;
        private bool playerNear;
        private bool soundPlayed;

        private void Awake()
        {
            platformCollider = GetComponent<Collider2D>();

            // Chỉ nhận renderer đang BẬT — renderer trên chính GameObject này đã bị tắt có chủ ý
            // (còn giữ sprite Square xám); bật nó lên sẽ để lộ một ô xám to bằng collider.
            var found = new List<SpriteRenderer>();
            GetComponentsInChildren<SpriteRenderer>(true, found);
            foreach (SpriteRenderer r in found)
            {
                if (!r.enabled)
                    continue;

                renderers.Add(r);
                baseColors.Add(r.color);
            }

            platformCollider.enabled = false;
            Paint(0f);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            proximityTimer -= dt;
            if (proximityTimer <= 0f)
            {
                proximityTimer = proximityCheckInterval;
                playerNear = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer) != null;
            }

            reveal = Mathf.MoveTowards(reveal, playerNear ? 1f : 0f, revealSpeed * dt);

            // Không bao giờ TẮT collider khi người chơi còn đứng trên: họ sẽ rơi xuyên qua sàn dưới
            // chân mình mà không hiểu vì sao. Chỉ tắt khi đã mờ hẳn VÀ không ai chạm vào.
            bool wantCollider = reveal >= colliderThreshold;
            if (platformCollider.enabled != wantCollider)
            {
                if (wantCollider || !IsPlayerOverlapping())
                    platformCollider.enabled = wantCollider;
            }

            if (revealSound != null)
            {
                if (!soundPlayed && reveal > 0.05f)
                {
                    AudioSource.PlayClipAtPoint(revealSound, transform.position);
                    soundPlayed = true;
                }
                else if (reveal <= 0.01f)
                {
                    soundPlayed = false;
                }
            }

            Paint(reveal);
        }

        private bool IsPlayerOverlapping()
        {
            Bounds b = platformCollider.bounds;
            return Physics2D.OverlapBox(b.center, b.size, 0f, playerLayer) != null;
        }

        private void Paint(float t)
        {
            // Lúc ẩn vẫn giữ một chút bụi trăng "thở" nhẹ, thay vì biến mất tuyệt đối.
            float hint = hintAlpha + Mathf.Sin(Time.time * Mathf.PI * 2f / Mathf.Max(0.1f, hintPulsePeriod)) * hintPulse;
            float alpha = Mathf.Lerp(Mathf.Max(0f, hint), 1f, t);

            for (int i = 0; i < renderers.Count; i++)
            {
                Color c = baseColors[i];
                renderers[i].color = new Color(c.r * revealTint.r, c.g * revealTint.g, c.b * revealTint.b,
                                               c.a * alpha);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.8f, 0.9f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
#endif
    }
}
