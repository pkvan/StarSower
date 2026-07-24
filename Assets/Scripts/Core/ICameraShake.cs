using UnityEngine;

namespace StarSower.Core
{
    // API rung camera dùng chung cho các hệ thống khác (Boss, Combo, Event) gọi tới sau này.
    public interface ICameraShake
    {
        Vector3 CurrentOffset { get; }
        void Shake(float duration, float magnitude);
    }
}
