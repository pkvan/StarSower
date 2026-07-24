using System.Collections;
using UnityEngine;

namespace StarSower.Platform
{
    // Platform rơi: khi Player đứng lên, chờ fallDelay rồi chuyển Rigidbody2D sang Dynamic để rơi
    // bằng vật lý. Tuỳ chọn tự reset về vị trí gốc (Kinematic, đứng yên) sau resetTime.
    // Nghe PlatformStandDetector nên logic phát hiện Player không lặp lại ở đây.
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlatformStandDetector))]
    public class FallingPlatform : MonoBehaviour
    {
        [Tooltip("Thời gian chờ sau khi Player đứng lên trước khi bắt đầu rơi.")]
        [SerializeField] private float fallDelay = 0.5f;

        [Tooltip("Tự đưa platform về vị trí gốc sau khi rơi.")]
        [SerializeField] private bool autoReset = true;

        [Tooltip("Thời gian (kể từ lúc bắt đầu rơi) trước khi reset về vị trí gốc.")]
        [SerializeField] private float resetTime = 3f;

        private Rigidbody2D rb;
        private PlatformStandDetector standDetector;
        private Vector2 startPosition;
        private bool isTriggered;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            standDetector = GetComponent<PlatformStandDetector>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Start()
        {
            startPosition = rb.position;
        }

        private void OnEnable()
        {
            standDetector.OnPlayerStand += HandlePlayerStand;
        }

        private void OnDisable()
        {
            standDetector.OnPlayerStand -= HandlePlayerStand;
        }

        private void HandlePlayerStand()
        {
            if (isTriggered)
                return;

            isTriggered = true;
            StartCoroutine(FallRoutine());
        }

        private IEnumerator FallRoutine()
        {
            yield return new WaitForSeconds(fallDelay);
            rb.bodyType = RigidbodyType2D.Dynamic;

            if (!autoReset)
                yield break;

            yield return new WaitForSeconds(resetTime);
            ResetToStart();
        }

        private void ResetToStart()
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = startPosition;
            rb.rotation = 0f;
            isTriggered = false;
        }
    }
}
