using System.Collections.Generic;

namespace Assets.SunsetIsland.Config.Data
{
    public class EnvironmentLayer
    {
        public Dictionary<string, ConditionSet> Blocks { get; set; }
        public float MinThickness { get; set; }
        public float MaxThickness { get; set; }
    }
}