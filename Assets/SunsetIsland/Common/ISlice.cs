using System;
using System.Threading;

namespace Assets.SunsetIsland.Common
{
    public interface ISlice
    {
        void Execute(CancellationToken token);
        int Priority { get; }
        bool Threadable { get; }
        int TimeQueuedMs { get; set; }
        void OnSuccess();
        void OnFailure(Exception exception);
    }
}