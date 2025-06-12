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
    /// Mystery Settings IO
    /// </summary>
    /// <seealso cref="MysterySetting"/>
    public static class MysterySettingsIO
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
        /// Reads the mystery settings from file.
        /// </summary>
        /// <returns>The mystery settings.</returns>
        /// <exception cref="InvalidOperationException">No mystery settings file has been found.</exception>
        public static async Task<Dictionary<string, MysterySetting>> LoadMysterySettingsAsync()
        {
            var homeFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;

            var settingsPath = k_ConfigFolders
                .Select(path => path
                    .Replace("$HOME", homeFolder)
                    .Replace("$APP", appFolder) + "/weekly-200.json")
                .FirstOrDefault(path => File.Exists(path));

            if (String.IsNullOrEmpty(settingsPath))
            {
                throw new InvalidOperationException($"No mystery settings found");
            }

            using (StreamReader r = new StreamReader(settingsPath))
            {
                var json = await r.ReadToEndAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, MysterySetting>>
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
