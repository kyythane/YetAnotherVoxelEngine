using Assets.SunsetIsland.Blocks;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks
{
    public interface IVisibilityCell
    {
        VisibilityData Connectivity { get; }
        Vector3Int SolidHullMax { get;}
        Vector3Int SolidHullMin { get;}
        Vector3Int Size { get; }
        IBlock GetBlockUnchecked(int x, int y, int z);
    }   
}