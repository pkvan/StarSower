using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using StarSower.Constellations;

namespace StarSower.UI
{
    // Mot the chapter. Cung nguyen tac voi LevelSelectEntryView: view thuan, khong tu tra tien do.
    public class ChapterSelectEntryView : MonoBehaviour
    {
        [Tooltip("Anh chu dao cua chapter, lay tu khu vuc MO DAU cua no. De trong thi the chi co " +
                 "mau phang.")]
        [SerializeField] private Image backgroundImage;

        [SerializeField] private Text nameLabel;
        [SerializeField] private Text progressLabel;
        [SerializeField] private Image lockIcon;
        [SerializeField] private Button selectButton;

        [Min(0.01f)]
        [SerializeField] private float tapDuration = 0.15f;
        [SerializeField] private float tapScale = 1.05f;
        [SerializeField] private float lockedShake = 14f;

        private Coroutine routine;
        private Sprite backgroundSprite;

        public void Setup(ChapterData data, bool unlocked, int restored, int total,
                          Sprite background, Action<ChapterData> onSelected)
        {
            SetBackground(background);

            if (nameLabel != null)
                nameLabel.text = data.ChapterName;

            if (progressLabel != null)
            {
                progressLabel.gameObject.SetActive(unlocked && total > 0);
                if (unlocked && total > 0)
                    progressLabel.text = "✦ " + restored + " / " + total;
            }

            if (lockIcon != null)
                lockIcon.gameObject.SetActive(!unlocked);

            selectButton.interactable = true;
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() =>
            {
                Play(unlocked ? TapRoutine() : LockedRoutine());
                if (unlocked)
                    onSelected(data);
            });
        }

        // Cung cach cat nhu the level: anh khu vuc la anh DOC, the thi ngang va det. Keo day be
        // NGANG the roi de phan thua tran ra theo chieu doc, RectMask2D tren the cat bot — ra mot
        // dai ngang khong meo.
        public void SetBackground(Sprite sprite)
        {
            backgroundSprite = sprite;

            if (backgroundImage == null)
                return;

            backgroundImage.gameObject.SetActive(sprite != null);
            if (sprite == null)
                return;

            backgroundImage.sprite = sprite;
            ResizeBackground();
        }

        // Luc Setup() chay thi VerticalLayoutGroup chua gan be ngang cho the, doc rect.width ra 0.
        private void OnRectTransformDimensionsChange()
        {
            ResizeBackground();
        }

        private void ResizeBackground()
        {
            if (backgroundImage == null || backgroundSprite == null)
                return;

            float width = ((RectTransform)transform).rect.width;
            if (width <= 0f)
                return;

            float ratio = backgroundSprite.rect.height / Mathf.Max(backgroundSprite.rect.width, 1f);
            backgroundImage.rectTransform.sizeDelta = new Vector2(0f, width * ratio);
        }

        private void Play(IEnumerator next)
        {
            if (routine != null)
                StopCoroutine(routine);
            routine = StartCoroutine(next);
        }

        private IEnumerator TapRoutine()
        {
            Transform tr = transform;
            float t = 0f;
            while (t < tapDuration)
            {
                t += Time.unscaledDeltaTime;
                float s = 1f + Mathf.Sin(Mathf.Clamp01(t / tapDuration) * Mathf.PI) * (tapScale - 1f);
                tr.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            tr.localScale = Vector3.one;
            routine = null;
        }

        private IEnumerator LockedRoutine()
        {
            var rect = (RectTransform)transform;
            Vector2 basePos = rect.anchoredPosition;
            float t = 0f;
            while (t < tapDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / tapDuration);
                rect.anchoredPosition = basePos + new Vector2(Mathf.Sin(k * Mathf.PI * 3f) * lockedShake * (1f - k), 0f);
                yield return null;
            }
            rect.anchoredPosition = basePos;
            routine = null;
        }
    }
}
