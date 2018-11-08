using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Managers;
using Assets.SunsetIsland.Utilities;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks.Processors.Meshes
{
    public struct PhysicsMeshProcessor
    {
        private bool[,,] _buffer; //TODO L: make shim
        private int _chunkSize;

        public void Process(IChunk chunk)
        {
            _chunkSize = chunk.Size.x;
            var blockSize = ConfigManager.Properties.BlockWorldScale;
            _buffer = PoolManager.GetArrayPool<bool[,,]>(_chunkSize).Pop();
            for (var x = 0; x < _chunkSize; ++x)
            {
                for (var y = 0; y < _chunkSize; ++y)
                {
                    for (var z = 0; z < _chunkSize; ++z)
                    {
                        _buffer[ x,  y,  z] =
                            chunk.GetBlockWithBoundCheck(x, y, z).AddToPhysicsMesh;
                    }
                }
            }
            chunk.PhysicsMeshData.Clear();
            var maskPool = PoolManager.GetArrayPool<int[]>(_chunkSize *_chunkSize);
            var mask = maskPool.Pop();
            for (var s = 0; s < 6; s++)
            {
                var backside = s > 2;
                var side = General.FlipDirection((FaceDirection) s);
                var axis = s % 3;

                var u = (axis + 1) % 3;
                var v = (axis + 2) % 3;

                var blockPosition = new Vector3Int {[axis] = backside ? _chunkSize - 1 : 0};

                while (blockPosition[axis] < _chunkSize && blockPosition[axis] >= 0)
                {
                    var maskIndex = 0;
                    for (blockPosition[v] = 0; blockPosition[v] < _chunkSize; blockPosition[v]++)
                    {
                        for (blockPosition[u] = 0; blockPosition[u] < _chunkSize; blockPosition[u]++)
                            mask[maskIndex++] =
                                GetFaceCollision(blockPosition.x, blockPosition.y, blockPosition.z, side);
                    }
                    maskIndex = 0;
                    for (var j = 0; j < _chunkSize; j++)
                    {
                        for (var i = 0; i < _chunkSize;)
                            if (mask[maskIndex] != 0)
                            {
                                var width = 1;
                                while (i + width < _chunkSize &&
                                       mask[maskIndex] == mask[maskIndex + width])
                                    width++;

                                int k;
                                int height;
                                for (height = 1; j + height < _chunkSize; height++)
                                {
                                    for (k = 0; k < width; k++)
                                        if(mask[maskIndex + k + height * _chunkSize] != mask[maskIndex])
                                        goto heightFound;
                                }

                                heightFound:

                                blockPosition[u] = i;
                                blockPosition[v] = j;

                                var offset = blockPosition[axis] + (backside ? 1 : 0);

                                var topLeft = new Vector3
                                              {
                                                  [u] = blockPosition[u] + width,
                                                  [v] = blockPosition[v],
                                                  [axis] = offset
                                              } *
                                              blockSize;
                                var topRight = new Vector3
                                               {
                                                   [u] = blockPosition[u] + width,
                                                   [v] = blockPosition[v] + height,
                                                   [axis] = offset
                                               } *
                                               blockSize;
                                var bottomRight = new Vector3
                                                  {
                                                      [u] = blockPosition[u],
                                                      [v] = blockPosition[v] + height,
                                                      [axis] = offset
                                                  } *
                                                  blockSize;

                                var bottomLeft = new Vector3
                                                 {
                                                     [u] = blockPosition[u],
                                                     [v] = blockPosition[v],
                                                     [axis] = offset
                                                 } *
                                                 blockSize;

                                chunk.PhysicsMeshData.AddQuad(topLeft,
                                                               topRight, bottomRight, bottomLeft, backside);

                                for (var l = 0; l < height; ++l)
                                {
                                    for (k = 0; k < width; ++k)
                                    {
                                        mask[maskIndex + k + l * _chunkSize] = 0;
                                    }
                                }

                                i += width;
                                maskIndex += width;
                            }
                            else
                            {
                                i++;
                                maskIndex++;
                            }
                    }
                    blockPosition[axis] += backside ? -1 : 1;
                }
            }
            maskPool.Push(mask);
            PoolManager.GetArrayPool<bool[,,]>(_chunkSize).Push(_buffer);
        }

        private int GetFaceCollision(int x, int y, int z, FaceDirection side)
        {
            var block = _buffer[x, y, z];
            if (!block)
                return 0;
            var neighborPos = General.Neighbor(x, y, z, side);
            if(neighborPos.x < 0 || neighborPos.x > _chunkSize - 1 || //TODO : use shim for get block (neighbor) function to avoid unecessary polygons
               neighborPos.y < 0 || neighborPos.y > _chunkSize - 1 ||
               neighborPos.z < 0 || neighborPos.z > _chunkSize - 1)
            {
                return 1;
            }
            var neighbor = _buffer[ neighborPos.x,  neighborPos.y,  neighborPos.z];
            return neighbor ? 0 : 1;
        }
    }
}