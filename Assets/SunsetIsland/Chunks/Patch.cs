using Assets.SunsetIsland.Blocks;
using Assets.SunsetIsland.Managers;
using UnityEngine;

namespace Assets.SunsetIsland.Chunks
{
    public class Patch : IVisibilityCell
    {
        public bool Renderable
        {
            get
            {
                if (_hullDirty)
                {
                    ComputeHull();
                }

                return RenderHullMax != RenderHullMin;
            }
        }

        public void MarkHullDirty(bool value)
        {
            _hullDirty = true;
        }

        public Vector3Int Size { get; } = new Vector3Int(ConfigManager.Properties.PatchSize,
            ConfigManager.Properties.PatchSize, ConfigManager.Properties.PatchSize);
        public Vector3Int Offset { get; private set; }
        public Vector3Int ChunkOffset { get; private set; }

        public VisibilityData Connectivity { get; private set; }
        public Vector3Int RenderHullMax { get; private set; }
        public Vector3Int RenderHullMin { get; private set; }
        public Vector3Int SolidHullMax { get; private set; }
        public Vector3Int SolidHullMin { get; private set; }
        
        private IChunk _parent;

        public Mesh RenderMesh { get; private set; }

        public RenderMeshData RenderMeshData
        {
            get
            {
                if (_renderMeshData == null)
                {
                    _renderMeshData = PoolManager.GetObjectPool<RenderMeshData>().Pop();
                    _renderMeshData.Clear();
                }
                return _renderMeshData;
            }
        }
        private RenderMeshData _renderMeshData;
        private bool _hullDirty;
        public LoadingState RenderState { get; set; }
        public bool RenderDirty => RenderState == LoadingState.Dirty || RenderState == LoadingState.Empty;
        public LoadingState VisGraphState { get; set; }
        public bool VisGraphDirty => VisGraphState == LoadingState.Dirty || VisGraphState == LoadingState.Empty;
        
        public void AttachRenderTarget()
        {
            RenderMesh = new Mesh();
        }

        public bool HasRenderTarget => RenderMesh != null;

        public void Initialize(IChunk parent, Vector3Int offset)
        {
            _parent = parent;
            ChunkOffset = offset;
            Offset = _parent.Offset + ChunkOffset;
            RenderState = LoadingState.Empty;
            VisGraphState = LoadingState.Empty;
            Connectivity =  PoolManager.GetObjectPool<VisibilityData>().Pop();
            Connectivity.Clear();
            MarkHullDirty(true);
            RenderHullMax = new Vector3Int();
            RenderHullMin  = new Vector3Int();
            SolidHullMax   = new Vector3Int();
            SolidHullMin   = new Vector3Int();
        }

        public void ComputeHull()
        {
            if(!_hullDirty)
                return;
            var solidHullMax = new Vector3Int();
            var solidHullMin = new Vector3Int();
            var emptyHullMax = new Vector3Int();
            var emptyHullMin = new Vector3Int();
            var solidSet = false;
            var emptySet = false;
            for (var x = -1; x <= Size.x; ++x)
            for (var y = -1; y <= Size.y; ++y)
            for (var z = -1; z <= Size.z; ++z)
            {
                var position = new Vector3Int(x, y, z);
                if (x < 0 || y < 0 || z < 0 || x == Size.x || y == Size.y || z == Size.z)
                {
                    if (!GetBlockWithBoundCheck(x, y, z).AddToRenderMesh)
                    {
                        emptySet = UpdateSet(position, emptySet, ref emptyHullMin, ref emptyHullMax);
                    }
                    continue;
                }
                var block = GetBlockUnchecked(x, y, z);
                if (block?.AddToRenderMesh == true)
                {
                    solidSet = UpdateSet(position, solidSet, ref solidHullMin, ref solidHullMax);
                }
                else
                {
                    emptySet = UpdateSet(position, emptySet, ref emptyHullMin, ref emptyHullMax);
                }
            }

            if (emptySet)
            {
                emptyHullMin -= Vector3Int.one;
                emptyHullMax += Vector3Int.one;
            }

            if (solidSet)
            {
                RenderHullMin = Vector3Int.Max(emptyHullMin, solidHullMin);
                RenderHullMax = Vector3Int.Min(emptyHullMax, solidHullMax) + Vector3Int.one;
                SolidHullMin = solidHullMin;
                SolidHullMax = solidHullMax + Vector3Int.one;
            }
            else
            {
                RenderHullMin = Vector3Int.zero;
                RenderHullMax = Vector3Int.zero;
                SolidHullMin = Vector3Int.zero;
                SolidHullMax = Vector3Int.zero;
            }
            _hullDirty = false;
        }

        private static bool UpdateSet(Vector3Int position, bool emptySet, ref Vector3Int min, ref Vector3Int max)
        {
            if (emptySet)
            {
                max = Vector3Int.Max(max, position);
                min = Vector3Int.Min(min, position);
            }
            else
            {
                min = position;
                max = position;
            }

            return true;
        }

        public IBlock GetBlockWithBoundCheck(int x, int y, int z)
        {
            return _parent.GetBlockWithBoundCheck(ChunkOffset.x + x, ChunkOffset.y + y, ChunkOffset.z + z);
        }
        
        public IBlock GetBlockUnchecked(int x, int y, int z)
        {
            return _parent.GetBlockUnchecked(ChunkOffset.x + x, ChunkOffset.y + y, ChunkOffset.z + z);
        }
        
        public void Evict()
        {
            PoolManager.GetObjectPool<VisibilityData>().Push(Connectivity);
            Connectivity = null;
        }
    }
}