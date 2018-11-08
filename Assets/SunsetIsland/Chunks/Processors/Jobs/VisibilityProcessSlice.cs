using System;
using System.Diagnostics;
using System.Threading;
using Assets.SunsetIsland.Chunks.Processors.Utility;
using Assets.SunsetIsland.Common;
using Assets.SunsetIsland.Managers;
using Debug = UnityEngine.Debug;

namespace Assets.SunsetIsland.Chunks.Processors.Slices
{
    public struct VisibilityProcessSlice : ISlice
    {
        public int Priority { get; }
        public bool Threadable => true;
        public int TimeQueuedMs { get; set; }

        private readonly Patch _patch;

        public VisibilityProcessSlice(Patch patch)
        {
            _patch = patch;
            Priority = 3;
            TimeQueuedMs = -1;
        }

        public void Execute(CancellationToken token)
        {
            var visibilityProcessor = new VisibilityProcessor();
            visibilityProcessor.Process(_patch);
        }
        
        public void OnSuccess()
        {
            _patch.VisGraphState = LoadingState.Loaded;
        }

        public void OnFailure(Exception exception)
        {
            _patch.VisGraphState = LoadingState.Dirty;
        }
    }
}