using System;
using System.Diagnostics;
using System.Threading;
using Assets.SunsetIsland.Chunks.Processors.Meshes;
using Assets.SunsetIsland.Common;
using Debug = UnityEngine.Debug;

namespace Assets.SunsetIsland.Chunks.Processors.Slices
{
    public struct PhysicsMeshBuild : ISlice
    {
        public int Priority { get; }
        public bool Threadable => true;
        public int TimeQueuedMs { get; set; }

        private readonly IChunk _chunk;

        public PhysicsMeshBuild(IChunk chunk)
        {
            _chunk = chunk;
            Priority = 3;
            TimeQueuedMs = -1;
        }

        public void Execute(CancellationToken token)
        {
            var stopWatch = Stopwatch.StartNew();
            var meshProcessor = new PhysicsMeshProcessor();
            meshProcessor.Process(_chunk);
            var ms = stopWatch.ElapsedMilliseconds;
         //   Debug.Log($"Phys time: {ms}");
        }
        
        public void OnSuccess()
        {
            SliceManager.Instance.QueueSlice(new PhysicsMeshFinish(_chunk));
        }

        public void OnFailure(Exception exception)
        {
            _chunk.PhysicsState = LoadingState.Dirty;
        }
    }
}