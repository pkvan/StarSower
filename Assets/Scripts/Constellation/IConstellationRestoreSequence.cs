using System.Collections;

namespace StarSower.Constellations
{
    // Kiểu trình diễn "khôi phục chòm sao" (bầu trời tối lại, các sao sáng lên, nối nét, chòm sao
    // hiện ra). Tách interface theo đúng cách ITransitionEffect đã làm cho transition: bên điều phối
    // chỉ biết "phát rồi chờ xong", không biết nó vẽ bằng gì. Khi có art/Timeline thật, chỉ cần gán
    // component khác vào ô restoreSequenceSource — không sửa dòng code điều phối nào.
    public interface IConstellationRestoreSequence
    {
        // Phát trình tự khôi phục cho 1 chòm sao. Bên gọi yield return để chờ hết trình tự.
        IEnumerator Play(ConstellationData constellation);
    }
}
