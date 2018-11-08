using System.Collections.Generic;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Config.Data;
using Assets.SunsetIsland.Managers;
using UnityEngine;

namespace Assets.SunsetIsland.Blocks
{
    public class BlockDictionary
    {
        private readonly Dictionary<string, int> _blockNameMap;
        private readonly Dictionary<int, int> _blockTextureMappings;
        private readonly Dictionary<string, List<int>> _blockTypeMap;
        private readonly Dictionary<int, BlockData> _dict;

        public BlockDictionary()
        {
            _dict = new Dictionary<int, BlockData>();
            _blockNameMap = new Dictionary<string, int>();
            _blockTypeMap = new Dictionary<string, List<int>>();
            _blockTextureMappings = new Dictionary<int, int>();
        }

        public Texture2DArray BlockTextures { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            var properties = ConfigManager.Properties;
            var textures = new Dictionary<string, Texture2D>();
            var textureIndexMap = new Dictionary<string, int>();
            var nextIndex = 0;
            foreach (var typeString in properties.BlockTypeMap)
            {
                var type = typeString.Key;
                var blockNameList = typeString.Value;
                _blockTypeMap[type] = new List<int>();
                foreach (var blockName in blockNameList)
                {
                    var blockData = ConfigManager.Load<BlockData>(blockName);
                    _blockTypeMap[type].Add(blockData.BlockId);
                    _dict.Add(blockData.BlockId, blockData);
                    _blockNameMap.Add(blockName, blockData.BlockId);
                    if (blockData.TextureNames == null || blockData.TextureNames.Count <= 0)
                        continue;
                    for (var faceIndex = 0; faceIndex < 6; faceIndex++)
                    {
                        var textureName = blockData.TextureNames[faceIndex % blockData.TextureNames.Count];
                        var textureIndex = (blockData.BlockId << 3) + faceIndex;
                        if (!textures.ContainsKey(textureName))
                        {
                            textures[textureName] = AssetManager.Load<Texture2D>(textureName);
                            textureIndexMap[textureName] = nextIndex++;
                        }
                        _blockTextureMappings.Add(textureIndex, textureIndexMap[textureName]);
                    }
                }
            }
            BlockTextures = new Texture2DArray(properties.BlockTextureWidth, properties.BlockTextureHeight,
                                               textures.Count, properties.BlockTextureFormat, true)
            {
                wrapModeU = TextureWrapMode.Repeat,
                wrapModeV = TextureWrapMode.Repeat,
                wrapModeW = TextureWrapMode.Repeat
            };
            foreach (var texture in textures)
                BlockTextures.SetPixels32(texture.Value.GetPixels32(), textureIndexMap[texture.Key]);
            BlockTextures.Apply();
            IsInitialized = true;
        }

        public bool IsValidBlockName(string blockName)
        {
            return _blockNameMap.ContainsKey(blockName);
        }

        public int GetBlockIdForName(string blockName)
        {
            return _blockNameMap[blockName];
        }

        public List<int> GetBlockIdsForType(string type)
        {
            return _blockTypeMap[type];
        }

        public bool IsValidBlockId(int blockId)
        {
            return _dict.ContainsKey(blockId);
        }

        public BlockData GetStaticData(int blockId)
        {
            return _dict[blockId];
        }

        public int GetTextureMapping(int blockType, FaceDirection faceDir)
        {
            return _blockTextureMappings[(blockType << 3) + (int) faceDir];
        }
    }
}