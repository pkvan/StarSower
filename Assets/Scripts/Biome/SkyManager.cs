using System.Collections;
using UnityEngine;

namespace StarSower.Biome
{
    // Người ghi DUY NHẤT lên bầu trời: tấm Sky Plane (gradient) và Camera.backgroundColor.
    //
    // Gradient được nướng thành một Texture2D dọc ngay lúc chạy, nên KHÔNG cần asset ảnh nào và
    // designer chỉnh Gradient trong Inspector là thấy đổi ngay. Sky Plane là con của Main Camera
    // nên luôn dính theo khung hình mà không cần viết vào transform của camera — không phạm quy tắc
    // "chỉ CameraFollow2D được ghi vị trí camera".
    //
    // Không biết Region nào là hiện tại, không tự chọn thời điểm — BiomeManager gọi.
    public class SkyManager : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer skyRenderer;

        [Tooltip("Camera sẽ được đặt lại backgroundColor theo Region. Để trống thì bỏ qua bước đó.")]
        [SerializeField] private Camera targetCamera;

        [Tooltip("Kích thước tấm trời theo world unit. Phải phủ rộng hơn khung hình ở mọi tỉ lệ màn hình.")]
        [SerializeField] private Vector2 skySize = new Vector2(30f, 24f);

        [Tooltip("Số bậc màu khi nướng gradient thành texture. Cao hơn = chuyển màu mịn hơn, tốn hơn.")]
        [SerializeField] private int gradientSteps = 64;

        private Texture2D skyTexture;
        private Sprite skySprite;
        private Color[] pixelBuffer;

        private void OnDestroy()
        {
            // Texture/Sprite tạo bằng code không được Unity thu gom tự động — phải tự huỷ, nếu
            // không mỗi lần load lại scene sẽ rò một bộ.
            if (skyTexture != null)
                Destroy(skyTexture);
            if (skySprite != null)
                Destroy(skySprite);
        }

        public void ApplyImmediate(RegionData region)
        {
            if (region == null)
                return;

            EnsureBuilt();
            WriteGradient(region.SkyGradient);

            if (targetCamera != null)
                targetCamera.backgroundColor = region.CameraBackgroundColor;
        }

        // Chuyển mượt từ bầu trời khu vực cũ sang khu vực mới. Gọi ngay lúc scene mới bắt đầu, chạy
        // song song với transition mở màn hình nên người chơi thấy bầu trời "sáng dần thành màu
        // khác" chứ không thấy màu nhảy một phát.
        public IEnumerator BlendTo(RegionData from, RegionData to, float duration)
        {
            if (to == null)
                yield break;

            if (from == null || duration <= 0f)
            {
                ApplyImmediate(to);
                yield break;
            }

            EnsureBuilt();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                WriteBlendedGradient(from.SkyGradient, to.SkyGradient, t);

                if (targetCamera != null)
                    targetCamera.backgroundColor =
                        Color.Lerp(from.CameraBackgroundColor, to.CameraBackgroundColor, t);

                yield return null;
            }

            ApplyImmediate(to);
        }

        // Dựng lười thay vì dựng trong Awake(): BiomeManager nằm cùng GameObject nên thứ tự Awake
        // giữa hai component là không đảm bảo. Dựng ngay tại điểm sử dụng thì không còn phụ thuộc
        // thứ tự nữa.
        private void EnsureBuilt()
        {
            if (skyTexture != null)
                return;

            gradientSteps = Mathf.Max(2, gradientSteps);

            skyTexture = new Texture2D(1, gradientSteps, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            pixelBuffer = new Color[gradientSteps];

            if (skyRenderer == null)
                return;

            // pixelsPerUnit = 1 nên sprite rộng 1, cao gradientSteps world unit; localScale bên dưới
            // quy đổi về đúng skySize, không phụ thuộc vào gradientSteps designer chọn.
            skySprite = Sprite.Create(skyTexture, new Rect(0f, 0f, 1f, gradientSteps),
                new Vector2(0.5f, 0.5f), 1f);
            skyRenderer.sprite = skySprite;
            skyRenderer.transform.localScale = new Vector3(skySize.x, skySize.y / gradientSteps, 1f);
        }

        private void WriteGradient(Gradient gradient)
        {
            if (gradient == null || skyTexture == null)
                return;

            for (int i = 0; i < gradientSteps; i++)
                pixelBuffer[i] = gradient.Evaluate(i / (float)(gradientSteps - 1));

            Upload();
        }

        // Nướng lại texture mỗi khung hình trong lúc blend. Chỉ 1 x gradientSteps pixel và chỉ kéo
        // dài đúng thời gian chuyển khu vực, nên chi phí không đáng kể trên mobile.
        private void WriteBlendedGradient(Gradient from, Gradient to, float t)
        {
            if (skyTexture == null)
                return;

            for (int i = 0; i < gradientSteps; i++)
            {
                float v = i / (float)(gradientSteps - 1);
                pixelBuffer[i] = Color.Lerp(from.Evaluate(v), to.Evaluate(v), t);
            }

            Upload();
        }

        private void Upload()
        {
            skyTexture.SetPixels(pixelBuffer);
            skyTexture.Apply(false);
        }
    }
}
