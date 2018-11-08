using System;
using System.Collections.Generic;

namespace Assets.SunsetIsland.Collections.Pools
{
    public class ArrayPool<T> : IPool<T>
    {
        protected readonly int _capacity;
        protected readonly int _length;
        protected readonly Stack<T> _pool;
        protected object _lock;

        public ArrayPool(int rank, int capacity = -1) : this(new Stack<T>(), rank, capacity)
        {
        }

        public ArrayPool(IEnumerable<T> initialSet, int length, int capacity = -1)
        {
            if (!typeof(T).IsArray)
                throw new ArgumentException(typeof(T).Name + " is not an array");
            _lock = new object();
            _pool = new Stack<T>(initialSet);
            _length = length;
            _capacity = capacity;
        }

        public T Pop()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                    return _pool.Pop();
                var arrType = typeof(T);
                var type = arrType.GetElementType();
                if (type == null)
                    throw new ArgumentException(typeof(T).Name + " is not an array");
                var rank = arrType.GetArrayRank();
                object arr;
                switch (rank)
                {
                    case 1:
                        arr = Array.CreateInstance(type, _length);
                        break;
                    case 2:
                        arr = Array.CreateInstance(type, _length, _length);
                        break;
                    case 3:
                        arr = Array.CreateInstance(type, _length, _length, _length);
                        break;
                    default:
                        throw new IndexOutOfRangeException($"{rank} is out of bounds");
                }
                return (T) arr; //THIS CAST
            }
        }

        public bool Push(T item)
        {
            lock (_lock)
            {
                if (_capacity > 0 && _pool.Count >= _capacity)
                    return false;
                _pool.Push(item);
                return true;
            }
        }

        public void Evict()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }

        public int Count => _pool.Count;
    }
}