using UnityEngine;

namespace StarSower.Core
{
    // Đảm bảo Development Console (overlay FPS/log góc màn hình) không hiện trên build thật,
    // kể cả khi build quên bỏ tick "Development Build". Chạy tự động trước khi scene đầu tiên
    // load nên không cần gắn vào bất kỳ scene/GameObject nào — không ảnh hưởng gameplay.
    internal static class DebugOverlaySuppressor
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void DisableDeveloperConsole()
        {
            Debug.developerConsoleEnabled = false;
        }
    }
}
