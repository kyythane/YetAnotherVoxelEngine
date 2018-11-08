using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Chunks.Processors.Slices;
using Assets.SunsetIsland.Collections;
using Assets.SunsetIsland.Common;
using Assets.SunsetIsland.Common.Enums;
using Assets.SunsetIsland.Game.Entities;
using Assets.SunsetIsland.Managers;
using Assets.SunsetIsland.Utilities;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.SunsetIsland.Chunks
{
    public class World
    {
        private readonly Vector3 _chunkOrigin;
        private readonly SparseArray2D<Column> _columns;
        private readonly int _chunkSize;

        private long _nextColumnId;
        private Player _player;
        private List<IChunkLoadingEntity> _entities;

        public World(float blockWorldScale, Vector3 chunkOrigin)
        {
            BlockWorldScale = blockWorldScale;
            _chunkSize = ConfigManager.Properties.ChunkSize;
            _columns = new SparseArray2D<Column>(_chunkSize);
            _chunkOrigin = chunkOrigin;
            _entities = new List<IChunkLoadingEntity>();
        }

        public float BlockWorldScale { get; }

        public long NextColumn //TODO : evaluate column/chunk ids
        {
            get
            {
                _nextColumnId += 10;
                return _nextColumnId;
            }
        }

         public void Initialize(Player player, int preloadWidth)
        {
            _player = player; // Used for rendering tasks
            _entities.Add(player);
            var spawnPosition = EntityOffset(player);
            LoadColumns(preloadWidth, spawnPosition);
        }

        private void LoadColumns(int loadRadius, Vector2Int loadCenterPosition)
        {
            foreach (var position in SampleInGrid(loadRadius, _chunkSize, loadCenterPosition))
            {
                var column = PoolManager.GetObjectPool<Column>().Pop();
                LoadColumn(column, position);
            }
        }

        private void LoadColumn(Column column, Vector2Int position)
        {
            if(column.State == ColumnState.Uninitialized)
                column.Initialize(position, NextColumn);
            _columns[position] = column;
            column.Load();
            if (_columns.ContainsKey(position.x - _chunkSize, position.y))
            {
                column.SetNeighbor(FaceDirection.XDecreasing, _columns[position.x - _chunkSize, position.y]);
            }

            if (_columns.ContainsKey(position.x + _chunkSize, position.y))
            {
                column.SetNeighbor(FaceDirection.XIncreasing, _columns[position.x + _chunkSize, position.y]);
            }

            if (_columns.ContainsKey(position.x, position.y - _chunkSize))
            {
                column.SetNeighbor(FaceDirection.ZDecreasing, _columns[position.x, position.y - _chunkSize]);
            }

            if (_columns.ContainsKey(position.x, position.y + _chunkSize))
            {
                column.SetNeighbor(FaceDirection.ZIncreasing, _columns[position.x, position.y + _chunkSize]);
            }
        }

        private Vector2Int EntityOffset(IChunkLoadingEntity entity)
        {
            var x = (int) ((entity.Position.x - _chunkOrigin.x) / BlockWorldScale);
            var y = (int) ((entity.Position.z - _chunkOrigin.z) / BlockWorldScale);
            return new Vector2Int(x, y);
        } 
        
        private Vector3Int EntityPosition(IChunkLoadingEntity entity)
        {
            return EntityPosition(entity.Position);
        }
        
        private Vector3Int EntityPosition(Vector3 entityPosition)
        {
            var x = (int) ((entityPosition.x - _chunkOrigin.x) / BlockWorldScale);
            var y = (int) ((entityPosition.y - _chunkOrigin.y) / BlockWorldScale);
            var z = (int) ((entityPosition.z - _chunkOrigin.z) / BlockWorldScale);
            return new Vector3Int(x, y, z);
        }

        public bool CanEnterChunk(Vector3 nextPosition)
        {
            var position = EntityPosition(nextPosition);
            var chunk = GetChunk(position);
            return chunk != null && (chunk.PhysicsState == LoadingState.Loaded || chunk.PhysicsState == LoadingState.Dirty);
        }

        [NotNull]
        private static IEnumerable<Vector2Int> SampleInGrid(int gridWidth, int cellSize, Vector2Int sourcePosition)
        {
            var origin = new Vector2Int(sourcePosition.x / cellSize, sourcePosition.y / cellSize) * cellSize;
            var half = gridWidth / 2;
            var x = half;
            var y = gridWidth % 2 == 0 ? half - 1 : half;

            var direction = 0;
            var stepsCount = 1;
            var stepPosition = 0;
            var stepChange = 0;

            for (var i = 0; i < gridWidth * gridWidth; i++)
            {
                yield return origin + (new Vector2Int(x - half, half - y) * cellSize);

                if (stepPosition < stepsCount)
                {
                    stepPosition++;
                }
                else
                {
                    stepPosition = 1;
                    if (stepChange == 1)
                        stepsCount++;
                    stepChange = (stepChange + 1) % 2;
                    direction = (direction + 1) % 4;
                }

                switch (direction)
                {
                    case 0:
                        y++;
                        break;
                    case 1:
                        x--;
                        break;
                    case 2:
                        y--;
                        break;
                    case 3:
                        x++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{direction} out of range");
                }
            }
        }

        public void Update(float deltaTime)
        {
            //TODO : load columns
            UpdateColumns();
          // UpdateCollisions();
        }
        
        private void UpdateColumns()
        {
            foreach (var chunkLoadingEntity in _entities)
            {
                var position = EntityOffset(chunkLoadingEntity);
                foreach (var sample in SampleInGrid(15, _chunkSize, position))
                {
                    var column = _columns[sample.x, sample.y];
                    if (column != null &&
                        column.State != ColumnState.Uninitialized &&
                        column.State != ColumnState.Empty) continue;
                    var newColumn = PoolManager.GetObjectPool<Column>().Pop();
                    LoadColumn(newColumn, sample);
                    //TODO : dirty columns?
                }
            }
            //TODO : evict far columns
        }

        private void UpdateCollisions()
        {
            foreach (var chunkLoadingEntity in _entities)
            {
                var position = EntityOffset(chunkLoadingEntity);
                foreach (var sample in SampleInGrid(15, _chunkSize, position))
                {
                    var column = _columns[sample.x, sample.y];
                    if(column == null)
                        continue;
                    foreach (var chunk in column)
                    {
                        if(chunk?.PhysicsDirty != true) continue;
                        chunk.PhysicsState = LoadingState.Loading;
                        var slice = new PhysicsMeshBuild(chunk);
                        SliceManager.Instance.QueueSlice(slice);
                    }
                }
            }
            //TODO : evict physics of far chunks
        }

        public void Draw()
        {
            var w = Stopwatch.StartNew();
            var patchSize = ConfigManager.Properties.PatchSize;
            var position = EntityPosition(_player);
            var camera = _player.Camera;
            var direction = camera.transform.rotation * Vector3.forward;
            var planes = new Plane[6];
            GeometryUtility.CalculateFrustumPlanes(_player.Camera, planes);
            var queue = PoolManager.GetObjectPool<Queue<VisibilityQueueItem>>().Pop();
            var visited = PoolManager.GetObjectPool<HashSet<Vector3Int>>().Pop();
            visited.Clear();
            var rayCache = PoolManager.GetObjectPool<Dictionary<Patch, List<Vector3>>>().Pop();
            foreach (var cache in rayCache)
            {
                cache.Value.Clear();
                PoolManager.GetObjectPool<List<Vector3>>().Push(cache.Value);
            }
            rayCache.Clear();
            var originSet = PoolManager.GetObjectPool<HashSet<Patch>>().Pop();
            for (var x = -1; x <= 1; x++)
            for (var y = -1; y <= 1; y++)
            for (var z = -1; z <= 1; z++)
            {
                var origin = GetPatch(position + new Vector3Int(x, y, z));
                if(origin == null || visited.Contains(origin.Offset))
                    continue;
                visited.Add(origin.Offset);
                originSet.Add(origin);
                queue.Enqueue(new VisibilityQueueItem
                {
                    Patch = origin,
                    Previous = FaceDirection.None
                });
            }
           while (queue.Count > 0)
           {
               var current = queue.Dequeue();
               if(current.Patch.Renderable)
               {
                   switch (current.Patch.RenderState)
                   {
                       case LoadingState.Dirty:
                       case LoadingState.Empty:
                           current.Patch.RenderState = LoadingState.Loading;
                           var slice = new RenderMeshBuild(false, new RenderShim(_columns[current.Patch.Offset.x, current.Patch.Offset.z], current.Patch));
                           SliceManager.Instance.QueueSlice(slice);
                           break;
                       case LoadingState.Loaded:
                           if(current.Patch.RenderMesh != null)
                               Graphics.DrawMeshNow(current.Patch.RenderMesh, (Vector3)current.Patch.Offset * ConfigManager.Properties.BlockWorldScale, Quaternion.identity);
                           break;
                   }
               }
               for (var i = 0; i < 6; i++)
               {
                   var nextFace = (FaceDirection) i;
                   var vector = General.FaceVectorsInt[nextFace];
                   var nextPos = current.Patch.Offset + vector * patchSize;
                   if (visited.Contains(nextPos) || nextFace == current.Previous)
                       continue;
                   visited.Add(nextPos);
                   var nextPatch = GetPatch(nextPos);
                   if (nextPatch == null)
                       continue;
                   switch (nextPatch.VisGraphState)
                   {
                       case LoadingState.Dirty:
                       case LoadingState.Empty:
                           nextPatch.ComputeHull();
                           nextPatch.VisGraphState = LoadingState.Loading;
                           var slice = new VisibilityProcessSlice(nextPatch);
                           SliceManager.Instance.QueueSlice(slice);
                           continue;
                       case LoadingState.Loading:
                       case LoadingState.Uninitialized:
                       case LoadingState.Evicting:
                           continue;
                   }
                   if (current.Previous != FaceDirection.None)
                   {
                       if (!current.Patch.Connectivity[current.Previous].Contains(nextFace))
                       {
                           continue;
                       }
                   }
                   var approach = (nextPatch.Offset + (Vector3)nextPatch.Size / 2 - position).normalized;
                   var dot = Vector3.Dot(approach, direction);
                   if (dot < 0)
                       continue;
                   var bounds = new Bounds();
                   bounds.SetMinMax((Vector3) nextPos * ConfigManager.Properties.BlockWorldScale,
                       ((Vector3) nextPos + nextPatch.Size) * ConfigManager.Properties.BlockWorldScale);
                   if (!GeometryUtility.TestPlanesAABB(planes, bounds))
                   {
                       continue;
                   }

                   //if (!MarchRay(nextPatch, (position - (nextPatch.Offset + (Vector3) nextPatch.Size / 2)).normalized,
                   //    originSet, rayCache))
                   //{
                   //    continue;
                   //}
                   
                  //Debug.DrawLine(
                  //    (Vector3) current.Patch.Offset * ConfigManager.Properties.BlockWorldScale + new Vector3(4, 4, 4),
                  //    (Vector3) nextPos * ConfigManager.Properties.BlockWorldScale + new Vector3(4, 4, 4),
                  //    Color.blue);
                   
                   queue.Enqueue(new VisibilityQueueItem
                   {
                       Patch = nextPatch,
                       Previous = General.FlipDirection(nextFace)
                   });
               }
           }
            
           PoolManager.GetObjectPool<Queue<VisibilityQueueItem>>().Push(queue);
           PoolManager.GetObjectPool<HashSet<Vector3Int>>().Push(visited);
           PoolManager.GetObjectPool<HashSet<Patch>>().Push(originSet);
           PoolManager.GetObjectPool<Dictionary<Patch, List<Vector3>>>().Push(rayCache);
          // Debug.Log($"Render Elapsed: {w.ElapsedTicks / 10000f}");
        }

        private void DrawPatchBounds(Patch p)
        {
            var bounds = new Bounds();
            bounds.SetMinMax((Vector3)(p.SolidHullMin + p.Offset) * 0.5f, (Vector3)(p.SolidHullMax + p.Offset) * 0.5f);
            
            var extents = bounds.extents;
            var center = bounds.center;

            var c0 = center - extents;
            var c1 = center + new Vector3(extents.x, -extents.y, -extents.z);
            var c2 = center + new Vector3(extents.x, extents.y, -extents.z);
            var c3 = center + new Vector3(-extents.x, extents.y, -extents.z);
            var c4 = center + new Vector3(-extents.x, extents.y, extents.z);
            var c5 = center + extents;
            var c6 = center + new Vector3(extents.x, -extents.y, extents.z);
            var c7 = center + new Vector3(-extents.x, -extents.y, extents.z);

            Debug.DrawLine(c0, c1, Color.magenta);
            Debug.DrawLine(c0, c3, Color.magenta);
            Debug.DrawLine(c0, c7, Color.magenta);
            Debug.DrawLine(c1, c2, Color.magenta);
            Debug.DrawLine(c1, c6, Color.magenta);
            Debug.DrawLine(c2, c3, Color.magenta);
            Debug.DrawLine(c2, c5, Color.magenta);
            Debug.DrawLine(c3, c4, Color.magenta);
            Debug.DrawLine(c4, c5, Color.magenta);
            Debug.DrawLine(c4, c7, Color.magenta);
            Debug.DrawLine(c5, c6, Color.magenta);
            Debug.DrawLine(c6, c7, Color.magenta);
        }

        private bool MarchRay(Patch patch, Vector3 vector, HashSet<Patch> originSet, Dictionary<Patch, List<Vector3>> rayCache)
        {
            var pos = (patch.Offset + (Vector3) patch.Size / 2);
            var ray = new Ray(pos / ConfigManager.Properties.PatchSize, vector);
            var lastPatch = patch;
            do
            {
                if (ray.MaxX < ray.MaxY)
                {
                    if (ray.MaxX < ray.MaxZ)
                    {
                        ray.X += ray.StepX;
                        var nextPos = ray.Vector * ConfigManager.Properties.PatchSize;
                        patch = GetPatch(nextPos);
                       //Debug.DrawRay(nextPos * 0.5f, ray.Cast * 3, Color.red);
                       //Debug.DrawLine(nextPos * 0.5f, pos * 0.5f, Color.magenta);
                       //pos = nextPos;
                        if (!CheckCell(nextPos, patch, originSet))
                            return false;
                        if (CheckTerminal(ray, lastPatch, patch, originSet, rayCache))
                            return true;
                        ray.MaxX += ray.DeltaX;
                    }
                    else
                    {
                        ray.Z += ray.StepZ;
                        var nextPos = ray.Vector * ConfigManager.Properties.PatchSize;
                        patch = GetPatch(nextPos);
                        //Debug.DrawRay(nextPos * 0.5f, ray.Cast * 3, Color.red);
                        //Debug.DrawLine(nextPos * 0.5f, pos * 0.5f, Color.magenta);
                        //pos = nextPos;
                        if (!CheckCell(nextPos, patch, originSet))
                            return false;
                        if (CheckTerminal(ray, lastPatch, patch, originSet, rayCache))
                            return true;
                        ray.MaxZ += ray.DeltaZ;
                    }
                }
                else
                {
                    if (ray.MaxY < ray.MaxZ)
                    {
                        ray.Y += ray.StepY;
                        var nextPos = ray.Vector * ConfigManager.Properties.PatchSize;
                        patch = GetPatch(nextPos);
                       //Debug.DrawRay(nextPos * 0.5f, ray.Cast * 3, Color.red);
                       //Debug.DrawLine(nextPos * 0.5f, pos * 0.5f, Color.magenta);
                       //pos = nextPos;
                        if (!CheckCell(nextPos, patch, originSet))
                            return false;
                        if (CheckTerminal(ray, lastPatch, patch, originSet, rayCache))
                            return true;
                        ray.MaxY += ray.DeltaY;
                    }
                    else
                    {
                        ray.Z += ray.StepZ;
                        var nextPos = ray.Vector * ConfigManager.Properties.PatchSize;
                        patch = GetPatch(nextPos);
                       //Debug.DrawRay(nextPos * 0.5f, ray.Cast * 3, Color.red);
                       //Debug.DrawLine(nextPos * 0.5f, pos * 0.5f, Color.magenta);
                       //pos = nextPos;
                        if (!CheckCell(nextPos, patch, originSet))
                            return false;
                        if (CheckTerminal(ray, lastPatch, patch, originSet, rayCache))
                            return true;
                        ray.MaxZ += ray.DeltaZ;
                    }
                }
            } while (!originSet.Contains(patch));
            return true;
        }

        private bool CheckTerminal(Ray cast, Patch lastPatch, Patch nextPatch, HashSet<Patch> originSet, Dictionary<Patch, List<Vector3>> rayCache)
        {
            if (originSet.Contains(nextPatch))
            {
                if (!rayCache.ContainsKey(lastPatch))
                {
                    rayCache.Add(lastPatch, PoolManager.GetObjectPool<List<Vector3>>().Pop());
                }
                rayCache[lastPatch].Add(cast.Cast);
                return true;
            }
            if (rayCache.ContainsKey(nextPatch))
            {
                foreach (var ray in rayCache[nextPatch])
                {
                    if (Vector3.Dot(ray, cast.Cast) >= 0.95f)
                    {
                        if (!rayCache.ContainsKey(lastPatch))
                        {
                            rayCache.Add(lastPatch, PoolManager.GetObjectPool<List<Vector3>>().Pop());
                        }
                        rayCache[lastPatch].Add(cast.Cast);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckCell(Vector3 nextPos, Patch patch, HashSet<Patch> originSet)
        {
            if (patch == null)
                return false;
            if (originSet.Contains(patch))
                return true;
            switch (patch.VisGraphState)
            {
                case LoadingState.Empty:
                case LoadingState.Loading:
                case LoadingState.Uninitialized:
                case LoadingState.Evicting:
                    return false;
                default:
                    var faceNext = General.PrincipleFace(nextPos);
                    var faceLast = General.PrincipleFace(-nextPos);
                    return patch.Connectivity[faceLast].Contains(faceNext);
            }
        }

        public struct Ray
        {
            public int X;
            public int Y;
            public int Z;
            public float DeltaX {get;}
            public float DeltaY {get;}
            public float DeltaZ {get;}
            public int StepX {get;}
            public int StepY {get;}
            public int StepZ {get;}
            public float MaxX;
            public float MaxY;
            public float MaxZ;

            public Ray(Vector3 origin, Vector3 delta)
            {
                Cast = delta;
                X = (int)Math.Floor(origin.x);
                Y = (int)Math.Floor(origin.y);
                Z = (int)Math.Floor(origin.z);
                StepX = Mathf.Approximately(delta.x, 0) ? 0 : (int)Mathf.Sign(delta.x);
                StepY = Mathf.Approximately(delta.y, 0) ? 0 : (int)Mathf.Sign(delta.y);
                StepZ = Mathf.Approximately(delta.z, 0) ? 0 : (int)Mathf.Sign(delta.z);
                if (Mathf.Approximately(delta.x, 0))
                {
                    DeltaX = 0;
                    MaxX = float.PositiveInfinity;
                }
                else
                {
                    DeltaX = 1/Math.Abs(delta.x);
                    MaxX = 0.5f * DeltaX;
                }
                if (Mathf.Approximately(delta.y, 0))
                {
                    DeltaY = 0;
                    MaxY = float.PositiveInfinity;
                }
                else
                {
                    DeltaY = 1/Math.Abs(delta.y);
                    MaxY = 0.5f * DeltaY;
                }
                if (Mathf.Approximately(delta.z, 0))
                {
                    DeltaZ = 0;
                    MaxZ = float.PositiveInfinity;
                }
                else
                {
                    DeltaZ = 1/Math.Abs(delta.z);
                    MaxZ = 0.5f * DeltaZ;
                }
            }

            public Vector3 Vector => new Vector3(X, Y, Z);
            public Vector3 Cast { get; }
        }

        public IBlock GetBlock(Vector3 position)
        {
            var chunk = GetChunk(position);
            var block = chunk != null
                            ? chunk.GetBlockWithBoundCheck(position - chunk.Offset)
                            : BlockFactory.Empty;
            return block ?? BlockFactory.Empty;
        }

        public IBlock GetBlock(Vector3Int position)
        {
            var chunk = GetChunk(position);
            var block = chunk != null ? chunk.GetBlockWithBoundCheck(position - chunk.Offset) : BlockFactory.Empty;
            return block ?? BlockFactory.Empty;
        }

        public IBlock GetBlock(int x, int y, int z)
        {
            var chunk = GetChunk(x, y, z);
            var block = chunk != null
                            ? chunk.GetBlockWithBoundCheck(x - chunk.Offset.x, y - chunk.Offset.y, z - chunk.Offset.z)
                            : BlockFactory.Empty;
            return block ?? BlockFactory.Empty;
        }
        
        public IChunk GetChunk(Vector3 position)
        {
            return GetChunk((int) position.x, (int) position.y, (int) position.z);
        }

        public IChunk GetChunk(Vector3Int position)
        {
            return GetChunk(position.x, position.y, position.z);
        }

        public IChunk GetChunk(int x, int y, int z)
        {
            return _columns[x, z]?[y];
        }
        
        public Patch GetPatch(Vector3 position)
        {
            return GetPatch((int) position.x, (int) position.y, (int) position.z);
        }

        public Patch GetPatch(Vector3Int position)
        {
            var chunk = _columns[position.x, position.z]?[position.y];
            return chunk == null || !chunk.BlockDataLoaded || !chunk.NeighborsSet ? null : chunk.GetPatch(position - chunk.Offset);
        }

        public Patch GetPatch(int x, int y, int z)
        {
            return GetPatch(new Vector3Int(x, y, z));
        }

        public void UpdateBlock(Vector3 position, IBlock block)
        {
            var chunk = GetChunk(position);
            chunk?.UpdateBlock(position, block);
        }

        public void UpdateBlock(Vector3Int position, IBlock block)
        {
            var chunk = GetChunk(position);
            chunk?.UpdateBlock(position, block);
        }

        public void UpdateBlock(int x, int y, int z, IBlock block)
        {
            var chunk = GetChunk(x, y, z);
            chunk?.UpdateBlock(x, y, z, block);
        }

        public void UpdateBlocks([NotNull] IEnumerable<BatchUpdateItem<IBlock>> edits)
        {
            foreach (var edit in edits)
            {
                var chunk = GetChunk(edit.Position);
                var local = edit.Position - chunk.Offset;
                chunk?.UpdateBlock(local, edit.Item);
            }
        }

        private struct VisibilityQueueItem
        {
            public Patch Patch;
            public FaceDirection Previous;
        }
    }
}