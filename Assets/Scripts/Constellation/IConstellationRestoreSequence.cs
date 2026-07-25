using System.Collections;

namespace StarSower.Constellations
{
    // Kiểu trình diễn "khôi phục chòm sao" (bầu trời tối lại, các sao sáng lên, nối nét, chòm sao
    // hiện ra). Tách interface theo đúng cách ITransitionEffect đã làm cho transition: bên điều phối
    // chỉ biết "vẽ rồi dọn", không biết nó vẽ bằng gì. Khi có art/Timeline thật, chỉ cần gán
    // component khác vào ô restoreSequenceSource — không sửa dòng code điều phối nào.
    //
    // Cố tình TÁCH ĐÔI thay vì một hàm Play() chạy trọn gói: thẻ tên phải hiện lên cùng lúc với nét
    // vẽ và tan đi cùng lúc với chòm sao. Muốn hai thứ chạy đồng bộ thì người giữ nhịp phải là
    // ConstellationManager, nên nó cần điều khiển được từng chặng chứ không thể chờ một cục.
    public interface IConstellationRestoreSequence
    {
        // Vẽ chòm sao ra màn hình và GIỮ NGUYÊN khi xong — không tự tan.
        IEnumerator Reveal(ConstellationData constellation);

        // Làm mờ dần rồi dọn sạch những gì Reveal() đã dựng.
        //
        // Thời lượng do BÊN GỌI truyền vào chứ không phải tự tính: thẻ tên phải tan hết đúng cùng
        // khoảnh khắc với chòm sao, mà chỉ bên điều phối mới thấy được cả hai. Nếu mỗi bên tự tính
        // theo trọng số riêng thì Lyra tan trong 0.62s còn Orion 1.24s, tên thì cố định — lệch.
        IEnumerator Dismiss(float duration);
    }
}
