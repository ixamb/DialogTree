using System;
using DialogTree.Runtime.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DialogTree.Runtime.Serialization
{
    public sealed class DialogNodeJsonConverter : JsonConverter<DialogNode>
    {
        private const string TypeField = "type";
        private const string StartTypeValue = "start";
        private const string CpuTypeValue = "cpu";
        private const string UserTypeValue = "user";

        public override void WriteJson(JsonWriter writer, DialogNode value, JsonSerializer serializer)
        {
            JObject obj;

            switch (value)
            {
                case StartNode start:
                    obj = JObject.FromObject(start, CreateSerializerWithoutSelf(serializer));
                    obj.AddFirst(new JProperty(TypeField, StartTypeValue));
                    break;

                case CpuNode cpu:
                    obj = JObject.FromObject(cpu, CreateSerializerWithoutSelf(serializer));
                    obj.AddFirst(new JProperty(TypeField, CpuTypeValue));
                    break;

                case UserNode user:
                    obj = JObject.FromObject(user, CreateSerializerWithoutSelf(serializer));
                    obj.AddFirst(new JProperty(TypeField, UserTypeValue));
                    break;

                default:
                    throw new JsonSerializationException($"Unsupported node type {nameof(DialogNodeJsonConverter)}: {value.GetType()}");
            }

            obj.WriteTo(writer);
        }

        public override DialogNode ReadJson(JsonReader reader, Type objectType, DialogNode existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var typeValue = obj[TypeField]?.Value<string>();

            DialogNode result = typeValue switch
            {
                StartTypeValue => new StartNode(),
                CpuTypeValue => new CpuNode(),
                UserTypeValue => new UserNode(),
                _ => throw new JsonSerializationException(
                    $"\"{TypeField}\" field unknown or missing (\"{typeValue}\") on a field node.")
            };

            using var subReader = obj.CreateReader();
            serializer.Populate(subReader, result);

            return result;
        }

        private static JsonSerializer CreateSerializerWithoutSelf(JsonSerializer original)
        {
            var clone = new JsonSerializer
            {
                Formatting = original.Formatting,
                NullValueHandling = original.NullValueHandling,
                DefaultValueHandling = original.DefaultValueHandling
            };

            foreach (var converter in original.Converters)
            {
                if (converter is not DialogNodeJsonConverter)
                {
                    clone.Converters.Add(converter);
                }
            }

            return clone;
        }
    }
}