using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.SunsetIsland.Config.Data
{
    public class ConditionConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IConditionData);
        }

        public override void WriteJson(JsonWriter writer,
                                       object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader,
                                        Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            IConditionData condition;
            if (jsonObject["NoiseType"] != null)
                condition = new NoiseCondition();
            else if (jsonObject["ElevationRange"] != null)
                condition = new ElevationCondition();
            else
                throw new InvalidOperationException("Undefined Condition Schema.");
            serializer.Populate(jsonObject.CreateReader(), condition);
            return condition;
        }
    }
}