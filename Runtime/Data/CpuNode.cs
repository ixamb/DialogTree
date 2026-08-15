using System.Collections.Generic;
using Newtonsoft.Json;

namespace DialogTree.Runtime.Data
{
    public sealed class CpuNode : DialogNode
    {
        [JsonProperty("text")]
        public string Text { get; internal set; }

        [JsonProperty("outputs")]
        public List<CpuOutputNode> Outputs { get; internal set; } = new();
    }
}