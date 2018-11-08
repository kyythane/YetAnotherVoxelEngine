namespace Assets.SunsetIsland.Common.Enums
{
    public enum ScanDirection
    {
        Xyz,
        Xzy,
        Yxz,
        Yzx,
        Zxy,
        Zyx
    }

    public enum CompressionFlag
    {
        LinearXyz,
        LinearXzy,
        LinearYxz,
        LinearYzx,
        LinearZxy,
        LinearZyx,
        Hilbert,
        None
    }
}