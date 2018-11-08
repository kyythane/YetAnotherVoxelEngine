using UnityEngine;

namespace Assets.SunsetIsland.Common
{
    public static class Rotations
    {
        public static byte RotationToByte(this Vector3 rotation)
        {
            var x = (int) rotation.x / 90 % 4;
            var y = (int) rotation.y / 90 % 4;
            var z = (int) rotation.z / 90 % 4;
            return (byte) ((x << 4) | (y << 2) | z);
        }

        public static byte RotationToByte(this Vector3Int rotation)
        {
            var x = rotation.x / 90 % 4;
            var y = rotation.y / 90 % 4;
            var z = rotation.z / 90 % 4;
            return (byte) ((x << 4) | (y << 2) | z);
        }

        public static Vector3 RotationToVector3(this byte b)
        {
            return new Vector3((b >> 4) & 3, (b >> 2) & 3, b & 3) * 90;
        }

        public static Vector3Int RotationToVector3Int(this byte b)
        {
            return new Vector3Int((b >> 4) & 3, (b >> 2) & 3, b & 3) * 90;
        }
    }
}