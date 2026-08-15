using System.IO;
using DialogTree.Runtime.Data;
using DialogTree.Runtime.Serialization;
using Newtonsoft.Json;

namespace DialogTree.Editor.Serialization
{
    public static class DialogueGraphSaver
    {
        private static JsonSerializerSettings Settings => new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new DialogNodeJsonConverter() }
        };

        public static void Save(DialogGraph graph, string absolutePath)
        {
            var json = JsonConvert.SerializeObject(graph, Settings);
            var directory = Path.GetDirectoryName(absolutePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(absolutePath, json);
        }

        public static string SaveToString(DialogGraph graph)
        {
            return JsonConvert.SerializeObject(graph, Settings);
        }
    }
}
