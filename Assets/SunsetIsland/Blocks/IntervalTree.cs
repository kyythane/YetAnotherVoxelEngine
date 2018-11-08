using System;
using System.Collections;
using System.Collections.Generic;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Utilities;
using JetBrains.Annotations;

namespace Assets.SunsetIsland.Blocks
{
    public class IntervalTree<T> : IEnumerable<BatchUpdateItem<T>>
    {
        private readonly List<Interval<T>> _tree;
        private int[] _blockToInterval; //used to handle the different ordering modes
        private int[] _intervalToBlock;

        public IntervalTree()
        {
            _tree = new List<Interval<T>>();
        }

        public int Modifications { get; private set; }

        //metadata used for tracking
        public CompressionFlag CompressionFlag { get; set; }

        public float ComressionRatio { get; set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        public void Initialize(int height, int width, int[] blockToInterval, int[] intervalToBlock)
        {
            _tree.Clear();
            _blockToInterval = blockToInterval;
            _intervalToBlock = intervalToBlock;
            Modifications = 0;
            Height = height;
            Width = width;
        }

        public void Add(Interval<T> interval)
        {
            _tree.Add(interval);
        }

        public void TrimExcess()
        {
            _tree.TrimExcess();
        }

        [CanBeNull]
        public T Get(int x, int y, int z)
        {
            var searchLocation = _blockToInterval[General.BlockIndex(x, y, z, Width, Height)];
            var index = _tree.BinarySearch(new Interval<T>(searchLocation, default(T)));
            return index < 0 ? default(T) : _tree[index].Data;
        }

        /// <summary>
        /// Insertions are slow. Does not condense runs.
        /// </summary>
        public void Update(int x, int y, int z, T update)
        {
            Update(General.BlockIndex(x, y, z, Width, Height), update);
        }

        /// <summary>
        /// Insertions are slow. Does not condense runs.
        /// </summary>
        public void Update(int arrayIndex, T upadte)
        {
            var searchLocation = _blockToInterval[arrayIndex];
            var @new = new Interval<T>(searchLocation, upadte);
            var index =_tree.BinarySearch(@new);
            if(index < 0) return;
            var interval = _tree[index];
            if (Equals(upadte, interval.Data))
                return;
            var min = interval.Min;
            var max = interval.Max;
            var data = interval.Data;
            _tree.Remove(interval);
            if (min < searchLocation)
            {
                var left = new Interval<T>(min, (ushort) (searchLocation - 1), data);
                if (index < _tree.Count) _tree.Insert(index, left);
                else _tree.Add(left);
                ++index;
            }
            if (index < _tree.Count) _tree.Insert(index, @new);
            else _tree.Add(@new);
            ++index;
            if (max > searchLocation)
            {
                var right = new Interval<T>((ushort) (searchLocation + 1), max, data);
                if (index < _tree.Count) _tree.Insert(index, right);
                else _tree.Add(right);
            }
            Modifications++;
        }

        public void Update(IEnumerable<BatchUpdateItem<T>> updates)
        {
            foreach (var updateItem in updates)
            {
                var pos = updateItem.Position;
                Update(General.BlockIndex(pos.x, pos.y, pos.z,  Width, Height), updateItem.Item);
            }
        }

        public void CopyTo(IList<T> array)
        {
            var count = 0;
            foreach (var node in _tree)
            {
                var interval = node;
                for (var i = interval.Min; i < interval.Max; ++i)
                {
                    array[_intervalToBlock[i]] = node.Data;
                    count++;
                }
            }
        }

        public override string ToString()
        {
            var str = "";
            foreach (var node in _tree)
                str += $" [{node.Min} : {node.Max} [{node.Data?.ToString()}]] ";
            return str;
        }

        public IEnumerator<BatchUpdateItem<T>> GetEnumerator()
        {
            for (var i = 0; i < _tree.Count; i++)
            {
                var node = _tree[i];
                var interval = node;
                for (var j = interval.Min; j < interval.Max; j++)
                {
                    var vector = General.Unmap(_intervalToBlock[i + j], Width, Height);
                    yield return new BatchUpdateItem<T>(vector, node.Data);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            _tree.Clear();
        }
    }
}