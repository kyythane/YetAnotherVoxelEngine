using System.Collections.Generic;

namespace Assets.SunsetIsland.Config.Data
{
    public class BlockData
    {
        public BlockData()
        {
            RenderOpaque = true;
            AddToPhysicsMesh = true;
            AddToRenderMesh = true;
        }

        public string Name { get; set; }
        public int BlockId { get; set; }
        public List<string> TextureNames { get; set; }
        public float Durability { get; set; }
        //TODO : make opacity axis dependant (i.e. objects that have a hole in them)
        public bool RenderOpaque { get; set; }
        public uint Opacity  { get; set; }
        public uint Emissivity { get; set; }
        public bool AddToRenderMesh { get; set; }
        public bool AddToPhysicsMesh { get; set; }
    }
}