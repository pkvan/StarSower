namespace StarSower.Core
{
    // Nguồn input di chuyển ngang + nhảy, tách khỏi cách nhận input thực tế
    // (Mobile joystick+nút, bàn phím...) để PlayerController không phụ thuộc trực tiếp vào UI.
    public interface IInputProvider
    {
        float Horizontal { get; }
        bool JumpPressed { get; }

        // Nút nhảy có đang được giữ hay không (khác JumpPressed — đây là trạng thái liên tục,
        // không phải sự kiện 1 frame). Dùng cho Variable Jump Height ở PlayerMotor.
        bool JumpHeld { get; }
    }
}
