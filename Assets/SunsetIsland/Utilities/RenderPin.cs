using UnityEngine;

namespace Assets.SunsetIsland.Utilities
{
    public class RenderPin
    {
        public RenderPin()
        {
        }

        public RenderPin(Bounds bounds, Quaternion rotation, IRenderData renderData)
        {
            Initialize(bounds, rotation, renderData);
        }

        public Bounds Bounds { get; private set; }
        public Quaternion Rotation { get; private set; }
        public IRenderData RenderData { get; private set; }

        public void Initialize(Bounds bounds, Quaternion rotation, IRenderData renderData)
        {
            Bounds = bounds;
            Rotation = rotation;
            RenderData = renderData;
        }
    }

    public interface IRenderData
    {
    }

    public class SpriteRenderData : IRenderData
    {
        public SpriteRenderData()
        {
        }

        public SpriteRenderData(Sprite sprite, bool billboard)
        {
            Initialize(sprite, billboard);
        }

        public Sprite Sprite { get; private set; }
        public bool Billboard { get; private set; }

        public void Initialize(Sprite sprite, bool billboard)
        {
            Sprite = sprite;
            Billboard = billboard;
        }
    }
}