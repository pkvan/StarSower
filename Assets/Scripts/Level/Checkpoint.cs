using UnityEngine;

namespace StarSower.Level
{
    // S3-R3 — mốc hồi sinh. Đặt một cái trên mỗi thềm nghỉ; chạm vào là RespawnManager ghi nhận.
    //
    // Chỉ làm đúng một việc: báo vị trí cho RespawnManager. Luật "mốc mới phải cao hơn mốc cũ"
    // nằm bên RespawnManager, không nằm ở đây — nếu không mỗi Checkpoint lại phải biết về mọi
    // Checkpoint khác.
    //
    // Cần một Collider2D bật Is Trigger trên cùng GameObject.
    //
    // Nhận diện Player bằng LAYER chứ không phải tag — Hero.prefab để Untagged, và StarFragment
    // lẫn AstralGateController đều đang lọc bằng LayerMask. Dùng tag ở đây sẽ là quy ước thứ hai
    // trong cùng một dự án, và sẽ im lặng không bao giờ khớp.
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private RespawnManager respawnManager;

        [Tooltip("Layer của Player (Hero đang ở layer 'Player').")]
        [SerializeField] private LayerMask playerLayer;

        [Tooltip("Vị trí hồi sinh so với tâm GameObject này. Để mặc định (0,0) là hồi sinh ngay " +
                 "giữa vùng trigger.")]
        [SerializeField] private Vector2 spawnOffset = Vector2.zero;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (respawnManager == null)
                return;

            if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
                return;

            respawnManager.SetCheckpoint((Vector2)transform.position + spawnOffset);
        }
    }
}
