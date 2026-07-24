using System.Collections;
using UnityEngine;

namespace StarSower.Platform
{
    // Platform vỡ: khi Player đứng đủ breakDelay, platform biến mất (tắt collider + sprite).
    // Respawn (hiện lại) sau respawnTime nếu bật. Nghe PlatformStandDetector để không lặp logic
    // phát hiện Player. Điểm móc animation vỡ (nếu có) là ngay trước khi ẩn — xem HidePlatform().
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PlatformStandDetector))]
    public class BreakablePlatform : MonoBehaviour
    {
        [Tooltip("Thời gian Player đứng trên trước khi platform vỡ.")]
        [SerializeField] private float breakDelay = 0.7f;

        [Tooltip("Cho platform hiện lại sau khi vỡ.")]
        [SerializeField] private bool autoRespawn = true;

        [Tooltip("Thời gian (kể từ lúc vỡ) trước khi respawn.")]
        [SerializeField] private float respawnTime = 3f;

        [SerializeField] private Collider2D platformCollider;
        [SerializeField] private SpriteRenderer platformRenderer;

        private PlatformStandDetector standDetector;
        private bool isTriggered;

        private void Awake()
        {
            standDetector = GetComponent<PlatformStandDetector>();
            if (platformCollider == null)
                platformCollider = GetComponent<Collider2D>();
            if (platformRenderer == null)
                platformRenderer = GetComponent<SpriteRenderer>();
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
            StartCoroutine(BreakRoutine());
        }

        private IEnumerator BreakRoutine()
        {
            yield return new WaitForSeconds(breakDelay);
            // Điểm móc animation vỡ sau này (vd: trigger Animator) — hiện tại ẩn ngay.
            SetVisible(false);

            if (!autoRespawn)
                yield break;

            yield return new WaitForSeconds(respawnTime);
            SetVisible(true);
            isTriggered = false;
        }

        private void SetVisible(bool visible)
        {
            if (platformCollider != null)
                platformCollider.enabled = visible;
            if (platformRenderer != null)
                platformRenderer.enabled = visible;
        }
    }
}
