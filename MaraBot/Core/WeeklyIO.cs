using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MaraBot.Core
{
    public class WeeklyIO
    {
        private static readonly string k_WeeklyFolder = "$HOME/marabot";

        public static void StoreWeekly(Weekly weekly)
        {
            var homeFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            var weeklyFolder = k_WeeklyFolder.Replace("$HOME", homeFolder);
            if (!Directory.Exists(weeklyFolder))
                Directory.CreateDirectory(weeklyFolder);

            var weeklyPath = weeklyFolder + "/weekly.json";

            // TODO do this async...
            using (StreamWriter w = new StreamWriter(weeklyPath))
            {
                var json = JsonConvert.SerializeObject(weekly);
                w.Write(json);
            }
        }

        public static Weekly LoadWeekly()
        {
            var homeFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            var weeklyPath = k_WeeklyFolder.Replace("$HOME", homeFolder) + "/weekly.json";

            var weekly = Weekly.Invalid;
            if (!File.Exists(weeklyPath))
            {
                return weekly;
            }

            using (StreamReader r = new StreamReader(weeklyPath))
            {
                var json = r.ReadToEnd();
                weekly = JsonConvert.DeserializeObject<Weekly>(json);
            }

            return weekly;
        }
    }
}
