using UnityEngine;
using StarSower.Core;

namespace StarSower.Platform
{
    // Platform trơn (S1-017, Aurora Cliffs). Gắn thêm vào một platform bình thường là nó thành băng —
    // KHÔNG đụng gì tới collider, vị trí, hay các script Moving/Falling đang có trên cùng GameObject.
    //
    // Bản thân class này không làm gì cả: nó chỉ TRẢ LỜI câu hỏi "bề mặt của mày trơn cỡ nào".
    // Toàn bộ việc áp dụng nằm ở PlayerMotor — nơi duy nhất được ghi Rigidbody2D. Nhờ vậy platform
    // không cần biết Player tồn tại, và thêm loại bề mặt mới sau này (bùn dính, nam châm...) chỉ là
    // thêm một component hiện thực IGroundSurface, không sửa Player.
    public class IcePlatform : MonoBehaviour, IGroundSurface
    {
        [Tooltip("Nhân vào deceleration của Player khi đang đứng trên mặt này. 0.25 = dừng lâu gấp 4 " +
                 "lần bình thường (trượt rõ nhưng vẫn kiểm soát được). CÀNG NHỎ CÀNG TRƠN.\n\n" +
                 "KHÔNG ảnh hưởng lực nhảy, tốc độ tối đa hay điều khiển trên không — chỉ riêng " +
                 "quãng dừng. Đây là chủ ý: nhảy phải luôn nhạy, nếu không cơ chế sẽ thành ức chế.")]
        [Range(0.05f, 1f)]
        [SerializeField] private float frictionMultiplier = 0.25f;

        [Tooltip("Tốc độ TRÔI khi người chơi buông tay: đứng trên băng thì KHÔNG đứng im được, " +
                 "nhân vật cứ trượt đều theo hướng đang đi (hoặc hướng vừa nhìn nếu đứng yên).\n\n" +
                 "1.2 = khoảng 1/4 tốc độ chạy (moveSpeed 5) — đủ để thấy rõ mình đang trôi, nhưng " +
                 "chỉ cần bấm ngược lại là ghìm được ngay vì acceleration không bị giảm.")]
        [Range(0f, 3f)]
        [SerializeField] private float driftSpeed = 1.2f;

        public float FrictionMultiplier => frictionMultiplier;
        public float DriftSpeed => driftSpeed;
    }
}
