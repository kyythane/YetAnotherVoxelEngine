using System.Collections.Generic;
using Assets.SunsetIsland.Blocks;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks.Processors.Meshes
{
    public interface IRenderCell
    {
        RenderMeshData RenderMeshData { get; }
        Vector3Int Min { get; }
        Vector3Int Max { get; }
        uint GetLight(int x, int y, int z);
        uint GetLight(Vector3Int position);
        IBlock GetBlock(int x, int y, int z);
        IBlock GetBlock(Vector3Int position);
    }
}