using UnityEngine;

namespace Assets.SunsetIsland.Blocks
{
    public struct BatchUpdateItem<T>
    {
        public Vector3Int Position { get; }
        public T Item { get; }

        public BatchUpdateItem(Vector3Int position, T item)
        {
            Position = position;
            Item = item;
        }
    }
}