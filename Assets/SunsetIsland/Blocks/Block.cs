using System.Collections.Generic;
using Assets.SunsetIsland.Common;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Config.Data;
using Assets.SunsetIsland.Managers;
using Assets.SunsetIsland.Utilities;
using UnityEngine;

namespace Assets.SunsetIsland.Blocks
{
    public class Block : IBlock
    {
        private readonly BlockData _blockData;
        private readonly Dictionary<FaceDirection, FaceDirection> _rotate;
        private readonly int _hash;

        public Block(int blockId, byte rotation, ushort[] tints)
        {
            _blockData = BlockFactory.BlockDictionary.GetStaticData(blockId);
            Tints = tints;
            _hash = BlockFactory.Hash(this);
            Rotation = rotation;
            //If the block is rotated, cache it off since the block is immutable
            if (Rotation != 0)
            {
                _rotate = new Dictionary<FaceDirection, FaceDirection>();
                var vector = rotation.RotationToVector3();
                for (var i = 0; i < 6; ++i)
                {
                    var direction = (FaceDirection) i;
                    _rotate[direction] = General.Rotate(direction, vector);
                }
            }
        }

        public int BlockId => _blockData.BlockId;

        public int GetTextureMapping(FaceDirection faceDir)
        {
            if (Rotation != 0)
                faceDir = _rotate[faceDir];
            return BlockFactory.BlockDictionary.GetTextureMapping(BlockId, faceDir);
        }

        public Color32 GetColor(FaceDirection faceDir)
        {
            if (Rotation != 0)
                faceDir = _rotate[faceDir];
            return Tints[(int) faceDir].Convert32();
        }

        //16-bit HighColor (5/6/5)
        public ushort[] Tints { get; }

        public byte Rotation { get; }

        public uint Emissivity => _blockData.Emissivity;

        public uint Opacity => _blockData.Opacity;

        public bool RenderOpaque => _blockData.RenderOpaque;

        public bool AddToRenderMesh => _blockData.AddToRenderMesh;

        public bool AddToPhysicsMesh => _blockData.AddToPhysicsMesh;

        public bool Equals(Block other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            var block = obj as Block;
            return !Equals(block, null) && Equals(block);
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        public static bool operator ==(Block b1, Block b2)
        {
            var b1Null = Equals(b1, null);
            var b2Null = Equals(b2, null);
            return b1Null && b2Null ||
                   !b1Null &&
                   !b2Null &&
                   b1.BlockId == b2.BlockId &&
                   b1.Rotation == b2.Rotation &&
                   b1.Tints[(int) FaceDirection.XDecreasing] == b2.Tints[(int) FaceDirection.XDecreasing] &&
                   b1.Tints[(int) FaceDirection.XIncreasing] == b2.Tints[(int) FaceDirection.XIncreasing] &&
                   b1.Tints[(int) FaceDirection.YDecreasing] == b2.Tints[(int) FaceDirection.YDecreasing] &&
                   b1.Tints[(int) FaceDirection.YIncreasing] == b2.Tints[(int) FaceDirection.YIncreasing] &&
                   b1.Tints[(int) FaceDirection.ZDecreasing] == b2.Tints[(int) FaceDirection.ZDecreasing] &&
                   b1.Tints[(int) FaceDirection.ZIncreasing] == b2.Tints[(int) FaceDirection.ZIncreasing];
        }

        public static bool operator !=(Block b1, Block b2)
        {
            return !(b1 == b2);
        }
    }
}