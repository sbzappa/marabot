using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    public static class OptionIO
    {
        public static Dictionary<string, Option> LoadConfig()
        {
            using (StreamReader r = new StreamReader("config/options.json"))
            {
                var json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<Dictionary<string, Option>>(json);
            }
        }
    }
}
