namespace StarSower.Core
{
    // Bề mặt mà nhân vật đang đứng lên có thể tự khai báo ma sát của nó.
    //
    // Đặt ở Core (không phải Platform) vì cả hai phía đều phụ thuộc vào nó mà KHÔNG được biết nhau:
    // GroundChecker (Player) đọc nó, IcePlatform (Platform) hiện thực nó. Nếu để interface trong
    // Platform thì Player sẽ phải tham chiếu ngược lên Platform — sai chiều phụ thuộc của dự án
    // (Core ← Systems ← Managers/Level ← UI).
    //
    // Platform bình thường KHÔNG cần gắn gì cả: không có component nào hiện thực interface này thì
    // GroundChecker trả về 1 (ma sát chuẩn). Đây là chủ ý — mọi platform đã có trong 3 khu vực
    // trước không phải sửa một dòng nào.
    public interface IGroundSurface
    {
        // 1 = như mặt đất thường. Nhỏ hơn 1 = trơn (dừng lâu hơn). Lớn hơn 1 = bám (dừng nhanh hơn).
        float FrictionMultiplier { get; }

        // Tốc độ TRÔI khi người chơi không bấm gì. 0 = đứng yên được như mặt đất thường.
        //
        // Cần riêng field này chứ không suy ra từ FrictionMultiplier: ma sát thấp chỉ làm DỪNG lâu
        // hơn, mà đứng im thì vận tốc đã bằng 0 nên chẳng có gì để hãm — mặt băng sẽ y hệt mặt
        // thường. Muốn "không đứng im được" thì phải có một vận tốc bị đẩy tới, không phải bị hãm về.
        float DriftSpeed { get; }
    }
}
