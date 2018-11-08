using System.Collections.Generic;
using UnityEngine;

namespace Assets.SunsetIsland.Config
{
    public class Properties
    {
        public int MaxIntervalTreeMods { get; set; }
        public int MaxPoolSize { get; set; }
        public Dictionary<string, List<string>> BlockTypeMap { get; set; }
        public float BlockUvScale { get; set; }
        public int BlockTextureWidth { get; set; }
        public int BlockTextureHeight { get; set; }
        public TextureFormat BlockTextureFormat { get; set; }
        public int ChunkSize { get; set; }
        public int PatchSize { get; set; }
        public int RenderRaySweep { get; set; }
        public int WorldHeight { get; set; }
        public int WorldWidthChunks { get; set; }
        public int WorldDepthChunks { get; set; }
        public int LoD0Radius { get; set; }
        public int LoD1Radius { get; set; }
        public int LoD2Radius { get; set; }
        public int LoD3Radius { get; set; }
        public float TimeSlice { get; set; }
        public float BlockWorldScale { get; set; }
        public float QueueEscelationTime { get; set; }
        public int BackgroundThreadTimeout { get; set; }
        public long AutoCompressTime { get; set; }
    }
}