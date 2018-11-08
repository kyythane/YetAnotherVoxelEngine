using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.SunsetIsland.Utilities.Noise
{
    public sealed class TiledNoise2D
    {
        private const int TileSize = 2500;
        private const float ScaleFactor = 25;
        private const float Radius = 128;
        public static readonly List<ulong> Seeds = new List<ulong>();
        private static readonly Dictionary<ulong, float[][]> NoiseFields = new Dictionary<ulong, float[][]>();

        public TiledNoise2D(ulong seed)
        {
            if (!NoiseFields.ContainsKey(seed))
            {
                var noiseField = new float[TileSize][];
                var generator = new SimplexNoise4D(seed);
                for (var row = 0; row < TileSize; ++row)
                {
                    var noiseRow = new float[TileSize];
                    var fNx = row / Radius;
                    var fRdx = fNx * 2 * (float) Math.PI;
                    for (var column = 0; column < TileSize; ++column)
                    {
                        var fNy = column / Radius;
                        var fRdy = fNy * 2 * (float) Math.PI;
                        var x = Radius * Math.Sin(fRdx) / ScaleFactor;
                        var y = Radius * Math.Cos(fRdx) / ScaleFactor;
                        var z = Radius * Math.Sin(fRdy) / ScaleFactor;
                        var w = Radius * Math.Cos(fRdy) / ScaleFactor;
                        noiseRow[column] = generator.Perlin(x, y, z, w);
                    }
                    noiseField[row] = noiseRow;
                }
                Seeds.Add(seed);
                NoiseFields[seed] = noiseField;
            }
            NoiseField = NoiseFields[seed];
        }

        public float[][] NoiseField { get; }

        public float RidgedMultiFractal(float x, float y, float scale, float offset, float gain, int octaves)
        {
            float rValue = 0;
            var position = new Vector2(x / scale, y / scale);
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

        public float FractalBrownianMotion(float x, float y, float scale, float offset, int octaves)
        {
            float rValue = 0;
            var position = new Vector2(x / scale, y / scale);
            float divisor = 0;
            for (var i = 0; i < octaves; ++i)
            {
                var octaveScale = (float) Math.Pow(2, i);
                divisor += 1.0f / octaveScale;
                rValue += 1.0f / octaveScale * (Perlin(position * octaveScale) + offset);
            }
            return rValue / divisor;
        }

        private float Perlin(Vector2 pos)
        {
            return Perlin(pos.x, pos.y);
        }

        private float Perlin(float pX, float pY)
        {
            var iX = (int) (pX * ScaleFactor);
            var rX = pX * ScaleFactor - iX;
            var iY = (int) (pY * ScaleFactor);
            var rY = pY * ScaleFactor - iY;
            //using "screenspace" so it is easier to debug. 
            var sUl = NoiseField[iX % TileSize][iY % TileSize];
            var sUr = NoiseField[(iX + 1) % TileSize][iY % TileSize];
            var sBl = NoiseField[iX % TileSize][(iY + 1) % TileSize];
            var sBr = NoiseField[(iX + 1) % TileSize][(iY + 1) % TileSize];

            var lerpU = Mathf.Lerp(sUl, sUr, rX);
            var lerpB = Mathf.Lerp(sBl, sBr, rX);

            return Mathf.Lerp(lerpU, lerpB, rY);
        }
    }
}