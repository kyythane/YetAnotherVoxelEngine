using System;
using System.Collections.Generic;

namespace Assets.SunsetIsland.Common
{
    public static class CollectionExtensions
    {
        public static ushort ClosestTo(this IEnumerable<ushort> collection, ushort target)
        {
            var closest = ushort.MaxValue;
            var minDifference = int.MaxValue;
            foreach (var element in collection)
            {
                var difference = Math.Abs(element - target);
                if (minDifference <= difference)
                    continue;
                minDifference = difference;
                closest = element;
            }
            return closest;
        }

        public static short ClosestTo(this IEnumerable<short> collection, short target)
        {
            var closest = short.MaxValue;
            var minDifference = int.MaxValue;
            foreach (var element in collection)
            {
                var difference = Math.Abs(element - target);
                if (minDifference <= difference)
                    continue;
                minDifference = difference;
                closest = element;
            }
            return closest;
        }

        public static int ClosestTo(this IEnumerable<int> collection, int target)
        {
            var closest = int.MaxValue;
            var minDifference = int.MaxValue;
            foreach (var element in collection)
            {
                var difference = Math.Abs((long) element - target);
                if (minDifference <= difference)
                    continue;
                minDifference = (int) difference;
                closest = element;
            }
            if (closest == int.MaxValue)
                throw new IndexOutOfRangeException("Collection contains no elements!");

            return closest;
        }

        public static float ClosestTo(this IEnumerable<float> collection, float target)
        {
            var closest = float.NaN;
            var minDifference = float.MaxValue;
            foreach (var element in collection)
            {
                var difference = Math.Abs((double) element - target);
                if (minDifference <= difference)
                    continue;
                minDifference = (float) difference;
                closest = element;
            }
            if (float.IsNaN(closest))
                throw new IndexOutOfRangeException("Collection contains no elements!");
            return closest;
        }
    }
}