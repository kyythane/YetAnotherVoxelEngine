namespace Assets.SunsetIsland.Chunks
{
    public enum LoadingState
    {
        Uninitialized,
        Empty,
        Loading,
        Loaded,
        Dirty,
        Evicting,
    }
}