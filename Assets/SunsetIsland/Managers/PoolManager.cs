using System;
using System.Collections.Generic;
using Assets.SunsetIsland.Collections.Pools;
using UnityEngine;

namespace Assets.SunsetIsland.Managers
{
    public static class PoolManager
    {
        private static readonly Dictionary<Type, IPool> ObjectPools = new Dictionary<Type, IPool>();

        private static readonly Dictionary<Type, Dictionary<int, IPool>> ArrayPools =
            new Dictionary<Type, Dictionary<int, IPool>>();

        private static readonly Dictionary<Type, IPool> MonoPools = new Dictionary<Type, IPool>();
        private static readonly object Lock = new object();

        public static ObjectPool<T> GetObjectPool<T>() where T : new()
        {
            var type = typeof(T);
            lock (Lock)
            {
                if (!ObjectPools.ContainsKey(type))
                    ObjectPools[type] = new ObjectPool<T>();
                return (ObjectPool<T>) ObjectPools[type];
            }
        }

        public static MonoBehaviourPool<T> GetMonoBehaviourPool<T>(T prefab) where T : MonoBehaviour
        {
            var type = typeof(T);
            lock (Lock)
            {
                if (!MonoPools.ContainsKey(type))
                    MonoPools[type] = new MonoBehaviourPool<T>(prefab);
                return (MonoBehaviourPool<T>) MonoPools[type];
            }
        }

        public static ArrayPool<T> GetArrayPool<T>(int arrayLength)
        {
            var type = typeof(T);
            lock (Lock)
            {
                if (!ArrayPools.ContainsKey(type))
                    ArrayPools[type] = new Dictionary<int, IPool>();
                if (!ArrayPools[type].ContainsKey(arrayLength))
                    ArrayPools[type][arrayLength] = new ArrayPool<T>(arrayLength);
                return (ArrayPool<T>) ArrayPools[type][arrayLength];
            }
        }

        public static void Evict()
        {
            lock (Lock)
            {
                ObjectPools.Clear();
                ArrayPools.Clear();
                MonoPools.Clear();
            }
        }

        public static void Evict<T>()
        {
            var type = typeof(T);
            lock (Lock)
            {
                if (ObjectPools.ContainsKey(type))
                    ObjectPools.Remove(type);
                else if (MonoPools.ContainsKey(type))
                    MonoPools.Remove(type);
                else if (ArrayPools.ContainsKey(type))
                    ArrayPools.Remove(type);
            }
        }

        public static IEnumerable<KeyValuePair<string, int>> PoolCounts()
        {
            foreach (var objectPool in ObjectPools)
                yield return new KeyValuePair<string, int>(objectPool.Key.Name, objectPool.Value.Count);
            foreach (var monoPool in MonoPools)
                yield return new KeyValuePair<string, int>(monoPool.Key.Name, monoPool.Value.Count);
            foreach (var arrayPoolSet in ArrayPools)
            {
                foreach (var arrayPool in arrayPoolSet.Value)
                    yield return new KeyValuePair<string, int>($"{arrayPoolSet.Key.Name}_{arrayPool.Key}",
                                                               arrayPool.Value.Count);
            }
        }
    }
}