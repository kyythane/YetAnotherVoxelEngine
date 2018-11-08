using System;
using System.Threading;
using Assets.SunsetIsland.Chunks.Processors.Generation;
using Assets.SunsetIsland.Common;
using JetBrains.Annotations;
using Debug = UnityEngine.Debug;

namespace Assets.SunsetIsland.Chunks.Processors.Slices
{
    public struct ColumnFillSlice : ISlice
    {
        public int Priority { get; }
        public bool Threadable => true;
        public int TimeQueuedMs { get; set; }

        [NotNull] private readonly Column _column;

        public ColumnFillSlice([NotNull] Column column)
        {
            _column = column;
            Priority = 5;
            TimeQueuedMs = -1;
        }

        public void Execute(CancellationToken token)
        {
            var columnFiller = new ColumnFiller();
            columnFiller.Process(_column);
        }
        
        public void OnSuccess()
        {
            _column.State = ColumnState.Loaded;
        }

        public void OnFailure(Exception exception)
        {
            _column.State = ColumnState.Empty;
        }
    }
}