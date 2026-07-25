using UnityEngine;

namespace StarSower.Biome
{
    // Tạo hiệu ứng parallax: object di chuyển theo MỘT PHẦN chuyển động của Camera, không phải
    // 100% như khi làm con trực tiếp trong Transform hierarchy (cách ParticleController.followTarget
    // đã làm ở S1-014C-003 — luôn bám camera y hệt, không có độ sâu).
    //
    // factor < 1 = lớp xa (di chuyển chậm hơn camera, cảm giác lùi ra sau, dùng cho nền).
    // factor > 1 = lớp gần (di chuyển nhanh hơn camera, cảm giác lướt qua gần ống kính, tiền cảnh).
    // factor = 1 = bám camera y hệt (không khác gì làm con Camera).
    //
    // KHÔNG parent vào Camera, KHÔNG sửa Camera/Player — chỉ ĐỌC Camera.main.transform.position mỗi
    // khung hình rồi tự ghi vào transform CỦA CHÍNH NÓ. Đúng nguyên tắc Single-Writer: object nào tự
    // ghi transform của chính nó, không object nào khác được đụng vào.
    //
    // Tự tìm Camera.main lúc Awake() thay vì field kéo-thả trong Inspector: component này nằm TRONG
    // các prefab hạt (cùng GameObject với AmbientParticleField), mà prefab được spawn ĐỘNG lúc chạy
    // (qua ParticleController.Switch()) — không có cách nào kéo-thả tham chiếu Camera của MỘT scene
    // cụ thể vào 1 file prefab dùng chung nhiều scene. Camera.main tự động đúng ở bất kỳ scene nào
    // có Camera gắn tag MainCamera (mọi scene của dự án đều vậy), không cần wiring riêng cho Region.
    public class ParallaxLayer : MonoBehaviour
    {
        [Tooltip("0 = đứng yên tuyệt đối. 1 = bám camera y hệt. <1 = lớp xa/nền. >1 = lớp gần/tiền cảnh.\n\n" +
                 "TÁCH RIÊNG X và Y có chủ đích: đây là game LEO DỌC, camera bị giật lên xuống liên tục " +
                 "mỗi cú nhảy. Y thấp khiến hạt quét ngược hết 1/4 màn hình mỗi lần nhảy — rất nhức mắt. " +
                 "Giữ Y sát 1 (0.94-0.98) để nhảy không làm hạt trôi, còn X thoải mái thấp để vẫn có chiều sâu.")]
        [SerializeField] private Vector2 parallaxFactor = new Vector2(0.3f, 0.95f);

        [Tooltip("Độ lệch tối đa so với camera trước khi tự \"neo lại\" vị trí gốc — tránh lớp nền trôi " +
                 "mất hút sau một hành trình leo dài (Forgotten Forest cao ~26 unit, factor 0.2-0.3 sẽ " +
                 "lệch quá xa nếu không neo lại). KHÔNG ảnh hưởng cảm giác parallax tức thời — việc neo " +
                 "lại xảy ra đúng tại vị trí đã tính, không có bước nhảy hình ảnh nào.")]
        [SerializeField] private float maxOffsetBeforeRebase = 6f;

        private Transform cameraTransform;
        private Vector3 anchorCameraPosition;
        private Vector3 anchorLocalPosition;

        private void Awake()
        {
            Camera main = Camera.main;
            if (main == null)
            {
                Debug.LogWarning("[Parallax] Khong tim thay Camera.main — layer nay se dung yen.", this);
                enabled = false;
                return;
            }

            cameraTransform = main.transform;
            Rebase();
        }

        private void LateUpdate()
        {
            Vector3 delta = cameraTransform.position - anchorCameraPosition;
            Vector2 offset = new Vector2(delta.x * parallaxFactor.x, delta.y * parallaxFactor.y);
            transform.position = anchorLocalPosition + new Vector3(offset.x, offset.y, 0f);

            if (offset.magnitude > maxOffsetBeforeRebase)
                Rebase();
        }

        // Chụp lại "vị trí camera hiện tại" + "vị trí layer hiện tại" làm mốc mới. Vì transform.position
        // đã được set đúng bằng công thức này ngay phía trên, gọi Rebase() không làm layer nhảy đi
        // đâu cả — chỉ reset phép tính delta về 0 cho khung hình kế tiếp.
        private void Rebase()
        {
            anchorCameraPosition = cameraTransform.position;
            anchorLocalPosition = transform.position;
        }
    }
}
