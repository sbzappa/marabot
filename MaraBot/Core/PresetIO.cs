using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MaraBot.Core
{
    public static class PresetIO
    {
        static readonly string[] k_PresetFolders = new []
        {
            "$HOME/marabot/presets",
            "presets",
            "../../../presets",
            "../../../../presets"
        };

        public static async Task<Dictionary<string, Preset>> LoadPresets(IReadOnlyDictionary<string, Option> options)
        {
            var homeFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            var presetFolder = k_PresetFolders
                .Select(path => path.Replace("$HOME", homeFolder))
                .FirstOrDefault(path => Directory.Exists(path));

            if (String.IsNullOrEmpty(presetFolder))
            {
                throw new InvalidOperationException("Could not find a preset folder.");
            }

            var presetPaths = Directory.GetFiles(presetFolder, "*.json");

            if (presetPaths.Length == 0)
            {
                throw new InvalidOperationException($"No presets found in preset folder {presetFolder}");
            }

            var presets = new Dictionary<string, Preset>();

            foreach (var presetPath in presetPaths)
            {
                using (StreamReader r = new StreamReader(presetPath))
                {
                    var jsonContent = await r.ReadToEndAsync();
                    presets[Path.GetFileNameWithoutExtension(presetPath)] = LoadPreset(jsonContent, options);
                }
            }

            return presets;
        }

        public static Preset LoadPreset(string jsonContent, IReadOnlyDictionary<string, Option> options)
        {
            var preset = JsonConvert.DeserializeObject<Preset>(jsonContent);
            preset?.MakeDisplayable(options);

            return preset;
        }
    }
}
