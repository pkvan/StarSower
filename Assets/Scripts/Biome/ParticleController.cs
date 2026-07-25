using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Biome
{
    // Người ghi DUY NHẤT cho các hiệu ứng hạt đặc trưng của Region (lá bay, sương, đom đóm, sao lấp
    // lánh...). Nhận danh sách prefab rồi dựng/dọn — không biết Region nào đang chạy, không tự chọn
    // lúc nào đổi. RegionAtmosphereManager gọi Switch() đúng lúc.
    //
    // Chưa cần object pool cho việc ĐỔI hạt: mỗi Region chỉ đổi hạt đúng 1 lần lúc vào scene
    // (Awake), không đổi liên tục trong lúc chơi. Riêng BÊN TRONG mỗi prefab (vd AmbientParticleField)
    // vẫn tự pool hạt của nó — đó là trách nhiệm của prefab, không phải của class này.
    public class ParticleController : MonoBehaviour
    {
        [Tooltip("Để trống thì hạt làm con của chính GameObject này (như cũ, đứng yên trong world). " +
                 "Gán Camera thì hạt luôn nằm trong khung hình dù người chơi leo cao tới đâu — dùng cho " +
                 "hạt môi trường kiểu AmbientParticleField (S1-014C-002). KHÔNG sửa Camera, chỉ thêm con vào Transform của nó.")]
        [SerializeField] private Transform followTarget;

        private readonly List<GameObject> spawned = new List<GameObject>();

        public void Switch(IReadOnlyList<GameObject> prefabs)
        {
            Clear();

            if (prefabs == null)
                return;

            Transform parent = followTarget != null ? followTarget : transform;

            foreach (GameObject prefab in prefabs)
            {
                if (prefab == null)
                    continue;

                // CỐ TÌNH không dùng Instantiate(prefab, parent.position, ...): cách đó ép vị trí
                // spawn trùng hệt parent.position, xoá mất offset mà bản thân prefab đã tự khai báo.
                // Bug thật đã xảy ra: parent = Main Camera Transform (world z = -10), hạt sinh ra
                // đúng tại vị trí camera — GẦN HƠN Near Clip Plane (0.3) — camera cắt bỏ, không bao
                // giờ render được dù logic C# chạy đúng.
                //
                // Set cha rồi COPY local transform của chính prefab là cách duy nhất không phụ
                // thuộc vào ngữ nghĩa mập mờ của các overload Instantiate() — prefab tự quyết định
                // nó cách parent bao xa (vd Particle_FallingLeaves đặt z = 10 để rơi đúng vào độ sâu
                // world z = 0, nơi Player/Platform đang render, thay vì kẹt tại chính ống kính camera).
                GameObject instance = Instantiate(prefab);
                instance.transform.SetParent(parent, worldPositionStays: false);
                instance.transform.localPosition = prefab.transform.localPosition;
                instance.transform.localRotation = prefab.transform.localRotation;
                instance.transform.localScale = prefab.transform.localScale;

                spawned.Add(instance);
            }
        }

        public void Clear()
        {
            foreach (GameObject instance in spawned)
            {
                if (instance != null)
                    Destroy(instance);
            }
            spawned.Clear();
        }
    }
}
