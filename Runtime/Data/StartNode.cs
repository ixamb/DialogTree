using Newtonsoft.Json;

namespace DialogTree.Runtime.Data
{
    public sealed class StartNode : DialogNode
    {
        [JsonProperty("nextNodeId")]
        public string NextNodeId { get; internal set; }
    }
}
