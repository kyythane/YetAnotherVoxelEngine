using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Chunks.Processors.Lighting;
using Assets.SunsetIsland.Chunks.Processors.Slices;
using Assets.SunsetIsland.Common;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Managers;
using Assets.SunsetIsland.Utilities;
using SunsetIsland.Blocks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.SunsetIsland.Chunks
{
    public enum ColumnState
    {
        Uninitialized,
        Empty,
        Loading,
        Loaded,
        Dirty,
        Saving,
        Evicting
    }

    public class Column : IEnumerable<IChunk>
    {
        private List<IChunk> _chunks;
        private CompressableArray<uint> _lightData;
        private Dictionary<FaceDirection, Column> _neighbors;

        public Column()
        {
            State = ColumnState.Uninitialized;
        }

        public Vector2Int Offset { get; private set; }
        private int _height;
        public int ChunkSize => ConfigManager.Properties.ChunkSize;
        public int MaxHeight => ConfigManager.Properties.WorldHeight;
        public long ColumnId { get; private set; }

        public ColumnState State { get; set; }

        public IChunk this[int index]
        {
            get
            {
                var y = index / ChunkSize;
                if (y < 0 || y >= _chunks.Count)
                    return default(IChunk);
                return _chunks?[index / ChunkSize];
            }
        }

        public void Initialize(Vector2Int offset, long columnId)
        {
            ColumnId = columnId;
            if (State != ColumnState.Uninitialized)
                throw new AccessViolationException("Attempting to initialize a column in use!");
            Offset = offset;
            _height = 0;
            _chunks = PoolManager.GetObjectPool<List<IChunk>>().Pop();
            _neighbors = PoolManager.GetObjectPool<Dictionary<FaceDirection, Column>>().Pop();
            State = ColumnState.Empty;
        }

        public void SetNeighbor(FaceDirection direction, Column otherColumn)
        {
            if(State == ColumnState.Uninitialized)
                throw new AccessViolationException("Attempting to access an unititialized column!");
            if(direction == FaceDirection.YIncreasing ||
               direction == FaceDirection.YDecreasing)
                throw new ArgumentException($"{nameof(direction)} cannot be of value {direction} for columns");
            var flipped = General.FlipDirection(direction);
            var old = _neighbors.ContainsKey(direction) ? _neighbors[direction] : null;
            _neighbors[direction] = otherColumn;
            if(otherColumn == null)
                return;
            if (otherColumn._neighbors != null)
            {
                otherColumn._neighbors[flipped] = this;
            }
            if(State != ColumnState.Loaded && State != ColumnState.Dirty)
                return;
            if (old != otherColumn)
                State = ColumnState.Dirty;
            if (otherColumn._chunks == null)
                return;
            for (var y = 0; y < _chunks.Count; y++)
            {
                _chunks[y]?.SetNeighbor(direction, otherColumn[y * ChunkSize]);
            }
        }

        public void Load()
        {
            if (State != ColumnState.Empty)
                throw new AccessViolationException("Cannot load into non-empty column!");
            State = ColumnState.Loading;
            //TODO : use a bloom filter to stroe if a column is loaded
            //TODO : query if this column exists in save data and load it!
            //TODO : how many columns per page
            Generate();
        }

        private void Generate()
        {
            SliceManager.Instance.QueueSlice(new ColumnFillSlice(this));
        }

        private void LoadFromDisk(Action<Column> onLoadComplete)
        {
            //TODO : file I/O
        }

        public void Save()
        {
            //TODO : file I/O
        }

        public void Evict()
        {
            if (_chunks == null)
            {
                return;
            }
            Save();
            State = ColumnState.Evicting;
            for (var y = 0; y < _chunks.Count; ++y)
            {
                if (!_chunks[y].Equals(default(IChunk)))
                {
                    var chunk = _chunks[y];
                    chunk.Evict();
                }
                _chunks[y] = default(IChunk);
            }

            if (_lightData != null)
            {
                _lightData.Evict();
                PoolManager.GetObjectPool<CompressableArray<uint>>().Push(_lightData);
            }
            if (_neighbors != null)
            {
                _neighbors.Clear();
                PoolManager.GetObjectPool<Dictionary<FaceDirection, Column>>().Push(_neighbors);
            }
            PoolManager.GetObjectPool<List<IChunk>>().Push(_chunks);
            _lightData = null;
            _chunks = null;
            State = ColumnState.Uninitialized;
        }

        public void BuildColumn(IEnumerable<BatchUpdateItem<LightBlockItem>> fill)
        {
            var lightProcessor = new LightProcessor(ChunkSize, ChunkSize, ChunkSize);
            var chunkPool = PoolManager.GetObjectPool<Chunk>();
            foreach (var batchUpdateItem in fill)
            {
                var pos = batchUpdateItem.Position;
                var chunkIndex = pos.y / ChunkSize;
                IChunk chunk;
                if(chunkIndex < _chunks.Count)
                {
                    chunk = _chunks[chunkIndex];
                }
                else
                {
                    if(batchUpdateItem.Item.Block.Equals(BlockFactory.Empty) || batchUpdateItem.Item.Block.Equals(BlockFactory.BottomOfWorld))
                        continue;
                    var newChunk = chunkPool.Pop();
                    newChunk.Initiailize(new Vector3Int(Offset.x, chunkIndex * ChunkSize, Offset.y), ColumnId + chunkIndex);
                    _chunks.Add(newChunk);
                    chunk = newChunk;
                    _height += ChunkSize;
                    lightProcessor.GrowArray(_height);
                }
                lightProcessor.SetupBufferStep(batchUpdateItem);
                chunk.UpdateBlock(pos.x, pos.y % ChunkSize, pos.z, batchUpdateItem.Item.Block);
            }
            var buffer = lightProcessor.Light(GetLight);
            _lightData = PoolManager.GetObjectPool<CompressableArray<uint>>().Pop();
            _lightData.Initialize(buffer, ChunkSize, _height);
            lightProcessor.Dispose();

            var missingChunks = MaxHeight / ChunkSize - _chunks.Count;
            for (var y = 0; y < missingChunks; ++y)
            {
                var fake = PoolManager.GetObjectPool<FakeChunk>().Pop();
                fake.Initiailize(new Vector3Int(Offset.x, _chunks.Count * ChunkSize, Offset.y), ColumnId + _chunks.Count, BlockFactory.Empty);
                _chunks.Add(fake);
            }

            for (var y = 0; y < _chunks.Count; y++)
            {
                var chunk = _chunks[y];
                chunk.PhysicsState = LoadingState.Empty;
                chunk.BlockDataLoaded = true;
                foreach (var neighbor in _neighbors)
                {
                    chunk.SetNeighbor(neighbor.Key, neighbor.Value[y * ChunkSize]);
                }

                IChunk bottomNeighbor;
                if (y > 0)
                {
                    bottomNeighbor = _chunks[y - 1];
                }
                else
                {
                    var bottomOfWorld = new FakeChunk();
                    bottomOfWorld.Initiailize(Vector3Int.zero, long.MinValue, BlockFactory.BottomOfWorld);
                    bottomOfWorld.SetNeighbor(FaceDirection.XIncreasing, bottomOfWorld);
                    bottomOfWorld.SetNeighbor(FaceDirection.ZIncreasing, bottomOfWorld);
                    bottomNeighbor = bottomOfWorld;
                }
                chunk.SetNeighbor(FaceDirection.YDecreasing, bottomNeighbor);
                
                if (y < _chunks.Count - 1)
                {
                    chunk.SetNeighbor(FaceDirection.YIncreasing, _chunks[y + 1]);
                }
            }
        }
        
        public uint GetLight(int x, int y, int z)
        {
            if (y < 0)
            return 0x0u;

            if (y >= _height)
                return LightProcessor.Sunlight;
            
            var column = this;
            if (x < 0)
            {
                column = column._neighbors.ContainsKey(FaceDirection.XDecreasing)
                    ? column._neighbors[FaceDirection.XDecreasing]
                    : null;
                x = MathUtilities.Modulo(x, ChunkSize);
            }
            else if (x >= ChunkSize)
            {
                column = column._neighbors.ContainsKey(FaceDirection.XIncreasing)
                    ? column._neighbors[FaceDirection.XIncreasing]
                    : null;
                x = MathUtilities.Modulo(x, ChunkSize);
            }

            if (z < 0)
            {
                column =  column?._neighbors.ContainsKey(FaceDirection.ZDecreasing) == true
                    ? column._neighbors[FaceDirection.ZDecreasing]
                    : null;
                z = MathUtilities.Modulo(z, ChunkSize);
            }
            else if (z >= ChunkSize)
            {
                column = column?._neighbors.ContainsKey(FaceDirection.ZIncreasing) == true
                    ? column._neighbors[FaceDirection.ZIncreasing]
                    : null;
                z = MathUtilities.Modulo(z, ChunkSize);
            }

            if (column?._lightData?.Initialized != true)
                return 0x0;
            
            return y >= column?._lightData?.Height ? LightProcessor.Sunlight : column._lightData.Get(x, y, z);
        }

        public IEnumerator<IChunk> GetEnumerator()
        {
            if (_chunks == null || !(State == ColumnState.Loaded || State == ColumnState.Dirty)) yield break;
            foreach (var chunk in _chunks)
            {
                yield return chunk;
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
        
    public struct LightBlockItem
    {
        public uint Light { get; set; }
        public IBlock Block { get; set; }
    }
}