using System.Collections.Generic;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Managers;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks
{
    public class FakeChunk : BaseChunk
    {
        protected IBlock _block;
        protected Patch _patch;

        public override bool Dirty
        {
            get { return false; }
            protected set { }
        }

        public void Initiailize( Vector3Int position, long chunkId, IBlock block)
        {
            Offset = position;
            _block = block;
            Neighbors = PoolManager.GetObjectPool<Dictionary<FaceDirection, IChunk>>().Pop();
            _patch = PoolManager.GetObjectPool<Patch>().Pop(); 
            _patch.Initialize(this, Offset);
            if (!block.RenderOpaque)
            {
                _patch.Connectivity.Add(FaceDirection.XIncreasing, FaceDirection.XDecreasing);
                _patch.Connectivity.Add(FaceDirection.YIncreasing, FaceDirection.YDecreasing);
                _patch.Connectivity.Add(FaceDirection.ZIncreasing, FaceDirection.ZDecreasing);
                
                _patch.Connectivity.Add(FaceDirection.YIncreasing, FaceDirection.XIncreasing);
                _patch.Connectivity.Add(FaceDirection.YIncreasing, FaceDirection.ZIncreasing);
                _patch.Connectivity.Add(FaceDirection.YIncreasing, FaceDirection.ZDecreasing);
                _patch.Connectivity.Add(FaceDirection.YIncreasing, FaceDirection.XDecreasing);
                
                _patch.Connectivity.Add(FaceDirection.YDecreasing, FaceDirection.XIncreasing);
                _patch.Connectivity.Add(FaceDirection.YDecreasing, FaceDirection.ZIncreasing);
                _patch.Connectivity.Add(FaceDirection.YDecreasing, FaceDirection.ZDecreasing);
                _patch.Connectivity.Add(FaceDirection.YDecreasing, FaceDirection.XDecreasing);
            }
            else
            {
                _patch.Connectivity.Clear();
            }
            _patch.MarkHullDirty(false);
            _patch.RenderState = LoadingState.Loaded;
            _patch.VisGraphState = LoadingState.Loaded;
            PhysicsState = LoadingState.Loaded;
        }

        public override IBlock GetBlockUnchecked(int x, int y, int z)
        {
            return _block;
        }

        public override Patch GetPatch(int x, int y, int z)
        {
            return _patch;
        }

        public override void Evict()
        {
            PoolManager.GetObjectPool<Patch>().Push(_patch);
            _patch = null;
        }
    }
}