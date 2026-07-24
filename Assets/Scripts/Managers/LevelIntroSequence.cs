using System;
using System.Collections;
using UnityEngine;
using StarSower.Core;
using StarSower.Player;
using StarSower.UI;

namespace StarSower.Managers
{
    // Điều phối cinematic mở màn level: tắt input -> camera zoom out -> hiện title card ->
    // camera zoom về Player -> bật lại input. Không tự vẽ UI, không tự tính easing camera —
    // chỉ gọi đúng thứ tự lên PlayerController/ICameraZoom/LevelTitleView đã có sẵn.
    public class LevelIntroSequence : MonoBehaviour
    {
        [Tooltip("Tự chạy cinematic ngay khi scene bắt đầu. Tắt đi nếu sau này có GameManager tự gọi Play().")]
        [SerializeField] private bool playOnStart = true;

        [Header("Player")]
        [SerializeField] private PlayerController playerController;

        [Header("Camera")]
        [Tooltip("Component implement ICameraZoom (vd: CameraZoom gắn trên Main Camera).")]
        [SerializeField] private MonoBehaviour cameraZoomSource;
        [SerializeField] private float zoomOutSize = 9f;
        [SerializeField] private float zoomOutDuration = 1.2f;
        [SerializeField] private float zoomInDuration = 1f;

        [Header("Title")]
        [SerializeField] private LevelTitleView titleView;
        [SerializeField] private string levelTitle = "STAR VALLEY";
        [SerializeField] private string chapterLabel = "Chapter 01";

        private ICameraZoom cameraZoom;

        private void Awake()
        {
            cameraZoom = cameraZoomSource as ICameraZoom;
        }

        private void Start()
        {
            if (playOnStart)
                Play();
        }

        public void Play(Action onComplete = null)
        {
            StartCoroutine(PlaySequence(onComplete));
        }

        private IEnumerator PlaySequence(Action onComplete)
        {
            playerController.SetInputEnabled(false);

            cameraZoom?.ZoomTo(zoomOutSize, zoomOutDuration);
            yield return new WaitForSeconds(zoomOutDuration);

            if (titleView != null)
                yield return titleView.PlayRoutine(levelTitle, chapterLabel);

            cameraZoom?.ResetZoom(zoomInDuration);
            yield return new WaitForSeconds(zoomInDuration);

            playerController.SetInputEnabled(true);
            onComplete?.Invoke();
        }
    }
}
