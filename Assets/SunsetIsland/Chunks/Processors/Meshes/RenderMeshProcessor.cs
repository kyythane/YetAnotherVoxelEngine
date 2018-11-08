using System.Collections.Generic;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Chunks.Processors.Lighting;
using Assets.SunsetIsland.Collections;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Managers;
using Assets.SunsetIsland.Utilities;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks.Processors.Meshes
{
    public struct RenderMeshProcessor : IRenderMeshProcessor
    {
        private IRenderCell _cell;
        private Dictionary<FaceDirection, SparseArray3D<BlockFace>> _masks;
        private Vector3Int _min, _max;

        public void Process(IRenderCell cell)
        {
            _cell = cell;
            _cell.RenderMeshData.Clear();
            var maskPool = PoolManager.GetObjectPool<Dictionary<FaceDirection, SparseArray3D<BlockFace>>>();
            _masks = maskPool.Pop();
            if (_masks.Count == 0)
            {
                _masks[FaceDirection.XIncreasing] = new SparseArray3D<BlockFace>();
                _masks[FaceDirection.YIncreasing] = new SparseArray3D<BlockFace>();
                _masks[FaceDirection.ZIncreasing] = new SparseArray3D<BlockFace>();
                _masks[FaceDirection.XDecreasing] = new SparseArray3D<BlockFace>();
                _masks[FaceDirection.YDecreasing] = new SparseArray3D<BlockFace>();
                _masks[FaceDirection.ZDecreasing] = new SparseArray3D<BlockFace>();
            }
            BuildMasks();
            if (_max.x < _min.x)
            {
                return;
            }
            for (var side = 0; side < 6; side++)
            {
                var backside = side > 2;
                var axis = side % 3;

                var u = (axis + 1) % 3;
                var v = (axis + 2) % 3;

                var mask = _masks[General.FlipDirection((FaceDirection) side)];

                var axisPos =  backside ? _max[axis] - 1 : _min[axis];
                while (axisPos < _max[axis] && axisPos >= _min[axis])
                {
                    for (var j = _min[v]; j < _max[v]; j++)
                    {
                        for (var i = _min[u]; i < _max[u];)
                        {
                            var maskPosition = new Vector3Int()
                            {
                                [u] = i,
                                [v] = j,
                                [axis] = axisPos
                            };
                            if (!mask.ContainsKey(maskPosition))
                            {
                                i++;
                                continue;
                            }
                            
                            var width = 1;
                            var blockFace = mask[maskPosition];
                            while (i + width < _max[u])
                            {
                                var nextPos = maskPosition;
                                nextPos[u] += width;
                                if (blockFace != mask[nextPos])
                                    break;
                                width++;
                            }
                            int height;
                            for (height = 1; j + height < _max[v]; height++)
                            {
                                for (var k = 0; k < width; k++)
                                {
                                    var nextPos = maskPosition;
                                    nextPos[u] += k;
                                    nextPos[v] += height;
                                    if(mask[nextPos] == blockFace)
                                        continue;
                                    goto heightFound;
                                }
                            }

                            heightFound:

                            var offset = axisPos + (backside ? 1 : 0);

                            var topLeft = new Vector3
                                          {
                                              [u] = i + width,
                                              [v] = j,
                                              [axis] = offset
                                          } ;
                            var topRight = new Vector3
                                           {
                                               [u] = i + width,
                                               [v] = j + height,
                                               [axis] = offset
                                           } ;
                            var bottomRight = new Vector3
                                              {
                                                  [u] = i,
                                                  [v] = j + height,
                                                  [axis] = offset
                                              } ;

                            var bottomLeft = new Vector3
                                             {
                                                 [u] = i,
                                                 [v] = j,
                                                 [axis] = offset
                                             } ;

                            _cell.RenderMeshData.AddQuad(topLeft, topRight, bottomRight, bottomLeft, height, width, blockFace, side);

                            for (var l = 0; l < height; ++l)
                            {
                                for (var k = 0; k < width; ++k)
                                {
                                    var remove = maskPosition;
                                    remove[u] += k;
                                    remove[v] += l;
                                    mask.Remove(remove);
                                }
                            }

                            i += width;
                        }
                    }
                    axisPos += backside ? -1 : 1;
                }
            }
            foreach (var maskPair in _masks)
            {
                if(maskPair.Value.Count > 0) 
                    maskPair.Value.Clear();
            }
            maskPool.Push(_masks);
        }

        private void BuildMasks()
        {
            _min = new Vector3Int(int.MaxValue,int.MaxValue,int.MaxValue);
            _max = new Vector3Int(int.MinValue,int.MinValue,int.MinValue);
            for (var x = _cell.Min.x; x < _cell.Max.x; x++)
            {
                for (var z = _cell.Min.z; z < _cell.Max.z; z++)
                {
                    for (var y = _cell.Min.y; y < _cell.Max.y; y++)
                    {
                        var block = _cell.GetBlock(x, y, z);
                        if (!block.AddToRenderMesh)
                            continue;
                        var added = false;
                        var blockLight = _cell.GetLight(x, y, z);
                        var blockId = block.BlockId;
                        var block100 = _cell.GetBlock(x + 1, y, z);
                        var block010 = _cell.GetBlock(x, y + 1, z);
                        var block001 = _cell.GetBlock(x, y, z + 1);
                        var blockN100 = _cell.GetBlock(x - 1, y, z);
                        var block0N10 = _cell.GetBlock(x, y - 1, z);
                        var block00N1 = _cell.GetBlock(x, y, z - 1);
                        
                        
                        var light100 = _cell.GetLight(x + 1, y, z);
                        var light010 = _cell.GetLight(x, y + 1, z);
                        var light001 = _cell.GetLight(x, y, z + 1);
                        var lightN100 = _cell.GetLight(x - 1, y, z);
                        var light0N10 = _cell.GetLight(x, y - 1, z);
                        var light00N1 = _cell.GetLight(x, y, z - 1);
                        
                        var light011 = _cell.GetLight(x + 0, y + 1, z + 1);    
                        var light01N1 = _cell.GetLight(x + 0, y + 1, z - 1);   
                        var light0N11 = _cell.GetLight(x + 0, y - 1, z + 1);   
                        var light0N1N1 = _cell.GetLight(x + 0, y - 1, z - 1);  
                        var light101 = _cell.GetLight(x + 1, y + 0, z + 1);    
                        var light10N1 = _cell.GetLight(x + 1, y + 0, z - 1);   
                        var light110 = _cell.GetLight(x + 1, y + 1, z + 0);    
                        var light111 = _cell.GetLight(x + 1, y + 1, z + 1);    
                        var light11N1 = _cell.GetLight(x + 1, y + 1, z - 1);   
                        var light1N10 = _cell.GetLight(x + 1, y - 1, z + 0);   
                        var light1N11 = _cell.GetLight(x + 1, y - 1, z + 1);   
                        var light1N1N1 = _cell.GetLight(x + 1, y - 1, z - 1);  
                        var lightN101 = _cell.GetLight(x - 1, y + 0, z + 1);   
                        var lightN10N1 = _cell.GetLight(x - 1, y + 0, z - 1);  
                        var lightN110 = _cell.GetLight(x - 1, y + 1, z + 0);   
                        var lightN111 = _cell.GetLight(x - 1, y + 1, z + 1);   
                        var lightN11N1 = _cell.GetLight(x - 1, y + 1, z - 1);  
                        var lightN1N10 = _cell.GetLight(x - 1, y - 1, z + 0);  
                        var lightN1N11 = _cell.GetLight(x - 1, y - 1, z + 1);  
                        var lightN1N1N1 = _cell.GetLight(x - 1, y - 1, z - 1); 

                        //XIncreasing
                        if (!(block100.RenderOpaque || block100.BlockId == blockId))
                        {
                            var blockFace = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[0],
                                TextureIndex = block.GetTextureMapping(FaceDirection.XIncreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace.LightTopLeft = blockLight;
                                blockFace.LightTopRight = blockLight;
                                blockFace.LightBottomRight = blockLight;
                                blockFace.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace.LightTopLeft = LightProcessor.LightAverage(light100, light10N1, light11N1, light110);  
                                blockFace.LightTopRight = LightProcessor.LightAverage(light100, light101, light111, light110);
                                blockFace.LightBottomRight = LightProcessor.LightAverage(light100, light101, light1N11, light1N10);
                                blockFace.LightBottomLeft = LightProcessor.LightAverage(light100, light10N1, light1N1N1, light1N10);
                            }

                            _masks[FaceDirection.XIncreasing][x, y, z] = blockFace;
                            added = true;
                        }

                        //YIncreasing
                        if (!(block010.RenderOpaque || block010.BlockId == blockId))
                        {
                            var blockFace = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[1],
                                TextureIndex = block.GetTextureMapping(FaceDirection.YIncreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace.LightTopLeft = blockLight;
                                blockFace.LightTopRight = blockLight;
                                blockFace.LightBottomRight = blockLight;
                                blockFace.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace.LightTopLeft = LightProcessor.LightAverage(light010, lightN110, lightN111, light011);  
                                blockFace.LightTopRight = LightProcessor.LightAverage(light010, light110, light111, light011);
                                blockFace.LightBottomRight = LightProcessor.LightAverage(light010, light110, light11N1, light01N1);
                                blockFace.LightBottomLeft = LightProcessor.LightAverage(light010, lightN110, lightN11N1, light01N1);
                            }

                            _masks[FaceDirection.YIncreasing][x, y, z] = blockFace;
                            added = true;
                        }
                        
                        //ZIncreasing
                        if (!(block001.RenderOpaque || block001.BlockId == blockId))
                        {
                            var blockFace = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[2],
                                TextureIndex = block.GetTextureMapping(FaceDirection.ZIncreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace.LightTopLeft = blockLight;
                                blockFace.LightTopRight = blockLight;
                                blockFace.LightBottomRight = blockLight;
                                blockFace.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace.LightTopLeft = LightProcessor.LightAverage(light001, light0N11, light1N11, light101);  
                                blockFace.LightTopRight = LightProcessor.LightAverage(light001, light011, light111, light101);
                                blockFace.LightBottomRight = LightProcessor.LightAverage(light001, light011, lightN111, lightN101);
                                blockFace.LightBottomLeft = LightProcessor.LightAverage(light001, light0N11, lightN1N11, lightN101);
                            }

                            _masks[FaceDirection.ZIncreasing][x, y, z] = blockFace;
                            added = true;
                        }

                        //XDecreasing
                        if (!(blockN100.RenderOpaque || blockN100.BlockId == blockId))
                        {
                            var blockFace = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[3],
                                TextureIndex = block.GetTextureMapping(FaceDirection.XDecreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace.LightTopLeft = blockLight;
                                blockFace.LightTopRight = blockLight;
                                blockFace.LightBottomRight = blockLight;
                                blockFace.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace.LightTopLeft = LightProcessor.LightAverage(lightN100, lightN10N1, lightN11N1, lightN110);  
                                blockFace.LightTopRight = LightProcessor.LightAverage(lightN100, lightN101, lightN111, lightN110);
                                blockFace.LightBottomRight = LightProcessor.LightAverage(lightN100, lightN101, lightN1N11, lightN1N10);
                                blockFace.LightBottomLeft = LightProcessor.LightAverage(lightN100, lightN10N1, lightN1N1N1, lightN1N10);
                            }

                            _masks[FaceDirection.XDecreasing][x, y, z] = blockFace;
                            added = true;
                        }

                        //YDecreasing
                        if (!(block0N10.RenderOpaque || block0N10.BlockId == blockId))
                        {
                            
                            var blockFace = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[4],
                                TextureIndex = block.GetTextureMapping(FaceDirection.YDecreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace.LightTopLeft = blockLight;
                                blockFace.LightTopRight = blockLight;
                                blockFace.LightBottomRight = blockLight;
                                blockFace.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace.LightTopLeft = LightProcessor.LightAverage(light0N10, lightN1N10, lightN1N11, light0N11);  
                                blockFace.LightTopRight = LightProcessor.LightAverage(light0N10, light1N10, light1N11, light0N11);
                                blockFace.LightBottomRight = LightProcessor.LightAverage(light0N10, light1N10, light1N1N1, light0N1N1);
                                blockFace.LightBottomLeft = LightProcessor.LightAverage(light0N10, lightN1N10, lightN1N1N1, light0N1N1);
                            }

                            _masks[FaceDirection.YDecreasing][x, y, z] = blockFace;
                            added = true;
                        }

                        //ZDecreasing
                        if (!(block00N1.RenderOpaque || block00N1.BlockId == blockId))
                        {
                            var blockFace = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[5],
                                TextureIndex = block.GetTextureMapping(FaceDirection.ZDecreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace.LightTopLeft = blockLight;
                                blockFace.LightTopRight = blockLight;
                                blockFace.LightBottomRight = blockLight;
                                blockFace.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace.LightTopLeft = LightProcessor.LightAverage(light00N1, light0N1N1, light1N1N1, light10N1);  
                                blockFace.LightTopRight = LightProcessor.LightAverage(light00N1, light01N1, light11N1, light10N1);
                                blockFace.LightBottomRight = LightProcessor.LightAverage(light00N1, light01N1, lightN11N1, lightN10N1);
                                blockFace.LightBottomLeft = LightProcessor.LightAverage(light00N1, light0N1N1, lightN1N1N1, lightN10N1);
                            }

                            _masks[FaceDirection.ZDecreasing][x, y, z] = blockFace;
                            added = true;
                        }

                        if (!added) continue;
                        var vec = new Vector3Int(x, y, z);
                        _min = Vector3Int.Min(_min, vec);
                        _max = Vector3Int.Max(_max, vec);
                    }
                }
            }
            _max += Vector3Int.one;
        }
    }
}