using UnityEngine;
using StarSower.Core;

namespace StarSower.Player
{
    // Tự động chọn IInputProvider phù hợp theo platform (Mobile -> MobileInputProvider,
    // còn lại -> KeyboardInputProvider) và uỷ quyền (delegate) sang provider đó.
    // Bản thân InputManager cũng implement IInputProvider — PlayerController không cần biết
    // nó tồn tại, chỉ cần trỏ inputProviderSource vào đây thay vì trỏ thẳng 1 provider cụ thể,
    // nên không còn rủi ro quên đổi provider tay mỗi lần build sang platform khác.
    public class InputManager : MonoBehaviour, IInputProvider
    {
        [SerializeField] private MonoBehaviour keyboardInputProviderSource;
        [SerializeField] private MonoBehaviour mobileInputProviderSource;

        private IInputProvider activeProvider;

        private void Awake()
        {
            activeProvider = DetermineActiveProvider();
        }

        private IInputProvider DetermineActiveProvider()
        {
            if (Application.isMobilePlatform && mobileInputProviderSource != null)
                return mobileInputProviderSource as IInputProvider;

            return keyboardInputProviderSource as IInputProvider;
        }

        public float Horizontal => activeProvider != null ? activeProvider.Horizontal : 0f;
        public bool JumpPressed => activeProvider != null && activeProvider.JumpPressed;
        public bool JumpHeld => activeProvider != null && activeProvider.JumpHeld;
    }
}
