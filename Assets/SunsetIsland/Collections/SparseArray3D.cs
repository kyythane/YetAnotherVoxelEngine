using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.SunsetIsland.Collections
{
    public class SparseArray3D<T> : Dictionary<long, T>
    {
        private const long stride1 = 1 << 16;
        private const long stride2 = stride1 * stride1;
        private readonly int _bucketShift;

        public SparseArray3D()
        {
            _bucketShift = 0;
        }

        public SparseArray3D(int capacity) : base(capacity)
        {
            _bucketShift = 0;
        }

        public SparseArray3D(int capacity, int bucketSize) : base(capacity)
        {
            _bucketShift = (int) Math.Log(bucketSize, 2);
        }

        public T this[int x, int y, int z]
        {
            get
            {
                T @out;
                TryGetValue(Hash(x, y, z), out @out);
                return @out;
            }
            set { this[Hash(x, y, z)] = value; }
        }

        public T this[Vector3Int pos]
        {
            get
            {
                T @out;
                TryGetValue(Hash(pos.x, pos.y, pos.z), out @out);
                return @out;
            }
            set { this[Hash(pos.x, pos.y, pos.z)] = value; }
        }

        public T this[Vector3 pos]
        {
            get
            {
                T @out;
                TryGetValue(Hash((int) pos.x, (int) pos.y, (int) pos.z), out @out);
                return @out;
            }
            set { this[Hash((int) pos.x, (int) pos.y, (int) pos.z)] = value; }
        }

        public bool ContainsKey(int x, int y, int z)
        {
            return ContainsKey(Hash(x, y, z));
        }

        public bool Remove(int x, int y, int z)
        {
            return Remove(Hash(x, y, z));
        }

        public bool ContainsKey(Vector3Int pos)
        {
            return ContainsKey(Hash(pos.x, pos.y, pos.z));
        }

        public bool Remove(Vector3Int pos)
        {
            return Remove(Hash(pos.x, pos.y, pos.z));
        }

        public bool ContainsKey(Vector3 pos)
        {
            return ContainsKey(Hash((int) pos.x, (int) pos.y, (int) pos.z));
        }

        public bool Remove(Vector3 pos)
        {
            return Remove(Hash((int) pos.x, (int) pos.y, (int) pos.z));
        }

        public long Hash(int x, int y, int z)
        {
            unchecked
            {
                return (x >> _bucketShift) * stride1 + (y >> _bucketShift) + (z >> _bucketShift) * stride2;
            }
        }
    }
}