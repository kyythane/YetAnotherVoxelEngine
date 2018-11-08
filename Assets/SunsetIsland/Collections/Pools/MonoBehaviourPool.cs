using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.SunsetIsland.Collections.Pools
{
    public class MonoBehaviourPool<T> : IPool<T> where T : MonoBehaviour
    {
        protected readonly int _capacity;
        protected readonly Stack<T> _pool;
        protected readonly T _prefab;
        protected object _lock;

        public MonoBehaviourPool(T prefab, int capacity = -1) : this(prefab, new Stack<T>(), capacity)
        {
        }

        public MonoBehaviourPool(T prefab, [NotNull] IEnumerable<T> initialSet, int capacity = -1)
        {
            _prefab = prefab;
            _lock = new object();
            _pool = new Stack<T>(initialSet);
            _capacity = capacity;
        }

        public T Pop()
        {
            lock (_lock)
            {
                return _pool.Count > 0 ? _pool.Pop() : Object.Instantiate(_prefab);
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