using System.Collections;
using UnityEngine;

namespace StarSower.Audio
{
    // Trình phát âm thanh crossfade 2 kênh (nhạc nền + âm thanh môi trường) — dùng chung được cho
    // bất kỳ hệ thống nào cần đổi bài mượt mà. Không biết Region/Chapter/Save gì cả; chỉ nhận
    // clip + volume + fade duration rồi phát.
    //
    // KHÔNG Singleton, KHÔNG DontDestroyOnLoad — đúng quy tắc kiến trúc của dự án (mục 8.6 #39).
    // Hệ quả: vì mỗi Region là một scene riêng và Unity phá huỷ toàn bộ GameObject ngay khi
    // SceneManager.LoadScene chạy, crossfade THẬT SỰ (2 bài chồng lên nhau) chỉ khả thi trong lúc
    // còn ở cùng 1 scene. Qua ranh giới scene, "fade out rồi fade in" được ghép từ hai lần fade độc
    // lập: scene cũ tự fade nhạc về 0 TRƯỚC khi bị huỷ (xem RegionAtmosphereManager.FadeOutForDeparture,
    // gọi từ LevelFlowManager lúc màn hình che kín), scene mới fade in từ im lặng khi vào Awake().
    public class AudioManager : MonoBehaviour
    {
        [Header("Music Channel (để trống thì tự tạo AudioSource lúc chạy)")]
        [SerializeField] private AudioSource musicSourceA;
        [SerializeField] private AudioSource musicSourceB;

        [Header("Ambient Channel (để trống thì tự tạo AudioSource lúc chạy)")]
        [SerializeField] private AudioSource ambientSourceA;
        [SerializeField] private AudioSource ambientSourceB;

        [Header("Mixing (S1-014B)")]
        [Tooltip("Nhân chung vào cả 2 kênh, phía trên Music/Ambient Volume của kênh và Music/Ambient Volume riêng của từng Region.")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;

        [Tooltip("Nhân vào TOÀN BỘ nhạc nền, độc lập với Music Volume khai báo riêng trong từng RegionData.")]
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 1f;

        [Tooltip("Nhân vào TOÀN BỘ âm thanh môi trường, độc lập với Ambient Volume khai báo riêng trong từng RegionData.")]
        [Range(0f, 1f)]
        [SerializeField] private float ambientVolume = 1f;

        private FadeChannel musicChannel;
        private FadeChannel ambientChannel;

        private void Awake()
        {
            EnsureSource(ref musicSourceA, "MusicA");
            EnsureSource(ref musicSourceB, "MusicB");
            EnsureSource(ref ambientSourceA, "AmbientA");
            EnsureSource(ref ambientSourceB, "AmbientB");

            musicChannel = new FadeChannel(this, musicSourceA, musicSourceB);
            ambientChannel = new FadeChannel(this, ambientSourceA, ambientSourceB);
        }

        private void EnsureSource(ref AudioSource source, string childName)
        {
            if (source != null)
                return;

            var child = new GameObject(childName);
            child.transform.SetParent(transform, worldPositionStays: false);
            source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = 0f;
        }

        // clip = null nghĩa là "im lặng" — kênh fade về 0 rồi dừng, không phát gì. Gọi lại đúng clip
        // đang phát thì bị bỏ qua (chống phát trùng / restart nhạc từ đầu vô cớ).
        //
        // volume truyền vào là Volume RIÊNG của Region (RegionData.MusicVolume/AmbientVolume) —
        // hàm này nhân thêm 2 tầng mixing chung (masterVolume, musicVolume/ambientVolume của kênh)
        // để ra volume phát thật sự.
        public void PlayMusic(AudioClip clip, float volume, float fadeDuration) =>
            musicChannel.Play(clip, volume * musicVolume * masterVolume, fadeDuration);

        public void PlayAmbient(AudioClip clip, float volume, float fadeDuration) =>
            ambientChannel.Play(clip, volume * ambientVolume * masterVolume, fadeDuration);

        // Fade cả hai kênh về im lặng — dùng lúc rời khu vực, TRƯỚC khi scene bị Unity phá huỷ.
        public void FadeOutAll(float duration)
        {
            musicChannel.FadeOutAndStop(duration);
            ambientChannel.FadeOutAndStop(duration);
        }

        // Đổi mức mixing chung. KHÔNG áp lại ngay lên âm thanh đang phát — dự án chưa có màn hình
        // Settings/slider runtime nào tiêu thụ giá trị này, nên đây là con số designer chỉnh trong
        // Inspector TRƯỚC khi Play, không phải một mixer phản ứng tức thời. Thêm khả năng phản ứng
        // sống là việc của sprint làm màn hình Settings, không phải sprint này.
        public void SetMasterVolume(float value) => masterVolume = Mathf.Clamp01(value);
        public void SetMusicVolume(float value) => musicVolume = Mathf.Clamp01(value);
        public void SetAmbientVolume(float value) => ambientVolume = Mathf.Clamp01(value);

        // Một kênh crossfade độc lập. Plain C# class (không phải MonoBehaviour) vì nó chỉ là sổ
        // sách kế toán cho 2 AudioSource có sẵn — mượn StartCoroutine của AudioManager làm chủ.
        private class FadeChannel
        {
            private readonly MonoBehaviour owner;
            private readonly AudioSource sourceA;
            private readonly AudioSource sourceB;
            private bool activeIsA = true;
            private AudioClip currentClip;
            private Coroutine running;

            public FadeChannel(MonoBehaviour owner, AudioSource sourceA, AudioSource sourceB)
            {
                this.owner = owner;
                this.sourceA = sourceA;
                this.sourceB = sourceB;
            }

            private AudioSource Active => activeIsA ? sourceA : sourceB;
            private AudioSource Idle => activeIsA ? sourceB : sourceA;

            public void Play(AudioClip clip, float volume, float fadeDuration)
            {
                if (clip == currentClip && (clip == null || Active.isPlaying))
                    return;

                currentClip = clip;

                if (running != null)
                    owner.StopCoroutine(running);

                running = owner.StartCoroutine(clip == null
                    ? FadeToSilence(fadeDuration)
                    : CrossfadeTo(clip, volume, fadeDuration));
            }

            public void FadeOutAndStop(float duration)
            {
                currentClip = null;
                if (running != null)
                    owner.StopCoroutine(running);
                running = owner.StartCoroutine(FadeToSilence(duration));
            }

            private IEnumerator CrossfadeTo(AudioClip clip, float targetVolume, float duration)
            {
                AudioSource next = Idle;
                AudioSource prev = Active;

                next.clip = clip;
                next.volume = 0f;
                next.loop = true;
                next.Play();

                float prevStartVolume = prev.volume;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
                    next.volume = Mathf.Lerp(0f, targetVolume, t);
                    prev.volume = Mathf.Lerp(prevStartVolume, 0f, t);
                    yield return null;
                }

                next.volume = targetVolume;
                prev.volume = 0f;
                prev.Stop();

                activeIsA = !activeIsA;
                running = null;
            }

            private IEnumerator FadeToSilence(float duration)
            {
                AudioSource active = Active;
                float startVolume = active.volume;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
                    active.volume = Mathf.Lerp(startVolume, 0f, t);
                    yield return null;
                }
                active.volume = 0f;
                active.Stop();
                running = null;
            }
        }
    }
}
