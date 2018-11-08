using System;
using System.Collections.Generic;
using Assets.SunsetIsland.Common;
using Assets.SunsetIsland.Common.Enums;
using UnityEngine;

namespace Assets.SunsetIsland.Utilities
{
    public static class General
    {
        public static Dictionary<FaceDirection, Vector3> FaceVectors = new Dictionary<FaceDirection, Vector3>
        {
            {FaceDirection.XIncreasing, new Vector3(1, 0, 0)},
            {FaceDirection.YIncreasing, new Vector3(0, 1, 0)},
            {FaceDirection.ZIncreasing, new Vector3(0, 0, 1)},
            {FaceDirection.XDecreasing, new Vector3(-1, 0, 0)},
            {FaceDirection.YDecreasing, new Vector3(0, -1, 0)},
            {FaceDirection.ZDecreasing, new Vector3(0, 0, -1)},
            {FaceDirection.None, new Vector3(0, 0, 0)},
        };
        
        public static Dictionary<FaceDirection, Vector3Int> FaceVectorsInt = new Dictionary<FaceDirection, Vector3Int>
        {
            {FaceDirection.XIncreasing, new Vector3Int(1, 0, 0)},
            {FaceDirection.YIncreasing, new Vector3Int(0, 1, 0)},
            {FaceDirection.ZIncreasing, new Vector3Int(0, 0, 1)},
            {FaceDirection.XDecreasing, new Vector3Int(-1, 0, 0)},
            {FaceDirection.YDecreasing, new Vector3Int(0, -1, 0)},
            {FaceDirection.ZDecreasing, new Vector3Int(0, 0, -1)},
            {FaceDirection.None, new Vector3Int(0, 0, 0)},
        };
        public static int BlockIndex(int x, int y, int z, int chunkSize)
        {
            var wrapX = MathUtilities.Modulo(x, chunkSize);
            var wrapY = MathUtilities.Modulo(y, chunkSize);
            var wrapZ = MathUtilities.Modulo(z, chunkSize);
            var flattenIndex = (wrapX * chunkSize + wrapZ) * chunkSize + wrapY;
            return flattenIndex;
        }
        
        public static int BlockIndex(int x, int y, int z, int width, int height)
        {
            var wrapX = MathUtilities.Modulo(x, width);
            var wrapY = MathUtilities.Modulo(y, height);
            var wrapZ = MathUtilities.Modulo(z, width);
            var flattenIndex = (wrapX * width + wrapZ) * height + wrapY;
            return flattenIndex;
        }
        
        public static Vector3Int Unmap(int index, int chunkSize)
        {
            var y = index % chunkSize;
            index /= chunkSize;
            var z = index % chunkSize;
            index /= chunkSize;
            return new Vector3Int(index, y, z);
        }
        
        public static Vector3Int Unmap(int index, int width, int height)
        {
            var y = index % height;
            index /= height;
            var z = index % width;
            index /= width;
            return new Vector3Int(index, y, z);
        }

        public static Vector3Int Neighbor(Vector3Int pos, FaceDirection direction)
        {
            return Neighbor(pos.x, pos.y, pos.z, direction);
        }

        public static Vector3Int Neighbor(int x, int y, int z, FaceDirection direction)
        {
            switch (direction)
            {
                case FaceDirection.XIncreasing:
                    return new Vector3Int(x + 1, y, z);
                case FaceDirection.YIncreasing:
                    return new Vector3Int(x, y + 1, z);
                case FaceDirection.ZIncreasing:
                    return new Vector3Int(x, y, z + 1);
                case FaceDirection.XDecreasing:
                    return new Vector3Int(x - 1, y, z);
                case FaceDirection.YDecreasing:
                    return new Vector3Int(x, y - 1, z);
                case FaceDirection.ZDecreasing:
                    return new Vector3Int(x, y, z - 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static FaceDirection Rotate(FaceDirection face, byte rotation)
        {
            var vector = rotation.RotationToVector3();
            return Rotate(face, vector);
        }

        public static FaceDirection Rotate(FaceDirection face, Vector3Int rotation)
        {
            return Rotate(face, (Vector3) rotation);
        }

        //TODO : rewrite rotation
        public static FaceDirection Rotate(FaceDirection face, Vector3 rotation)
        {
            var vector = FaceVectors[face];
            var quat = Quaternion.Euler(rotation);
            return PrincipleFace(quat * vector);
        }

        public static FaceDirection Rotate(FaceDirection face, int x, int y, int z)
        {
            return Rotate(face, new Vector3(x, y, z));
        }

        public static FaceDirection PrincipleFace(Vector3 direction)
        {
            var norm = direction.normalized;
            var maxDot = -1.0f;
            var faceDir = FaceDirection.None;
            foreach (var faceVector in FaceVectors)
            {
                var dot = Vector3.Dot(faceVector.Value, norm);
                if (dot < maxDot) continue;
                maxDot = dot;
                faceDir = faceVector.Key;
            }

            return faceDir;
        }
        
        public static Vector3 PrincipleVector(Vector3 direction)
        {
            var norm = direction.normalized;
            var maxDot = -1.0f;
            var faceDir = new Vector3();
            foreach (var faceVector in FaceVectors)
            {
                var dot = Vector3.Dot(faceVector.Value, norm);
                if (dot < maxDot) continue;
                maxDot = dot;
                faceDir = faceVector.Value;
            }

            return faceDir;
        }
        
        public static KeyValuePair<FaceDirection, Vector3> PrincipleFromVector(Vector3 direction)
        {
            var norm = direction.normalized;
            var maxDot = -1.0f;
            var faceDir = new KeyValuePair<FaceDirection, Vector3>();
            foreach (var faceVector in FaceVectors)
            {
                var dot = Vector3.Dot(faceVector.Value, norm);
                if (dot < maxDot) continue;
                maxDot = dot;
                faceDir = faceVector;
            }
            return faceDir;
        }
        
        public static FaceDirection FlipDirection(FaceDirection face)
        {
            switch (face)
            {
                case FaceDirection.XIncreasing:
                    return FaceDirection.XDecreasing;
                case FaceDirection.YIncreasing:
                    return FaceDirection.YDecreasing;
                case FaceDirection.ZIncreasing:
                    return FaceDirection.ZDecreasing;
                case FaceDirection.XDecreasing:
                    return FaceDirection.XIncreasing;
                case FaceDirection.YDecreasing:
                    return FaceDirection.YIncreasing;
                case FaceDirection.ZDecreasing:
                    return FaceDirection.ZIncreasing;
                case FaceDirection.None:
                    return FaceDirection.None;
                default:
                    throw new ArgumentOutOfRangeException(nameof(face), face, null);
            }
        }

        public static int BlockIndex(Vector3Int blockPosition, int chunkSize)
        {
            var wrapX = MathUtilities.Modulo(blockPosition.x, chunkSize);
            var wrapY = MathUtilities.Modulo(blockPosition.y, chunkSize);
            var wrapZ = MathUtilities.Modulo(blockPosition.z, chunkSize);
            var flattenIndex = (wrapX * chunkSize + wrapZ) * chunkSize + wrapY;
            return flattenIndex;
        }
        
        public static Vector3 Clamp(Vector3 posiiton, Bounds maskBounds)
        {
            posiiton.x = Mathf.Clamp(posiiton.x, maskBounds.min.x, maskBounds.max.x);
            posiiton.y = Mathf.Clamp(posiiton.y, maskBounds.min.y, maskBounds.max.y);
            return posiiton;
        }
    }
}