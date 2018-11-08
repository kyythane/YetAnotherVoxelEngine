using System;

namespace Assets.SunsetIsland.Utilities
{
    public struct Interval<T> : IComparable<Interval<T>>, IEquatable<Interval<T>>
    {
        public int Min { get; }
        public int Max { get; }
        public T Data { get; }

        public Interval(int point, T data) : this(point, point, data)
        {
        }

        public Interval(int min, int max, T data) : this()
        {
            if (max < min)
            {
                var temp = min;
                min = max;
                max = temp;
            }
            Min = min;
            Max = max;
            Data = data;
        }

        public int CompareTo(Interval<T> other)
        {
            if (other.Min > Max)
                return -1;
            return other.Max < Min ? 1 : 0;
        }

        public bool Equals(Interval<T> other)
        {
            return Min == other.Min && Max == other.Max;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Interval<T> && Equals((Interval<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Min.GetHashCode() * 397) ^ Max.GetHashCode();
            }
        }
    }
}