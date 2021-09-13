using System;
using System.Collections.Generic;
using System.Linq;

namespace MaraBot.Core
{
    /// <summary>
    /// Holds information on the weekly race settings.
    /// </summary>
    public class Weekly
    {
        /// <summary>Week number.</summary>
        public int WeekNumber;
        /// <summary>Preset name.</summary>
        public string PresetName;
        /// <summary>Seed used in weekly. This is a string of 16 hexadecimal values.</summary>
        public string Seed;
        /// <summary>Leaderboard for the weekly race.</summary>
        public Dictionary<string, TimeSpan> Leaderboard;
        /// <summary>Timestamp at which weekly seed has been created.</summary>
        public DateTime Timestamp;

        public void AddToLeaderboard(string username, TimeSpan time)
        {
            if (Leaderboard == null)
                Leaderboard = new Dictionary<string, TimeSpan>();

            if (Leaderboard.ContainsKey(username))
                Leaderboard[username] = time;
            else
                Leaderboard.Add(username, time);
        }

        /// <summary>
        /// Load new weekly parameters into weely.
        /// </summary>
        /// <param name="weekly">Weekly instance.</param>
        public void Load(Weekly weekly)
        {
            WeekNumber = weekly.WeekNumber;
            PresetName = weekly.PresetName;
            Seed = weekly.Seed;
            Leaderboard = weekly.Leaderboard;
            Timestamp = weekly.Timestamp;
        }

        /// <summary>
        /// Retrieves invalid weekly settings.
        /// </summary>
        public static Weekly Invalid => new Weekly
        {
            WeekNumber = -1,
            PresetName = String.Empty,
            Seed = String.Empty,
            Leaderboard = null,
            Timestamp = DateTime.MinValue
        };

        public static Weekly NotSet => new Weekly
        {
            WeekNumber = RandomUtils.GetWeekNumber(),
            PresetName = "not-set",
            Seed = "0",
            Leaderboard = null,
            Timestamp = DateTime.Now
        };

        /// <summary>
        /// Generates a random weekly race using specified presets.
        /// Each preset has a weight, and so a random preset will be chosen
        /// based on accumulated weights.
        /// </summary>
        /// <param name="presets">List of available presets.</param>
        /// <returns>Returns new weekly settings.</returns>
        public static Weekly Generate(IReadOnlyDictionary<string, Preset> presets)
        {
            var weekNumber = RandomUtils.GetWeekNumber();
            var seed = RandomUtils.GetRandomSeed();

            var weeklyPresets = presets.Where(preset => preset.Value.Weight > 0);
            var totalWeight = weeklyPresets.Sum(preset => preset.Value.Weight);

            if (totalWeight == 0)
                return Weekly.Invalid;

            var index = RandomUtils.GetRandomIndex(0, totalWeight - 1);

            var presetName = String.Empty;

            var sum = 0;
            foreach (var preset in weeklyPresets)
            {
                sum += preset.Value.Weight;
                if (sum > index)
                {
                    presetName = preset.Key;
                    break;
                }
            }

            return new Weekly
            {
                WeekNumber = weekNumber,
                PresetName = presetName,
                Seed = seed,
                Leaderboard = new Dictionary<string, TimeSpan>(),
                Timestamp = DateTime.Now
            };
        }
    }
}
