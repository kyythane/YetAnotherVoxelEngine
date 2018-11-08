using System.Collections.Generic;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Chunks;
using UnityEngine;

namespace Assets.SunsetIsland.Game.Tools
{
    public abstract class BaseTool
    {
        public float Durability { get; protected set; }
        public float Mass { get; protected set; }

        public virtual void Use(World world, Vector3Int position)
        {
            Modify(world, GetShape(position));
        }

        //TODO : rework so it splits the shape among chunks
        protected virtual void Modify(World world, IEnumerable<Vector3Int> shape)
        {
            /*foreach (var point in shape)
            {
                var chunk = chunkCache.GetChunk(point);
                var local = point - chunk.Offset;
                var block = chunk.GetBlockLocalSpace(local);
                var updated = Transform(point, block);
                if (!Equals(updated, block))
                    chunk.UpdateBlockLocalSpace(local, updated, UpdateType.Edit, -1);
            }*/
        }

        protected abstract IEnumerable<Vector3Int> GetShape(Vector3Int position);
        protected abstract IBlock Transform(Vector3Int position, IBlock block);
    }
}