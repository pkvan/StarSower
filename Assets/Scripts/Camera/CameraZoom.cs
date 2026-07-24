using UnityEngine;
using StarSower.Core;

namespace StarSower.CameraSystem
{
    // Zoom camera orthographic mượt theo thời gian. Chỉ chỉnh Camera.orthographicSize,
    // không đụng transform.position nên hoạt động độc lập với CameraFollowY/CameraShake.
    [RequireComponent(typeof(Camera))]
    public class CameraZoom : MonoBehaviour, ICameraZoom
    {
        [SerializeField] private float defaultOrthographicSize = 5f;

        private Camera targetCamera;
        private float zoomVelocity;
        private float targetSize;
        private float zoomSmoothTime;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            targetSize = targetCamera.orthographicSize;
        }

        public void ZoomTo(float targetOrthographicSize, float duration)
        {
            targetSize = targetOrthographicSize;
            zoomSmoothTime = Mathf.Max(duration, 0.0001f);
        }

        public void ResetZoom(float duration)
        {
            ZoomTo(defaultOrthographicSize, duration);
        }

        private void Update()
        {
            if (zoomSmoothTime <= 0f)
                return;

            targetCamera.orthographicSize = Mathf.SmoothDamp(
                targetCamera.orthographicSize, targetSize, ref zoomVelocity, zoomSmoothTime);
        }
    }
}
