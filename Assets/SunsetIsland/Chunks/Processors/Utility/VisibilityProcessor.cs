using System.Collections.Generic;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Managers;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks.Processors.Utility
{
    public struct VisibilityProcessor
    {
        private const int Solid = -1;

        public void Process(IVisibilityCell cell)
        {
            var connSets = PoolManager.GetObjectPool<Dictionary<FaceDirection, HashSet<int>>>().Pop();
            if (connSets.Count > 0)
            {
                foreach (var connSet in connSets)
                {
                    connSet.Value.Clear();
                }
            }
            else
            {
                connSets[FaceDirection.XIncreasing] = new HashSet<int>();
                connSets[FaceDirection.YIncreasing] = new HashSet<int>();
                connSets[FaceDirection.ZIncreasing] = new HashSet<int>();
                connSets[FaceDirection.XDecreasing] = new HashSet<int>();
                connSets[FaceDirection.YDecreasing] = new HashSet<int>();
                connSets[FaceDirection.ZDecreasing] = new HashSet<int>();
            }

            var minFilled = cell.SolidHullMin;
            var maxFilled = cell.SolidHullMax;

            var connectivity = cell.Connectivity;
            connectivity.Clear();
            
            var maxColor = 0;
            var bufferPool = PoolManager.GetArrayPool<int[]>(cell.Size.x * cell.Size.z);
            var bufferLast = bufferPool.Pop();
            var bufferCurrent = bufferPool.Pop();
            for (var i = 0; i < bufferLast.Length; i++)
            {
                bufferLast[i] = Solid;
                bufferCurrent[i] = Solid;
            }

            for (var y = cell.Size.y - 1; y >= 0; --y)
            {
                if (maxFilled.y < y)
                {
                    for (var z = 0; z < cell.Size.z; ++z)
                    for (var x = 0; x < cell.Size.x; ++x)
                    {
                        var index = x + z * cell.Size.x;
                        var color = y < cell.Size.y - 1 ? bufferLast[index] : maxColor;
                        SetCell(x, y, z, cell.Size, color, bufferCurrent, connSets);
                    }
                }
                else
                {
                    var color = maxColor;
                    for (var z = 0; z < cell.Size.z; ++z)
                    for (var x = 0; x < cell.Size.x; ++x)
                    {
                        var index = x + z * cell.Size.x;
                        if (x < minFilled.x || z < minFilled.z)
                        {
                            SetCell(x, y, z, cell.Size, color, bufferCurrent, connSets);
                            continue;
                        }

                        if (x < maxFilled.x && z < maxFilled.z && cell.GetBlockUnchecked(x, y, z).RenderOpaque)
                        {
                            bufferCurrent[index] = Solid;
                            continue;
                        }

                        var previousXColor = x > 0 ? bufferCurrent[index - 1] : Solid;
                        var previousZColor = z > 0 ? bufferCurrent[index - cell.Size.x] : Solid;
                        var previousYColor = y < cell.Size.y - 1 ? bufferLast[index] : Solid;

                        if (previousXColor == Solid)
                        {
                            if (previousZColor == Solid)
                            {
                                if (previousYColor == Solid)
                                {
                                    SetCell(x, y, z, cell.Size, ++maxColor, bufferCurrent, connSets);  //x solid, y solid, z solid
                                    continue;
                                }
                                SetCell(x, y, z, cell.Size, previousYColor, bufferCurrent, connSets); // x solid, y pass, z solid
                                continue;
                            }

                            if (previousYColor == Solid)
                            {
                                SetCell(x, y, z, cell.Size, previousZColor, bufferCurrent, connSets); //x solid, y solid, z pass
                                continue;
                            }

                            color = previousYColor < previousZColor ? previousZColor : previousYColor;
                                    
                            SetCell(x, y, z, cell.Size, color, bufferCurrent, connSets); //x solid, y pass, z pass
                        }
                        else
                        {
                            if (previousZColor == Solid)
                            {
                                if (previousYColor == Solid)
                                {
                                    SetCell(x, y, z, cell.Size, previousXColor, bufferCurrent, connSets);//x pass, y solid, z solid
                                    continue;
                                }

                                color = previousYColor < previousXColor ? previousYColor : previousXColor;
                                SetCell(x, y, z, cell.Size, color, bufferCurrent, connSets); //x pass, y pass, z solid
                            }
                            else
                            {
                                if (previousYColor == Solid)
                                {
                                    color = previousZColor < previousXColor ? previousZColor : previousXColor;
                                    SetCell(x, y, z, cell.Size, color, bufferCurrent, connSets); //x pass, y solid, z pass
                                }
                                else
                                {
                                    if (previousYColor < previousZColor && previousYColor < previousXColor)
                                    {
                                        color = previousYColor;
                                    }
                                    else if (previousZColor < previousXColor)
                                    {
                                        color = previousZColor;
                                    }
                                    else
                                    {
                                        color = previousXColor;
                                    }

                                    SetCell(x, y, z, cell.Size, color, bufferCurrent, connSets); //x pass, y pass, z pass
                                }
                            }
                        }
                    }
                }

                var temp = bufferLast;
                bufferLast = bufferCurrent;
                bufferCurrent = temp;
            }

            bufferPool.Push(bufferLast);
            bufferPool.Push(bufferCurrent);

            foreach (var connSet in connSets)
            {
                for (int i = 0; i < (int)FaceDirection.None; i++)
                {
                    var face = (FaceDirection) i;
                    if (face == connSet.Key)
                        continue;
                    foreach (var color in connSet.Value)
                    {
                        if(connSets[face].Contains(color))
                            connectivity.Add(connSet.Key, face);
                    }
                }
            }
            PoolManager.GetObjectPool<Dictionary<FaceDirection, HashSet<int>>>().Push(connSets);
            
        }

        private void SetCell(int x, int y, int z, Vector3Int size, int color, int[] buffer, Dictionary<FaceDirection, HashSet<int>> connectivity)
        {
            var index = x + z * size.x;
            buffer[index] = color;
            if (x == 0)
            {
                connectivity[FaceDirection.XDecreasing].Add(color);
            }
            else if (x == size.x - 1)
            {
                connectivity[FaceDirection.XIncreasing].Add(color);
            }

            if (y == 0)
            {
                connectivity[FaceDirection.YDecreasing].Add(color);
            }
            else if (y == size.y - 1)
            {
                connectivity[FaceDirection.YIncreasing].Add(color);
            }
            
            if (z == 0)
            {
                connectivity[FaceDirection.ZDecreasing].Add(color);
            }
            else if (z == size.z - 1)
            {
                connectivity[FaceDirection.ZIncreasing].Add(color);
            }
        }
    }
}