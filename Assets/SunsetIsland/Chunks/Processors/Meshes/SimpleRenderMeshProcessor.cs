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
    public interface IRenderMeshProcessor
    {
        void Process(IRenderCell cell);
    }

    public struct SimpleRenderMeshProcessor : IRenderMeshProcessor
    {
        private IRenderCell _cell;

        public void Process(IRenderCell cell)
        {
            _cell = cell;
            _cell.RenderMeshData.Clear();

            for (var x = _cell.Min.x; x < _cell.Max.x; x++)
            {
                for (var z = _cell.Min.z; z < _cell.Max.z; z++)
                {
                    for (var y = _cell.Min.y; y < _cell.Max.y; y++)
                    {
                        var block = _cell.GetBlock(x, y, z);
                        if (!block.AddToRenderMesh)
                            continue;
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

                        //XIncreasing
                        if (!(block100.RenderOpaque || block100.BlockId == blockId))
                        {
                            var blockFace1 = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[0],
                                TextureIndex = block.GetTextureMapping(FaceDirection.XIncreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace1.LightTopLeft = blockLight;
                                blockFace1.LightTopRight = blockLight;
                                blockFace1.LightBottomRight = blockLight;
                                blockFace1.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace1.LightTopLeft = light100;  
                                blockFace1.LightTopRight = light100;
                                blockFace1.LightBottomRight = light100;
                                blockFace1.LightBottomLeft = light100;
                            }

                            var topLeft = new Vector3(x + 1, y + 1, z);
                            var topRight = new Vector3(x + 1, y + 1, z + 1);
                            var bottomRight = new Vector3(x + 1, y, z + 1);
                            var bottomLeft = new Vector3(x + 1, y, z);

                            _cell.RenderMeshData.AddQuad(topLeft, topRight, bottomRight, bottomLeft, 1, 1, blockFace1,
                                (int) General.FlipDirection(FaceDirection.XIncreasing));
                        }

                        //YIncreasing
                        if (!(block010.RenderOpaque || block010.BlockId == blockId))
                        {
                            var blockFace2 = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[1],
                                TextureIndex = block.GetTextureMapping(FaceDirection.YIncreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace2.LightTopLeft = blockLight;
                                blockFace2.LightTopRight = blockLight;
                                blockFace2.LightBottomRight = blockLight;
                                blockFace2.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace2.LightTopLeft = light010;  
                                blockFace2.LightTopRight = light010;
                                blockFace2.LightBottomRight = light010;
                                blockFace2.LightBottomLeft = light010;
                            }

                            var topLeft = new Vector3(x, y + 1, z + 1);
                            var topRight = new Vector3(x + 1, y + 1, z + 1);
                            var bottomRight = new Vector3(x + 1, y + 1, z);
                            var bottomLeft = new Vector3(x, y + 1, z );

                            _cell.RenderMeshData.AddQuad(topLeft, topRight, bottomRight, bottomLeft, 1, 1, blockFace2,
                                (int) General.FlipDirection(FaceDirection.YIncreasing));
                        }
                        
                        //ZIncreasing
                        if (!(block001.RenderOpaque || block001.BlockId == blockId))
                        {
                            var blockFace3 = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[2],
                                TextureIndex = block.GetTextureMapping(FaceDirection.ZIncreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace3.LightTopLeft = blockLight;
                                blockFace3.LightTopRight = blockLight;
                                blockFace3.LightBottomRight = blockLight;
                                blockFace3.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace3.LightTopLeft = light001;  
                                blockFace3.LightTopRight = light001;
                                blockFace3.LightBottomRight = light001;
                                blockFace3.LightBottomLeft = light001;
                            }

                            var topLeft = new Vector3(x + 1, y, z + 1);
                            var topRight = new Vector3(x + 1, y + 1, z + 1);
                            var bottomRight = new Vector3(x, y + 1, z + 1);
                            var bottomLeft = new Vector3(x, y, z + 1);

                            _cell.RenderMeshData.AddQuad(topLeft, topRight, bottomRight, bottomLeft, 1, 1, blockFace3,
                                (int) General.FlipDirection(FaceDirection.ZIncreasing));
                        }

                        //XDecreasing
                        if (!(blockN100.RenderOpaque || blockN100.BlockId == blockId))
                        {
                            var blockFace4 = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[3],
                                TextureIndex = block.GetTextureMapping(FaceDirection.XDecreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace4.LightTopLeft = blockLight;
                                blockFace4.LightTopRight = blockLight;
                                blockFace4.LightBottomRight = blockLight;
                                blockFace4.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace4.LightTopLeft = lightN100;  
                                blockFace4.LightTopRight = lightN100;
                                blockFace4.LightBottomRight = lightN100;
                                blockFace4.LightBottomLeft = lightN100;
                            }

                            var topLeft = new Vector3(x, y + 1, z);
                            var topRight = new Vector3(x, y + 1, z + 1);
                            var bottomRight = new Vector3(x, y, z + 1);
                            var bottomLeft = new Vector3(x, y, z);

                            _cell.RenderMeshData.AddQuad(topLeft, topRight, bottomRight, bottomLeft, 1, 1, blockFace4,
                                (int) General.FlipDirection(FaceDirection.XDecreasing));
                        }

                        //YDecreasing
                        if (!(block0N10.RenderOpaque || block0N10.BlockId == blockId))
                        {
                            
                            var blockFace5 = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[4],
                                TextureIndex = block.GetTextureMapping(FaceDirection.YDecreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace5.LightTopLeft = blockLight;
                                blockFace5.LightTopRight = blockLight;
                                blockFace5.LightBottomRight = blockLight;
                                blockFace5.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace5.LightTopLeft = light0N10;  
                                blockFace5.LightTopRight = light0N10;
                                blockFace5.LightBottomRight = light0N10;
                                blockFace5.LightBottomLeft = light0N10;
                            }

                            var topLeft = new Vector3(x, y, z + 1);
                            var topRight = new Vector3(x + 1, y, z + 1);
                            var bottomRight = new Vector3(x + 1, y, z);
                            var bottomLeft = new Vector3(x, y, z );

                            _cell.RenderMeshData.AddQuad(topLeft, topRight, bottomRight, bottomLeft, 1, 1, blockFace5,
                                (int) General.FlipDirection(FaceDirection.YDecreasing));
                        }

                        //ZDecreasing
                        if (!(block00N1.RenderOpaque || block00N1.BlockId == blockId))
                        {
                            var blockFace6 = new BlockFace
                            {
                                Exists = true,
                                BlockId = block.BlockId,
                                Tint = block.Tints[5],
                                TextureIndex = block.GetTextureMapping(FaceDirection.ZDecreasing)
                            };
                            if (block.Emissivity > 0)
                            {
                                blockFace6.LightTopLeft = blockLight;
                                blockFace6.LightTopRight = blockLight;
                                blockFace6.LightBottomRight = blockLight;
                                blockFace6.LightBottomLeft = blockLight;
                            }
                            else
                            {
                                blockFace6.LightTopLeft = light00N1;  
                                blockFace6.LightTopRight = light00N1;
                                blockFace6.LightBottomRight = light00N1;
                                blockFace6.LightBottomLeft = light00N1;
                            }

                            var topLeft = new Vector3(x + 1, y, z);
                            var topRight = new Vector3(x + 1, y + 1, z);
                            var bottomRight = new Vector3(x, y + 1, z);
                            var bottomLeft = new Vector3(x, y, z );
            
                            _cell.RenderMeshData.AddQuad(topLeft, topRight, bottomRight, bottomLeft, 1, 1, blockFace6, (int) General.FlipDirection(FaceDirection.ZDecreasing));
                        }

                    }
                }
            }
        }
    }
}