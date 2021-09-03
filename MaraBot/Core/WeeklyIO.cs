using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    public class WeeklyIO
    {
        private static readonly string k_WeeklyFolder = "$HOME/marabot";

        public static async void StoreWeekly(Weekly weekly, string weeklyFilename = "weekly.json")
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

        public static async Task<Weekly> LoadWeekly(string weeklyFilename = "weekly.json")
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
                return JsonConvert.DeserializeObject<Weekly>(json);
            }
        }
    }
}
