namespace StarSower.Core
{
    // "Tôi biết bề mặt đang đỡ nhân vật trơn cỡ nào."
    //
    // Tách RIÊNG khỏi IGroundDetector thay vì thêm thẳng property vào đó — cố ý: IGroundDetector
    // chỉ trả lời đúng một câu "có đang chạm đất không", và mọi thứ đang hiện thực nó (kể cả test
    // double sau này) không phải sửa một dòng nào vì Aurora Cliffs cần thêm ma sát.
    //
    // PlayerController hỏi bằng pattern-matching, nên detector nào không cài interface này thì
    // Player đơn giản là dùng ma sát chuẩn — không lỗi, không cần cấu hình.
    public interface ISurfaceProvider
    {
        // 1 = mặt thường. Nhỏ hơn 1 = trơn. Xem IGroundSurface.
        float SurfaceFriction { get; }

        // Tốc độ trôi của bề mặt đang đứng. 0 = đứng yên được.
        float SurfaceDriftSpeed { get; }
    }
}
