using System;
using UnityEngine;

namespace Assets.SunsetIsland.Utilities
{
    public static class MathUtilities
    {
        private static float[] s_biasTable;

        public static ulong NextRand(ulong seed)
        {
            return 6364136223846793005 * seed + 1442695040888963407;
        }

        public static float RandFloat(ulong seed, out ulong nextSeed)
        {
            nextSeed = NextRand(seed);
            return (float) (nextSeed / (double) ulong.MaxValue);
        }

        public static int RandInt(ulong seed, out ulong nextSeed, int min, int max)
        {
            nextSeed = NextRand(seed);
            return min + (int) (max * (nextSeed / (double) ulong.MaxValue));
        }

        public static float Bias(float value, float bias)
        {
            if (s_biasTable != null)
                return (float) Math.Pow(value, s_biasTable[(int) (bias * 100)]);
            s_biasTable = new float[101];
            for (var i = 0; i < 101; ++i)
                s_biasTable[i] = (float) (Math.Log(i * 0.01) / Math.Log(0.5));
            return (float) Math.Pow(value, s_biasTable[(int) (bias * 100)]);
        }

        public static float Gain(float value, float gain)
        {
            float ret;
            if (value < 0.5f)
                ret = Bias(2 * value, 1 - gain) * 0.5f;
            else
                ret = 1 - Bias(2 - 2 * value, 1 - gain) * 0.5f;
            return ret;
        }

        public static int FastFloor(float f)
        {
            return f > 0 ? (int) f : (int) f - 1;
        }

        public static int FastFloor(double d)
        {
            return d > 0 ? (int) d : (int) d - 1;
        }

        public static int Modulo(int x, int m)
        {
            return (x % m + m) % m;
        }

        public static float DistanceFromPointToLineSegment(Vector2 point, Vector2 anchor, Vector2 end)
        {
            var d = end - anchor;
            var length = d.magnitude;
            if (Math.Abs(length) < 0.0001)
                return (point - anchor).magnitude;
            d.Normalize();
            var intersect = Vector2.Dot(point - anchor, d);
            if (intersect < 0)
                return (point - anchor).magnitude;
            return intersect > length ? (point - end).magnitude : (point - (anchor + d * intersect)).magnitude;
        }

        public static float DistanceFromPointToLineSegment(Vector3 point, Vector3 anchor, Vector3 end)
        {
            var d = end - anchor;
            var length = d.magnitude;
            if (Math.Abs(length) < 0.0001)
                return (point - anchor).magnitude;
            d.Normalize();
            var intersect = Vector3.Dot(point - anchor, d);
            if (intersect < 0)
                return (point - anchor).magnitude;
            return intersect > length ? (point - end).magnitude : (point - (anchor + d * intersect)).magnitude;
        }

        public static int NearestPowerOf2(int number)
        {
            var n = number > 0 ? number - 1 : 0;

            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;

            return n;
        }
    }
}