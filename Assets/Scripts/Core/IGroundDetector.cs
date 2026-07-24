namespace StarSower.Core
{
    // Trạng thái "đang chạm đất/platform" của một thực thể, tách khỏi cách phát hiện thực tế.
    public interface IGroundDetector
    {
        bool IsGrounded { get; }
    }
}
