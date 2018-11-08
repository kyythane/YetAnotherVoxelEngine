using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Managers;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.SunsetIsland.Chunks.Processors.Lighting
{
    public struct LightData
    {
        public Vector3Int Position { get; set; }
        public uint Light { get; set; }
    }

    public struct LightProcessor
    {
        // Word level parallelization based on: https://0fps.net/2018/02/21/voxel-lighting/

        private const uint ComponentMask = 0x1F07C1Fu;
        private const uint BorrowGuard = 0x4010040u;
        private const uint CarryMask = 0x2008020u;
        private const uint SunlightMask = 0x7FFFu;
        private const uint BlockMask = 0x3FFF8000u;
        private const int Bits = 5;
        private const int BitDepth = (1 << Bits) - 1;

        private static readonly Vector3Int[] _offsets =
        {
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0)
        };

        private readonly uint _minusOneSun;
        private readonly Queue<Node> _propogateQueue;
        private readonly Queue<Node> _removeQueue;

        private Vector3Int _maxBound;
        private uint[] _buffer;
        private uint[] _opacity;

        public LightProcessor(int sizeX, int sizeY, int sizeZ)
        {
            _minusOneSun = GetOpacity(Vector3Int.zero, new Vector3Int(1, 1, 1));
            var queuePool = PoolManager.GetObjectPool<Queue<Node>>();
            _propogateQueue = queuePool.Pop();
            _removeQueue = queuePool.Pop();
            _maxBound = new Vector3Int(sizeX, sizeY, sizeZ);
            var len = sizeX * sizeY * sizeZ;
            var pool = PoolManager.GetArrayPool<uint[]>(len);
            _buffer = pool.Pop();
            _opacity = pool.Pop();
        }

        public static uint Sunlight { get; } = ToLight(Color.black, Color.white);

        public void GrowArray(int newHeight)
        {
            if(newHeight == _maxBound.y)
                return;
            var len = _maxBound.x * newHeight * _maxBound.z;
            var newBuffer = PoolManager.GetArrayPool<uint[]>(len).Pop();
            var newOpacity = PoolManager.GetArrayPool<uint[]>(len).Pop();
            for (var y = 0; y < newHeight; ++y)
            for (var x = 0; x < _maxBound.x; ++x)
            for (var z = 0; z < _maxBound.z; ++z)
            {
                var newIndex = (x * _maxBound.x + z) * newHeight + y;
                if (y >= _maxBound.y)
                {
                    newBuffer[newIndex] = 0x0u;
                    newOpacity[newIndex] = 0x0u;
                    continue;
                }

                var oldIndex = Index(x, y, z);
                newBuffer[newIndex] = _buffer[oldIndex];
                newOpacity[newIndex] = _opacity[oldIndex];
            }

            _maxBound.y = newHeight;
            var pool = PoolManager.GetArrayPool<uint[]>(_buffer.Length);
            pool.Push(_buffer);
            pool.Push(_opacity);
            _buffer = newBuffer;
            _opacity = newOpacity;
        }

        public void SetupBufferStep(BatchUpdateItem<LightBlockItem> batchUpdateItem)
        {
            var pos = batchUpdateItem.Position;
            var light = batchUpdateItem.Item.Light;
            var block = batchUpdateItem.Item.Block;
            var lightMax = LightMax(light, block.Emissivity);
            var index = Index(pos.x, pos.y, pos.z);
            _buffer[index] = lightMax;
            _opacity[index] = block.Opacity;
        }

        public static uint GetOpacity(Vector3Int subBlock, Vector3Int subSun)
        {
            return GetOpacity(subBlock.x, subBlock.y, subBlock.z, subSun.x, subSun.y, subSun.z);
        }

        public static uint GetOpacity(int blockR, int blockG, int blockB, int sunR, int sunG, int sunB)
        {
            var uintBlockR = (uint) (blockR & BitDepth);
            var uintBlockG = (uint) (blockG & BitDepth);
            var uintBlockB = (uint) (blockB & BitDepth);
            var uintSunR = (uint) (sunR & BitDepth);
            var uintSunG = (uint) (sunG & BitDepth);
            var uintSunB = (uint) (sunB & BitDepth);
            var opacity = (uintBlockR << 25) | (uintBlockG << 20) | (uintBlockB << 15) | (uintSunR << 10) |
                          (uintSunG << 5) | uintSunB;
            return opacity;
        }

        public uint[] Light(Func<int, int, int, uint> neighborLights)
        {
            for (var y = _maxBound.y - 1; y >= 0; --y)
            for (var x = 0; x < _maxBound.x; ++x)
            for (var z = 0; z < _maxBound.z; ++z)
            {
                var index = Index(x, y, z);
                var origValue = _buffer[index];
                var propogateValue = origValue;
                var opacity = _opacity[index];
                var position = new Vector3Int(x, y, z);
                //if (y == _maxBound.y - 1) //Add sunlight to column
                //{
                //    var sunSub = opacity;
                //    if ((opacity & SunlightMask) == _minusOneSun) sunSub &= BlockMask;
                //    propogateValue = LightMax(propogateValue, LightDecrement(Sunlight, sunSub));
                //}

                if (x == 0)
                {
                    var neighborLight = neighborLights(-1, y, z);
                    propogateValue = LightMax(propogateValue, LightDecrement(neighborLight, opacity));
                }
                else if (x == _maxBound.x - 1)
                {
                    var neighborLight = neighborLights(_maxBound.x, y, z);
                    propogateValue = LightMax(propogateValue, LightDecrement(neighborLight, opacity));
                }

                if (z == 0)
                {
                    var neighborLight = neighborLights(x, y, -1);
                    propogateValue = LightMax(propogateValue, LightDecrement(neighborLight, opacity));
                }
                else if (z == _maxBound.z - 1)
                {
                    var neighborLight = neighborLights(x, y, _maxBound.z);
                    propogateValue = LightMax(propogateValue, LightDecrement(neighborLight, opacity));
                }

                _buffer[index] = propogateValue;
                Enqueue(propogateValue, position, index, position);
            }

            Propogate();
            return _buffer;
        }

        public uint[] IncrementalLight(IEnumerable<LightData> removeList, IList<LightData> addList)
        {
            foreach (var lightData in removeList)
            {
                RemoveLight(lightData);
                Propogate();
            }

            foreach (var lightData in addList)
            {
                _buffer[Index(lightData.Position.x, lightData.Position.y, lightData.Position.z)] = lightData.Light;
                _propogateQueue.Enqueue(new Node
                {
                    Light = lightData.Light,
                    Position = lightData.Position,
                    LastPosition = lightData.Position
                });
            }

            Propogate();
            return _buffer;
        }

        public void Dispose()
        {
            var pool = PoolManager.GetArrayPool<uint[]>(_opacity.Length);
            pool.Push(_opacity);
            var queuePool = PoolManager.GetObjectPool<Queue<Node>>();
            queuePool.Push(_propogateQueue);
            queuePool.Push(_removeQueue);
        }

        private void RemoveLight(LightData lightData)
        {
            _removeQueue.Enqueue(new Node
            {
                Light = lightData.Light,
                Position = lightData.Position,
                LastPosition = lightData.Position
            });
            var testMask = 0u;
            if ((lightData.Light & BlockMask) != 0) testMask |= BlockMask;
            if ((lightData.Light & SunlightMask) != 0) testMask |= SunlightMask;
            while (_removeQueue.Count > 0)
            {
                var current = _removeQueue.Dequeue();
                var currentPos = current.Position;
                var currentIndex = current.Index;
                var incominglight = current.Light;
                var blockLight = _buffer[currentIndex];
                var clearMask = LightLessThan(incominglight, blockLight);
                var clearedLight = clearMask & blockLight;
                if ((clearedLight & testMask) != 0)
                    _propogateQueue.Enqueue(new Node
                    {
                        Light = clearedLight,
                        Position = currentPos,
                        LastPosition = currentPos
                    });
                if ((clearedLight & testMask) == (blockLight & testMask)) continue;
                _buffer[currentIndex] = clearedLight;
                foreach (var offset in _offsets)
                {
                    var nextPos = currentPos + offset;
                    if (!Contains(nextPos) || nextPos == current.LastPosition)
                        continue; //don't propogate backwards or out of bounds
                    var nextIndex = Index(nextPos.x, nextPos.y, nextPos.z);
                    var sub = GetSub(offset.y == -1, incominglight, _opacity[nextIndex]);
                    var nextLight = LightDecrement(incominglight, sub);
                    _removeQueue.Enqueue(new Node
                    {
                        Light = nextLight,
                        Position = nextPos,
                        Index = nextIndex,
                        LastPosition = currentPos
                    });
                }
            }
        }

        private void Propogate()
        {
            while (_propogateQueue.Count > 0)
            {
                var current = _propogateQueue.Dequeue();
                foreach (var offset in _offsets)
                {
                    var nextPos = current.Position + offset;
                    var nextIndex = Index(nextPos.x, nextPos.y, nextPos.z);
                    if (!Contains(nextPos) || nextPos == current.LastPosition)
                        continue; //don't propogate backwards or out of bounds
                    uint nextLight;
                    if (!ComputeNextLight(nextIndex, offset.y == -1, current.Light, out nextLight))
                        continue;
                    _buffer[nextIndex] = nextLight;
                    Enqueue(nextLight, nextPos, nextIndex, current.Position);
                }
            }
        }

        private int Index(int x, int y, int z)
        {
            return (x * _maxBound.x + z) * _maxBound.y + y;
        }

        private int Index(Vector3Int vec)
        {
            return (vec.x * _maxBound.x + vec.z) * _maxBound.y + vec.y;
        }

        private bool Contains(Vector3Int nextPos)
        {
            return nextPos.x >= 0 && nextPos.x < _maxBound.x &&
                   nextPos.y >= 0 && nextPos.y < _maxBound.y &&
                   nextPos.z >= 0 && nextPos.z < _maxBound.z;
        }

        private bool ComputeNextLight(int nextIndex, bool down, uint currentLight, out uint nextLight)
        {
            var sub = GetSub(down, currentLight, _opacity[nextIndex]);

            nextLight = LightDecrement(currentLight, sub);
            var checkLight = _buffer[nextIndex];
            nextLight = LightMax(nextLight, checkLight);
            return checkLight != nextLight;
        }

        private uint GetSub(bool down, uint currentLight, uint opacity)
        {
            if (down && (currentLight & SunlightMask) == SunlightMask &&
                (opacity & SunlightMask) == _minusOneSun
            ) //white sunlight doesn't lose eneergy going down through air
                opacity &= BlockMask;

            return opacity;
        }

        private void Enqueue(uint lightValue, Vector3Int position, int index, Vector3Int lastPosition)
        {
            if (lightValue == 0) return;

            foreach (var offset in _offsets)
            {
                var next = position + offset;
                if (!Contains(next) ||
                    LightLessThan(_buffer[Index(next)], lightValue) == 0) continue;
                _propogateQueue.Enqueue(new Node
                {
                    Light = lightValue,
                    Position = position,
                    Index = index,
                    LastPosition = lastPosition
                });
                return;
            }
        }

        private static uint LightLessThanHalf(uint lightA, uint lightB)
        {
            var underflow = (((lightA & ComponentMask) | BorrowGuard) - (lightB & ComponentMask)) & CarryMask;
            return (underflow >> 1) | (underflow >> 2) | (underflow >> 3) | (underflow >> 4) |
                   (underflow >> 5); //smear bits
        }

        private static uint LightLessThan(uint lightA, uint lightB)
        {
            return LightLessThanHalf(lightA, lightB) | (LightLessThanHalf(lightA >> Bits, lightB >> Bits) << Bits);
        }

        private static uint LightMax(uint lightA, uint lightB)
        {
            return lightA ^ ((lightA ^ lightB) & LightLessThan(lightA, lightB));
        }

        private static uint LightDecrementHalf(uint light, uint attenuation)
        {
            var difference = ((light & ComponentMask) | BorrowGuard) - (attenuation & ComponentMask);

            var underflow = difference & CarryMask; // check underflow
            var res = difference &
                      ~((underflow >> 1) | (underflow >> 2) | (underflow >> 3) | (underflow >> 4) | (underflow >> 5)) &
                      ComponentMask; //saturate

            return res;
        }

        private static uint LightDecrement(uint light, uint attenuation)
        {
            return LightDecrementHalf(light, attenuation) |
                   (LightDecrementHalf(light >> Bits, attenuation >> Bits) << Bits);
        }

        private static uint LightAverageHalf(uint light1, uint light2, uint light3, uint light4)
        {
            var sum = (light1 & ComponentMask) +
                      (light2 & ComponentMask) +
                      (light3 & ComponentMask) +
                      (light4 & ComponentMask);
            return (sum >> 2) & ComponentMask;
        }

        public static uint LightAverage(uint light1, uint light2, uint light3, uint light4)
        {
            return LightAverageHalf(light1, light2, light3, light4) |
                   (LightAverageHalf(light1 >> Bits, light2 >> Bits, light3 >> Bits, light4 >> Bits) << Bits);
        }

        public static uint ToLight(Color32 colorBlock, Color32 colorSun)
        {
            uint uintLight = 0;
            uintLight |= (uint) (colorBlock.r >> 3) << 25;
            uintLight |= (uint) (colorBlock.g >> 3) << 20;
            uintLight |= (uint) (colorBlock.b >> 3) << 15;
            uintLight |= (uint) (colorSun.r >> 3) << 10;
            uintLight |= (uint) (colorSun.g >> 3) << 5;
            uintLight |= (uint) (colorSun.b >> 3);
            return uintLight;
        }

        public static void ToColors(uint uintLight, out Color32 colorBlock, out Color32 colorSun)
        {
            var blockR = (byte) (((uintLight >> 25) & 31) << 3);
            var blockG = (byte) (((uintLight >> 20) & 31) << 3);
            var blockB = (byte) (((uintLight >> 15) & 31) << 3);
            var sunR = (byte) (((uintLight >> 10) & 31) << 3);
            var sunG = (byte) (((uintLight >> 5) & 31) << 3);
            var sunB = (byte) ((uintLight & 31) << 3);
            colorBlock = new Color32(blockR, blockG, blockB, 255);
            colorSun = new Color32(sunR, sunG, sunB, 255);
        }

        public static uint ToLight(ushort colorBlock, ushort colorSun)
        {
            //ushort - 565
            uint uintLight = 0;
            uintLight |= (uint) ((colorBlock >> 11) & 31) << 25;
            uintLight |= (uint) ((colorBlock >> 6) & 31) << 20;
            uintLight |= (uint) (colorBlock & 31) << 15;
            uintLight |= (uint) ((colorSun >> 11) & 31) << 10;
            uintLight |= (uint) ((colorSun >> 6) & 31) << 5;
            uintLight |= (uint) (colorSun & 31);
            return uintLight;
        }

        public static void ToColors(uint uintLight, out ushort colorBlock, out ushort colorSun)
        {
            var blockR = (uintLight >> 25) & 31;
            var blockG = (uintLight >> 20) & 31;
            var blockB = (uintLight >> 15) & 31;
            var sunR = (uintLight >> 10) & 31;
            var sunG = (uintLight >> 5) & 31;
            var sunB = uintLight & 31;
            colorBlock = (ushort) ((blockR << 11) | (blockG << 6) | blockB);
            colorSun = (ushort) ((sunR << 11) | (sunG << 6) | sunB);
        }

        public static void ToColors(uint uintLight, out Vector3 colorBlock, out Vector3 colorSun)
        {
            var blockR = ((uintLight >> 25) & 31) / 31.0f;
            var blockG = ((uintLight >> 20) & 31) / 31.0f;
            var blockB = ((uintLight >> 15) & 31) / 31.0f;
            var sunR = ((uintLight >> 10) & 31) / 31.0f;
            var sunG = ((uintLight >> 5) & 31) / 31.0f;
            var sunB = (uintLight & 31) / 31.0f;
            colorBlock = new Vector3(blockR, blockG, blockB);
            colorSun = new Vector3(sunR, sunG, sunB);
        }

        private struct Node
        {
            public uint Light { get; set; }
            public Vector3Int Position { get; set; }
            public Vector3Int LastPosition { get; set; }
            public int Index { get; set; }
        }
    }
}