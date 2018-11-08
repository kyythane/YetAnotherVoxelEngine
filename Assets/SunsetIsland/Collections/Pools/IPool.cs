namespace Assets.SunsetIsland.Collections.Pools
{
    public interface IPool
    {
        int Count { get; }
    }

    public interface IPool<T> : IPool
    {
        T Pop();
        bool Push(T item);
        void Evict();
    }
}