using System;
using System.Diagnostics;
using System.Threading;
using Assets.SunsetIsland.Chunks.Processors.Meshes;
using Assets.SunsetIsland.Common;
using Debug = UnityEngine.Debug;

namespace Assets.SunsetIsland.Chunks.Processors.Slices
{
    public struct RenderMeshBuild : ISlice
    {
        public int Priority { get; }
        public bool Threadable => true;
        public int TimeQueuedMs { get; set; }
        private readonly RenderShim _shim;
        private readonly bool _simple;

        public RenderMeshBuild(bool simple, RenderShim shim)
        {
            _shim = shim;
            Priority = 4;
            TimeQueuedMs = -1;
            _simple = simple;
        }

        public void Execute(CancellationToken token)
        {
            var meshProcessor = _simple ? (IRenderMeshProcessor)new SimpleRenderMeshProcessor() : new RenderMeshProcessor();
            meshProcessor.Process(_shim);
        }
        
        public void OnSuccess()
        {
            SliceManager.Instance.QueueSlice(new RenderMeshFinish(_shim.Patch));
            _shim.Dispose();
        }

        public void OnFailure(Exception exception)
        {
            _shim.Patch.RenderState = LoadingState.Dirty;
            _shim.Dispose();
        }
    }
}