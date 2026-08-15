using Newtonsoft.Json;

namespace DialogTree.Runtime.Data
{
    public sealed class UserNode : DialogNode
    {
        [JsonProperty("text")]
        public string Text { get; internal set; }

        [JsonProperty("shortText")]
        public string ShortText { get; internal set; }

        [JsonProperty("nextNodeId")]
        public string NextNodeId { get; internal set; }
    }
}