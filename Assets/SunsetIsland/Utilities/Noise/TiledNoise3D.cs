using System;
using UnityEngine;

namespace Assets.SunsetIsland.Utilities.Noise
{
    public class TiledNoise3D : TiledPseudoNoise
    {
        public TiledNoise3D(ulong seed) : base(seed, 3)
        {
        }

        public float RidgedMultiFractal(float x, float y, float z, float scale, float offset, float gain, int octaves)
        {
            float rValue = 0;
            var position = new Vector3(x / scale, y / scale, z / scale);
            float divisor = 0;
            float weight = 1;
            for (var i = 0; i < octaves; ++i)
            {
                var octaveScale = (float) Math.Pow(2, i);
                divisor += 1.0f / octaveScale;
                var signal = Perlin(position * octaveScale);
                signal = offset - (signal < 0 ? -signal : signal);
                signal *= signal;
                signal *= weight;
                weight = signal * gain;
                if (weight > 1.0f) weight = 1.0f;
                if (weight < 0.0f) weight = 0.0f;
                rValue += signal * (1.0f / octaveScale);
            }

            return rValue / divisor;
        }

        public float FractalBrownianMotion(float x, float y, float z, float scale, float offset, int octaves)
        {
            float rValue = 0;
            var position = new Vector3(x / scale, y / scale, z / scale);
            float divisor = 0;
            for (var i = 0; i < octaves; ++i)
            {
                var octaveScale = (float) Math.Pow(2, i);
                divisor += 1.0f / octaveScale;
                rValue += 1.0f / octaveScale * (Perlin(position * octaveScale) + offset);
            }
            return rValue / divisor;
        }

        private float Perlin(Vector3 pos)
        {
            return Perlin(pos.x, pos.y, pos.z);
        }

        private float Perlin(double pX, double pY, double pZ)
        {
            var sampleX = NoiseField[0][(int) (pX * ScaleFactor) % TileSize];
            var sampleY = NoiseField[1][(int) (pY * ScaleFactor) % TileSize];
            var sampleZ = NoiseField[2][(int) (pZ * ScaleFactor) % TileSize];
            return (sampleX + sampleY + sampleZ) / 3;
        }
    }
}