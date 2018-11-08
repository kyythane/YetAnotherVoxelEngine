using System;
using Assets.SunsetIsland.Utilities.Noise;
using Assets.SunsetIsland.Utilities.Random;
using UnityEngine;

/*
 * http://procworld.blogspot.de/2013/10/water-bodies.html
 * http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/
 * https://procworld.blogspot.co.uk/2016/04/geometry-is-destiny.html
 * https://procworld.blogspot.co.uk/2016/07/geometry-is-destiny-part-2.html
 */

namespace Assets.SunsetIsland.Generation
{
    public class GlobalGenerator
    {
        private const float FeatureScale = 256;
        private readonly SimplexNoise2D _heightMap0;
        private readonly FastRandom _random;
        private readonly System.Random _sysRand;

        public GlobalGenerator(uint seed)
        {
            _random = new FastRandom(seed);
            _sysRand = new System.Random((int)seed);
            _heightMap0 = new SimplexNoise2D(_random.NextUInt());
        }

        public int[,] GetHeightMap(Vector4 bounds)
        {
            var grid = new int[(int) bounds.z, (int) bounds.w];
            for (var x = 0; x < (int) bounds.z; x++)
            {
                for (var y = 0; y < (int) bounds.w; y++)
                    grid[x, y] = GetHeight((int) bounds.x + x, (int) bounds.y + y);
            }
            return grid;
        }

        public int GetHeight(int x, int y)
        {
            return (int) (128 * _heightMap0.RidgedMultiFractal(x, y, FeatureScale, 0.1f, 0.8f, 8));
        }

        public int Range(int min, int max)
        {
            return _sysRand.Next(min, max);
        }
    }
}