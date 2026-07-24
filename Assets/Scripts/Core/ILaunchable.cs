using UnityEngine;

namespace StarSower.Core
{
    // Thứ có thể bị "bắn" đi với 1 vận tốc cho trước (vd: Player bị SpringPlatform bật lên).
    // Tách qua interface để Platform không phụ thuộc trực tiếp namespace Player.
    public interface ILaunchable
    {
        void Launch(Vector2 velocity);
    }
}
