using System.Collections.Generic;
using Newtonsoft.Json;

namespace DialogTree.Runtime.Data
{
    public sealed class DialogGraph
    {
        [JsonProperty("graphId")]
        public string GraphId { get; internal set; }

        [JsonProperty("title")]
        public string Title { get; internal set; }

        [JsonProperty("nodes")]
        public List<DialogNode> Nodes { get; internal set; } = new();
    }
}
