namespace StarSower.Core
{
    // Trạng thái di chuyển hiện tại của Player, suy ra từ vận tốc + grounded — không phải input.
    // Đặt ở Core vì Animation/Audio/VFX sau này chỉ cần biết enum này, không cần biết
    // PlayerMovementStateMachine (StarSower.Player) tồn tại.
    public enum PlayerMovementState
    {
        Idle,
        Running,
        Jumping,
        Falling
    }
}
