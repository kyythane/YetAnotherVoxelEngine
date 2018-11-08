using System;

namespace Assets.SunsetIsland.Initialization
{
    public class PipelineStatusArgs : EventArgs
    {
        public string CompletedStepName;
        public int CompletedSteps;
        public int TotalSteps;
    }
}