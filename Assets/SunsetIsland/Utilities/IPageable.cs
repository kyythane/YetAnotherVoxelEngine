namespace Assets.SunsetIsland.Utilities
{
    public interface IPageable<T>
    {
        T PageId { get; }
    }
}