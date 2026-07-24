using System;
using UnityEngine;

namespace StarSower.Platform
{
    // Component tái sử dụng: phát hiện Player đứng LÊN (từ phía trên) và RỜI khỏi platform này.
    // Tách riêng để FallingPlatform/BreakablePlatform (và loại mới sau này) dùng chung, không lặp
    // logic phát hiện. Phân biệt Player qua playerLayer nên không phụ thuộc namespace Player.
    [RequireComponent(typeof(Collider2D))]
    public class PlatformStandDetector : MonoBehaviour
    {
        [Tooltip("Layer của Player. Chỉ vật thuộc layer này mới tính là 'người đứng lên'.")]
        [SerializeField] private LayerMask playerLayer;

        [Tooltip("Player phải nằm cao hơn tâm platform bao nhiêu (world units) mới tính là đứng TỪ TRÊN, " +
                 "tránh kích hoạt khi Player va chạm từ bên/dưới.")]
        [SerializeField] private float minStandHeight = 0.05f;

        public bool IsPlayerStanding { get; private set; }

        // Phát đúng 1 lần khi Player bắt đầu đứng lên / khi rời đi.
        public event Action OnPlayerStand;
        public event Action OnPlayerLeave;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsPlayer(collision.gameObject.layer))
                return;

            if (collision.transform.position.y < transform.position.y + minStandHeight)
                return;

            if (IsPlayerStanding)
                return;

            IsPlayerStanding = true;
            OnPlayerStand?.Invoke();
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!IsPlayer(collision.gameObject.layer))
                return;

            if (!IsPlayerStanding)
                return;

            IsPlayerStanding = false;
            OnPlayerLeave?.Invoke();
        }

        private bool IsPlayer(int layer)
        {
            return (playerLayer.value & (1 << layer)) != 0;
        }
    }
}
