using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.SunsetIsland.Initialization
{
    public class DependencyGraph<T>
    {
        private readonly Dictionary<T, List<T>> _dependencies = new Dictionary<T, List<T>>();
        private readonly Dictionary<T, List<T>> _dependents = new Dictionary<T, List<T>>();

        public int Size => _dependents.Count;

        public List<T> Roots
        {
            get
            {
                return _dependencies
                    .Where(kvp => kvp.Value.Count == 0)
                    .Select(kvp => kvp.Key)
                    .ToList();
            }
        }

        public IDependencyNode<T> AddItem(T item)
        {
            if (_dependents.ContainsKey(item))
                throw new ArgumentException("item cannot be added again");

            _dependents[item] = new List<T>();
            _dependencies[item] = new List<T>();

            return new Node<T>(this, item);
        }

        public List<T> GetDependents(T item)
        {
            MustExist(item, _dependents);
            return _dependents[item];
        }

        public List<T> GetDependencies(T item)
        {
            MustExist(item, _dependencies);
            return _dependencies[item];
        }

        private void MustExist(T item)
        {
            // By definition, it would also exist in dependencies
            MustExist(item, _dependents);
        }

        private void MustExist(T item, Dictionary<T, List<T>> map)
        {
            if (!_dependents.ContainsKey(item))
                throw new ArgumentException("item not found");
        }

        private class Node<TV> : IDependencyNode<TV>
        {
            private readonly DependencyGraph<TV> _graph;
            private readonly TV _item;

            public Node(DependencyGraph<TV> graph, TV item)
            {
                _graph = graph;
                _item = item;
            }

            public void DependsOn(params TV[] dependencies)
            {
                foreach (var dependency in dependencies)
                {
                    _graph.MustExist(dependency);
                    AddItemToList(_item, dependency, _graph._dependencies);
                    AddItemToList(dependency, _item, _graph._dependents);
                }
            }

            private static void AddItemToList(TV key, TV toAdd, IReadOnlyDictionary<TV, List<TV>> map)
            {
                var list = map[key];
                if (!list.Contains(toAdd))
                    list.Add(toAdd);
            }
        }
    }
}