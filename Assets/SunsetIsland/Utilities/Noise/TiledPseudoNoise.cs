using System;
using System.Collections.Generic;

namespace Assets.SunsetIsland.Utilities.Noise
{
    public abstract class TiledPseudoNoise
    {
        protected const int TileSize = 10000;
        protected const float ScaleFactor = 10;
        protected const float Radius = 128;
        public static readonly Dictionary<int, List<ulong>> Seeds = new Dictionary<int, List<ulong>>();

        protected static readonly Dictionary<int, Dictionary<ulong, float[][]>> NoiseFields =
            new Dictionary<int, Dictionary<ulong, float[][]>>();

        protected TiledPseudoNoise(ulong seed, int dimensionality)
        {
            if (!NoiseFields.ContainsKey(dimensionality))
                NoiseFields[dimensionality] = new Dictionary<ulong, float[][]>();
            var fields = NoiseFields[dimensionality];
            if (!fields.ContainsKey(seed))
            {
                var noiseField = new float[dimensionality][];
                for (var dim = 0; dim < dimensionality; ++dim)
                {
                    var generator = new SimplexNoise2D(seed);
                    seed = MathUtilities.NextRand(seed);
                    var noiseRow = new float[TileSize];
                    for (var i = 0; i < TileSize; ++i)
                    {
                        var fNx = i / Radius;
                        var fRdx = fNx * 2 * (float) Math.PI;
                        var x = Radius * Math.Sin(fRdx) / ScaleFactor;
                        var y = Radius * Math.Cos(fRdx) / ScaleFactor;
                        noiseRow[i] = generator.Perlin(x, y);
                    }
                    noiseField[dim] = noiseRow;
                }
                if (!Seeds.ContainsKey(dimensionality))
                    Seeds[dimensionality] = new List<ulong>();
                Seeds[dimensionality].Add(seed);
                fields[seed] = noiseField;
            }
            NoiseField = fields[seed];
        }

        public float[][] NoiseField { get; }
    }
}