using System;
using System.Collections.Generic;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Managers;
using Assets.SunsetIsland.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.SunsetIsland.Blocks
{
    public struct BlockCompressor
    {
        private static readonly Dictionary<int, Dictionary<CompressionFlag, int[]>> IntervalToBlockMappings =
            new Dictionary<int, Dictionary<CompressionFlag, int[]>>();

        private static readonly Dictionary<int, Dictionary<CompressionFlag, int[]>> BlockToIntervalMappings =
            new Dictionary<int, Dictionary<CompressionFlag, int[]>>();

        private readonly Dictionary<CompressionFlag, int[]> _intervalToBlock;
        private readonly int _chunkSize;
        private readonly Dictionary<CompressionFlag, int[]> _blockToInterval;

        public BlockCompressor(int chunkSize)
        {
            _chunkSize = chunkSize;

            if (!IntervalToBlockMappings.ContainsKey(_chunkSize))
            {
                var count = _chunkSize * _chunkSize * _chunkSize;
                var mappingFunctions = new Dictionary<CompressionFlag, int[]>();
                var bitsPerAxis = (int) Math.Ceiling(Math.Log(chunkSize, 2));
                var hilbertToBlockIndex = new int[count];
                for (uint index = 0; index < count; ++index)
                {
                    var arr = HilbertCurve.HilbertAxes(index, bitsPerAxis);
                    var blockIndex = General.BlockIndex(arr.x, arr.y, arr.z, _chunkSize);
                    hilbertToBlockIndex[index] = blockIndex;
                }
                mappingFunctions[CompressionFlag.Hilbert] = hilbertToBlockIndex;
                foreach (ScanDirection scanDirection in Enum.GetValues(typeof(ScanDirection)))
                {
                    var workCoords = new Vector3Int();
                    var mapping = new int[count];
                    for (var i = 0; i < count; ++i)
                        mapping[GetNextIndex(scanDirection, _chunkSize, ref workCoords)] = i;
                    CompressionFlag compressionFlag;
                    switch (scanDirection)
                    {
                        case ScanDirection.Xyz:
                            compressionFlag = CompressionFlag.LinearXyz;
                            break;
                        case ScanDirection.Xzy:
                            compressionFlag = CompressionFlag.LinearXzy;
                            break;
                        case ScanDirection.Yxz:
                            compressionFlag = CompressionFlag.LinearYxz;
                            break;
                        case ScanDirection.Yzx:
                            compressionFlag = CompressionFlag.LinearYzx;
                            break;
                        case ScanDirection.Zxy:
                            compressionFlag = CompressionFlag.LinearZxy;
                            break;
                        case ScanDirection.Zyx:
                            compressionFlag = CompressionFlag.LinearZyx;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    mappingFunctions[compressionFlag] = mapping;
                }
                IntervalToBlockMappings[chunkSize] = mappingFunctions;
                BlockToIntervalMappings[chunkSize] = new Dictionary<CompressionFlag, int[]>();
                foreach (var pair in IntervalToBlockMappings[chunkSize])
                {
                    var mappingFunction = pair.Value;
                    var inverseFunction = new int[mappingFunction.Length];
                    for (int i = 0; i < mappingFunction.Length; i++)
                        inverseFunction[mappingFunction[i]] = i;
                    BlockToIntervalMappings[chunkSize][pair.Key] = inverseFunction;
                }
            }
            _intervalToBlock = IntervalToBlockMappings[chunkSize];
            _blockToInterval = BlockToIntervalMappings[chunkSize];
        }

        private static int GetNextIndex(ScanDirection direction, int chunkSize, ref Vector3Int workCoords)
        {
            var x = workCoords.x;
            var y = workCoords.y;
            var z = workCoords.z;
            switch (direction)
            {
                case ScanDirection.Xyz:
                    workCoords.x = (x + 1) % chunkSize;
                    if (workCoords.x < x)
                    {
                        workCoords.y = (y + 1) % chunkSize;
                        if (workCoords.y < y)
                            workCoords.z = (z + 1) % chunkSize;
                    }
                    break;
                case ScanDirection.Xzy:
                    workCoords.x = (x + 1) % chunkSize;
                    if (workCoords.x < x)
                    {
                        workCoords.z = (z + 1) % chunkSize;
                        if (workCoords.z < z)
                            workCoords.y = (y + 1) % chunkSize;
                    }
                    break;
                case ScanDirection.Yxz:
                    workCoords.y = (y + 1) % chunkSize;
                    if (workCoords.y < y)
                    {
                        workCoords.x = (x + 1) % chunkSize;
                        if (workCoords.x < x)
                            workCoords.z = (z + 1) % chunkSize;
                    }
                    break;
                case ScanDirection.Yzx:
                    workCoords.y = (y + 1) % chunkSize;
                    if (workCoords.y < y)
                    {
                        workCoords.z = (z + 1) % chunkSize;
                        if (workCoords.z < z)
                            workCoords.x = (x + 1) % chunkSize;
                    }
                    break;
                case ScanDirection.Zxy:
                    workCoords.z = (z + 1) % chunkSize;
                    if (workCoords.z < z)
                    {
                        workCoords.x = (x + 1) % chunkSize;
                        if (workCoords.x < x)
                            workCoords.y = (y + 1) % chunkSize;
                    }
                    break;
                case ScanDirection.Zyx:
                    workCoords.z = (z + 1) % chunkSize;
                    if (workCoords.z < z)
                    {
                        workCoords.y = (y + 1) % chunkSize;
                        if (workCoords.y < y)
                            workCoords.x = (x + 1) % chunkSize;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
            return General.BlockIndex(workCoords, chunkSize);
        }

        [NotNull]
        public IntervalTree<T> Compress<T>([NotNull] T[] items)
        {
            CompressionFlag flag;
            int size;
            EvaluateCompressionMode(items, out flag, out size);
            var min = 0;
            var max = 0;
            var intervals = PoolManager.GetObjectPool<IntervalTree<T>>().Pop();
            intervals.Initialize(_chunkSize, _chunkSize, _blockToInterval[flag], _intervalToBlock[flag]);
            var intervalToBlock = _intervalToBlock[flag];
            var current = items[intervalToBlock[0]];
            for (int i = 1; i < intervalToBlock.Length; ++i)
            {
                var block = items[intervalToBlock[i]];
                if (Equals(block, current))
                {
                    max = i;
                    continue;
                }
                intervals.Add(new Interval<T>(min, max, current));
                current = block;
                min = i;
                max = i;
            }
            intervals.Add(new Interval<T>(min, max, current));
            intervals.ComressionRatio = (items.Length - size) / (float) intervalToBlock.Length;
            intervals.CompressionFlag = flag;
            return intervals;
        }

        private void EvaluateCompressionMode<T>(IList<T> blocks, out CompressionFlag optimal, out int nodeCount)
        {
            var count = _chunkSize * _chunkSize * _chunkSize;
            var maxNodesRemoved = 0;
            optimal = CompressionFlag.LinearXyz;
            nodeCount = 0;
            for (var scanDirection = 0; scanDirection < (int) CompressionFlag.None; scanDirection++)
            {
                var currentNodesRemoved = 0;
                var current = default(T);
                var mappingFunction = _intervalToBlock[(CompressionFlag) scanDirection];
                for (var i = 0; i < count; ++i)
                {
                    var block = blocks[mappingFunction[i]];
                    if (Equals(block, current))
                    {
                        ++currentNodesRemoved;
                        continue;
                    }
                    current = block;
                }
                if (currentNodesRemoved <= maxNodesRemoved)
                    continue;
                maxNodesRemoved = currentNodesRemoved;
                nodeCount = count - maxNodesRemoved;
                optimal = (CompressionFlag) scanDirection;
                //this means we are mostly uniform and can just exit out now!
                if (maxNodesRemoved / (float) count > 0.97f)
                    break;
            }
        }
    }
}