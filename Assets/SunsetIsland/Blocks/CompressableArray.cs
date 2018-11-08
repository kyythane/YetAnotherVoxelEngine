using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Common;
using Assets.SunsetIsland.Managers;
using Assets.SunsetIsland.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Thread = System.Threading.Thread;
using Timer = System.Timers.Timer;

namespace SunsetIsland.Blocks
{
    public class CompressableArray<T> : IEnumerable<BatchUpdateItem<T>>
    {
        private const int Empty = 0;
        private const int List = 1;
        private const int Interval = 2;

     //   private static readonly CompressionScheduler Scheduler = new CompressionScheduler();
        private T[] _array;
        private int _compressState;
        private IntervalTree<T> _intervalTree;
        private long _lastTouched;
        private readonly ReaderWriterLockSlim _lock;
        private int _mode;
        private Thread _thread;
        private int _size;

        public CompressableArray()
        {
            _lock = new ReaderWriterLockSlim();
            _mode = Empty;
            _lastTouched = DateTime.UtcNow.Ticks;
        }

        public int Width { get; private set; } 

        public int Height { get; private set; }

        public bool Initialized { get; private set; }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _lock.EnterReadLock();
            var last = Interlocked.Read(ref _lastTouched);
            var mode = _mode;
            _lock.ExitReadLock();
            var now = DateTime.UtcNow.Ticks;
            if (now > last + ConfigManager.Properties.AutoCompressTime && mode == List) QueueCompress();
        }

        public void QueueCompress(int priority = 9)
        {
            if (Interlocked.CompareExchange(ref _compressState, 1, 0) == 0)
                SliceManager.Instance.QueueSlice(new CompressionSlice(this, true, priority));
        }

        public void QueueDecompress(int priority = 9)
        {
            if (Interlocked.CompareExchange(ref _compressState, 2, 0) == 0)
                SliceManager.Instance.QueueSlice(new CompressionSlice(this, false, priority));
        }

        public void Initialize(int width)
        {
            var arr = PoolManager.GetArrayPool<T[]>(width * width * width).Pop();
            Width = width;
            Height = width;
            _size = Width * Height * Width;
            Setup(arr);
        }

        private void Setup(T[] arr)
        {
            _lock.EnterWriteLock();
            ClearArrays();
            _array = arr;
            _mode = List;
            _compressState = 0;
            _lastTouched = DateTime.Now.Ticks;
            //Scheduler.Handler += TimerOnElapsed;
            Initialized = true;
            _lock.ExitWriteLock();
        }

        public void Initialize(T[] source, int width)
        {
            Width = width;
            Height = width;
            _size = Width * Height * Width;
            Setup(source);
        }

        public void Initialize(int width, int height)
        {
            var arr = PoolManager.GetArrayPool<T[]>(width * height * width).Pop();
            Width = width;
            Height = height;
            _size = Width * Height * Width;
            Setup(arr);
        }

        public void Initialize(T[] source, int width, int height)
        {
            Width = width;
            Height = height;
            _size = Width * Height * Width;
            Setup(source);
        }

        public T Get(int x, int y, int z)
        {
            var @lock = _thread != Thread.CurrentThread;
            if (@lock)
                _lock.EnterReadLock();
            T result;
            switch (_mode)
            {
                case List:
                    var index = General.BlockIndex(x, y, z, Width, Height);
                    result = _array[index];
                    break;
                case Interval:
                    result = _intervalTree.Get(x, y, z);
                    break;
                case Empty:
                    result = default(T);
                    Debug.LogError("CompressibleArray is empty and cannot return a item.");
                    break;
                default:
                    result = default(T);
                    Debug.LogError("Get failed. Mode is undefined");
                    break;
            }

            if (@lock)
                _lock.ExitReadLock();
            Interlocked.Exchange(ref _lastTouched, DateTime.Now.Ticks);
            return result;
        }

        public IEnumerable<BatchUpdateItem<T>> GetNeighborhood(int cX, int cY, int cZ, int radius = 1)
        {
            var @lock = _thread != Thread.CurrentThread;
            if (@lock)
                _lock.EnterReadLock();
            for (var x = cX - radius; x < cX + radius; ++x)
            for (var y = cY - radius; y < cY + radius; ++y)
            for (var z = cZ - radius; z < cZ + radius; ++z)
            {
                if (x < 0 || x >= Width ||
                    y < 0 || y >= Height ||
                    z < 0 || z >= Width)
                    continue;
                switch (_mode)
                {
                    case List:
                        var index = General.BlockIndex(x, y, z, Width, Height);
                        yield return new BatchUpdateItem<T>(new Vector3Int(x, y, z), _array[index]);
                        break;
                    case Interval:
                        yield return new BatchUpdateItem<T>(new Vector3Int(x, y, z), _intervalTree.Get(x, y, z));
                        break;
                    case Empty:
                        Debug.LogError("CompressibleArray is empty and cannot return a item.");
                        break;
                    default:
                        Debug.LogError("Get failed. Mode is undefined");
                        break;
                }
            }

            if (@lock)
                _lock.ExitReadLock();
            Interlocked.Exchange(ref _lastTouched, DateTime.Now.Ticks);
        }

        public bool Update(int x, int y, int z, T item, int timeout = 0)
        {
            var @lock = _thread != Thread.CurrentThread;
            if (@lock)
                if (timeout <= 0)
                {
                    _lock.EnterWriteLock();
                }
                else if (!_lock.TryEnterWriteLock(timeout))
                {
                    Interlocked.Exchange(ref _lastTouched, DateTime.Now.Ticks);
                    return false;
                }

            var success = false;
            switch (_mode)
            {
                case List:
                    var index = General.BlockIndex(x, y, z, Width);
                    _array[index] = item;
                    success = true;
                    break;
                case Interval:
                    _intervalTree.Update(x, y, z, item);
                    if (_intervalTree.Modifications > ConfigManager.Properties.MaxIntervalTreeMods) QueueDecompress();
                    success = true;
                    break;
                case Empty:
                    Debug.LogError("CompressibleArray is empty and cannot update a item.");
                    break;
                default:
                    Debug.LogError("Update failed. Mode is undefined");
                    break;
            }

            if (@lock)
                _lock.ExitWriteLock();
            Interlocked.Exchange(ref _lastTouched, DateTime.Now.Ticks);
            return success;
        }

        public bool Update([NotNull] IEnumerable<BatchUpdateItem<T>> items, int timeout = 0)
        {
            if (timeout <= 0)
            {
                _lock.EnterWriteLock();
            }
            else if (!_lock.TryEnterWriteLock(timeout))
            {
                Interlocked.Exchange(ref _lastTouched, DateTime.Now.Ticks);
                return false;
            }

            if (Interlocked.CompareExchange(ref _thread, Thread.CurrentThread, null) != null)
            {
                Debug.LogError("Batch update failed.");
                _lock.ExitWriteLock();
                Interlocked.Exchange(ref _lastTouched, DateTime.Now.Ticks);
                return false;
            }

            var success = false;
            switch (_mode)
            {
                case List:
                    foreach (var pair in items)
                    {
                        var index = General.BlockIndex(pair.Position.x, pair.Position.y, pair.Position.z, Width);
                        _array[index] = pair.Item;
                    }

                    success = true;
                    break;
                case Interval:
                    _intervalTree.Update(items);
                    if (_intervalTree.Modifications > ConfigManager.Properties.MaxIntervalTreeMods) QueueDecompress();

                    success = true;
                    break;
                case Empty:
                    Debug.LogError("CompressibleArray is empty and cannot update a item.");
                    break;
                default:
                    Debug.LogError("Batch update failed. Mode is undefined");
                    break;
            }

            Interlocked.Exchange(ref _thread, null);
            _lock.ExitWriteLock();
            Interlocked.Exchange(ref _lastTouched, DateTime.Now.Ticks);
            return success;
        }

        private void Compress()
        {
            if (Interlocked.CompareExchange(ref _compressState, 3, 1) != 1)
                return;
            _lock.EnterReadLock();
            if (_mode == Empty)
                return;
            if (_array == null)
                return;
            IntervalTree<T> intervalTree = null;
            try
            {
                if (Width == Height)
                {
                    var compressor = new BlockCompressor(Width);
                    intervalTree = compressor.Compress(_array);
                }
                else
                {
                    var compressor = new ColumnCompressor(Width, Height);
                    intervalTree = compressor.Compress(_array);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            try
            {
                if (intervalTree != null && _lock.TryEnterWriteLock(150))
                {
                    _intervalTree = intervalTree;
                    _mode = Interval;
                    var temp = _array;
                    _array = null;
                    for (var i = 0; i < temp.Length; i++)
                    {
                        temp[i] = default(T);
                    }

                    PoolManager.GetArrayPool<T[]>(_size).Push(temp);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                _lock.ExitWriteLock();
                Interlocked.Exchange(ref _lastTouched, DateTime.Now.Ticks);
                Interlocked.Exchange(ref _compressState, 0);
            }
        }

        private void Decompress()
        {
            if (Interlocked.CompareExchange(ref _compressState, 4, 2) != 2)
                return;
            if (_mode == Empty || _intervalTree == null)
                return;
            var array = PoolManager.GetArrayPool<T[]>(_size).Pop();

            _lock.EnterReadLock();
            try
            {
                _intervalTree.CopyTo(array);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            try
            {
                if (_lock.TryEnterWriteLock(150))
                {
                    _array = array;
                    _mode = List;
                    var temp = _intervalTree;
                    _intervalTree = null;
                    temp.Clear();
                    PoolManager.GetObjectPool<IntervalTree<T>>().Push(temp);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                _lock.ExitWriteLock();
                Interlocked.Exchange(ref _lastTouched, DateTime.Now.Ticks);
                Interlocked.Exchange(ref _compressState, 0);
            }
        }

        public void Evict()
        {
            _lock.EnterWriteLock();
            ClearArrays();
            _mode = Empty;
            Interlocked.Exchange(ref _compressState, 0);
            //Scheduler.Handler -= TimerOnElapsed;
            Initialized = false;
            _lock.ExitWriteLock();
        }

        private void ClearArrays()
        {
            if (_array != null)
            {
                PoolManager.GetArrayPool<T[]>(_size).Push(_array);
                _array = null;
            }

            if (_intervalTree != null)
            {
                PoolManager.GetObjectPool<IntervalTree<T>>().Push(_intervalTree);
                _intervalTree = null;
            }
        }

        private class CompressionScheduler
        {
            private readonly Timer _timer;
            private int _count;
            private readonly object _lockObject = new object();
            private ElapsedEventHandler _onIntervalElapsed;

            public CompressionScheduler()
            {
                _timer = new Timer(1000);
                _timer.Elapsed += OnElapsed;
            }

            private void OnElapsed(object sender, ElapsedEventArgs e)
            {
                _onIntervalElapsed.Invoke(sender, e);
            }

            public event ElapsedEventHandler Handler
            {
                add
                {
                    lock (_lockObject)
                    {
                        if (_count++ == 0)
                            _timer.Start();
                        _onIntervalElapsed += value;
                    }
                }
                remove
                {
                    lock (_lockObject)
                    {
                        if (_count-- == 1)
                            _timer.Stop();
                        _onIntervalElapsed -= value;
                    }
                }
            }
        }

        private struct CompressionSlice : ISlice
        {
            private readonly CompressableArray<T> _array;
            private readonly bool _compress;

            public CompressionSlice(CompressableArray<T> array, bool compress, int priority = 9)
            {
                _array = array;
                _compress = compress;
                Priority = priority;
                TimeQueuedMs = 0;
            }

            public void Execute(CancellationToken token)
            {
                if (_compress)
                    _array.Compress();
                else
                    _array.Decompress();
            }

            public int Priority { get; }
            public bool Threadable => true;
            public int TimeQueuedMs { get; set; }
            public void OnSuccess()
            {
                //No-Op
            }

            public void OnFailure(Exception exception)
            {
                //No-Op
            }
        }

        public IEnumerator<BatchUpdateItem<T>> GetEnumerator()
        {
            switch (_mode)
            {
                case List:
                    return EnumeratorInternal();
                case Interval:
                    return _intervalTree.GetEnumerator();
                case Empty:
                    throw new MemberAccessException("CompressibleArray is empty and cannot return a item.");
                default:
                    throw new IndexOutOfRangeException("GetEnumerator failed. Mode is undefined");
            }
        }

        private IEnumerator<BatchUpdateItem<T>> EnumeratorInternal()
        {
            for (var index = 0; index < _array.Length; index++)
            {
                var vector = General.Unmap(index, Width, Height);
                yield return new BatchUpdateItem<T>(vector, _array[index]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}