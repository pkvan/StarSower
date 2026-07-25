using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Audio
{
    // Phát một AmbientProfile: mỗi layer Loop được một AudioSource lặp riêng, mọi layer
    // RandomOneShot dùng CHUNG 1 AudioSource.PlayOneShot (overlap được, không cần 1 nguồn/layer).
    // Không biết Region/Biome gì cả — nhận đúng 1 AmbientProfile rồi phát, dùng lại được cho bất kỳ
    // khu vực nào (Cloud Garden, Sky Ruins, Aurora Cliffs, Moon Gate...) miễn có asset riêng.
    //
    // Random moi vong lap ca do tre lan clip -> khong bao gio ra dung 1 nhip co dinh, dung "avoid
    // repetitive patterns" ma design yeu cau.
    public class LayeredAmbientPlayer : MonoBehaviour
    {
        [Tooltip("Nhân chung vào MỌI layer, phía trên Volume riêng từng layer trong AmbientProfile. " +
                 "Để 1 (mặc định) thì Volume của layer chính là % cuối cùng nghe được — ưu tiên khai " +
                 "đúng con số mục tiêu (vd 0.15 = 15%) ngay trên từng layer thay vì nhân qua núm này.")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;

        [Tooltip("Thời gian fade khi Stop() một layer Loop đang lặp. Region hiện tại (Forgotten Forest) " +
                 "không còn layer Loop nào nên trường này chưa có tác dụng, nhưng vẫn cần cho region khác sau này.")]
        [SerializeField] private float fadeOutDuration = 1f;

        [Tooltip("Để trống thì tự tạo lúc chạy. Dùng chung cho MỌI layer RandomOneShot.")]
        [SerializeField] private AudioSource oneShotSource;

        private readonly List<AudioSource> loopSources = new List<AudioSource>();
        private readonly List<Coroutine> randomRoutines = new List<Coroutine>();

        private void Awake()
        {
            if (oneShotSource == null)
            {
                var child = new GameObject("AmbientOneShot");
                child.transform.SetParent(transform, worldPositionStays: false);
                oneShotSource = child.AddComponent<AudioSource>();
                oneShotSource.playOnAwake = false;
            }
        }

        private void OnDestroy() => Stop(immediate: true);

        // Bắt đầu phát toàn bộ layer trong profile. Tự Stop() layer cũ (nếu có) trước — gọi Play()
        // 2 lần không bao giờ tạo ra 2 bộ AudioSource chồng nhau.
        public void Play(AmbientProfile profile)
        {
            Stop(immediate: true);

            if (profile == null)
                return;

            foreach (AmbientLayerData layer in profile.Layers)
            {
                if (layer == null || layer.clips == null || layer.clips.Count == 0)
                    continue;

                if (layer.layerType == AmbientLayerType.Loop)
                    StartLoopLayer(layer);
                else
                    randomRoutines.Add(StartCoroutine(RandomOneShotRoutine(layer)));
            }
        }

        // Dừng toàn bộ layer. immediate = true (huỷ scene, Play() gọi lại) thì cắt ngay lập tức —
        // không có ý nghĩa gì để fade khi GameObject sắp biến mất. immediate = false (rời Region,
        // scene còn sống thêm một nhịp trước khi bị huỷ) thì fade cho êm.
        public void Stop(bool immediate = false)
        {
            foreach (Coroutine routine in randomRoutines)
            {
                if (routine != null)
                    StopCoroutine(routine);
            }
            randomRoutines.Clear();

            foreach (AudioSource source in loopSources)
            {
                if (source == null)
                    continue;

                if (immediate)
                    Destroy(source.gameObject);
                else
                    StartCoroutine(FadeOutAndDestroy(source, fadeOutDuration));
            }
            loopSources.Clear();
        }

        private void StartLoopLayer(AmbientLayerData layer)
        {
            AudioClip clip = layer.clips[Random.Range(0, layer.clips.Count)];
            if (clip == null)
                return;

            var child = new GameObject($"Loop_{layer.layerName}");
            child.transform.SetParent(transform, worldPositionStays: false);

            var source = child.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.volume = layer.volume * masterVolume;
            source.Play();

            loopSources.Add(source);
        }

        // Vòng lặp vô hạn: chờ ngẫu nhiên -> phát 1 clip ngẫu nhiên -> lặp lại. Bị StopCoroutine()
        // cắt ngang trong Stop(), không tự kết thúc.
        private IEnumerator RandomOneShotRoutine(AmbientLayerData layer)
        {
            while (true)
            {
                float delay = Random.Range(layer.minDelay, layer.maxDelay);
                yield return new WaitForSeconds(delay);

                AudioClip clip = layer.clips[Random.Range(0, layer.clips.Count)];
                if (clip != null)
                    oneShotSource.PlayOneShot(clip, layer.volume * masterVolume);
            }
        }

        private IEnumerator FadeOutAndDestroy(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;
            while (elapsed < duration && source != null)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            if (source != null)
                Destroy(source.gameObject);
        }
    }
}
