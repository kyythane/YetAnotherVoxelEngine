using System.Collections.Generic;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Chunks.Processors.Lighting;
using Assets.SunsetIsland.Common;
using Assets.SunsetIsland.Managers;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks.Processors.Generation
{
    public struct ColumnFiller
    {

        public void Process(Column column)
        {
            column.BuildColumn(Fill(column));
        }

        private IEnumerable<BatchUpdateItem<LightBlockItem>> Fill(Column column)
        {
            var ground = BlockFactory.CreateBlock(BlockFactory.BlockDictionary.GetBlockIdForName("Grass"));
            var snow = BlockFactory.CreateBlock(BlockFactory.BlockDictionary.GetBlockIdForName("Snow"));
            var trunk = BlockFactory.CreateBlock(BlockFactory.BlockDictionary.GetBlockIdForName("TreeTrunk"));
            var leaves = BlockFactory.CreateBlock(BlockFactory.BlockDictionary.GetBlockIdForName("Leaves"));
            var dirt = BlockFactory.CreateBlock(BlockFactory.BlockDictionary.GetBlockIdForName("Dirt"));
            var rock = BlockFactory.CreateBlock(BlockFactory.BlockDictionary.GetBlockIdForName("Stone"));
            var trees = GenerationManager.GetPlants(new Vector4(column.Offset.x - 8, column.Offset.y - 8,
                                                                column.Offset.x + column.ChunkSize + 8,
                                                                column.Offset.y + column.ChunkSize + 8));
            var columnBuffer = PoolManager.GetArrayPool<LightBlockItem[]>(column.MaxHeight).Pop();
            for (var x = 0; x < column.ChunkSize; ++x)
            {
                for (var z = 0; z < column.ChunkSize; ++z)
                {
                    var threshold = GenerationManager.GetHeight(column.Offset.x + x, column.Offset.y + z);
                    var distance = ToClosestTree(trees, new Vector2Int(x + column.Offset.x, z + column.Offset.y));
                    var sunLight = LightProcessor.ToLight(Colors.Black, Colors.White);
                    for (var height = column.MaxHeight - 1; height >= 0 ; --height)
                    {
                        IBlock block;
                        if (height > threshold)
                            if (distance.x <= 1 && height - distance.y <= 16)
                            {
                                sunLight = 0x0u; 
                                block = trunk;
                            }
                            else if (distance.x <= 4 && height - distance.y <= 18 && height - distance.y >= 8)
                            {
                                sunLight = 0x0u;
                                block = leaves;
                            }
                            else
                            {
                                block = BlockFactory.Empty;
                            }
                        else if (height == threshold)
                        {
                            sunLight = 0x0u;
                            block = ground;
                        }
                        else if (height > threshold - 2)
                        {
                            sunLight = 0x0u;
                            block = dirt;
                        }
                        else
                        {
                            sunLight = 0x0u;
                            block = rock;
                        }
                        columnBuffer[height] = new LightBlockItem
                        {
                             Block = block,
                             Light = sunLight
                        };
                    }

                    for (var y = 0; y < column.MaxHeight; ++y)
                    {
                        var bufferItem = columnBuffer[y];
                        yield return new BatchUpdateItem<LightBlockItem>(new Vector3Int(x, y, z), bufferItem);
                    }
                }
            }
        }

        private Vector2 ToClosestTree(List<Vector3Int> trees, Vector2Int vector2Int)
        {
            var minDistance = float.MaxValue;
            var treeHeight = 0;
            foreach (var tree in trees)
            {
                var dx = tree.x - vector2Int.x;
                var dy = tree.z - vector2Int.y;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    treeHeight = tree.y;
                }
            }
            return new Vector2(minDistance, treeHeight);
        }
    }
}