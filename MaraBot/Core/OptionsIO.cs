using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    public static class OptionsIO
    {
        static readonly string[] k_ConfigFolders = new []
        {
            "config",
            "../../../config",
            "../../../../config",
        };

        public static async Task<Dictionary<string, Option>> LoadOptions()
        {
            var optionsPath = k_ConfigFolders
                .Select(path => $"{path}/options.json")
                .FirstOrDefault(path => File.Exists(path));

            if (String.IsNullOrEmpty(optionsPath))
            {
                throw new InvalidOperationException($"No options found");
            }

            using (StreamReader r = new StreamReader(optionsPath))
            {
                var json = await r.ReadToEndAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, Option>>(json);
            }
        }
    }
}
