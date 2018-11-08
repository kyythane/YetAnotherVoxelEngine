using System.Collections.Concurrent;
using System.Collections.Generic;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Common;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.SunsetIsland.Managers
{
    public static class BlockFactory
    {
        public static readonly BlockDictionary BlockDictionary = new BlockDictionary();
        public static Material BlockMaterialInstance;

        public static readonly ushort[] Untinted =
        {
            ushort.MaxValue,
            ushort.MaxValue,
            ushort.MaxValue,
            ushort.MaxValue,
            ushort.MaxValue,
            ushort.MaxValue
        };

        public static IBlock Empty, BottomOfWorld;

        private static readonly ConcurrentDictionary<int, Block> HashedBlocks =
            new ConcurrentDictionary<int, Block>();

        public static int Size => HashedBlocks.Count;

        public static void Initialize()
        {
            Colors.Initiailize();
            BlockDictionary.Initialize();
            BlockMaterialInstance = new Material(ConfigManager.UnityProperties.BlockMaterial);
            BlockMaterialInstance.SetTexture("_TextureArr", BlockDictionary.BlockTextures);
            Empty = CreateBlock(0);
            BottomOfWorld = CreateBlock(-1);
            CreateBlock(6, GetTint(Colors.Yellow));
        }

        public static ushort[] GetTint(ushort tint)
        {
            return GetTint(tint, tint, tint, tint, tint, tint);
        }

        public static ushort[] GetTint(ushort tintXIncreasing, ushort tintYIncreasing,
            ushort tintZDecresing, ushort tintXDecresing,
            ushort tintYDecresing, ushort tintZIncreasing)
        {
            return new[]
            {
                tintXIncreasing,
                tintYIncreasing,
                tintZIncreasing,
                tintXDecresing,
                tintYDecresing,
                tintZDecresing
            };
        }

        public static IBlock CreateBlock(int blockId, ushort[] tints = null)
        {
            if (tints == null)
                tints = Untinted;
            var hash = Hash(blockId, tints);
            return HashedBlocks.GetOrAdd(hash, key => new Block(blockId, 0, tints));
        }

        public static IBlock UpdateBlock([NotNull] IBlock block, ushort[] tints = null)
        {
            if (tints == null)
                tints = block.Tints;
            var hash = Hash(block.BlockId, tints);
            return HashedBlocks.GetOrAdd(hash, key => new Block(block.BlockId, 0, tints));
        }

        private static int Hash(IReadOnlyList<ushort> tints)
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 3145739;
                hash = (hash * 1572869) ^ tints[0];
                hash = (hash * 1572869) ^ tints[1];
                hash = (hash * 1572869) ^ tints[2];
                hash = (hash * 1572869) ^ tints[3];
                hash = (hash * 1572869) ^ tints[4];
                hash = (hash * 1572869) ^ tints[5];
                return hash;
            }
        }

        //Uses a long to reduce collision probability
        private static int Hash(int blockId, ushort[] tints = null)
        {
            if (tints == null)
                tints = Untinted;
            unchecked // Overflow is fine, just wrap
            {
                var hash = 3145739 * blockId;
                return (hash * 1572869) ^ Hash(tints);
            }
        }

        public static int Hash([NotNull] IBlock block)
        {
            return Hash(block.BlockId, block.Tints);
        }
    }
}