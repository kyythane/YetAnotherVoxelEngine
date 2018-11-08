using System;
using System.Collections;
using System.Collections.Generic;
using Assets.SunsetIsland.Common.Enums;

namespace Assets.SunsetIsland.Chunks
{
    public class VisibilityData : IEnumerable<KeyValuePair<FaceDirection, HashSet<FaceDirection>>>
    {
        private readonly HashSet<FaceDirection> _xIncreasing = new HashSet<FaceDirection>();
        private readonly HashSet<FaceDirection> _yIncreasing = new HashSet<FaceDirection>();
        private readonly HashSet<FaceDirection> _zIncreasing = new HashSet<FaceDirection>();
        private readonly HashSet<FaceDirection> _xDecreasing = new HashSet<FaceDirection>();
        private readonly HashSet<FaceDirection> _yDecreasing = new HashSet<FaceDirection>();
        private readonly HashSet<FaceDirection> _zDecreasing = new HashSet<FaceDirection>();

        public void Clear()
        {
            _xIncreasing.Clear();
            _yIncreasing.Clear();
            _zIncreasing.Clear();
            _xDecreasing.Clear();
            _yDecreasing.Clear();
            _zDecreasing.Clear();
        }

        public void Add(FaceDirection source, FaceDirection target)
        {
            //ensure bidirectional connections!!!!
            this[source].Add(target);
            this[target].Add(source);
        }

        public HashSet<FaceDirection> this[FaceDirection index]
        {
            get
            {
                switch (index)
                {
                    case FaceDirection.XIncreasing:
                        return _xIncreasing;
                    case FaceDirection.YIncreasing:
                        return _yIncreasing;
                    case FaceDirection.ZIncreasing:
                        return _zIncreasing;
                    case FaceDirection.XDecreasing:
                        return _xDecreasing;
                    case FaceDirection.YDecreasing:
                        return _yDecreasing;
                    case FaceDirection.ZDecreasing:
                        return _zDecreasing;
                    case FaceDirection.None:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index), index, null);
                }
            }
        }

        public bool this[FaceDirection source, FaceDirection target]
        {
            get
            {
                switch (source)
                {
                    case FaceDirection.XIncreasing:
                        return _xIncreasing.Contains(target);
                    case FaceDirection.YIncreasing:
                        return _yIncreasing.Contains(target);
                    case FaceDirection.ZIncreasing:
                        return _zIncreasing.Contains(target);
                    case FaceDirection.XDecreasing:
                        return _xDecreasing.Contains(target);
                    case FaceDirection.YDecreasing:
                        return _yDecreasing.Contains(target);
                    case FaceDirection.ZDecreasing:
                        return _zDecreasing.Contains(target);
                    case FaceDirection.None:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(source), source, null);
                }
            }
        }

        public IEnumerator<KeyValuePair<FaceDirection, HashSet<FaceDirection>>> GetEnumerator()
        {
            yield return new KeyValuePair<FaceDirection, HashSet<FaceDirection>>(FaceDirection.XIncreasing,
                _xIncreasing);
            yield return new KeyValuePair<FaceDirection, HashSet<FaceDirection>>(FaceDirection.YIncreasing,
                _yIncreasing);
            yield return new KeyValuePair<FaceDirection, HashSet<FaceDirection>>(FaceDirection.ZIncreasing,
                _zIncreasing);
            yield return new KeyValuePair<FaceDirection, HashSet<FaceDirection>>(FaceDirection.XDecreasing,
                _xDecreasing);
            yield return new KeyValuePair<FaceDirection, HashSet<FaceDirection>>(FaceDirection.YDecreasing,
                _yDecreasing);
            yield return new KeyValuePair<FaceDirection, HashSet<FaceDirection>>(FaceDirection.ZDecreasing,
                _zDecreasing);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}