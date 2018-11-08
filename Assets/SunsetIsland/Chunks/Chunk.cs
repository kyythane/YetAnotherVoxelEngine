using System.Collections.Generic;
using System.Linq;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Managers;
using Assets.SunsetIsland.Utilities;
using JetBrains.Annotations;
using SunsetIsland.Blocks;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks
{
    public class Chunk : BaseChunk
    {
        private CompressableArray<IBlock> _compressableArray;
        private Patch[,,] _patches;
        
        public void Initiailize(Vector3Int position, long chunkId)
        {
            Offset = position;
            _compressableArray = PoolManager.GetObjectPool<CompressableArray<IBlock>>().Pop();
            _compressableArray.Initialize(Size.x);
            ChunkId = chunkId;
            Neighbors = PoolManager.GetObjectPool<Dictionary<FaceDirection, IChunk>>().Pop();
            _patches = PoolManager.GetArrayPool<Patch[,,]>(PatchesWide).Pop();
            for (var x = 0; x < PatchesWide; x++)
            for (var y = 0; y < PatchesWide; y++)
            for (var z = 0; z < PatchesWide; z++)
            {
                if(_patches[x, y, z] == null)
                    _patches[x, y, z] = new Patch();
                _patches[x, y, z].Initialize(this, new Vector3Int(x, y, z) * ConfigManager.Properties.PatchSize);
            }
        }

        public override IBlock GetBlockUnchecked(int x, int y, int z)
        {
            return _compressableArray?.Get(x, y, z) ?? BlockFactory.Empty;
        }

        public override void UpdateBlock(int x, int y, int z, IBlock block)
        {
            _compressableArray.Update(x, y, z, block);
            Dirty = true;
            GetPatch(x, y, z).MarkHullDirty(true);
        }

        public override void UpdateBlocks(IEnumerable<BatchUpdateItem<IBlock>> blocks)
        {
            var list = blocks.ToList();
            _compressableArray.Update(list);
            Dirty = true;
            foreach (var item in list)
            {
                GetPatch(item.Position).MarkHullDirty(true);
            }
        }

        public override Patch GetPatch(int x, int y, int z)
        {
            x = MathUtilities.Modulo(x / ConfigManager.Properties.PatchSize, PatchesWide);
            y = MathUtilities.Modulo(y / ConfigManager.Properties.PatchSize, PatchesWide);
            z = MathUtilities.Modulo(z / ConfigManager.Properties.PatchSize, PatchesWide);
            return _patches[x, y, z];
        }

        public override void Evict()
        {
            base.Evict();
            _compressableArray.Evict();
            PoolManager.GetObjectPool<CompressableArray<IBlock>>().Push(_compressableArray);
            _compressableArray = null;
            for (var x = 0; x < PatchesWide; x++)
            for (var y = 0; y < PatchesWide; y++)
            for (var z = 0; z < PatchesWide; z++)
                _patches[x, y, z]?.Evict();
        }
    }
}