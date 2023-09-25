using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MaraBot.IO
{
    using Core;

    /// <summary>
    /// 8ball IO.
    /// </summary>
    public static class EightBallIO
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
        /// Reads the 8ball responses from file.
        /// </summary>
        /// <returns>The 8ball responses.</returns>
        /// <exception cref="InvalidOperationException">No config file has been found.</exception>
        public static async Task<string[]> LoadResponsesAsync()
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
                    .Replace("$APP", appFolder) + "/8ball.json")
                .FirstOrDefault(path => File.Exists(path));

            if (String.IsNullOrEmpty(optionsPath))
            {
                throw new InvalidOperationException($"No 8ball responses found");
            }

            using (StreamReader r = new StreamReader(optionsPath))
            {
                var json = await r.ReadToEndAsync();
                return JsonConvert.DeserializeObject<string[]>(json);
            }
        }
    }
}
