using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MaraBot.Core
{
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
        
        public static Config LoadConfig()
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

            Config config = default;
            using (StreamReader r = new StreamReader(configPath))
            {
                var json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject<Config>(json);
            }

            return config; 
        }
    }
}