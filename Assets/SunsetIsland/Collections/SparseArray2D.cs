using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.SunsetIsland.Collections
{
    public class SparseArray2D<T> : Dictionary<long, T>
    {
        private const long stride1 = 1 << 24;
        private readonly int _bucketShift;

        public SparseArray2D()
        {
            _bucketShift = 0;
        }

        public SparseArray2D(int bucketSize)
        {
            _bucketShift = (int) Math.Log(bucketSize, 2);
        }

        public T this[int x, int y]
        {
            get
            {
                T @out;
                TryGetValue(Hash(x, y), out @out);
                return @out;
            }
            set { this[Hash(x, y)] = value; }
        }

        public T this[Vector2Int pos]
        {
            get
            {
                T @out;
                TryGetValue(Hash(pos.x, pos.y), out @out);
                return @out;
            }
            set { this[Hash(pos.x, pos.y)] = value; }
        }

        public T this[Vector2 pos]
        {
            get
            {
                T @out;
                TryGetValue(Hash((int) pos.x, (int) pos.y), out @out);
                return @out;
            }
            set { this[Hash((int) pos.x, (int) pos.y)] = value; }
        }

        public bool ContainsKey(int x, int y)
        {
            return ContainsKey(Hash(x, y));
        }

        public bool Remove(int x, int y)
        {
            return Remove(Hash(x, y));
        }

        public bool ContainsKey(Vector2Int pos)
        {
            return ContainsKey(Hash(pos.x, pos.y));
        }

        public bool Remove(Vector2Int pos)
        {
            return Remove(Hash(pos.x, pos.y));
        }

        public bool ContainsKey(Vector2 pos)
        {
            return ContainsKey(Hash((int) pos.x, (int) pos.y));
        }

        public bool Remove(Vector2 pos)
        {
            return Remove(Hash((int) pos.x, (int) pos.y));
        }

        public long Hash(int x, int y)
        {
            unchecked
            {
                return (x >> _bucketShift) * stride1 + (y >> _bucketShift);
            }
        }
    }
}