using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MaraBot.IO
{
    using Core;

    /// <summary>
    /// Config file IO.
    /// </summary>
    /// <seealso cref="Config"/>
    public static class ConfigIO
    {
        static readonly string[] k_ConfigFolders = new []
        {
            "config",
            "../../../config",
            "../../../../config",
            "$HOME/marabot/config",
            "$HOME/marabot"
        };

        /// <summary>
        /// Reads the bot configuration from file.
        /// </summary>
        /// <returns>The bot configuration.</returns>
        /// <exception cref="InvalidOperationException">No config file has been found.</exception>
        public static async Task<Config> LoadConfigAsync()
        {
            var homeFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            var configPath = k_ConfigFolders
                .Select(path => path.Replace("$HOME", homeFolder) + "/config.json")
                .FirstOrDefault(path => File.Exists(path));

            if (String.IsNullOrEmpty(configPath))
            {
                throw new InvalidOperationException($"No config found");
            }

            using (StreamReader r = new StreamReader(configPath))
            {
                var json = await r.ReadToEndAsync();
                return JsonConvert.DeserializeObject<Config>(json);
            }
        }
    }
}
