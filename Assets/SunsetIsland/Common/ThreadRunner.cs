using System;
using System.Threading;
using System.Threading.Tasks;
using Assets.SunsetIsland.Managers;
using UnityEngine;

namespace Assets.SunsetIsland.Common
{
    public class ThreadRunner
    {
        public bool Idle { get; private set; }

        public ThreadRunner()
        {
            Idle = true;
            _cancelSource = new CancellationTokenSource();
        }
        private ISlice _slice;
        private readonly CancellationTokenSource _cancelSource;

        public Task Run(ISlice slice)
        {
            if (!Idle) return null;
            _slice = slice;
            _cancelSource.CancelAfter(ConfigManager.Properties.BackgroundThreadTimeout);
            var token = _cancelSource.Token;
            //TODO : Fix cancelation
            var task = Task.Run(() => Start(token));//, token);
            Idle = false;
            return task;
        }

        private void Start(CancellationToken token)
        {
            try
            {
                _slice.Execute(token);
                _slice.OnSuccess();
                Idle = true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                _slice.OnFailure(e);
                Idle = true;
            }
        }
    }
}