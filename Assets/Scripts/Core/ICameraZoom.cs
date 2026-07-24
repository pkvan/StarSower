namespace StarSower.Core
{
    // API zoom camera dùng chung cho các hệ thống khác (Boss, Combo, Event) gọi tới sau này.
    public interface ICameraZoom
    {
        void ZoomTo(float targetOrthographicSize, float duration);
        void ResetZoom(float duration);
    }
}
