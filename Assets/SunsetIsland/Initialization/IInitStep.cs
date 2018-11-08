using System.Collections;

namespace Assets.SunsetIsland.Initialization
{
    public interface IInitStep
    {
        string Name { get; }
        IEnumerator Execute();
    }
}