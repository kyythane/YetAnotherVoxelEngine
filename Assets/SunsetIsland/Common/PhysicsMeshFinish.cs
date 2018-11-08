using System;
using System.Threading;
using Assets.SunsetIsland.Chunks;
using Assets.SunsetIsland.Managers;
using UnityEngine;

namespace Assets.SunsetIsland.Common
{
    public struct PhysicsMeshFinish : ISlice
    {
        private readonly IChunk _chunk;

        public PhysicsMeshFinish(IChunk chunk)
        {
            _chunk = chunk;
            Priority = 1;
            TimeQueuedMs = -1;
        }
        public void Execute(CancellationToken token)
        {
            if (!_chunk.HasPhysicsTarget)
            {
                var chunkTarget = PoolManager
                    .GetMonoBehaviourPool(ConfigManager
                        .UnityProperties
                        .ChunkPhysicsTarget).Pop();
                chunkTarget.transform.localPosition =
                    (Vector3) _chunk.Offset *
                    ConfigManager.Properties.BlockWorldScale;
                _chunk.AttachPhysicsTarget(chunkTarget);
            }
            if (_chunk.MeshCollider?.sharedMesh != null)
            {
                PoolManager.GetObjectPool<Mesh>()
                    .Push(_chunk.MeshCollider.sharedMesh);
                _chunk.MeshCollider.sharedMesh = null;
            }
            var mesh = PoolManager.GetObjectPool<Mesh>().Pop();
            _chunk.PhysicsMeshData.UpdateMesh(mesh);
            if (_chunk.MeshCollider != null) _chunk.MeshCollider.sharedMesh = mesh;
        }
        public int Priority { get; }
        public bool Threadable => false;
        public int TimeQueuedMs { get; set; }
        public void OnSuccess()
        {
            _chunk.PhysicsState = LoadingState.Loaded;
        }

        public void OnFailure(Exception exception)
        {
            _chunk.PhysicsState = LoadingState.Dirty;
        }
    }
}