using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Audio
{
    // Loop = 1 bài nền lặp liên tục (gió, tiếng mây...). RandomOneShot = 1 nhóm âm thanh rời rạc,
    // phát ngẫu nhiên theo chu kỳ (chim, lá xào xạc...) — không lặp, không nối đuôi nhau đều đặn.
    public enum AmbientLayerType
    {
        Loop,
        RandomOneShot,
    }

    // Một lớp âm thanh môi trường. Là plain data sống bên trong AmbientProfile (giống
    // BackgroundLayerData sống trong RegionData) nên dùng public field — không có ai tham chiếu
    // độc lập tới nó.
    //
    // "Chim buổi sáng" và "chim thường" là 2 layer RandomOneShot RIÊNG (không gộp vào 1 danh sách
    // clip có trọng số) — cố tình đơn giản: 2 layer cùng kiểu, khác minDelay/maxDelay, thay vì thêm
    // khái niệm "trọng số" vào model. "Hiếm hơn" đạt được bằng cách đặt minDelay/maxDelay dài hơn,
    // không cần thêm field mới nào.
    [Serializable]
    public class AmbientLayerData
    {
        [Tooltip("Chỉ để hiện tên trong Inspector/log, không ảnh hưởng phát âm thanh. Vd: Wind, Birds, Morning Bird, Leaves.")]
        public string layerName;

        public AmbientLayerType layerType = AmbientLayerType.RandomOneShot;

        [Tooltip("Loop: chọn ngẫu nhiên 1 clip trong danh sách để lặp. RandomOneShot: mỗi lần đến giờ, chọn ngẫu nhiên 1 clip để phát 1 lần.")]
        public List<AudioClip> clips = new List<AudioClip>();

        [Range(0f, 1f)]
        [Tooltip("Nhân với Master Volume của LayeredAmbientPlayer để ra âm lượng phát thật.")]
        public float volume = 1f;

        [Header("Chỉ dùng cho Random One-Shot")]
        [Tooltip("Khoảng chờ ngẫu nhiên giữa 2 lần phát, tính bằng giây.")]
        public float minDelay = 10f;
        public float maxDelay = 30f;
    }
}
