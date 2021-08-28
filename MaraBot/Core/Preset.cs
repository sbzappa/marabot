using System.Collections.Generic;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    public class Preset
    {
        public string Name;
        public string Version;
        [JsonConverter(typeof(OptionsConverter))] public Dictionary<string, string> Options;
        public string Author;
        public string Description;
        public int Weight;
    }
}