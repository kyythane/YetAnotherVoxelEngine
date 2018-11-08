using System.Collections.Generic;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Common.Enums;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks
{
    public interface IChunk 
    {
        long ChunkId { get; set; }
        Vector3Int Size { get; }
        Vector3Int Offset { get; }
        bool BlockDataLoaded { get; set; }

        int PatchesWide { get; }
        Patch GetPatch(Vector3Int pos);
        Patch GetPatch(int x, int y, int z);
        
        PhysicsMeshData PhysicsMeshData { get; }
        MeshCollider MeshCollider { get; }
        LoadingState PhysicsState { get; set; }
        bool PhysicsDirty { get; }
        bool HasPhysicsTarget { get; }
        void AttachPhysicsTarget(ChunkPhysicsTarget target);
        
        void Evict();
        
        IBlock GetBlockWithBoundCheck(Vector3 position);
        IBlock GetBlockWithBoundCheck(Vector3Int position);
        IBlock GetBlockWithBoundCheck(int x, int y, int z);
        IBlock GetBlockUnchecked(int x, int y, int z);

        void UpdateBlock(Vector3 position, IBlock block);
        void UpdateBlock(Vector3Int position, IBlock block);
        void UpdateBlock(int x, int y, int z, IBlock block);
        void UpdateBlocks(IEnumerable<BatchUpdateItem<IBlock>> blocks);
        
        bool NeighborsSet { get; }
        void SetNeighbor(FaceDirection direction, IChunk chunk);
        
        Dictionary<FaceDirection, IChunk> Neighbors { get; }
    }
}