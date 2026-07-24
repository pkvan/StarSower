using UnityEngine;

namespace StarSower.Core
{
    // Trừu tượng hoá "đối tượng Camera đang theo dõi", tách CameraFollowY khỏi việc phải biết
    // đó là Player hay một điểm cố định (Boss Intro, Cutscene, Region Transition sau này).
    public interface ICameraTarget
    {
        Vector3 Position { get; }
    }
}
