using UnityEngine;

namespace StarSower.FX
{
    // Ben nhan bao "ngoi sao da bay toi noi". Tach interface de StarFlyAnimator khong con buoc
    // cung vao StarCollectEffect — nho vay doan phim khoi phuc chom sao dung lai duoc y nguyen
    // phan Bezier + duoi + bui + lap lanh ma khong phai nhan ban mot ban thu hai.
    //
    // Truyen ca 'source' de mot ben nghe co the theo doi NHIEU ngoi sao bay cung luc: doan phim
    // chom sao ban 5 sao gan nhu dong thoi, moi sao toi mot node khac nhau, nen chi bao "co sao
    // toi roi" la khong du. Doc source.Tag ra biet la sao nao.
    public interface IStarFlightListener
    {
        void OnStarFlightArrived(StarFlyAnimator source, Vector3 position);
    }
}
