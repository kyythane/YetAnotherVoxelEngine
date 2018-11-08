using System.Collections.Concurrent;
using System.Collections.Generic;
using Assets.SunsetIsland.Managers;
using Assets.SunsetIsland.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.SunsetIsland.Blocks
{
    public struct ColumnCompressor
    {
        private static readonly ConcurrentDictionary<Vector3Int, int[]> IntervalToBlockMappings =
            new ConcurrentDictionary<Vector3Int, int[]>();

        private static readonly ConcurrentDictionary<Vector3Int, int[]> BlockToIntervalMappings =
            new ConcurrentDictionary<Vector3Int, int[]>();
        public int[] BlockToInterval { get; }
        public int[] IntervalToBlock { get; }

        private readonly int _width;
        private readonly int _height;

        public ColumnCompressor(int width, int height)
        {
            _width = width;
            _height = height;
            var key = new Vector3Int(_width, _height, _width);
            if (!IntervalToBlockMappings.ContainsKey(key))
            {
                var count = _width * _height * _width;
                var map = new int[count];
                var inverse = new int[count];
                var index = 0;
                var flipY = false;
                var flipZ = false;
                for (var x = 0; x < _width; x++)
                {
                    int incZ, z, limZ;
                    if (flipZ)
                    {
                        incZ = -1;
                        z = _width - 1;
                        limZ = 0;
                    }
                    else
                    {
                        incZ = 1;
                        z = 0;
                        limZ = _width;
                    }

                    while ((flipZ && z >= limZ) || (!flipZ && z < limZ))
                    {
                        int incY, y, limY;
                        if (flipY)
                        {
                            incY = 1;
                            y = 0;
                            limY = _height;
                        }
                        else
                        {
                            incY = -1;
                            y = _height - 1;
                            limY = 0;
                        }

                        while ((flipY && y < limY) || (!flipY && y >= limY))
                        {
                            var blockIndex = General.BlockIndex(x, y, z, _width, _height);
                            map[index] = blockIndex;
                            inverse[blockIndex] = index;
                            ++index;
                            y += incY;
                        }

                        z += incZ;
                        flipY = !flipY;
                    }

                    flipZ = !flipZ;
                }

                IntervalToBlockMappings.TryAdd(key, map);
                BlockToIntervalMappings.TryAdd(key, inverse);
            }

            IntervalToBlock = IntervalToBlockMappings[key];
            BlockToInterval = BlockToIntervalMappings[key];
        }

        public IntervalTree<T> Compress<T>([NotNull] T[] items)
        {
            var added = 0;
            var min = 0;
            var max = 0;
            var current = items[IntervalToBlock[0]];
            var intervals = PoolManager.GetObjectPool<IntervalTree<T>>().Pop();
            intervals.Initialize(_height, _width, BlockToInterval, IntervalToBlock);
            for (var i = 1; i < IntervalToBlock.Length; ++i)
            {
                var block = items[IntervalToBlock[i]];
                if (Equals(block, current))
                {
                    max = i;
                    continue;
                }
                intervals.Add(new Interval<T>(min, max, current));
                current = block;
                min = i;
                max = i;
                ++added;
            }
            intervals.Add(new Interval<T>(min, max, current));
            ++added;
            intervals.ComressionRatio = (items.Length - added) / (float) IntervalToBlock.Length;
            return intervals;
        }
    }
}