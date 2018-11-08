using System.Collections.Generic;

namespace Assets.SunsetIsland.Collections.Pools
{
    public class ObjectPool<T> : IPool<T> where T : new()
    {
        protected readonly int _capacity;
        protected readonly Stack<T> _pool;
        protected object _lock;

        public ObjectPool(int capacity = -1) : this(new Stack<T>(), capacity)
        {
        }

        public ObjectPool(IEnumerable<T> initialSet, int capacity = -1)
        {
            _lock = new object();
            _pool = new Stack<T>(initialSet);
            _capacity = capacity;
        }

        public T Pop()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                    return _pool.Pop();
                return new T();
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