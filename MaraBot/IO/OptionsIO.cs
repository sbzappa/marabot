using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MaraBot.IO
{
    using Core;

    /// <summary>
    /// Options file IO.
    /// </summary>
    /// <seealso cref="Options"/>
    public static class OptionsIO
    {
        static readonly string[] k_ConfigFolders = new []
        {
            "config",
            "../../../config",
            "../../../../config",
        };

        /// <summary>
        /// Reads the randomizer options from file.
        /// </summary>
        /// <returns>The randomizer options.</returns>
        /// <exception cref="InvalidOperationException">No options file has been found.</exception>
        public static async Task<Dictionary<string, Option>> LoadOptionsAsync()
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
                return JsonConvert.DeserializeObject<Dictionary<string, Option>>
                (
                    json,
                    new JsonSerializerSettings
                    {
                        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                        TypeNameHandling = TypeNameHandling.All,
                    }
                );
            }
        }
    }
}
