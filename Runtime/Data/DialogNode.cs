using DialogTree.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace DialogTree.Runtime.Data
{
    public abstract class DialogNode
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }

        [JsonProperty("position")]
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 Position { get; internal set; }
    }
}
