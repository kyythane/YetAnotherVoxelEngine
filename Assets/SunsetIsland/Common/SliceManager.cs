using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Assets.SunsetIsland.Collections;
using Assets.SunsetIsland.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.SunsetIsland.Common
{
    public class SliceManager
    {
        private static SliceManager _instance;
        public static SliceManager Instance => _instance ?? (_instance = new SliceManager());

        private readonly MinHeap<ISlice> _sliceQueue, _threadQueue;
        private readonly List<ThreadRunner> _threads;
        private int _timeStamp;

        private static readonly int _maxThreads = SystemInfo.processorCount - 1;

        private SliceManager()
        {
            _sliceQueue = new MinHeap<ISlice>(new SliceCompare());
            _threadQueue = new MinHeap<ISlice>(new SliceCompare());
            _threads = new List<ThreadRunner>();
            for (var i = 0; i < _maxThreads; i++)
            {
                _threads.Add(new ThreadRunner());
            }
            var go = new GameObject();
            Object.DontDestroyOnLoad(go);
            go.name = "__slice__runner__";
            var runnner = go.AddComponent<SliceRunner>();
            runnner.Initialize(this);
        }

        public void QueueSlice(ISlice slice)
        {
            if (slice == null)
                return;
            slice.TimeQueuedMs = _timeStamp;
            _sliceQueue.Add(slice);
        }

        private class SliceCompare : IComparer<ISlice>
        {
            public int Compare(ISlice slice1, ISlice slice2)
            {
                if (slice1 == null)
                    return 1;
                if (slice2 == null)
                    return -1;
                if (slice1.Priority == slice2.Priority)
                    return slice1.TimeQueuedMs - slice2.TimeQueuedMs;
                return slice1.Priority - slice2.Priority;
            }
        }

        private class SliceRunner : MonoBehaviour
        {
            private SliceManager _manager;
            private Coroutine _routine;

            public void Initialize(SliceManager manager)
            {
                _manager = manager;
            }

            private void Update()
            {
                _manager._timeStamp = (int) (Time.realtimeSinceStartup * 1000);
                _manager.PumpThreadQueue();
                if(_routine != null || _manager._sliceQueue.Count == 0) return;
                _routine = StartCoroutine(_manager.ExecuteSlices());
            }

            private void OnError(Exception exception)
            {
                if(exception == null) return;
                Debug.LogError(exception);
                
            }
        }

        private IEnumerator ExecuteSlices()
        {
            var time = Time.realtimeSinceStartup;
            while (_sliceQueue.Count > 0)
            {
                var slice = _sliceQueue.ExtractDominating();
                if(slice == null)
                    continue;
                if (slice.Threadable)
                {
                    _threadQueue.Add(slice);
                }
                else
                {
                    try
                    {
                        slice.Execute(new CancellationToken());
                        slice.OnSuccess();
                    }
                    catch (Exception e)
                    {
                        slice.OnFailure(e);
                    }
                }
                if (Time.realtimeSinceStartup - time >= ConfigManager.Properties.TimeSlice)
                    yield return new WaitForEndOfFrame();
            }
        }

        private void PumpThreadQueue()
        {
            if (_threadQueue.Count == 0) return;
            foreach (var thread in _threads)
            {
                if (!thread.Idle) continue;
                var item = _threadQueue.ExtractDominating();
                if(item == null)
                    continue;
                if(thread.Run(item) == null)
                    _threadQueue.Add(item);
                if (_threadQueue.Count == 0) return;
            }
        }
    }
}