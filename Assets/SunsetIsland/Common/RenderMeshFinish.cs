using System;
using System.Threading;
using Assets.SunsetIsland.Chunks;
using Assets.SunsetIsland.Managers;
using UnityEngine;

namespace Assets.SunsetIsland.Common
{
    public struct RenderMeshFinish : ISlice
    {
        private readonly Patch _patch;

        public RenderMeshFinish(Patch patch)
        {
            _patch = patch;
            Priority = 2;
            TimeQueuedMs = -1;
        }

        public void Execute(CancellationToken token)
        {
            if (!_patch.HasRenderTarget)
            {
                _patch.AttachRenderTarget();
            }
            _patch.RenderMeshData.UpdateMesh(_patch.RenderMesh);
        }

        public int Priority { get; }
        public bool Threadable => false;
        public int TimeQueuedMs { get; set; }
        public void OnSuccess()
        {
            _patch.RenderState = LoadingState.Loaded;
        }

        public void OnFailure(Exception exception)
        {
            _patch.RenderState = LoadingState.Dirty;
        }
    }
}