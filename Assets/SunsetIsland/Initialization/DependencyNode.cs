namespace Assets.SunsetIsland.Initialization
{
    public interface IDependencyNode<in T>
    {
        void DependsOn(params T[] dependencies);
    }
}