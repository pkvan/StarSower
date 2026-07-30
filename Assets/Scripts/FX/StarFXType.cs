namespace StarSower.FX
{
    // Khoa tra cuu pool. Dung enum thay vi chuoi: tra cuu bang chi so mang, khong bam chuoi,
    // khong cap phat, va go nham ten thi bao loi luc bien dich chu khong phai luc chay.
    //
    // Thu tu KHONG duoc doi tuy tien — StarFXPool danh chi so mang theo dung thu tu nay.
    public enum StarFXType
    {
        Flash = 0,
        Burst01 = 1,
        Burst02 = 2,
        Burst03 = 3,
        FlyCore = 4,
        Trail01 = 5,
        Trail02 = 6,
        Trail03 = 7,
        Dust01 = 8,
        Dust02 = 9,
        Dust03 = 10,
        Sparkle01 = 11,
        Sparkle02 = 12,
        Sparkle03 = 13,
        PocketGlow = 14,
        PocketBurst = 15,
        Ring = 16,
    }

    public static class StarFXTypeInfo
    {
        public const int Count = 17;
    }
}
