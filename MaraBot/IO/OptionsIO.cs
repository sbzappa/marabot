using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            "$HOME/marabot/config",
            "$HOME/marabot",
            "$APP/../../../../config"
        };

        /// <summary>
        /// Reads the randomizer options from file.
        /// </summary>
        /// <returns>The randomizer options.</returns>
        /// <exception cref="InvalidOperationException">No options file has been found.</exception>
        public static async Task<Dictionary<string, Option>> LoadOptionsAsync()
        {
            var homeFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;

            var optionsPath = k_ConfigFolders
                .Select(path => path
                    .Replace("$HOME", homeFolder)
                    .Replace("$APP", appFolder) + "/options.json")
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
