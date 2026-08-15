using System.IO;
using DialogTree.Runtime.Data;
using DialogTree.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace DialogTree.Editor.Serialization
{
    public static class DialogueGraphLoader
    {
        private static JsonSerializerSettings Settings => new()
        {
            Converters = { new DialogNodeJsonConverter() }
        };

        public static DialogGraph Load(string absolutePath)
        {
            if (!File.Exists(absolutePath))
            {
                Debug.LogWarning($"[DialogueTreeEditor] Cannot find file: {absolutePath}");
                return null;
            }

            var json = File.ReadAllText(absolutePath);
            return LoadFromString(json);
        }

        public static DialogGraph LoadFromString(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<DialogGraph>(json, Settings);
            }
            catch (JsonException e)
            {
                Debug.LogError($"[DialogueTreeEditor] Error while parsing JSON: {e.Message}");
                return null;
            }
        }
    }
}
