using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSower.UI
{
    // Hieu ung hover/press cho nut menu ve tay (S2-013 rebuild): phong to nhe khi ren, hop nhe
    // khi bam. CHI lo phan scale — doi sprite Normal/Hover/Pressed da co san o Button.transition
    // = Sprite Swap (Selectable lo phan do), component nay khong dung toi Image.sprite.
    //
    // Dung Time.unscaledDeltaTime chu KHONG phai deltaTime: nut nay CHI hien khi bang tam dung
    // dang mo, luc do Time.timeScale = 0 nen deltaTime luon bang 0 — dung no thi hoat anh dung
    // hinh vinh vien. Cung ly do PauseController.OpenAnimation da giai thich.
    [RequireComponent(typeof(RectTransform))]
    public class MenuButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                                IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("De trong thi tu lay RectTransform tren chinh GameObject nay.")]
        [SerializeField] private RectTransform target;

        [Header("Ren (hover)")]
        [SerializeField] private float hoverScale = 1.05f;
        [Min(0.01f)]
        [SerializeField] private float hoverDuration = 0.1f;

        [Header("Bam (press)")]
        [SerializeField] private float pressScale = 0.95f;
        [Min(0.01f)]
        [SerializeField] private float pressDuration = 0.05f;

        // Nut co the tat/an giua chung mot hoat anh (vd bam Resume dong ca bang) — ghi lai de
        // biet quay ve dau khi bat lai, tranh ket o mot co giua chung.
        private bool pointerInside;
        private Coroutine routine;

        private void Awake()
        {
            if (target == null)
                target = GetComponent<RectTransform>();
        }

        private void OnDisable()
        {
            // Tat giua chung hoat anh: tra ve ty le binh thuong ngay, khong de nut "dong bang" o
            // trang thai phong to/hop lai khi Bang bi an roi lai hien ra lan sau.
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
            pointerInside = false;
            if (target != null)
                target.localScale = Vector3.one;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerInside = true;
            AnimateTo(hoverScale, hoverDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerInside = false;
            AnimateTo(1f, hoverDuration);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateTo(pressScale, pressDuration);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Nha nut ma con dang ren tren no thi ve lai co ren (1.05), nha ra ngoai thi ve 1.0 —
            // ca hai truong hop deu da tung xay ra o UI cham: keo ngon tay ra khoi nut roi moi nha.
            AnimateTo(pointerInside ? hoverScale : 1f, hoverDuration);
        }

        private void AnimateTo(float scale, float duration)
        {
            if (!isActiveAndEnabled)
                return;

            if (routine != null)
                StopCoroutine(routine);
            routine = StartCoroutine(ScaleRoutine(scale, duration));
        }

        private IEnumerator ScaleRoutine(float toScale, float duration)
        {
            float from = target.localScale.x;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                float s = Mathf.Lerp(from, toScale, k);
                target.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            target.localScale = new Vector3(toScale, toScale, 1f);
            routine = null;
        }
    }
}
