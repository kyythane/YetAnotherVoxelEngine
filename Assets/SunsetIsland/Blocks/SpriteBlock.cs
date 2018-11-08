using Assets.SunsetIsland.Common;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Config.Data;
using Assets.SunsetIsland.Managers;
using UnityEngine;

namespace Assets.SunsetIsland.Blocks
{
    public class SpriteBlock : IBlock
    {
        private readonly BlockData _blockData;

        public SpriteBlock(int blockId, byte rotation, ushort[] tints)
        {
            _blockData = BlockFactory.BlockDictionary.GetStaticData(blockId);
            Tints = tints;
            Rotation = rotation;
        }

        public int BlockId => _blockData.BlockId;

        int IBlock.GetTextureMapping(FaceDirection faceDir)
        {
            return BlockFactory.BlockDictionary.GetTextureMapping(BlockId, faceDir);
        }

        //16-bit HighColor (5/6/5)
        public ushort[] Tints { get; }

        public byte Rotation { get; }

        public Color32 GetColor(FaceDirection faceDir)
        {
            return Tints[0].Convert32();
        }

        public bool RenderOpaque => _blockData.RenderOpaque;

        public bool AddToRenderMesh => false;

        public bool AddToPhysicsMesh => _blockData.AddToPhysicsMesh;

        public uint Emissivity => _blockData.Emissivity;

        public uint Opacity => _blockData.Opacity;

        public bool Equals(SpriteBlock other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            var block = obj as SpriteBlock;
            return !Equals(block, null) && Equals(block);
        }

        public override int GetHashCode()
        {
            return BlockFactory.Hash(this);
        }

        public static bool operator ==(SpriteBlock b1, SpriteBlock b2)
        {
            var b1Null = Equals(b1, null);
            var b2Null = Equals(b2, null);
            return b1Null && b2Null ||
                   !b1Null &&
                   !b2Null &&
                   b1.BlockId == b2.BlockId &&
                   b1.Rotation == b2.Rotation &&
                   b1.Tints[0] == b2.Tints[0];
        }

        public static bool operator !=(SpriteBlock b1, SpriteBlock b2)
        {
            return !(b1 == b2);
        }
    }
}