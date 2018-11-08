using System.Collections.Generic;
using System.Linq;
using Assets.SunsetIsland.Generation;
using Assets.SunsetIsland.Utilities.Random;
using UnityEngine;

namespace Assets.SunsetIsland.Managers
{
    public static class GenerationManager
    {
        private static GlobalGenerator _generator;
        private static List<Vector3Int> _treePositions;

        public static void Initialize(uint seed)
        {
            _generator = new GlobalGenerator(seed);
            _treePositions = new List<Vector3Int>();
            var rand = new FastRandom(seed);
            for (var i = 0; i < 5000; i++)
            {
                var sample = new Vector2Int((int) ((rand.NextDouble() * 100 - 50) * 32),
                                            (int) ((rand.NextDouble() * 100 - 50) * 32));
                var height = _generator.GetHeight(sample.x, sample.y);
                _treePositions.Add(new Vector3Int(sample.x, height, sample.y));
            }
        }

        public static int Range(int min, int max)
        {
            return _generator.Range(min, max);
        }

        public static int GetHeight(int x, int y)
        {
            return _generator.GetHeight(x, y);
        }

        public static List<Vector3Int> GetPlants(Vector4 rect)
        {
            return _treePositions.Where(p => p.x >= rect.x && p.x <= rect.z && p.z >= rect.y && p.z <= rect.w).ToList();
        }
    }
}