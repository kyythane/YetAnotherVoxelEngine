using System;
using System.Collections.Generic;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Managers;
using Assets.SunsetIsland.Utilities;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks
{
    public abstract class BaseChunk : IChunk
    {
        private ChunkPhysicsTarget _physicsTarget;
        private PhysicsMeshData _physicsMeshData;

        public PhysicsMeshData PhysicsMeshData
        {
            get
            {
                if (_physicsMeshData == null)
                {
                    _physicsMeshData = PoolManager.GetObjectPool<PhysicsMeshData>().Pop();
                    _physicsMeshData.Clear();
                }
                return _physicsMeshData;
            }
        }
       
        public MeshCollider MeshCollider => _physicsTarget?.MeshCollider;

        public Vector3Int Offset { get; protected set; }
        public long ChunkId { get; set; }
        public Vector3Int Size { get; } = new Vector3Int(ConfigManager.Properties.ChunkSize,
            ConfigManager.Properties.ChunkSize, ConfigManager.Properties.ChunkSize);
        public bool BlockDataLoaded { get; set; }

        public LoadingState PhysicsState { get; set; }
        public bool PhysicsDirty => PhysicsState == LoadingState.Dirty || PhysicsState == LoadingState.Empty;
        
        public virtual bool Dirty
        {
            get
            {
                return PhysicsDirty;
            }
            protected set
            {
                if(!value) return;
                if (PhysicsState == LoadingState.Loaded)
                    PhysicsState = LoadingState.Dirty;
            }
        }

        public void AttachPhysicsTarget(ChunkPhysicsTarget target)
        {
            _physicsTarget = target;
        }

        public bool HasPhysicsTarget => _physicsTarget != null;

        public Dictionary<FaceDirection, IChunk> Neighbors { get; protected set; }
        public bool NeighborsSet => Neighbors != null &&
                                    Neighbors.ContainsKey(FaceDirection.XIncreasing) &&
                                    Neighbors[FaceDirection.XIncreasing]?.BlockDataLoaded == true &&
                                    Neighbors.ContainsKey(FaceDirection.XDecreasing) &&
                                    Neighbors[FaceDirection.XDecreasing]?.BlockDataLoaded == true &&
                                    Neighbors.ContainsKey(FaceDirection.ZIncreasing) &&
                                    Neighbors[FaceDirection.ZIncreasing]?.BlockDataLoaded == true &&
                                    Neighbors.ContainsKey(FaceDirection.ZDecreasing) &&
                                    Neighbors[FaceDirection.ZDecreasing]?.BlockDataLoaded == true;

        public IBlock GetBlockWithBoundCheck(Vector3 position)
        {
            return GetBlockWithBoundCheck((int) position.x, (int) position.y, (int) position.z);
        }

        public IBlock GetBlockWithBoundCheck(Vector3Int position)
        {
            return GetBlockWithBoundCheck(position.x, position.y, position.z);
        }

        public abstract IBlock GetBlockUnchecked(int x, int y, int z);

        public IBlock GetBlockWithBoundCheck(int x, int y, int z)
        {
            var chunk = (IChunk)this;
            if (x < 0)
            {
                chunk = chunk.Neighbors.ContainsKey(FaceDirection.XDecreasing)
                    ? chunk.Neighbors[FaceDirection.XDecreasing]
                    : null;
                x = MathUtilities.Modulo(x, Size.x);
            }
            else if (x >= Size.x)
            {
                chunk = chunk.Neighbors.ContainsKey(FaceDirection.XIncreasing)
                    ? chunk.Neighbors[FaceDirection.XIncreasing]
                    : null;
                x = MathUtilities.Modulo(x, Size.x);
            }
            
            if (y < 0)
            {
                chunk = chunk != null && chunk.Neighbors.ContainsKey(FaceDirection.YDecreasing)
                    ? chunk.Neighbors[FaceDirection.YDecreasing]
                    : null;
                y = MathUtilities.Modulo(y, Size.y);
            }
            else if (y >= Size.y)
            {
                chunk = chunk != null && chunk.Neighbors.ContainsKey(FaceDirection.YIncreasing)
                    ? chunk.Neighbors[FaceDirection.YIncreasing]
                    : null;
                y = MathUtilities.Modulo(y, Size.y);
            }

            if (z < 0)
            {
                chunk = chunk != null && chunk.Neighbors.ContainsKey(FaceDirection.ZDecreasing)
                    ? chunk.Neighbors[FaceDirection.ZDecreasing]
                    : null;
                z = MathUtilities.Modulo(z, Size.z);
            }
            else if (z >= Size.z)
            {
                chunk = chunk != null && chunk.Neighbors.ContainsKey(FaceDirection.ZIncreasing)
                    ? chunk.Neighbors[FaceDirection.ZIncreasing]
                    : null;
                z = MathUtilities.Modulo(z, Size.z);
            }

            return chunk?.GetBlockUnchecked(x, y, z) ?? BlockFactory.Empty;
        }

        public void UpdateBlock(Vector3 position, IBlock block)
        {
            UpdateBlock((int) position.x, (int) position.y, (int) position.z, block);
        }

        public void UpdateBlock(Vector3Int position, IBlock block)
        {
            UpdateBlock(position.x, position.y, position.z, block);
        }
        public virtual void UpdateBlock(int x, int y, int z, IBlock block){ }
        public virtual void UpdateBlocks(IEnumerable<BatchUpdateItem<IBlock>> blocks){ }

        public void SetNeighbor(FaceDirection direction, IChunk chunk)
        {
            Neighbors[direction] = chunk;
            Dirty = true;
            if (chunk?.Neighbors == null) 
                return;
            try
            {
                chunk.Neighbors[General.FlipDirection(direction)] = this; //TODO WHAT THE FUCK! FUCK THREADS
            }
            catch (Exception e)
            {
                Debug.LogError($"NPE: {chunk?.GetType()} {chunk?.ChunkId}");
            }
        }

        public Patch GetPatch(Vector3Int position)
        {
            return GetPatch(position.x, position.y, position.z);
        }

        public abstract Patch GetPatch(int x, int y, int z);
        public int PatchesWide { get; } = ConfigManager.Properties.ChunkSize / ConfigManager.Properties.PatchSize;
        
        public virtual void Evict()
        {
            Neighbors.Clear();
            PoolManager.GetObjectPool<Dictionary<FaceDirection, IChunk>>().Push(Neighbors);
            PoolManager.GetMonoBehaviourPool(ConfigManager.UnityProperties.ChunkPhysicsTarget).Push(_physicsTarget);
            _physicsTarget = null;
        }
        
    }
}