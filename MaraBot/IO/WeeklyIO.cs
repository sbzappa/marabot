using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MaraBot.IO
{
    using Core;

    /// <summary>
    /// Weekly file IO.
    /// </summary>
    /// <seealso cref="Weekly"/>
    public class WeeklyIO
    {
        private static readonly string k_WeeklyFolder = "$HOME/marabot";

        /// <summary>
        /// Writes the weekly settings to file.
        /// </summary>
        /// <param name="weekly">Weekly settings.</param>
        /// <param name="weeklyFilename">Weekly filename.</param>
        public static async Task StoreWeeklyAsync(Weekly weekly, string weeklyFilename = "weekly.json")
        {
            var homeFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            var weeklyFolder = k_WeeklyFolder.Replace("$HOME", homeFolder);
            if (!Directory.Exists(weeklyFolder))
                Directory.CreateDirectory(weeklyFolder);

            var weeklyPath = weeklyFolder + "/" + weeklyFilename;

            using (StreamWriter w = new StreamWriter(weeklyPath))
            {
                var json = JsonConvert.SerializeObject(weekly, Formatting.Indented);
                await w.WriteAsync(json);
            }
        }

        /// <summary>
        /// Reads the weekly settings from file.
        /// </summary>
        /// <param name="options">Randomizer options</param>
        /// <param name="weeklyFilename">Weekly filename.</param>
        /// <returns>Returns the weekly settings.</returns>
        public static async Task<Weekly> LoadWeeklyAsync(IReadOnlyDictionary<string, Option> options, string weeklyFilename = "weekly.json")
        {
            var homeFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            var weeklyPath = k_WeeklyFolder.Replace("$HOME", homeFolder) + "/" + weeklyFilename;

            var weekly = Weekly.Invalid;
            if (!File.Exists(weeklyPath))
            {
                return weekly;
            }

            using (StreamReader r = new StreamReader(weeklyPath))
            {
                var json = await r.ReadToEndAsync();
                weekly = JsonConvert.DeserializeObject<Weekly>(json);

                if (weekly != null)
                {
                    weekly.Preset.MakeDisplayable(options);
                }

                return weekly;
            }
        }
    }
}
