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
    /// Preset File IO.
    /// </summary>
    /// <seealso cref="Preset"/>
    public static class PresetIO
    {
        static readonly string[] k_ChallengePresetsFolders = new []
        {
            "$HOME/marabot/challenges",
            "challenges",
            "../../../challenges",
            "../../../../challenges",
            "$APP/../../../../challenges"
        };

        /// <summary>
        /// Reads the presets from file.
        /// </summary>
        /// <param name="options">Randomizer options.</param>
        /// <returns>Returns a list of available presets.</returns>
        /// <exception cref="InvalidOperationException">No preset folder could be found.</exception>
        public static async Task<Dictionary<string, Preset>> LoadChallengePresetsAsync(IReadOnlyDictionary<string, Option> options, Config config)
        {
            var homeFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;

            var challengePresetsFolder = k_ChallengePresetsFolders
                .Select(path => path
                    .Replace("$HOME", homeFolder)
                    .Replace("$APP", appFolder))
                .FirstOrDefault(path => Directory.Exists(path));

            if (String.IsNullOrEmpty(challengePresetsFolder))
            {
                throw new InvalidOperationException("Could not find a preset folder.");
            }

            var presetPaths = Directory.GetFiles(challengePresetsFolder, "*.json");

            if (presetPaths.Length == 0)
            {
                throw new InvalidOperationException($"No presets found in preset folder {challengePresetsFolder}");
            }

            var presets = new Dictionary<string, Preset>();

            foreach (var presetPath in presetPaths)
            {
                using (StreamReader r = new StreamReader(presetPath))
                {
                    var jsonContent = await r.ReadToEndAsync();
                    presets[Path.GetFileNameWithoutExtension(presetPath)] = LoadPreset(jsonContent, options, config);
                }
            }

            return presets;
        }

        /// <summary>
        /// Load a single preset from a JSON buffer.
        /// </summary>
        /// <param name="jsonContent">JSON content buffer.</param>
        /// <param name="options">Randomizer options.</param>
        /// <returns>Returns the loaded preset.</returns>
        public static Preset LoadPreset(string jsonContent, IReadOnlyDictionary<string, Option> options, Config config)
        {
            var preset = JsonConvert.DeserializeObject<Preset>(jsonContent);

            // Make sure we load a preset with a valid version.
            if (!preset.Options.ContainsKey("version"))
                preset.Options.Add("version", config.RandomizerVersion);

            preset.MakeDisplayable(options);

            return preset;
        }

        /// <summary>
        /// Stores a single preset to a JSON buffer.
        /// </summary>
        /// <param name="preset">Preset</param>
        /// <returns>JSON buffer.</returns>
        public static string StorePreset(in Preset preset)
        {
            return JsonConvert.SerializeObject(preset, Formatting.Indented);
        }
    }
}
