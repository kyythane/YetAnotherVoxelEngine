using System;
using Assets.SunsetIsland.Chunks.Processors.Lighting;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.SunsetIsland.Config.Data
{
    public class BlockDataConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BlockData);
        }

        public override void WriteJson(JsonWriter writer,
                                       object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        [NotNull]
        public override object ReadJson(JsonReader reader,
                                        Type objectType, object existingValue,
                                        [NotNull] JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            int opacityRed = 31,
                opacityGreen = 31,
                opacityBlue = 31,
                opacitySunRed = 31,
                opacitySunGreen = 31,
                opacitySunBlue = 31;

            int emissivityRed = 0,
                emissivityGreen = 0,
                emissivityBlue = 0,
                emissivitySunRed = 0,
                emissivitySunGreen = 0,
                emissivitySunBlue = 0;

            if (jsonObject["Opacity"] != null)
            {
                var value = jsonObject["Opacity"].Value<int>();
                opacityRed = value;
                opacityGreen = value;
                opacityBlue = value;
                opacitySunRed = value;
                opacitySunGreen = value;
                opacitySunBlue = value;
            }

            if (jsonObject["Emissivity"] != null)
            {
                var value = jsonObject["Emissivity"].Value<int>();
                emissivityRed = value;
                emissivityGreen = value;
                emissivityBlue = value;
            }

            if (jsonObject["EmissivitySun"] != null)
            {
                var value = jsonObject["EmissivitySun"].Value<int>();
                emissivitySunRed = value;
                emissivitySunGreen = value;
                emissivitySunBlue = value;
            }

            if (jsonObject["OpacitySun"] != null)
            {
                var value = jsonObject["OpacitySun"].Value<int>();
                opacitySunRed = value;
                opacitySunGreen = value;
                opacitySunBlue = value;
            }

            if (jsonObject["OpacityRed"] != null)
            {
                var value = jsonObject["OpacityRed"].Value<int>();
                opacityRed = value;
                opacitySunRed = value;
            }

            if (jsonObject["OpacityGreen"] != null)
            {
                var value = jsonObject["OpacityGreen"].Value<int>();
                opacityGreen = value;
                opacitySunGreen = value;
            }

            if (jsonObject["OpacityBlue"] != null)
            {
                var value = jsonObject["OpacityBlue"].Value<int>();
                opacityBlue = value;
                opacitySunBlue = value;
            }

            if (jsonObject["OpacitySunRed"] != null)
            {
                var value = jsonObject["OpacitySunRed"].Value<int>();
                opacitySunRed = value;
            }

            if (jsonObject["OpacitySunGreen"] != null)
            {
                var value = jsonObject["OpacitySunGreen"].Value<int>();
                opacitySunGreen = value;
            }

            if (jsonObject["OpacitySunBlue"] != null)
            {
                var value = jsonObject["OpacitySunBlue"].Value<int>();
                opacitySunBlue = value;
            }

            if (jsonObject["EmissivityRed"] != null)
            {
                var value = jsonObject["EmissivityRed"].Value<int>();
                emissivityRed = value;
            }

            if (jsonObject["EmissivityGreen"] != null)
            {
                var value = jsonObject["EmissivityGreen"].Value<int>();
                emissivityGreen = value;
            }

            if (jsonObject["EmissivityBlue"] != null)
            {
                var value = jsonObject["EmissivityBlue"].Value<int>();
                emissivityBlue = value;
            }

            if (jsonObject["EmissivitySunRed"] != null)
            {
                var value = jsonObject["EmissivitySunRed"].Value<int>();
                emissivitySunRed = value;
            }

            if (jsonObject["EmissivitySunGreen"] != null)
            {
                var value = jsonObject["EmissivitySunGreen"].Value<int>();
                emissivitySunGreen = value;
            }

            if (jsonObject["EmissivitySunBlue"] != null)
            {
                var value = jsonObject["EmissivitySunBlue"].Value<int>();
                emissivitySunBlue = value;
            }

            var block = new BlockData();
            serializer.Populate(jsonObject.CreateReader(), block);

            block.Opacity = LightProcessor.GetOpacity(opacityRed, opacityGreen, opacityBlue,
                                                      opacitySunRed, opacitySunGreen, opacitySunBlue);

            block.Emissivity = (uint) ((emissivityRed & 31) << 25) |
                               (uint) ((emissivityGreen & 31) << 20) |
                               (uint) ((emissivityBlue & 31) << 15) |
                               (uint) ((emissivitySunRed & 31) << 10) |
                               (uint) ((emissivitySunGreen & 31) << 5) |
                               (uint) (emissivitySunBlue & 31);

            return block;
        }
    }
}