using Newtonsoft.Json;

namespace DialogTree.Runtime.Data
{
    public sealed class CpuOutputNode
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }

        [JsonProperty("nextNodeId")]
        public string NextNodeId { get; internal set; }
    }
}