using System;
using System.Collections.Generic;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Chunks.Processors.Meshes;
using Assets.SunsetIsland.Collections;
using Assets.SunsetIsland.Managers;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks
{
    public struct RenderShim : IRenderCell
    {
        private readonly Column _column;
        public Patch Patch { get; }
        private SparseArray3D<LightBlockItem> _items;

        public RenderShim(Column column, Patch patch)
        {
            _column = column;
            Patch = patch;
            _items = PoolManager.GetObjectPool<SparseArray3D<LightBlockItem>>().Pop();
        }

        public Vector3Int Min => Patch.RenderHullMin;
        public Vector3Int Max => Patch.RenderHullMax;
        public RenderMeshData RenderMeshData => Patch.RenderMeshData;
        public uint GetLight(int x, int y, int z)
        {
            if (_items.ContainsKey(x, y, z))
                return _items[x, y, z].Light;
            var item = GetItem(x, y, z); 
            _items[x, y, z] = item;
            return item.Light;
        }
        
        public uint GetLight(Vector3Int position)
        {
            return GetLight(position.x, position.y, position.z);
        }
        
        public IBlock GetBlock(int x, int y, int z)
        {
            if (_items.ContainsKey(x, y, z))
                return _items[x, y, z].Block;
            var item = GetItem(x, y, z); 
            _items[x, y, z] = item;
            return item.Block;
        }

        private LightBlockItem GetItem(int x, int y, int z)
        {
            return new LightBlockItem()
            {
                Light = _column.GetLight(x + Patch.ChunkOffset.x, y + Patch.Offset.y, z + Patch.ChunkOffset.z),
                Block = Patch.GetBlockWithBoundCheck(x, y, z),
            };
        }

        public IBlock GetBlock(Vector3Int position)
        {
            return Patch.GetBlockWithBoundCheck(position.x, position.y, position.z);
        }

        public void Dispose()
        {
            _items.Clear();
            PoolManager.GetObjectPool<SparseArray3D<LightBlockItem>>().Push(_items);
        }
    }
}