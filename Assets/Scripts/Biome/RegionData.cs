using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Biome
{
    // Một lớp background của Region. Là plain data sống bên trong RegionData (giống LevelDefinition
    // sống trong LevelDatabase) nên dùng public field — không có ai tham chiếu độc lập tới nó.
    //
    // parallaxFactor CHƯA ĐƯỢC DÙNG ở S1-013: đây là chỗ cắm sẵn cho hệ Parallax sau này. Để trống
    // (0) nghĩa là lớp đứng yên đúng như hành vi hiện tại.
    [Serializable]
    public class BackgroundLayerData
    {
        [Tooltip("Để trống thì giữ nguyên sprite đang có trong scene — chỉ đổi màu.")]
        public Sprite sprite;

        [Tooltip("Màu nhân lên sprite. Alpha < 1 để lộ bầu trời phía sau.")]
        public Color color = Color.white;

        [Tooltip("TODO S1-014: hệ số parallax, chưa có hệ thống nào đọc giá trị này.")]
        [Range(0f, 1f)]
        public float parallaxFactor;
    }

    // Toàn bộ BẢN SẮC HÌNH ẢNH của một Region, gom vào 1 asset để designer sửa mà không đụng scene.
    // Thêm Region mới = tạo 1 asset mới + gán vào BiomeManager của scene đó, không sửa code.
    //
    // Cố tình KHÔNG chứa dữ liệu gameplay (platform, số sao, độ khó) — RegionData chỉ nói "khu vực
    // này trông như thế nào", đúng phạm vi S1-013.
    [CreateAssetMenu(fileName = "RegionData", menuName = "StarSower/Region Data")]
    public class RegionData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Id ổn định, không đổi sau khi đã dùng — dành cho tra cứu về sau.")]
        [SerializeField] private string regionId;

        [Tooltip("Tên hiện lên giữa màn hình lúc vào khu vực (Region Intro).")]
        [SerializeField] private string regionName;

        [Header("Sky")]
        [Tooltip("t = 0 là CHÂN TRỜI (dưới), t = 1 là ĐỈNH TRỜI (trên).")]
        [SerializeField] private Gradient skyGradient = new Gradient();

        [Tooltip("Màu nền của Camera — chỉ lộ ra nếu Sky Plane không phủ hết khung hình.")]
        [SerializeField] private Color cameraBackgroundColor = Color.black;

        [Header("Background")]
        [Tooltip("Lớp thứ 0 khớp với lớp thứ 0 khai báo trong BackgroundManager của scene.")]
        [SerializeField] private List<BackgroundLayerData> backgroundLayers = new List<BackgroundLayerData>();

        [Tooltip("0 = không mây, 1 = mây dày. Chỉ có tác dụng khi BackgroundManager có khai báo Cloud Layer Index.")]
        [Range(0f, 1f)]
        [SerializeField] private float cloudDensity;

        [Header("Audio (placeholder — chưa có asset thật)")]
        [SerializeField] private AudioClip defaultMusic;
        [SerializeField] private AudioClip ambient;

        public string RegionId => regionId;
        public string RegionName => regionName;
        public Gradient SkyGradient => skyGradient;
        public Color CameraBackgroundColor => cameraBackgroundColor;
        public IReadOnlyList<BackgroundLayerData> BackgroundLayers => backgroundLayers;
        public float CloudDensity => cloudDensity;
        public AudioClip DefaultMusic => defaultMusic;
        public AudioClip Ambient => ambient;
    }
}
