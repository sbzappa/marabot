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
        /// <summary>
        /// Load a single preset from a JSON buffer.
        /// </summary>
        /// <param name="jsonContent">JSON content buffer.</param>
        /// <param name="options">Randomizer options.</param>
        /// <returns>Returns the loaded preset.</returns>
        public static Preset LoadPreset(string jsonContent, IReadOnlyDictionary<string, Option> options)
        {
            var preset = JsonConvert.DeserializeObject<Preset>(jsonContent);
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
