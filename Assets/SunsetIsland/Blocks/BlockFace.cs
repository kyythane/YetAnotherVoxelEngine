using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Assets.SunsetIsland.Blocks
{
    //Does not use auto props so we don't get a copy on access
    public struct BlockFace
    {
        public bool Equals(BlockFace other)
        {
            return Exists == other.Exists &&
                   BlockId == other.BlockId &&
                   TextureIndex == other.TextureIndex &&
                   Tint == other.Tint &&
                   LightTopLeft.Equals(other.LightTopLeft) &&
                   LightTopRight.Equals(other.LightTopRight) &&
                   LightBottomLeft.Equals(other.LightBottomLeft) &&
                   LightBottomRight.Equals(other.LightBottomRight);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BlockFace && Equals((BlockFace) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Exists.GetHashCode();
                hashCode = (hashCode * 397) ^ BlockId;
                hashCode = (hashCode * 397) ^ TextureIndex;
                hashCode = (hashCode * 397) ^ Tint.GetHashCode();
                hashCode = (hashCode * 397) ^ LightTopLeft.GetHashCode();
                hashCode = (hashCode * 397) ^ LightTopRight.GetHashCode();
                hashCode = (hashCode * 397) ^ LightBottomLeft.GetHashCode();
                hashCode = (hashCode * 397) ^ LightBottomRight.GetHashCode();
                return hashCode;
            }
        }

        public bool Exists;
        public int BlockId;
        public int TextureIndex;
        public ushort Tint;
        public uint LightTopLeft;
        public uint LightTopRight;
        public uint LightBottomLeft;
        public uint LightBottomRight;

        public static bool operator !=(BlockFace a, BlockFace b)
        {
            return !(a == b);
        }

        public static bool operator ==(BlockFace a, BlockFace b)
        {
            return a.Exists == b.Exists &&
                   a.BlockId == b.BlockId &&
                   a.TextureIndex == b.TextureIndex &&
                   a.LightTopLeft == b.LightTopLeft &&
                   a.LightTopRight == b.LightTopRight &&
                   a.LightBottomRight == b.LightBottomRight &&
                   a.LightBottomLeft == b.LightBottomLeft &&
                   a.Tint == b.Tint;
        }
    }
}