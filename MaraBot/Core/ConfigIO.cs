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
            "$HOME/marabot/config",
            "config",
            "../../../config",
            "../../../../config"
        };
        
        public static Config LoadConfig()
        {
            var homeFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            
            var configFolder = k_ConfigFolders
                .Select(path => path.Replace("$HOME", homeFolder))
                .FirstOrDefault(path => Directory.Exists(path));

            if (String.IsNullOrEmpty(configFolder))
            {
                throw new InvalidOperationException("Could not find a config folder.");
            }

            var configPath = $"{configFolder}/config.json"; 

            if (!File.Exists(configPath))
            {
                throw new InvalidOperationException($"No config found in config folder {configFolder}");
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